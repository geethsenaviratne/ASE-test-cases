using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using Ecommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Ecommerce.Tests.Services;

public class CartValidatorTests
{
    [Fact]
    public void Validate_WithNullCart_ShouldReturnInvalid()
    {
        // Act
        var result = CartValidator.Validate(null!);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("null");
    }

    [Fact]
    public void Validate_WithEmptyCart_ShouldReturnInvalid()
    {
        // Arrange
        var mockCatalog = new Mock<ICatalog>();
        var mockInventory = new Mock<IInventoryService>();
        var cart = new Cart(mockCatalog.Object, mockInventory.Object);

        // Act
        var result = CartValidator.Validate(cart);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("empty");
    }

    [Fact]
    public void Validate_WithValidCart_ShouldReturnValid()
    {
        // Arrange
        var mockCatalog = new Mock<ICatalog>();
        var mockInventory = new Mock<IInventoryService>();
        var product = new Product("SKU001", "Laptop", 1000m);
        
        mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(product);
        mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(10);

        var cart = new Cart(mockCatalog.Object, mockInventory.Object);
        cart.AddItem("SKU001", 1);

        // Act
        var result = CartValidator.Validate(cart);

        // Assert
        result.IsValid.Should().BeTrue();
        result.ErrorMessage.Should().BeNull();
    }
}