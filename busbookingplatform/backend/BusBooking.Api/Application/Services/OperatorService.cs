using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using BusBooking.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Application.Services;

internal class OperatorService : IOperatorService
{
    private readonly AppDbContext _db;
    private readonly ILogger<OperatorService> _logger;

    public OperatorService(AppDbContext db, ILogger<OperatorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task AddVehicleAsync(Guid operatorUserId, AddVehicleRequest request)
    {
        _logger.LogInformation("AddVehicle requested. OperatorId={OperatorId} VehicleNumber={VehicleNumber}", operatorUserId, request.VehicleNumber);
        await ValidateOperatorProfileAsync(operatorUserId);

        var vehicle = new Domain.Entities.Vehicle { OperatorId = operatorUserId, VehicleNumber = request.VehicleNumber, BusName = request.BusName, SeatLayoutType = request.SeatLayoutType, TotalSeats = request.TotalSeats };
        _db.Vehicles.Add(vehicle);

        GenerateSeatsForVehicle(vehicle, request.TotalSeats);

        await _db.SaveChangesAsync();
        _logger.LogInformation("Vehicle created. VehicleId={VehicleId}", vehicle.Id);
    }

    private async Task ValidateOperatorProfileAsync(Guid operatorUserId)
    {
        var profile = await _db.OperatorProfiles.FirstAsync(x => x.UserId == operatorUserId);
        if (profile.ApprovalStatus != ApprovalStatus.Approved || !profile.IsEnabled)
        {
            throw new InvalidOperationException("Operator is not approved or enabled");
        }
    }

    private void GenerateSeatsForVehicle(Domain.Entities.Vehicle vehicle, int totalSeats)
    {
        for (var i = 1; i <= totalSeats; i++)
        {
            _db.Seats.Add(new Domain.Entities.Seat { Vehicle = vehicle, SeatNumber = i.ToString("D2") });
        }
    }

    public async Task<List<VehicleResponse>> GetMyVehiclesAsync(Guid operatorUserId)
    {
        return await _db.Vehicles
            .Where(x => x.OperatorId == operatorUserId)
            .Select(x => new VehicleResponse(x.Id, x.VehicleNumber, x.BusName, x.SeatLayoutType, x.TotalSeats, x.IsActive))
            .ToListAsync();
    }

    public async Task AddBusAsync(Guid operatorUserId, AddBusRequest request)
    {
        _logger.LogInformation("AddBus requested. OperatorId={OperatorId} RouteId={RouteId}", operatorUserId, request.RouteId);
        await ValidateOperatorProfileAsync(operatorUserId);

        var vehicle = await GetVehicleForOperatorAsync(operatorUserId, request.VehicleId);
        var route = await GetRouteWithLocationsAsync(request.RouteId);
        var boardingOffice = await GetOperatorOfficeForCityAsync(operatorUserId, route.Source!.Name);
        var dropOffice = await GetOperatorOfficeForCityAsync(operatorUserId, route.Destination!.Name);
        var platformFee = await ResolvePlatformFeeAsync(request.BasePrice);

        var bus = CreateBusEntity(operatorUserId, request, boardingOffice.Address, dropOffice.Address, platformFee);

        _db.Buses.Add(bus);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Bus created (pending approval). BusId={BusId} OperatorId={OperatorId} TotalPrice={TotalPrice}", bus.Id, operatorUserId, bus.TotalPrice);
    }

    private async Task<Domain.Entities.Vehicle> GetVehicleForOperatorAsync(Guid operatorUserId, Guid vehicleId)
    {
        return await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId && x.OperatorId == operatorUserId) ?? throw new InvalidOperationException("Vehicle not found");
    }

    private async Task<Domain.Entities.Route> GetRouteWithLocationsAsync(Guid routeId)
    {
        return await _db.Routes.Include(x => x.Source).Include(x => x.Destination).FirstOrDefaultAsync(x => x.Id == routeId) ?? throw new InvalidOperationException("Route not found");
    }

