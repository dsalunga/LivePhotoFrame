using Microsoft.Extensions.Logging;

namespace LivePhotoFrame.WebApp.Services
{
    /// <summary>
    /// Placeholder email sender. Replace with a real provider (SendGrid, SMTP, etc.)
    /// before enabling RequireConfirmedEmail in Identity options.
    /// </summary>
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            _logger.LogWarning("Email sending is not configured. Discarding email to {Email} with subject '{Subject}'.", email, subject);
            return Task.CompletedTask;
        }
    }
}
