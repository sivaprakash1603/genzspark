using BusBooking.Api.Domain.Enums;

namespace BusBooking.Api.Domain.Entities;

internal class EmailLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string TemplateKey { get; set; } = string.Empty;
    public EmailStatus Status { get; set; } = EmailStatus.Sent;
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public override string ToString()
    {
        return $"EmailLog: {Id} | To: {ToEmail} | Subject: {Subject} | Status: {Status}";
    }
}
