using System;
using RabbitMQ.Client;

namespace Pluto.EventBus.RabbitMQ.Connection
{
    public interface IRabbitMQConnection:IDisposable,IAsyncDisposable
    {
        bool IsConnected { get; }

        bool TryConnect();

        IModel CreateModel();
    }
}