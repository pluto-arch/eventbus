using Aliyun.MQ;
using Aliyun.MQ.Model;
using Aliyun.MQ.Model.Exp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Pluto.EventBus.Abstract;
using Pluto.EventBus.Abstract.Interfaces;

namespace Pluto.EventBus.AliyunRocketMQ
{
    public class EventBusRocketMQ : IEventBus, IDisposable
    {

        private Lazy<MQProducer> _producer;
        private Lazy<MQClient> _mQClient;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IServiceScopeFactory _service;
        private bool disposedValue;
        private readonly ILogger<EventBusRocketMQ> _logger;
        private readonly IMessageSerializeProvider _messageSerializeProvider;
        private readonly IIntegrationEventStore _eventStore;
        private readonly AliyunRocketMqOption _mqOption;

        #region mq parame
        private bool isConsumerTaskRunning = false;
        private readonly object _consumerTasklockObj = new object();
        private CancellationTokenSource cancellationTokenSource;
        #endregion


        public EventBusRocketMQ(
            IServiceScopeFactory serviceFactory,
            AliyunRocketMqOption option,
            IMessageSerializeProvider messageSerializeProvider,
            IIntegrationEventStore eventStore=null,
            ILogger<EventBusRocketMQ> logger = null,
            IEventBusSubscriptionsManager subsManager=null)
        {
            _messageSerializeProvider = messageSerializeProvider??throw new InvalidOperationException("no message serializer found");
            _subsManager = subsManager??new InMemoryEventBusSubscriptionsManager();
            _service = serviceFactory;
            _mqOption = option?? throw new ArgumentNullException(nameof(option));
            _logger = logger ?? NullLogger<EventBusRocketMQ>.Instance;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved; ;
            _eventStore = eventStore??NullIntegrationEventStore.Instance;
            Init();
        }



        /// <inheritdoc />
        public virtual string Name => nameof(EventBusRocketMQ);


        private void Init()
        {
            _mQClient = new Lazy<MQClient>(() =>
            {
                var accessInfo = _mqOption.AuthenticationConfiguration;
                _logger.LogInformation("initialize rocketmq client...");
                if (!string.IsNullOrEmpty(accessInfo.StsToken))
                {
                    return new MQClient(accessInfo.AccessId, accessInfo.AccessKey, _mqOption.HttpEndPoint, accessInfo.StsToken);
                }
                return new MQClient(accessInfo.AccessId, accessInfo.AccessKey, _mqOption.HttpEndPoint);
            });

            _producer = new Lazy<MQProducer>(() =>
            {
                _logger.LogInformation("initialize rocketmq producer...");
                return _mQClient.Value.GetProducer(_mqOption.InstranceId, _mqOption.Topic);
            });

        }



        private void SubsManager_OnEventRemoved(string eventName, SubscriptionInfo subRemoved)
        {
            if (_subsManager.IsEmpty)
            {
                if (cancellationTokenSource!=null && cancellationTokenSource.Token.CanBeCanceled)
                {
                    cancellationTokenSource.Cancel();
                }
            }
        }


        /// <inheritdoc />
        public void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;
            var p = _producer.Value;
            var topicMsg = new TopicMessage(_messageSerializeProvider.Serialize(@event), eventName);
            if (@event.StartDeliverTime > 0)
            {
                topicMsg.StartDeliverTime = @event.StartDeliverTime;
            }
            p.PublishMessage(topicMsg);
        }



        /// <inheritdoc />
        public async Task PublishAsync(IntegrationEvent @event)
        {
            this.Publish(@event);
            await Task.CompletedTask;
        }



       

