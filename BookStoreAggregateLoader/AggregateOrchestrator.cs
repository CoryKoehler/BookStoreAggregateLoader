using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookStoreAggregateLoader
{
    public class AggregateOrchestrator
    {
        [FunctionName(nameof(OrchestrateAggregateLoad))]
        public async Task OrchestrateAggregateLoad([OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext, ILogger log)
        {
            await Task.WhenAll(
                orchestrationContext.CallSubOrchestratorAsync(nameof(OrchestrateAuthors), new { }),
                orchestrationContext.CallSubOrchestratorAsync(nameof(OrchestratePublishers), new { })
                );

            await Task.WhenAll(
                orchestrationContext.CallSubOrchestratorAsync(nameof(OrchestrateBooks), new { }));

            await Task.WhenAll(
               orchestrationContext.CallSubOrchestratorAsync(nameof(OrchestrateOrders), new { }),
               orchestrationContext.CallSubOrchestratorAsync(nameof(OrchestrateStoreInventory), new { })
               );

            await Task.WhenAll(
              orchestrationContext.CallSubOrchestratorAsync(nameof(OrchestrateCustomers), new { })
              );
        }

        [FunctionName(nameof(OrchestrateAuthors))]
        public async Task OrchestrateAuthors([OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            var take = 1000;
            var parallelTasks = new List<Task>();
            var numberOfEntity = 10000;

            for (var pageNumber = 0; numberOfEntity >= (pageNumber * take); pageNumber++)
            {
                var skip = pageNumber * take;
                var activity = orchestrationContext.CallSubOrchestratorAsync<LegacyAuthorRetrivalOrchestrator>(
                    nameof(LegacyAuthorRetrivalOrchestrator.OrchestrateRetrievalAndTransformationOfLegacyAuthorBatch), (take, skip));
                parallelTasks.Add(activity);
            }

            await Task.WhenAll(parallelTasks);
        }

        [FunctionName(nameof(OrchestratePublishers))]
        public async Task OrchestratePublishers([OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            var take = 1000;
            var parallelTasks = new List<Task>();
            var numberOfEntity = 10000;

            for (var pageNumber = 0; numberOfEntity >= (pageNumber * take); pageNumber++)
            {
                var skip = pageNumber * take;
                var activity = orchestrationContext.CallSubOrchestratorAsync<LegacyPublisherRetrivalOrchestrator>(
                    nameof(LegacyPublisherRetrivalOrchestrator.OrchestrateRetrievalAndTransformationOfLegacyPublisherBatch), (take, skip));
                parallelTasks.Add(activity);
            }

            await Task.WhenAll(parallelTasks);
        }


        [FunctionName(nameof(OrchestrateBooks))]
        public async Task OrchestrateBooks([OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
            ILogger log)
        {
            var take = 1000;
            var parallelTasks = new List<Task>();
            var numberOfEntity = 10000;

            for (var pageNumber = 0; numberOfEntity >= (pageNumber * take); pageNumber++)
            {
                var skip = pageNumber * take;
                var activity = orchestrationContext.CallSubOrchestratorAsync<LegacyBookRetrivalOrchestrator>(
                    nameof(LegacyBookRetrivalOrchestrator.OrchestrateRetrievalAndTransformationOfLegacyBookBatch), (take, skip));
                parallelTasks.Add(activity);
            }

            await Task.WhenAll(parallelTasks);
        }

        [FunctionName(nameof(OrchestrateOrders))]
        public async Task OrchestrateOrders([OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
           ILogger log)
        {
            var take = 1000;
            var parallelTasks = new List<Task>();
            var numberOfEntity = 10000;

            for (var pageNumber = 0; numberOfEntity >= (pageNumber * take); pageNumber++)
            {
                var skip = pageNumber * take;
                var activity = orchestrationContext.CallSubOrchestratorAsync<LegacyOrderRetrivalOrchestrator>(
                    nameof(LegacyOrderRetrivalOrchestrator.OrchestrateRetrievalAndTransformationOfLegacyOrderBatch), (take, skip));
                parallelTasks.Add(activity);
            }

            await Task.WhenAll(parallelTasks);
        }

        [FunctionName(nameof(OrchestrateStoreInventory))]
        public async Task OrchestrateStoreInventory([OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
           ILogger log)
        {
            var take = 1000;
            var parallelTasks = new List<Task>();
            var numberOfEntity = 10000;

            for (var pageNumber = 0; numberOfEntity >= (pageNumber * take); pageNumber++)
            {
                var skip = pageNumber * take;
                var activity = orchestrationContext.CallSubOrchestratorAsync<LegacyStoreInventoryRetrivalOrchestrator>(
                    nameof(LegacyStoreInventoryRetrivalOrchestrator.OrchestrateRetrievalAndTransformationOfLegacyStoreInventoryBatch), (take, skip));
                parallelTasks.Add(activity);
            }

            await Task.WhenAll(parallelTasks);
        }

        [FunctionName(nameof(OrchestrateCustomers))]
        public async Task OrchestrateCustomers([OrchestrationTrigger] IDurableOrchestrationContext orchestrationContext,
          ILogger log)
        {
            var take = 1000;
            var parallelTasks = new List<Task>();
            var numberOfEntity = 10000;

            for (var pageNumber = 0; numberOfEntity >= (pageNumber * take); pageNumber++)
            {
                var skip = pageNumber * take;
                var activity = orchestrationContext.CallSubOrchestratorAsync<LegacyCustomerRetrivalOrchestrator>(
                    nameof(LegacyCustomerRetrivalOrchestrator.OrchestrateRetrievalAndTransformationOfLegacyCustomerBatch), (take, skip));
                parallelTasks.Add(activity);
            }

            await Task.WhenAll(parallelTasks);
        }
    }
}
