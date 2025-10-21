using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using Ecommerce.Domain.Services.DiscountRules;
using Ecommerce.Tests.Fakes;
using FluentAssertions;
using Xunit;

namespace Ecommerce.Tests.Services;

public class CheckoutIntegrationTests
{
    [Fact]
    public void FullCheckoutFlow_WithSuccessfulPayment_ShouldComplete()
    {
        // Arrange - Setup entire system
        var catalog = new Catalog();
        var laptop = new Product("SKU001", "Laptop", 1000m);
        var mouse = new Product("SKU002", "Mouse", 50m);
        catalog.AddProduct(laptop);
        catalog.AddProduct(mouse);

        var inventory = new InventoryService();
        inventory.InitializeStock("SKU001", 10);
        inventory.InitializeStock("SKU002", 50);

        var discountEngine = new DiscountEngine();
        discountEngine.AddRule(new BulkDiscountRule());
        discountEngine.AddRule(new OrderDiscountRule());

        var paymentGateway = new FakePaymentGateway(shouldSucceed: true);
        var checkoutService = new CheckoutService(paymentGateway);

        var cart = new Cart(catalog, inventory, discountEngine);

        // Act - Add items and checkout
        cart.AddItem("SKU001", 2);  // 2000
        cart.AddItem("SKU002", 5);  // 250
        // Subtotal: 2250
        // Order discount: 5% of 2250 = 112.5
        // Total: 2137.5

        var result = checkoutService.Checkout(cart, "valid_token");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.OrderId.Should().NotBeNullOrEmpty();
        result.OrderId.Should().StartWith("ORD-");
        result.PaymentResult.Should().NotBeNull();
        result.PaymentResult!.IsSuccess.Should().BeTrue();
        result.PaymentResult.TransactionId.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void FullCheckoutFlow_WithFailedPayment_ShouldNotCreateOrder()
    {
        // Arrange
        var catalog = new Catalog();
        var laptop = new Product("SKU001", "Laptop", 1000m);
        catalog.AddProduct(laptop);

        var inventory = new InventoryService();
        inventory.InitializeStock("SKU001", 10);

        var paymentGateway = new FakePaymentGateway(shouldSucceed: false);
        var checkoutService = new CheckoutService(paymentGateway);

        var cart = new Cart(catalog, inventory);
        cart.AddItem("SKU001", 1);

        // Act
        var result = checkoutService.Checkout(cart, "invalid_token");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.OrderId.Should().BeNull();
        result.ErrorMessage.Should().Contain("Payment failed");
        result.PaymentResult.Should().NotBeNull();
        result.PaymentResult!.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void FullCheckoutFlow_WithDiscounts_ShouldChargeCorrectAmount()
    {
        // Arrange
        var catalog = new Catalog();
        var laptop = new Product("SKU001", "Laptop", 100m);
        catalog.AddProduct(laptop);

        var inventory = new InventoryService();
        inventory.InitializeStock("SKU001", 20);

        var discountEngine = new DiscountEngine();
        discountEngine.AddRule(new BulkDiscountRule());
        discountEngine.AddRule(new OrderDiscountRule());

        var paymentGateway = new FakePaymentGateway();
        var checkoutService = new CheckoutService(paymentGateway);

        var cart = new Cart(catalog, inventory, discountEngine);
        cart.AddItem("SKU001", 15);  // 1500

        // Act
        var subtotal = cart.CalculateSubtotal();
        var discount = cart.CalculateDiscount();
        var total = cart.CalculateTotal();
        var result = checkoutService.Checkout(cart, "token123");

        // Assert
        subtotal.Should().Be(1500m);
        discount.Should().Be(225m); // Bulk: 150, Order: 75
        total.Should().Be(1275m);
        result.IsSuccess.Should().BeTrue();
    }
}