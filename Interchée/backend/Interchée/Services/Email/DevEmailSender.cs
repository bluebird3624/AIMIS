namespace Interchée.Services.Email
{
    /// <summary>
    /// Development-only email sender: logs the email body.
    /// Replace with SMTP/SendGrid in production.
    /// </summary>
    public class DevEmailSender : IEmailSender
    {
        private readonly ILogger<DevEmailSender> _log;
        public DevEmailSender(ILogger<DevEmailSender> log) => _log = log;

        public Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            _log.LogInformation("DEV EMAIL -> To: {To}\nSubject: {Subject}\nBody:\n{Body}", toEmail, subject, htmlBody);
            return Task.CompletedTask;
        }
    }
}
