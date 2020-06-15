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
    public class LegacyPublisherRetrivalOrchestrator
    {
        private readonly BookStoreContext _bookStoreContext;
        private readonly IBookStoreApiClient _bookStoreApiClient;
        public LegacyPublisherRetrivalOrchestrator(BookStoreContext bookStoreContext, IBookStoreApiClient bookStoreApiClient)
        {
            _bookStoreContext = bookStoreContext;
            _bookStoreApiClient = bookStoreApiClient;
        }

        [FunctionName(nameof(OrchestrateRetrievalAndTransformationOfLegacyPublisherBatch))]
        public async Task OrchestrateRetrievalAndTransformationOfLegacyPublisherBatch([OrchestrationTrigger] IDurableOrchestrationContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();
            var publishers = await durableContext.CallActivityAsync<List<Publisher>>(nameof(GetLegacyPublishersActivity), (take, skip));

            var tasks = publishers
                .Select(publisher => durableContext.CallActivityAsync(nameof(GetLegacyPublishersActivity), publisher))
                .ToList();

            await Task.WhenAll(tasks);
        }

        [FunctionName(nameof(GetLegacyPublishersActivity))]
        public async Task<List<Publisher>> GetLegacyPublishersActivity([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();

            return await _bookStoreContext.Publishers.AsNoTracking().Take(take).Skip(skip).ToListAsync();
        }

        [FunctionName(nameof(TransformLegacyPublisherAndSendToEventStoreApi))]
        public async Task<IResult> TransformLegacyPublisherAndSendToEventStoreApi([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var legacyPublisher = durableContext.GetInput<Publisher>();
            IResult result;

            var request = new
            {

            };

            var response = await _bookStoreApiClient.TryPostAsync("/publishers", request);

            if (response.IsSuccessStatusCode)
            {
                var aggregateId = Guid.Parse(await response.Content.ReadAsStringAsync());
                //_bookStoreContext.Correlation.Add(new Correlation() { TableName = "Publishers", LegacyId = legacyPublisher.Id, AggregateId = aggregateId });
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
