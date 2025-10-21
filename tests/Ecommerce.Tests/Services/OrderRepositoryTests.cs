using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Tests.Services;

public class OrderRepositoryTests
{
    [Fact]
    public void Save_ShouldStoreOrder()
    {
        // Arrange
        var repository = new InMemoryOrderRepository();
        var order = CreateTestOrder("ORD-001");

        // Act
        repository.Save(order);
        var retrieved = repository.GetById("ORD-001");

        // Assert
        retrieved.Should().NotBeNull();
        retrieved!.OrderId.Should().Be("ORD-001");
    }

    [Fact]
    public void GetById_WhenOrderDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var repository = new InMemoryOrderRepository();

        // Act
        var result = repository.GetById("NONEXISTENT");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetAll_ShouldReturnAllOrders()
    {
        // Arrange
        var repository = new InMemoryOrderRepository();
        var order1 = CreateTestOrder("ORD-001");
        var order2 = CreateTestOrder("ORD-002");

        repository.Save(order1);
        repository.Save(order2);

        // Act
        var allOrders = repository.GetAll();

        // Assert
        allOrders.Should().HaveCount(2);
        allOrders.Should().Contain(o => o.OrderId == "ORD-001");
        allOrders.Should().Contain(o => o.OrderId == "ORD-002");
    }

    [Fact]
    public void GetAll_WhenNoOrders_ShouldReturnEmptyCollection()
    {
        // Arrange
        var repository = new InMemoryOrderRepository();

        // Act
        var allOrders = repository.GetAll();

        // Assert
        allOrders.Should().BeEmpty();
    }

    [Fact]
    public void Save_WithDuplicateOrderId_ShouldUpdateExisting()
    {
        // Arrange
        var repository = new InMemoryOrderRepository();
        var order1 = CreateTestOrder("ORD-001", total: 1000m);
        var order2 = CreateTestOrder("ORD-001", total: 2000m);

        // Act
        repository.Save(order1);
        repository.Save(order2);

        // Assert
        var allOrders = repository.GetAll();
        allOrders.Should().HaveCount(1);
        allOrders.First().Total.Should().Be(2000m);
    }

    private static Order CreateTestOrder(string orderId, decimal total = 1000m)
    {
        var lineItems = new List<OrderLineItem>
        {
            new OrderLineItem("SKU001", "Test Product", total, 1)
        };

        return new Order(orderId, lineItems, total, 0m, total, "txn_test");
    }
}