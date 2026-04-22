using BusBooking.Api.Domain.Entities;
using BusBooking.Api.Domain.Enums;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Infrastructure.Bootstrap;

public class AdminSeederHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminSeederHostedService> _logger;

    public AdminSeederHostedService(IServiceProvider serviceProvider, IConfiguration configuration, ILogger<AdminSeederHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin seeder starting");

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        _logger.LogInformation("Applying EF Core migrations (if any)");
        await db.Database.MigrateAsync(cancellationToken);

        _logger.LogInformation("Seeding roles (Passenger/Operator/Admin)");

        var roleNames = new[] { RoleNames.Passenger, RoleNames.Operator, RoleNames.Admin };
        foreach (var roleName in roleNames)
        {
            if (!await db.Roles.AnyAsync(x => x.Name == roleName, cancellationToken))
            {
                db.Roles.Add(new Role { Name = roleName });
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        var adminUsername = _configuration["ADMIN_USERNAME"];
        var adminPassword = _configuration["ADMIN_PASSWORD"];
        var adminEmail = _configuration["ADMIN_EMAIL"] ?? "admin@busbooking.local";

        if (string.IsNullOrWhiteSpace(adminUsername) || string.IsNullOrWhiteSpace(adminPassword))
        {
            _logger.LogWarning("ADMIN_USERNAME/ADMIN_PASSWORD not set; skipping admin creation");
            return;
        }

        var existingAdmin = await db.Users.FirstOrDefaultAsync(x => x.Username == adminUsername, cancellationToken);
        if (existingAdmin is not null)
        {
            _logger.LogInformation("Admin user already exists; skipping creation. Username={Username}", adminUsername);
            return;
        }

        var adminRole = await db.Roles.FirstAsync(x => x.Name == RoleNames.Admin, cancellationToken);
        var hasher = new PasswordHasher<User>();

        var admin = new User
        {
            Username = adminUsername,
            Email = adminEmail,
            RoleId = adminRole.Id,
            IsActive = true
        };
        admin.PasswordHash = hasher.HashPassword(admin, adminPassword);

        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Admin user created. Username={Username} Email={Email}", adminUsername, adminEmail);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
