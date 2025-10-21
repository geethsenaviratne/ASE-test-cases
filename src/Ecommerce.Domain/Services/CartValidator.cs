using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Services;

public static class CartValidator
{
    public static (bool IsValid, string? ErrorMessage) Validate(Cart cart)
    {
        if (cart == null)
            return (false, "Cart cannot be null");

        if (!cart.GetItems().Any())
            return (false, "Cannot checkout with an empty cart");

        var total = cart.CalculateTotal();
        if (total <= 0)
            return (false, "Cart total must be greater than zero");

        return (true, null);
    }
}