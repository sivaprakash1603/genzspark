using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Controllers;

[ApiController]
[Route("api/routes")]
public class RoutesController : ControllerBase
{
    private readonly IRouteService _routeService;

    public RoutesController(IRouteService routeService)
    {
        _routeService = routeService;
    }

    [HttpGet]
    public async Task<ActionResult<List<RouteResponse>>> GetAll()
    {
        return Ok(await _routeService.GetAllRoutesAsync());
    }
}
