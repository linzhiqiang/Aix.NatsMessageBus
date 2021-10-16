using Aix.NatsMessageBus;
using Aix.NatsMessageBus.Model;
using Aix.NatsMessageBus.Serializer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Aix.NatsMessageBusPublishSample.Hosted
{
    public class PublishHostedService : IHostedService
    {
        private ILogger<PublishHostedService> _logger;
        private INatsMessageBus _natsMessageBus;
        private INatsJSMessageBus _natsJSMessageBus;
        private IHostApplicationLifetime _applicationLifetime { get; }

        static int Count = 0;
        public PublishHostedService(ILogger<PublishHostedService> logger, INatsMessageBus natsMessageBus, INatsJSMessageBus natsJSMessageBus,
            IHostApplicationLifetime hostApplicationLifetime)
        {
            _logger = logger;
            _natsMessageBus = natsMessageBus;
            _natsJSMessageBus = natsJSMessageBus;
            _applicationLifetime = hostApplicationLifetime;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {

            for (int i = 0; i < 10; i++)
            {
               NatsDemo(_natsMessageBus, 1 * 10000);
            }
            for (int i = 0; i < 10; i++)
            {
                NatsJSDemo(_natsJSMessageBus,1*10000);
            }

            await Task.CompletedTask;
        }

        public async Task NatsDemo(INatsMessageBus messageBus,int count )
        {


            for (int i = 0; i < count; i++)
            {
                _applicationLifetime.ApplicationStopping.ThrowIfCancellationRequested();
                try
                {
                    var orderId = Interlocked.Increment(ref Count);
                    var order = new  OrderDTO { OrderId = orderId };

                    _logger.LogInformation($"request order:{order.OrderId}");
                    var replyResult = await messageBus.RequestAsync<OrderDTO, ReplyResponse>(order, 3000);
                    _logger.LogInformation($"reply data:{replyResult.Message}");
                    //await messageBus.PublishAsync(order);

                    _logger.LogInformation($"request payorder:{order.OrderId}");
                    var replyResult2 = await messageBus.RequestAsync<OrderPayDTO, ReplyResponse>(new OrderPayDTO { OrderId = orderId }, 3000);
                    _logger.LogInformation($"reply data:{replyResult2.Message}");
                    //await messageBus.PublishAsync(new OrderPayDTO { OrderId = orderId });


                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "request error");
                }
            }
            await Task.CompletedTask;
        }

        public async Task NatsJSDemo(INatsJSMessageBus messageBus, int count)
        {
            for (int i = 0; i < count; i++)
            {
                _applicationLifetime.ApplicationStopping.ThrowIfCancellationRequested();
                var orderId = Interlocked.Increment(ref Count);
                var order = new OrderDTO
                {
                    OrderId = orderId
                };


                try
                {
                    _logger.LogInformation($"request data:{order.OrderId}");
                    await messageBus.PublishAsync<OrderDTO>(order);

                    await messageBus.PublishAsync<OrderPayDTO>(new OrderPayDTO { OrderId = orderId });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "request error");
                }
            }
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }
    }

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
        public string Message { get; set; }
    }
}

