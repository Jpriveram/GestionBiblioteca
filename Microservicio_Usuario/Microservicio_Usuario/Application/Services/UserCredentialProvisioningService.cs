using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ServicioUsuario.Application.Interfaces;
using ServicioUsuario.Domain.Entities;
using ServicioUsuario.Domain.Ports;
using ServicioUsuario.Infrastructure.Persistence;

namespace ServicioUsuario.Application.Services;

public class UserCredentialProvisioningService : IUserCredentialProvisioningService
{
    private const int TemporaryPasswordLength = 10;
    private const int MaxUserNameLength = 50;
    private const string Sha2Algorithm = "SHA-256";
    private const string TemporaryHashPrefix = "TEMP$";
    private const string PasswordCharacters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";

    private readonly UsuarioRepository _usuarioRepositorio;
    private readonly IEmailSender _emailSender;

    public UserCredentialProvisioningService(UsuarioRepository usuarioRepositorio, IEmailSender emailSender)
    {
        _usuarioRepositorio = usuarioRepositorio;
        _emailSender = emailSender;
    }

    public async Task<CredentialProvisioningResult> PrepareAndNotifyAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        if (usuario is null)
        {
            throw new ArgumentNullException(nameof(usuario));
        }

        if (string.IsNullOrWhiteSpace(usuario.Email))
        {
            throw new InvalidOperationException("El usuario debe tener email para enviar credenciales.");
        }

        var nombreUsuario = GenerarNombreUsuarioUnico(usuario.Nombres, usuario.PrimerApellido, usuario.SegundoApellido);
        var passwordTemporal = GenerarPasswordTemporalSegura();
        var hashPassword = TemporaryHashPrefix + BCrypt.Net.BCrypt.HashPassword(passwordTemporal);

        usuario.NombreUsuario = nombreUsuario;
        usuario.PasswordHash = hashPassword;

        var message = new EmailMessage
        {
            To = usuario.Email,
            Subject = "Credenciales de acceso - Sistema de Biblioteca",
            PlainTextContent =
                $"Hola {usuario.Nombres},\n\n" +
                "Tu usuario fue creado correctamente.\n" +
                $"Nombre de usuario: {nombreUsuario}\n" +
                $"Contraseña temporal: {passwordTemporal}\n\n" +
                "Por seguridad, en tu primer inicio de sesión se te pedirá cambiar la contraseña.\n"
        };

        var emailSent = true;
        string? emailError = null;

        try
        {
            await _emailSender.SendAsync(message, cancellationToken);
        }
        catch (Exception ex)
        {
            emailSent = false;
            emailError = ex.Message;

            Console.WriteLine($"[WARN] No se pudo enviar el correo al usuario {usuario.Email}: {ex.Message}");
        }

        return new CredentialProvisioningResult
        {
            GeneratedUserName = nombreUsuario,
            PasswordHash = hashPassword,
            PasswordAlgorithm = "BCrypt",
            EmailSent = emailSent,
            EmailError = emailError
        };
    }

    private string GenerarNombreUsuarioUnico(string nombres, string primerApellido, string? segundoApellido)
    {
        var baseName = BuildBaseUserName(nombres, primerApellido, segundoApellido);

        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "usuario";
        }

        baseName = baseName.Length > MaxUserNameLength
            ? baseName[..MaxUserNameLength]
            : baseName;

        if (!_usuarioRepositorio.ExisteNombreUsuario(baseName))
        {
            return baseName;
        }

        var contador = 1;

        while (true)
        {
            var sufijo = contador.ToString(CultureInfo.InvariantCulture);
            var maxBaseLen = MaxUserNameLength - sufijo.Length;

            if (maxBaseLen <= 0)
            {
                throw new InvalidOperationException("No fue posible generar un nombre de usuario único.");
            }

            var candidateBase = baseName.Length > maxBaseLen ? baseName[..maxBaseLen] : baseName;
            var candidate = $"{candidateBase}{sufijo}";

            if (!_usuarioRepositorio.ExisteNombreUsuario(candidate))
            {
                return candidate;
            }

            contador++;
        }
    }

    private static string BuildBaseUserName(string nombres, string primerApellido, string? segundoApellido)
    {
        var nombre = FormatToken(TakeFirstToken(nombres));
        var apellido1 = FormatToken(TakeFirstToken(primerApellido));
        var apellido2 = FormatToken(TakeFirstToken(segundoApellido));

        return $"{nombre}{apellido1}{apellido2}";
    }

    private static string FormatToken(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var normalized = NormalizeForUserName(value);
        if (string.IsNullOrWhiteSpace(normalized)) return string.Empty;

        var truncated = normalized.Length > 3 ? normalized[..3] : normalized;
        return char.ToUpperInvariant(truncated[0]) + truncated[1..].ToLowerInvariant();
    }

    private static string TakeFirstToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;
    }

    private static string NormalizeForUserName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Normalize(NormalizationForm.FormD);
        var chars = normalized
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .Where(char.IsAscii)
            .Where(char.IsLetterOrDigit)
            .ToArray();

        return new string(chars);
    }

    private static string GenerarPasswordTemporalSegura()
    {
        var passwordChars = new char[TemporaryPasswordLength];

        for (var i = 0; i < TemporaryPasswordLength; i++)
        {
            var idx = RandomNumberGenerator.GetInt32(PasswordCharacters.Length);
            passwordChars[i] = PasswordCharacters[idx];
        }

        return new string(passwordChars);
    }

}
