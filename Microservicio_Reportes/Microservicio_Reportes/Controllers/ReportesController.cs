using System.Net.Http.Json;
using Microservicio_Reportes.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio_Reportes.Controllers;

[ApiController]
[Route("api/reportes")]
public class ReportesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ReportesController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet("libros-mas-prestados")]
    public async Task<ActionResult<IEnumerable<ReporteLibroDto>>> GetLibrosMasPrestados([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, CancellationToken cancellationToken)
    {
        var data = await ObtenerReportePrestamosAsync(fechaInicio, fechaFin, cancellationToken);

        var resultado = data
            .GroupBy(x => x.TituloLibro)
            .Select(x => new ReporteLibroDto
            {
                TituloLibro = x.Key,
                CantidadPrestamos = x.Count()
            })
            .OrderByDescending(x => x.CantidadPrestamos)
            .ToList();

        return Ok(resultado);
    }

    [HttpGet("prestamos-ordenados")]
    public async Task<ActionResult<IEnumerable<PrestamoOrdenadoDto>>> GetPrestamosOrdenados([FromQuery] DateTime? fechaInicio, [FromQuery] DateTime? fechaFin, [FromQuery] string? estadoFiltro, CancellationToken cancellationToken)
    {
        var data = await ObtenerReportePrestamosAsync(fechaInicio, fechaFin, cancellationToken);

        if (!string.IsNullOrWhiteSpace(estadoFiltro))
        {
            data = data.Where(x => string.Equals(x.EstadoTexto, estadoFiltro, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return Ok(data);
    }

    private async Task<List<PrestamoOrdenadoDto>> ObtenerReportePrestamosAsync(DateTime? fechaInicio, DateTime? fechaFin, CancellationToken cancellationToken)
    {
        var prestamoClient = _httpClientFactory.CreateClient("ServicioPrestamo");
        var usuarioClient = _httpClientFactory.CreateClient("ServicioUsuario");
        var libroEjemplarClient = _httpClientFactory.CreateClient("ServicioLibroEjemplar");

        var prestamos = await prestamoClient.GetFromJsonAsync<List<PrestamoDto>>("api/prestamos?todos=true", cancellationToken) ?? new();
        var detalles = await prestamoClient.GetFromJsonAsync<List<DetalleDto>>("api/detalles", cancellationToken) ?? new();
        var usuarios = await usuarioClient.GetFromJsonAsync<List<UsuarioDto>>("api/usuarios", cancellationToken) ?? new();
        var ejemplares = await libroEjemplarClient.GetFromJsonAsync<List<EjemplarDto>>("api/ejemplares?todos=true", cancellationToken) ?? new();
        var libros = await libroEjemplarClient.GetFromJsonAsync<List<LibroDto>>("api/libros", cancellationToken) ?? new();

        var titulosLibros = libros.ToDictionary(x => x.LibroId, x => x.Titulo);
        var ejemplaresDict = ejemplares.ToDictionary(x => x.EjemplarId, x => x);

        var resultado = new List<PrestamoOrdenadoDto>();

        foreach (var prestamo in prestamos)
        {
            if (fechaInicio.HasValue && prestamo.FechaPrestamo.Date < fechaInicio.Value.Date)
                continue;

            if (fechaFin.HasValue && prestamo.FechaPrestamo.Date > fechaFin.Value.Date)
                continue;

            var lector = usuarios.FirstOrDefault(x => x.UsuarioId == prestamo.LectorId);
            var nombreLector = lector == null
                ? "Desconocido"
                : $"{lector.Nombres} {lector.PrimerApellido} {lector.SegundoApellido ?? string.Empty}".Trim();

            var detallesPrestamo = detalles.Where(x => x.PrestamoId == prestamo.PrestamoId).ToList();
            foreach (var detalle in detallesPrestamo)
            {
                if (!ejemplaresDict.TryGetValue(detalle.EjemplarId, out var ejemplar))
                    continue;

                var tituloLibro = !string.IsNullOrWhiteSpace(ejemplar.LibroTitulo)
                    ? ejemplar.LibroTitulo!
                    : titulosLibros.TryGetValue(ejemplar.LibroId, out var titulo)
                        ? titulo
                        : "Sin título";

                var estadoTexto = prestamo.Estado == 0
                    ? "Anulado"
                    : prestamo.FechaDevolucionReal.HasValue
                        ? "Devuelto"
                        : "Activo";

                resultado.Add(new PrestamoOrdenadoDto
                {
                    FechaPrestamo = prestamo.FechaPrestamo,
                    NombreLector = nombreLector,
                    TituloLibro = tituloLibro,
                    CodigoInventario = string.IsNullOrWhiteSpace(ejemplar.CodigoInventario) ? "S/C" : ejemplar.CodigoInventario!,
                    FechaDevolucionEsperada = prestamo.FechaDevolucionEsperada,
                    FechaDevolucionReal = prestamo.FechaDevolucionReal,
                    EstadoTexto = estadoTexto
                });
            }
        }

        return resultado
            .OrderByDescending(x => x.FechaPrestamo)
            .ThenBy(x => x.NombreLector)
            .ThenBy(x => x.TituloLibro)
            .ToList();
    }
}