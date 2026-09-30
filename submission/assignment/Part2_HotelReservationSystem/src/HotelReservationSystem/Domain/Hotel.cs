namespace HotelReservationSystem.Domain;

public sealed class Hotel
{
    private readonly List<Guest> _guests = [];
    private readonly List<Room> _rooms = [];
    private readonly HashSet<int> _reservationIds = [];

    public IReadOnlyList<Guest> Guests => _guests.AsReadOnly();
    public IReadOnlyList<Room> Rooms => _rooms.AsReadOnly();

    public void RegisterGuest(Guest guest)
    {
        ArgumentNullException.ThrowIfNull(guest);

        if (_guests.Any(g => g.GuestId == guest.GuestId))
            throw new InvalidOperationException($"Guest id {guest.GuestId} already exists.");

        _guests.Add(guest);
    }

    public void AddRoom(Room room)
    {
        ArgumentNullException.ThrowIfNull(room);

        if (_rooms.Any(r => r.RoomNumber == room.RoomNumber))
            throw new InvalidOperationException($"Room number {room.RoomNumber} already exists.");

        _rooms.Add(room);
    }

    public Reservation BookReservation(
        int reservationId,
        int guestId,
        int roomNumber,
        DateOnly checkInDate,
        DateOnly checkOutDate)
    {
        if (_reservationIds.Contains(reservationId))
            throw new InvalidOperationException($"Reservation id {reservationId} already exists.");

        Guest guest = _guests.FirstOrDefault(g => g.GuestId == guestId)
            ?? throw new InvalidOperationException($"Guest id {guestId} not found.");

        Room room = _rooms.FirstOrDefault(r => r.RoomNumber == roomNumber)
            ?? throw new InvalidOperationException($"Room number {roomNumber} not found.");

        Reservation reservation = guest.MakeReservation(
            reservationId,
            room,
            checkInDate,
            checkOutDate);

        _reservationIds.Add(reservationId);
        return reservation;
    }
}
