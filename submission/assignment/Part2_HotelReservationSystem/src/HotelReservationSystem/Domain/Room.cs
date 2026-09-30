using HotelReservationSystem.Domain.Enums;

namespace HotelReservationSystem.Domain;

public sealed class Room
{
    private readonly List<Reservation> _reservations = [];

    public int RoomNumber { get; }
    public RoomType RoomType { get; }
    public decimal NightlyRate { get; private set; }
    public bool IsUnderMaintenance { get; private set; }

    public Room(int roomNumber, RoomType roomType, decimal nightlyRate)
    {
        if (roomNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(roomNumber), "Room number must be positive.");

        if (!Enum.IsDefined(roomType))
            throw new ArgumentOutOfRangeException(nameof(roomType), "Room type must be Single, Double, or Suite.");

        if (nightlyRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(nightlyRate), "Nightly rate must be positive.");

        RoomNumber = roomNumber;
        RoomType = roomType;
        NightlyRate = nightlyRate;
    }

    public void ChangeNightlyRate(decimal newRate)
    {
        if (newRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(newRate), "Nightly rate must be positive.");

        NightlyRate = newRate;
    }

    public void StartMaintenance()
    {
        IsUnderMaintenance = true;
    }

    public void EndMaintenance()
    {
        IsUnderMaintenance = false;
    }

    internal Reservation CreateReservation(int reservationId, DateOnly checkInDate, DateOnly checkOutDate)
    {
        if (IsUnderMaintenance)
            throw new InvalidOperationException("A room under maintenance cannot be booked.");

        if (checkOutDate <= checkInDate)
            throw new ArgumentException("Check-out date must be after check-in date.");

        bool overlaps = _reservations.Any(existing =>
            existing.IsActiveForBooking &&
            checkInDate < existing.CheckOutDate &&
            checkOutDate > existing.CheckInDate);

        if (overlaps)
            throw new InvalidOperationException("The room is already booked for an overlapping date range.");

        var reservation = new Reservation(reservationId, checkInDate, checkOutDate, this);
        _reservations.Add(reservation);
        return reservation;
    }
}
