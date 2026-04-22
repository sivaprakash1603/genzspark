using BusBooking.Api.Domain.Entities;

namespace BusBooking.Api.Application.Interfaces;

public interface IBusRepository
{
    Task<Bus?> GetByIdAsync(Guid id);
    Task AddAsync(Bus bus);
    Task SaveChangesAsync();
}
