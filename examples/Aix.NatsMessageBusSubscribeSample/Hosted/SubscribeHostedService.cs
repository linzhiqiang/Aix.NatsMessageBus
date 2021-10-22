using Aix.NatsMessageBus;
using Aix.NatsMessageBus.Model;
using Aix.NatsMessageBus.Serializer;
using Common.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client.Internals;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.NatsMessageBusSubscribeSample.Hosted
{
    public class SubscribeHostedService : IHostedService
    {
        private ILogger<SubscribeHostedService> _logger;
        private INatsMessageBus _natsMessageBus;
        private INatsJSMessageBus _natsJSMessageBus;
        private NatsAdmin _natsAdmin;
        public SubscribeHostedService(ILogger<SubscribeHostedService> logger, INatsMessageBus natsMessageBus, INatsJSMessageBus natsJSMessageBus,
            NatsAdmin natsAdmin)
        {
            _logger = logger;
            _natsMessageBus = natsMessageBus;
            _natsJSMessageBus = natsJSMessageBus;
            _natsAdmin = natsAdmin;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.Run(() => NatsDemo(_natsMessageBus));
                    await Task.Run(() => NatsJSDemo(_natsJSMessageBus));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
            });

            await Task.CompletedTask;
        }

        static int Count = 0;

        private async Task NatsDemo(INatsMessageBus messageBus)
        {
            //订阅模式 Durable 不同，Queue必须有，相同不相同没关系
            await messageBus.SubscribeAsync<OrderDTO>(async (order, context) =>
            {
                var count = Interlocked.Increment(ref Count);
                _logger.LogInformation("order.new----" + order.OrderId.ToString() + "----------" + count);
               // await Task.Delay(TimeSpan.FromMilliseconds(10));
                // throw new Exception("333");
                await Task.CompletedTask;

                return new ReplyResponse { Message = "Success" + order.OrderId.ToString() };
            }, new AixSubscribeOptions { ConsumerThreadCount = 8, Topic = "order.>", Queue = "order_new_consumer_queue", Durable = "order_new_consumer", DeliverPolicy = NATS.Client.JetStream.DeliverPolicy.New });

            await messageBus.SubscribeAsync<OrderPayDTO>(async (order, context) =>
            {
                var count = Interlocked.Increment(ref Count);
                _logger.LogInformation("order.pay----" + order.OrderId.ToString() + "----------" + count);
                // await Task.Delay(TimeSpan.FromMilliseconds(10));
                await Task.CompletedTask;

                return new ReplyResponse { Message = "Success" + order.OrderId.ToString() };
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
        private async Task NatsJSDemo(INatsJSMessageBus messageBus)
        {
            //这里要加上应用前缀   这里是 demo_
            _natsAdmin.SaveStream("demo_order", builder =>
            {
                builder.WithName("demo_order")
                                              //.WithStorageType(StorageType.File)
                                              .WithSubjects("demo_order.>")
                                              // .WithNoAck(false)
                                              .WithMaxAge(Duration.OfDays(1));
            });

            _natsAdmin.SaveStream("demo_user", builder =>
            {
                builder.WithName("demo_user")
                                              //.WithStorageType(StorageType.File)
                                              .WithSubjects("demo_user.>")
                                              // .WithNoAck(false)
                                              .WithMaxAge(Duration.OfDays(1));
            });

            //订阅模式 Durable 不同，Queue必须有，相同不相同没关系
            await messageBus.SubscribeAsync<OrderDTO>(async (order, context) =>
            {
                var count = Interlocked.Increment(ref Count);
                _logger.LogInformation("js----------order.new----" + order.OrderId.ToString() + "----------" + count);
                //  await Task.Delay(TimeSpan.FromMilliseconds(100));
                // throw new Exception("333");
                await Task.CompletedTask;

                return new ReplyResponse { Message = "Success" + order.OrderId.ToString() };
            }, new AixSubscribeOptions { ConsumerThreadCount = 8, Topic = "order.>", Queue = "order_new_consumer_queue", Durable = "order_new_consumer", DeliverPolicy = NATS.Client.JetStream.DeliverPolicy.New });

            await messageBus.SubscribeAsync<OrderPayDTO>(async (order, context) =>
            {
                var count = Interlocked.Increment(ref Count);
                _logger.LogInformation("js------------order.pay----" + order.OrderId.ToString() + "----------" + count);
                // await Task.Delay(TimeSpan.FromMilliseconds(100));
                await Task.CompletedTask;

                return new ReplyResponse { Message = "Success" + order.OrderId.ToString() };
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

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }


    
}
