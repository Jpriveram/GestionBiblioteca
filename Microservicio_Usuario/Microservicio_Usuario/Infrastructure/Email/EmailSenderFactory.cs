using Microsoft.Extensions.Options;
using ServicioUsuario.Domain.Ports;
using ServicioUsuario.Infrastructure.Configuration;

namespace ServicioUsuario.Infrastructure.Email;

public static class EmailSenderFactory
{
    public static IEmailSender Create(IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<IOptions<EmailSettings>>().Value;

        if (settings.UseDevelopmentMode)
        {
            return ActivatorUtilities.CreateInstance<DevelopmentEmailSender>(serviceProvider);
        }

        if (string.Equals(settings.Provider, "Smtp", StringComparison.OrdinalIgnoreCase))
        {
            return ActivatorUtilities.CreateInstance<SmtpEmailSender>(serviceProvider);
        }

        if (string.Equals(settings.Provider, "Api", StringComparison.OrdinalIgnoreCase))
        {
            return ActivatorUtilities.CreateInstance<HttpApiEmailSender>(serviceProvider);
        }

        throw new InvalidOperationException($"Proveedor de correo electrónico no compatible: {settings.Provider}");
    }
}
