using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace FuncApp.DurableHttp
{
    public static class DurableHttpFunction
    {
        [Function(nameof(DurableHttpFunction))]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            ILogger logger = context.CreateReplaySafeLogger(nameof(DurableHttpFunction));
            logger.LogInformation("Saying hello.");
            var outputs = new List<string>();

            outputs.Add(await context.CallActivityAsync<string>(nameof(Question), "How are you ?"));
            await context.CreateTimer(TimeSpan.FromMinutes(2), CancellationToken.None);

            outputs.Add(await context.CallActivityAsync<string>(nameof(Question), "What is your name ?"));
            await context.CreateTimer(TimeSpan.FromMinutes(2), CancellationToken.None);

            outputs.Add(await context.CallActivityAsync<string>(nameof(Question), "How much do I owe you ?"));

            return outputs;
        }

        [Function(nameof(Question))]
        public static string Question([ActivityTrigger] string name, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("Question");
            logger.LogInformation("Saying hello to {name}.", name);
            return $"Hello {name}!";
        }

        [Function("DurableHttpFunction_HttpStart")]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("DurableHttpFunction_HttpStart");

            // Function input comes from the request content.
            string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                nameof(DurableHttpFunction));

            logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            return await client.CreateCheckStatusResponseAsync(req, instanceId);
        }
    }
}
