using BuilderPattern.Domain;

namespace BuilderPattern.Builders;

public sealed class AddressBuilder
{
    private string? _street;
    private string? _city;
    private string? _state;
    private string? _zipCode;
    private string? _country;

    public AddressBuilder WithStreet(string street)
    {
        _street = street;
        return this;
    }

    public AddressBuilder WithCity(string city)
    {
        _city = city;
        return this;
    }

    public AddressBuilder WithState(string state)
    {
        _state = state;
        return this;
    }

    public AddressBuilder WithZipCode(string zipCode)
    {
        _zipCode = zipCode;
        return this;
    }

    public AddressBuilder WithCountry(string country)
    {
        _country = country;
        return this;
    }

    public AddressData Build()
    {
        EnsureText(_street, "Street");
        EnsureText(_city, "City");
        EnsureText(_state, "State");
        EnsureText(_zipCode, "Zip code");
        EnsureText(_country, "Country");

        return new AddressData(
            _street!.Trim(),
            _city!.Trim(),
            _state!.Trim(),
            _zipCode!.Trim(),
            _country!.Trim());
    }

    private static void EnsureText(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"{fieldName} is required.");
    }
}
