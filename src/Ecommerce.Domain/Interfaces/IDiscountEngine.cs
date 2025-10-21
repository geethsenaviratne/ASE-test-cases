using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Interfaces;

public interface IDiscountEngine
{
    void AddRule(IDiscountRule rule);
    decimal CalculateDiscount(IReadOnlyCollection<LineItem> items, decimal subtotal);
    IReadOnlyCollection<DiscountApplication> GetAppliedDiscounts(IReadOnlyCollection<LineItem> items, decimal subtotal);
}