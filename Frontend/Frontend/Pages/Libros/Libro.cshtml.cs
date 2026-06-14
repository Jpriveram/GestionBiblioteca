using Frontend.Dtos;
using Frontend.Adapters;
using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Data;

namespace Frontend.Pages;

public class LibroModel : PageModel
{
    private readonly RouteTokenService _routeTokenService;
    private readonly ILibroServicio _libroServicio;

    public IEnumerable<LibroDto> Libros { get; set; } = new List<LibroDto>();
    public Dictionary<int, string> AutoresNombres { get; set; } = new();
    public IEnumerable<AutorDto> Autores { get; set; } = new List<AutorDto>();
    public Dictionary<int, string> LibroTokens { get; set; } = new();

    public LibroModel(
        RouteTokenService routeTokenService,
        ILibroServicio libroServicio)
    {
        _routeTokenService = routeTokenService;
        _libroServicio = libroServicio;
    }

    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        CargarDatos();
        return Page();
    }

    private void CargarDatos()
    {
        Libros = _libroServicio.Select(todos: EsAdmin()) ?? new List<LibroDto>();
        LibroTokens = new Dictionary<int, string>();

        foreach (var l in Libros)
        {
            LibroTokens[l.LibroId] = _routeTokenService.CrearToken(l.LibroId);
        }

        AutoresNombres = _libroServicio.ObtenerNombresAutores();
        Autores = _libroServicio.ObtenerAutoresActivos();
    }

    public IActionResult OnGetAutoresActivos()
    {
        if (!EsAdminOBibliotecario())
        {
            return Unauthorized();
        }

        var autores = _libroServicio.ObtenerAutoresActivos();
        var resultado = autores.Select(a => new
        {
            id = a.AutorId,
            nombres = (a.Nombres ?? "").ToDisplayName(),
            apellidos = (a.Apellidos ?? "").ToDisplayName(),
            nacionalidad = a.Nacionalidad,
            displayText = $"{(a.Nombres ?? "").ToDisplayName()} {(a.Apellidos ?? "").ToDisplayName()}{(!string.IsNullOrWhiteSpace(a.Nacionalidad) ? $" ({a.Nacionalidad})" : "")}"
        }).ToList();

        return new JsonResult(resultado);
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        if (!_routeTokenService.TryObtenerId(token, out var libroId))
        {
            return NotFound();
        }

        var result = _libroServicio.Delete(libroId, ObtenerUsuarioSesionId());

        if (result.IsFailure)
        {
        }

        return RedirectToPage();
    }

    public IActionResult OnPostEditar(
        string token,
        int? AutorId,
        string? Titulo,
        string? ISBN,
        string? Editorial,
        string? Genero,
        string? Edicion,
        int? AñoPublicacion,
        int? NumeroPaginas,
        string? Idioma,
        string? PaisPublicacion,
        string? Descripcion,
        bool Estado)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            ModelState.AddModelError("token", "Petición inválida o token expirado.");
            CargarDatos();
            return Page();
        }

        var dto = new LibroDto
        {
            LibroId = id,
            UsuarioSesionId = ObtenerUsuarioSesionId(),
            AutorId = AutorId ?? 0,
            Titulo = Titulo ?? string.Empty,
            ISBN = ISBN,
            Editorial = Editorial,
            Genero = Genero,
            Edicion = Edicion,
            AñoPublicacion = AñoPublicacion,
            NumeroPaginas = NumeroPaginas,
            Idioma = Idioma,
            PaisPublicacion = PaisPublicacion,
            Descripcion = Descripcion,
            Estado = Estado
        };

        var resultado = _libroServicio.Update(dto);

        if (resultado.IsFailure)
        {
            AgregarError(resultado.Error);
        }

        if (!ModelState.IsValid)
        {
            if (EsAjax())
            {
                return new JsonResult(new { success = false, errors = ObtenerErroresJson() });
            }

            CargarDatos();
            return Page();
        }

        if (EsAjax())
        {
            return new JsonResult(new { success = true });
        }

        return RedirectToPage();
    }

    public IActionResult OnPostCrear(
        string? NombreAutorNuevo,
        string? Titulo,
        string? ISBN,
        string? Editorial,
        string? Genero,
        string? Edicion,
        int? AñoPublicacion,
        int? NumeroPaginas,
        string? Idioma,
        string? PaisPublicacion,
        string? Descripcion,
        bool Estado = true)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        ModelState.Remove("AutorId");

        var autorEntrada = Request.Form["AutorId"].ToString();
        int? AutorId = null;
        if (int.TryParse(autorEntrada, out var parsedId))
        {
            AutorId = parsedId;
        }
        else if (string.IsNullOrWhiteSpace(NombreAutorNuevo) && !string.IsNullOrWhiteSpace(autorEntrada))
        {
            NombreAutorNuevo = autorEntrada;
        }

        NombreAutorNuevo = string.IsNullOrWhiteSpace(NombreAutorNuevo) ? null : NombreAutorNuevo.Trim();

        var dto = new LibroDto
        {
            UsuarioSesionId = ObtenerUsuarioSesionId(),
            AutorId = AutorId ?? 0,
            Titulo = Titulo ?? string.Empty,
            ISBN = ISBN,
            Editorial = Editorial,
            Genero = Genero,
            Edicion = Edicion,
            AñoPublicacion = AñoPublicacion,
            NumeroPaginas = NumeroPaginas,
            Idioma = Idioma,
            PaisPublicacion = PaisPublicacion,
            Descripcion = Descripcion,
            Estado = Estado
        };

        var resultado = _libroServicio.Create(dto, NombreAutorNuevo);

        if (resultado.IsFailure)
        {
            AgregarError(resultado.Error);
        }

        if (!ModelState.IsValid)
        {
            if (EsAjax())
            {
                return new JsonResult(new { success = false, errors = ObtenerErroresJson() });
            }

            CargarDatos();
            return Page();
        }

        if (EsAjax())
        {
            return new JsonResult(new { success = true });
        }

        return RedirectToPage();
    }

    private Dictionary<string, List<string>> ObtenerErroresJson()
    {
        return ModelState
            .Where(kvp => kvp.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToList());
    }

    private void AgregarError(Error error)
    {
        var key = error.Code.Split('.').LastOrDefault() ?? string.Empty;
        key = key.Replace("A\uFFFDoPublicacion", "AñoPublicacion");

        if (string.Equals(error.Code, "Post", StringComparison.OrdinalIgnoreCase)
            || string.Equals(error.Code, "Put", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(key))
        {
            key = string.Empty;
        }

        ModelState.AddModelError(key, error.Message);
    }

    private bool EsAjax()
    {
        return Request.Headers["X-Requested-With"] == "XMLHttpRequest";
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
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);

        return string.Equals(rol, Roles.Bibliotecario, StringComparison.OrdinalIgnoreCase);
    }

    private bool EsAdmin()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);
        return string.Equals(rol, Roles.Admin, StringComparison.OrdinalIgnoreCase);
    }
}
