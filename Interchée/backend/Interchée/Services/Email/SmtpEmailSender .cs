using Interchée.Services.Email.Entity;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Interchée.Services.Email
{
    /// <summary>
    /// Production-ready SMTP sender using MailKit.
    /// </summary>
    public class SmtpEmailSender : IEmailSender
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<SmtpEmailSender> _log;

        public SmtpEmailSender(IOptions<EmailSettings> options, ILogger<SmtpEmailSender> log)
        {
            _settings = options.Value;
            _log = log;
        }

        public async Task SendAsync(string toEmail, string subject, string htmlBody)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;

            var body = new BodyBuilder { HtmlBody = htmlBody };
            msg.Body = body.ToMessageBody();

            using var client = new SmtpClient();
            try
            {
                // Optional: dev-only trust (DO NOT enable in prod)
                if (_settings.Smtp.DisableCertificateValidation)
                {
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                }

                await client.ConnectAsync(
                    _settings.Smtp.Host,
                    _settings.Smtp.Port,
                    _settings.Smtp.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.SslOnConnect);

                if (!string.IsNullOrWhiteSpace(_settings.Smtp.User))
                {
                    await client.AuthenticateAsync(_settings.Smtp.User, _settings.Smtp.Password);
                }

                await client.SendAsync(msg);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw; // let caller decide whether to swallow or report
            }
        }
    }
}
