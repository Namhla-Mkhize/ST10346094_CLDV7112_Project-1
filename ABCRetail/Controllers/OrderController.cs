using ABCRetail.Models;
using ABCRetail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class OrderController : Controller
    {
        private readonly TableStorageService tableStorageService;

        public OrderController(TableStorageService tableStorageService)
        {
            this.tableStorageService = tableStorageService;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var products = await tableStorageService.GetAllProductsAsync();
            ViewBag.Products = products;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string productId, string productName)
        {
            var order = new Order
            {
                OrderId = Guid.NewGuid().ToString(),
                ProductId = productId,
                ProductName = productName,
                Action = "ProcessOrder",
                Timestamp = DateTime.UtcNow
            };

            await tableStorageService.SendOrderMessageAsync(order);
            await tableStorageService.LogMessageAsync($"Order placed: {order.OrderId} for product {productName}");

            TempData["Message"] = $"Order placed successfully for {productName}.";
            return RedirectToAction("Create");
        } 
         

    }
}