using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using Ecommerce.Domain.Services.DiscountRules;
using Ecommerce.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Tests.Integration;

public class CompleteEcommerceFlowTests
{
    [Fact]
    public void CompleteFlow_FromProductToOrder_ShouldSucceed()
    {
        // Arrange - Build the entire system
        var catalog = new Catalog();
        var inventory = new InventoryService();
        var discountEngine = new DiscountEngine();
        var paymentGateway = new FakePaymentGateway(shouldSucceed: true);
        var orderRepository = new InMemoryOrderRepository();
        var checkoutService = new CheckoutService(paymentGateway, orderRepository);

        // Setup products
        var laptop = new Product("LAPTOP-001", "Gaming Laptop", 1500m);
        var mouse = new Product("MOUSE-001", "Wireless Mouse", 50m);
        var keyboard = new Product("KEYBOARD-001", "Mechanical Keyboard", 150m);

        catalog.AddProduct(laptop);
        catalog.AddProduct(mouse);
        catalog.AddProduct(keyboard);

        // Setup inventory
        inventory.InitializeStock("LAPTOP-001", 10);
        inventory.InitializeStock("MOUSE-001", 50);
        inventory.InitializeStock("KEYBOARD-001", 30);

        // Setup discount rules
        discountEngine.AddRule(new BulkDiscountRule());
        discountEngine.AddRule(new OrderDiscountRule());

        // Create cart
        var cart = new Cart(catalog, inventory, discountEngine);

        // Act - Customer shopping flow
        cart.AddItem("LAPTOP-001", 2);      // 2 * 1500 = 3000
        cart.AddItem("MOUSE-001", 12);      // 12 * 50 = 600 (bulk discount applies)
        cart.AddItem("KEYBOARD-001", 3);    // 3 * 150 = 450

        var subtotal = cart.CalculateSubtotal();
        var discount = cart.CalculateDiscount();
        var total = cart.CalculateTotal();
        var appliedDiscounts = cart.GetAppliedDiscounts();

        // Checkout
        var checkoutResult = checkoutService.Checkout(cart, "valid_payment_token");

        // Assert - Verify calculations
        subtotal.Should().Be(4050m);
        // Bulk discount on mouse: 10% of 600 = 60
        // Order discount: 5% of 4050 = 202.5
        // Total discount = 262.5
        discount.Should().Be(262.5m);
        total.Should().Be(3787.5m);

        appliedDiscounts.Should().HaveCount(2);
        appliedDiscounts.Should().Contain(d => d.RuleName == "Bulk Discount" && d.DiscountAmount == 60m);
        appliedDiscounts.Should().Contain(d => d.RuleName == "Order Discount" && d.DiscountAmount == 202.5m);

        // Assert - Verify checkout success
        checkoutResult.IsSuccess.Should().BeTrue();
        checkoutResult.OrderId.Should().NotBeNullOrEmpty();
        checkoutResult.PaymentResult.Should().NotBeNull();
        checkoutResult.PaymentResult!.IsSuccess.Should().BeTrue();

        // Assert - Verify order was saved
        var savedOrder = orderRepository.GetById(checkoutResult.OrderId!);
        savedOrder.Should().NotBeNull();
        savedOrder!.OrderId.Should().Be(checkoutResult.OrderId);
        savedOrder.LineItems.Should().HaveCount(3);
        savedOrder.Subtotal.Should().Be(4050m);
        savedOrder.DiscountAmount.Should().Be(262.5m);
        savedOrder.Total.Should().Be(3787.5m);
        savedOrder.Status.Should().Be(OrderStatus.Completed);
        savedOrder.TransactionId.Should().NotBeNullOrEmpty();

        // Verify line items
        var laptopLine = savedOrder.LineItems.First(li => li.Sku == "LAPTOP-001");
        laptopLine.Quantity.Should().Be(2);
        laptopLine.UnitPrice.Should().Be(1500m);
        laptopLine.LineTotal.Should().Be(3000m);

        var mouseLine = savedOrder.LineItems.First(li => li.Sku == "MOUSE-001");
        mouseLine.Quantity.Should().Be(12);
        mouseLine.UnitPrice.Should().Be(50m);
        mouseLine.LineTotal.Should().Be(600m);
    }

    [Fact]
    public void CompleteFlow_WithPaymentFailure_ShouldNotCreateOrder()
    {
        // Arrange
        var catalog = new Catalog();
        var inventory = new InventoryService();
        var paymentGateway = new FakePaymentGateway(shouldSucceed: false);
        var orderRepository = new InMemoryOrderRepository();
        var checkoutService = new CheckoutService(paymentGateway, orderRepository);

        var product = new Product("SKU001", "Product", 100m);
        catalog.AddProduct(product);
        inventory.InitializeStock("SKU001", 10);

        var cart = new Cart(catalog, inventory);
        cart.AddItem("SKU001", 1);

        // Act
        var checkoutResult = checkoutService.Checkout(cart, "invalid_token");

        // Assert
        checkoutResult.IsSuccess.Should().BeFalse();
        checkoutResult.OrderId.Should().BeNull();
        orderRepository.GetAll().Should().BeEmpty();
    }

    [Fact]
    public void CompleteFlow_WithInsufficientInventory_ShouldPreventCheckout()
    {
        // Arrange
        var catalog = new Catalog();
        var inventory = new InventoryService();
        var product = new Product("SKU001", "Product", 100m);
        
        catalog.AddProduct(product);
        inventory.InitializeStock("SKU001", 5); // Only 5 available

        var cart = new Cart(catalog, inventory);

        // Act & Assert
        Action act = () => cart.AddItem("SKU001", 10); // Trying to add 10
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient inventory*");
    }
}