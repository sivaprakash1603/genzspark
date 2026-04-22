using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = "Passenger")]
[Route("api/bookings")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("initiate")]
    public async Task<ActionResult<BookingResponse>> Initiate(InitiateBookingRequest request)
    {
        return Ok(await _bookingService.InitiateBookingAsync(User.GetUserId(), request));
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<BookingResponse>>> MyBookings()
    {
        return Ok(await _bookingService.GetMyBookingsAsync(User.GetUserId()));
    }

    [HttpPost("{bookingId:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid bookingId, CancelBookingRequest request)
    {
        await _bookingService.CancelBookingAsync(User.GetUserId(), bookingId, request.Reason);
        return Ok();
    }
}
