using Aix.NatsMessageBus.Model;
using Aix.NatsMessageBus.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Aix.NatsMessageBus
{
    internal static class Helper
    {
        /// <summary>
        /// topic缓存
        /// </summary>
        private static ConcurrentDictionary<Type, string> TopicCache = new ConcurrentDictionary<Type, string>();

        private static ConcurrentDictionary<Type, string> StreamCache = new ConcurrentDictionary<Type, string>();

        private static ConcurrentDictionary<Type, PropertyInfo> RouteKeyCache = new ConcurrentDictionary<Type, PropertyInfo>();

        public static string GetPubTopic(Type type, NatsMessageBusOptions options, AixPublishOptions publishOptions)
        {
            var topic = "";
            if (!string.IsNullOrEmpty(publishOptions?.Topic))
            {
                topic = AddTopicPrefix(publishOptions.Topic, options); ;
            }
            else
            {
                topic = Helper.GetTopic(options, type);
            }

            return topic;
        }
        public static string GetSubTopic(Type type, NatsMessageBusOptions options, AixSubscribeOptions subscribeOptions)
        {
            var topic = "";
            if (!string.IsNullOrEmpty(subscribeOptions?.Topic))
            {
                topic = AddTopicPrefix(subscribeOptions.Topic, options); ;
            }
            else
            {
                topic = Helper.GetTopic(options, type);
            }
            

            return topic;
        }

        private static string GetTopic(NatsMessageBusOptions options, Type type)
        {
            string topicName = null;

            if (TopicCache.TryGetValue(type, out topicName))
            {
                return topicName;
            }

            topicName = type.Name; //默认等于该类型的名称
            var topicAttr = TopicAttribute.GetTopicAttribute(type);
            if (topicAttr != null && !string.IsNullOrEmpty(topicAttr.Name))
            {
                topicName = topicAttr.Name;
            }

            topicName = AddTopicPrefix(topicName, options);

            TopicCache.TryAdd(type, topicName);

            return topicName;
        }

        public static string AddTopicPrefix(string name, NatsMessageBusOptions options)
        {
            if (!string.IsNullOrEmpty(options.TopicPrefix) && !name.StartsWith(options.TopicPrefix))
            {
                return $"{options.TopicPrefix ?? ""}{name}";
            }
            return name;
        }

        public static string GetStream(NatsMessageBusOptions options, Type type, AixSubscribeOptions subscribeOptions)
        {
            string stream = null;

            if (StreamCache.TryGetValue(type, out stream))
            {
                return stream;
            }

            var streamAttr = JetStreamAttribute.GetStreamAttribute(type) ?? new JetStreamAttribute();
            if (!string.IsNullOrEmpty(subscribeOptions.Stream))
            {
                stream = AddTopicPrefix(subscribeOptions.Stream, options);
            }
            else if (!string.IsNullOrEmpty(streamAttr.Stream))
            {
                stream = AddTopicPrefix(streamAttr.Stream, options);
            }
            else
            {
                stream = GetTopic(options, type);// + "Stream";
            }

            StreamCache.TryAdd(type, stream);

            return stream;
        }

        public static string GetKey(object message)
        {
            if (message == null) return null;


            var type = message.GetType();
            PropertyInfo property = null;
            if (RouteKeyCache.TryGetValue(type, out property))
            {

            }
            else
            {
                property = AttributeUtils.GetProperty<RouteKeyAttribute>(message);
                RouteKeyCache.TryAdd(type, property);
            }

            var keyValue = property?.GetValue(message);
            return keyValue != null ? keyValue.ToString() : null;
        }
    }
}
