using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using Ecommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Ecommerce.Tests.Services;

public class CartWithInventoryTests
{
    private readonly Mock<ICatalog> _mockCatalog;
    private readonly Mock<IInventoryService> _mockInventory;
    private readonly Product _laptop;
    private readonly Product _mouse;

    public CartWithInventoryTests()
    {
        _mockCatalog = new Mock<ICatalog>();
        _mockInventory = new Mock<IInventoryService>();
        
        _laptop = new Product("SKU001", "Laptop", 999.99m);
        _mouse = new Product("SKU002", "Mouse", 25.50m);
    }

    [Fact]
    public void AddItem_WithSufficientInventory_ShouldAddToCart()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(10);
        
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);

        // Act
        cart.AddItem("SKU001", 5);

        // Assert
        cart.GetItems().Should().HaveCount(1);
        cart.GetItems().First().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_WithInsufficientInventory_ShouldThrowException()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(3);
        
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);

        // Act & Assert
        Action act = () => cart.AddItem("SKU001", 5);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*insufficient inventory*");
    }

    [Fact]
    public void AddItem_WithZeroInventory_ShouldThrowException()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(0);
        
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);

        // Act & Assert
        Action act = () => cart.AddItem("SKU001", 1);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*insufficient inventory*");
    }

    [Fact]
    public void AddItem_ShouldCheckInventoryForExactQuantity()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(5);
        
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);

        // Act
        cart.AddItem("SKU001", 5);

        // Assert - should succeed with exact quantity
        cart.GetItems().First().Quantity.Should().Be(5);
    }

    [Fact]
    public void AddItem_WhenAddingToExistingItem_ShouldCheckTotalQuantity()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(10);
        
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);
        cart.AddItem("SKU001", 3);

        // Act & Assert - trying to add 8 more when only 10 total available should fail
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(7); // Only 7 left after first add
        Action act = () => cart.AddItem("SKU001", 8);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*insufficient inventory*");
    }

    [Fact]
    public void AddItem_WithMultipleProducts_ShouldCheckEachInventorySeparately()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockCatalog.Setup(c => c.GetProductBySku("SKU002")).Returns(_mouse);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(5);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU002")).Returns(20);
        
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);

        // Act
        cart.AddItem("SKU001", 5);
        cart.AddItem("SKU002", 10);

        // Assert
        cart.GetItems().Should().HaveCount(2);
        _mockInventory.Verify(i => i.GetAvailableQuantity("SKU001"), Times.Once);
        _mockInventory.Verify(i => i.GetAvailableQuantity("SKU002"), Times.Once);
    }
}