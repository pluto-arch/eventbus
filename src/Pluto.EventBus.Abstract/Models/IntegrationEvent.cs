using System;
using System.Collections.Generic;

namespace Dncy.EventBus.Abstract.Models
{
    /// <summary>
    /// 集成事件
    /// </summary>
    public class IntegrationEvent
    {
        public IntegrationEvent()
        {
            Id = $"{Guid.NewGuid():N}";
            CreationDate = DateTimeOffset.Now;
        }


        public IntegrationEvent(string id, DateTimeOffset createDate)
        {
            Id = id;
            CreationDate = createDate;
        }

        public string Id { get; set; }

        /// <summary>
        /// 消息生成时间
        /// </summary>
        public DateTimeOffset CreationDate { get; set; }

        /// <summary>
        /// 消息过期时间
        /// </summary>
        public long Expiration { get; set; }

        /// <summary>
        /// 是否持久化 - 默认是
        /// </summary>
        public bool Persistent { get; set; } = true;

        /// <summary>
        /// 消息类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 消息路由key
        /// 默认为事件名称
        /// EventBus.AliyunRocketMQCore支持自定义
        /// </summary>
        public string RouteKey { get; set; }

        /// <summary>
        /// 开始投递时间
        /// </summary>
        public long StartDeliverTime { get; set; }
        
        /// <summary>
        /// 其他属性
        /// </summary>
        public IDictionary<string,string> Properties { get; set; }
    }
}