        /// <inheritdoc />
        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _subsManager.AddSubscription<T, TH>(this.Name?? nameof(EventBusRocketMQ));
            lock (_consumerTasklockObj)
            {
                if (!isConsumerTaskRunning)
                {
                    isConsumerTaskRunning = true;
                    cancellationTokenSource = new CancellationTokenSource();
                    cancellationTokenSource.Token.Register(() =>
                    {
                        isConsumerTaskRunning = false;
                        _logger.LogInformation($"消费者task被取消");
                        cancellationTokenSource.Dispose();
                    });
                    StartBasicConsume(cancellationTokenSource);
                }
            }
        }

        /// <inheritdoc />
        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            _subsManager.RemoveSubscription<T, TH>(this.Name?? nameof(EventBusRocketMQ));
        }

        /// <inheritdoc />
        public void SubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            throw new NotImplementedException("暂未支持");
        }

        /// <inheritdoc />
        public void UnsubscribeDynamic<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            throw new NotImplementedException("暂未支持");
        }





        private Task StartBasicConsume(CancellationTokenSource tokenSource)
        {
            return Task.Factory.StartNew(async () =>
            {
                var consumer = _mQClient.Value.GetConsumer(_mqOption.InstranceId, _mqOption.Topic, _mqOption.GroupId, string.Empty);
                if (consumer == null)
                {
                    _logger.LogInformation($"初始化消费者失败");
                }
                _logger.LogInformation($"初始化消费者成功: topic:{_mqOption.Topic}  groupid:{_mqOption.GroupId}");
                while (true)
                {
                    if (tokenSource.IsCancellationRequested)
                    {
                        tokenSource.Cancel();
                        tokenSource.Token.ThrowIfCancellationRequested();
                    }

                    try
                    {
                        var messages = consumer.ConsumeMessage(_mqOption.BitchSize, _mqOption.WaitSecond);
                        if (messages == null || !messages.Any())
                        {
                            continue;
                        }

                        using (var scope=_service.CreateScope())
                        {
                            foreach (var message in messages)
                            {
                                var handlersForEvent = _subsManager.TryGetHandlersForEvent(message.MessageTag);
                                if (handlersForEvent == null || !handlersForEvent.Any())
                                {
                                    _logger.LogWarning($"{message.MessageTag}没有配置任何处理程序");
                                    continue;
                                }
                                _logger.LogInformation($"消息：{message.MessageTag}, 订阅者数量：{handlersForEvent.Count()}");
                                _logger.LogDebug("消息内容 ：{@message}",message);
                                await TryStoredEvent(message.MessageTag,message.Body);
                                consumer.AckMessage(new List<string>(){ message.ReceiptHandle });
                                foreach (var subscriptionInfo in handlersForEvent)
                                {
                                    if (subscriptionInfo.IsDynamic)
                                    {
                                        var handle = scope.ServiceProvider.GetRequiredService(subscriptionInfo.HandlerType) as IDynamicIntegrationEventHandler;
                                        if (handle == null) continue;
                                        var obj2 = _messageSerializeProvider.Deserialize<dynamic>(message.Body);
                                        await handle.Handle(obj2);
                                    }
                                    else
                                    {
                                        var handle = scope.ServiceProvider.GetRequiredService(subscriptionInfo.HandlerType);
                                        if (handle == null) continue;
                                        var eventType = _subsManager.GetEventTypeByName(message.MessageTag);
                                        var type = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                                        var obj2 = _messageSerializeProvider.Deserialize(message.Body, eventType);
                                        await (Task)type.GetMethod("Handle").Invoke(handle, new object[1] { obj2 });
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        if (!(e is MessageNotExistException))
                        {
                            _logger.LogError(e.Message);
                        }
                    }
                }
            }, tokenSource.Token);
        }

        private async Task TryStoredEvent(string messageTag, string messageBody)
        {
            try
            {
                await _eventStore.SaveAsync(messageTag,messageBody,this.Name??nameof(EventBusRocketMQ));
            }
            catch (Exception e)
            {
                _logger.LogError(e,$"storage integration event has an error：{e.Message}");
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
                        _logger.LogInformation("initialize rocketmq client...");
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
