using Dncy.EventBus.Abstract.Interfaces;
using Dncy.EventBus.RabbitMQ;
using Dncy.EventBus.RabbitMQ.Connection;
using Dncy.EventBus.RabbitMQ.Options;
using Dncy.EventBus.SubscribeActivator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using RabbitMQ.Client;
using Sample.Events;

namespace Sample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            services.AddLogging(l =>
            {
                l.AddSimpleConsole(options =>
                {
                    options.IncludeScopes = true;
                    options.SingleLine = true;
                    options.TimestampFormat = "HH:mm:ss ";
                    options.ColorBehavior = LoggerColorBehavior.Enabled;
                });
                l.SetMinimumLevel(LogLevel.Debug);
            });

            // 注入消息激活器，会将所有的IntegrationEventHandler中使用了Subscribe标记的方法注册进总线
            services.AddSingleton<IntegrationEventHandlerActivator>();


            // 注入rabbitmq链接
            services.AddSingleton<IRabbitMQConnection>(sp =>
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    DispatchConsumersAsync = true
                };
                factory.UserName = "admin";
                factory.Password = "admin";
                return new DefaultRabbitMQConnection(factory);
            });

            // 注入rabbitmq事件总线
            services.AddSingleton<EventBusRabbitMQ>(sp =>
            {
                var connection = sp.GetRequiredService<IRabbitMQConnection>();
                var msa = sp.GetRequiredService<IntegrationEventHandlerActivator>();
                var declre = new RabbitMQDeclaration
                {
                    ExchangeName = "delay_handle_exchange",
                    QueueName = "delay_msgq",
                    ConfigExchangeType = ExchangeType.Fanout
                };
                var logger = sp.GetRequiredService<ILogger<DelayEventBus>>();
                return new EventBusRabbitMQ(connection, declre, msa,logger:logger);
            });
            
            services.AddSingleton<DelayEventBus>(sp =>
            {
                var connection = sp.GetRequiredService<IRabbitMQConnection>();
                var msa = sp.GetRequiredService<IntegrationEventHandlerActivator>();
                var declre = new RabbitMQDeclaration
                {
                    ExchangeName = "delay", // 死信交换机
                    QueueName = "delay_queue1", // 有过期时间的队列  不能加消费者
                    ConfigExchangeType = ExchangeType.Fanout,
                    QueueArguments = new Dictionary<string, object>
                    {
                        { "x-message-ttl", 20000 }, // 设置 TTL，单位为毫秒
                        { "x-dead-letter-exchange", "delay_handle_exchange" },// 代表消息过期后，消息要进入的交换机
                        { "x-dead-letter-routing-key", "/delay_key" } // 是配置消息过期后，进入死信交换机的routing-key,
                        // 以上意思就是，delay 交换机的消息丢入delay_queue1队列，delay_queue1队列是没有消费者的，ttl后会丢入delay_handle_exchange交换机，并将路由key设置为/delay_key
                    }
                };
                var logger = sp.GetRequiredService<ILogger<DelayEventBus>>();
                return new DelayEventBus(connection, declre, msa,logger:logger);
            });



            var app = services.BuildServiceProvider();

            var log = app.GetRequiredService<ILogger<Program>>();
            var ieha = app.GetService<IntegrationEventHandlerActivator>();
            log.LogInformation("订阅处理器信息");
            foreach (var item in ieha.SubscribeList())
            {
                log.LogInformation($"{item.Id} - {item.SubscribeEventBusName}:{item.RouteTemplate}. status:{item.Order}");
            }
            
            var bus = app.GetRequiredService<DelayEventBus>();
            var bus2 = app.GetRequiredService<EventBusRabbitMQ>();
            // 开始消费
            bus.StartDelayQueue(); // 延迟消息eventbus只建议 开始延迟，不可以添加消费者
            bus2.StartBasicConsume();
            // 发送事件
            
            await bus2.PublishAsync(new ChatMessageIntegrationEvent("a","b","ccc")
            {
                RouteKey = "handle_msg"
            });
            await Task.Delay(800);
            
            //var t1 = Task.Run(async () =>
            //{
            //    foreach (var item in Enumerable.Range(1,100))
            //    {
            //        await bus.PublishAsync(new ChatMessageIntegrationEvent("a","b","ccc")
            //        {
            //            RouteKey = "handle_msg"
            //        });
            //        await Task.Delay(800);
            //    }
            //});
           
            //await Task.WhenAll(t1);
            Console.ReadKey();
        }
    }

    public sealed class DelayEventBus:EventBusRabbitMQ
    {
        public DelayEventBus(IRabbitMQConnection connection, RabbitMQDeclaration queueDeclare, IntegrationEventHandlerActivator ieha, ILogger<EventBusRabbitMQ> logger = null, IIntegrationEventStore eventStore = null) : base(connection, queueDeclare, ieha, logger, eventStore)
        {
        }

        public override string Name => nameof(DelayEventBus);
    }
}