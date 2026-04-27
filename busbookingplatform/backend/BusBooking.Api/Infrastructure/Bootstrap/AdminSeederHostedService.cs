using BusBooking.Api.Domain.Entities;
using BusBooking.Api.Domain.Enums;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Infrastructure.Bootstrap;

public class AdminSeederHostedService : IHostedService
{
    private static readonly string[] DefaultSources =
    [
        "Bangalore",
        "Chennai",
        "Hyderabad",
        "Mumbai",
        "Pune"
    ];

    private static readonly string[] DefaultDestinations =
    [
        "Bangalore",
        "Chennai",
        "Hyderabad",
        "Mumbai",
        "Pune"
    ];

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

        await SeedLocationsAsync(db, cancellationToken);
        await SeedPlatformFeeAsync(db, cancellationToken);

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

    private async Task SeedLocationsAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var configuredSources = _configuration.GetSection("SeedData:Sources").Get<string[]>();
        var configuredDestinations = _configuration.GetSection("SeedData:Destinations").Get<string[]>();

        var locationsToSeed = (configuredSources ?? Array.Empty<string>())
            .Concat(configuredDestinations ?? Array.Empty<string>())
            .Concat(DefaultSources)
            .Concat(DefaultDestinations)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var existingLocations = await db.Locations
            .Select(x => x.Name)
            .ToListAsync(cancellationToken);

        foreach (var name in locationsToSeed.Where(x => !existingLocations.Contains(x, StringComparer.OrdinalIgnoreCase)))
        {
            db.Locations.Add(new Location { Name = name, IsActive = true });
        }

        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Location seeding completed. UniqueLocations={Count}", locationsToSeed.Count);
    }

    private async Task SeedPlatformFeeAsync(AppDbContext db, CancellationToken cancellationToken)
    {
        var activeFee = await db.PlatformFeeConfigs.FirstOrDefaultAsync(x => x.IsActive, cancellationToken);
        
        if (activeFee != null)
        {
            if (activeFee.Value != 30m)
            {
                activeFee.Value = 30m;
                await db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Existing platform fee updated to 30.");
            }
            return;
        }

        db.PlatformFeeConfigs.Add(new PlatformFeeConfig
        {
            FeeType = "Fixed",
            Value = 30m,
            IsActive = true,
            EffectiveFrom = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Default platform fee seeded at 30.");
    }
}
