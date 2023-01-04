using System;

namespace Dncy.EventBus.Abstract.EventActivator
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

        public SubscribeAttribute(string routeTemplate)
        {
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