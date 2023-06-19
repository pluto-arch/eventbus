using Dncy.EventBus.AliyunRocketMQCore;
using Dncy.EventBus.AliyunRocketMQCore.Options;
using Dncy.EventBus.SubscribeActivator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sample.EventHandlers;

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

            // 注入事件总线
            services.AddSingleton<AliyunRocketEventBusCore>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBusCore>>();
                var msa = sp.GetRequiredService<IntegrationEventHandlerActivator>();
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
                return new AliyunRocketEventBusCore(options, msa, null, logger);
            });

            var provider = services.BuildServiceProvider();
            var bus = provider.GetRequiredService<AliyunRocketEventBusCore>();
            // 开始消费
            await bus.StartBasicConsume();


            // 发送事件
            await bus.PublishAsync(new UserDisabledEvent
            {
                Email = "sample@ddd.com"
            });
            Console.ReadKey();
        }
    }
}