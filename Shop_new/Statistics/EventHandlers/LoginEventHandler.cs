using System;
using System.Linq;
using System.Threading.Tasks;
using Statistics.EventBus;
using Statistics.Events;
using Statistics.Entities;
using Microsoft.Extensions.Logging;

namespace Statistics.EventHandlers
{
    public class LoginEventHandler : EventHandler<LoginEvent>
    {
        private ILogger<LoginEventHandler> logger;

        public LoginEventHandler(IEventBus eventBus,
            DbProxy proxy,
            ILogger<LoginEventHandler> logger)
            : base(eventBus, proxy)
        {
            this.logger = logger;
        }

        public async override Task Handle(LoginEvent @event)
        {
            try
            {
                var eventDescription = $"{@event.GetType().Name} { @event}";
                logger.LogInformation($"Processing {eventDescription}");
                LoginInfo entity = new LoginInfo
                {
                    DateTime = @event.OccurenceTime,
                    Username = @event.Name,
                    From = @event.Origin,
                    Id = @event.Id + @event.GetType().Name
                };
                if (dbContext.Logins.FirstOrDefault(r => r.Id == entity.Id) == null)
                {
                    dbContext.Logins.Add(entity);
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
