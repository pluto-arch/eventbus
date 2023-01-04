using System;
using System.Threading;
using System.Threading.Tasks;
using Dncy.EventBus.Abstract.EventActivator;
using Microsoft.Extensions.Logging;
using rabbitmq_exentbus.Event;

namespace rabbitmq_exentbus.EventConsumers;

public class DemoConsumer:IntegrationEventHandler
{
    private readonly ILogger<DemoConsumer> _logger;

    private static int value = 0;

    public DemoConsumer(ILogger<DemoConsumer> logger)
    {
        _logger = logger;
    }


    [Subscribe("UserRegisterEvent")]
    public async Task PostMessageHandler(UserRegisterEvent customMessage)
    {
        var v= Interlocked.Increment(ref value);
        _logger.LogInformation("收到用户注册事件：{email}. 处理数量：{count}",customMessage.Email,v);
        await Task.Delay(1000);
    }



    [Subscribe("UserDisabledEvent")]
    public async Task UserDisabledEventHandler(UserRegisterEvent customMessage)
    {
        var v= Interlocked.Increment(ref value);
        _logger.LogInformation("收到被禁用事件：{email}. 处理数量：{count}",customMessage.Email,v);
        await Task.Delay(1000);

    }
}