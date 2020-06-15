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
    public class LegacyBookRetrivalOrchestrator
    {
        private readonly BookStoreContext _bookStoreContext;
        private readonly IBookStoreApiClient _bookStoreApiClient;
        public LegacyBookRetrivalOrchestrator(BookStoreContext bookStoreContext, IBookStoreApiClient bookStoreApiClient)
        {
            _bookStoreContext = bookStoreContext;
            _bookStoreApiClient = bookStoreApiClient;
        }

        [FunctionName(nameof(OrchestrateRetrievalAndTransformationOfLegacyBookBatch))]
        public async Task OrchestrateRetrievalAndTransformationOfLegacyBookBatch([OrchestrationTrigger] IDurableOrchestrationContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();
            var books = await durableContext.CallActivityAsync<List<Book>>(nameof(GetLegacyBooksActivity), (take, skip));

            var tasks = books
                .Select(book => durableContext.CallActivityAsync(nameof(GetLegacyBooksActivity), book))
                .ToList();

            await Task.WhenAll(tasks);
        }

        [FunctionName(nameof(GetLegacyBooksActivity))]
        public async Task<List<Book>> GetLegacyBooksActivity([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();

            return await _bookStoreContext.Books.AsNoTracking().Take(take).Skip(skip).ToListAsync();
        }

        [FunctionName(nameof(TransformLegacyBookAndSendToEventStoreApi))]
        public async Task<IResult> TransformLegacyBookAndSendToEventStoreApi([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var legacyBook = durableContext.GetInput<Book>();
            IResult result;

            //var newPublisherId = _bookStoreContext.Correlation
            //    .Where(_ => string.Equals(_.TableName, "Publishers", StringComparison.InvariantCultureIgnoreCase) &&
            //        _.LegacyId == legacyBook.PublisherId).Select(_ => _.AggregateId);
            //var newAuthorId = _bookStoreContext.Correlation
            //    .Where(_ => string.Equals(_.TableName, "Authors", StringComparison.InvariantCultureIgnoreCase) &&
            //        _.LegacyId == legacyBook.AuthorId).Select(_ => _.AggregateId);

            var request = new
            {

            };

            var response = await _bookStoreApiClient.TryPostAsync("/books", request);

            if (response.IsSuccessStatusCode)
            {
                var aggregateId = Guid.Parse(await response.Content.ReadAsStringAsync());
                //_bookStoreContext.Correlation.Add(new Correlation() { TableName = "Books", LegacyId = legacyBook.Id, AggregateId = aggregateId });
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
