using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Domain.Entities;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Infrastructure.Repositories;

internal class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext db) : base(db)
    {
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return _dbSet
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
    }

    public override async Task<User?> GetByIdAsync(Guid id)
    {
        return await _dbSet.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
    }

}
