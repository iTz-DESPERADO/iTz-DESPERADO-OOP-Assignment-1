namespace OrderSystem.Domain;

internal class Order : Entity
{
    private readonly List<OrderLine> _lines = [];

    public Customer Customer { get; }
    public DateOnly Date { get; }
    public bool IsPaid { get; private set; }
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Total => _lines.Sum(line => line.Total);

    private Order(
        int id,
        Customer customer,
        DateOnly date) : base(id)
    {
        Customer = customer;
        Date = date;
    }

    public static Order Create(
        int id,
        Customer customer,
        DateOnly date)
    {
        ArgumentNullException.ThrowIfNull(customer);

        return new Order(
            id,
            customer,
            date);
    }

    public void AddLine(Product product, int quantity)
    {
        if (IsPaid)
            throw new InvalidOperationException("Cannot change a paid order.");

        ArgumentNullException.ThrowIfNull(product);

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        var existingLine = _lines.FirstOrDefault(line => line.Product.Id == product.Id);
        if (existingLine is not null)
        {
            existingLine.IncreaseQuantity(quantity);
            return;
        }

        var orderLine = new OrderLine(
            this,
            product,
            quantity);

        _ = checked(Total + orderLine.Total);
        product.DecreaseStock(quantity);
        _lines.Add(orderLine);
    }

    public void MarkPaid()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException("Cannot pay an empty order.");

        IsPaid = true;
    }

    public override string ToString()
        => $"Order #{Id} - {Customer.Name} - {Date:yyyy-MM-dd} - Paid={IsPaid}";
}
