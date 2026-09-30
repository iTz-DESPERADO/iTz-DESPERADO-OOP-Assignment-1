using OrderSystem.Domain;

namespace OrderSystem.Pricing;

internal sealed class CustomerDiscountPolicy
{
    public decimal RegularRate { get; }
    public decimal VipRate { get; }

    public CustomerDiscountPolicy(decimal regularRate = 0m, decimal vipRate = 0.10m)
    {
        ValidateRate(regularRate);
        ValidateRate(vipRate);
        RegularRate = regularRate;
        VipRate = vipRate;
    }

    public decimal GetDiscountRate(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        return customer.IsVip ? VipRate : RegularRate;
    }

    private static void ValidateRate(decimal rate)
    {
        if (rate < 0m || rate > 1m)
            throw new ArgumentOutOfRangeException(nameof(rate), "Discount rate must be between 0 and 1.");
    }
}
