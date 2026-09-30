using BuilderPattern.Domain;

namespace BuilderPattern.Builders;

public sealed class ComposedInvoiceBuilder
{
    private int? _invoiceId;
    private string? _customerName;
    private string? _customerEmail;
    private string? _customerPhone;
    private AddressData? _billingAddress;
    private AddressData? _shippingAddress;
    private OrderData? _order;

    public ComposedInvoiceBuilder WithInvoiceId(int invoiceId)
    {
        _invoiceId = invoiceId;
        return this;
    }

    public ComposedInvoiceBuilder WithCustomer(string name, string email, string phone)
    {
        _customerName = name;
        _customerEmail = email;
        _customerPhone = phone;
        return this;
    }

    public ComposedInvoiceBuilder WithBillingAddress(Action<AddressBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new AddressBuilder();
        configure(builder);
        _billingAddress = builder.Build();
        return this;
    }

    public ComposedInvoiceBuilder WithShippingAddress(Action<AddressBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new AddressBuilder();
        configure(builder);
        _shippingAddress = builder.Build();
        return this;
    }

    public ComposedInvoiceBuilder WithOrder(Action<OrderBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);
        var builder = new OrderBuilder();
        configure(builder);
        _order = builder.Build();
        return this;
    }

    public Invoice Build()
    {
        if (_invoiceId is null or <= 0)
            throw new InvalidOperationException("Invoice id is required and must be positive.");

        EnsureText(_customerName, "Customer name");
        EnsureText(_customerEmail, "Customer email");
        EnsureText(_customerPhone, "Customer phone");

        if (_billingAddress is null)
            throw new InvalidOperationException("Billing address is required.");

        if (_shippingAddress is null)
            throw new InvalidOperationException("Shipping address is required.");

        if (_order is null)
            throw new InvalidOperationException("Order information is required.");

        return new Invoice(
            _invoiceId.Value,
            _customerName!,
            _customerEmail!,
            _customerPhone!,
            _billingAddress.Street,
            _billingAddress.City,
            _billingAddress.State,
            _billingAddress.ZipCode,
            _billingAddress.Country,
            _shippingAddress.Street,
            _shippingAddress.City,
            _shippingAddress.State,
            _shippingAddress.ZipCode,
            _shippingAddress.Country,
            _order.OrderDate,
            _order.PaymentMethod,
            _order.Currency,
            _order.SubTotal,
            _order.DiscountAmount,
            _order.TaxAmount,
            _order.TotalAmount);
    }

    private static void EnsureText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{fieldName} is required.");
    }
}
