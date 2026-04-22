using BusBooking.Api.Application.DTOs;
using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Application.Services;

public class RouteService : IRouteService
{
    private readonly AppDbContext _db;

    public RouteService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<RouteResponse>> GetAllRoutesAsync()
    {
        return await _db.Routes
            .Include(x => x.Source)
            .Include(x => x.Destination)
            .Where(x => x.IsActive)
            .Select(x => new RouteResponse(x.Id, x.Source!.Name, x.Destination!.Name))
            .ToListAsync();
    }
}
