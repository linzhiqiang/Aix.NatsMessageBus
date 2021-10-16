using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.NatsMessageBus.Model
{
    public class ReplyResult
    {
        public int Code { get; set; }

        public string Message { get; set; }

        public byte[] Data { get; set; }
    }

   
}
