using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Domain.Entities;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BusBooking.Api.Infrastructure.Repositories;

internal class BusRepository : Repository<Bus>, IBusRepository
{
    public BusRepository(AppDbContext db) : base(db)
    {
    }

}
