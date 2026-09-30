using HotelReservationSystem.Domain;
using HotelReservationSystem.Domain.Enums;

var hotel = new Hotel();

var guest = new Guest(1, "Gamal Khaled", "+218-91-0000000");
var room101 = new Room(101, RoomType.Single, 150m);
var room201 = new Room(201, RoomType.Suite, 350m);

hotel.RegisterGuest(guest);
hotel.AddRoom(room101);
hotel.AddRoom(room201);

Reservation reservation = hotel.BookReservation(
    1001,
    guest.GuestId,
    room101.RoomNumber,
    new DateOnly(2026, 10, 1),
    new DateOnly(2026, 10, 4));

reservation.Confirm();
reservation.CheckIn();
reservation.CheckOut();

Console.WriteLine($"Guest: {guest.FullName}");
Console.WriteLine($"Reservation: #{reservation.ReservationId}");
Console.WriteLine($"Room: {reservation.Room.RoomNumber}");
Console.WriteLine($"Status: {reservation.Status}");
Console.WriteLine($"Nights: {reservation.NumberOfNights}");
Console.WriteLine($"Total: {reservation.TotalCost:F2}");