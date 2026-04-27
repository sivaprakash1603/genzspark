using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BusBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = "Operator")]
[Route("api/operator")]
public class OperatorController : ControllerBase
{
    private readonly IOperatorService _operatorService;
    private readonly ILogger<OperatorController> _logger;

    public OperatorController(IOperatorService operatorService, ILogger<OperatorController> logger)
    {
        _operatorService = operatorService;
        _logger = logger;
    }

    [HttpPost("vehicles")]
    public async Task<IActionResult> AddVehicle(AddVehicleRequest request)
    {
        _logger.LogInformation("AddVehicle requested. OperatorId={OperatorId}", User.GetUserId());
        await _operatorService.AddVehicleAsync(User.GetUserId(), request);
        return Ok();
    }

    [HttpGet("vehicles")]
    public async Task<ActionResult<List<VehicleResponse>>> MyVehicles()
    {
        _logger.LogInformation("MyVehicles requested. OperatorId={OperatorId}", User.GetUserId());
        return Ok(await _operatorService.GetMyVehiclesAsync(User.GetUserId()));
    }

    [HttpDelete("vehicles/{vehicleId}")]
    public async Task<IActionResult> RemoveVehicle(Guid vehicleId)
    {
        _logger.LogInformation("RemoveVehicle requested. OperatorId={OperatorId} VehicleId={VehicleId}", User.GetUserId(), vehicleId);
        await _operatorService.RemoveVehicleAsync(User.GetUserId(), vehicleId);
        return Ok();
    }

    [HttpPost("buses")]
    public async Task<IActionResult> AddBus(AddBusRequest request)
    {
        _logger.LogInformation("AddBus requested. OperatorId={OperatorId} RouteId={RouteId} VehicleId={VehicleId}", User.GetUserId(), request.RouteId, request.VehicleId);
        await _operatorService.AddBusAsync(User.GetUserId(), request);
        return Ok();
    }

    [HttpGet("buses")]
    public async Task<ActionResult<List<OperatorBusResponse>>> MyBuses()
    {
        _logger.LogInformation("MyBuses requested. OperatorId={OperatorId}", User.GetUserId());
        return Ok(await _operatorService.GetMyBusesAsync(User.GetUserId()));
    }

    [HttpPut("buses/{busId:guid}")]
    public async Task<IActionResult> UpdateBus(Guid busId, UpdateBusRequest request)
    {
        _logger.LogInformation("UpdateBus requested. OperatorId={OperatorId} BusId={BusId}", User.GetUserId(), busId);
        await _operatorService.UpdateBusAsync(User.GetUserId(), busId, request);
        return Ok();
    }

    [HttpDelete("buses/{busId:guid}")]
    public async Task<IActionResult> RemoveBus(Guid busId)
    {
        _logger.LogInformation("RemoveBus requested. OperatorId={OperatorId} BusId={BusId}", User.GetUserId(), busId);
        await _operatorService.RemoveBusAsync(User.GetUserId(), busId);
        return Ok();
    }

    [HttpPost("buses/{busId:guid}/disable-temporary")]
    public async Task<IActionResult> DisableTemporarily(Guid busId)
    {
        _logger.LogInformation("DisableTemporarily requested. OperatorId={OperatorId} BusId={BusId}", User.GetUserId(), busId);
        await _operatorService.DisableBusTemporarilyAsync(User.GetUserId(), busId);
        return Ok();
    }

    [HttpPost("buses/{busId:guid}/enable-temporary")]
    public async Task<IActionResult> EnableTemporarily(Guid busId)
    {
        _logger.LogInformation("EnableTemporarily requested. OperatorId={OperatorId} BusId={BusId}", User.GetUserId(), busId);
        await _operatorService.EnableBusTemporarilyAsync(User.GetUserId(), busId);
        return Ok();
    }

    [HttpPost("buses/{busId:guid}/maintenance")]
    public async Task<IActionResult> ScheduleMaintenance(Guid busId, [FromBody] ScheduleMaintenanceRequest request)
    {
        _logger.LogInformation("ScheduleMaintenance requested. OperatorId={OperatorId} BusId={BusId}", User.GetUserId(), busId);
        await _operatorService.ScheduleMaintenanceAsync(User.GetUserId(), busId, request.Start, request.End);
        return Ok();
    }

    [HttpGet("bookings")]
    public async Task<ActionResult<List<OperatorBookingResponse>>> Bookings()
    {
        _logger.LogInformation("Operator bookings requested. OperatorId={OperatorId}", User.GetUserId());
        return Ok(await _operatorService.GetBookingsAsync(User.GetUserId()));
    }

    [HttpGet("revenue")]
    public async Task<ActionResult<OperatorRevenueResponse>> Revenue()
    {
        _logger.LogInformation("Operator revenue requested. OperatorId={OperatorId}", User.GetUserId());
        return Ok(await _operatorService.GetRevenueAsync(User.GetUserId()));
    }

    [HttpGet("offices")]
    public async Task<ActionResult<List<OperatorOfficeResponse>>> Offices()
    {
        _logger.LogInformation("Operator offices requested. OperatorId={OperatorId}", User.GetUserId());
        return Ok(await _operatorService.GetMyOfficesAsync(User.GetUserId()));
    }

    [HttpPost("offices")]
    public async Task<IActionResult> AddOffice([FromBody] AddOfficeRequest request)
    {
        await _operatorService.AddOfficeAsync(User.GetUserId(), request);
        return Ok();
    }

    [HttpDelete("offices/{officeId}")]
    public async Task<IActionResult> RemoveOffice(Guid officeId)
    {
        _logger.LogInformation("RemoveOffice requested. OperatorId={OperatorId} OfficeId={OfficeId}", User.GetUserId(), officeId);
        await _operatorService.RemoveOfficeAsync(User.GetUserId(), officeId);
        return Ok();
    }
}
