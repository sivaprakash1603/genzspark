using BusBooking.Api.Application.DTOs;

namespace BusBooking.Api.Application.Interfaces;

public interface IBookingService
{
    Task<List<BusSearchResponse>> SearchBusesAsync(string source, string destination, DateTime date);
    Task<List<LocationDto>> GetLocationsAsync(string query);
    Task<List<SeatResponse>> GetSeatsAsync(Guid busId, DateTime journeyDate, Guid? userId = null);
    Task<bool> LockSeatAsync(Guid userId, Guid seatId, DateTime journeyDate);
    Task<bool> UnlockSeatAsync(Guid userId, Guid seatId);
    Task<BookingResponse> InitiateBookingAsync(Guid passengerId, InitiateBookingRequest request);
    Task<List<BookingResponse>> GetMyBookingsAsync(Guid passengerId, string? filter = null);
    Task<byte[]> GetTicketPdfAsync(Guid passengerId, Guid bookingId);
    Task CancelBookingAsync(Guid passengerId, Guid bookingId, string reason);
    Task<List<RouteResponse>> GetPublicRoutesAsync();
}
