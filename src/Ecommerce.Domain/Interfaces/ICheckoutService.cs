using Ecommerce.Domain.Models;
using Ecommerce.Domain.Services;

namespace Ecommerce.Domain.Interfaces;

public interface ICheckoutService
{
    CheckoutResult Checkout(Cart cart, string paymentToken);
}