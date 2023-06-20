using Dncy.EventBus.Abstract.Models;

namespace Sample.Events;

public class UserRegisterEvent : IntegrationEvent
{
    public string Email { get; set; }
}


public class UserDisabledEvent : IntegrationEvent
{
    public string Email { get; set; }
}


public class UserEnableEvent : IntegrationEvent
{
    public string Email { get; set; }
}