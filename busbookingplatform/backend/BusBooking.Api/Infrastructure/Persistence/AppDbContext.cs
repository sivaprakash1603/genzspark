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
    public DbSet<OperatorOffice> OperatorOffices => Set<OperatorOffice>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<RouteEntity> Routes => Set<RouteEntity>();
    public DbSet<Bus> Buses => Set<Bus>();
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    public DbSet<BookingPassenger> BookingPassengers => Set<BookingPassenger>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Refund> Refunds => Set<Refund>();
    public DbSet<PlatformFeeConfig> PlatformFeeConfigs => Set<PlatformFeeConfig>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Roles
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.Id).HasName("PK_Roles");
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_Roles_Name");
        });

        // Users
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id).HasName("PK_Users");
            entity.HasIndex(e => e.Username).IsUnique().HasDatabaseName("UQ_Users_Username");
            entity.HasIndex(e => e.Email).IsUnique().HasDatabaseName("UQ_Users_Email");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Users_Roles_RoleId");
        });

        // Locations
        modelBuilder.Entity<Location>(entity =>
        {
            entity.ToTable("Locations");
            entity.HasKey(e => e.Id).HasName("PK_Locations");
            entity.HasIndex(e => e.Name).IsUnique().HasDatabaseName("UQ_Locations_Name");
        });

        // Routes
        modelBuilder.Entity<RouteEntity>(entity =>
        {
            entity.ToTable("Routes");
            entity.HasKey(e => e.Id).HasName("PK_Routes");
            entity.HasIndex(e => new { e.SourceId, e.DestinationId }).IsUnique().HasDatabaseName("UQ_Routes_Source_Destination");

            entity.HasOne(d => d.Source)
                .WithMany(p => p.SourceRoutes)
                .HasForeignKey(d => d.SourceId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Routes_Locations_SourceId");

            entity.HasOne(d => d.Destination)
                .WithMany(p => p.DestinationRoutes)
                .HasForeignKey(d => d.DestinationId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Routes_Locations_DestinationId");
        });

        // Vehicles
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicles");
            entity.HasKey(e => e.Id).HasName("PK_Vehicles");
            entity.HasIndex(e => e.VehicleNumber).IsUnique().HasDatabaseName("UQ_Vehicles_Number");

            entity.HasOne(d => d.Operator)
                .WithMany()
                .HasForeignKey(d => d.OperatorId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Vehicles_Users_OperatorId");
        });

        // Seats
        modelBuilder.Entity<Seat>(entity =>
        {
            entity.ToTable("Seats");
            entity.HasKey(e => e.Id).HasName("PK_Seats");
            entity.HasIndex(e => new { e.VehicleId, e.SeatNumber }).IsUnique().HasDatabaseName("UQ_Seats_Vehicle_Number");

            entity.HasOne(d => d.Vehicle)
                .WithMany(p => p.Seats)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Seats_Vehicles_VehicleId");
        });

        // Buses
        modelBuilder.Entity<Bus>(entity =>
        {
            entity.ToTable("Buses");
            entity.HasKey(e => e.Id).HasName("PK_Buses");

            entity.HasOne(d => d.Operator)
                .WithMany()
                .HasForeignKey(d => d.OperatorId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Buses_Users_OperatorId");

            entity.HasOne(d => d.Vehicle)
                .WithMany(p => p.Buses)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Buses_Vehicles_VehicleId");

            entity.HasOne(d => d.Route)
                .WithMany(p => p.Buses)
                .HasForeignKey(d => d.RouteId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Buses_Routes_RouteId");
        });

        // Bookings
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.ToTable("Bookings");
            entity.HasKey(e => e.Id).HasName("PK_Bookings");

            entity.HasOne(d => d.Passenger)
                .WithMany(p => p.Bookings)
                .HasForeignKey(d => d.PassengerId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Bookings_Users_PassengerId");

            entity.HasOne(d => d.Bus)
                .WithMany(p => p.Bookings)
                .HasForeignKey(d => d.BusId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_Bookings_Buses_BusId");
        });

        // BookingSeats
        modelBuilder.Entity<BookingSeat>(entity =>
        {
            entity.ToTable("BookingSeats");
            entity.HasKey(e => e.Id).HasName("PK_BookingSeats");
            entity.HasIndex(e => new { e.BookingId, e.SeatId }).IsUnique().HasDatabaseName("UQ_BookingSeats_Booking_Seat");

            entity.HasOne(d => d.Booking)
                .WithMany(p => p.BookingSeats)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_BookingSeats_Bookings_BookingId");

            entity.HasOne(d => d.Seat)
                .WithMany()
                .HasForeignKey(d => d.SeatId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_BookingSeats_Seats_SeatId");
        });

        // BookingPassengers
        modelBuilder.Entity<BookingPassenger>(entity =>
        {
            entity.ToTable("BookingPassengers");
            entity.HasKey(e => e.Id).HasName("PK_BookingPassengers");
            entity.HasIndex(e => new { e.BookingId, e.SeatId }).IsUnique().HasDatabaseName("UQ_BookingPassengers_Booking_Seat");

            entity.HasOne(d => d.Booking)
                .WithMany(p => p.BookingPassengers)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_BookingPassengers_Bookings_BookingId");
        });

        // Payments
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(e => e.Id).HasName("PK_Payments");
            entity.HasIndex(e => e.TransactionId).IsUnique().HasDatabaseName("UQ_Payments_TransactionId");

            entity.HasOne(d => d.Booking)
                .WithMany(p => p.Payments)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Payments_Bookings_BookingId");
        });

        // Tickets
        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(e => e.Id).HasName("PK_Tickets");
            entity.HasIndex(e => e.BookingId).IsUnique().HasDatabaseName("UQ_Tickets_BookingId");
            entity.HasIndex(e => e.TicketNumber).IsUnique().HasDatabaseName("UQ_Tickets_Number");

            entity.HasOne(d => d.Booking)
                .WithMany(p => p.Tickets)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Tickets_Bookings_BookingId");
        });

        // Refunds
        modelBuilder.Entity<Refund>(entity =>
        {
            entity.ToTable("Refunds");
            entity.HasKey(e => e.Id).HasName("PK_Refunds");
            entity.HasIndex(e => e.BookingId).IsUnique().HasDatabaseName("UQ_Refunds_BookingId");

            entity.HasOne(d => d.Booking)
                .WithMany(p => p.Refunds)
                .HasForeignKey(d => d.BookingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_Refunds_Bookings_BookingId");
        });

        // OperatorProfiles
        modelBuilder.Entity<OperatorProfile>(entity =>
        {
            entity.ToTable("OperatorProfiles");
            entity.HasKey(e => e.UserId).HasName("PK_OperatorProfiles");

            entity.HasOne(d => d.User)
                .WithMany(p => p.OperatorProfiles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OperatorProfiles_Users_UserId");
        });

        // OperatorOffices
        modelBuilder.Entity<OperatorOffice>(entity =>
        {
            entity.ToTable("OperatorOffices");
            entity.HasKey(e => e.Id).HasName("PK_OperatorOffices");
            entity.HasIndex(e => new { e.OperatorId, e.CityName }).IsUnique().HasDatabaseName("UQ_OperatorOffices_Operator_City");

            entity.HasOne(d => d.Operator)
                .WithMany(p => p.OperatorOffices)
                .HasForeignKey(d => d.OperatorId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_OperatorOffices_Users_OperatorId");
        });

        // PlatformFeeConfigs
        modelBuilder.Entity<PlatformFeeConfig>(entity =>
        {
            entity.ToTable("PlatformFeeConfigs");
            entity.HasKey(e => e.Id).HasName("PK_PlatformFeeConfigs");
            entity.HasIndex(e => e.IsActive).HasDatabaseName("IX_PlatformFeeConfigs_IsActive");
        });

        // EmailLogs
        modelBuilder.Entity<EmailLog>(entity =>
        {
            entity.ToTable("EmailLogs");
            entity.HasKey(e => e.Id).HasName("PK_EmailLogs");
        });
    }
}
