using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Services.DiscountRules;

public class OrderDiscountRule : IDiscountRule
{
    private const decimal MinimumOrderTotal = 1000m;
    private const decimal DiscountPercentage = 0.05m;

    public string Name => "Order Discount";

    public decimal CalculateDiscount(IReadOnlyCollection<LineItem> items, decimal subtotal)
    {
        if (subtotal >= MinimumOrderTotal)
        {
            return subtotal * DiscountPercentage;
        }

        return 0m;
    }
}