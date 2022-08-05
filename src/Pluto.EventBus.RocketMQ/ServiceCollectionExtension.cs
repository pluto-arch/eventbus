using System;
using Aliyun.MQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pluto.EventBus.Abstract.Interfaces;

namespace Pluto.EventBus.AliyunRocketMQ
{
    public static class ServiceCollectionExtension
    {
        public static void AddAliRocketMQ(this IServiceCollection service,Action<AliyunRocketMqOption> optionAction)
        {
            service.Configure<AliyunRocketMqOption>(optionAction);
            service.AddSingleton<IEventBus, EventBusRocketMQ>();
        }

        public static void AddAliRocketMQ(this IServiceCollection service,IConfiguration configuration)
        {
            service.Configure<AliyunRocketMqOption>(opt =>
            {
                configuration.GetSection("AliyunRocketMQ").Bind(opt);
            });
            service.AddSingleton<IEventBus, EventBusRocketMQ>();
        }
    }
}