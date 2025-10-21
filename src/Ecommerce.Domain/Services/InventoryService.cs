using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Domain.Services;

public class InventoryService : IInventoryService
{
    private readonly Dictionary<string, int> _stock = new();

    public void InitializeStock(string sku, int quantity)
    {
        if (quantity < 0)
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
        
        _stock[sku] = quantity;
    }

    public int GetAvailableQuantity(string sku)
    {
        return _stock.TryGetValue(sku, out var quantity) ? quantity : 0;
    }

    public void ReserveStock(string sku, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        var available = GetAvailableQuantity(sku);
        if (quantity > available)
            throw new InvalidOperationException(
                $"Cannot reserve {quantity} units of SKU '{sku}'. Only {available} available.");

        _stock[sku] = available - quantity;
    }

    public void ReleaseStock(string sku, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        var current = GetAvailableQuantity(sku);
        _stock[sku] = current + quantity;
    }
}