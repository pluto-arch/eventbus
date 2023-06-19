## aliyun rocketmq net core事件总线
## 仅支持阿里云rockermq 4和之前的http连接方式。

> 事件处理程序使用激活器方式

### 使用实例

1. 服务注入

```csharp
// 注入消息激活器
builder.Services.AddSingleton<IntegrationEventHandlerActivator>();

// 注入事件总线
builder.Services.AddSingleton<AliyunRocketEventBusCore>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<AliyunRocketEventBusCore>>();
    var msa = sp.GetRequiredService<IntegrationEventHandlerActivator>();
    var options = new AliyunRocketMqOption()
    {
        InstranceId="",
        Topic="",
        GroupId="",
        HttpEndPoint="http://.mqrest.cn-hangzhou.aliyuncs.com",
        AuthenticationConfiguration=new AliyunRocketMqOption.AuthenticationConfig
        {
            AccessId = "",
            AccessKey = "",
        }
    };
    return new AliyunRocketEventBusCore(options,msa,null,logger);
});
```


2. 事件处理程序

```csharp
public class DemoConsumer:MessageHandler
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

    /// subscribe UserRegisterEvent event
    [Subscribe("UserRegisterEvent")]
    public async Task PostMessageHandler(UserRegisterEvent customMessage)
    {
        _logger.LogInformation("收到用户注册事件：{email}. 时间：{time}",customMessage.Email,$"{DateTime.Now:yyyy-mm-dd HH:ss:mm}");

        _scopedService.OutPutHashCode();
        _tranService.OutPutHashCode();
        _signalService.OutPutHashCode();
        await Task.Delay(1000);

    }


    /// subscribe UserDisabledEvent event
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
```