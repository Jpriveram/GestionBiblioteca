using System.Net.Http.Json;
using System.Text.Json;
using System.Net.Http.Headers;
using Frontend.Dtos;
using Microsoft.AspNetCore.Http;
using Frontend.Adapters;
using Frontend.Helpers;

namespace Frontend.Adapters;

public class EjemplarAdapter : IEjemplarServicio
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public EjemplarAdapter(IHttpClientFactory f, IHttpContextAccessor httpContextAccessor) { _http = f.CreateClient("ServicioLibroEjemplar"); _httpContextAccessor = httpContextAccessor; }

    public IEnumerable<EjemplarDto> Select(bool todos = false) => CallGet<List<EjemplarDto>>(todos ? "api/ejemplares?todos=true" : "api/ejemplares") ?? new();
    public EjemplarDto? GetById(int id) => CallGet<EjemplarDto>($"api/ejemplares/{id}");
    public Result<EjemplarDto> Create(EjemplarDto d) => CallPostR<EjemplarDto>("api/ejemplares", d);
    public Result<EjemplarDto> Update(EjemplarDto d) => CallPutR<EjemplarDto>($"api/ejemplares/{d.EjemplarId}", d);
    public Result Delete(EjemplarDto d) => CallDeleteR($"api/ejemplares/{d.EjemplarId}");
    public Dictionary<int, string> ObtenerTitulosLibros()
    {
        var libros = CallGet<List<LibroDto>>("api/libros?todos=true") ?? new();

        return libros.ToDictionary(
            l => l.LibroId,
            l => l.Titulo
        );
    }
    public IEnumerable<LibroDto> ObtenerLibrosActivos() =>
        (CallGet<List<LibroDto>>("api/libros") ?? new()).Where(l => l.Estado);
    public bool ExisteLibroActivo(int id)
    {
        var libro = CallGet<LibroDto>($"api/libros/{id}");
        return libro != null && libro.Estado;
    }
    public Dictionary<int, string> ObtenerEjemplaresDisponibles()
    {
        try
        {
            EnsureAuthorizationHeader();
            var response = _http.GetAsync("api/ejemplares/disponibles").Result;
            if (!response.IsSuccessStatusCode) return new();
            var ejemplares = response.Content.ReadFromJsonAsync<List<EjemplarDto>>().Result ?? new();
            var titulos = ObtenerTitulosLibros();
            return ejemplares.ToDictionary(
                e => e.EjemplarId,
                e => titulos.TryGetValue(e.LibroId, out var titulo)
                    ? $"{titulo} ({e.CodigoInventario})"
                    : $"Libro #{e.LibroId} ({e.CodigoInventario})"
            );
        }
        catch { return new(); }
    }
    public Result ValidarEjemplar(EjemplarDto e) => Result.Success();

    private T? CallGet<T>(string u) where T : class { try { EnsureAuthorizationHeader(); var r = _http.GetAsync(u).Result; r.EnsureSuccessStatusCode(); return r.Content.ReadFromJsonAsync<T>().Result; } catch { return null; } }
    private Result<EjemplarDto> CallPostR<T>(string u, object d) { try { EnsureAuthorizationHeader(); var r = _http.PostAsJsonAsync(u, d).Result; if (!r.IsSuccessStatusCode) return Result<EjemplarDto>.Failure(new Error("Post", LeerError(r))); return Result<EjemplarDto>.Success(r.Content.ReadFromJsonAsync<EjemplarDto>().Result!); } catch (Exception ex) { return Result<EjemplarDto>.Failure(new Error("Post", ex.Message)); } }
    private Result<EjemplarDto> CallPutR<T>(string u, object d)
    {
        try
        {
            EnsureAuthorizationHeader();

            var r = _http.PutAsJsonAsync(u, d).Result;

            return r.IsSuccessStatusCode
                ? Result<EjemplarDto>.Success(d as EjemplarDto ?? new())
                : Result<EjemplarDto>.Failure(new Error("Put", LeerError(r)));
        }
        catch (Exception ex)
        {
            return Result<EjemplarDto>.Failure(new Error("Put", ex.Message));
        }
    }
    private Result CallDeleteR(string u) { try { EnsureAuthorizationHeader(); var r = _http.DeleteAsync(u).Result; return r.IsSuccessStatusCode ? Result.Success() : Result.Failure(new Error("Delete", "Error")); } catch (Exception ex) { return Result.Failure(new Error("Delete", ex.Message)); } }
    
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
            // ignore
        }
    }

    private static string LeerError(HttpResponseMessage response)
    {
        var content = response.Content.ReadAsStringAsync().Result;

        if (string.IsNullOrWhiteSpace(content))
            return "Error al procesar.";

        try
        {
            using var json = JsonDocument.Parse(content);

            if (json.RootElement.TryGetProperty("message", out var message))
                return message.GetString() ?? "Error al procesar.";

            if (json.RootElement.TryGetProperty("error", out var error))
                return error.GetString() ?? "Error al procesar.";

            if (json.RootElement.TryGetProperty("title", out var title))
                return title.GetString() ?? "Error al procesar.";
        }
        catch
        {
        }

        return content;
    }
}
