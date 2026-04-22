using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Domain.Entities;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Infrastructure.Repositories;

public class BusRepository : IBusRepository
{
    private readonly AppDbContext _db;

    public BusRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<Bus?> GetByIdAsync(Guid id)
    {
        return _db.Buses.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(Bus bus)
    {
        await _db.Buses.AddAsync(bus);
    }

    public Task SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
