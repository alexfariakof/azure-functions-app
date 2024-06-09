using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace FuncApp.DurableOrchestration;
public static class DurableOrchestrationFunction
{
    [Function(nameof(DurableOrchestrationFunction))]
    public static async Task<List<string>> RunOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        ILogger logger = context.CreateReplaySafeLogger(nameof(DurableOrchestrationFunction));
        logger.LogInformation("Durable Functions App Example");
        var outputs = new List<string>();

        outputs.Add(await context.CallActivityAsync<string>(nameof(Question), "How are you ?"));
        await context.CreateTimer(TimeSpan.FromMinutes(1), CancellationToken.None);

        outputs.Add(await context.CallActivityAsync<string>(nameof(Question), "What is your name ?"));
        await context.CreateTimer(TimeSpan.FromMinutes(1), CancellationToken.None);

        outputs.Add(await context.CallActivityAsync<string>(nameof(Question), "How much do I owe you ?"));
        await context.CreateTimer(TimeSpan.FromMinutes(1), CancellationToken.None);

        outputs.Add(await context.CallActivityAsync<string>(nameof(Question), "Stuff"));

        return outputs;
    }

    [Function(nameof(Question))]
    public static string Question([ActivityTrigger] string question, FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("Question");
        logger.LogInformation("Received question: {question}", question);
        var response = GenerateResponse(question);
        logger.LogInformation(response);
        return response;
    }

    [Function("DurableOrchestrationFunction_HttpStart")]
    public static async Task<HttpResponseData> HttpStart(
        [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
        [DurableClient] DurableTaskClient client,
        FunctionContext executionContext)
    {
        ILogger logger = executionContext.GetLogger("DurableOrchestrationFunction_HttpStart");

        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
            nameof(DurableOrchestrationFunction));

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

        // Returns an HTTP 202 response with an instance management payload.
        // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
        return await client.CreateCheckStatusResponseAsync(req, instanceId);
    }
    private static string GenerateResponse(string question)
    {
        // Lógica para gerar uma resposta com base na pergunta
        switch (question.ToLower())
        {
            case string q when q.Contains("how are you"):
                return "I am fine!";
            case string q when q.Contains("what is your name"):
                return "My name is Function App!";
            case string q when q.Contains("how much do i owe you"):
                return "You don't owe me anything!";
            default:
                return "Sorry, I don't understand your question.";
        }
    }
}
