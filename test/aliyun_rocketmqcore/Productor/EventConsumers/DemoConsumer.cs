using Dncy.EventBus.Abstract.EventActivator;
using Productor.Event;
using Productor.Services;

namespace Productor.EventConsumers;

public class DemoConsumer:IntegrationEventHandler
{
    private readonly ILogger<DemoConsumer> _logger;

    private readonly IScopedService _scopedService;
    private readonly ITranService _tranService;
    private readonly ISignalService _signalService;

    public DemoConsumer(ILogger<DemoConsumer> logger, IScopedService scopedService, ITranService tranService, ISignalService signalService)
    {
        _logger = logger;
        _scopedService = scopedService;
        _tranService = tranService;
        _signalService = signalService;
    }


    [Subscribe("UserRegisterEvent")]
    public async Task PostMessageHandler(UserRegisterEvent customMessage)
    {
        _logger.LogInformation("收到用户注册事件：{email}. 时间：{time}",customMessage.Email,$"{DateTime.Now:yyyy-mm-dd HH:ss:mm}");
        _scopedService.OutPutHashCode();
        _tranService.OutPutHashCode();
        _signalService.OutPutHashCode();
        await Task.Delay(1000);

    }



    [Subscribe("UserDisabledEvent")]
    public async Task UserDisabledEventHandler(UserRegisterEvent customMessage)
    {
        _logger.LogInformation("收到被禁用事件：{email}. 时间：{time}",customMessage.Email,$"{DateTime.Now:yyyy-mm-dd HH:ss:mm}");
        _scopedService.OutPutHashCode();
        _tranService.OutPutHashCode();
        _signalService.OutPutHashCode();
        await Task.Delay(1000);

    }
}