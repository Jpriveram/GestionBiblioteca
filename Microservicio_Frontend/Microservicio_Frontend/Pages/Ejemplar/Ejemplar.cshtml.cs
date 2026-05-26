using Frontend.Dtos;
using Frontend.Adapters;
using Frontend.Adapters;
using Frontend.Helpers;
using Frontend.Dtos;
using Frontend.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;



namespace Frontend.Pages;

public class EjemplarModel : PageModel
{
    private readonly IEjemplarServicio _ejemplarServicio;
    private readonly RouteTokenService _routeTokenService;
    

    public List<EjemplarDto> Ejemplares { get; set; } = new();
    public Dictionary<int, string> LibrosTitulos { get; set; } = new();
    public List<LibroDto> Libros { get; set; } = new();

    public string ErrorMessage { get; set; } = string.Empty;

    public EjemplarModel(
        IEjemplarServicio ejemplarServicio,
        RouteTokenService routeTokenService
        )
    {
        _ejemplarServicio = ejemplarServicio;
        _routeTokenService = routeTokenService;
   
    }

    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        CargarPagina();
        return Page();
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        if (!_routeTokenService.TryObtenerId(token, out var id))
        {
            return NotFound();
        }

        try
        {
            var EjemplarDto = _ejemplarServicio.GetById(id);

            if (EjemplarDto == null)
            {
                return NotFound();
            }

            if (!EjemplarDto.Estado)
            {
                return RedirectToPage();
            }

            EjemplarDto.UsuarioSesionId = ObtenerUsuarioSesionId() ?? EjemplarDto.UsuarioSesionId;
            var result = _ejemplarServicio.Delete(EjemplarDto);
            if (!result.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, result.Error.Message);
                CargarPagina();
                return Page();
            }

            return RedirectToPage();
        }
        catch
        {
            CargarPagina();
            return Page();
        }
    }

    public IActionResult OnPostEditar(
        string token,
        int LibroId,
        string CodigoInventario,
        string? EstadoConservacion,
        bool? Disponible,
        bool? DadoDeBaja,
        string? MotivoBaja,
        string? Ubicacion,
        bool? Estado)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        if (!_routeTokenService.TryObtenerId(token, out var ejemplarId))
        {
            return NotFound();
        }

        var dto = new EjemplarDto
        {
            EjemplarId = ejemplarId,
            LibroId = LibroId,
            CodigoInventario = CodigoInventario ?? string.Empty,
            EstadoConservacion = EstadoConservacion,
            Disponible = Disponible ?? false,
            DadoDeBaja = DadoDeBaja ?? false,
            MotivoBaja = MotivoBaja,
            Ubicacion = Ubicacion,
            Estado = Estado ?? false,
            UsuarioSesionId = ObtenerUsuarioSesionId()
        };

        if (!ModelState.IsValid)
        {
            CargarPagina();
            return Page();
        }

        try
        {
            var result = _ejemplarServicio.Update(dto);
            if (!result.IsSuccess)
            {
                AgregarError(result.Error);
                CargarPagina();
                return Page();
            }
        }
        catch (Exception ex) when (ex.Message.Contains("Duplicate"))
        {
            ModelState.AddModelError("CodigoInventario", "Código duplicado.");
            CargarPagina();
            return Page();
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "Error al procesar.");
            CargarPagina();
            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostCrear(EjemplarDto EjemplarDto)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        EjemplarDto.UsuarioSesionId = ObtenerUsuarioSesionId();

        if (!ModelState.IsValid)
        {
            ErrorMessage = "Campos incompletos.";
            CargarPagina();
            return Page();
        }

        try
        {
            var result = _ejemplarServicio.Create(EjemplarDto);
            if (!result.IsSuccess)
            {
                if (result.Error.Code == "Post")
                {
                    ErrorMessage = result.Error.Message;
                }
                else
                {
                    AgregarError(result.Error, "EjemplarDto");
                }

                CargarPagina();
                return Page();
            }
            return RedirectToPage();
        }
        catch (Exception ex) when (ex.Message.Contains("Duplicate"))
        {
            ModelState.AddModelError("EjemplarDto.CodigoInventario", "Código duplicado.");
            CargarPagina();
            return Page();
        }
        catch (Exception)
        {
            ErrorMessage = "Error al procesar.";
            CargarPagina();
            return Page();
        }
    }

    private void CargarPagina()
    {
        Ejemplares = _ejemplarServicio.Select(todos: EsAdmin()).ToList();
        foreach (var EjemplarDto in Ejemplares)
        {
            EjemplarDto.RouteToken = _routeTokenService.CrearToken(EjemplarDto.EjemplarId);
        }
        LibrosTitulos = _ejemplarServicio.ObtenerTitulosLibros();
        Libros = _ejemplarServicio.ObtenerLibrosActivos().ToList();
    }

    private void AgregarError(Error error, string? prefix = null)
    {
        var key = error.Code.Split('.').LastOrDefault() ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(prefix) && !string.IsNullOrWhiteSpace(key))
        {
            key = $"{prefix}.{key}";
        }

        ModelState.AddModelError(key, error.Message);
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

        return string.Equals(rol, Roles.Admin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, Roles.Bibliotecario, StringComparison.OrdinalIgnoreCase);
    }

    private bool EsAdmin()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);
        return string.Equals(rol, Roles.Admin, StringComparison.OrdinalIgnoreCase);
    }
}
