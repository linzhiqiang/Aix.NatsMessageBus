using Aix.NatsMessageBus.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.NatsMessageBus
{

    public interface INatsPublisher
    {
        /// <summary>
        /// publish message
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task PublishAsync(Type messageType, object message);

        Task PublishAsync(Type messageType, object message, AixPublishOptions publishOptions);
    }

    public interface INatsRequester
    {
        Task<TResult> RequestAsync<TResult>(Type messageType, object message, int timeoutMillisecond);

        Task<TResult> RequestAsync<TResult>(Type messageType, object message, int timeoutMillisecond, AixPublishOptions publishOptions);
    }

    public interface INatsSubscriber
    {
        /// <summary>
        /// subscribe message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="subscribeOptions"></param>
        /// <returns></returns>
        Task SubscribeAsync<T>(Func<T, SubscribeContext, Task> handler, AixSubscribeOptions subscribeOptions = null) where T : class;

        /// <summary>
        /// subscribe message and with reply
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="subscribeOptions"></param>
        /// <returns></returns>
        Task SubscribeAsync<T>(Func<T, SubscribeContext, Task<object>> handler, AixSubscribeOptions subscribeOptions = null) where T : class;
    }
    public interface INatsMessageBus : INatsPublisher, INatsRequester, INatsSubscriber, IDisposable
    {

    }

    public interface INatsJSMessageBus : INatsPublisher, INatsRequester, INatsSubscriber, IDisposable
    {

    }


}
