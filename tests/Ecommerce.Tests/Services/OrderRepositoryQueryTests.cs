using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Tests.Services;

public class OrderRepositoryQueryTests
{
    [Fact]
    public void GetByDateRange_ShouldReturnOrdersInRange()
    {
        // Arrange
        var repository = new InMemoryOrderRepository();
        
        // We can't easily test with actual dates in the past, so this is a basic structure test
        var order = CreateTestOrder("ORD-001");
        repository.Save(order);

        // Act
        var startDate = DateTime.UtcNow.AddDays(-1);
        var endDate = DateTime.UtcNow.AddDays(1);
        var ordersInRange = repository.GetByDateRange(startDate, endDate);

        // Assert
        ordersInRange.Should().HaveCount(1);
        ordersInRange.First().OrderId.Should().Be("ORD-001");
    }

    [Fact]
    public void GetByDateRange_WithNoOrdersInRange_ShouldReturnEmpty()
    {
        // Arrange
        var repository = new InMemoryOrderRepository();

        // Act
        var startDate = DateTime.UtcNow.AddDays(-10);
        var endDate = DateTime.UtcNow.AddDays(-5);
        var ordersInRange = repository.GetByDateRange(startDate, endDate);

        // Assert
        ordersInRange.Should().BeEmpty();
    }

    private static Order CreateTestOrder(string orderId)
    {
        var lineItems = new List<OrderLineItem>
        {
            new OrderLineItem("SKU001", "Test Product", 100m, 1)
        };

        return new Order(orderId, lineItems, 100m, 0m, 100m, "txn_test");
    }
}