    private async Task<Domain.Entities.OperatorOffice> GetOperatorOfficeForCityAsync(Guid operatorUserId, string cityName)
    {
        return await _db.OperatorOffices.FirstOrDefaultAsync(x => x.OperatorId == operatorUserId && x.CityName == cityName) ?? throw new InvalidOperationException($"No office found for city: {cityName}. Please add an office first.");
    }

    private Domain.Entities.Bus CreateBusEntity(Guid operatorUserId, AddBusRequest request, string boardingPoint, string dropPoint, decimal platformFee)
    {
        return new Domain.Entities.Bus
        {
            OperatorId = operatorUserId, VehicleId = request.VehicleId, RouteId = request.RouteId, BoardingPoint = boardingPoint, DropPoint = dropPoint,
            DepartureTime = request.DepartureTime, AvailableDays = request.AvailableDays, DurationMinutes = request.DurationMinutes, BasePrice = request.BasePrice,
            PlatformFee = platformFee, TotalPrice = request.BasePrice + platformFee, ApprovalStatus = ApprovalStatus.Pending
        };
    }

    public async Task UpdateBusAsync(Guid operatorUserId, Guid busId, UpdateBusRequest request)
    {
        _logger.LogInformation("UpdateBus requested. OperatorId={OperatorId} BusId={BusId}", operatorUserId, busId);
        var bus = await ValidateOperatorBusAsync(operatorUserId, busId);

        bus.DepartureTime = request.DepartureTime;
        bus.AvailableDays = request.AvailableDays;
        bus.DurationMinutes = request.DurationMinutes;
        bus.BasePrice = request.BasePrice;
        bus.TotalPrice = request.BasePrice + bus.PlatformFee;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Bus updated. BusId={BusId} OperatorId={OperatorId} TotalPrice={TotalPrice}", busId, operatorUserId, bus.TotalPrice);
    }

    public async Task<List<OperatorBusResponse>> GetMyBusesAsync(Guid operatorUserId)
    {
        _logger.LogInformation("GetMyBuses requested. OperatorId={OperatorId}", operatorUserId);
        return await _db.Buses
            .Include(x => x.Vehicle)
            .Include(x => x.Route)
            .ThenInclude(x => x!.Source)
            .Include(x => x.Route)
            .ThenInclude(x => x!.Destination)
            .Where(x => x.OperatorId == operatorUserId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new OperatorBusResponse(
                x.Id,
                x.Vehicle!.BusName,
                x.ApprovalStatus.ToString(),
                x.IsActive,
                x.IsTemporarilyDisabled,
                x.RouteId,
                x.Route!.Source!.Name,
                x.Route.Destination!.Name,
                x.BoardingPoint,
                x.DropPoint,
                x.DepartureTime,
                x.AvailableDays,
                x.DurationMinutes,
                x.Vehicle.SeatLayoutType,
                x.Vehicle.TotalSeats,
                x.BasePrice,
                x.PlatformFee,
                x.TotalPrice,
                x.MaintenanceStart,
                x.MaintenanceEnd,
                x.IsMarkedForRemoval,
                x.RetirementDate,
                x.CreatedAt))
            .ToListAsync();
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
        var futureBookings = await GetFutureConfirmedBookingsAsync(busId);

        if (futureBookings.Any())
        {
            MarkBusForFutureRemoval(bus, futureBookings.First().JourneyDate);
        }
        else
        {
            _db.Buses.Remove(bus);
        }
        await _db.SaveChangesAsync();
    }

    private async Task<List<Domain.Entities.Booking>> GetFutureConfirmedBookingsAsync(Guid busId)
    {
        return await _db.Bookings
            .Where(x => x.BusId == busId && x.BookingStatus == BookingStatus.Confirmed && x.JourneyDate >= DateTime.UtcNow.Date)
            .OrderByDescending(x => x.JourneyDate).ToListAsync();
    }

