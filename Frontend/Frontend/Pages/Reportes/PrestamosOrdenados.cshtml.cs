using Frontend.Adapters;
using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Reportes;

public class PrestamosOrdenadosModel : PageModel
{
    private readonly IPrestamoServicio _prestamoServicio;
    private readonly IDetalleServicio _detalleServicio;
    private readonly IEjemplarServicio _ejemplarServicio;
    private readonly IUsuarioServicio _usuarioServicio;

    public List<PrestamoOrdenadoReporte> Reporte { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaInicio { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FechaFin { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? EstadoFiltro { get; set; }

    public PrestamosOrdenadosModel(
        IPrestamoServicio prestamoServicio,
        IDetalleServicio detalleServicio,
        IEjemplarServicio ejemplarServicio,
        IUsuarioServicio usuarioServicio)
    {
        _prestamoServicio = prestamoServicio;
        _detalleServicio = detalleServicio;
        _ejemplarServicio = ejemplarServicio;
        _usuarioServicio = usuarioServicio;
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
        var prestamos = _prestamoServicio.Select(todos: true);

        if (FechaInicio.HasValue)
        {
            prestamos = prestamos.Where(x => x.FechaPrestamo.Date >= FechaInicio.Value.Date).ToList();
        }

        if (FechaFin.HasValue)
        {
            prestamos = prestamos.Where(x => x.FechaPrestamo.Date <= FechaFin.Value.Date).ToList();
        }

        var detalles = _detalleServicio.ObtenerTodos(todos: true);
        var titulosLibros = _ejemplarServicio.ObtenerTitulosLibros();
        var usuarios = _usuarioServicio.Select();

        var resultado = new List<PrestamoOrdenadoReporte>();

        foreach (var prestamo in prestamos)
        {
            var lector = usuarios.FirstOrDefault(u => u.UsuarioId == prestamo.LectorId);

            var nombreLector = lector == null
                ? "Desconocido"
                : $"{lector.Nombres} {lector.PrimerApellido} {lector.SegundoApellido ?? string.Empty}".Trim();

            var detallesPrestamo = detalles.Where(d => d.PrestamoId == prestamo.PrestamoId);

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

                var estadoTexto = prestamo.Estado == 0
                    ? "Anulado"
                    : prestamo.FechaDevolucionReal.HasValue
                        ? "Devuelto"
                        : "Activo";

                resultado.Add(new PrestamoOrdenadoReporte
                {
                    FechaPrestamo = prestamo.FechaPrestamo,
                    NombreLector = nombreLector,
                    TituloLibro = titulo,
                    CodigoInventario = string.IsNullOrWhiteSpace(ejemplar.CodigoInventario)
                        ? "S/C"
                        : ejemplar.CodigoInventario,
                    FechaDevolucionEsperada = prestamo.FechaDevolucionEsperada,
                    FechaDevolucionReal = prestamo.FechaDevolucionReal,
                    EstadoTexto = estadoTexto
                });
            }
        }

        if (!string.IsNullOrWhiteSpace(EstadoFiltro))
        {
            resultado = resultado.Where(x => x.EstadoTexto == EstadoFiltro).ToList();
        }

        Reporte = resultado
            .OrderByDescending(x => x.FechaPrestamo)
            .ThenBy(x => x.NombreLector)
            .ThenBy(x => x.TituloLibro)
            .ToList();
    }

    public class PrestamoOrdenadoReporte
    {
        public DateTime FechaPrestamo { get; set; }
        public string NombreLector { get; set; } = string.Empty;
        public string TituloLibro { get; set; } = string.Empty;
        public string CodigoInventario { get; set; } = string.Empty;
        public DateTime FechaDevolucionEsperada { get; set; }
        public DateTime? FechaDevolucionReal { get; set; }
        public string EstadoTexto { get; set; } = string.Empty;
    }
}
