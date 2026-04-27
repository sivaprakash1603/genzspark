using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Controllers;

[ApiController]
[Route("api/routes")]
public class RoutesController : ControllerBase
{
    private readonly IRouteService _routeService;
    private readonly ILogger<RoutesController> _logger;

    public RoutesController(IRouteService routeService, ILogger<RoutesController> logger)
    {
        _routeService = routeService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<RouteResponse>>> GetAll()
    {
        _logger.LogInformation("GetAll routes requested");
        return Ok(await _routeService.GetAllRoutesAsync());
    }
}
