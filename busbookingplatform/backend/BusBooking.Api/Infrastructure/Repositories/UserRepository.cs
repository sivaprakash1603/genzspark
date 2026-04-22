using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Domain.Entities;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<User?> GetByUsernameAsync(string username)
    {
        return _db.Users
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Username.ToLower() == username.ToLower());
    }

    public Task<User?> GetByIdAsync(Guid id)
    {
        return _db.Users.Include(x => x.Role).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(User user)
    {
        await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();
    }
}
