namespace BusBooking.Api.Application.DTOs;

public record PassengerDetailsDto(Guid SeatId, string Name, int Age, string Gender);

public record InitiateBookingRequest(Guid BusId, DateTime JourneyDate, List<PassengerDetailsDto> Passengers);

public record BookingResponse(
    Guid BookingId, 
    string BookingStatus, 
    decimal TotalAmount, 
    DateTime BookedAt, 
    DateTime JourneyDate,
    string? BusName,
    string? Source,
    string? Destination
);

public record TicketDownloadResponse(string TicketNumber, DateTime IssuedAt, string Content);

public record CancelBookingRequest(string Reason);
