using Microsoft.Extensions.Logging;
using NATS.Client;
using NATS.Client.JetStream;
using System;
using System.Collections.Generic;
using System.Text;
using static NATS.Client.JetStream.StreamConfiguration;

namespace Aix.NatsMessageBus
{
    /// <summary>
    /// nats admin
    /// </summary>
    public class NatsAdmin
    {
        private ILogger<NatsAdmin> _logger;
        private NatsMessageBusOptions _options;
        private IConnection _connection;

        private static object SyncLock = new object();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        /// <param name="connection"></param>
        public NatsAdmin(ILogger<NatsAdmin> logger, NatsMessageBusOptions options, IConnection connection)
        {
            _logger = logger;
            _options = options;
            _connection = connection;
        }

        /// <summary>
        /// add or update stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="streamConfigurationBuilder"></param>
        public void SaveStream(string stream, Action<StreamConfigurationBuilder> streamConfigurationBuilder)
        {
            var builder = StreamConfiguration.Builder();
            streamConfigurationBuilder(builder);

            //var subjects = builder.Build().Subjects ?? new List<string>();
            //var newSubjects = new List<string>();
            //subjects.ForEach(x =>
            //{
            //    newSubjects.Add(Helper.AddTopicPrefix(x, _options));
            //});

            var streamConfiguration = builder.Build();
            //streamConfiguration.Subjects.Clear();
            //streamConfiguration.Subjects.AddRange(newSubjects);

            IJetStreamManagement jsm = _connection.CreateJetStreamManagementContext();
            lock (SyncLock)
            {
                try
                {
                    var streamInfo = jsm.GetStreamInfo(stream); // this throws if the stream does not exist

                    if (streamInfo != null)
                    {
                        jsm.UpdateStream(streamConfiguration);

                    }

                    return;
                }
                catch (NATSJetStreamException) { /* stream does not exist */ }

                jsm.AddStream(streamConfiguration);
            }

        }
    }
}
