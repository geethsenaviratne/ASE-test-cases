using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using Ecommerce.Domain.Services.DiscountRules;
using Ecommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;

namespace Ecommerce.Tests.Services;

public class CartWithDiscountsTests
{
    [Fact]
    public void Cart_WithDiscountEngine_ShouldApplyDiscounts()
    {
        // Arrange
        var mockCatalog = new Mock<ICatalog>();
        var mockInventory = new Mock<IInventoryService>();
        var discountEngine = new DiscountEngine();
        discountEngine.AddRule(new BulkDiscountRule());
        discountEngine.AddRule(new OrderDiscountRule());

        var laptop = new Product("SKU001", "Laptop", 100m);
        mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(laptop);
        mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(20);

        var cart = new Cart(mockCatalog.Object, mockInventory.Object, discountEngine);

        // Act
        cart.AddItem("SKU001", 15);  // 15 * 100 = 1500

        // Assert
        var subtotal = cart.CalculateSubtotal();
        var discount = cart.CalculateDiscount();
        var total = cart.CalculateTotal();

        subtotal.Should().Be(1500m);
        // Bulk: 10% of 1500 = 150, Order: 5% of 1500 = 75, Total discount = 225
        discount.Should().Be(225m);
        total.Should().Be(1275m);
    }

    [Fact]
    public void Cart_WithoutDiscountEngine_ShouldNotApplyDiscounts()
    {
        // Arrange
        var mockCatalog = new Mock<ICatalog>();
        var mockInventory = new Mock<IInventoryService>();

        var laptop = new Product("SKU001", "Laptop", 100m);
        mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(laptop);
        mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(20);

        var cart = new Cart(mockCatalog.Object, mockInventory.Object);

        // Act
        cart.AddItem("SKU001", 15);

        // Assert
        var discount = cart.CalculateDiscount();
        discount.Should().Be(0m);
        cart.CalculateTotal().Should().Be(1500m);
    }

    [Fact]
    public void GetAppliedDiscounts_ShouldReturnAllActiveDiscounts()
    {
        // Arrange
        var mockCatalog = new Mock<ICatalog>();
        var mockInventory = new Mock<IInventoryService>();
        var discountEngine = new DiscountEngine();
        discountEngine.AddRule(new BulkDiscountRule());
        discountEngine.AddRule(new OrderDiscountRule());

        var laptop = new Product("SKU001", "Laptop", 100m);
        mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(laptop);
        mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(20);

        var cart = new Cart(mockCatalog.Object, mockInventory.Object, discountEngine);
        cart.AddItem("SKU001", 12);

        // Act
        var appliedDiscounts = cart.GetAppliedDiscounts();

        // Assert
        appliedDiscounts.Should().HaveCount(2);
        appliedDiscounts.Should().Contain(d => d.RuleName == "Bulk Discount");
        appliedDiscounts.Should().Contain(d => d.RuleName == "Order Discount");
    }
}