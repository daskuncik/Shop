using System;
using System.Linq;
using System.Threading.Tasks;
using Statistics.EventBus;
using Statistics.Events;
using Statistics.Entities;
using Microsoft.Extensions.Logging;

namespace Statistics.EventHandlers
{
    public class DeleteGoodsEventHandler : EventHandler<DeleteGoodsEvent>
    {
        private ILogger<DeleteGoodsEventHandler> logger;

        public DeleteGoodsEventHandler(IEventBus eventBus,
            DbProxy proxy,
            ILogger<DeleteGoodsEventHandler> logger)
             : base(eventBus, proxy)
        {
            this.logger = logger;
        }

        public async override Task Handle(DeleteGoodsEvent @event)
        {
            try
            {
                logger.LogInformation($"Processing {@event.GetType().Name} {@event}");
                GoodsOperationInfo entity = new GoodsOperationInfo
                {
                    Id = @event.Id + @event.GetType().Name,
                    orderId = @event.Orderid,
                    goodsId = @event.Goodsid,
                    Operation = Operation.Delete,
                    actionMoment = @event.OccurenceTime

                };
                if (dbContext.GoodOperations.FirstOrDefault(r => r.Id == entity.Id) == null)
                {
                    dbContext.GoodOperations.Add(entity);
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
