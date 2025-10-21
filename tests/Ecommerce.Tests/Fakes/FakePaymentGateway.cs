using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;

namespace Ecommerce.Tests.Fakes;

public class FakePaymentGateway : IPaymentGateway
{
    private readonly bool _shouldSucceed;
    private readonly string _transactionId;
    private readonly string _errorMessage;

    public FakePaymentGateway(bool shouldSucceed = true, string transactionId = "txn_fake_123", string errorMessage = "Payment declined")
    {
        _shouldSucceed = shouldSucceed;
        _transactionId = transactionId;
        _errorMessage = errorMessage;
    }

    public PaymentResult Charge(decimal amount, string paymentToken)
    {
        if (string.IsNullOrWhiteSpace(paymentToken))
            return PaymentResult.Failure("Invalid payment token");

        if (amount <= 0)
            return PaymentResult.Failure("Invalid amount");

        return _shouldSucceed 
            ? PaymentResult.Success(_transactionId) 
            : PaymentResult.Failure(_errorMessage);
    }
}