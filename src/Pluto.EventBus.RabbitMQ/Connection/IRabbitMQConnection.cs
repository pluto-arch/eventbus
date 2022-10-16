using System;
using RabbitMQ.Client;

namespace Pluto.EventBus.RabbitMQ.Connection
{
#if NET5_0_OR_GREATER
    public interface IRabbitMQConnection : IDisposable, IAsyncDisposable
#else
    public interface IRabbitMQConnection:IDisposable
#endif
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}