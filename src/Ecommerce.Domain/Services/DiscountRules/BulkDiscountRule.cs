using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Services.DiscountRules;

public class BulkDiscountRule : IDiscountRule
{
    private const int MinimumQuantity = 10;
    private const decimal DiscountPercentage = 0.10m;

    public string Name => "Bulk Discount";

    public decimal CalculateDiscount(IReadOnlyCollection<LineItem> items, decimal subtotal)
    {
        if (items == null || !items.Any())
            return 0m;

        decimal totalDiscount = 0m;

        foreach (var item in items)
        {
            if (item.Quantity >= MinimumQuantity)
            {
                var lineTotal = item.GetLineTotal();
                totalDiscount += lineTotal * DiscountPercentage;
            }
        }

        return totalDiscount;
    }
}