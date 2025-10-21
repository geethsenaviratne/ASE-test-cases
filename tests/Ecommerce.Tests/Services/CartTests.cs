using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using Ecommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Ecommerce.Tests.Services;

public class CartTests
{
    private readonly Mock<ICatalog> _mockCatalog;
    private readonly Product _laptop;
    private readonly Product _mouse;

    public CartTests()
    {
        _mockCatalog = new Mock<ICatalog>();
        _laptop = new Product("SKU001", "Laptop", 999.99m);
        _mouse = new Product("SKU002", "Mouse", 25.50m);
    }

    [Fact]
    public void AddItem_WithValidProduct_ShouldAddToCart()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        var cart = new Cart(_mockCatalog.Object);

        // Act
        cart.AddItem("SKU001", 2);

        // Assert
        cart.GetItems().Should().HaveCount(1);
        cart.GetItems().First().Sku.Should().Be("SKU001");
        cart.GetItems().First().Quantity.Should().Be(2);
    }

    [Fact]
    public void AddItem_WithNonExistentProduct_ShouldThrowException()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("INVALID")).Returns((Product?)null);
        var cart = new Cart(_mockCatalog.Object);

        // Act & Assert
        Action act = () => cart.AddItem("INVALID", 1);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*not found*catalog*");
    }

    [Fact]
    public void AddItem_WithZeroQuantity_ShouldThrowException()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        var cart = new Cart(_mockCatalog.Object);

        // Act & Assert
        Action act = () => cart.AddItem("SKU001", 0);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*quantity*greater than zero*");
    }

    [Fact]
    public void AddItem_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        var cart = new Cart(_mockCatalog.Object);

        // Act & Assert
        Action act = () => cart.AddItem("SKU001", -5);
        act.Should().Throw<ArgumentException>()
            .WithMessage("*quantity*greater than zero*");
    }

    [Fact]
    public void RemoveItem_WhenItemExists_ShouldRemoveFromCart()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        var cart = new Cart(_mockCatalog.Object);
        cart.AddItem("SKU001", 2);

        // Act
        cart.RemoveItem("SKU001");

        // Assert
        cart.GetItems().Should().BeEmpty();
    }

    [Fact]
    public void RemoveItem_WhenItemDoesNotExist_ShouldNotThrow()
    {
        // Arrange
        var cart = new Cart(_mockCatalog.Object);

        // Act & Assert
        Action act = () => cart.RemoveItem("NONEXISTENT");
        act.Should().NotThrow();
    }

    [Fact]
    public void CalculateTotal_WithMultipleItems_ShouldReturnCorrectSum()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockCatalog.Setup(c => c.GetProductBySku("SKU002")).Returns(_mouse);
        var cart = new Cart(_mockCatalog.Object);

        // Act
        cart.AddItem("SKU001", 2);  // 2 * 999.99 = 1999.98
        cart.AddItem("SKU002", 3);  // 3 * 25.50 = 76.50
        var total = cart.CalculateTotal();

        // Assert
        total.Should().Be(2076.48m);
    }

    [Fact]
    public void CalculateTotal_WithEmptyCart_ShouldReturnZero()
    {
        // Arrange
        var cart = new Cart(_mockCatalog.Object);

        // Act
        var total = cart.CalculateTotal();

        // Assert
        total.Should().Be(0m);
    }

    [Fact]
    public void AddItem_WithSameSkuTwice_ShouldUpdateQuantity()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        var cart = new Cart(_mockCatalog.Object);

        // Act
        cart.AddItem("SKU001", 2);
        cart.AddItem("SKU001", 3);

        // Assert
        cart.GetItems().Should().HaveCount(1);
        cart.GetItems().First().Quantity.Should().Be(5);
    }
}