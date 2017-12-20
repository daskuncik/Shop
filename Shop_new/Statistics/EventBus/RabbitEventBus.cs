using System.Linq;
using System.Threading.Tasks;
using Statistics.EventHandlers;
using Statistics.Events;
using RabbitMQ.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Text;
using System;
using Statistics.RabbitHelper;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Logging;
using Polly.Retry;
using RabbitMQ.Client.Exceptions;
using System.Net.Sockets;
using Polly;
using System.Threading;
using Statistics.EventsStorage;
using System.Collections.Generic;


namespace Statistics.EventBus
{
    public class RabbitEventBus : IEventBus
    {
        private string exchangeName = "statistics";
        private string queueName = "custom_queue";
        private Dictionary<string, List<IEventHandler>> handlers = new Dictionary<string, List<IEventHandler>>();
        private List<Type> eventTypes = new List<Type>();
        private IRabbitPersistentConnection connection;
        private IModel consumerChannel;
        private IEventStorage eventStorage;
        private ILogger<RabbitEventBus> logger;
        private int retryCount = 2;
        private RetryPolicy policy;

        public RabbitEventBus(IConfiguration configuration, ILogger<RabbitEventBus> logger, IEventStorage eventStorage)
        {
            this.connection = new RabbitPersistentConnection(configuration, logger);
            policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .Or<InvalidOperationException>()
                .WaitAndRetry(retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                {
                    logger.LogWarning(ex.ToString());
                });
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.consumerChannel = CreateConsumerChannel();
            this.eventStorage = eventStorage;
        }

        public void Publish(Event @event, bool ack = false)
        {
            new Thread(o =>
            {
                try
                {
                    @event = o as Event;

                    if (ack)
                    {
                        int @try = 0;
                        eventStorage.AddEvent(@event);
                        while (@try < retryCount)
                        {
                            if ((@event = eventStorage.GetEvent(@event.Id)) != null)
                                PublishInner(@event);
                            else
                                return;
                            Thread.Sleep(5000);
                        }
                        logger.LogCritical($"Failed to publish event after {retryCount} tries");
                    }
                    else
                    {
                        PublishInner(@event);
                    }
                }
                catch (Exception e)
                {
                    logger.LogCritical($"Failed to publish event, {e}");
                }
            }).Start(@event);
        }

        public void Subscribe<T>(EventHandlers.EventHandler<T> eventHandler) where T : Event
        {
            if (!connection.IsConnected)
                connection.TryConnect();

            var name = typeof(T).FullName;

            if (handlers.ContainsKey(name))
                handlers[name].Add(eventHandler);
            else
            {
                policy.Execute(() =>
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: name);
                        handlers.Add(name, new List<IEventHandler>());
                        handlers[name].Add(eventHandler);
                        eventTypes.Add(typeof(T));
                    }
                });
            }
        }

        public void Unsubscribe<T>(EventHandlers.EventHandler<T> eventHandler) where T : Event
        {
            if (!connection.IsConnected)
                connection.TryConnect();

            var name = typeof(T).FullName;

            if (handlers.ContainsKey(name) && handlers[name].Contains(eventHandler))
                handlers[name].Remove(eventHandler);
        }

        private void PublishInner(Event @event)
        {
            if (!connection.IsConnected)
                connection.TryConnect();

            @event.OccurenceTime = DateTime.Now;
            var name = @event.GetType().FullName;

            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(exchange: exchangeName, type: "direct");
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));
                channel.BasicPublish(exchange: exchangeName,
                    routingKey: name,
                    basicProperties: null,
                    body: body);
            }
        }

        private IModel CreateConsumerChannel()
        {
            if (!connection.IsConnected)
                connection.TryConnect();

            return policy.Execute(() =>
            {
                var channel = connection.CreateModel();
                channel.ExchangeDeclare(exchange: exchangeName, type: "direct");
                queueName = channel.QueueDeclare().QueueName;

                var consumer = new EventingBasicConsumer(channel);
                consumer.ConsumerCancelled += (sender, args) =>
                {
                    consumerChannel.Dispose();
                    consumerChannel = CreateConsumerChannel();
                };
                consumer.Received += async (model, ea) =>
                {
                    var eventName = ea.RoutingKey;
                    var message = Encoding.UTF8.GetString(ea.Body);
                    await ProcessEvent(eventName, message);
                    channel.BasicAck(ea.DeliveryTag, false);
                };
                channel.CallbackException += (sender, ea) =>
                {
                    consumerChannel.Dispose();
                    consumerChannel = CreateConsumerChannel();
                };

                channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
                return channel;
            });
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            if (handlers.ContainsKey(eventName))
            {
                var type = Type.GetType(eventName);
                var template = typeof(EventHandlers.EventHandler<>);
                var @event = JsonConvert.DeserializeObject(message, type);
                foreach (var handler in handlers[eventName])
                {
                    try
                    {
                        string id = (@event as Event).Id;
                        {
                            var genericType = template.MakeGenericType(type);
                            await (Task)genericType.GetMethod("Handle").Invoke(handler, new object[] { @event });
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogCritical($"Exception while trying to execute event handler: {e}");
                    }
                }
            }
        }
    }
}
