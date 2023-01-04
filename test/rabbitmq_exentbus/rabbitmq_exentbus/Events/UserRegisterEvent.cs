using Dncy.EventBus.Abstract.Models;

namespace rabbitmq_exentbus.Event;

public class UserRegisterEvent:IntegrationEvent
{
    public string Email { get; set; }
}


public class UserDisabledEvent:IntegrationEvent
{
    public string Email { get; set; }
}