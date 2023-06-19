# dncy_eventbus

### 使用方式
参见示例代码


## 事件处理程序示例
1. 约定模式
```
public class DemoHandler : IIntegrationEventHandler<DemoEvent>
    {
        private readonly ILogger<DemoHandler> _logger;

        public DemoHandler(ILogger<DemoHandler> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public async Task Handle(DemoEvent @event)
        {
            if (@event == null)
            {
                _logger.LogWarning("【DemoHandler】接收到 DemoEvent。 数据为空");
            }

            _logger.LogInformation("【DemoHandler】接收到 DemoEvent 。 数据：{@event}",JsonSerializer.Serialize(@event));
            await Task.Delay(2000);
        }
    }
```
2. 激活器模式
```csharp
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
        Console.WriteLine("UserRegisterEvent");
        await Task.Delay(Random.Next(10, 500));
    }



    [Subscribe("UserDisabledEvent")]
    public async Task UserDisabledEventHandler(UserRegisterEvent customMessage)
    {
        Console.WriteLine("UserDisabledEvent");
        await Task.Delay(Random.Next(15, 560));
    }
}
```
