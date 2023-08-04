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


public class ChatMessageIntegrationEvent:IntegrationEvent
{
    public string User { get; }
    public string ToUser { get; }
    public string Message { get; }

    public ChatMessageIntegrationEvent(string user,string toUser,string message)
    {
        User = user;
        ToUser = toUser;
        Message = message;
    }

    public ChatMessageIntegrationEvent()
    {
            
    }
}