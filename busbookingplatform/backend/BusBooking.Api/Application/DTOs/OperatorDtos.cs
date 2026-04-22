namespace BusBooking.Api.Application.DTOs;

public record OperatorBookingResponse(Guid BookingId, Guid BusId, string BookingStatus, decimal Amount, DateTime BookedAt);
public record OperatorRevenueResponse(decimal TotalRevenue, int ConfirmedBookings);
