using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Application.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingService> _logger;

    public BookingService(AppDbContext db, IEmailService emailService, ILogger<BookingService> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<List<BusSearchResponse>> SearchBusesAsync(string source, string destination)
    {
        _logger.LogInformation("SearchBuses requested. Source={Source} Destination={Destination}", source, destination);

        var results = await _db.Buses
            .Include(x => x.Route).ThenInclude(r => r!.Source)
            .Include(x => x.Route).ThenInclude(r => r!.Destination)
            .Join(_db.OperatorProfiles,
                bus => bus.OperatorId,
                profile => profile.UserId,
                (bus, profile) => new { bus, profile })
            .Where(x => x.bus.ApprovalStatus == "Approved" && x.bus.IsActive && !x.bus.IsTemporarilyDisabled)
            .Where(x => x.profile.ApprovalStatus == "Approved" && x.profile.IsEnabled)
            .Where(x => x.bus.Route!.Source!.Name == source && x.bus.Route.Destination!.Name == destination)
            .Select(x => new BusSearchResponse(
                x.bus.Id,
                x.bus.BusName,
                x.bus.Route!.Source!.Name,
                x.bus.Route.Destination!.Name,
                x.bus.BoardingPoint,
                x.bus.DropPoint,
                x.bus.DepartureTime,
                x.bus.DurationMinutes,
                x.bus.TotalPrice,
                x.bus.TotalSeats))
            .ToListAsync();

        _logger.LogInformation("SearchBuses completed. ResultCount={Count}", results.Count);
        return results;
    }

    public async Task<List<SeatResponse>> GetSeatsAsync(Guid busId)
    {
        _logger.LogInformation("GetSeats requested. BusId={BusId}", busId);

        var bookedSeatIds = await _db.BookingSeats
            .Include(x => x.Booking)
            .Where(x => x.Seat!.BusId == busId && x.Booking!.BookingStatus == "Confirmed")
            .Select(x => x.SeatId)
            .ToListAsync();

        _logger.LogDebug("GetSeats booked seats loaded. BusId={BusId} BookedSeatCount={Count}", busId, bookedSeatIds.Count);

        var seats = await _db.Seats
            .Where(x => x.BusId == busId && x.IsActive)
            .OrderBy(x => x.SeatNumber)
            .Select(x => new SeatResponse(x.Id, x.SeatNumber, bookedSeatIds.Contains(x.Id)))
            .ToListAsync();

        _logger.LogInformation("GetSeats completed. BusId={BusId} SeatCount={Count}", busId, seats.Count);
        return seats;
    }

    public async Task<BookingResponse> InitiateBookingAsync(Guid passengerId, InitiateBookingRequest request)
    {
        _logger.LogInformation("InitiateBooking requested. PassengerId={PassengerId} BusId={BusId} SeatCount={SeatCount}", passengerId, request.BusId, request.SeatIds.Count);

        var bus = await _db.Buses.FirstOrDefaultAsync(x => x.Id == request.BusId && x.ApprovalStatus == "Approved" && x.IsActive && !x.IsTemporarilyDisabled)
            ?? throw new InvalidOperationException("Bus not available");

        var seats = await _db.Seats.Where(x => request.SeatIds.Contains(x.Id) && x.BusId == request.BusId).ToListAsync();
        if (seats.Count != request.SeatIds.Count)
        {
            _logger.LogWarning("InitiateBooking blocked: invalid seat selection. PassengerId={PassengerId} BusId={BusId} Requested={Requested} Found={Found}", passengerId, request.BusId, request.SeatIds.Count, seats.Count);
            throw new InvalidOperationException("Invalid seat selection");
        }

        var bookedSeatIds = await _db.BookingSeats
            .Include(x => x.Booking)
            .Where(x => request.SeatIds.Contains(x.SeatId) && x.Booking!.BookingStatus == "Confirmed")
            .Select(x => x.SeatId)
            .ToListAsync();

        if (bookedSeatIds.Count > 0)
        {
            _logger.LogWarning("InitiateBooking blocked: seats already booked. PassengerId={PassengerId} BusId={BusId} ConflictingSeats={Count}", passengerId, request.BusId, bookedSeatIds.Count);
            throw new InvalidOperationException("One or more seats are already booked");
        }

        var booking = new Domain.Entities.Booking
        {
            PassengerId = passengerId,
            BusId = request.BusId,
            BookingStatus = "PendingPayment",
            TotalAmount = bus.TotalPrice * request.SeatIds.Count
        };

        _db.Bookings.Add(booking);
        foreach (var seat in seats)
        {
            _db.BookingSeats.Add(new Domain.Entities.BookingSeat
            {
                Booking = booking,
                SeatId = seat.Id,
                Fare = bus.TotalPrice
            });
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking created (pending payment). BookingId={BookingId} PassengerId={PassengerId} BusId={BusId} Amount={Amount}", booking.Id, passengerId, request.BusId, booking.TotalAmount);

        return new BookingResponse(booking.Id, booking.BookingStatus, booking.TotalAmount, booking.BookedAt);
    }

    public async Task<List<BookingResponse>> GetMyBookingsAsync(Guid passengerId)
    {
        _logger.LogInformation("GetMyBookings requested. PassengerId={PassengerId}", passengerId);
        return await _db.Bookings
            .Where(x => x.PassengerId == passengerId)
            .OrderByDescending(x => x.BookedAt)
            .Select(x => new BookingResponse(x.Id, x.BookingStatus, x.TotalAmount, x.BookedAt))
            .ToListAsync();
    }

    public async Task CancelBookingAsync(Guid passengerId, Guid bookingId, string reason)
    {
        _logger.LogInformation("CancelBooking requested. PassengerId={PassengerId} BookingId={BookingId}", passengerId, bookingId);
        var booking = await _db.Bookings.Include(x => x.Bus).FirstOrDefaultAsync(x => x.Id == bookingId && x.PassengerId == passengerId)
            ?? throw new InvalidOperationException("Booking not found");

        if (booking.BookingStatus != "Confirmed")
        {
            _logger.LogWarning("CancelBooking blocked: booking not confirmed. PassengerId={PassengerId} BookingId={BookingId} Status={Status}", passengerId, bookingId, booking.BookingStatus);
            throw new InvalidOperationException("Only confirmed bookings can be cancelled");
        }

        if (booking.Bus!.DepartureTime <= DateTime.UtcNow.AddHours(2))
        {
            _logger.LogWarning("CancelBooking blocked: cancellation window ended. PassengerId={PassengerId} BookingId={BookingId} DepartureTime={DepartureTime}", passengerId, bookingId, booking.Bus.DepartureTime);
            throw new InvalidOperationException("Cancellation window has ended");
        }

        booking.BookingStatus = "Cancelled";
        booking.CancelledAt = DateTime.UtcNow;
        booking.CancellationReason = reason;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking cancelled. PassengerId={PassengerId} BookingId={BookingId}", passengerId, bookingId);

        var passenger = await _db.Users.FirstAsync(x => x.Id == passengerId);
        await _emailService.SendAsync(passenger.Email, "Booking Cancelled", "Your booking has been cancelled.", "booking-cancelled", passengerId);
    }
}
