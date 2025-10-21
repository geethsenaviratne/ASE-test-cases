namespace Ecommerce.Domain.Models;

public class DiscountApplication
{
    public string RuleName { get; }
    public decimal DiscountAmount { get; }

    public DiscountApplication(string ruleName, decimal discountAmount)
    {
        if (string.IsNullOrWhiteSpace(ruleName))
            throw new ArgumentException("Rule name cannot be empty", nameof(ruleName));
        
        if (discountAmount < 0)
            throw new ArgumentException("Discount amount cannot be negative", nameof(discountAmount));

        RuleName = ruleName;
        DiscountAmount = discountAmount;
    }
}