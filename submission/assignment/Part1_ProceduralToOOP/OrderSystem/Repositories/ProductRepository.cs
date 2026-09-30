using OrderSystem.Domain;

namespace OrderSystem.Repositories;

internal class ProductRepository
{
    private readonly List<Product> _products = [];
    private int _nextId = 1;

    public IReadOnlyList<Product> GetAll() => _products.AsReadOnly();

    public Product Add(
        string name,
        decimal price,
        int stock)
    {
        var product = Product.Create(
            _nextId,
            name,
            price,
            stock);

        _products.Add(product);
        _nextId++;

        return product;
    }

    public Product GetById(int id)
    {
        return _products.FirstOrDefault(product => product.Id == id)
            ?? throw new InvalidOperationException($"Product #{id} was not found.");
    }

    public IReadOnlyList<Product> GetLowStock(int threshold = 5)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(threshold);

        return _products
            .Where(product => product.Stock <= threshold)
            .OrderBy(product => product.Stock)
            .ThenBy(product => product.Id)
            .ToList();
    }

    public IReadOnlyList<Product> GetByIds(IEnumerable<int> ids)
    {
        ArgumentNullException.ThrowIfNull(ids);

        return _products
            .Where(product => ids.Contains(product.Id))
            .ToList();
    }
}
