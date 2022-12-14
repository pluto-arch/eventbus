using Dncy.EventBus.Abstract.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pluto.EventBus.AliyunRocketMQ;

namespace AspNetCoreTest.EventbUSS
{
    public class AdminEventBus:AliyunRocketEventBus
    {
        /// <inheritdoc />
        public AdminEventBus(IServiceScopeFactory serviceFactory, AliyunRocketMqOption option,  IIntegrationEventStore eventStore = null, ILogger<AliyunRocketEventBus> logger = null, IEventBusSubscriptionsManager subsManager = null) : base(serviceFactory, option, eventStore, logger, subsManager)
        {
        }

        public override string Name => nameof(AdminEventBus);
    }
}