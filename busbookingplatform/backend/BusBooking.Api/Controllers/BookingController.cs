using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace BusBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = "Passenger")]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ILogger<BookingController> _logger;

    public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
    {
        _bookingService = bookingService;
        _logger = logger;
    }

    [HttpPost("initiate")]
    public async Task<ActionResult<BookingResponse>> Initiate(InitiateBookingRequest request)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("Initiate booking requested. UserId={UserId} BusId={BusId} PassengerCount={PassengerCount}", userId, request.BusId, request.Passengers.Count);
        return Ok(await _bookingService.InitiateBookingAsync(userId, request));
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<BookingResponse>>> MyBookings([FromQuery] string? filter)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("MyBookings requested. UserId={UserId} Filter={Filter}", userId, filter);
        return Ok(await _bookingService.GetMyBookingsAsync(userId, filter));
    }

    [HttpPost("{bookingId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid bookingId, CancelBookingRequest request)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("Cancel booking requested. UserId={UserId} BookingId={BookingId}", userId, bookingId);
        await _bookingService.CancelBookingAsync(userId, bookingId, request.Reason);
        return Ok();
    }

    [HttpGet("{bookingId:guid}/ticket")]
    public async Task<IActionResult> DownloadTicket(Guid bookingId)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("Ticket download requested. UserId={UserId} BookingId={BookingId}", userId, bookingId);

        var bytes = await _bookingService.GetTicketPdfAsync(userId, bookingId);
        return File(bytes, "application/pdf", $"Ticket-{bookingId}.pdf");
    }

    [HttpPost("seats/{seatId:guid}/lock")]
    public async Task<IActionResult> LockSeat(Guid seatId, [FromQuery] DateTime journeyDate)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("LockSeat requested from controller. UserId={UserId} SeatId={SeatId}", userId, seatId);
        var locked = await _bookingService.LockSeatAsync(userId, seatId, journeyDate);
        return locked ? Ok() : Conflict(new { message = "Seat is unavailable" });
    }

    [HttpPost("seats/{seatId:guid}/unlock")]
    public async Task<IActionResult> UnlockSeat(Guid seatId)
    {
        var userId = User.GetUserId();
        _logger.LogInformation("UnlockSeat requested from controller. UserId={UserId} SeatId={SeatId}", userId, seatId);
        var unlocked = await _bookingService.UnlockSeatAsync(userId, seatId);
        return unlocked ? Ok() : Conflict(new { message = "Seat unlock failed" });
    }
}
