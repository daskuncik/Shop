using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;
using Polly;
using Polly.Retry;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;
using Statistics.EventBus;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
namespace Statistics.RabbitHelper
{
    public class RabbitPersistentConnection : IRabbitPersistentConnection
    {
        private readonly IConnectionFactory connectionFactory;
        private readonly ILogger<RabbitEventBus> logger;
        IConnection connection;
        bool disposed;
        int retryCount = 3;

        object sync_root = new object();

        public RabbitPersistentConnection(IConfiguration configuration, ILogger<RabbitEventBus> logger)
        {
            connectionFactory = new ConnectionFactory { Uri = new Uri(configuration.GetConnectionString("MQ")) };
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool IsConnected
        {
            get
            {
                return connection != null && connection.IsOpen && !disposed;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return connection.CreateModel();
        }

        public void Dispose()
        {
            if (disposed) return;

            disposed = true;

            try
            {
                connection.Dispose();
            }
            catch (IOException ex)
            {
                logger.LogCritical(ex.ToString());
            }
        }

        public bool TryConnect()
        {
            logger.LogInformation("RabbitMQ Client is trying to connect");

            lock (sync_root)
            {
                var policy = Policy.Handle<SocketException>()
                   .Or<BrokerUnreachableException>()
                   .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(5), (ex, time) =>
                   {
                       logger.LogWarning($"{ex.Message}");
                   }
                );
                policy.Execute(() => connection = connectionFactory.CreateConnection());
                if (IsConnected)
                {
                    connection.ConnectionShutdown += OnConnectionShutdown;
                    connection.CallbackException += OnCallbackException;
                    connection.ConnectionBlocked += OnConnectionBlocked;

                    logger.LogInformation($"RabbitMQ persistent connection acquired a connection {connection.Endpoint.HostName} and is subscribed to failure events");

                    return true;
                }
                else
                {
                    logger.LogCritical("RabbitMQ connections could not be created and opened");

                    return false;
                }
            }
        }


        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (disposed) return;

            logger.LogWarning("A RabbitMQ connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (disposed) return;

            logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (disposed) return;

            logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }
    }
}
