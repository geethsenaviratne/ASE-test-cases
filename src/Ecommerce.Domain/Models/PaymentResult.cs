namespace Ecommerce.Domain.Models;

public class PaymentResult
{
    public bool IsSuccess { get; }
    public string TransactionId { get; }
    public string? ErrorMessage { get; }

    private PaymentResult(bool isSuccess, string transactionId, string? errorMessage = null)
    {
        IsSuccess = isSuccess;
        TransactionId = transactionId;
        ErrorMessage = errorMessage;
    }

    public static PaymentResult Success(string transactionId)
    {
        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));
        
        return new PaymentResult(true, transactionId);
    }

    public static PaymentResult Failure(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty", nameof(errorMessage));
        
        return new PaymentResult(false, string.Empty, errorMessage);
    }
}