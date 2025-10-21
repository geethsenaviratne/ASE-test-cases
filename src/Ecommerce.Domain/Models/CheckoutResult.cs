namespace Ecommerce.Domain.Models;

public class CheckoutResult
{
    public bool IsSuccess { get; }
    public string? OrderId { get; }
    public string? ErrorMessage { get; }
    public PaymentResult? PaymentResult { get; }

    private CheckoutResult(bool isSuccess, string? orderId, string? errorMessage, PaymentResult? paymentResult)
    {
        IsSuccess = isSuccess;
        OrderId = orderId;
        ErrorMessage = errorMessage;
        PaymentResult = paymentResult;
    }

    public static CheckoutResult Success(string orderId, PaymentResult paymentResult)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("Order ID cannot be empty", nameof(orderId));
        
        return new CheckoutResult(true, orderId, null, paymentResult);
    }

    public static CheckoutResult Failure(string errorMessage, PaymentResult? paymentResult = null)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));
        
        return new CheckoutResult(false, null, errorMessage, paymentResult);
    }
}