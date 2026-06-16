using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using Frontend.Dtos;
using Frontend.Helpers;
using Microsoft.AspNetCore.Http;

namespace Frontend.Adapters;

public class LibroAdapter : ILibroServicio
{
    private readonly HttpClient _http;
    private readonly HttpClient _httpAutor;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LibroAdapter(IHttpClientFactory f, IHttpContextAccessor httpContextAccessor)
    {
        _http = f.CreateClient("ServicioLibroEjemplar");
        _httpAutor = f.CreateClient("ServicioAutor");
        _httpContextAccessor = httpContextAccessor;
    }

    public IEnumerable<LibroDto> Select(bool todos = false) =>
        CallGet<List<LibroDto>>(todos ? "api/libros?todos=true" : "api/libros") ?? new();

    public LibroDto? GetById(int id) =>
        CallGet<LibroDto>($"api/libros/{id}");

    public Result Create(LibroDto dto, string? n) =>
    CallPost("api/libros", dto);

    public Result Update(LibroDto dto) =>
        CallPut($"api/libros/{dto.LibroId}", dto);

    public Result Delete(int id, int? uid) =>
        CallDelete($"api/libros/{id}");

    public Dictionary<int, string> ObtenerNombresAutores()
    {
        try
        {
            var autores = _httpAutor.GetFromJsonAsync<List<AutorDto>>("api/Autores").Result
                          ?? new List<AutorDto>();

            return autores
                .Where(a => a.Estado)
                .ToDictionary(
                    a => a.AutorId,
                    a => $"{a.Nombres} {a.Apellidos} ({a.Nacionalidad})".Trim()
                );
        }
        catch
        {
            return new Dictionary<int, string>();
        }
    }

    public IEnumerable<AutorDto> ObtenerAutoresActivos()
    {
        try
        {
            var autores = _httpAutor.GetFromJsonAsync<List<AutorDto>>("api/Autores").Result
                          ?? new List<AutorDto>();

            return autores.Where(a => a.Estado);
        }
        catch
        {
            return new List<AutorDto>();
        }
    }

    public bool ExisteAutorActivo(int id)
    {
        try
        {
            var autor = _httpAutor.GetFromJsonAsync<AutorDto>($"api/Autores/{id}").Result;
            return autor != null && autor.Estado;
        }
        catch
        {
            return false;
        }
    }

    public int InsertarAutorYObtenerID(string n, int? uid) => 0;

    private T? CallGet<T>(string url) where T : class
    {
        try
        {
            EnsureAuthorizationHeader();
            var r = _http.GetAsync(url).Result;
            r.EnsureSuccessStatusCode();
            return r.Content.ReadFromJsonAsync<T>().Result;
        }
        catch
        {
            return null;
        }
    }

    private Result CallPost(string url, object d)
    {
        try
        {
            EnsureAuthorizationHeader();
            var r = _http.PostAsJsonAsync(url, d).Result;
            return r.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(LeerError(r, "Post"));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Post", ex.Message));
        }
    }

    private Result CallPut(string url, object d)
    {
        try
        {
            EnsureAuthorizationHeader();
            var r = _http.PutAsJsonAsync(url, d).Result;
            return r.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(LeerError(r, "Put"));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Put", ex.Message));
        }
    }

    private Result CallDelete(string url)
    {
        try
        {
            EnsureAuthorizationHeader();
            var r = _http.DeleteAsync(url).Result;
            return r.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(new Error("Delete", "Error"));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Delete", ex.Message));
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
            // ignore
        }
    }

    private static Error LeerError(HttpResponseMessage response, string fallbackCode)
    {
        var content = response.Content.ReadAsStringAsync().Result;

        if (string.IsNullOrWhiteSpace(content))
            return new Error(fallbackCode, "Error al procesar.");

        try
        {
            using var json = JsonDocument.Parse(content);
            var root = json.RootElement;

            if (root.TryGetProperty("errors", out var errorsElement)
                && errorsElement.ValueKind == JsonValueKind.Object)
            {
                foreach (var fieldError in errorsElement.EnumerateObject())
                {
                    if (fieldError.Value.ValueKind != JsonValueKind.Array
                        || fieldError.Value.GetArrayLength() == 0)
                    {
                        continue;
                    }

                    var message = fieldError.Value[0].GetString();
                    if (!string.IsNullOrWhiteSpace(message))
                    {
                        return new Error(fieldError.Name, message);
                    }
                }
            }

            var code = root.TryGetProperty("code", out var codeElement)
                ? codeElement.GetString()
                : fallbackCode;

            if (root.TryGetProperty("error", out var errorElement))
            {
                var message = errorElement.GetString();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return new Error(code ?? fallbackCode, message);
                }
            }

            if (root.TryGetProperty("message", out var messageElement))
            {
                var message = messageElement.GetString();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return new Error(code ?? fallbackCode, message);
                }
            }

            if (root.TryGetProperty("title", out var titleElement))
            {
                var title = titleElement.GetString();
                if (!string.IsNullOrWhiteSpace(title)
                    && !string.Equals(title, "One or more validation errors occurred.", StringComparison.OrdinalIgnoreCase))
                {
                    return new Error(code ?? fallbackCode, title);
                }
            }
        }
        catch
        {
            return new Error(fallbackCode, content);
        }

        return new Error(fallbackCode, "Error al procesar la solicitud.");
    }
}