using Ecommerce.Domain.Models;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Domain.Services;

public class Cart
{
    private readonly ICatalog _catalog;
    private readonly IInventoryService? _inventoryService;
    private readonly IDiscountEngine? _discountEngine;
    private readonly Dictionary<string, LineItem> _items = new();

    // Constructor with all dependencies
    public Cart(ICatalog catalog, IInventoryService inventoryService, IDiscountEngine discountEngine)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _discountEngine = discountEngine;
    }

    // Constructor with inventory service only
    public Cart(ICatalog catalog, IInventoryService inventoryService)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
        _discountEngine = null;
    }

    // Constructor without inventory service (backward compatibility)
    public Cart(ICatalog catalog)
    {
        _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        _inventoryService = null;
        _discountEngine = null;
    }

    public void AddItem(string sku, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        var product = _catalog.GetProductBySku(sku);
        if (product == null)
            throw new InvalidOperationException($"Product with SKU '{sku}' not found in catalog");

        // Validate inventory availability
        ValidateInventoryAvailability(sku, quantity);

        if (_items.ContainsKey(sku))
        {
            _items[sku].AddQuantity(quantity);
        }
        else
        {
            _items[sku] = new LineItem(product, quantity);
        }
    }

    public void RemoveItem(string sku)
    {
        _items.Remove(sku);
    }

    public IReadOnlyCollection<LineItem> GetItems()
    {
        return _items.Values.ToList().AsReadOnly();
    }

    public decimal CalculateSubtotal()
    {
        return _items.Values.Sum(item => item.GetLineTotal());
    }

    public decimal CalculateDiscount()
    {
        if (_discountEngine == null)
            return 0m;
        var subtotal = CalculateSubtotal();
        return _discountEngine?.CalculateDiscount(GetItems(), subtotal) ?? 0m;
    }

    public decimal CalculateTotal()
    {
    var subtotal = CalculateSubtotal();
    var discount = CalculateDiscount();
    return subtotal - discount;
    }

    public IReadOnlyCollection<DiscountApplication> GetAppliedDiscounts()
    {
        if (_discountEngine == null)
            return new List<DiscountApplication>().AsReadOnly();

        var subtotal = CalculateSubtotal();
        return _discountEngine.GetAppliedDiscounts(GetItems(), subtotal);
    }

    public void Clear()
    {
        _items.Clear();
    }

    private void ValidateInventoryAvailability(string sku, int requestedQuantity)
    {
        if (_inventoryService == null)
            return;

        var availableQuantity = _inventoryService.GetAvailableQuantity(sku);
        var currentQuantity = _items.ContainsKey(sku) ? _items[sku].Quantity : 0;
        var totalRequested = currentQuantity + requestedQuantity;

        if (totalRequested > availableQuantity)
        {
            throw new InvalidOperationException(
                $"Insufficient inventory for SKU '{sku}'. " +
                $"Requested: {totalRequested}, Available: {availableQuantity}");
        }
    }
}