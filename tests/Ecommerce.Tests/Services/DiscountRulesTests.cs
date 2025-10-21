using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services.DiscountRules;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Tests.Services;

public class DiscountRulesTests
{
    [Fact]
    public void BulkDiscount_WithQuantityLessThan10_ShouldReturnZero()
    {
        // Arrange
        var rule = new BulkDiscountRule();
        var product = new Product("SKU001", "Laptop", 1000m);
        var items = new List<LineItem>
        {
            new LineItem(product, 5)
        };

        // Act
        var discount = rule.CalculateDiscount(items, 5000m);

        // Assert
        discount.Should().Be(0m);
    }

    [Fact]
    public void BulkDiscount_WithQuantityEqualTo10_ShouldApply10PercentDiscount()
    {
        // Arrange
        var rule = new BulkDiscountRule();
        var product = new Product("SKU001", "Laptop", 1000m);
        var items = new List<LineItem>
        {
            new LineItem(product, 10)
        };

        // Act
        var discount = rule.CalculateDiscount(items, 10000m);

        // Assert
        // 10% discount on 10 * 1000 = 1000
        discount.Should().Be(1000m);
    }

    [Fact]
    public void BulkDiscount_WithQuantityGreaterThan10_ShouldApply10PercentDiscount()
    {
        // Arrange
        var rule = new BulkDiscountRule();
        var product = new Product("SKU001", "Laptop", 1000m);
        var items = new List<LineItem>
        {
            new LineItem(product, 15)
        };

        // Act
        var discount = rule.CalculateDiscount(items, 15000m);

        // Assert
        // 10% discount on 15 * 1000 = 1500
        discount.Should().Be(1500m);
    }

    [Fact]
    public void BulkDiscount_WithMultipleItems_ShouldApplyToEligibleItemsOnly()
    {
        // Arrange
        var rule = new BulkDiscountRule();
        var laptop = new Product("SKU001", "Laptop", 1000m);
        var mouse = new Product("SKU002", "Mouse", 50m);
        var items = new List<LineItem>
        {
            new LineItem(laptop, 12),  // Eligible: 12 * 1000 = 12000, discount = 1200
            new LineItem(mouse, 5)     // Not eligible: only 5 items
        };

        // Act
        var discount = rule.CalculateDiscount(items, 12250m);

        // Assert
        // Only laptop line gets discount: 10% of 12000 = 1200
        discount.Should().Be(1200m);
    }

    [Fact]
    public void OrderDiscount_WithTotalLessThan1000_ShouldReturnZero()
    {
        // Arrange
        var rule = new OrderDiscountRule();
        var product = new Product("SKU001", "Mouse", 50m);
        var items = new List<LineItem>
        {
            new LineItem(product, 10)
        };

        // Act
        var discount = rule.CalculateDiscount(items, 500m);

        // Assert
        discount.Should().Be(0m);
    }

    [Fact]
    public void OrderDiscount_WithTotalEqualTo1000_ShouldApply5PercentDiscount()
    {
        // Arrange
        var rule = new OrderDiscountRule();
        var product = new Product("SKU001", "Laptop", 1000m);
        var items = new List<LineItem>
        {
            new LineItem(product, 1)
        };

        // Act
        var discount = rule.CalculateDiscount(items, 1000m);

        // Assert
        // 5% of 1000 = 50
        discount.Should().Be(50m);
    }

    [Fact]
    public void OrderDiscount_WithTotalGreaterThan1000_ShouldApply5PercentDiscount()
    {
        // Arrange
        var rule = new OrderDiscountRule();
        var product = new Product("SKU001", "Laptop", 1500m);
        var items = new List<LineItem>
        {
            new LineItem(product, 2)
        };

        // Act
        var discount = rule.CalculateDiscount(items, 3000m);

        // Assert
        // 5% of 3000 = 150
        discount.Should().Be(150m);
    }
}