using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Interfaces;

public interface ICatalog
{
    void AddProduct(Product product);
    Product? GetProductBySku(string sku);
}