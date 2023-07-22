﻿using Dncy.EventBus.RabbitMQ;
using Dncy.EventBus.RabbitMQ.Connection;
using Dncy.EventBus.RabbitMQ.Options;
using Dncy.EventBus.SubscribeActivator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                l.AddConsole();
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
                    ExchangeName = "orderExchange1",
                    QueueName = "orderCreated2",
                    ConfigExchangeType = ExchangeType.Fanout
                };
                return new EventBusRabbitMQ(connection, declre, msa);
            });



            var app = services.BuildServiceProvider();

            var bus = app.GetRequiredService<EventBusRabbitMQ>();
            // 开始消费
            bus.StartBasicConsume();

            foreach (var item in Enumerable.Range(1, 100))
            {
                // 发送事件
                await bus.PublishAsync(new UserEnableEvent
                {
                    Email = $"{item}@gmail.com"
                });

                await Task.Delay(500);
            }

            Console.ReadKey();
        }
    }
}