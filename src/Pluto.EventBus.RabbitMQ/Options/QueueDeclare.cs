using System.Collections.Generic;
using RabbitMQ.Client;

namespace Dncy.EventBus.RabbitMQ.Options
{
    public class RabbitMQDeclaration
    {
        /// <summary>
        /// 交换机名称
        /// </summary>
        public string ExchangeName { get; set; }

        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 交换机类型
        /// <see cref="ExchangeType"/>
        /// </summary>
        public string ConfigExchangeType { get; set; } = ExchangeType.Direct;

        /// <summary>
        /// 队列参数
        /// </summary>
        public Dictionary<string, object> QueueArguments { get; set; }
    }
}

