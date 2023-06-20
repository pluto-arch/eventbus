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


    [Subscribe("UserRegisterEvent")]
    public async Task PostMessageHandler(UserRegisterEvent customMessage)
    {
        Console.WriteLine($"UserRegisterEvent : {customMessage.Email}");
        await Task.Delay(Random.Next(10, 500));
    }



    [Subscribe("UserDisabledEvent")]
    public async Task UserDisabledEventHandler(UserRegisterEvent customMessage)
    {
        Console.WriteLine($"UserDisabledEvent : {customMessage.Email}");
        await Task.Delay(Random.Next(15, 560));
    }
}