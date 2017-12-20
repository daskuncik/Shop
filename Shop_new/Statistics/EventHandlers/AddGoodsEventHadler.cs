using Microsoft.Extensions.Logging;
using Statistics.EventBus;
using Statistics.Events;
using Statistics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Statistics.EventHandlers
{
    public class AddGoodsEventHadler : EventHandler<AddGoodsEvent>
    {
        private ILogger<AddGoodsEventHadler> logger;

        public AddGoodsEventHadler(IEventBus eventBus,
            DbProxy proxy,
            ILogger<AddGoodsEventHadler> logger)
             : base(eventBus, proxy)
        {
            this.logger = logger;
        }

        public async override Task Handle(AddGoodsEvent @event)
        {
            try
            {
                logger.LogInformation($"Processing {@event.GetType().Name} {@event}");
                GoodsOperationInfo entity = new GoodsOperationInfo
                {
                    Id = @event.Id + @event.GetType().Name,
                    orderId = @event.Orderid,
                    goodsId = @event.Goodsid,
                    Operation = Operation.Add,
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
