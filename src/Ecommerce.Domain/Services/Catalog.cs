using Ecommerce.Domain.Models;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Domain.Services;

public class Catalog : ICatalog
{
    private readonly Dictionary<string, Product> _products = new();

    public void AddProduct(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));
        
        _products[product.Sku] = product;
    }

    public Product? GetProductBySku(string sku)
    {
        return _products.TryGetValue(sku, out var product) ? product : null;
    }
}