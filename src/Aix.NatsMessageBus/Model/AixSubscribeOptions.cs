using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.NatsMessageBus.Model
{
    /// <summary>
    /// 单个订阅的配置，针对当前订阅有效
    /// </summary>
    public class AixSubscribeOptions
    {
        /// <summary>
        /// 分组 默认取全局配置
        /// </summary>
        public string Queue { get; set; }

        /// <summary>
        /// 消费者线程数 默认取全局配置
        /// </summary>
        public int ConsumerThreadCount { get; set; }

        public string Durable { get; set; }

        public DeliverPolicy DeliverPolicy { get; set; } = DeliverPolicy.All;

        /// <summary>
        /// 覆盖特性配置
        /// </summary>
        public string Topic { get; set; }

        /// <summary>
        /// 覆盖特性配置
        /// </summary>
        public string Stream { get; set; }


    }
}
