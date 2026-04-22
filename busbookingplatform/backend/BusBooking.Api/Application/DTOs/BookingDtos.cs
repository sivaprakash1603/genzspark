namespace BusBooking.Api.Application.DTOs;

public record PassengerDetailsDto(Guid SeatId, string Name, int Age, string Gender);

public record InitiateBookingRequest(Guid BusId, List<PassengerDetailsDto> Passengers);

public record BookingResponse(Guid BookingId, string BookingStatus, decimal TotalAmount, DateTime BookedAt);

public record CancelBookingRequest(string Reason);
