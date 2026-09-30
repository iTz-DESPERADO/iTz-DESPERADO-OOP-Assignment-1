using OrderSystem.ValueObject;

namespace OrderSystem.Domain;

internal class Customer : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public Email Email { get; private set; } = null!;
    public CustomerType Type { get; private set; }
    public bool IsVip => Type == CustomerType.Vip;

    private Customer(
        int id,
        string name,
        string city,
        Email email,
        CustomerType type) : base(id)
    {
        Name = name;
        City = city;
        Email = email;
        Type = type;
    }

    public static Customer Create(
        int id,
        string name,
        string city,
        string email,
        CustomerType customerType)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Customer name is required.");

        if (string.IsNullOrWhiteSpace(city))
            throw new InvalidOperationException("Customer city is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("Customer email is required.");

        if (!Enum.IsDefined(customerType))
            throw new InvalidOperationException("Customer type is invalid.");

        var customerEmail = Email.Create(email);

        return new Customer(
            id,
            name.Trim(),
            city.Trim(),
            customerEmail,
            customerType);
    }

    public void ChangeCustomerType(CustomerType customerType)
    {
        if (!Enum.IsDefined(customerType))
            throw new InvalidOperationException("Customer type is invalid.");

        Type = customerType;
    }

    public void ChangeName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new InvalidOperationException("Customer name is required.");

        Name = newName.Trim();
    }

    public void ChangeEmail(string newEmail)
    {
        if (string.IsNullOrWhiteSpace(newEmail))
            throw new InvalidOperationException("Customer email is required.");

        Email = Email.Create(newEmail);
    }

    public void ChangeCity(string newCity)
    {
        if (string.IsNullOrWhiteSpace(newCity))
            throw new InvalidOperationException("Customer city is required.");

        City = newCity.Trim();
    }

    public override string ToString()
        => $"#{Id}  {Name}  <{Email.Value}>  {City}  {Type}";
}

public enum CustomerType
{
    Regular = 1,
    Vip
}
