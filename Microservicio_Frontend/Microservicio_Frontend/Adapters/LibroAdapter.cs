using System.Net.Http.Json;
using System.Text.Json;
using Frontend.Dtos;
using Frontend.Adapters;
using Frontend.Helpers;
using Frontend.Dtos;

namespace Frontend.Adapters;

public class LibroAdapter : ILibroServicio
{
    private readonly HttpClient _http;
    public LibroAdapter(IHttpClientFactory f) => _http = f.CreateClient("ServicioLibroEjemplar");

    public IEnumerable<LibroDto> Select(bool todos = false) => CallGet<List<LibroDto>>(todos ? "api/libros?todos=true" : "api/libros") ?? new();
    public LibroDto? GetById(int id) => CallGet<LibroDto>($"api/libros/{id}");
    public Result Create(LibroDto dto, string? n) => CallPost("api/libros", new { libro = dto, nombreAutorNuevo = n });
    public Result Update(LibroDto dto) => CallPut($"api/libros/{dto.LibroId}", dto);
    public Result Delete(int id, int? uid) => CallDelete($"api/libros/{id}");
    public Dictionary<int, string> ObtenerNombresAutores() => CallGet<Dictionary<int, string>>("api/libros/autores-nombres") ?? new();
    public IEnumerable<AutorDto> ObtenerAutoresActivos() => CallGet<List<AutorDto>>("api/libros/autores-activos") ?? new();
    public bool ExisteAutorActivo(int id)
    {
        try
        {
            var r = _http.GetAsync($"api/autores/{id}/existe").Result;
            if (!r.IsSuccessStatusCode) return false;
            return r.Content.ReadFromJsonAsync<bool>().Result;
        }
        catch { return false; }
    }
    public int InsertarAutorYObtenerID(string n, int? uid) => 0;

    private T? CallGet<T>(string url) where T : class { try { var r = _http.GetAsync(url).Result; r.EnsureSuccessStatusCode(); return r.Content.ReadFromJsonAsync<T>().Result; } catch { return null; } }
    private Result CallPost(string url, object d) { try { var r = _http.PostAsJsonAsync(url, d).Result; return r.IsSuccessStatusCode ? Result.Success() : Result.Failure(LeerError(r, "Post")); } catch (Exception ex) { return Result.Failure(new Error("Post", ex.Message)); } }
    private Result CallPut(string url, object d) { try { var r = _http.PutAsJsonAsync(url, d).Result; return r.IsSuccessStatusCode ? Result.Success() : Result.Failure(LeerError(r, "Put")); } catch (Exception ex) { return Result.Failure(new Error("Put", ex.Message)); } }
    private Result CallDelete(string url) { try { var r = _http.DeleteAsync(url).Result; return r.IsSuccessStatusCode ? Result.Success() : Result.Failure(new Error("Delete", "Error")); } catch (Exception ex) { return Result.Failure(new Error("Delete", ex.Message)); } }

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
