using Ecommerce.Domain.Models;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Tests.Models;

public class OrderTests
{
    [Fact]
    public void CreateOrder_WithValidData_ShouldSucceed()
    {
        // Arrange
        var lineItems = new List<OrderLineItem>
        {
            new OrderLineItem("SKU001", "Laptop", 1000m, 2)
        };

        // Act
        var order = new Order(
            "ORD-12345",
            lineItems,
            subtotal: 2000m,
            discountAmount: 100m,
            total: 1900m,
            transactionId: "txn_123");

        // Assert
        order.OrderId.Should().Be("ORD-12345");
        order.LineItems.Should().HaveCount(1);
        order.Subtotal.Should().Be(2000m);
        order.DiscountAmount.Should().Be(100m);
        order.Total.Should().Be(1900m);
        order.TransactionId.Should().Be("txn_123");
        order.Status.Should().Be(OrderStatus.Completed);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CreateOrder_WithEmptyOrderId_ShouldThrowException()
    {
        // Arrange
        var lineItems = new List<OrderLineItem>
        {
            new OrderLineItem("SKU001", "Laptop", 1000m, 1)
        };

        // Act & Assert
        Action act = () => new Order("", lineItems, 1000m, 0m, 1000m, "txn_123");
        act.Should().Throw<ArgumentException>().WithMessage("*Order ID*");
    }

    [Fact]
    public void CreateOrder_WithNoLineItems_ShouldThrowException()
    {
        // Arrange
        var lineItems = new List<OrderLineItem>();

        // Act & Assert
        Action act = () => new Order("ORD-123", lineItems, 0m, 0m, 0m, "txn_123");
        act.Should().Throw<ArgumentException>().WithMessage("*line item*");
    }

    [Fact]
    public void OrderLineItem_FromLineItem_ShouldConvertCorrectly()
    {
        // Arrange
        var product = new Product("SKU001", "Laptop", 1000m);
        var lineItem = new LineItem(product, 2);

        // Act
        var orderLineItem = OrderLineItem.FromLineItem(lineItem);

        // Assert
        orderLineItem.Sku.Should().Be("SKU001");
        orderLineItem.ProductName.Should().Be("Laptop");
        orderLineItem.UnitPrice.Should().Be(1000m);
        orderLineItem.Quantity.Should().Be(2);
        orderLineItem.LineTotal.Should().Be(2000m);
    }
}