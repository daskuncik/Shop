using System;
using RabbitMQ.Client;

namespace Statistics.RabbitHelper
{
    interface IRabbitPersistentConnection :IDisposable
    {
        bool IsConnected { get; }
        bool TryConnect();
        IModel CreateModel();
    }
}
