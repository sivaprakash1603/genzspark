using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register/passenger")]
    public async Task<ActionResult<AuthResponse>> RegisterPassenger(RegisterPassengerRequest request)
    {
        _logger.LogInformation("RegisterPassenger requested. Username={Username}", request.Username);
        return Ok(await _authService.RegisterPassengerAsync(request));
    }

    [HttpPost("register/operator")]
    public async Task<ActionResult<AuthResponse>> RegisterOperator(RegisterOperatorRequest request)
    {
        _logger.LogInformation("RegisterOperator requested. Username={Username}", request.Username);
        return Ok(await _authService.RegisterOperatorAsync(request));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        _logger.LogInformation("Login requested. Username={Username}", request.Username);
        return Ok(await _authService.LoginAsync(request));
    }
}
