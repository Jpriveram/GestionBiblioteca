using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Dtos;
using Frontend.Helpers;

namespace Frontend.Adapters;

public class LibroAdapter : ILibroServicio
{
    private readonly HttpClient _http;
    private readonly HttpClient _httpAutor;

    public LibroAdapter(IHttpClientFactory f)
    {
        _http = f.CreateClient("ServicioLibroEjemplar");
        _httpAutor = f.CreateClient("ServicioAutor");
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

    private static Error LeerError(HttpResponseMessage response, string fallbackCode)
    {
        var content = response.Content.ReadAsStringAsync().Result;

        if (string.IsNullOrWhiteSpace(content))
            return new Error(fallbackCode, "Error al procesar.");

        try
        {
            using var json = JsonDocument.Parse(content);

            var code = json.RootElement.TryGetProperty("code", out var codeElement)
                ? codeElement.GetString()
                : fallbackCode;

            var message = json.RootElement.TryGetProperty("error", out var errorElement)
                ? errorElement.GetString()
                : content;

            return new Error(code ?? fallbackCode, message ?? "Error al procesar.");
        }
        catch
        {
            return new Error(fallbackCode, content);
        }
    }
}