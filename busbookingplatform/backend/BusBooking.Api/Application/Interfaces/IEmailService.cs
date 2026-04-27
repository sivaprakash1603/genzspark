namespace BusBooking.Api.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string body, string templateKey, Guid? userId = null, byte[]? attachment = null, string? attachmentName = null);
}
