using Aix.NatsMessageBus.Model;
using Aix.NatsMessageBus.Utils;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

        #region Raw

        public async Task PublishAsync(string subject, byte[] data)
        {
            //AssertUtils.IsNotEmpty(subject, "subject is null or empty");
            //AssertUtils.IsNotNull(data, "data is null");
            _connection.Publish(subject, data);
            await Task.CompletedTask;
        }

        public async Task<byte[]> RequestAsync(string subject, byte[] data, int timeout = 10 * 1000)
        {
            //AssertUtils.IsNotEmpty(subject, "subject is null or empty");
            //AssertUtils.IsNotNull(data, "data is null");
            var msg = await _connection.RequestAsync(subject, data, timeout);

            return msg.Data;
        }

        public async Task<IMySubscription> SubscribeAsync(string subject, Func<byte[], Task<byte[]>> handleMessage)
        {
            return await SubscribeAsync(subject, null, handleMessage);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="queue">传null： 就是订阅模式，(所有订阅者都会收到消息)，不为空：队列模式，所有订阅者只有一个订阅者收到消息</param>
        /// <param name="handleMessage"></param>
        /// <returns></returns>
        public async Task<IMySubscription> SubscribeAsync(string subject, string queue, Func<byte[], Task<byte[]>> handleMessage)
        {
            //AssertUtils.IsNotEmpty(subject, "subject is null or empty");
            EventHandler<MsgHandlerEventArgs> eventHandler = (sender, args) =>
            {
                var msg = args.Message;
                try
                {
                    var result = handleMessage(msg.Data).ConfigureAwait(false).GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(msg.Reply))
                    {
                        msg.Respond(result);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Nats handle Message error, Subject: {msg.Subject}");
                }

            };

            IAsyncSubscription subscription = _connection.SubscribeAsync(subject, queue);  //null:Random name, An empty string causes an error.
            subscription.MessageHandler += eventHandler;
            subscription.Start();

            _logger.LogInformation($"Nats subscribe success, Subject:{subject}");
            return new NatsSubscription(subscription);
        }

        #endregion

        public async Task PublishAsync(Type messageType, object message)
        {
            await PublishAsync(messageType, message, null);
        }

        public async Task PublishAsync(Type messageType, object message, AixPublishOptions publishOptions)
        {
            AssertUtils.IsNotNull(message, "message is null");
            var topic = GetTopic(messageType, publishOptions);

            var data = _options.Serializer.Serialize(message);
            //_connection.Publish(topic, data);
            //await Task.CompletedTask;

            await this.PublishAsync(topic, data);
        }

        public async Task<TResult> RequestAsync<TResult>(Type messageType, object message, int timeoutMillisecond)
        {
            return await RequestAsync<TResult>(messageType, message, timeoutMillisecond, null);
        }

        public async Task<TResult> RequestAsync<TResult>(Type messageType, object message, int timeoutMillisecond, AixPublishOptions publishOptions)
        {

            //NATS.Client.NATSTimeoutException 

            AssertUtils.IsNotNull(message, "message is null");
            var topic = GetTopic(messageType, publishOptions);

            var data = _options.Serializer.Serialize(message);
            //var responseMsg = await _connection.RequestAsync(topic, data, GetTimeoutMillisecond(timeoutMillisecond));
            //return _options.Serializer.Deserialize<TResult>(responseMsg.Data);

            var responseMsg = await this.RequestAsync(topic, data, GetTimeoutMillisecond(timeoutMillisecond));
            return _options.Serializer.Deserialize<TResult>(responseMsg);

        }

        public async Task<IMySubscription> SubscribeAsync<T>(Func<T, SubscribeContext, Task> handler, AixSubscribeOptions subscribeOptions = null) where T : class
        {
            return await SubscribeAsync<T>(async (obj, context) =>
            {
                await handler(obj, context);
                return null;
            }, subscribeOptions);
        }

        public async Task<IMySubscription> SubscribeAsync1<T>(Func<T, SubscribeContext, Task<object>> handler, AixSubscribeOptions subscribeOptions = null) where T : class
        {
            string topic = GetTopic(typeof(T), subscribeOptions);
            var queue = subscribeOptions?.Queue;
            var threadCount = subscribeOptions?.ConsumerThreadCount ?? 0;
            threadCount = threadCount > 0 ? threadCount : _options.DefaultConsumerThreadCount;
            AssertUtils.IsTrue(threadCount > 0, "nats subscribe threadCount is zero");
            var autoAck = true;
            SemaphoreSlim semaphoregate = new SemaphoreSlim(threadCount - 1, threadCount);//控制异步 和多线程 

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
            return new NatsSubscription(subscription);
        }

        public async Task<IMySubscription> SubscribeAsync<T>(Func<T, SubscribeContext, Task<object>> handler, AixSubscribeOptions subscribeOptions = null) where T : class
        {
            string topic = GetTopic(typeof(T), subscribeOptions);
            var queue = subscribeOptions?.Queue;
            var threadCount = subscribeOptions?.ConsumerThreadCount ?? 0;
            threadCount = threadCount > 0 ? threadCount : _options.DefaultConsumerThreadCount;
            AssertUtils.IsTrue(threadCount > 0, "nats subscribe threadCount is zero");
            var autoAck = true;


            #region eventHandler

            //byte[], Task<byte[]>
            Func<byte[], Task<byte[]>> eventHandler = async (data) =>
            {
                var obj = _options.Serializer.Deserialize<T>(data);
                var subscribeContext = new SubscribeContext { Topic = topic, Queue = queue };
                var res = await handler(obj, subscribeContext);
                return _options.Serializer.Serialize(res);
            };

            #endregion

            return await this.SubscribeAsync(topic, queue, eventHandler);

        }

        private async Task HandleMessage<T>(Func<T, SubscribeContext, Task<object>> handler, string queue, Msg message)
        {
            string myreply = message.Reply;// message.Header[Constants.MyReply];
            bool isReply = !string.IsNullOrEmpty(myreply);
            try
            {
                var obj = _options.Serializer.Deserialize<T>(message.Data);
                var subscribeContext = new SubscribeContext { Topic = message.Subject, Queue = queue };
                var replyObj = await handler(obj, subscribeContext);
                if (isReply)
                {
                    // _connection.Publish(myreply, _options.Serializer.Serialize(replyObj));
                    message.Respond(_options.Serializer.Serialize(replyObj));
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
