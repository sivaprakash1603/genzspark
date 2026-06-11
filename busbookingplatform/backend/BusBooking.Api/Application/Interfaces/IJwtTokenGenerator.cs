using BusBooking.Api.Domain.Entities;

namespace BusBooking.Api.Application.Interfaces;

internal interface IJwtTokenGenerator
{
    string Generate(User user, string roleName);
}
