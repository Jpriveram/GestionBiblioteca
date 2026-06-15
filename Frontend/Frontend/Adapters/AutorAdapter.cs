using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Frontend.Dtos;
using Frontend.Helpers;
using Microsoft.AspNetCore.Http;

namespace Frontend.Adapters;

public class AutorAdapter : IAutorServicio
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AutorAdapter(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
    {
        _http = factory.CreateClient("ServicioAutor");
        _httpContextAccessor = httpContextAccessor;
    }

    public IEnumerable<AutorDto> Select(bool todos = false)
    {
        var url = todos ? "api/autores?todos=true" : "api/autores";

        EnsureAuthorizationHeader();

        var response = _http.GetAsync(url).Result;
        response.EnsureSuccessStatusCode();

        return response.Content.ReadFromJsonAsync<List<AutorDto>>().Result ?? new List<AutorDto>();
    }

    public AutorDto? GetById(int id)
    {
        EnsureAuthorizationHeader();

        var response = _http.GetAsync($"api/autores/{id}").Result;

        if (!response.IsSuccessStatusCode)
            return null;

        return response.Content.ReadFromJsonAsync<AutorDto>().Result;
    }

    public Result<AutorDto> Create(AutorDto dto)
    {
        try
        {
            dto.Nombres = NormalizarNombreObligatorio(dto.Nombres);
            dto.Apellidos = NormalizarApellidoOpcional(dto.Apellidos);
            dto.Nacionalidad = NormalizarTextoOpcional(dto.Nacionalidad);

            EnsureAuthorizationHeader();

            var response = _http.PostAsJsonAsync("api/autores", dto).Result;

            if (!response.IsSuccessStatusCode)
                return Result<AutorDto>.Failure(new Error("Create", "Error al crear el autor."));

            var created = response.Content.ReadFromJsonAsync<AutorDto>().Result!;

            return Result<AutorDto>.Success(created);
        }
        catch (Exception ex)
        {
            return Result<AutorDto>.Failure(new Error("Create", ex.Message));
        }
    }

    public Result<AutorDto> Update(AutorDto dto)
    {
        try
        {
            dto.Nombres = NormalizarNombreObligatorio(dto.Nombres);
            dto.Apellidos = NormalizarApellidoOpcional(dto.Apellidos);
            dto.Nacionalidad = NormalizarTextoOpcional(dto.Nacionalidad);

            EnsureAuthorizationHeader();

            var response = _http.PutAsJsonAsync($"api/autores/{dto.AutorId}", dto).Result;

            return response.IsSuccessStatusCode
                ? Result<AutorDto>.Success(dto)
                : Result<AutorDto>.Failure(new Error("Update", "Error al actualizar"));
        }
        catch (Exception ex)
        {
            return Result<AutorDto>.Failure(new Error("Update", ex.Message));
        }
    }

    public Result Delete(int id, int? usuarioSesionId)
    {
        try
        {
            var url = $"api/autores/{id}";

            if (usuarioSesionId.HasValue)
                url += $"?sid={usuarioSesionId.Value}";

            EnsureAuthorizationHeader();

            var response = _http.DeleteAsync(url).Result;

            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(new Error("Delete", "Error al eliminar"));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Delete", ex.Message));
        }
    }

    public Dictionary<int, string> ObtenerAutoresActivos()
    {
        try
        {
            EnsureAuthorizationHeader();

            var response = _http.GetAsync("api/autores/activos").Result;

            if (!response.IsSuccessStatusCode)
                return new Dictionary<int, string>();

            return response.Content.ReadFromJsonAsync<Dictionary<int, string>>().Result
                   ?? new Dictionary<int, string>();
        }
        catch
        {
            return new Dictionary<int, string>();
        }
    }

    public bool ExisteAutorActivo(int autorId)
    {
        try
        {
            EnsureAuthorizationHeader();

            var response = _http.GetAsync($"api/autores/{autorId}/existe").Result;

            if (!response.IsSuccessStatusCode)
                return false;

            return response.Content.ReadFromJsonAsync<bool>().Result;
        }
        catch
        {
            return false;
        }
    }

    private void EnsureAuthorizationHeader()
    {
        try
        {
            var token = _httpContextAccessor?.HttpContext?.Session?.GetString(SessionKeys.JwtToken);

            if (!string.IsNullOrWhiteSpace(token))
            {
                if (_http.DefaultRequestHeaders.Authorization == null ||
                    _http.DefaultRequestHeaders.Authorization.Parameter != token)
                {
                    _http.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", token);
                }
            }
            else
            {
                _http.DefaultRequestHeaders.Authorization = null;
            }
        }
        catch
        {
            // Se ignoran errores de sesión para evitar romper el flujo del frontend.
        }
    }

    private static string NormalizarNombreObligatorio(string? value)
    {
        return FormatearNombrePropio(NormalizarEspacios(value));
    }

    private static string? NormalizarTextoOpcional(string? value)
    {
        var texto = FormatearNombrePropio(NormalizarEspacios(value));

        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }

    private static string? NormalizarApellidoOpcional(string? value)
    {
        var texto = NormalizarEspacios(value);

        if (string.IsNullOrWhiteSpace(texto))
            return null;

        texto = SepararPalabrasPegadasPorMayuscula(texto);
        texto = CorregirApellidosCompuestos(texto);

        return FormatearNombrePropio(texto);
    }

    private static string NormalizarEspacios(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Join(" ", value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string SepararPalabrasPegadasPorMayuscula(string? value)
    {
        var texto = NormalizarEspacios(value);

        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        return Regex.Replace(texto, @"(?<=[a-záéíóúñü])(?=[A-ZÁÉÍÓÚÑÜ])", " ");
    }

    private static string FormatearNombrePropio(string value)
    {
        value = SepararPalabrasPegadasPorMayuscula(value);

        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var palabras = value
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(CapitalizarPalabra);

        return string.Join(" ", palabras);
    }

    private static string CapitalizarPalabra(string palabra)
    {
        if (string.IsNullOrWhiteSpace(palabra))
            return string.Empty;

        if (palabra.Length == 1)
            return palabra.ToUpperInvariant();

        return char.ToUpperInvariant(palabra[0]) + palabra[1..];
    }

    private static string CorregirApellidosCompuestos(string value)
    {
        var limpio = NormalizarEspacios(value);
        var partes = limpio.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var partesCorregidas = partes.Select(CorregirParteApellidoCompuesto);

        return string.Join(" ", partesCorregidas);
    }

    private static string CorregirParteApellidoCompuesto(string value)
    {
        var clave = value.ToLowerInvariant();

        return clave switch
        {
            "delarosa" => "De La Rosa",
            "delafuente" => "De La Fuente",
            "delacruz" => "De La Cruz",
            "delatorre" => "De La Torre",
            "delvalle" => "Del Valle",
            "delrio" => "Del Río",
            "delrío" => "Del Río",
            "delosrios" => "De Los Ríos",
            "delosríos" => "De Los Ríos",
            "delossantos" => "De Los Santos",
            "delcastillo" => "Del Castillo",
            "delcampo" => "Del Campo",
            "delpilar" => "Del Pilar",
            "delmonte" => "Del Monte",
            "delpozo" => "Del Pozo",
            _ => value
        };
    }
}