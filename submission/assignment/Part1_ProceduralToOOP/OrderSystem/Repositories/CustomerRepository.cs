using OrderSystem.Domain;

namespace OrderSystem.Repositories;

internal class CustomerRepository
{
    private readonly List<Customer> _customers = [];
    private int _nextId = 1;

    public IReadOnlyList<Customer> GetAll() => _customers.AsReadOnly();

    public Customer Add(
        string name,
        string email,
        string city,
        CustomerType customerType)
    {
        EnsureEmailAvailable(email);
        var customer = Customer.Create(
            _nextId,
            name,
            city,
            email,
            customerType);

        _customers.Add(customer);
        _nextId++;

        return customer;
    }

    public IReadOnlyList<Customer> Search(string query) => _customers
        .Where(customer => customer.Name.Contains(query, StringComparison.OrdinalIgnoreCase)
            || customer.Email.Value.Contains(query, StringComparison.OrdinalIgnoreCase)
            || customer.City.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

    public void UpdateName(int id, string name)
        => GetById(id).ChangeName(name);

    public void UpdateEmail(int id, string email)
    {
        var customer = GetById(id);
        EnsureEmailAvailable(email, id);
        customer.ChangeEmail(email);
    }

    public void UpdateCity(int id, string city)
        => GetById(id).ChangeCity(city);

    public void UpdateCustomerType(int id, CustomerType type)
        => GetById(id).ChangeCustomerType(type);

    private void EnsureEmailAvailable(string email, int? exceptId = null)
    {
        if (_customers.Any(customer => customer.Id != exceptId
            && string.Equals(customer.Email.Value, email?.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("A customer with this email already exists.");
    }

    public Customer GetById(int id)
    {
        return _customers.FirstOrDefault(customer => customer.Id == id)
            ?? throw new InvalidOperationException($"Customer #{id} was not found.");
    }
}
