using Ecommerce.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Tests.Services;

public class InventoryServiceTests
{
    [Fact]
    public void InitializeStock_ShouldSetAvailableQuantity()
    {
        // Arrange
        var inventory = new InventoryService();

        // Act
        inventory.InitializeStock("SKU001", 100);

        // Assert
        inventory.GetAvailableQuantity("SKU001").Should().Be(100);
    }

    [Fact]
    public void GetAvailableQuantity_ForUnknownSku_ShouldReturnZero()
    {
        // Arrange
        var inventory = new InventoryService();

        // Act
        var quantity = inventory.GetAvailableQuantity("UNKNOWN");

        // Assert
        quantity.Should().Be(0);
    }

    [Fact]
    public void ReserveStock_WithSufficientQuantity_ShouldReduceAvailable()
    {
        // Arrange
        var inventory = new InventoryService();
        inventory.InitializeStock("SKU001", 100);

        // Act
        inventory.ReserveStock("SKU001", 30);

        // Assert
        inventory.GetAvailableQuantity("SKU001").Should().Be(70);
    }

    [Fact]
    public void ReserveStock_WithInsufficientQuantity_ShouldThrowException()
    {
        // Arrange
        var inventory = new InventoryService();
        inventory.InitializeStock("SKU001", 10);

        // Act & Assert
        Action act = () => inventory.ReserveStock("SKU001", 20);
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot reserve*");
    }

    [Fact]
    public void ReleaseStock_ShouldIncreaseAvailableQuantity()
    {
        // Arrange
        var inventory = new InventoryService();
        inventory.InitializeStock("SKU001", 100);
        inventory.ReserveStock("SKU001", 30);

        // Act
        inventory.ReleaseStock("SKU001", 10);

        // Assert
        inventory.GetAvailableQuantity("SKU001").Should().Be(80);
    }

    [Fact]
    public void InitializeStock_WithNegativeQuantity_ShouldThrowException()
    {
        // Arrange
        var inventory = new InventoryService();

        // Act & Assert
        Action act = () => inventory.InitializeStock("SKU001", -10);
        act.Should().Throw<ArgumentException>();
    }
}