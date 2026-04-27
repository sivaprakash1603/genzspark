using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Common;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Controllers;

[ApiController]
[Route("api/buses")]
public class BusesController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BusesController> _logger;

    public BusesController(IBookingService bookingService, ILogger<BusesController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<BusSearchResponse>>> Search([FromQuery] string source, [FromQuery] string destination, [FromQuery] DateTime date)
    {
        _logger.LogInformation("Bus search requested. Source={Source} Destination={Destination} Date={Date}", source, destination, date);
        var journeyDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        return Ok(await _bookingService.SearchBusesAsync(source, destination, journeyDate));
    }

    [HttpGet("locations")]
    public async Task<ActionResult<List<LocationDto>>> Locations([FromQuery] string query = "")
    {
        _logger.LogInformation("Locations autocomplete requested. Query={Query}", query);
        return Ok(await _bookingService.GetLocationsAsync(query));
    }

    [HttpGet("routes")]
    public async Task<ActionResult<List<RouteResponse>>> GetPublicRoutes()
    {
        _logger.LogInformation("Public routes list requested");
        return Ok(await _bookingService.GetPublicRoutesAsync());
    }

    [HttpGet("{busId:guid}/seats")]
    public async Task<ActionResult<List<SeatResponse>>> Seats(Guid busId, [FromQuery] DateTime journeyDate)
    {
        var userId = User?.Identity?.IsAuthenticated == true ? User.GetUserId() : (Guid?)null;
        var utcJourneyDate = DateTime.SpecifyKind(journeyDate.Date, DateTimeKind.Utc);
        _logger.LogInformation("Seat map requested. BusId={BusId} UserId={UserId}", busId, userId);
        return Ok(await _bookingService.GetSeatsAsync(busId, utcJourneyDate, userId));
    }
}
