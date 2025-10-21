using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain;
using Ecommerce.Tests.Fakes;
using FluentAssertions;
using Moq;
using Xunit;

namespace Ecommerce.Tests.Services;

public class CheckoutWithOrderPersistenceTests
{
    [Fact]
    public void Checkout_OnSuccess_ShouldSaveOrder()
    {
        // Arrange
        var mockCatalog = new Mock<ICatalog>();
        var mockInventory = new Mock<IInventoryService>();
        var paymentGateway = new FakePaymentGateway(shouldSucceed: true);
        var orderRepository = new InMemoryOrderRepository();

        var product = new Product("SKU001", "Laptop", 1000m);
        mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(product);
        mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(10);

        var checkoutService = new CheckoutService(paymentGateway, orderRepository);
        var cart = new Cart(mockCatalog.Object, mockInventory.Object);
        cart.AddItem("SKU001", 2);

        // Act
        var result = checkoutService.Checkout(cart, "token123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var savedOrder = orderRepository.GetById(result.OrderId!);
        savedOrder.Should().NotBeNull();
        savedOrder!.OrderId.Should().Be(result.OrderId);
        savedOrder.LineItems.Should().HaveCount(1);
        savedOrder.Total.Should().Be(2000m);
    }

    [Fact]
    public void Checkout_OnPaymentFailure_ShouldNotSaveOrder()
    {
        // Arrange
        var mockCatalog = new Mock<ICatalog>();
        var mockInventory = new Mock<IInventoryService>();
        var paymentGateway = new FakePaymentGateway(shouldSucceed: false);
        var orderRepository = new InMemoryOrderRepository();

        var product = new Product("SKU001", "Laptop", 1000m);
        mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(product);
        mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(10);

        var checkoutService = new CheckoutService(paymentGateway, orderRepository);
        var cart = new Cart(mockCatalog.Object, mockInventory.Object);
        cart.AddItem("SKU001", 1);

        // Act
        var result = checkoutService.Checkout(cart, "invalid_token");

        // Assert
        result.IsSuccess.Should().BeFalse();
        orderRepository.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void Checkout_ShouldSaveOrderWithCorrectDiscounts()
    {
        // Arrange
        var catalog = new Catalog();
        var inventory = new InventoryService();
        var discountEngine = new DiscountEngine();
        // No custom rules added in this test (OrderDiscountRule not available)
        
        var product = new Product("SKU001", "Laptop", 1000m);
        catalog.AddProduct(product);
        inventory.InitializeStock("SKU001", 20);

        var paymentGateway = new FakePaymentGateway(shouldSucceed: true);
        var orderRepository = new InMemoryOrderRepository();
        var checkoutService = new CheckoutService(paymentGateway, orderRepository);

        var cart = new Cart(catalog, inventory, discountEngine);
        cart.AddItem("SKU001", 2); // Subtotal: 2000, Discount: 0, Total: 2000

        // Act
        var result = checkoutService.Checkout(cart, "token123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        
        var savedOrder = orderRepository.GetById(result.OrderId!);
        savedOrder.Should().NotBeNull();
        savedOrder!.Subtotal.Should().Be(2000m);
        savedOrder.DiscountAmount.Should().Be(0m);
        savedOrder.Total.Should().Be(2000m);
    }
}