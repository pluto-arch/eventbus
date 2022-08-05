# dncy_eventbus

### 使用方式
参见示例代码


## 事件处理程序示例：
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
