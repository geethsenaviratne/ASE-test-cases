namespace Ecommerce.Domain.Models;

public class Order
{
    public string OrderId { get; }
    public IReadOnlyCollection<OrderLineItem> LineItems { get; }
    public decimal Subtotal { get; }
    public decimal DiscountAmount { get; }
    public decimal Total { get; }
    public DateTime CreatedAt { get; }
    public string TransactionId { get; }
    public OrderStatus Status { get; }

    public Order(
        string orderId,
        IEnumerable<OrderLineItem> lineItems,
        decimal subtotal,
        decimal discountAmount,
        decimal total,
        string transactionId,
        OrderStatus status = OrderStatus.Completed)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Order ID cannot be empty", nameof(orderId));

        if (lineItems == null || !lineItems.Any())
            throw new ArgumentException("Order must have at least one line item", nameof(lineItems));

        if (subtotal < 0)
            throw new ArgumentException("Subtotal cannot be negative", nameof(subtotal));

        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        if (total < 0)
            throw new ArgumentException("Total cannot be negative", nameof(total));

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));

        OrderId = orderId;
        LineItems = lineItems.ToList().AsReadOnly();
        Subtotal = subtotal;
        DiscountAmount = discountAmount;
        Total = total;
        TransactionId = transactionId;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }
}