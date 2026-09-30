using BuilderPattern.Domain;

namespace BuilderPattern.Builders;

public sealed class InvoiceBuilder
{
    private int? _invoiceId;
    private string? _customerName;
    private string? _customerEmail;
    private string? _customerPhone;

    private string? _billingStreet;
    private string? _billingCity;
    private string? _billingState;
    private string? _billingZipCode;
    private string? _billingCountry;

    private string? _shippingStreet;
    private string? _shippingCity;
    private string? _shippingState;
    private string? _shippingZipCode;
    private string? _shippingCountry;

    private DateOnly? _orderDate;
    private string? _paymentMethod;
    private string? _currency;
    private decimal? _subTotal;
    private decimal _discountAmount;
    private decimal _taxAmount;

    public InvoiceBuilder WithInvoiceId(int invoiceId)
    {
        _invoiceId = invoiceId;
        return this;
    }

    public InvoiceBuilder WithCustomer(string name, string email, string phone)
    {
        _customerName = name;
        _customerEmail = email;
        _customerPhone = phone;
        return this;
    }

    public InvoiceBuilder WithBillingAddress(
        string street,
        string city,
        string state,
        string zipCode,
        string country)
    {
        _billingStreet = street;
        _billingCity = city;
        _billingState = state;
        _billingZipCode = zipCode;
        _billingCountry = country;
        return this;
    }

    public InvoiceBuilder WithShippingAddress(
        string street,
        string city,
        string state,
        string zipCode,
        string country)
    {
        _shippingStreet = street;
        _shippingCity = city;
        _shippingState = state;
        _shippingZipCode = zipCode;
        _shippingCountry = country;
        return this;
    }

    public InvoiceBuilder WithOrderDate(DateOnly orderDate)
    {
        _orderDate = orderDate;
        return this;
    }

    public InvoiceBuilder WithPaymentMethod(string paymentMethod)
    {
        _paymentMethod = paymentMethod;
        return this;
    }

    public InvoiceBuilder WithCurrency(string currency)
    {
        _currency = currency;
        return this;
    }

    public InvoiceBuilder WithSubTotal(decimal subTotal)
    {
        _subTotal = subTotal;
        return this;
    }

    public InvoiceBuilder WithDiscount(decimal discountAmount)
    {
        _discountAmount = discountAmount;
        return this;
    }

    public InvoiceBuilder WithTax(decimal taxAmount)
    {
        _taxAmount = taxAmount;
        return this;
    }

    public Invoice Build()
    {
        if (_invoiceId is null or <= 0)
            throw new InvalidOperationException("Invoice id is required and must be positive.");

        EnsureText(_customerName, "Customer name");
        EnsureText(_customerEmail, "Customer email");
        EnsureText(_customerPhone, "Customer phone");

        EnsureText(_billingStreet, "Billing street");
        EnsureText(_billingCity, "Billing city");
        EnsureText(_billingState, "Billing state");
        EnsureText(_billingZipCode, "Billing zip code");
        EnsureText(_billingCountry, "Billing country");

        EnsureText(_shippingStreet, "Shipping street");
        EnsureText(_shippingCity, "Shipping city");
        EnsureText(_shippingState, "Shipping state");
        EnsureText(_shippingZipCode, "Shipping zip code");
        EnsureText(_shippingCountry, "Shipping country");

        if (_orderDate is null)
            throw new InvalidOperationException("Order date is required.");

        EnsureText(_paymentMethod, "Payment method");
        EnsureText(_currency, "Currency");

        if (_subTotal is null or < 0)
            throw new InvalidOperationException("Sub total is required and cannot be negative.");

        if (_discountAmount < 0)
            throw new InvalidOperationException("Discount amount cannot be negative.");

        if (_taxAmount < 0)
            throw new InvalidOperationException("Tax amount cannot be negative.");

        decimal total = _subTotal.Value - _discountAmount + _taxAmount;

        if (total < 0)
            throw new InvalidOperationException("Total amount cannot be negative.");

        return new Invoice(
            _invoiceId.Value,
            _customerName!,
            _customerEmail!,
            _customerPhone!,
            _billingStreet!,
            _billingCity!,
            _billingState!,
            _billingZipCode!,
            _billingCountry!,
            _shippingStreet!,
            _shippingCity!,
            _shippingState!,
            _shippingZipCode!,
            _shippingCountry!,
            _orderDate.Value,
            _paymentMethod!,
            _currency!,
            _subTotal.Value,
            _discountAmount,
            _taxAmount,
            total);
    }

    private static void EnsureText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{fieldName} is required.");
    }
}
