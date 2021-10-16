using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

namespace Aix.NatsMessageBus.Serializer
{
    public class DefaultJsonSerializer : ISerializer
    {
        static JsonSerializerOptions Options = new JsonSerializerOptions
        {
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Create(UnicodeRanges.All),
            //WriteIndented=true //格式化的
        };
        public T Deserialize<T>(byte[] bytes)
        {
            return JsonSerializer.Deserialize<T>(bytes, Options);
        }

        public byte[] Serialize<T>(T data)
        {
            byte[] jsonUtf8Bytes = JsonSerializer.SerializeToUtf8Bytes(data, Options);
            return jsonUtf8Bytes;
        }
    }
    public class JsonSerializer2 : ISerializer
    {
        public T Deserialize<T>(byte[] bytes)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(T));
                stream.Write(bytes, 0, bytes.Length);
                stream.Position = 0;
                return (T)serializer.ReadObject(stream);
            }
        }

        public byte[] Serialize<T>(T data)
        {
            if (data == null)
                return null;

            var serializer = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream stream = new MemoryStream())
            {
                serializer.WriteObject(stream, data);
                return stream.ToArray();
            }
        }
    }
}
