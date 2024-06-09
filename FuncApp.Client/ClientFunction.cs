using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace FuncApp.Client
{
    public class ClientFunction
    {
        private readonly ILogger<ClientFunction> _logger;

        public ClientFunction(ILogger<ClientFunction> logger)
        {
            _logger = logger;
        }

        [Function("ClientFunction")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("Client function processed a request.");
            return new OkObjectResult("Client Functions!");
        }
    }
}
