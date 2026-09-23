using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

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
    /// Sends password-reset verification codes through SMTP. The code, message
    /// body, recipient address, and SMTP password are never written to logs.
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

        public Task<bool> SendPasswordResetCodeAsync(string recipient, string username, string code, TimeSpan lifetime)
        {
            var body = $"""
Hello {username},

We received a request to reset the password for your account.

Your verification code is:

{code}

This code will expire in {lifetime.TotalMinutes:0} minutes.

If you did not request a password reset, you may ignore this email.

Española Birthing Home
Patient Management System
""";

            return SendAsync(recipient, "Password Reset Verification Code", body);
        }

        private async Task<bool> SendAsync(string recipient, string subject, string body)
        {
            if (string.IsNullOrWhiteSpace(_settings.Host) || string.IsNullOrWhiteSpace(_settings.SenderAddress)
                || string.IsNullOrWhiteSpace(_settings.AppPassword))
            {
                _logger.LogWarning("Password reset email was not sent because SMTP settings are incomplete.");
                return false;
            }

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderAddress));
                message.To.Add(MailboxAddress.Parse(recipient));
                message.Subject = subject;
                message.Body = new TextPart("plain")
                {
                    Text = body
                };

                using var client = new SmtpClient();
                client.Timeout = 20_000;
                var socketOptions = _settings.EnableSsl
                    ? _settings.Port == 465 ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

                await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions);
                await client.AuthenticateAsync(_settings.SenderAddress, _settings.AppPassword);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError("Password reset email could not be sent ({ErrorType}).", ex.GetType().Name);
                return false;
            }
        }
    }
}
