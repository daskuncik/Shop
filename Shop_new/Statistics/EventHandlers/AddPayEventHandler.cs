using System;
using System.Linq;
using System.Threading.Tasks;
using Statistics.EventBus;
using Statistics.Events;
using Statistics.Entities;
using Microsoft.Extensions.Logging;

namespace Statistics.EventHandlers
{
    public class AddPayEventHandler : EventHandler<AddPayEvent>
    {
        private ILogger<AddPayEventHandler> logger;

        public AddPayEventHandler(IEventBus eventBus,
            DbProxy proxy,
            ILogger<AddPayEventHandler> logger)
             : base(eventBus, proxy)
        {
            this.logger = logger;
        }

        public async override Task Handle(AddPayEvent @event)
        {
            try
            {
                logger.LogInformation($"Processing {@event.GetType().Name} {@event}");
                OrderOperationInfo entity = new OrderOperationInfo
                {
                    Id = @event.Id + @event.GetType().Name,
                    orderId = @event.Orderid,
                    userId = @event.Userid,
                    sum = @event.Sum,
                    Operation = Operation.Add_Pay,
                    actionMoment = @event.OccurenceTime

                };
                if (dbContext.OrderOperations.FirstOrDefault(r => r.Id == entity.Id) == null)
                {
                    dbContext.OrderOperations.Add(entity);
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
