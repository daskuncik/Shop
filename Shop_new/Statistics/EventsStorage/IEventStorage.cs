using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.EventsStorage
{
    public interface IEventStorage
    {
        Event GetEvent(string id);
        bool AddEvent(Event @event);
        bool RemoveEvent(string id);
    }
}
