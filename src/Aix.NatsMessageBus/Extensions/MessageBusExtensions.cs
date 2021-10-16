using Aix.NatsMessageBus.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aix.NatsMessageBus
{
    public static class MessageBusExtensions
    {
        public static Task PublishAsync<T>(this INatsMessageBus messageBus, T message)
        {
            return messageBus.PublishAsync(typeof(T), message);
        }

        public static Task<TResult> RequestAsync<T, TResult>(this INatsMessageBus messageBus,T message, int timeoutMillisecond)
        {
            return messageBus.RequestAsync<TResult>(typeof(T), message, timeoutMillisecond);
        }

        public static Task<TResult> RequestAsync<T, TResult>(this INatsMessageBus messageBus, T message, int timeoutMillisecond, AixPublishOptions publishOptions)
        {
            return messageBus.RequestAsync<TResult>(typeof(T), message, timeoutMillisecond, publishOptions);
        }
       

    }

    public static class JSMessageBusExtensions
    {
        public static Task PublishAsync<T>(this INatsJSMessageBus messageBus, T message)
        {
            return messageBus.PublishAsync(typeof(T), message);
        }

        public static Task<TResult> RequestAsync<T, TResult>(this INatsJSMessageBus messageBus, T message, int timeoutMillisecond)
        {
            return messageBus.RequestAsync<TResult>(typeof(T), message, timeoutMillisecond);
        }

        public static Task<TResult> RequestAsync<T, TResult>(this INatsJSMessageBus messageBus, T message, int timeoutMillisecond, AixPublishOptions publishOptions)
        {
            return messageBus.RequestAsync<TResult>(typeof(T), message, timeoutMillisecond, publishOptions);
        }


    }
}
