using Ecommerce.Domain.Models;

namespace Ecommerce.Domain.Interfaces;

public interface IDiscountRule
{
    string Name { get; }
    decimal CalculateDiscount(IReadOnlyCollection<LineItem> items, decimal subtotal);
}