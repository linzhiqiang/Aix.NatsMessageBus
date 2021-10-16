using Aix.NatsMessageBus.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.NatsMessageBus.Model
{
   public class JetStreamAttribute : Attribute
    {
      
        /// <summary>
        /// Stream
        /// </summary>
        public string Stream { get; set; }

        /// <summary>
        /// 覆盖全局配置
        /// </summary>
        //public TimeSpan MaxAge { get; set; } = TimeSpan.Zero;

        public JetStreamAttribute()
        {
        }

        public static JetStreamAttribute GetStreamAttribute(Type type)
        {
            return AttributeUtils.GetAttribute<JetStreamAttribute>(type);

        }
    }
}
