
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
                x.VehicleNumber,
                x.ApprovalStatus,
                x.IsEnabled))
            .ToListAsync();
    }

    public async Task ApproveOperatorAsync(Guid operatorId)
    {
        _logger.LogInformation("ApproveOperator requested. OperatorId={OperatorId}", operatorId);
        var profile = await _db.OperatorProfiles.Include(x => x.User).FirstAsync(x => x.UserId == operatorId);
        profile.ApprovalStatus = "Approved";
        profile.IsEnabled = true;
        profile.ApprovedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Operator approved. OperatorId={OperatorId} Email={Email}", operatorId, profile.User!.Email);

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

    public async Task DisableOperatorAsync(Guid operatorId)
    {
        _logger.LogInformation("DisableOperator requested. OperatorId={OperatorId}", operatorId);
        var profile = await _db.OperatorProfiles.Include(x => x.User).FirstAsync(x => x.UserId == operatorId);
        profile.IsEnabled = false;
        await _db.SaveChangesAsync();

        _logger.LogInformation("Operator disabled. OperatorId={OperatorId}", operatorId);
        await _emailService.SendAsync(profile.User!.Email, "Operator Disabled", "Your account has been disabled.", "operator-disabled", operatorId);
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

    public async Task<RouteResponse> CreateRouteAsync(CreateRouteRequest request)
    {
        _logger.LogInformation("CreateRoute requested. SourceId={SourceId} DestinationId={DestinationId}", request.SourceId, request.DestinationId);
        var source = await _db.Sources.FirstAsync(x => x.Id == request.SourceId);
        var destination = await _db.Destinations.FirstAsync(x => x.Id == request.DestinationId);

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

    public async Task<RouteResponse> CreateSourceAsync(CreateSourceRequest request)
    {
        _logger.LogInformation("CreateSource requested. Name={Name}", request.Name);
        var source = new Domain.Entities.SourceLocation { Name = request.Name };
        _db.Sources.Add(source);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Source created. SourceId={SourceId} Name={Name}", source.Id, source.Name);
        return new RouteResponse(source.Id, source.Name, string.Empty);
    }

    public async Task<RouteResponse> CreateDestinationAsync(CreateDestinationRequest request)
    {
        _logger.LogInformation("CreateDestination requested. Name={Name}", request.Name);
        var destination = new Domain.Entities.DestinationLocation { Name = request.Name };
        _db.Destinations.Add(destination);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Destination created. DestinationId={DestinationId} Name={Name}", destination.Id, destination.Name);
        return new RouteResponse(destination.Id, string.Empty, destination.Name);
    }
}
