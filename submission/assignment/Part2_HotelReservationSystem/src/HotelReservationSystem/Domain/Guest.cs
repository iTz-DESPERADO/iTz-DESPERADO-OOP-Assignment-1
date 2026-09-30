namespace HotelReservationSystem.Domain;

public sealed class Guest
{
    private readonly List<Reservation> _reservations = [];

    public int GuestId { get; }
    public string FullName { get; }
    public string PhoneNumber { get; }
    public IReadOnlyList<Reservation> Reservations => _reservations.AsReadOnly();

    public Guest(int guestId, string fullName, string phoneNumber)
    {
        if (guestId <= 0)
            throw new ArgumentOutOfRangeException(nameof(guestId), "Guest id must be positive.");

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Guest full name is required.", nameof(fullName));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Guest phone number is required.", nameof(phoneNumber));

        GuestId = guestId;
        FullName = fullName.Trim();
        PhoneNumber = phoneNumber.Trim();
    }

    internal Reservation MakeReservation(
        int reservationId,
        Room room,
        DateOnly checkInDate,
        DateOnly checkOutDate)
    {
        ArgumentNullException.ThrowIfNull(room);

        Reservation reservation = room.CreateReservation(
            reservationId,
            checkInDate,
            checkOutDate);

        _reservations.Add(reservation);
        return reservation;
    }
}
