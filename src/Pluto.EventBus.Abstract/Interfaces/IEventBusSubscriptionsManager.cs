﻿using Dncy.EventBus.Abstract.Models;
using System;
using System.Collections.Generic;

namespace Dncy.EventBus.Abstract.Interfaces
{
    /// <summary>
    /// 事件订阅管理器
    /// </summary>
    public interface IEventBusSubscriptionsManager
    {
        /// <summary>
        /// 是否为空
        /// </summary>
        bool IsEmpty { get; }


        /// <summary>
        /// 事件移除时事件
        /// </summary>
        event Action<string, SubscriptionInfo> OnEventRemoved;



        /// <summary>
        /// 添加订阅
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <typeparam name="TH">事件处理程序</typeparam>
        void AddSubscription<T, TH>(string eventBusName = null)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;



        /// <summary>
        /// 添加动态事件订阅者
        /// </summary>
        /// <typeparam name="TH"></typeparam>
        /// <param name="eventName"></param>
        void AddDynamicSubscription<TH>(string eventName, string eventBusName = null)
            where TH : IDynamicIntegrationEventHandler;




        /// <summary>
        /// 移除订阅
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <typeparam name="TH">事件处理程序</typeparam>
        void RemoveSubscription<T, TH>(string eventBusName = null)
            where TH : IIntegrationEventHandler<T>
            where T : IntegrationEvent;


        /// <summary>
        /// 移除动态事件订阅者
        /// </summary>
        /// <typeparam name="TH"></typeparam>
        /// <param name="eventName"></param>
        void RemoveDynamicSubscription<TH>(string eventName, string eventBusName = null)
            where TH : IDynamicIntegrationEventHandler;



        /// <summary>
        /// 事件T是否有订阅
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <returns></returns>
        bool HasSubscriptionsForEvent<T>(string eventBusName = null) where T : IntegrationEvent;



        /// <summary>
        /// 事件eventName是否有订阅
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <returns></returns>
        bool HasSubscriptionsForEvent(string eventName, string eventBusName = null);




        /// <summary>
        /// 根据名称获取事件类型
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <returns></returns>
        Type GetEventTypeByName(string eventName);


        /// <summary>
        /// 获取事件的处理程序
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <returns></returns>
        IEnumerable<SubscriptionInfo> TryGetHandlersForEvent(string messageMessageTag, string eventBusName = null);


        /// <summary>
        /// 清除订阅
        /// </summary>
        void Clear(string eventBusName = null);



        /// <summary>
        /// 获取事件的处理程序
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <returns></returns>
        IEnumerable<SubscriptionInfo> GetHandlersForEvent<T>(string eventBusName = null) where T : IntegrationEvent;


        /// <summary>
        /// 获取事件的处理程序
        /// </summary>
        /// <param name="eventName">事件名称</param>
        /// <returns></returns>
        IEnumerable<SubscriptionInfo> GetHandlersForEvent(string eventName, string eventBusName = null);

        /// <summary>
        /// 获取事件的key
        /// </summary>
        /// <typeparam name="T">事件类型</typeparam>
        /// <returns></returns>
        string GetEventKey<T>();

    }
}