using Aix.NatsMessageBus.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.Models
{
    [Topic(Name = "order.new")]
    [JetStream(Stream = "order")]
    public class OrderDTO
    {
        public int OrderId { get; set; }
    }

    [Topic(Name = "order.pay")]
    [JetStream(Stream = "order")]
    public class OrderPayDTO
    {
        public int OrderId { get; set; }
    }

    [Topic(Name = "user.new")]
    [JetStream(Stream = "user")]
    public class UserDTO
    {
        public int UserId { get; set; }
    }

    public class ReplyResponse
    {
        public int Code { get; set; }
        public string Message { get; set; }
    }
}
