﻿using Dncy.EventBus.Abstract.Interfaces;
using Dncy.EventBus.Abstract.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dncy.EventBus.Abstract
{
    /// <summary>
    /// 内存事件管理器
    /// </summary>
    public class InMemoryEventBusSubscriptionsManager : IEventBusSubscriptionsManager
    {

        private readonly Dictionary<string, List<SubscriptionInfo>> _handlers;
        private readonly List<Type> _eventTypes;
        public event Action<string, SubscriptionInfo> OnEventRemoved;



        public InMemoryEventBusSubscriptionsManager()
        {
            _handlers = new Dictionary<string, List<SubscriptionInfo>>();
            _eventTypes = new List<Type>();
        }


        public bool IsEmpty => _handlers is { Count: 0 };

        public void Clear(string eventBusName = null)
        {
            if (eventBusName == null)
            {
                _handlers.Clear();
            }
            else
            {
                var targetHandles = _handlers.Where(x => x.Key.StartsWith(eventBusName)).Select(x => x.Key);
                foreach (var key in targetHandles)
                {
                    _handlers.Remove(key);
                }
            }
        }




        /// <inheritdoc />
        public void AddSubscription<T, TH>(string eventBusName = null)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = $"{eventBusName}_{GetEventKey<T>()}";

            DoAddSubscription(typeof(TH), eventName, isDynamic: false);

            if (!_eventTypes.Contains(typeof(T)))
            {
                _eventTypes.Add(typeof(T));
            }
        }


        /// <inheritdoc />
        public void AddDynamicSubscription<TH>(string eventName, string eventBusName = null)
            where TH : IDynamicIntegrationEventHandler
        {
            DoAddSubscription(typeof(TH), $"{eventBusName}_{eventName}", isDynamic: true);
        }

        /// <inheritdoc />
        public void RemoveSubscription<T, TH>(string eventBusName = null)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = $"{eventBusName}_{GetEventKey<T>()}";
            var handlerToRemove = FindSubscriptionToRemove<T, TH>(eventName);
            DoRemoveHandler(eventName, handlerToRemove);
        }

        /// <inheritdoc />
        public void RemoveDynamicSubscription<TH>(string eventName, string eventBusName = null)
            where TH : IDynamicIntegrationEventHandler
        {
            eventName = $"{eventBusName}_{eventName}";
            var handlerToRemove = FindDynamicSubscriptionToRemove<TH>(eventName);
            DoRemoveHandler(eventName, handlerToRemove);
        }

        /// <inheritdoc />
        public bool HasSubscriptionsForEvent<T>(string eventBusName = null)
            where T : IntegrationEvent
        {
            var key = $"{eventBusName}_{GetEventKey<T>()}";
            return HasSubscriptionsForEvent(key);
        }

        /// <inheritdoc />
        public bool HasSubscriptionsForEvent(string eventName, string eventBusName = null) => _handlers.ContainsKey($"{eventBusName}_{eventName}");

        /// <inheritdoc />
        public Type GetEventTypeByName(string eventName) => _eventTypes.SingleOrDefault(t => t.Name == eventName);




        /// <inheritdoc />
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>(string eventBusName = null)
            where T : IntegrationEvent
        {
            var key = $"{eventBusName}_{GetEventKey<T>()}";
            return GetHandlersForEvent(key);
        }

        /// <inheritdoc />
        public IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName, string eventBusName = null) => _handlers[$"{eventBusName}_{eventName}"];


        /// <inheritdoc />
        public IEnumerable<SubscriptionInfo> TryGetHandlersForEvent(string eventName, string eventBusName = null) => _handlers.ContainsKey($"{eventBusName}_{eventName}") ? _handlers[$"{eventBusName}_{eventName}"] : null;

        /// <inheritdoc />
        public string GetEventKey<T>()
        {
            return typeof(T).Name;
        }



        private void DoAddSubscription(Type handlerType, string eventName, bool isDynamic)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                _handlers.Add(eventName, new List<SubscriptionInfo>());
            }

            if (_handlers[eventName].Any(s => s.HandlerType == handlerType))
            {
                throw new ArgumentException($"Handler Type {handlerType.Name} already registered for '{eventName}'", nameof(handlerType));
            }

            if (isDynamic)
            {
                _handlers[eventName].Add(SubscriptionInfo.Dynamic(handlerType));
            }
            else
            {
                _handlers[eventName].Add(SubscriptionInfo.Typed(handlerType));
            }

        }


        private void DoRemoveHandler(string eventName, SubscriptionInfo subsToRemove)
        {
            if (subsToRemove != null)
            {
                _handlers[eventName].Remove(subsToRemove);
                if (!_handlers[eventName].Any())
                {
                    _handlers.Remove(eventName);
                    var eventType = _eventTypes.SingleOrDefault(e => e.Name == eventName);
                    if (eventType != null)
                    {
                        _eventTypes.Remove(eventType);
                    }
                    RaiseOnEventRemoved(eventName, subsToRemove);
                }

            }
        }

        private void RaiseOnEventRemoved(string eventName, SubscriptionInfo subsRemoved)
        {
            var handler = OnEventRemoved;
            handler?.Invoke(eventName, subsRemoved);
        }

        private SubscriptionInfo FindDynamicSubscriptionToRemove<TH>(string eventName)
            where TH : IDynamicIntegrationEventHandler
        {
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }


        private SubscriptionInfo DoFindSubscriptionToRemove(string eventName, Type handlerType)
        {
            if (!HasSubscriptionsForEvent(eventName))
            {
                return null;
            }

            return _handlers[eventName].SingleOrDefault(s => s.HandlerType == handlerType);

        }


        private SubscriptionInfo FindSubscriptionToRemove<T, TH>(string eventName)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            return DoFindSubscriptionToRemove(eventName, typeof(TH));
        }
    }
}