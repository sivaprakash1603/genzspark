using BusBooking.Api.Application.Interfaces;
using BusBooking.Api.Domain.Entities;
using BusBooking.Api.Infrastructure.Persistence;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace BusBooking.Api.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly SmtpOptions _options;
    private readonly AppDbContext _db;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<SmtpOptions> options, AppDbContext db, ILogger<SmtpEmailService> logger)
    {
        _options = options.Value;
        _db = db;
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string subject, string body, string templateKey, Guid? userId = null)
    {
        _logger.LogInformation("Email send requested. To={ToEmail} Subject={Subject} Template={TemplateKey} UserId={UserId}", toEmail, subject, templateKey, userId);

        var log = new EmailLog
        {
            UserId = userId,
            ToEmail = toEmail,
            Subject = subject,
            TemplateKey = templateKey,
            Status = "Sent"
        };

        try
        {
            if (string.IsNullOrWhiteSpace(_options.Host))
            {
                _logger.LogWarning("SMTP host is empty; email will fail. Template={TemplateKey} To={ToEmail}", templateKey, toEmail);
            }

            using var client = new SmtpClient(_options.Host, _options.Port)
            {
                EnableSsl = _options.UseSsl,
                Credentials = new NetworkCredential(_options.Username, _options.Password)
            };

            using var mail = new MailMessage(_options.FromEmail, toEmail, subject, body);
            await client.SendMailAsync(mail);

            _logger.LogInformation("Email sent. To={ToEmail} Template={TemplateKey}", toEmail, templateKey);
        }
        catch (Exception ex)
        {
            log.Status = "Failed";
            log.ErrorMessage = ex.Message;

            _logger.LogError(ex, "Email send failed. To={ToEmail} Template={TemplateKey}", toEmail, templateKey);
        }

        _db.EmailLogs.Add(log);
        await _db.SaveChangesAsync();

        _logger.LogDebug("Email log persisted. Status={Status} Template={TemplateKey} To={ToEmail}", log.Status, templateKey, toEmail);
    }
}
