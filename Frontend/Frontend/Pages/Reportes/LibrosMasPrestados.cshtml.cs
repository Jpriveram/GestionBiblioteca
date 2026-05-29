using Frontend.Adapters;
using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Reportes;

public class LibrosMasPrestadosModel : PageModel
{
    private readonly IPrestamoServicio _prestamoServicio;
    private readonly IDetalleServicio _detalleServicio;
    private readonly IEjemplarServicio _ejemplarServicio;

    public List<ReporteLibro> Reporte { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaInicio { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaFin { get; set; }

    public LibrosMasPrestadosModel(
        IPrestamoServicio prestamoServicio,
        IDetalleServicio detalleServicio,
        IEjemplarServicio ejemplarServicio)
    {
        _prestamoServicio = prestamoServicio;
        _detalleServicio = detalleServicio;
        _ejemplarServicio = ejemplarServicio;
    }

    public IActionResult OnGet()
    {
        if (!UsuarioEsBibliotecario())
        {
            return Redirect("/");
        }

        CargarReporte();
        return Page();
    }

    private bool UsuarioEsBibliotecario()
    {
        var rolSesion = HttpContext.Session.GetString(SessionKeys.Rol);

        return string.Equals(
            rolSesion,
            Roles.Bibliotecario,
            StringComparison.OrdinalIgnoreCase);
    }

    private void CargarReporte()
    {
        var prestamos = _prestamoServicio.Select();

        if (FechaInicio.HasValue)
        {
            prestamos = prestamos
                .Where(x => x.FechaPrestamo.Date >= FechaInicio.Value.Date)
                .ToList();
        }

        if (FechaFin.HasValue)
        {
            prestamos = prestamos
                .Where(x => x.FechaPrestamo.Date <= FechaFin.Value.Date)
                .ToList();
        }

        var detalles = _detalleServicio.ObtenerTodos();
        var titulosLibros = _ejemplarServicio.ObtenerTitulosLibros();

        var resultado = new Dictionary<string, int>();

        foreach (var prestamo in prestamos)
        {
            var detallesPrestamo = detalles
                .Where(d => d.PrestamoId == prestamo.PrestamoId);

            foreach (var detalle in detallesPrestamo)
            {
                var ejemplar = _ejemplarServicio.GetById(detalle.EjemplarId);

                if (ejemplar == null)
                    continue;

                var titulo = !string.IsNullOrWhiteSpace(ejemplar.LibroTitulo)
                    ? ejemplar.LibroTitulo
                    : titulosLibros.TryGetValue(ejemplar.LibroId, out var tituloLibro)
                        ? tituloLibro
                        : "Sin título";

                if (resultado.ContainsKey(titulo))
                    resultado[titulo]++;
                else
                    resultado[titulo] = 1;
            }
        }

        Reporte = resultado
            .Select(x => new ReporteLibro
            {
                TituloLibro = x.Key,
                CantidadPrestamos = x.Value
            })
            .OrderByDescending(x => x.CantidadPrestamos)
            .ToList();
    }

    public class ReporteLibro
    {
        public string TituloLibro { get; set; } = string.Empty;
        public int CantidadPrestamos { get; set; }
    }
}