    private void MarkBusForFutureRemoval(Domain.Entities.Bus bus, DateTime lastDate)
    {
        _logger.LogWarning("Cannot remove bus with active future bookings. Marking for removal after last journey. LastDate={LastDate}", lastDate);
        bus.IsMarkedForRemoval = true;
        bus.RetirementDate = lastDate;
    }

    public async Task<List<OperatorBookingResponse>> GetBookingsAsync(Guid operatorUserId)
    {
        _logger.LogInformation("GetOperatorBookings requested. OperatorId={OperatorId}", operatorUserId);
        return await _db.Bookings
            .Include(x => x.Bus).ThenInclude(b => b!.Vehicle)
            .Include(x => x.Bus).ThenInclude(b => b!.Route).ThenInclude(r => r!.Source)
            .Include(x => x.Bus).ThenInclude(b => b!.Route).ThenInclude(r => r!.Destination)
            .Include(x => x.Passenger)
            .Where(x => x.Bus!.OperatorId == operatorUserId)
            .OrderByDescending(x => x.BookedAt)
            .Select(x => new OperatorBookingResponse(
                x.Id, 
                x.BusId, 
                x.Bus!.Vehicle!.BusName,
                $"{x.Bus.Route!.Source!.Name} ➔ {x.Bus.Route.Destination!.Name}",
                x.Passenger!.Username,
                x.BookingStatus.ToString(), 
                x.TotalAmount, 
                x.JourneyDate,
                x.BookedAt))
            .ToListAsync();
    }

    public async Task<OperatorRevenueResponse> GetRevenueAsync(Guid operatorUserId)
    {
        _logger.LogInformation("GetOperatorRevenue requested. OperatorId={OperatorId}", operatorUserId);
        
        var bookings = await GetOperatorBookingsForRevenueAsync(operatorUserId);
        var refunds = await GetRefundsForBookingsAsync(bookings.Select(b => b.Id).ToList());

        var totalRevenue = CalculateNetRevenue(bookings, refunds);

        return new OperatorRevenueResponse(totalRevenue, bookings.Count(x => x.BookingStatus == BookingStatus.Confirmed));
    }

    private async Task<List<Domain.Entities.Booking>> GetOperatorBookingsForRevenueAsync(Guid operatorUserId)
    {
        return await _db.Bookings.Include(x => x.Bus).Where(x => x.Bus!.OperatorId == operatorUserId)
            .Where(x => x.BookingStatus == BookingStatus.Confirmed || x.BookingStatus == BookingStatus.Cancelled).ToListAsync();
    }

    private async Task<Dictionary<Guid, decimal>> GetRefundsForBookingsAsync(List<Guid> bookingIds)
    {
        return await _db.Refunds.Where(r => bookingIds.Contains(r.BookingId)).ToDictionaryAsync(r => r.BookingId, r => r.RefundAmount);
    }

    private decimal CalculateNetRevenue(List<Domain.Entities.Booking> bookings, Dictionary<Guid, decimal> refunds)
    {
        decimal totalRevenue = 0;
        foreach (var b in bookings)
        {
            if (b.BookingStatus == BookingStatus.Confirmed) totalRevenue += b.TotalAmount;
            else if (b.BookingStatus == BookingStatus.Cancelled) totalRevenue += (b.TotalAmount - refunds.GetValueOrDefault(b.Id, 0m));
        }
        return totalRevenue;
    }

    public async Task<List<OperatorOfficeResponse>> GetMyOfficesAsync(Guid operatorUserId)
    {
        _logger.LogInformation("GetMyOffices requested. OperatorId={OperatorId}", operatorUserId);
        return await _db.OperatorOffices
            .Where(x => x.OperatorId == operatorUserId)
            .Select(x => new OperatorOfficeResponse(x.Id, x.CityName, x.Address))
            .ToListAsync();
    }

