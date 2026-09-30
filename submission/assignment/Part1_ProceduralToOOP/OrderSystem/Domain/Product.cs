namespace OrderSystem.Domain;

internal class Product : Entity
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }

    private Product(
        int id,
        string name,
        decimal price,
        int stock) : base(id)
    {
        Name = name;
        Price = price;
        Stock = stock;
    }

    public static Product Create(
        int id,
        string name,
        decimal price,
        int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Product name is required.");

        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "Product price must be greater than zero.");

        if (stock < 0)
            throw new ArgumentOutOfRangeException(nameof(stock), "Product stock cannot be negative.");

        return new Product(
            id,
            name.Trim(),
            price,
            stock);
    }

    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new InvalidOperationException("Product name is required.");

        Name = newName.Trim();
    }

    public void ChangePrice(decimal newPrice)
    {
        if (newPrice <= 0)
            throw new ArgumentOutOfRangeException(nameof(newPrice), "Product price must be greater than zero.");

        Price = newPrice;
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        Stock = checked(Stock + quantity);
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be positive.");

        if (Stock < quantity)
            throw new InvalidOperationException($"Not enough stock for product #{Id}.");

        Stock -= quantity;
    }

    public override string ToString()
        => $"#{Id}  {Name}  Price = {Price:F2}  Stock = {Stock}";
}
