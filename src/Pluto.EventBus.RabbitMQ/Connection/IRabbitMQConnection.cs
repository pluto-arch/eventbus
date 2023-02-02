using RabbitMQ.Client;
using System;

namespace Dncy.EventBus.RabbitMQ.Connection
{
#if NET5_0_OR_GREATER
    public interface IRabbitMQConnection : IDisposable, IAsyncDisposable
#else
    public interface IRabbitMQConnection : IDisposable
#endif
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}