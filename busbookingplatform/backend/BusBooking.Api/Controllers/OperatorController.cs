using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = "Operator")]
[Route("api/operator")]
public class OperatorController : ControllerBase
{
    private readonly IOperatorService _operatorService;

    public OperatorController(IOperatorService operatorService)
    {
        _operatorService = operatorService;
    }

    [HttpPost("buses")]
    public async Task<IActionResult> AddBus(AddBusRequest request)
    {
        await _operatorService.AddBusAsync(User.GetUserId(), request);
        return Ok();
    }

    [HttpDelete("buses/{busId:guid}")]
    public async Task<IActionResult> RemoveBus(Guid busId)
    {
        await _operatorService.RemoveBusAsync(User.GetUserId(), busId);
        return Ok();
    }

    [HttpPost("buses/{busId:guid}/disable-temporary")]
    public async Task<IActionResult> DisableTemporarily(Guid busId)
    {
        await _operatorService.DisableBusTemporarilyAsync(User.GetUserId(), busId);
        return Ok();
    }

    [HttpPost("buses/{busId:guid}/enable-temporary")]
    public async Task<IActionResult> EnableTemporarily(Guid busId)
    {
        await _operatorService.EnableBusTemporarilyAsync(User.GetUserId(), busId);
        return Ok();
    }

    [HttpGet("bookings")]
    public async Task<ActionResult<List<OperatorBookingResponse>>> Bookings()
    {
        return Ok(await _operatorService.GetBookingsAsync(User.GetUserId()));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<OperatorRevenueResponse>> Revenue()
    {
        return Ok(await _operatorService.GetRevenueAsync(User.GetUserId()));
    }
}
