using Pluto.EventBus.Abstract;

namespace Productor.Event;

public class UserRegisterEvent:IntegrationEvent
{
    public string Email { get; set; }
}