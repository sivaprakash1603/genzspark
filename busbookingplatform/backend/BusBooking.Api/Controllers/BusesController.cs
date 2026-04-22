using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Controllers;

[ApiController]
[Route("api/buses")]
public class BusesController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BusesController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<BusSearchResponse>>> Search([FromQuery] string source, [FromQuery] string destination)
    {
        return Ok(await _bookingService.SearchBusesAsync(source, destination));
    }

    [HttpGet("{busId:guid}/seats")]
    public async Task<ActionResult<List<SeatResponse>>> Seats(Guid busId)
    {
        return Ok(await _bookingService.GetSeatsAsync(busId));
    }
}
