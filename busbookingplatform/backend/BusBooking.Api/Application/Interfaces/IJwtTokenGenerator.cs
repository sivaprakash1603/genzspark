using BusBooking.Api.Domain.Entities;

namespace BusBooking.Api.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string Generate(User user, string roleName);
}
