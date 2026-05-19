using Frontend.Adapters;
using Frontend.Helpers;
using Frontend.Dtos;
using Frontend.Dtos;
using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace Frontend.Pages;

public class AutorModel : PageModel
{
    private readonly IAutorServicio _autorServicio;
    private readonly RouteTokenService _routeTokenService;

    public List<AutorDto> Autores { get; set; } = new();

    [BindProperty]
    public AutorDto AutorDto { get; set; } = new AutorDto();

    public string ModalActivo { get; set; } = string.Empty;

    public AutorModel(
        IAutorServicio autorServicio,
        RouteTokenService routeTokenService)
    {
        _autorServicio = autorServicio;
        _routeTokenService = routeTokenService;
    }


    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        CargarAutores();
        return Page();
    }

    private void CargarAutores()
    {
        var autores = _autorServicio.Select(todos: EsAdmin()).ToList();
        
        foreach (var AutorDto in autores)
        {
            AutorDto.Nombres = AutorDto.Nombres.ToDisplayName();
            AutorDto.Apellidos = AutorDto.Apellidos.ToDisplayName();

            if (string.IsNullOrEmpty(AutorDto.RouteToken))
            {
                AutorDto.RouteToken = _routeTokenService.CrearToken(AutorDto.AutorId);
            }
        }
        
        Autores = autores;
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        if (!_routeTokenService.TryObtenerId(token, out var id))
            return NotFound();

        var result = _autorServicio.Delete(id, ObtenerUsuarioSesionId());

        if (result.IsFailure)
        {
            // Opcional: manejar error de eliminación
        }

        return RedirectToPage();
    }

    public IActionResult OnPostCrear()
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        ModalActivo = "crear";
        AutorDto.Nombres = AutorDto.Nombres.ToDisplayName();
        AutorDto.Apellidos = AutorDto.Apellidos.ToDisplayName();
        AutorDto.UsuarioSesionId = ObtenerUsuarioSesionId();

        var result = _autorServicio.Create(AutorDto);

        if (result.IsFailure)
        {
            AgregarError(result.Error, true);
        }

        if (!ModelState.IsValid)
        {
            CargarAutores();
            return Page();
        }

        return RedirectToPage();
    }

    public IActionResult OnPostEditar(
        string token,
        string Nombres,
        string? Apellidos,
        string? Nacionalidad,
        DateTime? FechaNacimiento,
        bool? Estado)
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        ModalActivo = "editar";

        if (!_routeTokenService.TryObtenerId(token, out var id))
            return NotFound();

        Nombres = Nombres.ToDisplayName();
        Apellidos = Apellidos.ToDisplayName();

        var autorDto = new AutorDto
        {
            AutorId = id,
            Nombres = Nombres,
            Apellidos = Apellidos,
            Nacionalidad = Nacionalidad,
            FechaNacimiento = FechaNacimiento,
            Estado = Estado ?? false,
            UsuarioSesionId = ObtenerUsuarioSesionId()
        };

        var result = _autorServicio.Update(autorDto);

        if (result.IsFailure)
        {
            AgregarError(result.Error);
        }

        if (!ModelState.IsValid)
        {
            ModelState.SetModelValue("token", new ValueProviderResult(token));
            ModelState.SetModelValue("Nombres", new ValueProviderResult(Nombres));
            ModelState.SetModelValue("Apellidos", new ValueProviderResult(Apellidos ?? ""));
            ModelState.SetModelValue("Nacionalidad", new ValueProviderResult(Nacionalidad ?? ""));
            ModelState.SetModelValue("FechaNacimiento", new ValueProviderResult(FechaNacimiento?.ToString("yyyy-MM-dd") ?? ""));
            ModelState.SetModelValue("Estado", new ValueProviderResult((Estado ?? false).ToString()));
            CargarAutores();
            return Page();
        }

        return RedirectToPage();
    }

    private void AgregarError(Error error, bool esCrear = false)
    {
        var key = error.Code.Split('.').LastOrDefault() ?? string.Empty;

        if (esCrear)
        {
            ModelState.AddModelError($"AutorDto.{key}", error.Message);
        }
        else
        {
            ModelState.AddModelError(key, error.Message);
        }
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

    private int? ObtenerUsuarioSesionId()
    {
        var usuarioSesion = HttpContext.Session.GetString(SessionKeys.UsuarioId);

        if (int.TryParse(usuarioSesion, out var usuarioId))
        {
            return usuarioId;
        }

        return null;
    }
}
