namespace OrderSystem.Pricing;

internal sealed record OrderPricing(decimal Subtotal, decimal DiscountRate, decimal DiscountAmount)
{
    public decimal Total => Subtotal - DiscountAmount;
}
