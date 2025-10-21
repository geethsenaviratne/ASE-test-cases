using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using Ecommerce.Domain.Interfaces;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Tests.Services;

public class CatalogTests
{
    [Fact]
    public void AddProduct_ShouldAllowRetrievalBySku()
    {
        // Arrange
        ICatalog catalog = new Catalog();
        var product = new Product("SKU001", "Laptop", 999.99m);
        
        // Act
        catalog.AddProduct(product);
        var retrieved = catalog.GetProductBySku("SKU001");
        
        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.Sku.Should().Be("SKU001");
    }

    [Fact]
    public void GetProductBySku_WhenNotFound_ShouldReturnNull()
    {
        // Arrange
        ICatalog catalog = new Catalog();
        
        // Act
        var result = catalog.GetProductBySku("NONEXISTENT");
        
        // Assert
        result.Should().BeNull();
    }
}