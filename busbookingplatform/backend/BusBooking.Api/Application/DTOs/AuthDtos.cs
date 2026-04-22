namespace BusBooking.Api.Application.DTOs;

public record RegisterPassengerRequest(string Username, string Email, string Password);
public record RegisterOperatorRequest(string Username, string Email, string Password, string VehicleNumber);
public record LoginRequest(string Username, string Password);
public record AuthResponse(string Token, string Username, string Role);
