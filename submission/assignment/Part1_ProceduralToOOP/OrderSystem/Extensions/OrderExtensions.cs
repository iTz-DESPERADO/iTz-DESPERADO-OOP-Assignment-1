using OrderSystem.Domain;
using OrderSystem.Pricing;

namespace OrderSystem.Extensions;

internal static class OrderExtensions
{
    public static OrderPricing CalculatePricing(this Order order, CustomerDiscountPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(order);
        return CalculatePricing(order.Total, order.Customer, policy);
    }

    public static OrderPricing CalculatePricing(decimal subtotal, Customer customer, CustomerDiscountPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentOutOfRangeException.ThrowIfNegative(subtotal);

        decimal rate = policy.GetDiscountRate(customer);
        decimal discount = decimal.Round(subtotal * rate, 2, MidpointRounding.AwayFromZero);
        return new OrderPricing(subtotal, rate, Math.Min(subtotal, discount));
    }
}
