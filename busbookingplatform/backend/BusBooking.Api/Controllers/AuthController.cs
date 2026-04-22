using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register/passenger")]
    public async Task<ActionResult<AuthResponse>> RegisterPassenger(RegisterPassengerRequest request)
    {
        return Ok(await _authService.RegisterPassengerAsync(request));
    }

    [HttpPost("register/operator")]
    public async Task<ActionResult<AuthResponse>> RegisterOperator(RegisterOperatorRequest request)
    {
        return Ok(await _authService.RegisterOperatorAsync(request));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        return Ok(await _authService.LoginAsync(request));
    }
}
