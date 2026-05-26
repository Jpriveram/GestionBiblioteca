using ServicioUsuario.Application.Dtos;
using ServicioUsuario.Application.Interfaces;
using ServicioUsuario.Domain.Entities;
using ServicioUsuario.Domain.Errors;
using ServicioUsuario.Domain.Ports;
using ServicioUsuario.Domain.Validations;
using ServicioUsuario.Infrastructure.Persistence;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ServicioUsuario.Application.Services;

public class UsuarioService : IUsuarioServicio
{
    private readonly UsuarioRepository _repositorio;
    private readonly IUserCredentialProvisioningService _credentialProvisioning;

    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public UsuarioService(UsuarioRepository repositorio, IUserCredentialProvisioningService credentialProvisioning, IJwtTokenGenerator jwtTokenGenerator, IHttpContextAccessor httpContextAccessor)
    {
        _repositorio = repositorio;
        _credentialProvisioning = credentialProvisioning;
        _jwtTokenGenerator = jwtTokenGenerator;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<List<UsuarioDto>> GetAllAsync()
    {
        var usuarios = _repositorio.GetAll()
            .Where(u => u.Estado)
            .Select(MapToDto)
            .ToList();
        return Task.FromResult(usuarios);
    }

    public Task<UsuarioDto?> GetByIdAsync(int id)
    {
        var usuario = _repositorio.GetById(id);
        return Task.FromResult(usuario != null ? MapToDto(usuario) : null);
    }

    public Task<UsuarioDto?> GetByEmailAsync(string email)
    {
        var usuarios = _repositorio.GetAll();
        var usuario = usuarios.FirstOrDefault(u => u.Email == email);
        return Task.FromResult(usuario != null ? MapToDto(usuario) : null);
    }

    public Task<UsuarioDto?> GetByCIAsync(string ci)
    {
        var usuario = _repositorio.GetByCi(ci);
        return Task.FromResult(usuario != null ? MapToDto(usuario) : null);
    }

    public async Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto)
    {
        ValidarCreacion(dto);

        var ci = UnirCiComplemento(dto.CI, dto.Complemento);
        var email = ValidadorEntrada.NormalizarEspacios(dto.Email);
        var rol = ValidadorEntrada.NormalizarEspacios(dto.Rol);

        if (_repositorio.ExisteCi(ci))
        {
            throw new UsuarioValidationException(UsuarioErrors.CiDuplicado);
        }

        if (_repositorio.ExisteEmail(email))
        {
            throw new UsuarioValidationException(UsuarioErrors.EmailDuplicado);
        }

        var usuario = new Usuario
        {
            UsuarioSesionId = GetCurrentUserId(),
            CI = ci,
            Nombres = NormalizeDisplayName(dto.Nombres),
            PrimerApellido = NormalizeDisplayName(dto.PrimerApellido),
            SegundoApellido = NormalizeDisplayName(dto.SegundoApellido),
            Email = email,
            NombreUsuario = null,
            PasswordHash = null,
            Rol = rol,
            Estado = true,
            FechaCreacion = DateTime.Now,
            FechaActualizacion = DateTime.Now
        };

        if (ShouldAutoProvisionCredentials(rol))
        {
            var provisioning = await _credentialProvisioning.PrepareAndNotifyAsync(usuario);

            if (!provisioning.EmailSent)
            {
                throw new InvalidOperationException($"No se pudo enviar correo de credenciales: {provisioning.EmailError}");
            }
        }
        else
        {
            usuario.NombreUsuario = !string.IsNullOrWhiteSpace(dto.NombreUsuario)
                ? dto.NombreUsuario
                : (!string.IsNullOrWhiteSpace(dto.CI) ? dto.CI : dto.Email);

            usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password ?? "temporal123");
        }

        _repositorio.Insert(usuario);

        var usuarioPersistido = _repositorio.GetByNombreUsuario(usuario.NombreUsuario ?? string.Empty)
            ?? (!string.IsNullOrWhiteSpace(usuario.CI)
                ? _repositorio.GetByCi(usuario.CI)
                : null);

