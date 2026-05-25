using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Text.Json;
using Frontend.Dtos;
using Frontend.Helpers;
using Microsoft.AspNetCore.Http;

namespace Frontend.Adapters;

public class MultaAdapter : IMultaServicio
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor? _httpContextAccessor;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public MultaAdapter(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
    {
        _http = factory.CreateClient("ServicioMultas");
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IEnumerable<MultaDto>> SelectAsync(int? usuarioId = null)
    {
        try
        {
            EnsureAuthorizationHeader();
            var url = usuarioId.HasValue ? $"api/multas?usuarioId={usuarioId.Value}" : "api/multas";
            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode) return new List<MultaDto>();
            return await response.Content.ReadFromJsonAsync<List<MultaDto>>(JsonOptions) ?? new();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"MultaAdapter.SelectAsync error: {ex.Message}");
            return new List<MultaDto>();
        }
    }

    public async Task<MultaDto?> GetByIdAsync(string id)
    {
        try
        {
            EnsureAuthorizationHeader();
            var response = await _http.GetAsync($"api/multas/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<MultaDto>(JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    public async Task<Result<MultaDto>> CreateAsync(MultaDto dto)
    {
        try
        {
            var payload = new
            {
                usuarioId = dto.UsuarioId,
                prestamoId = dto.PrestamoId,
                monto = dto.Monto,
                motivo = dto.Motivo,
                usuarioSesionId = dto.UsuarioSesionId
            };

            EnsureAuthorizationHeader();
            var response = await _http.PostAsJsonAsync("api/multas", payload);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var message = TryExtractMessage(errorBody) ?? "Error al crear multa.";
                return Result<MultaDto>.Failure(new Error("Create", message));
            }

            var created = await response.Content.ReadFromJsonAsync<MultaDto>(JsonOptions);
            return Result<MultaDto>.Success(created ?? dto);
        }
        catch (Exception ex)
        {
            return Result<MultaDto>.Failure(new Error("Create", ex.Message));
        }
    }

    public async Task<Result<MultaDto>> UpdateAsync(MultaDto dto)
    {
        try
        {
            var payload = new
            {
                monto = dto.Monto,
                motivo = dto.Motivo,
                estado = dto.Estado,
                usuarioSesionId = dto.UsuarioSesionId
            };

            EnsureAuthorizationHeader();
            var response = await _http.PutAsJsonAsync($"api/multas/{dto.Id}", payload);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                var message = TryExtractMessage(errorBody) ?? "Error al actualizar multa.";
                return Result<MultaDto>.Failure(new Error("Update", message));
            }

            var updated = await response.Content.ReadFromJsonAsync<MultaDto>(JsonOptions);
            return Result<MultaDto>.Success(updated ?? dto);
        }
        catch (Exception ex)
        {
            return Result<MultaDto>.Failure(new Error("Update", ex.Message));
        }
    }

    public async Task<Result> DeleteAsync(string id)
    {
        try
        {
            EnsureAuthorizationHeader();
            var response = await _http.DeleteAsync($"api/multas/{id}");
            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(new Error("Delete", "Error al eliminar multa."));
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
            // ignore session/accessor errors
        }
    }

    private static string? TryExtractMessage(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody)) return null;
        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            if (doc.RootElement.TryGetProperty("message", out var msg))
                return msg.GetString();
            if (doc.RootElement.TryGetProperty("error", out var err))
                return err.GetString();
        }
        catch { }
        return responseBody;
    }
}
