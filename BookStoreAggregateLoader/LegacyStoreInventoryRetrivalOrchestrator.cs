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
    public class LegacyStoreInventoryRetrivalOrchestrator
    {
        private readonly BookStoreContext _bookStoreContext;
        private readonly IBookStoreApiClient _bookStoreApiClient;
        public LegacyStoreInventoryRetrivalOrchestrator(BookStoreContext bookStoreContext, IBookStoreApiClient bookStoreApiClient)
        {
            _bookStoreContext = bookStoreContext;
            _bookStoreApiClient = bookStoreApiClient;
        }

        [FunctionName(nameof(OrchestrateRetrievalAndTransformationOfLegacyStoreInventoryBatch))]
        public async Task OrchestrateRetrievalAndTransformationOfLegacyStoreInventoryBatch([OrchestrationTrigger] IDurableOrchestrationContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();
            var storeInventorys = await durableContext.CallActivityAsync<List<Inventory>>(nameof(GetLegacyStoreInventorysActivity), (take, skip));

            var tasks = storeInventorys
                .Select(storeInventory => durableContext.CallActivityAsync(nameof(GetLegacyStoreInventorysActivity), storeInventory))
                .ToList();

            await Task.WhenAll(tasks);
        }

        [FunctionName(nameof(GetLegacyStoreInventorysActivity))]
        public async Task<List<Inventory>> GetLegacyStoreInventorysActivity([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();

            return await _bookStoreContext.StoreInventory.AsNoTracking().Take(take).Skip(skip).ToListAsync();
        }

        [FunctionName(nameof(TransformLegacyStoreInventoryAndSendToEventStoreApi))]
        public async Task<IResult> TransformLegacyStoreInventoryAndSendToEventStoreApi([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var legacyStoreInventory = durableContext.GetInput<Inventory>();
            IResult result;

            var request = new
            {

            };

            var response = await _bookStoreApiClient.TryPostAsync("/storeInventorys", request);

            if (response.IsSuccessStatusCode)
            {
                var aggregateId = Guid.Parse(await response.Content.ReadAsStringAsync());
                //_bookStoreContext.Correlation.Add(new Correlation() { TableName = "StoreInventory", LegacyId = legacyStoreInventory.Id, AggregateId = aggregateId });
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
