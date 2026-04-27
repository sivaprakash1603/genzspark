using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;


namespace BusBooking.Api.Application.Services;

public class BookingService : IBookingService
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ILogger<BookingService> _logger;
    private static readonly TimeSpan SeatLockDuration = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan CancellationCutoff = TimeSpan.FromHours(2);

    public BookingService(AppDbContext db, IEmailService emailService, ILogger<BookingService> logger)
    {
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<List<BusSearchResponse>> SearchBusesAsync(string source, string destination, DateTime date)
    {
        var journeyDate = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
        var dayOfWeek = (int)journeyDate.DayOfWeek;
        _logger.LogInformation("SearchBuses requested. Source={Source} Destination={Destination} Date={Date}", source, destination, journeyDate);

        var results = await _db.Buses
            .Include(x => x.Route).ThenInclude(r => r!.Source)
            .Include(x => x.Route).ThenInclude(r => r!.Destination)
            .Include(x => x.Vehicle)
            .Join(_db.OperatorProfiles,
                bus => bus.OperatorId,
                profile => profile.UserId,
                (bus, profile) => new { bus, profile })
            .Where(x => x.bus.ApprovalStatus == "Approved" && x.bus.IsActive && !x.bus.IsTemporarilyDisabled)
            .Where(x => !x.bus.IsMarkedForRemoval || (x.bus.RetirementDate != null && journeyDate <= x.bus.RetirementDate))
            .Where(x => !(x.bus.MaintenanceStart != null && x.bus.MaintenanceEnd != null && journeyDate >= x.bus.MaintenanceStart && journeyDate <= x.bus.MaintenanceEnd))
            .Where(x => x.profile.ApprovalStatus == "Approved" && x.profile.IsEnabled)
            .Where(x => x.bus.Route!.Source!.Name.ToLower() == source.ToLower() && x.bus.Route.Destination!.Name.ToLower() == destination.ToLower())
            .Where(x => x.bus.AvailableDays.Contains(dayOfWeek))
            .Where(x => (journeyDate + x.bus.DepartureTime) > DateTime.UtcNow)
            .Select(x => new BusSearchResponse(
                x.bus.Id,
                x.bus.Vehicle!.BusName,
                x.bus.Route!.Source!.Name,
                x.bus.Route.Destination!.Name,
                x.bus.BoardingPoint,
                x.bus.DropPoint,
                journeyDate.Add(x.bus.DepartureTime),
                journeyDate.Add(x.bus.DepartureTime).AddMinutes(x.bus.DurationMinutes),
                x.bus.DurationMinutes,
                x.bus.Vehicle!.SeatLayoutType,
                x.bus.TotalPrice,
                x.bus.Vehicle.TotalSeats,
                _db.Seats.Count(s => s.VehicleId == x.bus.VehicleId && s.IsActive)
                - _db.BookingSeats.Count(bs => bs.Seat!.VehicleId == x.bus.VehicleId && bs.Booking!.BookingStatus == "Confirmed" && bs.Booking.JourneyDate == journeyDate)))
            .ToListAsync();

        _logger.LogInformation("SearchBuses completed. ResultCount={Count}", results.Count);
        return results;
    }

    public async Task<List<LocationDto>> GetLocationsAsync(string query)
    {
        var normalizedQuery = (query ?? string.Empty).Trim();
        _logger.LogInformation("GetLocations requested. Query={Query}", normalizedQuery);
        var pattern = $"%{normalizedQuery}%";

        var locations = await _db.Locations
            .Where(x => x.IsActive && (normalizedQuery == string.Empty || EF.Functions.ILike(x.Name, pattern)))
            .OrderBy(x => x.Name)
            .Take(20)
            .Select(x => new LocationDto(x.Id, x.Name))
            .ToListAsync();

        _logger.LogInformation("GetLocations completed. ResultCount={Count}", locations.Count);
        return locations;
    }

    public async Task<List<SeatResponse>> GetSeatsAsync(Guid busId, DateTime journeyDate, Guid? userId = null)
    {
        _logger.LogInformation("GetSeats requested. BusId={BusId} UserId={UserId}", busId, userId);

        var bus = await _db.Buses.FirstOrDefaultAsync(x => x.Id == busId);
        if (bus is null) return new List<SeatResponse>();

        var vehicleId = bus.VehicleId;
        var now = DateTime.UtcNow;

        var queryDate = DateTime.SpecifyKind(journeyDate.Date, DateTimeKind.Utc);
        var bookedSeatIds = await _db.BookingSeats
            .Include(x => x.Booking)
            .Include(x => x.Seat)
            .Where(x => x.Seat!.VehicleId == vehicleId && x.Booking!.BookingStatus == "Confirmed" && x.Booking.JourneyDate == queryDate)
            .Select(x => x.SeatId)
            .ToListAsync();

        _logger.LogDebug("GetSeats booked seats loaded. BusId={BusId} BookedSeatCount={Count}", busId, bookedSeatIds.Count);

        var seats = await _db.Seats
            .Where(x => x.VehicleId == vehicleId && x.IsActive)
            .OrderBy(x => x.SeatNumber)
            .Select(x => new SeatResponse(
                x.Id,
                x.SeatNumber,
                bookedSeatIds.Contains(x.Id),
                x.LockedUntil.HasValue && x.LockedUntil.Value > now,
                x.LockedUntil.HasValue && x.LockedUntil.Value > now
                    ? (userId.HasValue && x.LockedByUserId == userId.Value ? "You" : "Other")
                    : null))
            .ToListAsync();

        _logger.LogInformation("GetSeats completed. BusId={BusId} SeatCount={Count}", busId, seats.Count);
        return seats;
    }

    public async Task<bool> LockSeatAsync(Guid userId, Guid seatId, DateTime journeyDate)
    {
        _logger.LogInformation("LockSeat requested. UserId={UserId} SeatId={SeatId}", userId, seatId);

        var seat = await _db.Seats.FirstOrDefaultAsync(x => x.Id == seatId && x.IsActive);
        if (seat is null)
        {
            _logger.LogWarning("LockSeat failed: seat not found. SeatId={SeatId}", seatId);
            return false;
        }

        var isLockedByAnother = seat.LockedUntil.HasValue
            && seat.LockedUntil.Value > DateTime.UtcNow
            && seat.LockedByUserId != userId;

        if (isLockedByAnother)
        {
            _logger.LogWarning("LockSeat failed: seat already locked by another user. SeatId={SeatId}", seatId);
            return false;
        }

        var queryDate = DateTime.SpecifyKind(journeyDate.Date, DateTimeKind.Utc);
        var isBooked = await _db.BookingSeats
            .Include(x => x.Booking)
            .AnyAsync(x => x.SeatId == seatId && x.Booking!.BookingStatus == "Confirmed" && x.Booking.JourneyDate == queryDate);

        if (isBooked)
        {
            _logger.LogWarning("LockSeat failed: seat already booked. SeatId={SeatId}", seatId);
            return false;
        }

        seat.LockedByUserId = userId;
        seat.LockedUntil = DateTime.UtcNow.Add(SeatLockDuration);
        await _db.SaveChangesAsync();

        _logger.LogInformation("LockSeat succeeded. UserId={UserId} SeatId={SeatId}", userId, seatId);
        return true;
    }

    public async Task<bool> UnlockSeatAsync(Guid userId, Guid seatId)
    {
        _logger.LogInformation("UnlockSeat requested. UserId={UserId} SeatId={SeatId}", userId, seatId);

        var seat = await _db.Seats.FirstOrDefaultAsync(x => x.Id == seatId);
        if (seat is null)
        {
            _logger.LogWarning("UnlockSeat failed: seat not found. SeatId={SeatId}", seatId);
            return false;
        }

        if (seat.LockedByUserId != userId)
        {
            _logger.LogWarning("UnlockSeat failed: seat lock belongs to a different user. UserId={UserId} SeatId={SeatId}", userId, seatId);
            return false;
        }

        seat.LockedByUserId = null;
        seat.LockedUntil = null;
        await _db.SaveChangesAsync();

        _logger.LogInformation("UnlockSeat succeeded. UserId={UserId} SeatId={SeatId}", userId, seatId);
        return true;
    }

    public async Task<BookingResponse> InitiateBookingAsync(Guid passengerId, InitiateBookingRequest request)
    {
        if (request.Passengers.Count == 0)
        {
            throw new InvalidOperationException("At least one passenger is required");
        }

        var seatIds = request.Passengers.Select(x => x.SeatId).Distinct().ToList();
        if (seatIds.Count != request.Passengers.Count)
        {
            throw new InvalidOperationException("Duplicate seat selection is not allowed");
        }

        _logger.LogInformation("InitiateBooking requested. PassengerId={PassengerId} BusId={BusId} SeatCount={SeatCount}", passengerId, request.BusId, seatIds.Count);

        var bus = await _db.Buses
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == request.BusId && x.ApprovalStatus == "Approved" && x.IsActive && !x.IsTemporarilyDisabled)
            ?? throw new InvalidOperationException("Bus not available");

        if (!bus.AvailableDays.Contains((int)request.JourneyDate.DayOfWeek))
        {
            throw new InvalidOperationException("Bus does not run on this day");
        }

        var seats = await _db.Seats.Where(x => seatIds.Contains(x.Id) && x.VehicleId == bus.VehicleId).ToListAsync();
        if (seats.Count != seatIds.Count)
        {
            _logger.LogWarning("InitiateBooking blocked: invalid seat selection. PassengerId={PassengerId} BusId={BusId} Requested={Requested} Found={Found}", passengerId, request.BusId, seatIds.Count, seats.Count);
            throw new InvalidOperationException("Invalid seat selection");
        }

        var journeyDateUtc = DateTime.SpecifyKind(request.JourneyDate.Date, DateTimeKind.Utc);
        var bookedSeatIds = await _db.BookingSeats
            .Include(x => x.Booking)
            .Where(x => seatIds.Contains(x.SeatId) && x.Booking!.BookingStatus == "Confirmed" && x.Booking.JourneyDate == journeyDateUtc)
            .Select(x => x.SeatId)
            .ToListAsync();

        if (bookedSeatIds.Count > 0)
        {
            _logger.LogWarning("InitiateBooking blocked: seats already booked. PassengerId={PassengerId} BusId={BusId} ConflictingSeats={Count}", passengerId, request.BusId, bookedSeatIds.Count);
            throw new InvalidOperationException("One or more seats are already booked");
        }

        // Check for active locks by other users
        var now = DateTime.UtcNow;
        var lockedByOthers = seats.Any(s => s.LockedUntil.HasValue && s.LockedUntil.Value > now && s.LockedByUserId != passengerId);
        if (lockedByOthers)
        {
            _logger.LogWarning("InitiateBooking blocked: seats locked by another user. PassengerId={PassengerId} BusId={BusId}", passengerId, request.BusId);
            throw new InvalidOperationException("One or more seats are currently held by another user. Please try again in a few minutes.");
        }

        var booking = new Domain.Entities.Booking
        {
            PassengerId = passengerId,
            BusId = request.BusId,
            JourneyDate = journeyDateUtc,
            BookingStatus = "PendingPayment",
            TotalAmount = bus.TotalPrice * seatIds.Count
        };

        _db.Bookings.Add(booking);
        foreach (var seat in seats)
        {
            _db.BookingSeats.Add(new Domain.Entities.BookingSeat
            {
                Booking = booking,
                SeatId = seat.Id,
                Fare = bus.TotalPrice
            });
        }

        foreach (var passenger in request.Passengers)
        {
            _db.BookingPassengers.Add(new Domain.Entities.BookingPassenger
            {
                Booking = booking,
                SeatId = passenger.SeatId,
                Name = passenger.Name.Trim(),
                Age = passenger.Age,
                Gender = passenger.Gender.Trim()
            });
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking created (pending payment). BookingId={BookingId} PassengerId={PassengerId} BusId={BusId} Amount={Amount}", booking.Id, passengerId, request.BusId, booking.TotalAmount);

        return new BookingResponse(
            booking.Id, 
            booking.BookingStatus, 
            booking.TotalAmount, 
            booking.BookedAt, 
            booking.JourneyDate,
            bus.Vehicle!.BusName,
            bus.Route?.Source?.Name,
            bus.Route?.Destination?.Name
        );
    }

    public async Task<List<BookingResponse>> GetMyBookingsAsync(Guid passengerId, string? filter = null)
    {
        _logger.LogInformation("GetMyBookings requested. PassengerId={PassengerId} Filter={Filter}", passengerId, filter);

        var normalizedFilter = (filter ?? "all").Trim().ToLowerInvariant();
        var now = DateTime.UtcNow;

        IQueryable<Domain.Entities.Booking> query = _db.Bookings
            .Include(x => x.Bus).ThenInclude(b => b!.Vehicle)
            .Include(x => x.Bus).ThenInclude(b => b!.Route).ThenInclude(r => r!.Source)
            .Include(x => x.Bus).ThenInclude(b => b!.Route).ThenInclude(r => r!.Destination)
            .Where(x => x.PassengerId == passengerId);

        query = normalizedFilter switch
        {
            "upcoming" => query.Where(x => x.BookingStatus == "Confirmed" && (x.JourneyDate + x.Bus!.DepartureTime) > now),
            "completed" or "past" => query.Where(x => x.BookingStatus == "Confirmed" && (x.JourneyDate + x.Bus!.DepartureTime) <= now),
            "cancelled" => query.Where(x => x.BookingStatus == "Cancelled"),
            _ => query
        };

        return await query
            .OrderByDescending(x => x.BookedAt)
            .Select(x => new BookingResponse(
                x.Id, 
                x.BookingStatus, 
                x.TotalAmount, 
                x.BookedAt, 
                x.JourneyDate,
                x.Bus!.Vehicle!.BusName,
                x.Bus.Route!.Source!.Name,
                x.Bus.Route.Destination!.Name
            ))
            .ToListAsync();
    }

    public async Task<byte[]> GetTicketPdfAsync(Guid passengerId, Guid bookingId)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var booking = await _db.Bookings
            .Include(x => x.Bus).ThenInclude(b => b!.Vehicle)
            .Include(x => x.Bus).ThenInclude(b => b!.Route).ThenInclude(r => r!.Source)
            .Include(x => x.Bus).ThenInclude(b => b!.Route).ThenInclude(r => r!.Destination)
            .FirstOrDefaultAsync(x => x.Id == bookingId && x.PassengerId == passengerId)
            ?? throw new InvalidOperationException("Booking not found");

        if (booking.BookingStatus != "Confirmed")
            throw new InvalidOperationException("Ticket is available only for confirmed bookings");

        var ticket = await _db.Tickets.FirstOrDefaultAsync(x => x.BookingId == bookingId)
            ?? throw new InvalidOperationException("Ticket not generated yet");

        var passengers = await _db.BookingPassengers
            .Include(x => x.Seat)
            .Where(x => x.BookingId == bookingId)
            .OrderBy(x => x.Seat!.SeatNumber)
            .ToListAsync();

        var departureTime = booking.JourneyDate + booking.Bus!.DepartureTime;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(1, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Helvetica"));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("BookMyTrip").FontSize(24).SemiBold().FontColor(Colors.Blue.Medium);
                        col.Item().Text("Bus Booking E-Ticket").FontSize(10).FontColor(Colors.Grey.Medium);
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text($"Ticket: {ticket.TicketNumber}").SemiBold();
                        col.Item().Text($"Date: {ticket.IssuedAt:yyyy-MM-dd HH:mm}").FontSize(9);
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Spacing(20);

                    // Trip Info
                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Column(col =>
                        {
                            col.Item().Text("FROM").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text(booking.Bus.Route!.Source!.Name).FontSize(14).SemiBold();
                            col.Item().Text(booking.Bus.BoardingPoint).FontSize(10);
                        });

                        table.Cell().AlignRight().Column(col =>
                        {
                            col.Item().Text("TO").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text(booking.Bus.Route!.Destination!.Name).FontSize(14).SemiBold();
                            col.Item().Text(booking.Bus.DropPoint).FontSize(10);
                        });
                    });

                    x.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten3);

                    var arrivalTime = departureTime.AddMinutes(booking.Bus.DurationMinutes);

                    x.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("DEPARTURE").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text(departureTime.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                        });
                        row.RelativeItem().AlignCenter().Column(col =>
                        {
                            col.Item().Text("DURATION").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text($"{booking.Bus.DurationMinutes / 60}h {booking.Bus.DurationMinutes % 60}m").SemiBold();
                        });
                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text("ARRIVAL").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text(arrivalTime.ToString("yyyy-MM-dd HH:mm")).SemiBold();
                        });
                    });

                    x.Item().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("BUS TYPE").FontSize(9).FontColor(Colors.Grey.Medium);
                            col.Item().Text(booking.Bus.Vehicle!.BusName).SemiBold();
                        });
                    });

                    // Passengers
                    x.Item().Column(col =>
                    {
                        col.Item().PaddingBottom(5).Text("PASSENGER DETAILS").FontSize(10).SemiBold().FontColor(Colors.Grey.Darken2);
                        col.Item().Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.ConstantColumn(30);
                                c.RelativeColumn();
                                c.ConstantColumn(50);
                                c.ConstantColumn(80);
                                c.ConstantColumn(60);
                            });

                            t.Header(h =>
                            {
                                h.Cell().Text("#");
                                h.Cell().Text("Name");
                                h.Cell().Text("Age");
                                h.Cell().Text("Gender");
                                h.Cell().Text("Seat");
                            });

                            for (int i = 0; i < passengers.Count; i++)
                            {
                                var p = passengers[i];
                                t.Cell().Text((i + 1).ToString());
                                t.Cell().Text(p.Name);
                                t.Cell().Text(p.Age.ToString());
                                t.Cell().Text(p.Gender);
                                t.Cell().Text(p.Seat!.SeatNumber);
                            }
                        });
                    });

                    // Payment Summary
                    x.Item().AlignRight().Column(col =>
                    {
                        col.Item().Text($"TOTAL AMOUNT PAID: ₹{booking.TotalAmount:F2}").FontSize(14).SemiBold().FontColor(Colors.Green.Medium);
                        col.Item().Text("Payment Status: Confirmed").FontSize(9).FontColor(Colors.Grey.Medium);
                    });

                    // Refund Policy
                    x.Item().Background(Colors.Grey.Lighten4).Padding(10).Column(col =>
                    {
                        col.Spacing(5);
                        col.Item().Text("CANCELLATION & REFUND POLICY").FontSize(10).SemiBold();
                        
                        var p100 = departureTime.AddHours(-72);
                        var p80 = departureTime.AddHours(-24);
                        var p50 = departureTime.AddHours(-12);
                        var p25 = departureTime.AddHours(-2);

                        col.Item().Text($"• 100% Refund: If cancelled before {p100:yyyy-MM-dd HH:mm}").FontSize(9);
                        col.Item().Text($"• 80% Refund: If cancelled between {p100:HH:mm} and {p80:yyyy-MM-dd HH:mm}").FontSize(9);
                        col.Item().Text($"• 50% Refund: If cancelled between {p80:HH:mm} and {p50:yyyy-MM-dd HH:mm}").FontSize(9);
                        col.Item().Text($"• 25% Refund: If cancelled between {p50:HH:mm} and {p25:yyyy-MM-dd HH:mm}").FontSize(9);
                        col.Item().Text($"• No Refund: If cancelled after {p25:yyyy-MM-dd HH:mm}").FontSize(9).FontColor(Colors.Red.Medium);
                    });

                    x.Item().AlignCenter().Text("Wish you a happy and safe journey!").FontSize(10).Italic().FontColor(Colors.Grey.Medium);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        }).GeneratePdf();
    }

    public async Task CancelBookingAsync(Guid passengerId, Guid bookingId, string reason)
    {
        _logger.LogInformation("CancelBooking requested. PassengerId={PassengerId} BookingId={BookingId}", passengerId, bookingId);
        var booking = await _db.Bookings.Include(x => x.Bus).FirstOrDefaultAsync(x => x.Id == bookingId && x.PassengerId == passengerId)
            ?? throw new InvalidOperationException("Booking not found");

        if (booking.BookingStatus != "Confirmed")
        {
            _logger.LogWarning("CancelBooking blocked: booking not confirmed. PassengerId={PassengerId} BookingId={BookingId} Status={Status}", passengerId, bookingId, booking.BookingStatus);
            throw new InvalidOperationException("Only confirmed bookings can be cancelled");
        }

        var now = DateTime.UtcNow;
        var actualDepartureTime = booking.JourneyDate + booking.Bus!.DepartureTime;
        var hoursToDeparture = (actualDepartureTime - now).TotalHours;
        
        if (hoursToDeparture <= CancellationCutoff.TotalHours)
        {
            _logger.LogWarning("CancelBooking blocked: cancellation window ended. PassengerId={PassengerId} BookingId={BookingId} DepartureTime={DepartureTime}", passengerId, bookingId, actualDepartureTime);
            throw new InvalidOperationException("Cancellation window has ended");
        }

        var refundPercentage = hoursToDeparture switch
        {
            >= 72 => 100m,
            >= 24 => 80m,
            >= 12 => 50m,
            _ => 25m
        };

        var refundAmount = Math.Round(booking.TotalAmount * (refundPercentage / 100m), 2, MidpointRounding.AwayFromZero);

        booking.BookingStatus = "Cancelled";
        booking.CancelledAt = now;
        booking.CancellationReason = reason;

        var existingRefund = await _db.Refunds.FirstOrDefaultAsync(x => x.BookingId == bookingId);
        if (existingRefund is null)
        {
            _db.Refunds.Add(new Domain.Entities.Refund
            {
                BookingId = bookingId,
                RefundStatus = "Pending",
                RefundAmount = refundAmount,
                RefundPercentage = refundPercentage,
                InitiatedAt = now,
                Notes = $"Cancellation refund created based on departure window ({hoursToDeparture:F1}h remaining)."
            });
        }

        await _db.SaveChangesAsync();

        _logger.LogInformation("Booking cancelled. PassengerId={PassengerId} BookingId={BookingId}", passengerId, bookingId);

        var passenger = await _db.Users.FirstAsync(x => x.Id == passengerId);
        var message = $"Your booking has been cancelled as requested. A refund of ₹{refundAmount} ({refundPercentage}% of total fare) has been initiated and will be credited to your account within 5-7 business days.";
        await _emailService.SendAsync(passenger.Email, "Booking Cancelled", message, "booking-cancelled", passengerId);
    }

    public async Task<List<RouteResponse>> GetPublicRoutesAsync()
    {
        _logger.LogInformation("GetPublicRoutes requested");
        return await _db.Routes
            .Include(x => x.Source)
            .Include(x => x.Destination)
            .OrderBy(x => x.Source!.Name)
            .Select(x => new RouteResponse(x.Id, x.Source!.Name, x.Destination!.Name))
            .Take(6)
            .ToListAsync();
    }
}
