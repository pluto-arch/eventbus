﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Threading.Tasks;

namespace Dncy.EventBus.RabbitMQ.Connection
{
    public class DefaultRabbitMQConnection : IRabbitMQConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<DefaultRabbitMQConnection> _logger;
        public IConnection _connection;

        bool _disposed;

        public DefaultRabbitMQConnection(IConnectionFactory connectionFactory, ILogger<DefaultRabbitMQConnection> logger = null)
        {
            _connectionFactory = connectionFactory;
            _logger = logger ?? NullLogger<DefaultRabbitMQConnection>.Instance;
        }


        /// <inheritdoc />
        public bool IsConnected => _connection is { IsOpen: true } && !_disposed;

        /// <inheritdoc />
        public bool TryConnect()
        {
            try
            {
                _connection = _connectionFactory.CreateConnection();
                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;
                    _logger.LogInformation("RabbitMQ Client acquired a persistent connection to '{HostName}' and is subscribed to failure events", _connection.Endpoint.HostName);
                    return true;
                }
                _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
                return false;
            }
            catch (Exception e)
            {
                _logger.LogError("FATAL ERROR: RabbitMQ connections could not be connected :{message}", e.Message);
                return false;
            }
        }

        /// <inheritdoc />
        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return _connection.CreateModel();
        }

#if NET5_0_OR_GREATER
        public ValueTask DisposeAsync()
        {
            Dispose(true);
            return ValueTask.CompletedTask;
        }
#endif



        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    _connection.ConnectionShutdown -= OnConnectionShutdown;
                    _connection.CallbackException -= OnCallbackException;
                    _connection.ConnectionBlocked -= OnConnectionBlocked;
                    _connection.Dispose();
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                _disposed = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~DefaultRabbitMQConnection()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }



        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");
            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");
            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;
            _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");
            TryConnect();
        }
    }
}

