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

namespace SubscribeSampleConsoleApp
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
                await NatsDemo(_messageBus);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
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

        private async Task NatsDemo(INatsMessageBus messageBus)
        {
            for (int i = 0; i < 2; i++)
            {


                //订阅模式 Durable 不同，Queue必须有，相同不相同没关系
                await messageBus.SubscribeAsync<OrderDTO>(async (order, context) =>
                {
                    var result = new ReplyResponse();
                    try
                    {
                        var count = Interlocked.Increment(ref Count);
                        _logger.LogInformation("order.new----" + order.OrderId.ToString() + "----------" + count);
                        // await Task.Delay(TimeSpan.FromMilliseconds(10));
                        // throw new Exception("333");
                        await Task.CompletedTask;

                        result.Message = "Success" + order.OrderId.ToString();
                    }
                    catch (Exception ex)
                    {
                        result.Code = -1;
                        result.Message = ex.Message;
                    }

                    return result;
                    //如果传Queue，就是队列模式(只有一个订阅者处理)
                }, new AixSubscribeOptions { ConsumerThreadCount = 8, Topic = "order.>", /*Queue = "order_new_consumer_queue",*/ Durable = "order_new_consumer", DeliverPolicy = NATS.Client.JetStream.DeliverPolicy.New });
            }
            return;
            await messageBus.SubscribeAsync<OrderPayDTO>(async (order, context) =>
            {
                var result = new ReplyResponse();
                try
                {
                    var count = Interlocked.Increment(ref Count);
                    _logger.LogInformation("order.pay----" + order.OrderId.ToString() + "----------" + count);
                    // await Task.Delay(TimeSpan.FromMilliseconds(10));
                    await Task.CompletedTask;

                    result.Message = "Success" + order.OrderId.ToString();
                }
                catch (Exception ex)
                {
                    result.Code = -1;
                    result.Message = ex.Message;
                }

                return result;

            }, new AixSubscribeOptions { ConsumerThreadCount = 8, Queue = "order_pay_consumer_queue", Durable = "order_pay_consumer", DeliverPolicy = NATS.Client.JetStream.DeliverPolicy.New });



            //队列模式  Durable 固定 ，Queue固定
            //await messageBus.SubscribeAsync<UserDTO>(async (user, context) =>
            //{
            //    var count = Interlocked.Increment(ref Count);
            //    _logger.LogInformation("user-----" + user.UserId.ToString() + "----------" + count);
            //    //await Task.Delay(TimeSpan.FromMilliseconds(1));
            //    await Task.CompletedTask;

            //    return new ReplyResponse { Message = "Success" + user.UserId.ToString() };
            //}, new AixSubscribeOptions { ConsumerThreadCount = 4, Queue = "user_queue", Durable = "user_new_consumer" });
        }

    }
}