        return MapToDto(usuarioPersistido ?? usuario);
    }

    private static void ValidarCreacion(CreateUsuarioDto dto)
    {
        var ci = ValidadorEntrada.NormalizarEspacios(dto.CI);
        var complemento = ValidadorEntrada.NormalizarEspacios(dto.Complemento);
        var nombres = ValidadorEntrada.NormalizarEspacios(dto.Nombres);
        var primerApellido = ValidadorEntrada.NormalizarEspacios(dto.PrimerApellido);
        var segundoApellido = ValidadorEntrada.NormalizarEspacios(dto.SegundoApellido);
        var email = ValidadorEntrada.NormalizarEspacios(dto.Email);
        var rol = ValidadorEntrada.NormalizarEspacios(dto.Rol);

        if (ValidadorEntrada.EstaVacio(ci)
            || ValidadorEntrada.EstaVacio(nombres)
            || ValidadorEntrada.EstaVacio(primerApellido)
            || ValidadorEntrada.EstaVacio(email)
            || ValidadorEntrada.EstaVacio(rol))
        {
            throw new UsuarioValidationException(UsuarioErrors.DatosObligatorios);
        }

        if (!ValidadorEntrada.CarnetIdentidadValido(ci))
        {
            throw new UsuarioValidationException(UsuarioErrors.CiInvalido);
        }

        if (!ValidadorEntrada.ComplementoCiValido(complemento))
        {
            throw new UsuarioValidationException(UsuarioErrors.ComplementoInvalido);
        }

        if (!ValidadorEntrada.SoloLetras(nombres))
        {
            throw new UsuarioValidationException(UsuarioErrors.NombresInvalidos);
        }

        if (!ValidadorEntrada.SoloLetras(primerApellido))
        {
            throw new UsuarioValidationException(UsuarioErrors.PrimerApellidoInvalido);
        }

        if (!string.IsNullOrWhiteSpace(segundoApellido) && !ValidadorEntrada.SoloLetras(segundoApellido))
        {
            throw new UsuarioValidationException(UsuarioErrors.SegundoApellidoInvalido);
        }

        if (!ValidadorEntrada.EmailValido(email))
        {
            throw new UsuarioValidationException(UsuarioErrors.EmailInvalido);
        }

        if (!ValidadorEntrada.RolUsuarioValido(rol))
        {
            throw new UsuarioValidationException(UsuarioErrors.RolInvalido);
        }
    }

    private static string UnirCiComplemento(string? ci, string? complemento)
    {
        var ciNormalizado = ValidadorEntrada.NormalizarEspacios(ci);
        var complementoNormalizado = ValidadorEntrada.NormalizarEspacios(complemento);
        return string.IsNullOrWhiteSpace(complementoNormalizado)
            ? ciNormalizado
            : $"{ciNormalizado}-{complementoNormalizado}";
    }

    public Task<UsuarioDto?> UpdateAsync(int id, UpdateUsuarioDto dto)
    {
        var usuario = _repositorio.GetById(id);
        if (usuario == null)
            return Task.FromResult<UsuarioDto?>(null);

        usuario.UsuarioSesionId = GetCurrentUserId();
        usuario.CI = dto.CI;
        usuario.Nombres = NormalizeDisplayName(dto.Nombres);
        usuario.PrimerApellido = NormalizeDisplayName(dto.PrimerApellido);
        usuario.SegundoApellido = NormalizeDisplayName(dto.SegundoApellido);
        usuario.Email = dto.Email;
        usuario.NombreUsuario = dto.NombreUsuario;
        usuario.Rol = dto.Rol;
        usuario.Estado = dto.Estado;
        usuario.FechaActualizacion = DateTime.Now;

        _repositorio.Update(usuario);
        return Task.FromResult<UsuarioDto?>(MapToDto(usuario));
    }

    public Task<bool> DeleteAsync(int id)
    {
        var usuario = _repositorio.GetById(id);
        if (usuario == null)
            return Task.FromResult(false);

        usuario.Estado = false;
        _repositorio.Update(usuario);
        return Task.FromResult(true);
    }

   public Task<(UsuarioDto? Usuario, string? Token)> LoginAsync(string nombreUsuario, string password)
    {
        var normalizedUserName = nombreUsuario?.Trim() ?? string.Empty;
        var normalizedPassword = password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedUserName) || string.IsNullOrWhiteSpace(normalizedPassword))
            return Task.FromResult<(UsuarioDto?, string?)>((null, null));

        var usuario = _repositorio.GetByNombreUsuario(normalizedUserName);

        if (usuario == null || !usuario.Estado || usuario.PasswordHash == null)
            return Task.FromResult<(UsuarioDto?, string?)>((null, null));

        if (!VerifyPassword(normalizedPassword, usuario.PasswordHash))
            return Task.FromResult<(UsuarioDto?, string?)>((null, null));

        var token = _jwtTokenGenerator.GenerateToken(usuario);
        var usuarioDto = MapToDto(usuario);

        return Task.FromResult<(UsuarioDto?, string?)>((usuarioDto, token));
    }

    public Task CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva, string passwordConfirmacion)
    {
        var usuario = _repositorio.GetById(usuarioId);

        if (usuario == null || !usuario.Estado || string.IsNullOrWhiteSpace(usuario.PasswordHash))
        {
            throw new InvalidOperationException("Usuario no encontrado o sin credenciales activas.");
        }

        var actual = passwordActual?.Trim() ?? string.Empty;
        var nueva = passwordNueva?.Trim() ?? string.Empty;
        var confirmacion = passwordConfirmacion?.Trim() ?? string.Empty;

        if (!VerifyPassword(actual, usuario.PasswordHash))
        {
            throw new InvalidOperationException("La contrasena actual no es correcta.");
        }

        if (!string.Equals(nueva, confirmacion, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("La nueva contrasena y su confirmacion no coinciden.");
        }

        var policyResult = ValidatePasswordPolicy(nueva);
        if (!policyResult.IsValid)
        {
            throw new InvalidOperationException(policyResult.ErrorMessage);
        }

        if (VerifyPassword(nueva, usuario.PasswordHash))
        {
            throw new InvalidOperationException("La nueva contrasena no puede ser igual a la contrasena actual.");
        }

        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(nueva);
        usuario.UsuarioSesionId = GetCurrentUserId();
        usuario.FechaActualizacion = DateTime.Now;

        _repositorio.Update(usuario);
        return Task.CompletedTask;
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            return false;

        var normalizedHash = IsTemporaryPasswordHash(storedHash)
            ? storedHash[TemporaryHashPrefix.Length..]
            : storedHash;

        try
        {
            if (BCrypt.Net.BCrypt.Verify(password, normalizedHash))
                return true;
        }
        catch
        {
            // Hash legado o formato no compatible con BCrypt.
        }

        return string.Equals(ComputeSha256(password), normalizedHash, StringComparison.Ordinal);
    }

    private static string ComputeSha256(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    private UsuarioDto MapToDto(Usuario usuario)
    {
        return new UsuarioDto
        {
            UsuarioId = usuario.UsuarioId,
            UsuarioSesionId = usuario.UsuarioSesionId,
            CI = usuario.CI,
            Nombres = NormalizeDisplayName(usuario.Nombres),
            PrimerApellido = NormalizeDisplayName(usuario.PrimerApellido),
            SegundoApellido = NormalizeDisplayName(usuario.SegundoApellido),
            Email = usuario.Email,
            NombreUsuario = usuario.NombreUsuario,
            Rol = usuario.Rol,
            Estado = usuario.Estado,
            DebeCambiarPassword = IsTemporaryPasswordHash(usuario.PasswordHash)
        };
    }

    private int? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var sub = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(sub, out var id))
            return id;
        return null;
    }

    private static (bool IsValid, string ErrorMessage) ValidatePasswordPolicy(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return (false, "La nueva contrasena es obligatoria.");
        }

        if (password.Length < 8)
        {
            return (false, "La contrasena debe tener al menos 8 caracteres.");
        }

        if (!Regex.IsMatch(password, "[A-Z]"))
        {
            return (false, "La contrasena debe incluir al menos una letra mayuscula.");
        }

        if (!Regex.IsMatch(password, "[a-z]"))
        {
            return (false, "La contrasena debe incluir al menos una letra minuscula.");
        }

        if (!Regex.IsMatch(password, "[0-9]"))
        {
            return (false, "La contrasena debe incluir al menos un numero.");
        }

        if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
        {
            return (false, "La contrasena debe incluir al menos un caracter especial.");
        }

        return (true, string.Empty);
    }

    private const string TemporaryHashPrefix = "TEMP$";

    private static bool IsTemporaryPasswordHash(string? passwordHash)
    {
        return !string.IsNullOrWhiteSpace(passwordHash)
            && passwordHash.StartsWith(TemporaryHashPrefix, StringComparison.Ordinal);
    }

    private static string NormalizeDisplayName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var compactado = string.Join(' ', value
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        var textInfo = CultureInfo.GetCultureInfo("es-ES").TextInfo;
        return textInfo.ToTitleCase(textInfo.ToLower(compactado));
    }

    private static bool ShouldAutoProvisionCredentials(string? rol)
    {
        if (string.IsNullOrWhiteSpace(rol))
        {
            return false;
        }

        return string.Equals(rol, "Usuario", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, "Bibliotecario", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, "Admin", StringComparison.OrdinalIgnoreCase);
    }
}
