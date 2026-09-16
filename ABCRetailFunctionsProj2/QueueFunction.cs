using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues;
using System.Text.Json;

namespace ABCRetailFunctionsProj2;

public class QueueFunction
{
    private readonly ILogger<QueueFunction> _logger;
    private readonly IConfiguration _config;

    public QueueFunction(ILogger<QueueFunction> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [Function("QueueFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("QueueFunction processing a request.");

        string productId = req.Query["productId"];
        string productName = req.Query["productName"];
        string action = req.Query["action"];

        var order = new Order
        {
            OrderId = Guid.NewGuid().ToString(),
            ProductId = productId ?? "P001",
            ProductName = productName ?? "Test Product",
            Action = action ?? "Created",
            Timestamp = DateTime.Now
        };

        var connectionString = _config["StorageConnection"];
        var queueClient = new QueueClient(connectionString, "orderqueue");
        await queueClient.CreateIfNotExistsAsync();

        string json = JsonSerializer.Serialize(order);
        await queueClient.SendMessageAsync(json);

        return new OkObjectResult($"Order message sent to queue: {json}");
    }
}