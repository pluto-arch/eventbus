using Dncy.EventBus.Abstract.Models;

namespace Event
{
    public class UserEvent:IntegrationEvent
    {
        public string Code { get; set; }
    }
}