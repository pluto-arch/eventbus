using System.Threading.Tasks;
using Dncy.EventBus.Abstract.Interfaces;
using Microsoft.Extensions.Logging;

namespace Event
{
    public class DemoEventHandler:IIntegrationEventHandler<DemoEvent>
    {
        private readonly ILogger<DemoEventHandler> _logger;
        public DemoEventHandler(ILogger<DemoEventHandler> logger)
        {
            _logger = logger;
        }



        /// <inheritdoc />
        public async Task Handle(DemoEvent @event)
        {
            await Task.Yield();

            _logger.LogInformation($"[DemoEventHandler] : {@event.Name}");
        }
    }
}