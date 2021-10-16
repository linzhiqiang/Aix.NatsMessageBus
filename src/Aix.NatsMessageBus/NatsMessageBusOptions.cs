using Aix.NatsMessageBus.Serializer;
using System;

namespace Aix.NatsMessageBus
{
    public class NatsMessageBusOptions
    {
        public NatsMessageBusOptions()
        {
            DefaultConsumerThreadCount = 2;
            this.Serializer = new DefaultJsonSerializer();
            DefaultTimeoutMillisecond = 3000;

            PendingMessageLimit = 10 * 10000; //65535;// 10 *10000;//default:65535 
            PendingByteLimit = 1048576 * 100;// 1048576 *64; //67108864 64M
        }
        /// <summary>
        /// nats服务地址
        /// </summary>
        public string[] Urls { get; set; }



        /// <summary>
        /// topic前缀，为了防止重复，建议用项目名称
        /// </summary>
        public string TopicPrefix { get; set; }


        /// <summary>
        /// 默认每个Topic的消费线程数 默认2个,请注意与分区数的关系
        /// </summary>
        public int DefaultConsumerThreadCount { get; set; }

        /// <summary>
        /// 自定义序列化，默认为MessagePack
        /// </summary>
        public ISerializer Serializer { get; set; }

        /// <summary>
        /// default value (request-reply) 3000
        /// </summary>
        public int DefaultTimeoutMillisecond { get; set; }

        /// <summary>
        /// for  jetStream default value is false
        /// </summary>
        public bool? AutoAck { get; set; }

        public TimeSpan DefaultMaxAge { get; set; } = TimeSpan.FromDays(14);

        public NatsAuthorization Authorization { get; set; }

        public long PendingMessageLimit { get; set; }

        public long PendingByteLimit { get; set; }
    }

    public class NatsAuthorization
    {
        public string Token { get; set; }

        public string User { get; set; }

        public string Password { get; set; }


    }
}
