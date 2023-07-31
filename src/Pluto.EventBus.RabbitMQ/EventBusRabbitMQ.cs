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
            WriteIndented = false
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
            _publishChannel = CreatePublishChannel();
            _channel = CreateChannel();
        }


        public virtual string Name => nameof(EventBusRabbitMQ);

        public virtual void Publish(IntegrationEvent @event)
        {
            if (!_connection.IsConnected)
                _connection.TryConnect();
            @event.RouteKey ??= @event.GetType().Name;
            @event.RouteKey = @event.RouteKey.StartsWith("/") ? @event.RouteKey : $"/{@event.RouteKey}";

            var properties = _channel.CreateBasicProperties();
            properties.MessageId = @event.Id;
            properties.Persistent = @event.Persistent;
            properties.Timestamp = new AmqpTimestamp(@event.CreationDate.ToUnixTimeMilliseconds());;
            if (@event.Expiration > 0)
            {
                properties.Expiration = @event.Expiration.ToString();
            }
            if (!string.IsNullOrEmpty(@event.Type))
            {
                properties.Type = @event.Type;
            }

            if (@event.StartDeliverTime>0)
            {
                _logger.LogWarning("rabbitmq does not support StartDeliverTime,place use Expiration.");
            }
            
            if (@event.Properties!=null)
            {
                foreach (var item in @event.Properties)
                {
                    properties.Headers.Add(item.Key,item.Value);
                }
            }
            
            _logger.LogDebug("Publishing Event {EventId} to RabbitMQ with [Exchange={change},Queue={queue}]", @event.Id,_queueDeclare.ExchangeName,_queueDeclare.QueueName);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize<object>(@event, options));
            _publishChannel.BasicPublish(
                exchange: _queueDeclare.ExchangeName,
                routingKey: @event.RouteKey,
                mandatory: true,
                basicProperties: properties,
                body: body);
        }

        public virtual Task PublishAsync(IntegrationEvent @event)
        {
            Publish(@event);
            return Task.CompletedTask;
        }

        public virtual void StartBasicConsume()
        {
            _logger.LogDebug($"Starting RabbitMQ basic consume on {_queueDeclare.QueueName} ...");
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += Consumer_Received;
            DoInternalSubscription();
            _channel.BasicConsume(
                queue: _queueDeclare.QueueName,
                autoAck: false,
                consumer: consumer);
        }

        /// <summary>
        /// 只进行绑定，不进行消费者创建 - 延迟队列使用
        /// </summary>
        public virtual void StartDelayQueue()
        {
            DoInternalSubscription();
        }
        


        public List<SubscribeDescriptorItemModel> Subscribes => _ieha.SubscribeList();


        public virtual void DisableSubscribe(params string[] ids)
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


        public virtual void EnableSubscribe(params string[] ids)
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


        /// <summary>
        /// 创建消费者channel并绑定队列到交换机
        /// </summary>
        /// <returns></returns>
        private IModel CreateChannel()
        {
            if (!_connection.IsConnected)
                _connection.TryConnect();
            var channel = _connection.CreateModel();
            
            channel.QueueDeclare(queue: _queueDeclare.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments:_queueDeclare.QueueArguments);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogError(ea.Exception, "channel has an exception details: {@detail}", ea.Detail);
                _channel?.Dispose();
                _channel = CreateChannel();
                StartBasicConsume();
            };
            return channel;
        }


        /// <summary>
        /// 创建生产者channel
        /// </summary>
        /// <returns></returns>
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
                _logger.LogError(ea.Exception, "publish channel has an exception details: {@detail}", ea.Detail);
                _publishChannel?.Dispose();
                _publishChannel = CreatePublishChannel();
            };

            return channel;
        }



        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            var eventName = @event.RoutingKey;
            var message = Encoding.UTF8.GetString(@event.Body.Span);
            try
            {
                _logger.LogDebug("RabbitMQ Message [Exchange={exchange},Queue={queue},RouteKey={eventName}] Message: {@message}", _queueDeclare.ExchangeName,_queueDeclare.QueueName, eventName, message);
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }
                await TryStoredEvent(eventName, message);
                await _ieha.ProcessRequestAsync(eventName, message,@event.BasicProperties.Headers,Name);
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