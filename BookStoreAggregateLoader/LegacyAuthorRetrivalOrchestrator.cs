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
    public class LegacyAuthorRetrivalOrchestrator
    {
        private readonly BookStoreContext _bookStoreContext;
        private readonly IBookStoreApiClient _bookStoreApiClient;
        public LegacyAuthorRetrivalOrchestrator(BookStoreContext bookStoreContext, IBookStoreApiClient bookStoreApiClient)
        {
            _bookStoreContext = bookStoreContext;
            _bookStoreApiClient = bookStoreApiClient;
        }

        [FunctionName(nameof(OrchestrateRetrievalAndTransformationOfLegacyAuthorBatch))]
        public async Task OrchestrateRetrievalAndTransformationOfLegacyAuthorBatch([OrchestrationTrigger] IDurableOrchestrationContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();
            var authors = await durableContext.CallActivityAsync<List<Author>>(nameof(GetLegacyAuthorsActivity), (take, skip));

            var tasks = authors
                .Select(author => durableContext.CallActivityAsync(nameof(GetLegacyAuthorsActivity), author))
                .ToList();

            await Task.WhenAll(tasks);
        }

        [FunctionName(nameof(GetLegacyAuthorsActivity))]
        public async Task<List<Author>> GetLegacyAuthorsActivity([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();

            return await _bookStoreContext.Authors.AsNoTracking().Take(take).Skip(skip).ToListAsync();
        }

        [FunctionName(nameof(TransformLegacyAuthorAndSendToEventStoreApi))]
        public async Task<IResult> TransformLegacyAuthorAndSendToEventStoreApi([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var legacyAuthor = durableContext.GetInput<Author>();
            IResult result;

            var request = new
            {

            };

            var response = await _bookStoreApiClient.TryPostAsync("/authors", request);

            if (response.IsSuccessStatusCode)
            {
                var aggregateId = Guid.Parse(await response.Content.ReadAsStringAsync());
                //_bookStoreContext.Correlation.Add(new Correlation() { TableName = "Authors", LegacyId = legacyAuthor.Id, AggregateId = aggregateId });
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
