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
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailService emailService, IServiceProvider sp, ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _db = sp.GetRequiredService<AppDbContext>();
        _logger = logger;
    }

    [HttpPost("test")]
    public async Task<IActionResult> SendTest([FromQuery] string to)
    {
        _logger.LogInformation("SendTest email requested. To={To}", to);
        await _emailService.SendAsync(to, "Test Email", "SMTP is configured.", "test-email", null);
        return Ok();
    }

    [HttpGet("logs")]
    public async Task<IActionResult> GetLogs()
    {
        _logger.LogInformation("Email logs requested");
        var logs = await _db.EmailLogs
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .ToListAsync();
        return Ok(logs);
    }
}
