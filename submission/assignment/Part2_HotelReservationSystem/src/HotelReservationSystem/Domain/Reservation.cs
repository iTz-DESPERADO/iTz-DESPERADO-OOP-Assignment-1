using HotelReservationSystem.Domain.Enums;

namespace HotelReservationSystem.Domain;

public sealed class Reservation
{
    public int ReservationId { get; }
    public DateOnly CheckInDate { get; }
    public DateOnly CheckOutDate { get; }
    public Room Room { get; }
    public ReservationStatus Status { get; private set; }

    public int NumberOfNights => CheckOutDate.DayNumber - CheckInDate.DayNumber;
    public decimal TotalCost => NumberOfNights * Room.NightlyRate;

    internal Reservation(int reservationId, DateOnly checkInDate, DateOnly checkOutDate, Room room)
    {
        if (reservationId <= 0)
            throw new ArgumentOutOfRangeException(nameof(reservationId), "Reservation id must be positive.");

        ArgumentNullException.ThrowIfNull(room);

        if (checkOutDate <= checkInDate)
            throw new ArgumentException("Check-out date must be after check-in date.");

        if (room.IsUnderMaintenance)
            throw new InvalidOperationException("A room under maintenance cannot be booked.");

        ReservationId = reservationId;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        Room = room;
        Status = ReservationStatus.Pending;
    }

    public void Confirm()
    {
        EnsureStatus(ReservationStatus.Pending, "Only a pending reservation can be confirmed.");
        Status = ReservationStatus.Confirmed;
    }

    public void CheckIn()
    {
        EnsureStatus(ReservationStatus.Confirmed, "A reservation must be confirmed before check-in.");
        Status = ReservationStatus.CheckedIn;
    }

    public void CheckOut()
    {
        EnsureStatus(ReservationStatus.CheckedIn, "Only a checked-in reservation can be checked out.");
        Status = ReservationStatus.CheckedOut;
    }

    public void Cancel()
    {
        if (Status is not ReservationStatus.Pending and not ReservationStatus.Confirmed)
            throw new InvalidOperationException("Only pending or confirmed reservations can be cancelled.");

        Status = ReservationStatus.Cancelled;
    }

    internal bool IsActiveForBooking =>
        Status is not ReservationStatus.Cancelled and not ReservationStatus.CheckedOut;

    private void EnsureStatus(ReservationStatus required, string message)
    {
        if (Status != required)
            throw new InvalidOperationException(message);
    }
}
