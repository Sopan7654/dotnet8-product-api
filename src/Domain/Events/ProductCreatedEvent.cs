namespace Domain.Events;

/// <summary>
/// Domain event raised when a new product is created.
/// </summary>
public class ProductCreatedEvent
{
    public int ProductId { get; }
    public string ProductName { get; }
    public string CreatedBy { get; }
    public DateTime OccurredOn { get; }

    public ProductCreatedEvent(int productId, string productName, string createdBy)
    {
        ProductId = productId;
        ProductName = productName;
        CreatedBy = createdBy;
        OccurredOn = DateTime.UtcNow;
    }
}
