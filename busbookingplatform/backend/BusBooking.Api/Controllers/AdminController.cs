using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using BusBooking.Api.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;
    private readonly AppDbContext _db;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, AppDbContext db, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _db = db;
        _logger = logger;
    }

    [HttpGet("operators/pending")]
    public async Task<ActionResult<List<OperatorReviewResponse>>> GetPendingOperators()
    {
        _logger.LogInformation("GetPendingOperators requested");
        return Ok(await _adminService.GetPendingOperatorsAsync());
    }

    [HttpPost("operators/{operatorId:guid}/approve")]
    public async Task<IActionResult> ApproveOperator(Guid operatorId)
    {
        var adminId = User.GetUserId();
        _logger.LogInformation("ApproveOperator requested. OperatorId={OperatorId} AdminId={AdminId}", operatorId, adminId);
        await _adminService.ApproveOperatorAsync(operatorId, adminId);
        return Ok();
    }

    [HttpPost("operators/{operatorId:guid}/reject")]
    public async Task<IActionResult> RejectOperator(Guid operatorId, OperatorStatusUpdateRequest request)
    {
        _logger.LogInformation("RejectOperator requested. OperatorId={OperatorId}", operatorId);
        await _adminService.RejectOperatorAsync(operatorId, request.Reason);
        return Ok();
    }

    [HttpPost("operators/{operatorId:guid}/enable")]
    public async Task<IActionResult> EnableOperator(Guid operatorId)
    {
        _logger.LogInformation("EnableOperator requested. OperatorId={OperatorId}", operatorId);
        await _adminService.EnableOperatorAsync(operatorId);
        return Ok();
    }

    [HttpPost("operators/{operatorId:guid}/disable")]
    public async Task<IActionResult> DisableOperator(Guid operatorId, OperatorStatusUpdateRequest request)
    {
        _logger.LogInformation("DisableOperator requested. OperatorId={OperatorId} Reason={Reason}", operatorId, request.Reason);
        await _adminService.DisableOperatorAsync(operatorId, request.Reason);
        return Ok();
    }

    [HttpPost("buses/{busId:guid}/approve")]
    public async Task<IActionResult> ApproveBus(Guid busId)
    {
        _logger.LogInformation("ApproveBus requested. BusId={BusId}", busId);
        await _adminService.ApproveBusAsync(busId);
        return Ok();
    }

    [HttpPost("buses/{busId:guid}/reject")]
    public async Task<IActionResult> RejectBus(Guid busId, OperatorStatusUpdateRequest request)
    {
        _logger.LogInformation("RejectBus requested. BusId={BusId}", busId);
        await _adminService.RejectBusAsync(busId, request.Reason);
        return Ok();
    }

    [HttpPost("sources")]
    public async Task<ActionResult<RouteResponse>> CreateSource(CreateSourceRequest request)
    {
        _logger.LogInformation("CreateSource requested. Name={Name}", request.Name);
        return Ok(await _adminService.CreateSourceAsync(request));
    }

    [HttpPost("destinations")]
    public async Task<ActionResult<RouteResponse>> CreateDestination(CreateDestinationRequest request)
    {
        _logger.LogInformation("CreateDestination requested. Name={Name}", request.Name);
        return Ok(await _adminService.CreateDestinationAsync(request));
    }

    [HttpPost("routes")]
    public async Task<ActionResult<RouteResponse>> CreateRoute(CreateRouteRequest request)
    {
        _logger.LogInformation("CreateRoute requested. SourceId={SourceId} DestinationId={DestinationId}", request.SourceId, request.DestinationId);
        return Ok(await _adminService.CreateRouteAsync(request));
    }

    [HttpPost("routes/quick")]
    public async Task<ActionResult<RouteResponse>> CreateRouteByName(CreateRouteByNameRequest request)
    {
        _logger.LogInformation("CreateRouteByName requested. Source={Source} Destination={Destination}", request.SourceName, request.DestinationName);
        return Ok(await _adminService.CreateRouteByNameAsync(request));
    }

    [HttpGet("sources")]
    public async Task<ActionResult<List<LocationResponse>>> GetSources()
    {
        _logger.LogInformation("GetSources requested");
        return Ok(await _adminService.GetSourcesAsync());
    }

    [HttpGet("destinations")]
    public async Task<ActionResult<List<LocationResponse>>> GetDestinations()
    {
        _logger.LogInformation("GetDestinations requested");
        return Ok(await _adminService.GetDestinationsAsync());
    }

    [HttpGet("routes")]
    public async Task<ActionResult<List<RouteResponse>>> GetRoutes()
    {
        _logger.LogInformation("GetRoutes requested");
        return Ok(await _adminService.GetRoutesAsync());
    }

    [HttpGet("platform-fee")]
    public async Task<ActionResult<PlatformFeeResponse>> GetPlatformFee()
    {
        _logger.LogInformation("GetPlatformFee requested");
        return Ok(await _adminService.GetPlatformFeeAsync());
    }

    [HttpPut("platform-fee")]
    public async Task<ActionResult<PlatformFeeResponse>> UpdatePlatformFee(UpdatePlatformFeeRequest request)
    {
        _logger.LogInformation("UpdatePlatformFee requested. FeeType={FeeType} Value={Value}", request.FeeType, request.Value);
        return Ok(await _adminService.UpdatePlatformFeeAsync(request));
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        _logger.LogInformation("GetUsers requested");
        var users = await _db.Users
            .Include(x => x.Role)
            .Select(x => new
            {
                x.Id,
                x.Username,
                x.Email,
                Role = x.Role!.Name,
                x.IsActive,
                x.CreatedAt
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpGet("buses")]
    public async Task<IActionResult> GetBuses()
    {
        _logger.LogInformation("GetBuses requested");
        var buses = await _db.Buses
            .Include(x => x.Vehicle)
            .Include(x => x.Route).ThenInclude(r => r!.Source)
            .Include(x => x.Route).ThenInclude(r => r!.Destination)
            .Select(x => new
            {
                x.Id,
                x.Vehicle!.BusName,
                x.ApprovalStatus,
                x.IsActive,
                x.IsTemporarilyDisabled,
                Source = x.Route!.Source!.Name,
                Destination = x.Route.Destination!.Name,
                x.DepartureTime,
                x.DurationMinutes,
                x.TotalPrice
            })
            .ToListAsync();

        return Ok(buses);
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookings()
    {
        _logger.LogInformation("GetBookings requested");
        var bookings = await _db.Bookings
            .Select(x => new
            {
                x.Id,
                x.PassengerId,
                x.BusId,
                x.BookingStatus,
                x.TotalAmount,
                x.BookedAt,
                x.CancelledAt
            })
            .ToListAsync();

        return Ok(bookings);
    }

    [HttpGet("system-logs")]
    public async Task<ActionResult<List<EmailLogResponse>>> GetSystemLogs()
    {
        _logger.LogInformation("GetSystemLogs requested");
        return Ok(await _adminService.GetEmailLogsAsync());
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsResponse>> GetStats()
    {
        _logger.LogInformation("GetStats requested");
        return Ok(await _adminService.GetAdminStatsAsync());
    }
}
