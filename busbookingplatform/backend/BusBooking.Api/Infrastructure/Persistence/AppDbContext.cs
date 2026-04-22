using BusBooking.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

using RouteEntity = BusBooking.Api.Domain.Entities.Route;

namespace BusBooking.Api.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<OperatorProfile> OperatorProfiles => Set<OperatorProfile>();
    public DbSet<SourceLocation> Sources => Set<SourceLocation>();
    public DbSet<DestinationLocation> Destinations => Set<DestinationLocation>();
    public DbSet<RouteEntity> Routes => Set<RouteEntity>();
    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>().HasIndex(x => x.Name).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Username).IsUnique();
        modelBuilder.Entity<User>().HasIndex(x => x.Email).IsUnique();

        modelBuilder.Entity<OperatorProfile>()
            .HasOne(x => x.User)
            .WithOne()
            .HasForeignKey<OperatorProfile>(x => x.UserId);

        modelBuilder.Entity<RouteEntity>()
            .HasIndex(x => new { x.SourceId, x.DestinationId })
            .IsUnique();

        modelBuilder.Entity<Seat>()
            .HasIndex(x => new { x.BusId, x.SeatNumber })
            .IsUnique();

        modelBuilder.Entity<BookingSeat>()
            .HasIndex(x => new { x.BookingId, x.SeatId })
            .IsUnique();

        modelBuilder.Entity<Payment>()
            .HasIndex(x => x.TransactionId)
            .IsUnique();
    }
}
