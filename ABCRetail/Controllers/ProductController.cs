using ABCRetail.Models;
using ABCRetail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class ProductController : Controller
    {
        private readonly TableStorageService tableStorageService; 
         
        public ProductController(TableStorageService tableStorageService)
        {
            this.tableStorageService = tableStorageService;
        }
        public async Task<IActionResult> Index()
        {
            var products = await tableStorageService.GetAllProductsAsync(); 
            return View(products);
        }

        [HttpGet] 
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product product, IFormFile imageFile)
        {
            product.PartitionKey = "Product";
            product.RowKey = Guid.NewGuid().ToString();

            if (imageFile != null)
            {
                product.ImageUrl = await tableStorageService.UploadImageAsync(imageFile);
                await tableStorageService.SendMessageAsync($"Image uploaded: {imageFile.FileName}");
                await tableStorageService.LogMessageAsync($"Image created: {imageFile.FileName}");
            }

            await tableStorageService.AddProductAsync(product);
            await tableStorageService.SendMessageAsync($"Product created: {product.ProductName}");
            await tableStorageService.LogMessageAsync($"Product created: {product.ProductName}");

            return RedirectToAction("Index");
        }

        [HttpPost] 
        public async Task<IActionResult> Delete(string partitionKey , string rowKey)
        {
            await tableStorageService.DeleteProductAsync(partitionKey, rowKey);
            await tableStorageService.SendMessageAsync($"Product deleted: {rowKey}");
            await tableStorageService.LogMessageAsync($"Product deleted: {rowKey}");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var products = await tableStorageService.GetAllProductsAsync();
            var product = products.FirstOrDefault(p => p.PartitionKey == partitionKey && p.RowKey == rowKey);
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product product, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                product.ImageUrl = await tableStorageService.UploadImageAsync(imageFile);
                await tableStorageService.SendMessageAsync($"Image updated: {imageFile.FileName}");
                await tableStorageService.LogMessageAsync($"Image updated: {imageFile.FileName}");
            }

            await tableStorageService.UpdateProductAsync(product);
            await tableStorageService.SendMessageAsync($"Product updated: {product.ProductName}");
            await tableStorageService.LogMessageAsync($"Product updated: {product.ProductName}");
            TempData["Message"] = $"Product '{product.ProductName}' updated successfully.";
            return RedirectToAction("Index");
        }


    }
}
