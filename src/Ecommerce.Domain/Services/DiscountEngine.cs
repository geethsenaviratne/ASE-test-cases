using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Services;

public class DiscountEngine : IDiscountEngine
{
    private readonly List<IDiscountRule> _rules = new();

    public void AddRule(IDiscountRule rule)
    {
        if (rule == null)
            throw new ArgumentNullException(nameof(rule));
        
        _rules.Add(rule);
    }

    public decimal CalculateDiscount(IReadOnlyCollection<LineItem> items, decimal subtotal)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        return _rules.Sum(rule => rule.CalculateDiscount(items, subtotal));
    }

    public IReadOnlyCollection<DiscountApplication> GetAppliedDiscounts(IReadOnlyCollection<LineItem> items, decimal subtotal)
    {
        if (items == null)
            throw new ArgumentNullException(nameof(items));

        var appliedDiscounts = new List<DiscountApplication>();

        foreach (var rule in _rules)
        {
            var discount = rule.CalculateDiscount(items, subtotal);
            if (discount > 0)
            {
                appliedDiscounts.Add(new DiscountApplication(rule.Name, discount));
            }
        }

        return appliedDiscounts.AsReadOnly();
    }
}