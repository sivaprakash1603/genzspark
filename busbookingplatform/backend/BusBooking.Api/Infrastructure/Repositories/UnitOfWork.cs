using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Infrastructure.Persistence;

namespace BusBooking.Api.Infrastructure.Repositories;

internal class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _db.Dispose();
    }
}
