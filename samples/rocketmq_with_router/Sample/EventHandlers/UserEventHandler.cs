using Dncy.EventBus.Abstract.EventActivator;
using Dncy.EventBus.Abstract.Models;
using Microsoft.Extensions.Logging;

namespace Sample.EventHandlers
{
    public class UserEventHandler : IntegrationEventHandler
    {
        private readonly ILogger<UserEventHandler> _logger;

        /// <inheritdoc />
        public UserEventHandler(ILogger<UserEventHandler> logger)
        {
            _logger = logger;
        }


        /// subscribe UserRegisterEvent event
        [Subscribe("UserRegisterEvent")]
        public async Task PostMessageHandler(UserRegisterEvent customMessage)
        {
            // 处理 UserRegisterEvent 事件 
            await Task.Delay(1000);

        }


        /// subscribe UserDisabledEvent event
        [Subscribe("UserDisabledEvent")]
        public async Task UserDisabledEventHandler(UserRegisterEvent customMessage)
        {
            // 处理 UserDisabledEvent 事件 
            await Task.Delay(1000);
        }
    }


    public class UserRegisterEvent : IntegrationEvent
    {
        public string Email { get; set; }
    }


    public class UserDisabledEvent : IntegrationEvent
    {
        public string Email { get; set; }
    }
}
