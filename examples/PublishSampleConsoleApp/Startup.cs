using Aix.NatsMessageBus;
using Aix.NatsMessageBus.Model;
using Common;
using Common.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PublishSampleConsoleApp
{
    internal class Startup
    {
        ILogger<Startup> _logger;
        INatsMessageBus _messageBus;

        public async Task Start()
        {

            try
            {
                await Initialize();
                await NatsDemo(_messageBus, 10);
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message);
            }
        }

        public void Stop()
        {
            IoCContainer.Instance.Dispose();
            _messageBus?.Dispose();
        }


        public async Task Initialize()
        {
            IoCContainer.Instance.ServiceCollection.AddLogging(builder =>
            {
                builder.AddConsole(configure =>
                {
                    // 可以配置 Console 日志选项
                    configure.LogToStandardErrorThreshold = LogLevel.Error;
                    // 其他配置...
                })
                .SetMinimumLevel(LogLevel.Debug); // 设置全局最低日志级别
            });

            IoCContainer.Instance.ServiceCollection.AddNatsMessageBus(new NatsMessageBusOptions
            {
                /*
                 {
    "topicPrefix": "demo_",
    "urls": [ "nats://localhost:4222" ],
    "PendingMessageLimit11": 1000,
    "Authorization": {
      "Token": "123456"
    }
                 */

                TopicPrefix = "demo_",
                Urls = new[] { "nats://localhost:4422" },
                DefaultConsumerThreadCount = 1,
                Authorization = new NatsAuthorization
                {
                    Token = "123456"
                }

            });

            IoCContainer.Instance.Build();

            _logger = IoCContainer.Instance.GetService<ILogger<Startup>>();
            _messageBus = IoCContainer.Instance.GetService<INatsMessageBus>();
        }


        static int Count = 0;


        public async Task NatsDemo(INatsMessageBus messageBus, int count)
        {
            for (int i = 0; i < count; i++)
            {
                try
                {
                    var orderId = Interlocked.Increment(ref Count);
                    var order = new OrderDTO { OrderId = orderId };

                    _logger.LogInformation($"request order:{order.OrderId}");
                    //var replyResult = await messageBus.RequestAsync<OrderDTO, ReplyResponse>(order, 3000);
                    //_logger.LogInformation($"reply data:{replyResult.Message}");
                    await messageBus.PublishAsync(order);

                    //_logger.LogInformation($"request payorder:{order.OrderId}");
                    //var replyResult2 = await messageBus.RequestAsync<OrderPayDTO, ReplyResponse>(new OrderPayDTO { OrderId = orderId }, 3000);
                    //_logger.LogInformation($"reply data:{replyResult2.Message}");
                    ////await messageBus.PublishAsync(new OrderPayDTO { OrderId = orderId });


                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "request error");
                }
            }
            await Task.CompletedTask;
        }
    }
}
