using Dncy.EventBus.Abstract;
using Dncy.EventBus.Abstract.Interfaces;
using Dncy.EventBus.Abstract.Models;
using Dncy.EventBus.AliyunRocketMQ;
using Dncy.EventBus.AliyunRocketMQ.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.EventBuses;

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

            // 1、注入事件处理程序
            services.AddTransient<UserEventHnadler>();

            // 2. 注册事件总线
            services.AddUserEventBus();
            services.AddAdminEventBus();

            var provider = services.BuildServiceProvider();

            var bus = provider.GetService<UserEventBus>();
            // 注册事件处理程序到事件总线
            bus?.Subscribe<UserEvent, UserEventHnadler>();

            // 发布事件到事件总线
            await bus?.PublishAsync(new UserEvent { Code = Guid.NewGuid().ToString() })!;

            Console.ReadKey();
        }
    }


    public class UserEvent : IntegrationEvent
    {
        public string Code { get; set; }
    }

    public class UserEventHnadler : IIntegrationEventHandler<UserEvent>
    {
        /// <inheritdoc />
        public async Task Handle(UserEvent @event)
        {
            // TODO 处理事件数据
            await Task.CompletedTask;
        }
    }



    public static class Inject
    {

        /*
         * 下边是使用自定义eventbus类的方式
         */

        public static IServiceCollection AddUserEventBus(this IServiceCollection services)
        {
            services.AddSingleton<UserEventBus>(sp =>
            {
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var options = new AliyunRocketMqOption()
                {
                    InstranceId = "",
                    Topic = "",
                    GroupId = "",
                    HttpEndPoint = "",
                    AuthenticationConfiguration = new AliyunRocketMqOption.AuthenticationConfig
                    {
                        AccessId = "",
                        AccessKey = "",
                    }
                };
                return new UserEventBus(serviceFactory, options);
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
                    InstranceId = "",
                    Topic = "",
                    GroupId = "",
                    HttpEndPoint = "",
                    AuthenticationConfiguration = new AliyunRocketMqOption.AuthenticationConfig
                    {
                        AccessId = "",
                        AccessKey = "",
                    }
                };
                return new AdminEventBus(serviceFactory, options);
            });

            return services;
        }

        /*
         * 下边是非自定义eventbus的使用接口解析的方式
         */


        public static IServiceCollection AddUserEventBusByInterface(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus, UserEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBus>>();
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var options = new AliyunRocketMqOption()
                {
                    InstranceId = "",
                    Topic = "",
                    GroupId = "",
                    HttpEndPoint = "",
                    AuthenticationConfiguration = new AliyunRocketMqOption.AuthenticationConfig
                    {
                        AccessId = "",
                        AccessKey = "",
                    }
                };
                return new UserEventBus(serviceFactory, options, NullIntegrationEventStore.Instance, logger);
            });

            return services;
        }


        public static IServiceCollection AddAdminEventBusByInterface(this IServiceCollection services)
        {
            services.AddSingleton<IEventBus, AdminEventBus>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBus>>();
                var serviceFactory = sp.GetRequiredService<IServiceScopeFactory>();
                var options = new AliyunRocketMqOption()
                {
                    InstranceId = "",
                    Topic = "",
                    GroupId = "",
                    HttpEndPoint = "",
                    AuthenticationConfiguration = new AliyunRocketMqOption.AuthenticationConfig
                    {
                        AccessId = "",
                        AccessKey = "",
                    }
                };
                return new AdminEventBus(serviceFactory, options, NullIntegrationEventStore.Instance, logger);
            });

            return services;
        }
    }
}