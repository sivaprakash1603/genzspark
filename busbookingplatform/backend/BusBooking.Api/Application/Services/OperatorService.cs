using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Application.Services;

public class OperatorService : IOperatorService
{
    private readonly AppDbContext _db;
    private readonly ILogger<OperatorService> _logger;

    public OperatorService(AppDbContext db, ILogger<OperatorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task AddBusAsync(Guid operatorUserId, AddBusRequest request)
    {
        _logger.LogInformation("AddBus requested. OperatorId={OperatorId} RouteId={RouteId} BusName={BusName} TotalSeats={TotalSeats}", operatorUserId, request.RouteId, request.BusName, request.TotalSeats);
        var profile = await _db.OperatorProfiles.FirstAsync(x => x.UserId == operatorUserId);
        if (profile.ApprovalStatus != "Approved" || !profile.IsEnabled)
        {
            _logger.LogWarning("AddBus blocked: operator not approved/enabled. OperatorId={OperatorId} Approval={Approval} Enabled={Enabled}", operatorUserId, profile.ApprovalStatus, profile.IsEnabled);
            throw new InvalidOperationException("Operator is not approved or enabled");
        }

        var platformFee = decimal.TryParse(Environment.GetEnvironmentVariable("PLATFORM_FEE_DEFAULT"), out var fee)
            ? fee
            : 20m;

        var bus = new Domain.Entities.Bus
        {
            OperatorId = operatorUserId,
            RouteId = request.RouteId,
            BusName = request.BusName,
            BoardingPoint = request.BoardingPoint,
            DropPoint = request.DropPoint,
            DepartureTime = request.DepartureTime,
            DurationMinutes = request.DurationMinutes,
            SeatLayoutType = request.SeatLayoutType,
            TotalSeats = request.TotalSeats,
            BasePrice = request.BasePrice,
            PlatformFee = platformFee,
            TotalPrice = request.BasePrice + platformFee,
            ApprovalStatus = "Pending"
        };

        _db.Buses.Add(bus);

        for (var i = 1; i <= request.TotalSeats; i++)
        {
            _db.Seats.Add(new Domain.Entities.Seat
            {
                Bus = bus,
                SeatNumber = i.ToString("D2")
            });
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Bus created (pending approval). BusId={BusId} OperatorId={OperatorId} TotalPrice={TotalPrice}", bus.Id, operatorUserId, bus.TotalPrice);
    }

    public async Task DisableBusTemporarilyAsync(Guid operatorUserId, Guid busId)
    {
        _logger.LogInformation("DisableBusTemporarily requested. OperatorId={OperatorId} BusId={BusId}", operatorUserId, busId);
        var bus = await ValidateOperatorBusAsync(operatorUserId, busId);
        bus.IsTemporarilyDisabled = true;
        await _db.SaveChangesAsync();
    }

    public async Task EnableBusTemporarilyAsync(Guid operatorUserId, Guid busId)
    {
        _logger.LogInformation("EnableBusTemporarily requested. OperatorId={OperatorId} BusId={BusId}", operatorUserId, busId);
        var bus = await ValidateOperatorBusAsync(operatorUserId, busId);
        bus.IsTemporarilyDisabled = false;
        await _db.SaveChangesAsync();
    }

    public async Task RemoveBusAsync(Guid operatorUserId, Guid busId)
    {
        _logger.LogInformation("RemoveBus requested. OperatorId={OperatorId} BusId={BusId}", operatorUserId, busId);
        var bus = await ValidateOperatorBusAsync(operatorUserId, busId);
        _db.Buses.Remove(bus);
        await _db.SaveChangesAsync();
    }

    public async Task<List<OperatorBookingResponse>> GetBookingsAsync(Guid operatorUserId)
    {
        _logger.LogInformation("GetOperatorBookings requested. OperatorId={OperatorId}", operatorUserId);
        return await _db.Bookings
            .Include(x => x.Bus)
            .Where(x => x.Bus!.OperatorId == operatorUserId)
            .OrderByDescending(x => x.BookedAt)
            .Select(x => new OperatorBookingResponse(x.Id, x.BusId, x.BookingStatus, x.TotalAmount, x.BookedAt))
            .ToListAsync();
    }

    public async Task<OperatorRevenueResponse> GetRevenueAsync(Guid operatorUserId)
    {
        _logger.LogInformation("GetOperatorRevenue requested. OperatorId={OperatorId}", operatorUserId);
        var confirmedBookings = await _db.Bookings
            .Include(x => x.Bus)
            .Where(x => x.Bus!.OperatorId == operatorUserId && x.BookingStatus == "Confirmed")
            .ToListAsync();

        return new OperatorRevenueResponse(confirmedBookings.Sum(x => x.TotalAmount), confirmedBookings.Count);
    }

    private async Task<Domain.Entities.Bus> ValidateOperatorBusAsync(Guid operatorUserId, Guid busId)
    {
        var bus = await _db.Buses.FirstOrDefaultAsync(x => x.Id == busId && x.OperatorId == operatorUserId)
            ?? throw new InvalidOperationException("Bus not found");
        return bus;
    }
}
