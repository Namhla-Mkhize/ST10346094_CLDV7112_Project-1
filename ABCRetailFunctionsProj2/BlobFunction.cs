using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace ABCRetailFunctionsProj2;

public class BlobFunction
{
    private readonly ILogger<BlobFunction> _logger;
    private readonly IConfiguration _config;

    public BlobFunction(ILogger<BlobFunction> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    [Function("BlobFunction")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("BlobFunction processing a request.");

        if (req.Form.Files.Count == 0)
        {
            return new BadRequestObjectResult("No file uploaded. Use form-data with key 'file'.");
        }

        var file = req.Form.Files[0];
        var connectionString = _config["StorageConnection"];
        var containerClient = new BlobContainerClient(connectionString, "productimages");
        await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

        var blobClient = containerClient.GetBlobClient(file.FileName);
        using (var stream = file.OpenReadStream())
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        return new OkObjectResult($"File '{file.FileName}' uploaded. URL: {blobClient.Uri}");
    }
}