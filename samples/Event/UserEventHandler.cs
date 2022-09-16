using System;
using System.Threading.Tasks;
using AspNetCoreTest.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pluto.EventBus.Abstract.Interfaces;

namespace Event
{
    public class UserEventHandler:IIntegrationEventHandler<UserEvent>
    {
        private readonly ILogger<DemoEventHandler> _logger;
        //private readonly DemoDbContext _demoDbContext;

        public UserEventHandler(ILogger<DemoEventHandler> logger)
        {
            _logger = logger;
        }



        /// <inheritdoc />
        public async Task Handle(UserEvent @event)
        {
            await Task.Yield();
            await Task.Delay(4000);
            _logger.LogInformation($"[UserEventHandler] : {@event.Code}");
        }
    }
}