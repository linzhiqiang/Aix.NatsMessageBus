using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.NatsMessageBus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNatsMessageBus(this IServiceCollection services, NatsMessageBusOptions options)
        {
            //var connection = CreateConnection(options);
            services
               .AddSingleton(options)
                .AddSingleton<INatsMessageBus, NatsMessageBus>()
               .AddSingleton<INatsJSMessageBus, NatsJetStreamMessageBus>()
               .AddSingleton<NatsAdmin>();

            services.AddSingleton<IConnection>((serviceProvider) =>
            {
                return CreateConnection(serviceProvider, options);
            });
            return services;
        }

        //public static IServiceCollection AddNatsJSMessageBus(this IServiceCollection services, NatsMessageBusOptions options)
        //{
        //    services
        //       .AddSingleton(options)
        //        .AddSingleton<INatsMessageBus, NatsMessageBus>()
        //       .AddSingleton<INatsJSMessageBus, NatsJetStreamMessageBus>()
        //       .AddSingleton<NatsAdmin>();

        //    services.AddSingleton<IConnection>((serviceProvider) =>
        //    {
        //        return CreateConnection(serviceProvider, options);
        //    });
        //    return services;
        //}

        private static IConnection CreateConnection(IServiceProvider serviceProvider, NatsMessageBusOptions options)
        {
            var logger = serviceProvider.GetService<ILogger<NatsMessageBus>>();
            IConnection connection = null;
            try
            {
                var opts = ConnectionFactory.GetDefaultOptions();
                opts.MaxReconnect = Options.ReconnectForever;
                opts.ReconnectWait = 1000;

                opts.Url = "";// "nats://localhost:4222";
                opts.Servers = options.Urls;
                if (options.Authorization != null)
                {
                    opts.Token = options.Authorization.Token;
                    opts.User = options.Authorization.User;
                    opts.Password = options.Authorization.Password;
                }

                RegisterConnectionEvent(logger, opts);
                connection = new ConnectionFactory().CreateConnection(opts);


            }
            catch (Exception ex)
            {
                logger.LogError(ex, "create nats connection error");
                throw;
            }
            return connection;
        }

        private static void RegisterConnectionEvent(ILogger<NatsMessageBus> logger, Options options)
        {
            options.AsyncErrorEventHandler += (sender, args) =>
            {
                var server = string.Join(";", args.Conn.Opts.Servers);
                var errorMsg = $"Nats-AsyncErrorEventHandler  Server:{ server},Subject:{args.Subscription?.Subject},Message:{args.Error}";
                logger.LogInformation(args.Error, errorMsg);
            };


            options.DisconnectedEventHandler += (sender, args) =>
            {
                var server = string.Join(";", args.Conn.Opts.Servers);

                var errorMsg = $"Nats-DisconnectedEventHandler  Server:{ server},Message:{args.Error}";
                logger.LogInformation(args.Error, errorMsg);
                //Nats-DisconnectedEventHandler  Server:nats://localhost:4222,Message:NATS.Client.NATSSlowConsumerException: Consumer is too slow.
               //NATS.Client.NATSSlowConsumerException: Consumer is too slow.
            };


            options.ReconnectedEventHandler += (sender, args) =>
            {
                var server = string.Join(";", args.Conn.Opts.Servers);
                var errorMsg = $"Nats-ReconnectedEventHandler  Server:{server},Message:{args.Error}";
                logger.LogInformation(args.Error,errorMsg);
            };

            options.ClosedEventHandler += (sender, args) =>
            {
                var server = string.Join(";", args.Conn.Opts.Servers);
                var errorMsg = $"Nats-ClosedEventHandler  Server:{ server},Message:{args.Error}";
                logger.LogInformation(args.Error, errorMsg);
            };
        }
    }
}
