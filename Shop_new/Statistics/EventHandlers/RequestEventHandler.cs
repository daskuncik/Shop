using Statistics.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statistics.EventBus;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace Statistics.EventHandlers
{
    public class RequestEventHandler : EventHandler<RequestEvent>
    {
        private ILogger<RequestEventHandler> logger;

        public RequestEventHandler(IEventBus eventBus,
            DbProxy proxy,
            ILogger<RequestEventHandler> logger)
            : base(eventBus, proxy)
        {
            this.logger = logger;
        }

        public async override Task Handle(RequestEvent @event)
        {
            try
            {
                var eventDescription = $"{@event.GetType().Name} { @event}";
                logger.LogInformation($"Processing {eventDescription}");
                Entities.RequestInfo entity = new Entities.RequestInfo
                {
                    From = $"{@event.Origin}",
                    To = $"{@event.Host}{@event.Route}",
                    Time = @event.OccurenceTime,
                    RequestType = @event.RequestType,
                    Id = @event.Id + @event.GetType().Name
                };
                if (dbContext.Requests.FirstOrDefault(r => r.Id == entity.Id) == null)
                {
                    dbContext.Requests.Add(entity);
                    dbContext.SaveChanges();
                }
                eventBus.Publish(new AckEvent { AdjEventId = @event.Id, Status = AckStatus.Success });
            }
            catch (Exception e)
            {
                eventBus.Publish(new AckEvent { AdjEventId = @event.Id, Description = e.ToString(), Status = AckStatus.Failed });
                logger.LogCritical(e.ToString());
            }
        }
    }
}
