using Aix.NatsMessageBus.Internals;
using Aix.NatsMessageBus.Model;
using Aix.NatsMessageBus.Utils;
using Microsoft.Extensions.Logging;
using NATS.Client;
using NATS.Client.Internals;
using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.NatsMessageBus
{
    /// <summary>
    /// 
    /// </summary>
    public class NatsJetStreamMessageBus : INatsJSMessageBus
    {
        private ILogger<NatsJetStreamMessageBus> _logger;
        private NatsMessageBusOptions _options;
        private IConnection _connection;
        private Lazy<IJetStream> _jetStreamFactory;
        private IJetStream _jetStream => _jetStreamFactory.Value;

        private static object SyncLock = new object();

        List<IAsyncSubscription> subscriptions = new List<IAsyncSubscription>();

        public NatsJetStreamMessageBus(ILogger<NatsJetStreamMessageBus> logger, NatsMessageBusOptions options, IConnection connection)
        {
            _logger = logger;
            _options = options;
            _connection = connection;

            _jetStreamFactory = new Lazy<IJetStream>(() => _connection.CreateJetStreamContext());
        }

        public void Dispose()
        {
            foreach (var item in subscriptions)
            {
                With.NoException(_logger, () => item.Unsubscribe(), "Unsubscribe");
                With.NoException(_logger, () => item.Dispose(), "Unsubscribe");
            }
        }

        public async Task PublishAsync(Type messageType, object message)
        {
            await PublishAsync(messageType, message, null);
        }

        public async Task PublishAsync(Type messageType, object message, AixPublishOptions publishOptions)
        {
            AssertUtils.IsNotNull(message, "message is null");
            var topic = GetTopic(messageType, publishOptions);

            var data = _options.Serializer.Serialize(message);
            Msg msg = new Msg(topic, null, data);

            var ack = await _jetStream.PublishAsync(msg);
        }

        public async Task<TResult> RequestAsync<TResult>(Type messageType, object message, int timeoutMillisecond)
        {
            return await RequestAsync<TResult>(messageType, message, timeoutMillisecond, null);
        }

        public async Task<TResult> RequestAsync<TResult>(Type messageType, object message, int timeoutMillisecond, AixPublishOptions publishOptions)
        {
            AssertUtils.IsNotNull(message, "message is null");
            var topic = GetTopic(messageType, publishOptions);

            var data = _options.Serializer.Serialize(message);
            Msg msg = new Msg
            {
                Subject = topic,
                Data = data,
            };

            var replyManage = AsyncCallResponseManage.JetStreamReplyManage;

            var requestId = replyManage.CreateRequestId();
            var replyId = Constants.MyReply + "_" + Guid.NewGuid().ToString().Replace("-", "");
            msg.Header.Add(Constants.MyReply, replyId);

            EventHandler<MsgHandlerEventArgs> eventHandler = (sender, args) =>
            {
                try
                {
                    var replyResult = _options.Serializer.Deserialize<TResult>(args.Message.Data);
                    replyManage.SetResult(requestId, replyResult);
                }
                catch (Exception ex)
                {
                    replyManage.SetException(requestId, ex);
                }
            };

            using (var requestScope = replyManage.RegisterRequest(requestId, GetTimeoutMillisecond(timeoutMillisecond)))
            {
                _connection.SubscribeAsync(replyId, eventHandler);
                var ack = await _jetStream.PublishAsync(msg);

                var result = default(TResult);
                try
                {
                    result = (TResult)await requestScope.Waiter.Task;
                }
                catch (Exception ex)
                {
                    //记录log
                    throw ex;
                }
                return result;

            }
        }

        public async Task SubscribeAsync<T>(Func<T, SubscribeContext, Task> handler, AixSubscribeOptions subscribeOptions = null) where T : class
        {
            await SubscribeAsync<T>(async (obj, context) =>
            {
                await handler(obj, context);
                return null;
            }, subscribeOptions);
        }

        public async Task SubscribeAsync<T>(Func<T, SubscribeContext, Task<object>> handler, AixSubscribeOptions subscribeOptions = null) where T : class
        {
            string topic = GetTopic(typeof(T), subscribeOptions);
            var queue = subscribeOptions?.Queue;
            try
            {
                var threadCount = subscribeOptions?.ConsumerThreadCount ?? 0;
                threadCount = threadCount > 0 ? threadCount : _options.DefaultConsumerThreadCount;
                AssertUtils.IsTrue(threadCount > 0, "nats subscribe threadCount is zero");
                string durable = StringUtils.IfEmpty(subscribeOptions?.Durable, topic + "_consumer");//消费者名称
                // string deliver = topic + "_deliver_subject"; 

                var autoAck = _options.AutoAck.HasValue && _options.AutoAck.Value == true;
                string stream = Helper.GetStream(_options, typeof(T), subscribeOptions);
                // stream = StringUtils.IfEmpty(subscribeOptions?.Stream, stream);

                AssertUtils.IsTrue(ValidStream(stream), $"nats stream no exists. {stream}");
                //CreateStreamWhenDoesNotExist<T>(stream, topic);

                SemaphoreSlim semaphoregate = new SemaphoreSlim(threadCount - 1);//控制异步 和多线程 

                #region eventHandler

                EventHandler<MsgHandlerEventArgs> eventHandler = (sender, args) =>
                {
                    try
                    {
                        Task.Run(async () =>
                        {
                            var message = args.Message;
                            await With.NoException(_logger, async () => await HandleMessage(handler, queue, message), "nats HandleMessage");
                            Ack(message, autoAck);
                            semaphoregate.Release();
                        });
                    }
                    finally
                    {
                        semaphoregate.Wait();
                    }
                };

                #endregion

                ConsumerConfiguration consumerConfiguration = ConsumerConfiguration.Builder()
                    .WithDeliverPolicy(subscribeOptions?.DeliverPolicy ?? DeliverPolicy.All)
                    .Build();
                PushSubscribeOptions pushSubscribeOptions = PushSubscribeOptions.Builder()
                        .WithStream(stream)
                        .WithDurable(durable)
                        //.WithDeliverSubject(deliver) //内部会一个默认值 随机数
                        // .WithDeliverGroup(queue)  //使用PushSubscribeAsync 参数传递的queue
                        .WithConfiguration(consumerConfiguration)
                        .Build();

                IJetStream jetStream = _connection.CreateJetStreamContext();
                var subscription = jetStream.PushSubscribeAsync(topic, queue, eventHandler, autoAck, pushSubscribeOptions); //topic 支持通配符
                subscription.PendingMessageLimit = _options.PendingMessageLimit;
                subscription.PendingByteLimit = _options.PendingByteLimit;

                subscriptions.Add(subscription);
                _logger.LogInformation($"natsjs subscribe Topic:{topic},Queue:{queue},stream:{stream},durable:{durable},ConsumerThreadCount:{threadCount}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"nats subscribe error: Topic:{topic},Queue:{queue}");
                throw ex;
            }

            await Task.CompletedTask;
        }

        #region private

        private void OnMsgHandler(object sender, MsgHandlerEventArgs args)
        {

        }

        private async Task HandleMessage<T>(Func<T, SubscribeContext, Task<object>> handler, string queue, Msg message)
        {
            string myreply = message.Header[Constants.MyReply];
            bool isReply = !string.IsNullOrEmpty(myreply);
            try
            {
                var obj = _options.Serializer.Deserialize<T>(message.Data);
                var subscribeContext = new SubscribeContext { Topic = message.Subject, Queue = queue };
                var replyObj = await handler(obj, subscribeContext);
                if (isReply)
                {
                    _connection.Publish(myreply, _options.Serializer.Serialize(replyObj));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"nats subscribe {message.Subject} error");
            }
        }

        private void Ack(Msg message, bool autoAck)
        {
            try
            {
                if (message.IsJetStream && !autoAck)
                {
                    message.Ack();

                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "nats ack error");
            }
        }



        private string GetTopic(Type type, AixPublishOptions publishOptions)
        {
            return Helper.GetPubTopic(type, _options, publishOptions);
        }

        private string GetTopic(Type type, AixSubscribeOptions subscribeOptions)
        {
            return Helper.GetSubTopic(type, _options, subscribeOptions);
        }



        private int GetTimeoutMillisecond(int value)
        {
            if (value <= 0)
            {
                return _options.DefaultTimeoutMillisecond;
            }
            return value;
        }

        bool ValidStream(string stream)
        {
            bool result = false;
            IJetStreamManagement jsm = _connection.CreateJetStreamManagementContext();
            try
            {

                var streamInfo = jsm.GetStreamInfo(stream);
                if (streamInfo != null)
                {
                    result = true;
                }
            }
            catch (NATSJetStreamException)
            { /* stream does not exist */
                result = false;
            }

            return result;
        }

        //void CreateStreamWhenDoesNotExist<T>(string stream, string subject, AixSubscribeOptions subscribeOptions)
        //{
        //    IJetStreamManagement jsm = _connection.CreateJetStreamManagementContext();
        //    lock (SyncLock)
        //    {
        //        try
        //        {
        //            var streamInfo = jsm.GetStreamInfo(stream); // this throws if the stream does not exist

        //            if (!streamInfo.Config.Subjects.Contains(subject))
        //            {
        //                var subjects = streamInfo.Config.Subjects;
        //                subjects.Add(subject);
        //                jsm.UpdateStream(CreateStreamConfiguration<T>(stream, subjects, subscribeOptions));

        //            }

        //            return;
        //        }
        //        catch (NATSJetStreamException) { /* stream does not exist */ }

        //        jsm.AddStream(CreateStreamConfiguration<T>(stream, new List<string> { subject }, subscribeOptions));
        //    }

        //}

        //private StreamConfiguration CreateStreamConfiguration<T>(string stream, List<string> subjects, AixSubscribeOptions subscribeOptions)
        //{
        //    var maxAge = _options.DefaultMaxAge;
        //    if (Helper.GetStream(_options, typeof(T), subscribeOptions).MaxAge > TimeSpan.Zero)
        //    {
        //        maxAge = Helper.GetStream(_options, typeof(T), subscribeOptions).MaxAge;
        //    }

        //    StreamConfiguration streamConfiguration = StreamConfiguration.Builder()
        //                                      .WithName(stream)
        //                                      //.WithStorageType(StorageType.File)
        //                                      .WithSubjects(subjects)
        //                                      // .WithNoAck(false)
        //                                      .WithMaxAge(Duration.OfSeconds((long)maxAge.TotalSeconds))
        //                                      .Build();

        //    return streamConfiguration;
        //}

        #endregion
    }
}
