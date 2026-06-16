using Frontend.Dtos;
using Frontend.Adapters;
using Frontend.Adapters;
using Frontend.Dtos;
using Frontend.Helpers;
using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Linq;
using PrestamoEntity = Frontend.Dtos.PrestamoDto;

namespace Frontend.Pages.Prestamo;

public class PrestamoModel : PageModel
{
    private readonly Frontend.Adapters.IPrestamoFachada _prestamoFachada;
    private readonly IAnulacionFachada _anulacionFachada;
    private readonly IPrestamoServicio _prestamoServicio;
    private readonly IEjemplarServicio _ejemplarServicio;
    private readonly IUsuarioServicio _usuarioServicio;
    private readonly IDetalleServicio _detalleServicio;
    private readonly RouteTokenService _routeTokenService;

    public List<PrestamoEntity> Prestamos { get; set; } = new();
    public List<PrestamoDetalleDTO> PrestamosDetallados { get; set; } = new();
    public Dictionary<int, string> LibrosTitulos { get; set; } = new();

    public DateTime FechaPrestamoDisplay { get; set; }
    public DateTime FechaDevolucionDefault { get; set; }

    [BindProperty]
    public PrestamoEntity NuevoPrestamo { get; set; } = new();

    [BindProperty]
    public Frontend.Dtos.LectorDto NuevoLector { get; set; } = new();

    public string? MensajeError { get; set; }
    public string? MensajeErrorNuevoLector { get; set; }
    public string? MensajeOk { get; set; }
    public bool MostrarModalComprobante { get; set; }
    public int? ComprobantePrestamoId { get; set; }

