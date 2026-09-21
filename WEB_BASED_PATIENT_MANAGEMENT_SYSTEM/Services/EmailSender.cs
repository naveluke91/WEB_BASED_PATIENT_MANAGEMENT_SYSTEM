using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;

namespace WEB_BASED_PATIENT_MANAGEMENT_SYSTEM.Services
{
    /// <summary>
    /// Gmail SMTP settings (section "Smtp"). SenderAddress and AppPassword come
    /// from dotnet user-secrets or environment variables, never from committed files.
    /// </summary>
    public class SmtpSettings
    {
        public string Host { get; set; } = "smtp.gmail.com";

        public int Port { get; set; } = 587;

        public bool EnableSsl { get; set; } = true;

        public string SenderName { get; set; } = "Española Birthing Home";

        public string SenderAddress { get; set; } = string.Empty;

        public string AppPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Sends the SuperAdmin recovery code through SMTP. The code, the message
    /// body and the SMTP password are never written to the logs.
    /// </summary>
    public class EmailSender
    {
        private readonly SmtpSettings _settings;
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(IOptions<SmtpSettings> options, ILogger<EmailSender> logger)
        {
            _settings = options.Value;
            _logger = logger;
        }

        // I-send ang recovery code; ang code dili i-log.
        public Task SendRecoveryCodeAsync(string recipient, string code, TimeSpan lifetime)
        {
            var body = new StringBuilder()
                .AppendLine("Española Birthing Home")
                .AppendLine("SuperAdmin Password Recovery")
                .AppendLine()
                .AppendLine("Recovery Code:")
                .AppendLine(code)
                .AppendLine()
                .AppendLine($"Expires in {lifetime.TotalMinutes:0} minutes.")
                .AppendLine()
                .AppendLine("If you did not request this, ignore this message.")
                .ToString();

            return SendAsync(recipient, "SuperAdmin Password Recovery", body);
        }

        private async Task SendAsync(string recipient, string subject, string body)
        {
            // Walay SMTP settings: dili ma-send (walay detalye sa log).
            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.SenderAddress)
                || string.IsNullOrWhiteSpace(_settings.AppPassword))
            {
                _logger.LogWarning("Recovery email not sent: SMTP settings (Smtp:SenderAddress, Smtp:AppPassword) are not configured.");
                return;
            }

            try
            {
                using var message = new MailMessage(new MailAddress(_settings.SenderAddress, _settings.SenderName), new MailAddress(recipient))
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                    SubjectEncoding = Encoding.UTF8,
                    BodyEncoding = Encoding.UTF8
                };

                using var client = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(_settings.SenderAddress, _settings.AppPassword),
                    Timeout = 20000
                };

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                // Ang klase ra sa error ang i-log, dili ang mensahe o credentials.
                _logger.LogError("Recovery email could not be sent ({ErrorType}).", ex.GetType().Name);
            }
        }
    }
}
