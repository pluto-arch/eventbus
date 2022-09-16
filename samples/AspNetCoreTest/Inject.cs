using AspNetCoreTest.EventbUSS;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pluto.EventBus.Abstract;
using Pluto.EventBus.Abstract.Interfaces;
using Pluto.EventBus.AliyunRocketMQ;

namespace AspNetCoreTest
{
    public static class Inject
    {
        public static IServiceCollection AddUserEventBus(this IServiceCollection services)
        {
            services.AddSingleton<UserEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<EventBusRocketMQ>>();
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
                var logger = sp.GetRequiredService<ILogger<EventBusRocketMQ>>();
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
                var logger = sp.GetRequiredService<ILogger<EventBusRocketMQ>>();
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var serializeProvider = sp.GetRequiredService<IMessageSerializeProvider>();
                var options = new AliyunRocketMqOption()
                {
                    InstranceId="",
                    Topic="",
                    GroupId="",
                    HttpEndPoint="http://1226776583375087.mqrest.cn-hangzhou.aliyuncs.com",
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
                var logger = sp.GetRequiredService<ILogger<EventBusRocketMQ>>();
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
    }
}