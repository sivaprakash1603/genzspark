namespace BusBooking.Api.Application.DTOs;

public record OperatorBookingResponse(
    Guid BookingId, 
    Guid BusId, 
    string BusName,
    string Route,
    string PassengerName,
    string BookingStatus, 
    decimal Amount, 
    DateTime JourneyDate,
    DateTime BookedAt
);
public record OperatorRevenueResponse(decimal TotalRevenue, int ConfirmedBookings);

public record AddOfficeRequest(string CityName, string Address);
public record OperatorOfficeResponse(Guid Id, string CityName, string Address);
