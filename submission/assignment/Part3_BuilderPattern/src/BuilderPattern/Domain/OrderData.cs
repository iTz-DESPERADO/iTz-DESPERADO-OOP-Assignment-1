namespace BuilderPattern.Domain;

public sealed record OrderData(
    DateOnly OrderDate,
    string PaymentMethod,
    string Currency,
    decimal SubTotal,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount);
