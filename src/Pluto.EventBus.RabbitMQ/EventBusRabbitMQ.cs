using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pluto.EventBus.Abstract;
using Pluto.EventBus.Abstract.Interfaces;
using Pluto.EventBus.RabbitMQ.Connection;
using Pluto.EventBus.RabbitMQ.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Pluto.EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private readonly IRabbitMQConnection _connection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IMessageSerializeProvider _messageSerializeProvider;
        private readonly IIntegrationEventStore _eventStore;
        private readonly IEventBusSubscriptionsManager _subsManager;

        private readonly QueueDeclare _queueDeclare;

        public EventBusRabbitMQ(
            IRabbitMQConnection connection, 
            ILogger<EventBusRabbitMQ> logger, 
            IMessageSerializeProvider messageSerializeProvider,
            QueueDeclare queueDeclare,
            IIntegrationEventStore eventStore=null,
            IEventBusSubscriptionsManager subsManager=null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger;
            _messageSerializeProvider = messageSerializeProvider;
            _queueDeclare = queueDeclare;
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
                channel.QueueUnbind(queue: _queueDeclare.QueueName,
                    exchange: Name,
                    routingKey: eventName);
            }
        }

        private IModel _channel;
        private void Init()
        {
            _channel ??= CreateConsumerChannel();
        }

        private IModel CreateConsumerChannel()
        {
            if (!_connection.IsConnected)
                _connection.TryConnect();
            var channel = _connection.CreateModel();
            channel.ExchangeDeclare(exchange: Name,
                type: _queueDeclare.ExchangeType);

            channel.QueueDeclare(queue: _queueDeclare.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, $"Recreating RabbitMQ channel");
                _channel?.Dispose();
                _channel = CreateConsumerChannel();
                StartBasicConsume();
            };
            return channel;
        }


        /// <inheritdoc />
        public string Name => nameof(EventBusRabbitMQ);

        /// <inheritdoc />
        public void Publish(IntegrationEvent @event)
        {
            if (!_connection.IsConnected)
                _connection.TryConnect();

            var eventName = @event.GetType().Name;
            if (_channel==null)
            {
                _channel = CreateConsumerChannel();
                StartBasicConsume();
            }
            var properties = _channel.CreateBasicProperties();
            properties.DeliveryMode = 2; // persistent
            _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);
            var body =Encoding.UTF8.GetBytes(_messageSerializeProvider.Serialize(@event));
            _channel.BasicPublish(
                exchange: Name,
                routingKey: eventName,
                mandatory: true,
                basicProperties: properties,
                body: body);
        }

        /// <inheritdoc />
        public Task PublishAsync(IntegrationEvent @event)
        {
            if (!_connection.IsConnected)
                _connection.TryConnect();

            var eventName = @event.GetType().Name;
            if (_channel==null)
            {
                _channel = CreateConsumerChannel();
                StartBasicConsume();
            }
            var properties = _channel.CreateBasicProperties();
            properties.DeliveryMode = 2; // persistent
            _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);
            var body =Encoding.UTF8.GetBytes(_messageSerializeProvider.Serialize(@event));
            _channel.BasicPublish(
                exchange: Name,
                routingKey: eventName,
                mandatory: true,
                basicProperties: properties,
                body: body);
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Subscribe<T, TH>() 
            where T : IntegrationEvent 
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();
            DoInternalSubscription(eventName);
            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).GetGenericTypeName());
            _subsManager.AddSubscription<T, TH>(this.Name??nameof(EventBusRabbitMQ));
            StartBasicConsume();
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


        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume ...");

            if (_channel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.Received += Consumer_Received; ;
                _channel.BasicConsume(
                    queue: _queueDeclare.QueueName,
                    autoAck: false,
                    consumer: consumer);
            }
            else
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }


        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName,this.Name??nameof(EventBusRabbitMQ));
            if (!containsKey)
            {
                if (!_connection.IsConnected)
                {
                    _connection.TryConnect();
                }
                _channel.QueueBind(queue: _queueDeclare.QueueName,
                    exchange: Name,
                    routingKey: eventName);
            }
        }




        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var message = Encoding.UTF8.GetString(@event.Body.Span);
            try
            {
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }

                _logger.LogInformation("receiver message from mq :{eventName}",eventName);
                await Task.CompletedTask;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                // Even on exception we take the message off the queue.
                // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
                // For more information see: https://www.rabbitmq.com/dlx.html
                _channel.BasicAck(@event.DeliveryTag, multiple: false);
            }
        }
    }
}