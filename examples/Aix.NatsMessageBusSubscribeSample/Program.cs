using Microsoft.Extensions.Hosting;
using System;

namespace Aix.NatsMessageBusSubscribeSample
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .ConfigureHostConfiguration(configurationBuilder =>
            {

            })
           .ConfigureAppConfiguration((hostBulderContext, configurationBuilder) =>
           {
           })
            .ConfigureLogging((hostBulderContext, loggingBuilder) =>
            {

            })
            .ConfigureServices(Startup.ConfigureServices);

        }
    }
}
