using System;
using Dncy.EventBus.Abstract.Models;

namespace Event
{
    public class DemoEvent:IntegrationEvent
    {

        public string Name { get; set; }

    }
}
