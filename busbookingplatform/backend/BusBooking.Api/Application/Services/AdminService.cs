
using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Application.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<AdminService> _logger;

    public AdminService(AppDbContext db, IEmailService emailService, ILogger<AdminService> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<List<OperatorReviewResponse>> GetPendingOperatorsAsync()
    {
        _logger.LogInformation("GetPendingOperators requested");
        return await _db.OperatorProfiles
            .Include(x => x.User)
            .Where(x => x.ApprovalStatus == "Pending")
            .Select(x => new OperatorReviewResponse(
                x.UserId,
                x.User!.Username,
                x.User.Email,
                x.ApprovalStatus,
                x.IsEnabled))
            .ToListAsync();
    }

    public async Task ApproveOperatorAsync(Guid operatorId, Guid approvedByAdminId)
    {
        _logger.LogInformation("ApproveOperator requested. OperatorId={OperatorId} ApprovedBy={ApprovedBy}", operatorId, approvedByAdminId);
        var profile = await _db.OperatorProfiles.Include(x => x.User).FirstAsync(x => x.UserId == operatorId);
        profile.ApprovalStatus = "Approved";
        profile.IsEnabled = true;
        profile.ApprovedAt = DateTime.UtcNow;
        profile.ApprovedBy = approvedByAdminId;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Operator approved. OperatorId={OperatorId} ApprovedBy={ApprovedBy} Email={Email}", operatorId, approvedByAdminId, profile.User!.Email);

        await _emailService.SendAsync(profile.User!.Email, "Operator Approved", "Your operator account is approved.", "operator-approved", operatorId);
    }

    public async Task RejectOperatorAsync(Guid operatorId, string reason)
    {
        _logger.LogInformation("RejectOperator requested. OperatorId={OperatorId}", operatorId);
        var profile = await _db.OperatorProfiles.Include(x => x.User).FirstAsync(x => x.UserId == operatorId);
        profile.ApprovalStatus = "Rejected";
        profile.IsEnabled = false;
        profile.RejectionReason = reason;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Operator rejected. OperatorId={OperatorId} Reason={Reason}", operatorId, reason);

        await _emailService.SendAsync(profile.User!.Email, "Operator Rejected", $"Operator rejected: {reason}", "operator-rejected", operatorId);
    }

    public async Task EnableOperatorAsync(Guid operatorId)
    {
        _logger.LogInformation("EnableOperator requested. OperatorId={OperatorId}", operatorId);
        var profile = await _db.OperatorProfiles.Include(x => x.User).FirstAsync(x => x.UserId == operatorId);
        profile.IsEnabled = true;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Operator enabled. OperatorId={OperatorId}", operatorId);
        await _emailService.SendAsync(profile.User!.Email, "Operator Enabled", "Your account has been enabled.", "operator-enabled", operatorId);
    }

    public async Task DisableOperatorAsync(Guid operatorId, string reason)
    {
        _logger.LogInformation("DisableOperator requested. OperatorId={OperatorId} Reason={Reason}", operatorId, reason);
        var profile = await _db.OperatorProfiles.Include(x => x.User).FirstAsync(x => x.UserId == operatorId);
        profile.IsEnabled = false;
        profile.RejectionReason = reason;

        var buses = await _db.Buses.Where(x => x.OperatorId == operatorId && x.IsActive).ToListAsync();
        foreach (var bus in buses)
        {
            bus.IsActive = false;
            bus.IsTemporarilyDisabled = true;
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Operator disabled. OperatorId={OperatorId} DisabledBusCount={DisabledBusCount}", operatorId, buses.Count);
        await _emailService.SendAsync(profile.User!.Email, "Operator Disabled", $"Your account has been disabled. Reason: {reason}", "operator-disabled", operatorId);
    }

    public async Task ApproveBusAsync(Guid busId)
    {
        _logger.LogInformation("ApproveBus requested. BusId={BusId}", busId);
        var bus = await _db.Buses.Include(x => x.Operator).FirstAsync(x => x.Id == busId);
        bus.ApprovalStatus = "Approved";
        await _db.SaveChangesAsync();

        _logger.LogInformation("Bus approved. BusId={BusId} OperatorId={OperatorId}", busId, bus.OperatorId);

        if (bus.Operator is not null)
        {
            await _emailService.SendAsync(bus.Operator.Email, "Bus Approved", "Your bus is approved.", "bus-approved", bus.OperatorId);
        }
    }

    public async Task RejectBusAsync(Guid busId, string reason)
    {
        _logger.LogInformation("RejectBus requested. BusId={BusId}", busId);
        var bus = await _db.Buses.Include(x => x.Operator).FirstAsync(x => x.Id == busId);
        bus.ApprovalStatus = "Rejected";
        await _db.SaveChangesAsync();

        _logger.LogInformation("Bus rejected. BusId={BusId} Reason={Reason}", busId, reason);

        if (bus.Operator is not null)
        {
            await _emailService.SendAsync(bus.Operator.Email, "Bus Rejected", $"Bus rejected: {reason}", "bus-rejected", bus.OperatorId);
        }
    }

    public async Task<List<LocationResponse>> GetSourcesAsync()
    {
        _logger.LogInformation("GetSources requested");
        return await _db.Locations
            .OrderBy(x => x.Name)
            .Select(x => new LocationResponse(x.Id, x.Name))
            .ToListAsync();
    }

    public async Task<List<LocationResponse>> GetDestinationsAsync()
    {
        _logger.LogInformation("GetDestinations requested");
        return await _db.Locations
            .OrderBy(x => x.Name)
            .Select(x => new LocationResponse(x.Id, x.Name))
            .ToListAsync();
    }

    public async Task<List<RouteResponse>> GetRoutesAsync()
    {
        _logger.LogInformation("GetRoutes requested");
        return await _db.Routes
            .Include(x => x.Source)
            .Include(x => x.Destination)
            .OrderBy(x => x.Source!.Name)
            .ThenBy(x => x.Destination!.Name)
            .Select(x => new RouteResponse(x.Id, x.Source!.Name, x.Destination!.Name))
            .ToListAsync();
    }

    public async Task<PlatformFeeResponse> GetPlatformFeeAsync()
    {
        _logger.LogInformation("GetPlatformFee requested");
        var fee = await _db.PlatformFeeConfigs
            .Where(x => x.IsActive)
            .OrderByDescending(x => x.EffectiveFrom)
            .FirstOrDefaultAsync();

        if (fee is null)
        {
            throw new InvalidOperationException("Platform fee configuration is not set");
        }

        return new PlatformFeeResponse(fee.Id, fee.FeeType, fee.Value, fee.IsActive, fee.EffectiveFrom, fee.CreatedAt);
    }

    public async Task<PlatformFeeResponse> UpdatePlatformFeeAsync(UpdatePlatformFeeRequest request)
    {
        _logger.LogInformation("UpdatePlatformFee requested. FeeType={FeeType} Value={Value}", request.FeeType, request.Value);

        var activeFees = await _db.PlatformFeeConfigs.Where(x => x.IsActive).ToListAsync();
        foreach (var activeFee in activeFees)
        {
            activeFee.IsActive = false;
        }

        var fee = new Domain.Entities.PlatformFeeConfig
        {
            FeeType = request.FeeType,
            Value = request.Value,
            IsActive = true,
            EffectiveFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _db.PlatformFeeConfigs.Add(fee);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Platform fee updated. PlatformFeeConfigId={PlatformFeeConfigId} FeeType={FeeType} Value={Value}", fee.Id, fee.FeeType, fee.Value);
        return new PlatformFeeResponse(fee.Id, fee.FeeType, fee.Value, fee.IsActive, fee.EffectiveFrom, fee.CreatedAt);
    }

    public async Task<RouteResponse> CreateRouteAsync(CreateRouteRequest request)
    {
        _logger.LogInformation("CreateRoute requested. SourceId={SourceId} DestinationId={DestinationId}", request.SourceId, request.DestinationId);
        var source = await _db.Locations.FirstAsync(x => x.Id == request.SourceId);
        var destination = await _db.Locations.FirstAsync(x => x.Id == request.DestinationId);

        var route = new Domain.Entities.Route
        {
            SourceId = request.SourceId,
            DestinationId = request.DestinationId
        };

        _db.Routes.Add(route);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Route created. RouteId={RouteId}", route.Id);

        return new RouteResponse(route.Id, source.Name, destination.Name);
    }

    public async Task<RouteResponse> CreateRouteByNameAsync(CreateRouteByNameRequest request)
    {
        _logger.LogInformation("CreateRouteByName requested. Source={Source} Destination={Destination}", request.SourceName, request.DestinationName);

        var source = await _db.Locations.FirstOrDefaultAsync(x => x.Name.ToLower() == request.SourceName.ToLower());
        if (source == null)
        {
            source = new Domain.Entities.Location { Name = request.SourceName, IsActive = true };
            _db.Locations.Add(source);
        }

        var destination = await _db.Locations.FirstOrDefaultAsync(x => x.Name.ToLower() == request.DestinationName.ToLower());
        if (destination == null)
        {
            destination = new Domain.Entities.Location { Name = request.DestinationName, IsActive = true };
            _db.Locations.Add(destination);
        }

        await _db.SaveChangesAsync();

        var route = await _db.Routes.FirstOrDefaultAsync(x => x.SourceId == source.Id && x.DestinationId == destination.Id);
        if (route == null)
        {
            route = new Domain.Entities.Route
            {
                SourceId = source.Id,
                DestinationId = destination.Id
            };
            _db.Routes.Add(route);
            await _db.SaveChangesAsync();
        }

        _logger.LogInformation("Route created via name. RouteId={RouteId}", route.Id);
        return new RouteResponse(route.Id, source.Name, destination.Name);
    }

    public async Task<RouteResponse> CreateSourceAsync(CreateSourceRequest request)
    {
        _logger.LogInformation("CreateSource requested. Name={Name}", request.Name);
        var source = new Domain.Entities.Location { Name = request.Name };
        _db.Locations.Add(source);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Source created. LocationId={LocationId} Name={Name}", source.Id, source.Name);
        return new RouteResponse(source.Id, source.Name, string.Empty);
    }

    public async Task<RouteResponse> CreateDestinationAsync(CreateDestinationRequest request)
    {
        _logger.LogInformation("CreateDestination requested. Name={Name}", request.Name);
        var destination = new Domain.Entities.Location { Name = request.Name };
        _db.Locations.Add(destination);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Destination created. LocationId={LocationId} Name={Name}", destination.Id, destination.Name);
        return new RouteResponse(destination.Id, string.Empty, destination.Name);
    }

    public async Task<List<EmailLogResponse>> GetEmailLogsAsync()
    {
        _logger.LogInformation("GetEmailLogs requested");
        return await _db.EmailLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(100)
            .Select(x => new EmailLogResponse(
                x.Id,
                x.UserId,
                x.ToEmail,
                x.Subject,
                x.TemplateKey,
                x.Status,
                x.ErrorMessage,
                x.CreatedAt))
            .ToListAsync();
    }

    public async Task<AdminStatsResponse> GetAdminStatsAsync()
    {
        _logger.LogInformation("GetAdminStats requested");

        var totalRevenue = await _db.Payments
            .Where(x => x.PaymentStatus == "Success")
            .SumAsync(x => x.Amount);

        var totalRefunds = await _db.Refunds
            .Where(x => x.RefundStatus == "Processed" || x.RefundStatus == "Pending")
            .SumAsync(x => x.RefundAmount);

        var activeBuses = await _db.Buses.CountAsync(x => x.IsActive && x.ApprovalStatus == "Approved");
        var totalBookings = await _db.Bookings.CountAsync(x => x.BookingStatus == "Confirmed");
        var totalOperators = await _db.OperatorProfiles.CountAsync();
        var totalUsers = await _db.Users.CountAsync();

        // Top Routes by Revenue - Simplified for EF translation
        var topRoutesData = await _db.Bookings
            .Where(x => x.BookingStatus == "Confirmed" && x.Bus != null && x.Bus.Route != null && x.Bus.Route.Source != null && x.Bus.Route.Destination != null)
            .GroupBy(x => new { Source = x.Bus!.Route!.Source!.Name, Dest = x.Bus!.Route!.Destination!.Name })
            .Select(g => new {
                g.Key.Source,
                g.Key.Dest,
                Revenue = g.Sum(x => x.TotalAmount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .Take(5)
            .ToListAsync();

        var topRoutes = topRoutesData.Select(x => new RouteRevenueDto(
            $"{x.Source} ➔ {x.Dest}",
            x.Revenue,
            x.Count
        )).ToList();

        return new AdminStatsResponse(
            totalRevenue,
            totalRefunds,
            totalRevenue - totalRefunds,
            activeBuses,
            totalBookings,
            totalOperators,
            totalUsers,
            topRoutes
        );
    }
}
