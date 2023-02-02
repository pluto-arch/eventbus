using Dncy.EventBus.Abstract.Interfaces;
using Dncy.EventBus.AliyunRocketMQ;
using Dncy.EventBus.AliyunRocketMQ.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Sample.EventBuses
{
    public class UserEventBus : AliyunRocketEventBus
    {
        /// <inheritdoc />
        public UserEventBus(IServiceScopeFactory serviceFactory, AliyunRocketMqOption option, IIntegrationEventStore eventStore = null, ILogger<AliyunRocketEventBus> logger = null, IEventBusSubscriptionsManager subsManager = null) : base(serviceFactory, option, eventStore, logger, subsManager)
        {
        }


        public override string Name => nameof(UserEventBus);
    }
}