using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Storage.Files.Shares;
using System.Text;

namespace ABCRetailFunctionsProj2;

public class FilesFunction
{
    private readonly ILogger<FilesFunction> _logger;
    private readonly IConfiguration _config;

    public FilesFunction(ILogger<FilesFunction> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [Function("FilesFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("FilesFunction processing a request.");

        string message = req.Query["message"];
        string logMessage = message ?? "Test log entry from Azure Function";

        var connectionString = _config["StorageConnection"];
        var shareClient = new ShareClient(connectionString, "logs");
        await shareClient.CreateIfNotExistsAsync();

        var directory = shareClient.GetRootDirectoryClient();
        string fileName = $"log_{DateTime.Now:yyyyMMdd_HHmmssfff}.txt";
        var file = directory.GetFileClient(fileName);

        string logEntry = $"{DateTime.Now}: {logMessage}\n";
        byte[] content = Encoding.UTF8.GetBytes(logEntry);

        await file.CreateAsync(content.Length);
        using (var stream = new MemoryStream(content))
        {
            await file.UploadRangeAsync(new Azure.HttpRange(0, content.Length), stream);
        }

        return new OkObjectResult($"Log file '{fileName}' created with entry: {logMessage}");
    }
}