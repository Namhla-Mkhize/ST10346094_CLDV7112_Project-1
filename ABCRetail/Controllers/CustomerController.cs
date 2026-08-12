using ABCRetail.Models;
using ABCRetail.Services;
using Microsoft.AspNetCore.Mvc;

namespace ABCRetail.Controllers
{
    public class CustomerController : Controller
    { 
         private readonly TableStorageService tableStorageService;   
         
        public CustomerController(TableStorageService tableStorageService)
        {
          this.tableStorageService = tableStorageService;
           
        }

        public async Task<IActionResult> Index()
        {
            var customers = await tableStorageService.GetAllCustomersAsync(); 
            return View(customers);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Customer customer)
        {
            customer.PartitionKey = "Customer";
            customer.RowKey = Guid.NewGuid().ToString();

            await tableStorageService.AddCustomerAsync(customer);
            await tableStorageService.SendMessageAsync($"Customer created: {customer.Name}"); 
            await tableStorageService.LogMessageAsync($"Customer created: {customer.Name}");
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string partitionKey, string rowKey)
        {
            await tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
            await tableStorageService.SendMessageAsync($"Customer deleted: {rowKey}");
            await tableStorageService.LogMessageAsync($"Customer deleted: {rowKey}");
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string partitionKey, string rowKey)
        {
            var customers = await tableStorageService.GetAllCustomersAsync();
            var customer = customers.FirstOrDefault(c => c.PartitionKey == partitionKey && c.RowKey == rowKey);
            return View(customer);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Customer customer)
        {
            await tableStorageService.UpdateCustomerAsync(customer);
            await tableStorageService.SendMessageAsync($"Customer updated: {customer.Name}");
            await tableStorageService.LogMessageAsync($"Customer updated: {customer.Name}");
            TempData["Message"] = $"Customer '{customer.Name}' updated successfully.";
            return RedirectToAction("Index");
        }
    }
}
