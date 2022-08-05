using System;
using Microsoft.Extensions.Logging;
using Pluto.EventBus.Abstract;
using Pluto.EventBus.Abstract.Interfaces;
using Pluto.EventBus.RabbitMQ.Connection;
using RabbitMQ.Client;

namespace Pluto.EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private readonly IRabbitMQConnection _connection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IMessageSerializeProvider _messageSerializeProvider;
        private readonly IIntegrationEventStore _eventStore;
        private readonly IEventBusSubscriptionsManager _subsManager;

        public EventBusRabbitMQ(
            IRabbitMQConnection connection, 
            ILogger<EventBusRabbitMQ> logger, 
            IMessageSerializeProvider messageSerializeProvider,
            IIntegrationEventStore eventStore=null,
            IEventBusSubscriptionsManager subsManager=null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger;
            _messageSerializeProvider = messageSerializeProvider;
            _eventStore = eventStore??NullIntegrationEventStore.Instance;
            _subsManager = subsManager??new InMemoryEventBusSubscriptionsManager();
            _subsManager.OnEventRemoved += OnEventRemoved;
            Init();
        }

        private void OnEventRemoved(string eventName, SubscriptionInfo subscriptionInfo)
        {
            if (!_connection.IsConnected)
                _connection.TryConnect();

            using (var channel = _connection.CreateModel())
            {
                channel.QueueUnbind(queue: "_queueName",
                    exchange: "BROKER_NAME",
                    routingKey: eventName);

                if (_subsManager.IsEmpty)
                {
                    _consumerChannel.Close();
                }
            }
        }

        private IModel _consumerChannel;
        private void Init()
        {
            _consumerChannel = CreateConsumerChannel();
        }

        private IModel CreateConsumerChannel()
        {
            return null;
        }






        /// <inheritdoc />
        public void Publish(IntegrationEvent @event)
        {
        }

        /// <inheritdoc />
        public void Subscribe<T, TH>() 
            where T : IntegrationEvent 
            where TH : IIntegrationEventHandler<T>
        {
        }

        /// <inheritdoc />
        public void Unsubscribe<T, TH>() 
            where T : IntegrationEvent 
            where TH : IIntegrationEventHandler<T>
        {
        }

        /// <inheritdoc />
        public void SubscribeDynamic<TH>(string eventName) 
            where TH : IDynamicIntegrationEventHandler
        {
        }

        /// <inheritdoc />
        public void UnsubscribeDynamic<TH>(string eventName) 
            where TH : IDynamicIntegrationEventHandler
        {
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }
    }
}