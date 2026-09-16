using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Data.Tables;


namespace ABCRetailFunctionsProj2;

public class TableFunction
{
    private readonly ILogger<TableFunction> _logger;
    private readonly IConfiguration _config;

    public TableFunction(ILogger<TableFunction> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [Function("TableFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("TableFunction processing a request.");

        string name = req.Query["name"];
        string surname = req.Query["surname"];
        string email = req.Query["email"];
        string phone = req.Query["phone"];

        var connectionString = _config["StorageConnection"];
        var tableClient = new TableClient(connectionString, "Customers");
        await tableClient.CreateIfNotExistsAsync();

        var customer = new Customer
        {
            PartitionKey = "Customer",
            RowKey = Guid.NewGuid().ToString(),
            Name = name ?? "Test",
            Surname = surname ?? "Customer",
            Email = email ?? "test@example.com",
            PhoneNumber = phone ?? "0000000000"
        };

        await tableClient.AddEntityAsync(customer);

        return new OkObjectResult($"Customer '{customer.Name} {customer.Surname}' added with RowKey {customer.RowKey}");
    }
}