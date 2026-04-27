using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = "Passenger")]
[Route("api/payments")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPaymentService paymentService, ILogger<PaymentController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost("process")]
    public async Task<ActionResult<PaymentResponse>> Process(ProcessPaymentRequest request)
    {
        _logger.LogInformation("Process payment requested. UserId={UserId} BookingId={BookingId}", User.GetUserId(), request.BookingId);
        return Ok(await _paymentService.ProcessAsync(User.GetUserId(), request));
    }
}
