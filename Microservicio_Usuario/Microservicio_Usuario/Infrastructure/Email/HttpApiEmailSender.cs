using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using ServicioUsuario.Domain.Ports;
using ServicioUsuario.Infrastructure.Configuration;

namespace ServicioUsuario.Infrastructure.Email;

public class HttpApiEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;

    public HttpApiEmailSender(HttpClient httpClient, IOptions<EmailSettings> options)
    {
        _httpClient = httpClient;
        _settings = options.Value;
    }

    public async Task<bool> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.ApiUrl))
        {
            throw new InvalidOperationException("Email:ApiUrl no está configurado.");
        }

        if (string.IsNullOrWhiteSpace(_settings.ApiKey))
        {
            throw new InvalidOperationException("Email:ApiKey no está configurado para proveedor API.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, _settings.ApiUrl);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _settings.ApiKey);
        request.Content = JsonContent.Create(new
        {
            from = new { email = _settings.FromAddress, name = _settings.FromName },
            to = new[] { new { email = message.To } },
            subject = message.Subject,
            text = message.PlainTextContent
        });

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Error al enviar el correo mediante API. Estado: {(int)response.StatusCode}. Detalle: {body}");
        }

        return true;
    }
}
