using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.NatsMessageBus.Model
{
    public class SubscribeContext
    {
        public string Topic { get; set; }

        public string Queue { get; set; }
    }
}
