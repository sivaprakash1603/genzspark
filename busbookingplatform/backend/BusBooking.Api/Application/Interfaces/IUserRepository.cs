using BusBooking.Api.Domain.Entities;

namespace BusBooking.Api.Application.Interfaces;

internal interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
}
