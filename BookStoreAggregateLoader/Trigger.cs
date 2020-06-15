using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net.Http;

namespace BookStoreAggregateLoader
{
    public class Trigger
    {
        [FunctionName(nameof(TriggerMethod))]
        public static async Task<HttpResponseMessage> TriggerMethod([HttpTrigger(AuthorizationLevel.Function,"post", Route = null)]
            AggregateLoaderRequest req, [DurableClient] IDurableOrchestrationClient starter, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var instanceId = Guid.NewGuid();
            try
            {
                //instanceId = await starter.StartNewAsync <(string, string)> (nameof(AggregateOrchestrator.Orchestrate), instanceId, ("input1", "input2"));
            }
            catch (Exception e)
            {
                log.LogError("something went wrong in trigger");
                throw e;
            }

            return starter.CreateCheckStatusResponse(req, instanceId.ToString());
        }
    }

    public class AggregateLoaderRequest : HttpRequestMessage
    {

    }
}
