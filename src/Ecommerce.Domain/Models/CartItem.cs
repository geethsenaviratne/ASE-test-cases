namespace Ecommerce.Domain.Models;

public class CartItem
{
    public string Sku { get; }
    public string Name { get; }
    public decimal Price { get; }
    public int Quantity { get; private set; }

    public CartItem(Product product, int quantity)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));
        
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));

        Sku = product.Sku;
        Name = product.Name;
        Price = product.Price;
        Quantity = quantity;
    }

    public void AddQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero", nameof(quantity));
        
        Quantity += quantity;
    }

    public decimal GetLineTotal()
    {
        return Price * Quantity;
    }
}