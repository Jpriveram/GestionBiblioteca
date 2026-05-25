using System.Net.Http.Json;
using System.Net.Http.Headers;
using Frontend.Dtos;
using Frontend.Helpers;
using Microsoft.AspNetCore.Http;

namespace Frontend.Adapters;

public class PrestamoFachadaAdapter : IPrestamoFachada
{
    private readonly IUsuarioServicio _usuarioServicio;

    public PrestamoFachadaAdapter(IUsuarioServicio usuarioServicio)
    {
        _usuarioServicio = usuarioServicio;
    }

    public IEnumerable<KeyValuePair<int, string>> BuscarEjemplaresActivos(string q) => new List<KeyValuePair<int, string>>();
    
    public IEnumerable<KeyValuePair<int, string>> BuscarLectoresPorCi(string q)
    {
        var usuarios = _usuarioServicio.Select();
        return usuarios
            .Where(u => u.CI != null && u.CI.Contains(q, StringComparison.OrdinalIgnoreCase))
            .Select(u => new KeyValuePair<int, string>(u.UsuarioId, $"{u.CI} - {u.Nombres} {u.PrimerApellido}"))
            .ToList();
    }

    public Result<int> CrearPrestamoMultiple(int lectorId, IEnumerable<int> ejIds, DateTime f, int? uid = null, string? obs = null) => Result<int>.Failure(new Error("NotImpl", "Not implemented"));
    public Result<int> CrearPrestamoMultiple(int lectorId, IEnumerable<(int, string?)> d, DateTime f, int? uid = null) => Result<int>.Failure(new Error("NotImpl", "Not implemented"));
    public Result CrearPrestamo(PrestamoDto p) => Result.Failure(new Error("NotImpl", "Not implemented"));
    public Result CrearPrestamos(IEnumerable<PrestamoDto> p) => Result.Failure(new Error("NotImpl", "Not implemented"));
    public int CountPrestamosActivos(int id) => 0;
    public PrestamoDto? ObtenerPrestamoPorId(int id) => null;
    public EjemplarDto? ObtenerEjemplarPorId(int id) => null;
    public string? ObtenerLabelEjemplar(int id) => null;
    
    public UsuarioDto? ObtenerUsuarioPorCi(string ci)
    {
        var usuarios = _usuarioServicio.Select();
        return usuarios.FirstOrDefault(u => u.CI != null && u.CI.Equals(ci, StringComparison.OrdinalIgnoreCase));
    }
    
    public List<object> ObtenerTodosLosLectores()
    {
        var usuarios = _usuarioServicio.Select();
        return usuarios.Cast<object>().ToList();
    }
}

public class AnulacionFachadaAdapter : IAnulacionFachada
{
    private readonly HttpClient _http;

    private readonly IHttpContextAccessor? _httpContextAccessor;

    public AnulacionFachadaAdapter(IHttpClientFactory factory, IHttpContextAccessor httpContextAccessor)
    {
        _http = factory.CreateClient("ServicioPrestamo");
        _httpContextAccessor = httpContextAccessor;
    }

    public Result AnularPrestamo(int id, int? uid, string m)
    {
        try
        {
            EnsureAuthorizationHeader();
            var response = _http.PostAsJsonAsync($"api/prestamos/{id}/anular", new { usuarioSesionId = uid, motivo = m }).Result;
            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(new Error("Anulacion", "Error al anular el préstamo."));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Anulacion", ex.Message));
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
}

public class EjemplarDisponibilidadFachadaAdapter : IEjemplarDisponibilidadFachada
{
    public Result CambiarDisponibilidad(int id, bool d, int? uid) => Result.Success();
}

public class PrestamoServicioAdapter : IPrestamoServicio
{
    private readonly HttpClient _http;

    public PrestamoServicioAdapter(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ServicioPrestamo");
    }

    public IEnumerable<PrestamoDto> Select(bool todos = false)
    {
        try
        {
            var url = todos ? "api/prestamos?todos=true" : "api/prestamos";
            var response = _http.GetAsync(url).Result;
            if (!response.IsSuccessStatusCode) return new List<PrestamoDto>();
            return response.Content.ReadFromJsonAsync<List<PrestamoDto>>().Result ?? new List<PrestamoDto>();
        }
        catch { return new List<PrestamoDto>(); }
    }

    public Result<PrestamoDto> Create(PrestamoDto d) => Result<PrestamoDto>.Failure(new Error("NotImpl", "Not implemented"));
    public Result<PrestamoDto> Update(PrestamoDto d) => Result<PrestamoDto>.Failure(new Error("NotImpl", "Not implemented"));

    public Result Delete(PrestamoDto d)
    {
        try
        {
            var response = _http.DeleteAsync($"api/prestamos/{d.PrestamoId}").Result;
            return response.IsSuccessStatusCode
                ? Result.Success()
                : Result.Failure(new Error("Prestamo", "Error al eliminar."));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Prestamo", ex.Message));
        }
    }

    public PrestamoDto? GetById(int id)
    {
        try
        {
            var response = _http.GetAsync($"api/prestamos/{id}").Result;
            if (!response.IsSuccessStatusCode) return null;
            return response.Content.ReadFromJsonAsync<PrestamoDto>().Result;
        }
        catch { return null; }
    }

    public Result ValidarPrestamo(PrestamoDto p) => Result.Success();
    public int CountPrestamosActivos(int id) => 0;
    public int InsertAndReturnId(PrestamoDto p) => 0;
}

public class DetalleServicioAdapter : IDetalleServicio
{
    private readonly HttpClient _http;

    public DetalleServicioAdapter(IHttpClientFactory factory)
    {
        _http = factory.CreateClient("ServicioPrestamo");
    }

    public IEnumerable<DetalleDto> Select()
    {
        return ObtenerTodos();
    }

    public IEnumerable<DetalleDto> ObtenerPorPrestamo(int prestamoId)
    {
        try
        {
            var response = _http.GetAsync($"api/detalles/prestamo/{prestamoId}").Result;
            if (!response.IsSuccessStatusCode) return new List<DetalleDto>();
            return response.Content.ReadFromJsonAsync<List<DetalleDto>>().Result ?? new List<DetalleDto>();
        }
        catch { return new List<DetalleDto>(); }
    }

    public IEnumerable<DetalleDto> ObtenerTodos()
    {
        try
        {
            var response = _http.GetAsync("api/detalles").Result;
            if (!response.IsSuccessStatusCode) return new List<DetalleDto>();
            return response.Content.ReadFromJsonAsync<List<DetalleDto>>().Result ?? new List<DetalleDto>();
        }
        catch { return new List<DetalleDto>(); }
    }

    public Result CrearMultiples(IEnumerable<DetalleDto> d) => Result.Success();
}
