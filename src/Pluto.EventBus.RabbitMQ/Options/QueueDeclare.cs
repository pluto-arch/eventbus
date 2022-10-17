using RabbitMQ.Client;

namespace Pluto.EventBus.RabbitMQ.Options
{
    public class QueueDeclare
    {
        /// <summary>
        /// 队列名称
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 交换机类型
        /// <see cref="ExchangeType"/>
        /// </summary>
        public string ExchangeType { get; set; }
    }
}

