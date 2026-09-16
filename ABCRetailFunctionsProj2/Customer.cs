using Azure;
using Azure.Data.Tables;

namespace ABCRetailFunctionsProj2;

public class Customer : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
}