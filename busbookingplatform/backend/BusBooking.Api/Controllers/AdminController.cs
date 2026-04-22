using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
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

    public AdminController(IAdminService adminService, AppDbContext db)
    {
        _adminService = adminService;
        _db = db;
    }

    [HttpGet("operators/pending")]
    public async Task<ActionResult<List<OperatorReviewResponse>>> GetPendingOperators()
    {
        return Ok(await _adminService.GetPendingOperatorsAsync());
    }

    [HttpPost("operators/{operatorId:guid}/approve")]
    public async Task<IActionResult> ApproveOperator(Guid operatorId)
    {
        await _adminService.ApproveOperatorAsync(operatorId);
        return Ok();
    }

    [HttpPost("operators/{operatorId:guid}/reject")]
    public async Task<IActionResult> RejectOperator(Guid operatorId, OperatorStatusUpdateRequest request)
    {
        await _adminService.RejectOperatorAsync(operatorId, request.Reason);
        return Ok();
    }

    [HttpPost("operators/{operatorId:guid}/enable")]
    public async Task<IActionResult> EnableOperator(Guid operatorId)
    {
        await _adminService.EnableOperatorAsync(operatorId);
        return Ok();
    }

    [HttpPost("operators/{operatorId:guid}/disable")]
    public async Task<IActionResult> DisableOperator(Guid operatorId)
    {
        await _adminService.DisableOperatorAsync(operatorId);
        return Ok();
    }

    [HttpPost("buses/{busId:guid}/approve")]
    public async Task<IActionResult> ApproveBus(Guid busId)
    {
        await _adminService.ApproveBusAsync(busId);
        return Ok();
    }

    [HttpPost("buses/{busId:guid}/reject")]
    public async Task<IActionResult> RejectBus(Guid busId, OperatorStatusUpdateRequest request)
    {
        await _adminService.RejectBusAsync(busId, request.Reason);
        return Ok();
    }

    [HttpPost("sources")]
    public async Task<ActionResult<RouteResponse>> CreateSource(CreateSourceRequest request)
    {
        return Ok(await _adminService.CreateSourceAsync(request));
    }

    [HttpPost("destinations")]
    public async Task<ActionResult<RouteResponse>> CreateDestination(CreateDestinationRequest request)
    {
        return Ok(await _adminService.CreateDestinationAsync(request));
    }

    [HttpPost("routes")]
    public async Task<ActionResult<RouteResponse>> CreateRoute(CreateRouteRequest request)
    {
        return Ok(await _adminService.CreateRouteAsync(request));
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
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
        var buses = await _db.Buses
            .Include(x => x.Route).ThenInclude(r => r!.Source)
            .Include(x => x.Route).ThenInclude(r => r!.Destination)
            .Select(x => new
            {
                x.Id,
                x.BusName,
                x.ApprovalStatus,
                x.IsActive,
                x.IsTemporarilyDisabled,
                Source = x.Route!.Source!.Name,
                Destination = x.Route.Destination!.Name,
                x.DepartureTime,
                x.TotalPrice
            })
            .ToListAsync();

        return Ok(buses);
    }

    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookings()
    {
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
}
