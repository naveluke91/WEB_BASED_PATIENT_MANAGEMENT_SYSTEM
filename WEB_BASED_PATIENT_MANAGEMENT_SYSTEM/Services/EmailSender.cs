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

Your password reset verification code is:

{code}

This code will expire in {lifetime.TotalMinutes:0} minutes.

If you did not request a password reset, you can ignore this email.

Española Birthing Home
Patient Management System
""";

            return SendAsync(recipient, "Password Reset Verification Code", body);
        }

        private async Task<bool> SendAsync(string recipient, string subject, string body)
        {
            // Key names only, never the values.
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(_settings.Host)) missing.Add("Smtp:Host");
            if (string.IsNullOrWhiteSpace(_settings.SenderAddress)) missing.Add("Smtp:SenderAddress");
            if (string.IsNullOrWhiteSpace(_settings.AppPassword)) missing.Add("Smtp:AppPassword");
            if (missing.Count > 0)
            {
                _logger.LogWarning("Password reset email was not sent: missing configuration {MissingKeys}. Set them with dotnet user-secrets.",
                    string.Join(", ", missing));
                return false;
            }

            // Gmail shows the App Password in 4 groups with spaces; SMTP needs it without spaces.
            var appPassword = _settings.AppPassword.Replace(" ", string.Empty);

            // Tracks which step was in progress when a failure happens, for the server log only.
            var stage = "building the message";
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

                stage = $"connecting to {_settings.Host}:{_settings.Port}";
                await client.ConnectAsync(_settings.Host, _settings.Port, socketOptions);

                stage = "authenticating";
                await client.AuthenticateAsync(_settings.SenderAddress, appPassword);

                stage = "sending the message";
                await client.SendAsync(message);
                _logger.LogInformation("Password reset email sent through {Host}:{Port}.", _settings.Host, _settings.Port);

                // Na-send na ang email; dili i-failure kung ang disconnect ra ang napakyas.
                try
                {
                    await client.DisconnectAsync(true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "SMTP disconnect failed after the email was already sent.");
                }
                return true;
            }
            catch (AuthenticationException ex)
            {
                _logger.LogError(ex, "Password reset email could not be sent: Gmail rejected Smtp:SenderAddress/Smtp:AppPassword. "
                    + "Use a 16-character Gmail App Password (2-Step Verification must be on), not the normal Gmail password.");
                return false;
            }
            catch (Exception ex)
            {
                // Full exception (type, message, stack trace) goes to the server log only, never to the browser.
                _logger.LogError(ex, "Password reset email could not be sent while {Stage}.", stage);
                return false;
            }
        }
    }
}
