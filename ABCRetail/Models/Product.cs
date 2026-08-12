using Azure;
using Azure.Data.Tables;

namespace ABCRetail.Models
{
    public class Product : ITableEntity
    { 
        public string PartitionKey { get; set; } 
        public string RowKey { get; set; }   
        public string ImageUrl { get; set; }
        public string ProductName { get; set; } 
        public string ProductDescription { get; set; } 
        public double Price { get; set; } 
        public DateTimeOffset? Timestamp { get; set; } 
        public ETag ETag { get; set; }
    }
}
