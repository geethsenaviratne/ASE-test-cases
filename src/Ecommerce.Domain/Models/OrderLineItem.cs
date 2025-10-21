namespace Ecommerce.Domain.Models;

public class OrderLineItem
{
    public string Sku { get; }
    public string ProductName { get; }
    public decimal UnitPrice { get; }
    public int Quantity { get; }
    public decimal LineTotal { get; }

    public OrderLineItem(string sku, string productName, decimal unitPrice, int quantity)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be empty", nameof(productName));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative", nameof(unitPrice));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        Sku = sku;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        LineTotal = unitPrice * quantity;
    }

    public static OrderLineItem FromLineItem(LineItem lineItem)
    {
        if (lineItem == null)
            throw new ArgumentNullException(nameof(lineItem));

        return new OrderLineItem(
            lineItem.Sku,
            lineItem.Name,
            lineItem.Price,
            lineItem.Quantity);
    }
}