namespace BusBooking.Api.Application.Interfaces;

internal interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync();
}
