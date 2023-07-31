using System.Collections.Generic;

namespace Dncy.EventBus.SubscribeActivator
{
    public class IntegrationEventContext
    {
        /// <summary>
        /// 原始信息
        /// </summary>
        public string Body { get; set; }

        public IDictionary<string, object> Properties { get; set; }
    }
}

