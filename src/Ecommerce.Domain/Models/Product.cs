namespace Ecommerce.Domain.Models;

public class Product
{
    public string Sku { get; }
    public string Name { get; }
    public decimal Price { get; }

    public Product(string sku, string name, decimal price)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));
        
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));
        
        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        Sku = sku;
        Name = name;
        Price = price;
    }
}