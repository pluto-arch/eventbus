using AspNetCoreTest.EventbUSS;
using Dncy.EventBus.Abstract;
using Dncy.EventBus.Abstract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
                return new UserEventBus(serviceFactory,options);
            });

            return services;
        }


        public static IServiceCollection AddAdminEventBus(this IServiceCollection services)
        {
            services.AddSingleton<AdminEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBus>>();
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
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
                return new AdminEventBus(serviceFactory,options);
            });

            return services;
        }


        public static IServiceCollection AddUserEventBusByInterface(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus,UserEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBus>>();
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
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
                return new UserEventBus(serviceFactory,options,NullIntegrationEventStore.Instance,logger);
            });

            return services;
        }


        public static IServiceCollection AddAdminEventBusByInterface(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus,AdminEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBus>>();
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
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
                return new AdminEventBus(serviceFactory,options,NullIntegrationEventStore.Instance,logger);
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

            return services;
        }

    }
}