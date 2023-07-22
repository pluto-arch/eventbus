using Dncy.EventBus.Abstract;
using Dncy.EventBus.Abstract.Interfaces;
using Dncy.EventBus.Abstract.Models;
using Dncy.EventBus.RabbitMQ.Connection;
using Dncy.EventBus.RabbitMQ.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Dncy.EventBus.SubscribeActivator;
using System.Data.Common;

namespace Dncy.EventBus.RabbitMQ
{
    public class EventBusRabbitMQ : IDisposable
    {
        private readonly IRabbitMQConnection _connection;

        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IIntegrationEventStore _eventStore;
        private readonly IntegrationEventHandlerActivator _ieha;


        static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private readonly RabbitMQDeclaration _queueDeclare;

        private IModel _channel;
        private IModel _publishChannel;
        private bool disposedValue;

        public EventBusRabbitMQ(
            IRabbitMQConnection connection,
            RabbitMQDeclaration queueDeclare,
            IntegrationEventHandlerActivator ieha,
            ILogger<EventBusRabbitMQ> logger = null,
            IIntegrationEventStore eventStore = null)
        {
            _connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _logger = logger ?? NullLogger<EventBusRabbitMQ>.Instance;
            _queueDeclare = queueDeclare;
            _eventStore = eventStore ?? NullIntegrationEventStore.Instance;
            _ieha = ieha;
        }


        public virtual string Name => nameof(EventBusRabbitMQ);

        public void Publish(IntegrationEvent @event)
        {
            if (!_connection.IsConnected)
                _connection.TryConnect();
            @event.RouteKey ??= @event.GetType().Name;
            @event.RouteKey = @event.RouteKey.StartsWith("/") ? @event.RouteKey : $"/{@event.RouteKey}";

            var properties = _channel.CreateBasicProperties();
            properties.DeliveryMode = 2;
            if (@event.StartDeliverTime > 0)
            {
                properties.Expiration = @event.StartDeliverTime.ToString();
            }
            _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

            _publishChannel ??= CreatePublishChannel();

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<object>(@event, options));
            _publishChannel.BasicPublish(
                exchange: _queueDeclare.ExchangeName,
                routingKey: @event.RouteKey,
                mandatory: true,
                basicProperties: properties,
                body: body);
        }

        public Task PublishAsync(IntegrationEvent @event)
        {
            Publish(@event);
            return Task.CompletedTask;
        }

        public void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume ...");
            _channel ??= CreateChannel();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;
            DoInternalSubscription();
            _channel.BasicConsume(
                queue: _queueDeclare.QueueName,
                autoAck: false,
                consumer: consumer);
        }



        public List<SubscribeDescriptorItemModel> Subscribes => _ieha.SubscribeList();


        public void DisableSubscribe(params string[] ids)
        {
            foreach (var item in ids)
            {
                _ieha.DisableEventHandler(item);
                var i = Subscribes.FirstOrDefault(x => x.Id == item);
                _channel.QueueUnbind(queue: _queueDeclare.QueueName,
                    exchange: _queueDeclare.ExchangeName,
                    routingKey: i.RouteTemplate);
            }
        }


        public void EnableSubscribe(params string[] ids)
        {
            foreach (var item in ids)
            {
                _ieha.EnableEventHandler(item);
                var i = Subscribes.FirstOrDefault(x => x.Id == item);
                _channel.QueueBind(queue: _queueDeclare.QueueName,
                    exchange: _queueDeclare.ExchangeName,
                    routingKey: i.RouteTemplate);
            }
        }


        private void DoInternalSubscription()
        {
            foreach (var item in Subscribes)
            {
                _channel.QueueBind(queue: _queueDeclare.QueueName,
                    exchange: _queueDeclare.ExchangeName,
                    routingKey: item.RouteTemplate);
            }
        }


        private IModel CreateChannel()
        {
            if (!_connection.IsConnected)
                _connection.TryConnect();
            var channel = _connection.CreateModel();
            channel.ExchangeDeclare(
                exchange: _queueDeclare.ExchangeName,
                type: _queueDeclare.ConfigExchangeType);

            channel.QueueDeclare(queue: _queueDeclare.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogInformation(ea.Exception, "channel has an exception details: {detail}", ea.Detail);
                _channel?.Dispose();
                _channel = CreateChannel();
                StartBasicConsume();
            };


            return channel;
        }


        private IModel CreatePublishChannel()
        {
            if (!_connection.IsConnected)
                _connection.TryConnect();
            var channel = _connection.CreateModel();
            channel.ExchangeDeclare(
                exchange: _queueDeclare.ExchangeName,
                type: _queueDeclare.ConfigExchangeType);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogInformation(ea.Exception, "publish channel has an exception details: {detail}", ea.Detail);
                _publishChannel?.Dispose();
                _publishChannel = CreateChannel();
            };

            return channel;
        }



        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var message = Encoding.UTF8.GetString(@event.Body.Span);
            try
            {
                _logger.LogDebug("receiver message from mq：{eventName}. message：{message}", eventName, message);
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }
                await TryStoredEvent(eventName, message);
                await _ieha.ProcessRequestAsync(eventName, message);
            }
            finally
            {
                _channel.BasicAck(@event.DeliveryTag, multiple: false);
            }
        }


        private async Task TryStoredEvent(string messageTag, string messageBody)
        {
            try
            {
                await _eventStore.SaveAsync(messageTag, messageBody, Name ?? nameof(EventBusRabbitMQ));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"storage integration event has an error：{e.Message}");
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    _channel?.Dispose();
                    _publishChannel?.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~EventBusRabbitMQ()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}