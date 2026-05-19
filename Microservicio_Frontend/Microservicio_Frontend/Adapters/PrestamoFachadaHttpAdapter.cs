using System.Net.Http.Json;
using Frontend.Dtos;
using Frontend.Helpers;

namespace Frontend.Adapters;

public class PrestamoFachadaHttpAdapter : IPrestamoFachada
{
    private readonly HttpClient _http;
    private readonly IEjemplarServicio _ejemplarServicio;
    private readonly IUsuarioServicio _usuarioServicio;

    public PrestamoFachadaHttpAdapter(IHttpClientFactory factory, IEjemplarServicio ejemplarServicio, IUsuarioServicio usuarioServicio)
    {
        _http = factory.CreateClient("ServicioPrestamo");
        _ejemplarServicio = ejemplarServicio;
        _usuarioServicio = usuarioServicio;
    }

    public IEnumerable<KeyValuePair<int, string>> BuscarEjemplaresActivos(string q)
    {
        try
        {
            var ejemplares = _ejemplarServicio.ObtenerEjemplaresDisponibles();
            if (string.IsNullOrWhiteSpace(q))
                return ejemplares;

            var lower = q.ToLowerInvariant();
            return ejemplares.Where(kv => kv.Value.ToLowerInvariant().Contains(lower)).ToList();
        }
        catch
        {
            return new List<KeyValuePair<int, string>>();
        }
    }

    public IEnumerable<KeyValuePair<int, string>> BuscarLectoresPorCi(string q)
    {
        try
        {
            var usuarios = _usuarioServicio.Select();
            return usuarios
                .Where(u => u.CI != null && u.CI.Contains(q, StringComparison.OrdinalIgnoreCase))
                .Select(u => new KeyValuePair<int, string>(u.UsuarioId, $"{u.CI} - {u.Nombres} {u.PrimerApellido}"))
                .ToList();
        }
        catch
        {
            return new List<KeyValuePair<int, string>>();
        }
    }

    public Result<int> CrearPrestamoMultiple(int lectorId, IEnumerable<int> ejemplarIds, DateTime fechaDevolucionEsperada, int? usuarioSesionId = null, string? observacionesSalida = null)
    {
        try
        {
            var ejemplares = ejemplarIds.Select(id => new { ejemplarId = id, observacionesSalida }).ToList();

            var response = _http.PostAsJsonAsync("api/prestamos", new
            {
                lectorId,
                ejemplares,
                fechaDevolucionEsperada,
                usuarioSesionId
            }).Result;

            if (!response.IsSuccessStatusCode)
                return Result<int>.Failure(new Error("Prestamo", "Error al crear préstamo"));

            var result = response.Content.ReadFromJsonAsync<int>().Result;
            return Result<int>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(new Error("Prestamo", ex.Message));
        }
    }

    public Result<int> CrearPrestamoMultiple(int lectorId, IEnumerable<(int EjemplarId, string? ObservacionesSalida)> detallesEjemplares, DateTime fechaDevolucionEsperada, int? usuarioSesionId = null)
    {
        try
        {
            var ejemplares = detallesEjemplares.Select(d => new { ejemplarId = d.EjemplarId, observacionesSalida = d.ObservacionesSalida }).ToList();

            var response = _http.PostAsJsonAsync("api/prestamos", new
            {
                lectorId,
                ejemplares,
                fechaDevolucionEsperada,
                usuarioSesionId
            }).Result;

            if (!response.IsSuccessStatusCode)
                return Result<int>.Failure(new Error("Prestamo", "Error al crear préstamo"));

            var result = response.Content.ReadFromJsonAsync<int>().Result;
            return Result<int>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(new Error("Prestamo", ex.Message));
        }
    }

    public Result CrearPrestamo(PrestamoDto p)
    {
        try
        {
            var response = _http.PostAsJsonAsync("api/prestamos", p).Result;
            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(new Error("Prestamo", "Error"));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Prestamo", ex.Message));
        }
    }

    public Result CrearPrestamos(IEnumerable<PrestamoDto> prestamos)
    {
        try
        {
            var response = _http.PostAsJsonAsync("api/prestamos/batch", prestamos).Result;
            return response.IsSuccessStatusCode ? Result.Success() : Result.Failure(new Error("Prestamo", "Error"));
        }
        catch (Exception ex)
        {
            return Result.Failure(new Error("Prestamo", ex.Message));
        }
    }

    public int CountPrestamosActivos(int lectorId)
    {
        try
        {
            var result = _http.GetAsync($"api/prestamos/activos/count/{lectorId}").Result;
            if (!result.IsSuccessStatusCode) return 0;
            var count = result.Content.ReadFromJsonAsync<int>().Result;
            return count;
        }
        catch
        {
            return 0;
        }
    }

    public PrestamoDto? ObtenerPrestamoPorId(int id)
    {
        try
        {
            var result = _http.GetAsync($"api/prestamos/{id}").Result;
            if (!result.IsSuccessStatusCode) return null;
            return result.Content.ReadFromJsonAsync<PrestamoDto>().Result;
        }
        catch
        {
            return null;
        }
    }

    public EjemplarDto? ObtenerEjemplarPorId(int id)
    {
        try
        {
            return _ejemplarServicio.GetById(id);
        }
        catch
        {
            return null;
        }
    }

    public string? ObtenerLabelEjemplar(int ejemplarId)
    {
        try
        {
            var ejemplar = _ejemplarServicio.GetById(ejemplarId);
            if (ejemplar == null) return null;
            var titulos = _ejemplarServicio.ObtenerTitulosLibros();
            if (titulos.TryGetValue(ejemplar.LibroId, out var titulo))
                return $"{titulo} ({ejemplar.CodigoInventario})";
            return null;
        }
        catch
        {
            return null;
        }
    }

    public UsuarioDto? ObtenerUsuarioPorCi(string ci)
    {
        try
        {
            var usuarios = _usuarioServicio.Select();
            return usuarios.FirstOrDefault(u => u.CI != null && u.CI.Equals(ci, StringComparison.OrdinalIgnoreCase));
        }
        catch
        {
            return null;
        }
    }

    public List<object> ObtenerTodosLosLectores()
    {
        try
        {
            var usuarios = _usuarioServicio.Select();
            return usuarios.Cast<object>().ToList();
        }
        catch
        {
            return new List<object>();
        }
    }
}
