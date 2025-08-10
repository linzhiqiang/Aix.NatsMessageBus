using Aix.NatsMessageBus.Serializer;
using NATS.Client.Internals.SimpleJSON;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common
{
    public class NewtonBinarySerializer : ISerializer
    {
        public T Deserialize<T>(byte[] bytes)
        {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public byte[] Serialize<T>(T data)
        {
            //JsonConvert.SerializeObject(data, Formatting.None, new JsonSerializerSettings
            //{
            //    NullValueHandling = NullValueHandling.Ignore,
            //    DefaultValueHandling = DefaultValueHandling.Ignore,
            //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //    DateFormatString = "yyyy-MM-dd HH:mm:ss"
            //}); 

           var json =  JsonConvert.SerializeObject(data);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}
