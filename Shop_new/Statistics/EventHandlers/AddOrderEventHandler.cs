using System;
using System.Linq;
using System.Threading.Tasks;
using Statistics.EventBus;
using Statistics.Events;
using Statistics.Entities;
using Microsoft.Extensions.Logging;

namespace Statistics.EventHandlers
{
    public class AddOrderEventHandler : EventHandler<AddOrderEvent>
    {
        private ILogger<AddOrderEventHandler> logger;

        public AddOrderEventHandler(IEventBus eventBus,
            DbProxy proxy,
            ILogger<AddOrderEventHandler> logger)
             : base(eventBus, proxy)
        {
            this.logger = logger;
        }

        public async override Task Handle(AddOrderEvent @event)
        {
            try
            {
                logger.LogInformation($"Processing {@event.GetType().Name} {@event}");
                OrderOperationInfo entity = new OrderOperationInfo
                {
                    Id = @event.Id + @event.GetType().Name,
                    orderId = @event.Orderid,
                    userId = @event.Userid,
                    sum = 0.ToString(),
                    Operation = Operation.Add,
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
