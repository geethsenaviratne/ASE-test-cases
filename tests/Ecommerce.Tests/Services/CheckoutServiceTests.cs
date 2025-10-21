using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;
using Ecommerce.Domain.Interfaces;
using FluentAssertions;
using Moq;
using Xunit;
using Ecommerce.Domain.Services.DiscountRules;

namespace Ecommerce.Tests.Services;

public class CheckoutServiceTests
{
    private readonly Mock<ICatalog> _mockCatalog;
    private readonly Mock<IInventoryService> _mockInventory;
    private readonly Mock<IPaymentGateway> _mockPaymentGateway;
    private readonly Product _laptop;

    public CheckoutServiceTests()
    {
        _mockCatalog = new Mock<ICatalog>();
        _mockInventory = new Mock<IInventoryService>();
        _mockPaymentGateway = new Mock<IPaymentGateway>();
        _laptop = new Product("SKU001", "Laptop", 999.99m);
    }

    [Fact]
    public void Checkout_WithEmptyCart_ShouldFail()
    {
        // Arrange
        var checkoutService = new CheckoutService(_mockPaymentGateway.Object);
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);

        // Act
        var result = checkoutService.Checkout(cart, "token123");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("empty");
    }

    [Fact]
    public void Checkout_WithValidCart_ShouldProcessPayment()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(10);
        _mockPaymentGateway
            .Setup(p => p.Charge(It.IsAny<decimal>(), "token123"))
            .Returns(PaymentResult.Success("txn_123456"));

        var checkoutService = new CheckoutService(_mockPaymentGateway.Object);
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);
        cart.AddItem("SKU001", 2);

        // Act
        var result = checkoutService.Checkout(cart, "token123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.OrderId.Should().NotBeNullOrEmpty();
        result.PaymentResult.Should().NotBeNull();
        result.PaymentResult!.IsSuccess.Should().BeTrue();
        result.PaymentResult.TransactionId.Should().Be("txn_123456");
    }

    [Fact]
    public void Checkout_ShouldChargeCorrectAmount()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(10);
        _mockPaymentGateway
            .Setup(p => p.Charge(1999.98m, "token123"))
            .Returns(PaymentResult.Success("txn_123456"));

        var checkoutService = new CheckoutService(_mockPaymentGateway.Object);
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);
        cart.AddItem("SKU001", 2); // 2 * 999.99 = 1999.98

        // Act
        var result = checkoutService.Checkout(cart, "token123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockPaymentGateway.Verify(p => p.Charge(1999.98m, "token123"), Times.Once);
    }

    [Fact]
    public void Checkout_WhenPaymentFails_ShouldReturnFailure()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(10);
        _mockPaymentGateway
            .Setup(p => p.Charge(It.IsAny<decimal>(), "invalid_token"))
            .Returns(PaymentResult.Failure("Payment declined"));

        var checkoutService = new CheckoutService(_mockPaymentGateway.Object);
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);
        cart.AddItem("SKU001", 1);

        // Act
        var result = checkoutService.Checkout(cart, "invalid_token");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.OrderId.Should().BeNull();
        result.ErrorMessage.Should().Contain("Payment");
        result.PaymentResult.Should().NotBeNull();
        result.PaymentResult!.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Checkout_WithInvalidPaymentToken_ShouldFail()
    {
        // Arrange
        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(10);

        var checkoutService = new CheckoutService(_mockPaymentGateway.Object);
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);
        cart.AddItem("SKU001", 1);

        // Act
        var result = checkoutService.Checkout(cart, "");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.ErrorMessage.Should().Contain("token");
    }

    [Fact]
    public void Checkout_ShouldNotCallPaymentGateway_WhenCartIsEmpty()
    {
        // Arrange
        var checkoutService = new CheckoutService(_mockPaymentGateway.Object);
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object);

        // Act
        var result = checkoutService.Checkout(cart, "token123");

        // Assert
        _mockPaymentGateway.Verify(p => p.Charge(It.IsAny<decimal>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Checkout_WithDiscounts_ShouldChargeDiscountedAmount()
    {
        // Arrange
        var discountEngine = new DiscountEngine();
        discountEngine.AddRule(new OrderDiscountRule());

        _mockCatalog.Setup(c => c.GetProductBySku("SKU001")).Returns(_laptop);
        _mockInventory.Setup(i => i.GetAvailableQuantity("SKU001")).Returns(10);
        
        // Subtotal: 1999.98, Discount: 5% = 99.999, Total: 1899.981
        decimal expectedSubtotal = 999.99m * 2;
        decimal expectedDiscount = expectedSubtotal * 0.05m;
        decimal expectedTotal = expectedSubtotal - expectedDiscount;
        _mockPaymentGateway
            .Setup(p => p.Charge(expectedTotal, "token123"))
            .Returns(PaymentResult.Success("txn_123456"));

        var checkoutService = new CheckoutService(_mockPaymentGateway.Object);
        var cart = new Cart(_mockCatalog.Object, _mockInventory.Object, discountEngine);
        cart.AddItem("SKU001", 2); // 2 * 999.99 = 1999.98

        // Act
        var result = checkoutService.Checkout(cart, "token123");

        // Assert
        result.IsSuccess.Should().BeTrue();
        _mockPaymentGateway.Verify(p => p.Charge(expectedTotal, "token123"), Times.Once);
    }
}