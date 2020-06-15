using BookStoreAggregateLoader.LegacyDb;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BookStoreAggregateLoader
{
    public class LegacyOrderRetrivalOrchestrator
    {
        private readonly BookStoreContext _bookStoreContext;
        private readonly IBookStoreApiClient _bookStoreApiClient;
        public LegacyOrderRetrivalOrchestrator(BookStoreContext bookStoreContext, IBookStoreApiClient bookStoreApiClient)
        {
            _bookStoreContext = bookStoreContext;
            _bookStoreApiClient = bookStoreApiClient;
        }

        [FunctionName(nameof(OrchestrateRetrievalAndTransformationOfLegacyOrderBatch))]
        public async Task OrchestrateRetrievalAndTransformationOfLegacyOrderBatch([OrchestrationTrigger] IDurableOrchestrationContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();
            var orders = await durableContext.CallActivityAsync<List<Order>>(nameof(GetLegacyOrdersActivity), (take, skip));

            var tasks = orders
                .Select(order => durableContext.CallActivityAsync(nameof(GetLegacyOrdersActivity), order))
                .ToList();

            await Task.WhenAll(tasks);
        }

        [FunctionName(nameof(GetLegacyOrdersActivity))]
        public async Task<List<Order>> GetLegacyOrdersActivity([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();

            return await _bookStoreContext.Orders.AsNoTracking().Take(take).Skip(skip).ToListAsync();
        }

        [FunctionName(nameof(TransformLegacyOrderAndSendToEventStoreApi))]
        public async Task<IResult> TransformLegacyOrderAndSendToEventStoreApi([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var legacyOrder = durableContext.GetInput<Order>();
            IResult result;

            var request = new
            {

            };

            var response = await _bookStoreApiClient.TryPostAsync("/orders", request);

            if (response.IsSuccessStatusCode)
            {
                var aggregateId = Guid.Parse(await response.Content.ReadAsStringAsync());
                //_bookStoreContext.Correlation.Add(new Correlation() { TableName = "Orders", LegacyId = legacyOrder.Id, AggregateId = aggregateId });
                //_bookStoreContext.SaveChanges();
                result = new Success();
            }
            else { 
                result = new Error();
            }

            return result;

        }
    }
}
