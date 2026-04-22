namespace BusBooking.Api.Application.DTOs;

public record AddBusRequest(
    Guid RouteId,
    string BusName,
    string BoardingPoint,
    string DropPoint,
    DateTime DepartureTime,
    int DurationMinutes,
    string SeatLayoutType,
    int TotalSeats,
    decimal BasePrice);

public record BusSearchResponse(
    Guid BusId,
    string BusName,
    string Source,
    string Destination,
    string BoardingPoint,
    string DropPoint,
    DateTime DepartureTime,
    int DurationMinutes,
    decimal TotalPrice,
    int TotalSeats,
    int AvailableSeats);

public record SeatResponse(
    Guid SeatId, 
    string SeatNumber, 
    bool IsBooked, 
    bool IsLocked, 
    string? LockedBy);

public record LocationDto(Guid Id, string Name);
