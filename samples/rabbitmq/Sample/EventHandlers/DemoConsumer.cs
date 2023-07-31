using Dncy.EventBus.RabbitMQ;
using Dncy.EventBus.SubscribeActivator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Sample.Events;

namespace Sample.EventHandlers;

public class DemoConsumer : IntegrationEventHandler
{
    private readonly ILogger<DemoConsumer> _logger;

    private static readonly Random Random = new Random();

    public DemoConsumer(ILogger<DemoConsumer> logger = null)
    {
        _logger = logger ?? NullLogger<DemoConsumer>.Instance;
    }


    //[Subscribe("UserRegisterEvent")]
    //public async Task PostMessageHandler(UserRegisterEvent customMessage)
    //{
    //    //Console.WriteLine($"用户注册 : {customMessage.Email}");
    //    await Task.Delay(Random.Next(10, 500));
    //}



    //[Subscribe("UserDisabledEvent")]
    //public async Task UserDisabledEventHandler(UserRegisterEvent customMessage)
    //{
    //    //Console.WriteLine($"停用用户 : {customMessage.Email}");
    //    await Task.Delay(Random.Next(15, 560));
    //}


    [Subscribe("UserEnableEvent",nameof(EventBusRabbitMQ))]
    public async Task UserEnableEventHandler(UserEnableEvent customMessage)
    {
        Console.WriteLine($"启用用户1 : {customMessage.Email}");
        await Task.Delay(Random.Next(15, 560));
    }
    
    
    [Subscribe("delay_key",nameof(EventBusRabbitMQ))]
    public async Task DelayMsg(UserEnableEvent customMessage)
    {
        Console.WriteLine($"启用用户1 - 延迟后的 : {customMessage.Email} - 时间：{DateTime.Now}");
        await Task.Delay(Random.Next(15, 560));
    }

    //[Subscribe("UserEnableEvent")]
    //public async Task UserEnableEventHandler2(UserEnableEvent customMessage)
    //{
    //    Console.WriteLine($"启用用户2 : {customMessage.Email}");
    //    await Task.Delay(Random.Next(15, 560));
    //}
}