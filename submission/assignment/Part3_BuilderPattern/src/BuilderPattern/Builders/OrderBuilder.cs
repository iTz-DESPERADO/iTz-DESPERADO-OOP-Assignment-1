using BuilderPattern.Domain;

namespace BuilderPattern.Builders;

public sealed class OrderBuilder
{
    private DateOnly? _orderDate;
    private string? _paymentMethod;
    private string? _currency;
    private decimal? _subTotal;
    private decimal _discountAmount;
    private decimal _taxAmount;

    public OrderBuilder WithOrderDate(DateOnly orderDate)
    {
        _orderDate = orderDate;
        return this;
    }

    public OrderBuilder WithPaymentMethod(string paymentMethod)
    {
        _paymentMethod = paymentMethod;
        return this;
    }

    public OrderBuilder WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public OrderBuilder WithSubTotal(decimal subTotal)
    {
        _subTotal = subTotal;
        return this;
    }

    public OrderBuilder WithDiscount(decimal discountAmount)
    {
        _discountAmount = discountAmount;
        return this;
    }

    public OrderBuilder WithTax(decimal taxAmount)
    {
        _taxAmount = taxAmount;
        return this;
    }

    public OrderData Build()
    {
        if (_orderDate is null)
            throw new InvalidOperationException("Order date is required.");

        if (string.IsNullOrWhiteSpace(_paymentMethod))
            throw new InvalidOperationException("Payment method is required.");

        if (string.IsNullOrWhiteSpace(_currency))
            throw new InvalidOperationException("Currency is required.");

        if (_subTotal is null or < 0)
            throw new InvalidOperationException("Sub total is required and cannot be negative.");

        if (_discountAmount < 0)
            throw new InvalidOperationException("Discount amount cannot be negative.");

        if (_taxAmount < 0)
            throw new InvalidOperationException("Tax amount cannot be negative.");

        decimal total = _subTotal.Value - _discountAmount + _taxAmount;

        if (total < 0)
            throw new InvalidOperationException("Total amount cannot be negative.");

        return new OrderData(
            _orderDate.Value,
            _paymentMethod.Trim(),
            _currency.Trim(),
            _subTotal.Value,
            _discountAmount,
            _taxAmount,
            total);
    }
}
