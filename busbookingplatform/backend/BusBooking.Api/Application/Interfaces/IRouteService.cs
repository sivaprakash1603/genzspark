using BusBooking.Api.Application.DTOs;

namespace BusBooking.Api.Application.Interfaces;

public interface IRouteService
{
    Task<List<RouteResponse>> GetAllRoutesAsync();
}
