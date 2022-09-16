using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pluto.EventBus.Abstract;
using Pluto.EventBus.Abstract.Interfaces;
using Pluto.EventBus.AliyunRocketMQ;

namespace AspNetCoreTest.EventbUSS
{
    public class AdminEventBus:EventBusRocketMQ
    {
        /// <inheritdoc />
        public AdminEventBus(IServiceScopeFactory serviceFactory, AliyunRocketMqOption option, IMessageSerializeProvider messageSerializeProvider, IIntegrationEventStore eventStore = null, ILogger<EventBusRocketMQ> logger = null, IEventBusSubscriptionsManager subsManager = null) : base(serviceFactory, option, messageSerializeProvider, eventStore, logger, subsManager)
        {
        }

        public override string Name => nameof(AdminEventBus);
    }
}