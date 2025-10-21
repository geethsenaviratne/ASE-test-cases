using Ecommerce.Domain.Models;
using FluentAssertions;
using FluentAssertions.Extensions;
using Xunit;

namespace Ecommerce.Tests.Models;

public class ProductTests
{
    [Fact]
    public void CreateProduct_WithValidData_ShouldSucceed()
    {
        // Arrange & Act
        var product = new Product("SKU001", "Laptop", 999.99m);
        
        // Assert
        product.Sku.Should().Be("SKU001");
        product.Name.Should().Be("Laptop");
        product.Price.Should().Be(999.99m);
    }

    [Fact]
    public void CreateProduct_WithNegativePrice_ShouldThrowException()
    {
        // Arrange, Act & Assert
        Action act = () => new Product("SKU001", "Laptop", -100m);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*price*negative*");
    }

    [Fact]
    public void CreateProduct_WithEmptySku_ShouldThrowException()
    {
        // Arrange, Act & Assert
        Action act = () => new Product("", "Laptop", 999.99m);
        act.Should().Throw<ArgumentException>();
    }
}