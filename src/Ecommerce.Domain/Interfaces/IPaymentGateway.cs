using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Interfaces;

public interface IPaymentGateway
{
    PaymentResult Charge(decimal amount, string paymentToken);
}