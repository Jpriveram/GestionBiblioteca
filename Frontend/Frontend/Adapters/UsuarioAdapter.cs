using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Dtos;
using Frontend.Adapters;
using Frontend.Helpers;
using Frontend.Dtos;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Frontend.Adapters;

public class UsuarioAdapter : IUsuarioServicio
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public UsuarioAdapter(IHttpClientFactory f, IHttpContextAccessor httpContextAccessor)
    {
        _http = f.CreateClient("ServicioUsuario");
        _httpContextAccessor = httpContextAccessor;
    }

    public IEnumerable<UsuarioDto> Select()
    {
        try
        {
            EnsureAuthorizationHeader();
            var response = _http.GetAsync("api/usuarios").Result;
            if (!response.IsSuccessStatusCode)
            {
                System.Diagnostics.Debug.WriteLine($"Error UsuarioAdapter.Select(): {response.StatusCode} - {response.Content.ReadAsStringAsync().Result}");
                return new List<UsuarioDto>();
            }

            var resultado = response.Content.ReadFromJsonAsync<List<UsuarioDto>>(JsonOptions).Result ?? new();
            System.Diagnostics.Debug.WriteLine($"UsuarioAdapter.Select() retornó {resultado.Count} usuarios");
            return resultado;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error UsuarioAdapter.Select() Exception: {ex.Message}");
            return new List<UsuarioDto>();
        }
    }

    public Result<UsuarioDto> Create(UsuarioDto d)
    {
        try
        {
            EnsureAuthorizationHeader();
            d.Nombres = d.Nombres.ToDisplayName();
            d.PrimerApellido = d.PrimerApellido.ToDisplayName();
            d.SegundoApellido = d.SegundoApellido.ToDisplayName();
            var response = _http.PostAsJsonAsync("api/usuarios", d).Result;
            if (!response.IsSuccessStatusCode)
                return Result<UsuarioDto>.Failure(ParseApiError(response.Content.ReadAsStringAsync().Result, "Create", "Error al crear usuario."));

            var created = response.Content.ReadFromJsonAsync<UsuarioDto>().Result;
            return Result<UsuarioDto>.Success(created ?? d);
        }
        catch (Exception ex)
        {
            return Result<UsuarioDto>.Failure(new Error("Create", ex.Message));
        }
    }

    public Result CrearLector(LectorDto d, int uid)
    {
        try
        {
            EnsureAuthorizationHeader();
            var createUsuarioDto = new UsuarioDto
            {
                NombreUsuario = !string.IsNullOrWhiteSpace(d.CI)
                    ? d.CI
                    : d.Nombres,
                Nombres = d.Nombres.ToDisplayName(),
                PrimerApellido = d.PrimerApellido.ToDisplayName(),
                SegundoApellido = d.SegundoApellido.ToDisplayName(),
                Email = d.Email,
                CI = d.CI,
                Complemento = d.Complemento,
                Rol = "Lector",
                Estado = true,
                UsuarioSesionId = uid
            };

            var response = _http.PostAsJsonAsync("api/usuarios", createUsuarioDto).Result;
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = response.Content.ReadAsStringAsync().Result;
                System.Diagnostics.Debug.WriteLine($"Error CrearLector: {response.StatusCode} - {errorContent}");
                return Result.Failure(ParseApiError(errorContent, "Create", "Error al crear lector."));
            }
            
            System.Diagnostics.Debug.WriteLine("Lector creado exitosamente");
            return Result.Success();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Exception CrearLector: {ex.Message}");
            return Result.Failure(new Error("Create", ex.Message));
        }
    }

    public Result DarDeBaja(int uid, int sid)
    {
        try
        {
            EnsureAuthorizationHeader();
            var usuario = CallGet<UsuarioDto>($"api/usuarios/{uid}");
            if (usuario == null) return Result.Failure(new Error("NotFound", "Usuario no encontrado"));

            usuario.Estado = false;
            usuario.UsuarioSesionId = sid;
            var response = _http.PutAsJsonAsync($"api/usuarios/{uid}", usuario).Result;
            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(new Error("Update", "Error al dar de baja"));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Delete", ex.Message));
        }
    }

    public async Task<Result> CrearUsuarioAsync(UsuarioDto d, int uid, CancellationToken ct = default)
    {
        try
        {
            EnsureAuthorizationHeader();
            d.UsuarioSesionId = uid;
            var response = await _http.PostAsJsonAsync("api/usuarios", d, ct);
            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(ParseApiError(await response.Content.ReadAsStringAsync(ct), "Create", "Error al crear usuario."));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Create", ex.Message));
        }
    }

    public async Task<Result> VerificarPasswordActualAsync(int usuarioId, string passwordActual, CancellationToken ct = default)
    {
        try
        {
            EnsureAuthorizationHeader();
            var payload = new
            {
                passwordActual = (passwordActual ?? string.Empty).Trim()
            };

            var response = await _http.PostAsJsonAsync($"api/usuarios/{usuarioId}/verificar-password-actual", payload, ct);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var errorBody = await response.Content.ReadAsStringAsync(ct);
            var fallback = "No se pudo verificar la contraseña actual.";
            var message = TryExtractApiMessage(errorBody) ?? fallback;
            return Result.Failure(new Error("VerifyPassword", message));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("VerifyPassword", ex.Message));
        }
    }

    public async Task<Result> CambiarPasswordAsync(int usuarioId, string passwordActual, string passwordNueva, string passwordConfirmacion, CancellationToken ct = default)
    {
        try
        {
            EnsureAuthorizationHeader();
            var payload = new
            {
                passwordActual = (passwordActual ?? string.Empty).Trim(),
                passwordNueva = (passwordNueva ?? string.Empty).Trim(),
                passwordConfirmacion = (passwordConfirmacion ?? string.Empty).Trim()
            };

            var response = await _http.PostAsJsonAsync($"api/usuarios/{usuarioId}/cambiar-password", payload, ct);

            if (response.IsSuccessStatusCode)
            {
                return Result.Success();
            }

            var errorBody = await response.Content.ReadAsStringAsync(ct);
            var fallback = "No se pudo cambiar la contraseña.";
            var message = TryExtractApiMessage(errorBody) ?? fallback;
            return Result.Failure(new Error("ChangePassword", message));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("ChangePassword", ex.Message));
        }
    }

    public Result<UsuarioDto> Login(string user, string pass)
    {
        try
        {
            var normalizedUser = (user ?? string.Empty).Trim();
            var normalizedPass = (pass ?? string.Empty).Trim();

            // Clear any existing auth header (we are authenticating)
            _http.DefaultRequestHeaders.Authorization = null;

            var response = _http.PostAsJsonAsync("api/usuarios/login", new { nombreUsuario = normalizedUser, password = normalizedPass }).Result;
            if (!response.IsSuccessStatusCode)
                return Result<UsuarioDto>.Failure(new Error("Login", "Credenciales inválidas"));

            var loginResponse = response.Content.ReadFromJsonAsync<LoginResponse>(JsonOptions).Result;
            var dto = loginResponse?.Usuario;
            if (dto == null)
                return Result<UsuarioDto>.Failure(new Error("Login", "Error al leer respuesta"));

            // Store token in server-side session so subsequent adapter calls can attach it
            var token = loginResponse?.Token;
            if (!string.IsNullOrWhiteSpace(token))
            {
                try
                {
                    _httpContextAccessor?.HttpContext?.Session?.SetString(SessionKeys.JwtToken, token);
                }
                catch { }
            }

            return Result<UsuarioDto>.Success(new UsuarioDto
            {
                UsuarioId = dto.UsuarioId,
                NombreUsuario = dto.NombreUsuario ?? normalizedUser,
                Nombres = dto.Nombres,
                PrimerApellido = dto.PrimerApellido,
                Rol = dto.Rol,
                Estado = dto.Estado,
                DebeCambiarPassword = dto.DebeCambiarPassword,
                Token = token
            });
        }
        catch (Exception ex)
        {
            return Result<UsuarioDto>.Failure(new Error("Login", ex.Message));
        }
    }

    private sealed class LoginResponse
    {
        public UsuarioDto? Usuario { get; set; }
        public string? Token { get; set; }
    }

    private T? CallGet<T>(string url) where T : class
    {
        try
        {
            EnsureAuthorizationHeader();
            var response = _http.GetAsync(url).Result;
            response.EnsureSuccessStatusCode();
            return response.Content.ReadFromJsonAsync<T>().Result;
        }
        catch
        {
            return null;
        }
    }

    private void EnsureAuthorizationHeader()
    {
        try
        {
            var token = _httpContextAccessor?.HttpContext?.Session?.GetString(SessionKeys.JwtToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                if (_http.DefaultRequestHeaders.Authorization == null || _http.DefaultRequestHeaders.Authorization.Parameter != token)
                    _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
            }
        }
        catch
        {
            // ignore session/accessor errors
        }
    }

    private static string? TryExtractApiMessage(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            if (document.RootElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString();
            }
            if (document.RootElement.TryGetProperty("error", out var errorElement))
            {
                return errorElement.GetString();
            }
        }
        catch
        {
            // Si no es JSON válido, devolvemos el texto original.
        }

        return responseBody;
    }

    private static Error ParseApiError(string? responseBody, string fallbackCode, string fallbackMessage)
    {
        if (!string.IsNullOrWhiteSpace(responseBody))
        {
            try
            {
                using var document = JsonDocument.Parse(responseBody);
                var root = document.RootElement;
                var code = root.TryGetProperty("code", out var codeElement)
                    ? codeElement.GetString()
                    : null;
                var message = root.TryGetProperty("message", out var messageElement)
                    ? messageElement.GetString()
                    : null;

                if (!string.IsNullOrWhiteSpace(message))
                {
                    return new Error(code ?? fallbackCode, message);
                }
            }
            catch
            {
                // El mensaje alternativo mantiene el formulario utilizable ante respuestas no JSON.
            }
        }

        return new Error(fallbackCode, fallbackMessage);
    }
}
