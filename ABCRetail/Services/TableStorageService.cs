using ABCRetail.Models;
using Azure;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Files.Shares;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

namespace ABCRetail.Services
{
    public class TableStorageService
    {
        private readonly TableClient _customerTable;
        private readonly TableClient _productTable;
        private readonly BlobContainerClient _blobContainer;
        private readonly QueueClient _orderQueue;
        private readonly ShareClient _fileShare;

        public TableStorageService(IConfiguration configuration)
        {
            string connectionString = configuration["AzureStorage:ConnectionString"];
            _customerTable = new TableClient(connectionString, "Customers");
            _customerTable.CreateIfNotExists();


            _productTable = new TableClient(connectionString, "Products");
            _productTable.CreateIfNotExists();

            _blobContainer = new BlobContainerClient(connectionString, "productimages");
            _blobContainer.CreateIfNotExists(PublicAccessType.Blob);

            _orderQueue = new QueueClient(connectionString, "orderqueue");
            _orderQueue.CreateIfNotExists();

            _fileShare = new ShareClient(connectionString, "logs");
            _fileShare.CreateIfNotExists();
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            await _customerTable.AddEntityAsync(customer);
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = _customerTable.Query<Customer>();
            return customers.ToList();
        }

        public async Task DeleteCustomerAsync(string partitionKey, string rowKey)
        {
            await _customerTable.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            await _customerTable.UpsertEntityAsync(customer);
        }

        public async Task AddProductAsync(Product product)
        {
            await _productTable.AddEntityAsync(product);
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var products = _productTable.Query<Product>();
            return products.ToList();
        }

        public async Task DeleteProductAsync(string partitionKey, string rowKey)
        {
            await _productTable.DeleteEntityAsync(partitionKey, rowKey);
        }

        public async Task UpdateProductAsync(Product product)
        {
            await _productTable.UpsertEntityAsync(product);
        }

        public async Task<string> UploadImageAsync(IFormFile file)
        {
            var blobClient = _blobContainer.GetBlobClient(file.FileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            return blobClient.Uri.ToString();
        }

        public async Task SendMessageAsync(string message)
        {
            await _orderQueue.SendMessageAsync(message);
        }

        public async Task LogMessageAsync(string message)
        {
            var directory = _fileShare.GetRootDirectoryClient();

            string fileName = $"log_{DateTime.Now:yyyyMMdd_HHmmssfff}.txt";
            var file = directory.GetFileClient(fileName);

            string logEntry = $"{DateTime.Now}: {message}\n";
            byte[] content = Encoding.UTF8.GetBytes(logEntry);

            await file.CreateAsync(content.Length);

            using (var stream = new MemoryStream(content))
            {
                await file.UploadRangeAsync(new HttpRange(0, content.Length), stream);
            }
        }

        public async Task SendOrderMessageAsync(Order order)
        {
            string json = JsonSerializer.Serialize(order);
            await _orderQueue.SendMessageAsync(json);
        } 
         

    }
}
