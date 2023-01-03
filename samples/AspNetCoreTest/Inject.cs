using AspNetCoreTest.EventbUSS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pluto.EventBus.Abstract;
using Pluto.EventBus.Abstract.Interfaces;
using Pluto.EventBus.AliyunRocketMQ;
using Pluto.EventBusRabbitMQ;
using Pluto.EventBusRabbitMQ.Connection;
using RabbitMQ.Client;

namespace AspNetCoreTest
{
    public static class Inject
    {
        public static IServiceCollection AddUserEventBus(this IServiceCollection services)
        {
            services.AddSingleton<UserEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBus>>();
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var serializeProvider = sp.GetRequiredService<IMessageSerializeProvider>();
                var options = new AliyunRocketMqOption()
                {
                    InstranceId="",
                    Topic="",
                    GroupId="",
                    HttpEndPoint="",
                    AuthenticationConfiguration=new AliyunRocketMqOption.AuthenticationConfig
                    {
                        AccessId = "",
                        AccessKey = "",
                    }
                };
                return new UserEventBus(serviceFactory,options,serializeProvider,NullIntegrationEventStore.Instance,logger);
            });

            return services;
        }


        public static IServiceCollection AddAdminEventBus(this IServiceCollection services)
        {
            services.AddSingleton<AdminEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBus>>();
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var serializeProvider = sp.GetRequiredService<IMessageSerializeProvider>();
                var options = new AliyunRocketMqOption()
                {
                    InstranceId="",
                    Topic="",
                    GroupId="",
                    HttpEndPoint="",
                    AuthenticationConfiguration=new AliyunRocketMqOption.AuthenticationConfig
                    {
                        AccessId = "",
                        AccessKey = "",
                    }
                };
                return new AdminEventBus(serviceFactory,options,serializeProvider,NullIntegrationEventStore.Instance,logger);
            });

            return services;
        }


        public static IServiceCollection AddUserEventBusByInterface(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus,UserEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBus>>();
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var serializeProvider = sp.GetRequiredService<IMessageSerializeProvider>();
                var options = new AliyunRocketMqOption()
                {
                    InstranceId="",
                    Topic="",
                    GroupId="",
                    HttpEndPoint="",
                    AuthenticationConfiguration=new AliyunRocketMqOption.AuthenticationConfig
                    {
                        AccessId = "",
                        AccessKey = "",
                    }
                };
                return new UserEventBus(serviceFactory,options,serializeProvider,NullIntegrationEventStore.Instance,logger);
            });

            return services;
        }


        public static IServiceCollection AddAdminEventBusByInterface(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus,AdminEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBus>>();
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var serializeProvider = sp.GetRequiredService<IMessageSerializeProvider>();
                var options = new AliyunRocketMqOption()
                {
                    InstranceId="",
                    Topic="",
                    GroupId="",
                    HttpEndPoint="",
                    AuthenticationConfiguration=new AliyunRocketMqOption.AuthenticationConfig
                    {
                        AccessId = "",
                        AccessKey = "",
                    }
                };
                return new AdminEventBus(serviceFactory,options,serializeProvider,NullIntegrationEventStore.Instance,logger);
            });

            return services;
        }




        // *************rabbitmq
        public static IServiceCollection AddRabbitMq(this IServiceCollection services)
        {
            services.AddSingleton<IRabbitMQConnection>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQConnection>>();
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    Port = 5672,
                    DispatchConsumersAsync = true
                };
                factory.UserName = "admin";
                factory.Password = "admin";
                return new DefaultRabbitMQConnection(factory, logger);
            });


            services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
            {
                var connection = sp.GetRequiredService<IRabbitMQConnection>();
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                var serializeProvider = sp.GetRequiredService<IMessageSerializeProvider>();
                return new EventBusRabbitMQ(connection,logger,serializeProvider,new Pluto.EventBusRabbitMQ.Options.RabbitNQDeclaration
                {
                    ExchangeName="订单广播",
                    QueueName = "Default",
                    ExchangeType = ExchangeType.Fanout
                });
            });

            return services;
        }

    }
}