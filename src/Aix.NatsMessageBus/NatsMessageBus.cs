using Aix.NatsMessageBus.Model;
using Aix.NatsMessageBus.Utils;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.NatsMessageBus
{
    public class NatsMessageBus : INatsMessageBus
    {
        private ILogger<NatsMessageBus> _logger;
        private NatsMessageBusOptions _options;
        private IConnection _connection;

        private object EmptyObj = new object();

        List<IAsyncSubscription> subscriptions = new List<IAsyncSubscription>();
        public NatsMessageBus(ILogger<NatsMessageBus> logger, NatsMessageBusOptions options, IConnection connection)
        {
            _logger = logger;
            _options = options;
            _connection = connection;
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
            _connection.Publish(topic, null, data);
            await Task.CompletedTask;
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
            var responseMsg = await _connection.RequestAsync(topic, data, GetTimeoutMillisecond(timeoutMillisecond));
            //return _options.Serializer.Deserialize<TResult>(responseMsg.Data);
            var replyResult = _options.Serializer.Deserialize<ReplyResult>(responseMsg.Data);
            if (replyResult?.Code == 0)
            {
                var obj = _options.Serializer.Deserialize<TResult>(replyResult.Data);
                return obj;
            }
            else
            {
                throw new Exception($"{replyResult?.Message ?? "nats reply error"}");
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
            var threadCount = subscribeOptions?.ConsumerThreadCount ?? 0;
            threadCount = threadCount > 0 ? threadCount : _options.DefaultConsumerThreadCount;
            AssertUtils.IsTrue(threadCount > 0, "nats subscribe threadCount is zero");
            var autoAck = true;
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

            IAsyncSubscription subscription = _connection.SubscribeAsync(topic, queue);  //不传 或 null默认就是随机数，传""报错
            subscription.PendingMessageLimit = _options.PendingMessageLimit;
            subscription.PendingByteLimit = _options.PendingByteLimit;
            subscription.MessageHandler += eventHandler;
            subscription.Start();
            subscriptions.Add(subscription);
            _logger.LogInformation($"nats subscribe Topic:{topic},Queue:{queue},ConsumerThreadCount:{threadCount}");

            await Task.CompletedTask;
        }

        private async Task HandleMessage<T>(Func<T, SubscribeContext, Task<object>> handler, string queue, Msg message)
        {
            string myreply = message.Reply;// message.Header[Constants.MyReply];
            bool isReply = !string.IsNullOrEmpty(myreply);
            ReplyResult replyResult = null;
            try
            {
                var obj = _options.Serializer.Deserialize<T>(message.Data);
                var subscribeContext = new SubscribeContext { Topic = message.Subject, Queue = queue };
                var replyObj = await handler(obj, subscribeContext);
                if (isReply) replyResult = new ReplyResult { Data = _options.Serializer.Serialize(replyObj) };
            }
            catch (Exception ex)
            {
                if (isReply) replyResult = new ReplyResult { Code = -1, Message = ex.Message };
                _logger.LogError(ex, $"nats subscribe {message.Subject} error");
            }

            if (isReply)
            {
                // _connection.Publish(myreply, _options.Serializer.Serialize(replyResult));
                message.Respond(_options.Serializer.Serialize(replyResult));
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
            return "s_" + Helper.GetPubTopic(type, _options, publishOptions);
        }

        private string GetTopic(Type type, AixSubscribeOptions subscribeOptions)
        {
            return "s_" + Helper.GetSubTopic(type, _options, subscribeOptions);
        }

        private int GetTimeoutMillisecond(int value)
        {
            if (value <= 0)
            {
                return _options.DefaultTimeoutMillisecond;
            }
            return value;
        }

    }
}
