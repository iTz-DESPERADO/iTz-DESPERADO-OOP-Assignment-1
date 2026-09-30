using System.Net.Mail;

namespace OrderSystem.ValueObject;

public sealed record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty.", nameof(value));

        value = value.Trim();

        if (value.Length > 254)
            throw new ArgumentException("Email is too long.", nameof(value));

        if (!IsValid(value))
            throw new ArgumentException("Email format is invalid.", nameof(value));

        return new Email(value);
    }

    private static bool IsValid(string value)
    {
        try
        {
            var address = new MailAddress(value);

            return address.Address == value;
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Value;
}
