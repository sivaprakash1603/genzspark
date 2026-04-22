using BusBooking.Api.Application.DTOs;

namespace BusBooking.Api.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterPassengerAsync(RegisterPassengerRequest request);
    Task<AuthResponse> RegisterOperatorAsync(RegisterOperatorRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}
