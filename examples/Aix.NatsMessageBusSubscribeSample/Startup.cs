using Aix.NatsMessageBus;
using Aix.NatsMessageBusSubscribeSample.Hosted;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aix.NatsMessageBusSubscribeSample
{
    public static class Startup
    {
        internal static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            var natsMessageBusOptions = context.Configuration.GetSection("nats").Get<NatsMessageBusOptions>();

           services.AddNatsMessageBus(natsMessageBusOptions);
           

            services.AddHostedService<SubscribeHostedService>();
        }
    }
}
