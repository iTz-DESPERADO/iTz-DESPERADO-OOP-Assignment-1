namespace BuilderPattern.Domain;

public sealed record AddressData(
    string Street,
    string City,
    string State,
    string ZipCode,
    string Country);
