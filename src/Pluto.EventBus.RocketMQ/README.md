## aliyun rocketmq net core事件总线

## 仅支持阿里云rockermq 4和之前的http连接方式。

## 使用方式

```csharp
// 事件处理程序
public class DemoHandler : IIntegrationEventHandler<DemoEvent>
{
        /// <inheritdoc />
        public async Task Handle(DemoEvent @event)
        {
            await Task.Delay(2000);
        }
}


// 注册事件，在startup的Configure中可以注册，或者运行中动态注册
var eb = app.Services.GetService<IEventBus>();
eb?.Subscribe<DemoEvent, DemoHandler>();
```