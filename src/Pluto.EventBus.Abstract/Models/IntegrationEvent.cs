using System;

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
            CreationDate = DateTime.UtcNow;
            StartDeliverTime = 0;
        }


        public IntegrationEvent(string id, DateTime createDate)
        {
            Id = id;
            CreationDate = createDate;
            StartDeliverTime = 0;
        }

        public string Id { get; set; }

        public DateTime CreationDate { get; set; }

        /// <summary>
        /// 延迟时间
        /// </summary>
        public long StartDeliverTime { get; set; }

        /// <summary>
        /// 消息路由key
        /// 默认为事件名称
        /// EventBus.AliyunRocketMQCore支持自定义
        /// </summary>
        public string RouteKey { get; set; }
    }
}