    public async Task AddOfficeAsync(Guid operatorUserId, AddOfficeRequest request)
    {
        _logger.LogInformation("AddOffice requested. OperatorId={OperatorId} City={City}", operatorUserId, request.CityName);

        var exists = await _db.OperatorOffices.AnyAsync(x => x.OperatorId == operatorUserId && x.CityName == request.CityName);
        if (exists)
        {
            throw new InvalidOperationException($"An office already exists for city: {request.CityName}");
        }

        var office = new Domain.Entities.OperatorOffice
        {
            OperatorId = operatorUserId,
            CityName = request.CityName,
            Address = request.Address
        };

        _db.OperatorOffices.Add(office);
        await _db.SaveChangesAsync();
    }

    public async Task ScheduleMaintenanceAsync(Guid operatorUserId, Guid busId, DateTime? start, DateTime? end)
    {
        _logger.LogInformation("ScheduleMaintenance requested. OperatorId={OperatorId} BusId={BusId}", operatorUserId, busId);
        var bus = await ValidateOperatorBusAsync(operatorUserId, busId);
        
        bus.MaintenanceStart = start?.Date;
        bus.MaintenanceEnd = end?.Date;
        
        if (start != null && end != null)
        {
             bus.MaintenanceStart = DateTime.SpecifyKind(start.Value.Date, DateTimeKind.Utc);
             bus.MaintenanceEnd = DateTime.SpecifyKind(end.Value.Date, DateTimeKind.Utc);
        }

        await _db.SaveChangesAsync();
    }

    public async Task RemoveVehicleAsync(Guid operatorUserId, Guid vehicleId)
    {
        _logger.LogInformation("RemoveVehicle requested. OperatorId={OperatorId} VehicleId={VehicleId}", operatorUserId, vehicleId);
        var vehicle = await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId && x.OperatorId == operatorUserId);
        if (vehicle == null) throw new KeyNotFoundException("Vehicle not found");

        var hasActiveSchedules = await _db.Buses.AnyAsync(x => x.VehicleId == vehicleId && x.IsActive && !x.IsMarkedForRemoval);
        if (hasActiveSchedules)
        {
            throw new InvalidOperationException("Cannot remove a vehicle that is currently assigned to active schedules. Please retire the schedules first.");
        }

        _db.Vehicles.Remove(vehicle);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveOfficeAsync(Guid operatorUserId, Guid officeId)
    {
        _logger.LogInformation("RemoveOffice requested. OperatorId={OperatorId} OfficeId={OfficeId}", operatorUserId, officeId);
        var office = await _db.OperatorOffices.FirstOrDefaultAsync(x => x.Id == officeId && x.OperatorId == operatorUserId);
        if (office == null) throw new KeyNotFoundException("Office not found");

        _db.OperatorOffices.Remove(office);
        await _db.SaveChangesAsync();
    }

    private async Task<Domain.Entities.Bus> ValidateOperatorBusAsync(Guid operatorUserId, Guid busId)
    {
        var bus = await _db.Buses.FirstOrDefaultAsync(x => x.Id == busId && x.OperatorId == operatorUserId)
            ?? throw new InvalidOperationException("Bus not found");
        return bus;
    }

    private async Task<decimal> ResolvePlatformFeeAsync(decimal basePrice)
    {
        var activeFee = await _db.PlatformFeeConfigs
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync();

        if (activeFee is not null)
        {
            return activeFee.FeeType.Equals("Percent", StringComparison.OrdinalIgnoreCase)
                ? Math.Round(basePrice * activeFee.Value / 100m, 2, MidpointRounding.AwayFromZero)
                : activeFee.Value;
        }

        if (decimal.TryParse(Environment.GetEnvironmentVariable("PLATFORM_FEE_DEFAULT"), out var envFee))
        {
            return envFee;
        }

        return 20m;
    }
}
