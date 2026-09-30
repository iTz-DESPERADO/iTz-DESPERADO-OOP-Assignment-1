using BuilderPattern.Builders;
using BuilderPattern.Domain;

Console.WriteLine("Single large builder:");

Invoice invoice1 = new InvoiceBuilder()
    .WithInvoiceId(1)
    .WithCustomer("Gamal Khaled", "Gamalkhaled@example.com", "+218910000000")
    .WithBillingAddress("10 Main St", "Tripoli", "Tripoli", "1000", "Libya")
    .WithShippingAddress("10 Main St", "Tripoli", "Tripoli", "1000", "Libya")
    .WithOrderDate(new DateOnly(2026, 9, 28))
    .WithPaymentMethod("Cash")
    .WithCurrency("LYD")
    .WithSubTotal(1000m)
    .WithDiscount(50m)
    .WithTax(0m)
    .Build();

Console.WriteLine($"Invoice #{invoice1.InvoiceId} total = {invoice1.TotalAmount:F2} {invoice1.Currency}");

Console.WriteLine("\nComposed builders:");

Invoice invoice2 = new ComposedInvoiceBuilder()
    .WithInvoiceId(2)
    .WithCustomer("Omar Hassan", "omar@example.com", "+218920000000")
    .WithBillingAddress(address => address
        .WithStreet("20 Palm St")
        .WithCity("Tripoli")
        .WithState("Tripoli")
        .WithZipCode("1000")
        .WithCountry("Libya"))
    .WithShippingAddress(address => address
        .WithStreet("25 Sea Road")
        .WithCity("Misrata")
        .WithState("Misrata")
        .WithZipCode("2000")
        .WithCountry("Libya"))
    .WithOrder(order => order
        .WithOrderDate(new DateOnly(2026, 9, 28))
        .WithPaymentMethod("Card")
        .WithCurrency("LYD")
        .WithSubTotal(2000m)
        .WithDiscount(100m)
        .WithTax(50m))
    .Build();

Console.WriteLine($"Invoice #{invoice2.InvoiceId} total = {invoice2.TotalAmount:F2} {invoice2.Currency}");
