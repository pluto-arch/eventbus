using Pluto.EventBus.Abstract;
using Pluto.EventBus.Abstract.Interfaces;

namespace Dncy.EventBus.NewLiftRocketMQ
{
    public class NewLiftRocketEventBus: IEventBus
    {
        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public void Publish(IntegrationEvent @event)
        {
        }

        /// <inheritdoc />
        public Task PublishAsync(IntegrationEvent @event)
        {
            return null;
        }

        /// <inheritdoc />
        public void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
        }

        /// <inheritdoc />
        public void Unsubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
        }

        /// <inheritdoc />
        public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
        }

        /// <inheritdoc />
        public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
        }
    }
}