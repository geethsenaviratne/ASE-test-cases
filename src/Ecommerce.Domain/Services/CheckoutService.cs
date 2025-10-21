using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Services;

public class CheckoutService : ICheckoutService
{
    private readonly IPaymentGateway _paymentGateway;

    public CheckoutService(IPaymentGateway paymentGateway)
    {
        _paymentGateway = paymentGateway ?? throw new ArgumentNullException(nameof(paymentGateway));
    }

    public CheckoutResult Checkout(Cart cart, string paymentToken)
    {
        // Validate payment token
        var tokenValidation = ValidatePaymentToken(paymentToken);
        if (!tokenValidation.IsValid)
            return CheckoutResult.Failure(tokenValidation.ErrorMessage!);

        // Validate cart
        var cartValidation = CartValidator.Validate(cart);
        if (!cartValidation.IsValid)
            return CheckoutResult.Failure(cartValidation.ErrorMessage!);

        // Calculate total amount
        var totalAmount = cart.CalculateTotal();

        // Process payment
        var paymentResult = ProcessPayment(totalAmount, paymentToken);

        if (!paymentResult.IsSuccess)
        {
            return CheckoutResult.Failure(
                $"Payment failed: {paymentResult.ErrorMessage}", 
                paymentResult);
        }

        // Generate order ID
        var orderId = GenerateOrderId();

        return CheckoutResult.Success(orderId, paymentResult);
    }

    private static (bool IsValid, string? ErrorMessage) ValidatePaymentToken(string paymentToken)
    {
        if (string.IsNullOrWhiteSpace(paymentToken))
            return (false, "Payment token is required");

        return (true, null);
    }

    private PaymentResult ProcessPayment(decimal amount, string paymentToken)
    {
        try
        {
            return _paymentGateway.Charge(amount, paymentToken);
        }
        catch (Exception ex)
        {
            return PaymentResult.Failure($"Payment processing error: {ex.Message}");
        }
    }

    private static string GenerateOrderId()
    {
        return $"ORD-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}