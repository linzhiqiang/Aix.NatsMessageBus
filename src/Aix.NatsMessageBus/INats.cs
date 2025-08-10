using Aix.NatsMessageBus.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.NatsMessageBus
{
    /// <summary>
    /// 
    /// </summary>
    public interface INatsPublisher
    {
        /// <summary>
        /// publish message
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        Task PublishAsync(Type messageType, object message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="publishOptions"></param>
        /// <returns></returns>
        Task PublishAsync(Type messageType, object message, AixPublishOptions publishOptions);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface INatsRequester
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="timeoutMillisecond"></param>
        /// <returns></returns>
        Task<TResult> RequestAsync<TResult>(Type messageType, object message, int timeoutMillisecond);

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="messageType"></param>
        /// <param name="message"></param>
        /// <param name="timeoutMillisecond"></param>
        /// <param name="publishOptions"></param>
        /// <returns></returns>
        Task<TResult> RequestAsync<TResult>(Type messageType, object message, int timeoutMillisecond, AixPublishOptions publishOptions);
    }

    /// <summary>
    /// 
    /// </summary>
    public interface INatsSubscriber
    {
        /// <summary>
        /// subscribe message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="subscribeOptions"></param>
        /// <returns></returns>
        Task<IMySubscription> SubscribeAsync<T>(Func<T, SubscribeContext, Task> handler, AixSubscribeOptions subscribeOptions = null) where T : class;

        /// <summary>
        /// subscribe message and with reply
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        /// <param name="subscribeOptions"></param>
        /// <returns></returns>
        Task<IMySubscription> SubscribeAsync<T>(Func<T, SubscribeContext, Task<object>> handler, AixSubscribeOptions subscribeOptions = null) where T : class;
    }
}
