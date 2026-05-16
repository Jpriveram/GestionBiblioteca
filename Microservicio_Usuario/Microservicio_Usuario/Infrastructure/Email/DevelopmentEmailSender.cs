using Microsoft.Extensions.Logging;
using ServicioUsuario.Application.Interfaces;

namespace ServicioUsuario.Infrastructure.Email;

public class DevelopmentEmailSender : IEmailSender
{
    private readonly ILogger<DevelopmentEmailSender> _logger;

    public DevelopmentEmailSender(ILogger<DevelopmentEmailSender> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _logger.LogWarning(
            "[DEV EMAIL MODE] Correo no enviado a proveedor real. To: {To}, Subject: {Subject}, Body: {Body}",
            message.To,
            message.Subject,
            message.PlainTextContent);

        return Task.FromResult(true);
    }
}
