using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/email")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly AppDbContext _db;

    public EmailController(IEmailService emailService, AppDbContext db)
    {
        _emailService = emailService;
        _db = db;
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTest([FromQuery] string to)
    {
        await _emailService.SendAsync(to, "Test Email", "SMTP is configured.", "test-email", null);
        return Ok();
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs()
    {
        var logs = await _db.EmailLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .ToListAsync();
        return Ok(logs);
    }
}
