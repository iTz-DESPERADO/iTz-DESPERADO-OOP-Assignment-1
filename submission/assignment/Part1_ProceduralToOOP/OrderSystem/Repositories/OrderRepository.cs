using OrderSystem.Domain;

namespace OrderSystem.Repositories;

internal class OrderRepository
{
    private readonly List<Order> _orders = [];
    private int _nextId = 1;

    public IReadOnlyList<Order> GetAll() => _orders.AsReadOnly();

    public Order Add(Customer customer, DateOnly date)
    {
        var order = Order.Create(
            _nextId,
            customer,
            date);

        _orders.Add(order);
        _nextId++;

        return order;
    }

    public IReadOnlyList<Order> GetByCustomerId(int customerId) => _orders
        .Where(order => order.Customer.Id == customerId)
        .OrderByDescending(order => order.Date).ThenByDescending(order => order.Id).ToList();

    public IReadOnlyList<Order> GetUnpaid() => _orders.Where(order => !order.IsPaid).ToList();

    public IReadOnlyList<Order> GetByDateRange(DateOnly from, DateOnly to)
    {
        if (from > to)
            throw new ArgumentException("Start date must be on or before end date.");
        return _orders.Where(order => order.Date >= from && order.Date <= to).ToList();
    }

    public Order GetById(int id)
    {
        return _orders.FirstOrDefault(order => order.Id == id)
            ?? throw new InvalidOperationException($"Order #{id} was not found.");
    }
}
