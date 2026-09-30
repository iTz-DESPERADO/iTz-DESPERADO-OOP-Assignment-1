# OOP Assignment 1

- Name: gamal khaled mohamed
- Student ID:

## Submission contents

The folder layout follows the assignment website: assignment work is in
`submission/assignment/`, and LeetCode account details, solution, and Accepted
screenshot are in `submission/leetcode/`.

- [Part 1: Procedural to OOP](submission/assignment/Part1_ProceduralToOOP/): C# order system and `Critique.md`.
- [Part 2: Hotel reservations](submission/assignment/Part2_HotelReservationSystem/): guests, rooms, reservations, and validation.
- [Part 3: Builder pattern](submission/assignment/Part3_BuilderPattern/): single and composed builders, plus `Answers.md`.
- [LeetCode 1679](submission/leetcode/): C# two-pointer solution and Accepted screenshot.

## Run

Requires the .NET 10 SDK. Run from the repository root:

```powershell
dotnet run --project submission/assignment/Part1_ProceduralToOOP/OrderSystem/OrderSystem.csproj
dotnet run --project submission/assignment/Part2_HotelReservationSystem/src/HotelReservationSystem/HotelReservationSystem.csproj
dotnet run --project submission/assignment/Part3_BuilderPattern/src/BuilderPattern/BuilderPattern.csproj
```

## Notes and assumptions

- Data is stored in memory and cleared when the program exits.
- Orders reserve stock when a line is added. Paid orders cannot be changed. Repeated additions of the same product increase its existing line quantity and keep that line's original unit price; paid orders keep their recorded discount and total.
- Hotel check-out dates are exclusive, so a room can be booked for a new stay starting on the previous check-out date. Cancelled and checked-out reservations do not block dates.
- Reservation cost uses the room's current nightly rate, as specified in the assignment.
- Invoice builders require identity, customer details, both addresses, order date, payment method, currency, and subtotal. Discount and tax default to zero; the total is calculated.