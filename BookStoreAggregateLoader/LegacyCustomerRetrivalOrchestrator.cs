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
    public class LegacyCustomerRetrivalOrchestrator
    {
        private readonly BookStoreContext _bookStoreContext;
        private readonly IBookStoreApiClient _bookStoreApiClient;
        public LegacyCustomerRetrivalOrchestrator(BookStoreContext bookStoreContext, IBookStoreApiClient bookStoreApiClient)
        {
            _bookStoreContext = bookStoreContext;
            _bookStoreApiClient = bookStoreApiClient;
        }

        [FunctionName(nameof(OrchestrateRetrievalAndTransformationOfLegacyCustomerBatch))]
        public async Task OrchestrateRetrievalAndTransformationOfLegacyCustomerBatch([OrchestrationTrigger] IDurableOrchestrationContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();
            var customers = await durableContext.CallActivityAsync<List<Customer>>(nameof(GetLegacyCustomersActivity), (take, skip));

            var tasks = customers
                .Select(customer => durableContext.CallActivityAsync(nameof(GetLegacyCustomersActivity), customer))
                .ToList();

            await Task.WhenAll(tasks);
        }

        [FunctionName(nameof(GetLegacyCustomersActivity))]
        public async Task<List<Customer>> GetLegacyCustomersActivity([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var (take, skip) = durableContext.GetInput<(int, int)>();

            return await _bookStoreContext.Customers.AsNoTracking().Take(take).Skip(skip).ToListAsync();
        }

        [FunctionName(nameof(TransformLegacyCustomerAndSendToEventStoreApi))]
        public async Task<IResult> TransformLegacyCustomerAndSendToEventStoreApi([ActivityTrigger] IDurableActivityContext durableContext, ILogger log)
        {
            var legacyCustomer = durableContext.GetInput<Customer>();
            IResult result;

            var request = new
            {

            };

            var response = await _bookStoreApiClient.TryPostAsync("/customers", request);

            if (response.IsSuccessStatusCode)
            {
                var aggregateId = Guid.Parse(await response.Content.ReadAsStringAsync());
                //_bookStoreContext.Correlation.Add(new Correlation() { TableName = "Customers", LegacyId = legacyCustomer.Id, AggregateId = aggregateId });
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
