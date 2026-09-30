namespace OrderSystem.Domain;

internal class OrderLine
{
    public Order Order { get; }
    public Product Product { get; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; }
    public decimal Total => UnitPrice * Quantity;

    internal void IncreaseQuantity(int quantity)
    {
        if (Order.IsPaid)
            throw new InvalidOperationException("Cannot change a paid order.");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        int newQuantity = checked(Quantity + quantity);
        _ = checked(Order.Total + UnitPrice * quantity);
        Product.DecreaseStock(quantity);
        Quantity = newQuantity;
    }

    internal OrderLine(
        Order order,
        Product product,
        int quantity)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(product);

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        Order = order;
        Product = product;
        Quantity = quantity;
        UnitPrice = product.Price;
    }
}
