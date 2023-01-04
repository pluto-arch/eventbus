using RabbitMQ.Client;

namespace Pluto.EventBusRabbitMQ.Options
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
        public string ExchangeType { get; set; } = RabbitMQ.Client.ExchangeType.Direct;
    }
}

