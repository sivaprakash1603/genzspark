namespace BusBooking.Api.Application.DTOs;

public record AddVehicleRequest(
    string VehicleNumber,
    string BusName,
    string SeatLayoutType,
    int TotalSeats);

public record VehicleResponse(
    Guid Id,
    string VehicleNumber,
    string BusName,
    string SeatLayoutType,
    int TotalSeats,
    bool IsActive);

public record ScheduleMaintenanceRequest(DateTime? Start, DateTime? End);

public record AddBusRequest(
    Guid VehicleId,
    Guid RouteId,
    TimeSpan DepartureTime,
    List<int> AvailableDays,
    int DurationMinutes,
    decimal BasePrice);

public record UpdateBusRequest(
    TimeSpan DepartureTime,
    List<int> AvailableDays,
    int DurationMinutes,
    decimal BasePrice);

public record OperatorBusResponse(
    Guid BusId,
    string BusName,
    string ApprovalStatus,
    bool IsActive,
    bool IsTemporarilyDisabled,
    Guid RouteId,
    string Source,
    string Destination,
    string BoardingPoint,
    string DropPoint,
    TimeSpan DepartureTime,
    List<int> AvailableDays,
    int DurationMinutes,
    string SeatLayoutType,
    int TotalSeats,
    decimal BasePrice,
    decimal PlatformFee,
    decimal TotalPrice,
    DateTime? MaintenanceStart,
    DateTime? MaintenanceEnd,
    bool IsMarkedForRemoval,
    DateTime? RetirementDate,
    DateTime CreatedAt);

public record BusSearchResponse(
    Guid BusId,
    string BusName,
    string Source,
    string Destination,
    string BoardingPoint,
    string DropPoint,
    DateTime DepartureTime,
    DateTime ArrivalTime,
    int DurationMinutes,
    string SeatLayoutType,
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
