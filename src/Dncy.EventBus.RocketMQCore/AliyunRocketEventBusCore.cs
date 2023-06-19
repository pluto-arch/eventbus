using Aliyun.MQ;
using Aliyun.MQ.Model;
using Aliyun.MQ.Model.Exp;
using Dncy.EventBus.Abstract;
using Dncy.EventBus.Abstract.Interfaces;
using Dncy.EventBus.Abstract.Models;
using Dncy.EventBus.AliyunRocketMQCore.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dncy.EventBus.SubscribeActivator;

namespace Dncy.EventBus.AliyunRocketMQCore
{
    public class AliyunRocketEventBusCore : IDisposable
    {

        private readonly Lazy<MQProducer> _producer;
        private Lazy<MQClient> _mQClient;

        private bool disposedValue;
        private readonly ILogger<AliyunRocketEventBusCore> _logger;
        private readonly IIntegrationEventStore _eventStore;
        private readonly AliyunRocketMqOption _mqOption;
        private readonly IntegrationEventHandlerActivator _ieha;

        static JsonSerializerOptions options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public AliyunRocketEventBusCore(
            AliyunRocketMqOption option,
            IntegrationEventHandlerActivator messageHandlerActivator,
            IIntegrationEventStore eventStore = null,
            ILogger<AliyunRocketEventBusCore> logger = null)
        {
            _mqOption = option ?? throw new ArgumentNullException(nameof(option));
            _ieha = messageHandlerActivator;
            _logger = logger ?? NullLogger<AliyunRocketEventBusCore>.Instance;
            _eventStore = eventStore ?? NullIntegrationEventStore.Instance;


            _mQClient = new Lazy<MQClient>(() =>
            {
                var accessInfo = _mqOption.AuthenticationConfiguration;
                _logger.DebugMessage("initialize rocketmq client...");
                if (!string.IsNullOrEmpty(accessInfo.StsToken))
                {
                    return new MQClient(accessInfo.AccessId, accessInfo.AccessKey, _mqOption.HttpEndPoint, accessInfo.StsToken);
                }
                return new MQClient(accessInfo.AccessId, accessInfo.AccessKey, _mqOption.HttpEndPoint);
            });

            _producer = new Lazy<MQProducer>(() =>
            {
                _logger.DebugMessage("initialize rocketmq producer...");
                return _mQClient.Value.GetProducer(_mqOption.InstranceId, _mqOption.Topic);
            });
        }


        public virtual string Name => nameof(AliyunRocketEventBusCore);

        public void Publish(IntegrationEvent @event)
        {
            if (@event == null)
            {
                _logger.WarningMessage("event is null");
                return;
            }
            @event.RouteKey ??= @event.GetType().Name;
            @event.RouteKey = @event.RouteKey.StartsWith("/") ? @event.RouteKey : $"/{@event.RouteKey}";
            var p = _producer.Value;
            var messageBody = JsonSerializer.Serialize<object>(@event, options);
            var topicMsg = new TopicMessage(messageBody, @event.RouteKey)
            {
                Id = @event.Id
            };
            topicMsg.PutProperty("KEYS", @event.Id);
            if (@event.StartDeliverTime > 0)
            {
                topicMsg.StartDeliverTime = @event.StartDeliverTime;
            }
            p.PublishMessage(topicMsg);
        }

        public async Task PublishAsync(IntegrationEvent @event)
        {
            Publish(@event);
            await Task.CompletedTask;
        }

        public Task StartBasicConsume(CancellationToken tokenSource = default)
        {
            return Task.Factory.StartNew(async () =>
            {
                var consumer = _mQClient.Value.GetConsumer(_mqOption.InstranceId, _mqOption.Topic, _mqOption.GroupId, string.Empty);
                if (consumer == null)
                {
                    _logger.WarningMessage("get consumer failed");
                    return;
                }
                _logger.ConsumerInitialized(_mqOption.Topic, _mqOption.GroupId);
                while (true)
                {
                    if (tokenSource is { IsCancellationRequested: true })
                    {
                        tokenSource.ThrowIfCancellationRequested();
                    }

                    try
                    {
                        var messages = consumer?.ConsumeMessage(_mqOption.BitchSize, _mqOption.WaitSecond);
                        if (messages == null || !messages.Any())
                        {
                            continue;
                        }

                        foreach (var message in messages)
                        {
                            _logger.MessageConsumed(message.MessageTag, message.Body);
                            consumer.AckMessage(new List<string>() { message.ReceiptHandle });
                            await TryStoredEvent(message.MessageTag, message.Body);
                            await _ieha.ProcessRequestAsync(message.MessageTag, message.Body);
                        }
                    }
                    catch (Exception e)
                    {
                        if (!( e is MessageNotExistException ))
                        {
                            _logger.LogError(e, "consumer message has an error :{message}", e.Message);
                        }
                    }
                }
            }, tokenSource);
        }

        public List<SubscribeDescriptorItemModel> Subscribes => _ieha.SubscribeList();


        public void DisableSubscribe(params string[] ids)
        {
            foreach (var item in ids)
            {
                _ieha.DisableEventHandler(item);
            }
        }


        public void EnableSubscribe(params string[] ids)
        {
            foreach (var item in ids)
            {
                _ieha.EnableEventHandler(item);
            }
        }


        private async Task TryStoredEvent(string messageTag, string messageBody)
        {
            try
            {
                await _eventStore.SaveAsync(messageTag, messageBody, Name ?? nameof(AliyunRocketEventBusCore));
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"storage integration event has an error：{e.Message}");
            }
        }


        #region disable

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    _mQClient.Value?.Dispose();
                    _mQClient = new Lazy<MQClient>(() =>
                    {
                        var accessInfo = _mqOption.AuthenticationConfiguration;
                        _logger.DebugMessage("initialize rocketmq client...");
                        if (!string.IsNullOrEmpty(accessInfo.StsToken))
                        {
                            return new MQClient(accessInfo.AccessId, accessInfo.AccessKey, _mqOption.HttpEndPoint, accessInfo.StsToken);
                        }
                        return new MQClient(accessInfo.AccessId, accessInfo.AccessKey, _mqOption.HttpEndPoint);
                    });
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~EventBusRocketMQ()
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

        #endregion
    }
}