    public PrestamoModel(Frontend.Adapters.IPrestamoFachada prestamoFachada, IPrestamoServicio prestamoServicio, IEjemplarServicio ejemplarServicio, IUsuarioServicio usuarioServicio, IDetalleServicio detalleServicio, RouteTokenService routeTokenService, IAnulacionFachada anulacionFachada)
    {
        _prestamoFachada = prestamoFachada;
        _prestamoServicio = prestamoServicio;
        _ejemplarServicio = ejemplarServicio;
        _usuarioServicio = usuarioServicio;
        _detalleServicio = detalleServicio;
        _routeTokenService = routeTokenService;
        _anulacionFachada = anulacionFachada;
    }

    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        CargarPrestamosDetallados();
        FechaPrestamoDisplay = DateTime.Now;
        FechaDevolucionDefault = FechaPrestamoDisplay.AddDays(14);
        return Page();
    }

    private void SetFechaDefaults()
    {
        FechaPrestamoDisplay = DateTime.Now;
        FechaDevolucionDefault = FechaPrestamoDisplay.AddDays(14);
    }

    public JsonResult OnGetAutocompleteEjemplares(string q)
    {
        if (!EsAdminOBibliotecario())
            return JsonAccesoDenegado();

        // Obtener IDs de ejemplares ya en préstamos activos
        var ejemplaresEnUso = new HashSet<int>();
        var prestamosActivos = _prestamoServicio.Select();
        foreach (var p in prestamosActivos)
        {
            var detalles = _detalleServicio.ObtenerPorPrestamo(p.PrestamoId);
            foreach (var d in detalles)
                ejemplaresEnUso.Add(d.EjemplarId);
        }

        var items = _prestamoFachada.BuscarEjemplaresActivos(q ?? string.Empty)
            .Where(kv => !ejemplaresEnUso.Contains(kv.Key))
            .Select(kv => new { id = kv.Key, label = kv.Value });

        return new JsonResult(items);
    }

    public JsonResult OnGetEjemplarDetalle(int id)
    {
        if (!EsAdminOBibliotecario())
            return JsonAccesoDenegado();

        var EjemplarDto = _prestamoFachada.ObtenerEjemplarPorId(id);
        if (EjemplarDto == null)
            return new JsonResult(null);

        return new JsonResult(new
        {
            id = EjemplarDto.EjemplarId,
            codigo = EjemplarDto.CodigoInventario,
            label = _prestamoFachada.ObtenerLabelEjemplar(id),
            estadoConservacion = EjemplarDto.EstadoConservacion,
            disponible = EjemplarDto.Disponible,
            observaciones = EjemplarDto.MotivoBaja
        });
    }

    public JsonResult OnGetAutocompleteLectores(string q)
    {
        if (!EsAdminOBibliotecario())
            return JsonAccesoDenegado();

        // Retorna solo CIs para el autocomplete
        var items = _prestamoFachada.BuscarLectoresPorCi(q ?? string.Empty)
            .Select(kv => new { id = kv.Key, label = kv.Value.Split(" - ")[0] }); // Solo el CI

        return new JsonResult(items);
    }

    public JsonResult OnGetBuscarLectorPorCi(string ci, string? complemento)
    {
        if (!EsAdminOBibliotecario())
            return JsonAccesoDenegado();

        if (string.IsNullOrWhiteSpace(ci))
            return new JsonResult(new { success = false, message = "CI requerido" });

        var ciFull = string.IsNullOrWhiteSpace(complemento) ? ci : $"{ci}-{complemento}";
        var UsuarioDto = _prestamoFachada.ObtenerUsuarioPorCi(ciFull);

        if (UsuarioDto == null)
            return new JsonResult(new { success = false, message = "Lector no encontrado" });

        return new JsonResult(new
        {
            success = true,
            id = UsuarioDto.UsuarioId,
            nombreCompleto = $"{UsuarioDto.Nombres} {UsuarioDto.PrimerApellido} {UsuarioDto.SegundoApellido ?? ""}".ToDisplayName()
        });
    }

    // DEBUG: Ver todos los lectores en la BD
    public JsonResult OnGetDebugLectores()
    {
        if (!EsAdminOBibliotecario())
            return JsonAccesoDenegado();

        var tabla = _prestamoFachada.ObtenerTodosLosLectores(); // Will implement this method
        return new JsonResult(tabla);
    }

    // Handler para creación desde la página Create. Recibe una lista de ids de EjemplarDto como cadena JSON.
    public IActionResult OnPostCrear(string EjemplarData, int LectorId, DateTime FechaDevolucionEsperada, string? LectorCi, string? LectorComp)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        // Resolver lector: si LectorId no provisto, intentar buscar por CI+complemento
        if (LectorId <= 0)
        {
            var ciFull = string.IsNullOrWhiteSpace(LectorComp) ? (LectorCi ?? string.Empty) : $"{LectorCi}-{LectorComp}";
            if (string.IsNullOrWhiteSpace(ciFull))
            {
                MensajeError = "Debe indicar el CI del lector.";
                CargarPrestamosDetallados();
                SetFechaDefaults();
                return Page();
            }

            var UsuarioDto = _prestamoFachada.ObtenerUsuarioPorCi(ciFull);
            if (UsuarioDto == null)
            {
                MensajeError = "Lector no encontrado.";
                CargarPrestamosDetallados();
                SetFechaDefaults();
                return Page();
            }

            LectorId = UsuarioDto.UsuarioId;
        }

        // Parse EjemplarData JSON: expected array of { id: int, observaciones: string }
        List<(int Id, string? Observaciones)> items = new();
        if (string.IsNullOrWhiteSpace(EjemplarData))
        {
            MensajeError = "Debe seleccionar al menos un ejemplar.";
            CargarPrestamosDetallados();
            SetFechaDefaults();
            return Page();
        }

        try
        {
            // Try to parse the JSON - handle both JsonElement[] and simpler formats
            var trimmedData = EjemplarData.Trim();
            if (trimmedData == "[]")
            {
                MensajeError = "Debe seleccionar al menos un ejemplar.";
                CargarPrestamosDetallados();
                SetFechaDefaults();
                return Page();
            }

            // Use JsonDocument for more reliable parsing
            using (var doc = System.Text.Json.JsonDocument.Parse(trimmedData))
            {
                var root = doc.RootElement;
                if (root.ValueKind != System.Text.Json.JsonValueKind.Array)
                {
                    MensajeError = "Formato de ejemplares inválido.";
                    CargarPrestamosDetallados();
                    SetFechaDefaults();
                    return Page();
                }

                foreach (var el in root.EnumerateArray())
                {
                    if (el.TryGetProperty("id", out var idEl))
                    {
                        int idVal = 0;
                        if (idEl.ValueKind == System.Text.Json.JsonValueKind.Number)
                        {
                            idEl.TryGetInt32(out idVal);
                        }
                        else if (idEl.ValueKind == System.Text.Json.JsonValueKind.String && int.TryParse(idEl.GetString(), out var parsedId))
                        {
                            idVal = parsedId;
                        }

                        if (idVal > 0)
                        {
                            var obs = el.TryGetProperty("observaciones", out var obsEl) ? obsEl.GetString() : null;
                            items.Add((idVal, obs));
                        }
                    }
                }
            }

            if (items.Count == 0)
            {
                MensajeError = "No se encontraron ejemplares válidos.";
                CargarPrestamosDetallados();
                SetFechaDefaults();
                return Page();
            }
        }
        catch (System.Text.Json.JsonException ex)
        {
            MensajeError = $"Error al procesar los datos: {ex.Message}";
            CargarPrestamosDetallados();
            SetFechaDefaults();
            return Page();
        }
        catch (Exception ex)
        {
            MensajeError = $"Datos de ejemplares inválidos: {ex.Message}";
            CargarPrestamosDetallados();
            SetFechaDefaults();
            return Page();
        }

        if (items.Count > 5)
        {
            MensajeError = "No se pueden prestar más de 5 ejemplares a la vez.";
            CargarPrestamosDetallados();
            SetFechaDefaults();
            return Page();
        }

        // Crear UN SOLO préstamo con múltiples ejemplares (opción 2 - DetalleDto es la relación)
        var resultado = _prestamoFachada.CrearPrestamoMultiple(
            LectorId,
            items.Select(it => (EjemplarId: it.Id, ObservacionesSalida: it.Observaciones)),
            FechaDevolucionEsperada,
            ObtenerUsuarioSesionId()
        );

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(string.Empty, resultado.Error.Message);
            MensajeError = resultado.Error.Message;
            CargarPrestamosDetallados();
            SetFechaDefaults();
            return Page();
        }

        MensajeOk = "Préstamo registrado correctamente.";
        ComprobantePrestamoId = resultado.Value;
        MostrarModalComprobante = true;
        CargarPrestamosDetallados();
        SetFechaDefaults();
        return Page();
    }

    public IActionResult OnPostCrearLector()
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        MensajeErrorNuevoLector = null;

        var usuarioSesionId = ObtenerUsuarioSesionId() ?? 1;

        var resultado = _usuarioServicio.CrearLector(NuevoLector, usuarioSesionId);
        if (resultado.IsFailure)
        {
            MensajeErrorNuevoLector = resultado.Error.Message;
            CargarPrestamosDetallados();
            SetFechaDefaults();
            return Page();
        }

        MensajeOk = "Lector creado correctamente.";
        ModelState.Clear();
        NuevoLector = new();
        CargarPrestamosDetallados();
        SetFechaDefaults();
        return Page();
    }

    private void CargarPrestamos()
    {
        var prestamosDto = _prestamoServicio.Select(todos: EsAdmin());

        Prestamos = new List<PrestamoEntity>();
        foreach (var row in prestamosDto)
        {
            Prestamos.Add(new PrestamoEntity
            {
                PrestamoId = row.PrestamoId,
                LectorId = row.LectorId,
                FechaPrestamo = row.FechaPrestamo,
                FechaDevolucionEsperada = row.FechaDevolucionEsperada,
                FechaDevolucionReal = row.FechaDevolucionReal,
                ObservacionesSalida = row.ObservacionesSalida,
                ObservacionesEntrada = row.ObservacionesEntrada,
                Estado = row.Estado
            });
        }

        // Cargar títulos para desplegables.
        LibrosTitulos = _prestamoFachada.BuscarEjemplaresActivos(string.Empty).ToDictionary(k => k.Key, v => v.Value);
    }

    private void CargarPrestamosDetallados()
    {
        var tabla = _prestamoServicio.Select();
        PrestamosDetallados = new List<PrestamoDetalleDTO>();

        var detallesPorPrestamo = _detalleServicio.ObtenerTodos()
            .GroupBy(d => d.PrestamoId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var titulosLibros = _ejemplarServicio.ObtenerTitulosLibros();
        var cacheEjemplares = new Dictionary<int, Frontend.Dtos.EjemplarDto?>();

        // Cargar todos los usuarios en memoria para búsqueda rápida
        var usuariosTabla = _usuarioServicio.Select();
        var usuariosDict = new Dictionary<int, (string Nombres, string PrimerApellido, string? SegundoApellido)>();
        foreach (var u in usuariosTabla)
        {
            try
            {
                int usuarioId = u.UsuarioId;
                string nombres = u.Nombres ?? string.Empty;
                string primerApellido = u.PrimerApellido ?? string.Empty;
                string? segundoApellido = u.SegundoApellido;
                usuariosDict[usuarioId] = (nombres, primerApellido, segundoApellido);
            }
            catch { }
        }

        foreach (var row in tabla)
        {
            int lectorId = row.LectorId;

            string nombreLector = "Desconocido";
            if (usuariosDict.TryGetValue(lectorId, out var UsuarioDto))
            {
                nombreLector = $"{UsuarioDto.Nombres} {UsuarioDto.PrimerApellido}".Trim();
                if (!string.IsNullOrWhiteSpace(UsuarioDto.SegundoApellido))
                {
                    nombreLector += $" { UsuarioDto.SegundoApellido}";
                }

                nombreLector = nombreLector.ToDisplayName();
            }

            var libros = new List<string>();
            var codigos = new List<string>();
            var observacionesPorLibro = new List<string>();

            if (detallesPorPrestamo.TryGetValue(row.PrestamoId, out var detallesPrestamo))
            {
                foreach (var DetalleDto in detallesPrestamo)
                {
                    if (!cacheEjemplares.TryGetValue(DetalleDto.EjemplarId, out var EjemplarDto))
                    {
                        EjemplarDto = _ejemplarServicio.GetById(DetalleDto.EjemplarId);
                        cacheEjemplares[DetalleDto.EjemplarId] = EjemplarDto;
                    }

                    if (EjemplarDto == null)
                    {
                        continue;
                    }

                    var titulo = !string.IsNullOrWhiteSpace(EjemplarDto.LibroTitulo)
                        ? EjemplarDto.LibroTitulo
                        : (titulosLibros.TryGetValue(EjemplarDto.LibroId, out var t) ? t : "Sin título");

                    libros.Add((titulo ?? "Sin título").ToDisplayName());
                    codigos.Add(string.IsNullOrWhiteSpace(EjemplarDto.CodigoInventario) ? "S/C" : EjemplarDto.CodigoInventario);
                    observacionesPorLibro.Add(string.IsNullOrWhiteSpace(DetalleDto.ObservacionesSalida) ? "Sin observaciones" : DetalleDto.ObservacionesSalida!);
                }
            }

            var tituloResumen = "Sin ejemplares";
            var codigoResumen = "N/A";

            if (libros.Count == 1)
            {
                tituloResumen = libros[0];
                codigoResumen = codigos.FirstOrDefault() ?? "S/C";
            }
            else if (libros.Count > 1)
            {
                tituloResumen = $"{libros[0]} (+{libros.Count - 1} más)";
                codigoResumen = $"{codigos[0]} (+{codigos.Count - 1})";
            }

            PrestamosDetallados.Add(new PrestamoDetalleDTO
            {
                PrestamoId = row.PrestamoId,
                LectorId = lectorId,
                TituloLibro = tituloResumen,
                CodigoInventario = codigoResumen,
                Libros = libros,
                Codigos = codigos,
                ObservacionesPorLibro = observacionesPorLibro,
                NombreLector = nombreLector,
                FechaPrestamo = row.FechaPrestamo,
                FechaDevolucionEsperada = row.FechaDevolucionEsperada,
                FechaDevolucionReal = row.FechaDevolucionReal,
                ObservacionesSalida = row.ObservacionesSalida,
                ObservacionesEntrada = row.ObservacionesEntrada,
                Estado = row.Estado
            });
        }
    }

    public JsonResult OnGetDetallesPrestamo(int id)
    {
        if (!EsAdminOBibliotecario())
            return JsonAccesoDenegado();

        var PrestamoDto = PrestamosDetallados.FirstOrDefault(p => p.PrestamoId == id);
        if (PrestamoDto == null)
        {
            // Recargar si no está en memoria
            CargarPrestamosDetallados();
            PrestamoDto = PrestamosDetallados.FirstOrDefault(p => p.PrestamoId == id);
        }

        if (PrestamoDto == null)
            return new JsonResult(new { success = false });

        return new JsonResult(new { success = true, data = PrestamoDto });
    }

    public JsonResult OnGetComprobantePrestamo(int id)
    {
        if (!EsAdminOBibliotecario())
            return JsonAccesoDenegado();

        try
        {
            // Obtener el préstamo base
            var PrestamoDto = _prestamoServicio.GetById(id);
            if (PrestamoDto == null)
                return new JsonResult(new { success = false, message = "Préstamo no encontrado." });

            // Obtener datos del lector
            var UsuarioDto = _usuarioServicio.Select().FirstOrDefault(u => u.UsuarioId == PrestamoDto.LectorId);
            var ci = UsuarioDto?.CI ?? string.Empty;
            var nombreLector = UsuarioDto != null ? $"{UsuarioDto.Nombres} {UsuarioDto.PrimerApellido} {UsuarioDto.SegundoApellido ?? ""}".Trim() : "Desconocido";

            var usuarioSesionId = ObtenerUsuarioSesionId();
            string bibliotecario = HttpContext.Session.GetString(SessionKeys.NombreUsuario) ?? "No registrado";
            if (usuarioSesionId.HasValue)
            {
                var usuarioSesion = _usuarioServicio.Select().FirstOrDefault(u => u.UsuarioId == usuarioSesionId.Value);
                if (usuarioSesion != null)
                {
                    bibliotecario = $"{usuarioSesion.Nombres} {usuarioSesion.PrimerApellido} {usuarioSesion.SegundoApellido ?? ""}".Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(bibliotecario))
            {
                bibliotecario = HttpContext.Session.GetString(SessionKeys.NombreUsuario) ?? "No registrado";
            }

            // Obtener DETALLES del préstamo (ejemplares prestados)
            var detalles = _detalleServicio.ObtenerPorPrestamo(id)?.ToList() ?? new List<DetalleDto>();
            
            var librosRelacionados = new List<object>();
            foreach (var DetalleDto in detalles)
            {
                var EjemplarDto = _ejemplarServicio.GetById(DetalleDto.EjemplarId);
                if (EjemplarDto != null)
                {
                    var etiqueta = _prestamoFachada.ObtenerLabelEjemplar(DetalleDto.EjemplarId);
                    var titulo = !string.IsNullOrWhiteSpace(etiqueta)
                        ? etiqueta.Split('(')[0].Trim()
                        : (EjemplarDto.LibroTitulo ?? "Sin título");

                    librosRelacionados.Add(new
                    {
                        detalleId = DetalleDto.DetalleId,
                        titulo = string.IsNullOrWhiteSpace(titulo) ? "Sin título" : titulo,
                        codigo = string.IsNullOrWhiteSpace(EjemplarDto.CodigoInventario) ? "S/C" : EjemplarDto.CodigoInventario,
                        observacionesSalida = DetalleDto.ObservacionesSalida
                    });
                }
            }

            var fechaDevolucion = PrestamoDto.FechaDevolucionEsperada;
            var diasPrestamo = (int)Math.Max(1, Math.Ceiling((fechaDevolucion.Date - PrestamoDto.FechaPrestamo.Date).TotalDays));
            var totalLibros = librosRelacionados.Count;

            var data = new
            {
                prestamoId = PrestamoDto.PrestamoId,
                folio = $"PR-{PrestamoDto.FechaPrestamo:yyyyMMdd}-{PrestamoDto.LectorId}",
                fechaEmision = DateTime.Now,
                nombreLector = nombreLector,
                ci = ci,
                clave = ci,
                grupo = string.Empty,
                dias = diasPrestamo,
                fechaPrestamo = PrestamoDto.FechaPrestamo,
                fechaDevolucion = fechaDevolucion,
                fechaEntrega = fechaDevolucion,
                libros = librosRelacionados,
                totalLibros,
                bibliotecario,
                usuarioNombre = bibliotecario
            };

            return new JsonResult(new { success = true, data });
        }
        catch (Exception ex)
        {
            return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
        }
    }

    private IEnumerable<PrestamoDetalleDTO> ObtenerPrestamosRelacionadosParaComprobante(PrestamoDetalleDTO prestamoBase)
    {
        var fechaBase = prestamoBase.FechaPrestamo;

        var relacionados = PrestamosDetallados
            .Where(p => p.LectorId == prestamoBase.LectorId
                        && p.Estado == prestamoBase.Estado
                        && p.FechaDevolucionEsperada.Date == prestamoBase.FechaDevolucionEsperada.Date
                        && Math.Abs((p.FechaPrestamo - fechaBase).TotalMinutes) <= 3)
            .OrderBy(p => p.PrestamoId)
            .ToList();

        if (!relacionados.Any())
        {
            relacionados.Add(prestamoBase);
        }

        return relacionados;
    }

    public IActionResult OnPostAnularPrestamo(int prestamoId)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        try
        {
            var usuarioSesionId = ObtenerUsuarioSesionId();

            var resultado = _anulacionFachada.AnularPrestamo(
                prestamoId,
                usuarioSesionId,
                string.Empty
            );

            if (resultado.IsFailure)
            {
                ModelState.AddModelError(string.Empty, resultado.Error.Message);
                MensajeError = resultado.Error.Message;
                CargarPrestamosDetallados();
                return Page();
            }

            MensajeOk = "Préstamo anulado correctamente.";
            CargarPrestamosDetallados();
            return Page();
        }
        catch (Exception ex)
        {
            MensajeError = $"Error al anular préstamo: {ex.Message}";
            CargarPrestamosDetallados();
            return Page();
        }
    }

    private int? ObtenerUsuarioSesionId()
    {
        var claim = HttpContext.Session.GetString(SessionKeys.UsuarioId);
        if (int.TryParse(claim, out var usuarioId))
        {
            return usuarioId;
        }

        return null;
    }

    private bool EsAdminOBibliotecario()
    {
        var rolSesion = HttpContext.Session.GetString(SessionKeys.Rol);
        return string.Equals(rolSesion, Roles.Bibliotecario, StringComparison.OrdinalIgnoreCase);
    }

    private bool EsAdmin()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);
        return string.Equals(rol, Roles.Admin, StringComparison.OrdinalIgnoreCase);
    }

    private JsonResult JsonAccesoDenegado()
    {
        return new JsonResult(new { success = false, message = "Acceso denegado." })
        {
            StatusCode = StatusCodes.Status403Forbidden
        };
    }
}
