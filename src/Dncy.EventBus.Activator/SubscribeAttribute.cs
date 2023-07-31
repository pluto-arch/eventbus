using System;

namespace Dncy.EventBus.SubscribeActivator
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SubscribeAttribute : Attribute
    {
        /// <summary>
        /// 路由模板
        /// </summary>
        public string RouteTemplate { get; set; }

        /// <summary>
        /// 顺序
        /// </summary>
        public int Order { get; set; }


        private const string prefix = "/";
        
        /// <summary>
        /// 订阅的总线名称
        /// </summary>
        public string SubscribeEventBusName { get; set; }
        

        public SubscribeAttribute(string routeTemplate,string subscribeEventBusName)
        {
            SubscribeEventBusName = subscribeEventBusName;
            if (routeTemplate.StartsWith(prefix))
            {
                RouteTemplate = routeTemplate;
            }
            else
            {
                RouteTemplate = $"/{routeTemplate}";
            }
        }
    }
}