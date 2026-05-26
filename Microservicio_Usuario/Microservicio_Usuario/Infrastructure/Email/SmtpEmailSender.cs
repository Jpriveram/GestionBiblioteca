using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using ServicioUsuario.Domain.Ports;
using ServicioUsuario.Infrastructure.Configuration;

namespace ServicioUsuario.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<EmailSettings> options, ILogger<SmtpEmailSender> logger)
    {
        _settings = options.Value;
        _logger = logger;
    }

    public async Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_settings.UseDevelopmentMode || string.IsNullOrWhiteSpace(_settings.Smtp.Host))
        {
            _logger.LogWarning("Email dev mode: To={To}, Subject={Subject}, Body={Body}",
                message.To, message.Subject, message.PlainTextContent);
            return true;
        }

        if (string.IsNullOrWhiteSpace(_settings.FromAddress))
        {
            throw new InvalidOperationException("Email.FromAddress no está configurado.");
        }

        if (string.IsNullOrWhiteSpace(_settings.Smtp.Username) || string.IsNullOrWhiteSpace(_settings.Smtp.Password))
        {
            throw new InvalidOperationException("Credenciales SMTP no configuradas.");
        }

        using var client = new SmtpClient(_settings.Smtp.Host, _settings.Smtp.Port)
        {
            EnableSsl = _settings.Smtp.EnableSsl,
            Credentials = new NetworkCredential(_settings.Smtp.Username, _settings.Smtp.Password)
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = message.Subject,
            Body = message.PlainTextContent,
            IsBodyHtml = false
        };

        mail.To.Add(message.To);

        await client.SendMailAsync(mail, cancellationToken);
        _logger.LogInformation("Email sent to {To}", message.To);
        return true;
    }
}
