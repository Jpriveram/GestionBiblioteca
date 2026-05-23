using System.Net.Http.Json;
using Frontend.Dtos;
using Frontend.Helpers;

namespace Frontend.Adapters;

public class AutorAdapter : IAutorServicio
{
    private readonly HttpClient _http;

    public AutorAdapter(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ServicioAutor");
    }

    public IEnumerable<AutorDto> Select(bool todos = false)
    {
        var url = todos ? "api/autores?todos=true" : "api/autores";
        var response = _http.GetAsync(url).Result;
        response.EnsureSuccessStatusCode();
        return response.Content.ReadFromJsonAsync<List<AutorDto>>().Result ?? new();
    }

    public AutorDto? GetById(int id)
    {
        var response = _http.GetAsync($"api/autores/{id}").Result;
        if (!response.IsSuccessStatusCode) return null;
        return response.Content.ReadFromJsonAsync<AutorDto>().Result;
    }

    public Result<AutorDto> Create(AutorDto dto)
    {
        try
        {
            dto.Nombres = dto.Nombres.ToDisplayName();
            dto.Apellidos = dto.Apellidos.ToDisplayName();

            var response = _http.PostAsJsonAsync("api/autores", dto).Result;

            if (!response.IsSuccessStatusCode)
                return Result<AutorDto>.Failure(new Error("Create", "Error al crear AutorDto"));

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
            dto.Nombres = dto.Nombres.ToDisplayName();
            dto.Apellidos = dto.Apellidos.ToDisplayName();

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
            var response = _http.GetAsync("api/autores/activos").Result;

            if (!response.IsSuccessStatusCode)
                return new Dictionary<int, string>();

            return response.Content.ReadFromJsonAsync<Dictionary<int, string>>().Result ?? new Dictionary<int, string>();
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
}