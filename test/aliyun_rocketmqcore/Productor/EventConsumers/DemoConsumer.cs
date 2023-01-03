using Dncy.MQMessageActivator;
using Productor.Event;

namespace Productor.EventConsumers;

public class DemoConsumer:MessageHandler
{
    private readonly ILogger<DemoConsumer> _logger;

    public DemoConsumer(ILogger<DemoConsumer> logger)
    {
        _logger = logger;
    }


    [Subscribe("/UserRegisterEvent")]
    public async Task PostMessage(UserRegisterEvent customMessage)
    {
        _logger.LogInformation("收到UserRegister事件：{email}. 时间：{time}",customMessage.Email,$"{DateTime.Now:yyyy-mm-dd HH:ss:mm}");
        await Task.Delay(1000);

    }

}