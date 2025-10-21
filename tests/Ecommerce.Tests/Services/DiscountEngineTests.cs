using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using Ecommerce.Domain.Services.DiscountRules;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Tests.Services;

public class DiscountEngineTests
{
    [Fact]
    public void CalculateDiscount_WithNoRules_ShouldReturnZero()
    {
        // Arrange
        var engine = new DiscountEngine();
        var product = new Product("SKU001", "Laptop", 1000m);
        var items = new List<LineItem>
        {
            new LineItem(product, 1)
        };

        // Act
        var discount = engine.CalculateDiscount(items, 1000m);

        // Assert
        discount.Should().Be(0m);
    }

    [Fact]
    public void CalculateDiscount_WithSingleRule_ShouldApplyThatRule()
    {
        // Arrange
        var engine = new DiscountEngine();
        engine.AddRule(new OrderDiscountRule());
        
        var product = new Product("SKU001", "Laptop", 1000m);
        var items = new List<LineItem>
        {
            new LineItem(product, 2)
        };

        // Act
        var discount = engine.CalculateDiscount(items, 2000m);

        // Assert
        // 5% of 2000 = 100
        discount.Should().Be(100m);
    }

    [Fact]
    public void CalculateDiscount_WithMultipleRules_ShouldSumAllDiscounts()
    {
        // Arrange
        var engine = new DiscountEngine();
        engine.AddRule(new BulkDiscountRule());
        engine.AddRule(new OrderDiscountRule());
        
        var product = new Product("SKU001", "Laptop", 100m);
        var items = new List<LineItem>
        {
            new LineItem(product, 15)  // Subtotal: 1500
        };

        // Act
        var discount = engine.CalculateDiscount(items, 1500m);

        // Assert
        // Bulk discount: 10% of 1500 = 150
        // Order discount: 5% of 1500 = 75
        // Total: 225
        discount.Should().Be(225m);
    }

    [Fact]
    public void GetAppliedDiscounts_ShouldReturnAllApplicableDiscounts()
    {
        // Arrange
        var engine = new DiscountEngine();
        engine.AddRule(new BulkDiscountRule());
        engine.AddRule(new OrderDiscountRule());
        
        var product = new Product("SKU001", "Laptop", 100m);
        var items = new List<LineItem>
        {
            new LineItem(product, 12)
        };

        // Act
        var appliedDiscounts = engine.GetAppliedDiscounts(items, 1200m);

        // Assert
        appliedDiscounts.Should().HaveCount(2);
        appliedDiscounts.Should().Contain(d => d.RuleName == "Bulk Discount" && d.DiscountAmount == 120m);
        appliedDiscounts.Should().Contain(d => d.RuleName == "Order Discount" && d.DiscountAmount == 60m);
    }

    [Fact]
    public void GetAppliedDiscounts_WithNoEligibleDiscounts_ShouldReturnEmptyList()
    {
        // Arrange
        var engine = new DiscountEngine();
        engine.AddRule(new BulkDiscountRule());
        engine.AddRule(new OrderDiscountRule());
        
        var product = new Product("SKU001", "Mouse", 50m);
        var items = new List<LineItem>
        {
            new LineItem(product, 5)  // Subtotal: 250 - not eligible for any discount
        };

        // Act
        var appliedDiscounts = engine.GetAppliedDiscounts(items, 250m);

        // Assert
        appliedDiscounts.Should().BeEmpty();
    }
}