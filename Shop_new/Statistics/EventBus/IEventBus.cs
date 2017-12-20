using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.EventBus
{
    public interface IEventBus
    {
        void Publish(Event @event, bool ack = false);
        void Subscribe<T>(EventHandlers.EventHandler<T> eventHandler) where T : Event;
        void Unsubscribe<T>(EventHandlers.EventHandler<T> eventHandler) where T : Event;
    }
}
