using Frontend.Adapters;
using Frontend.Helpers;
using Frontend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

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
            return LocalRedirect("/");

        CargarAutores();
        return Page();
    }

    private void CargarAutores()
    {
        var autores = _autorServicio.Select(todos: EsAdmin()).ToList();

        foreach (var autorDto in autores)
        {
            autorDto.Nombres = LimpiarTexto(autorDto.Nombres).ToDisplayName();
            autorDto.Apellidos = LimpiarTexto(autorDto.Apellidos).ToDisplayName();
            autorDto.Nacionalidad = LimpiarTexto(autorDto.Nacionalidad);

            if (string.IsNullOrEmpty(autorDto.RouteToken))
                autorDto.RouteToken = _routeTokenService.CrearToken(autorDto.AutorId);
        }

        Autores = autores;
    }

    public IActionResult OnPostEliminar(string token)
    {
        if (!EsAdminOBibliotecario())
            return LocalRedirect("/");

        if (!_routeTokenService.TryObtenerId(token, out var id))
            return NotFound();

        _autorServicio.Delete(id, ObtenerUsuarioSesionId());

        return RedirectToPage();
    }

    public IActionResult OnPostCrear()
    {
        if (!EsAdminOBibliotecario())
            return LocalRedirect("/");

        ModalActivo = "crear";

        AutorDto.Nombres = LimpiarTexto(AutorDto.Nombres).ToDisplayName();
        AutorDto.Apellidos = LimpiarTexto(AutorDto.Apellidos).ToDisplayName();
        AutorDto.Nacionalidad = LimpiarTexto(AutorDto.Nacionalidad);
        AutorDto.UsuarioSesionId = ObtenerUsuarioSesionId();
        AutorDto.Estado = true;

        ValidarAutorCrear();

        if (!ModelState.IsValid)
        {
            CargarAutores();
            return Page();
        }

        var result = _autorServicio.Create(AutorDto);

        if (result.IsFailure)
            AgregarError(result.Error, true);

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
            return LocalRedirect("/");

        ModalActivo = "editar";

        if (!_routeTokenService.TryObtenerId(token, out var id))
            return NotFound();

        Nombres = LimpiarTexto(Nombres).ToDisplayName();
        Apellidos = LimpiarTexto(Apellidos).ToDisplayName();
        Nacionalidad = LimpiarTexto(Nacionalidad);

        ValidarAutorEditar(Nombres, Apellidos, Nacionalidad, FechaNacimiento);

        if (!ModelState.IsValid)
        {
            ModelState.SetModelValue("token", new ValueProviderResult(token));
            ModelState.SetModelValue("Nombres", new ValueProviderResult(Nombres));
            ModelState.SetModelValue("Apellidos", new ValueProviderResult(Apellidos ?? ""));
            ModelState.SetModelValue("Nacionalidad", new ValueProviderResult(Nacionalidad ?? ""));
            ModelState.SetModelValue("FechaNacimiento", new ValueProviderResult(FechaNacimiento?.ToString("yyyy-MM-dd") ?? ""));
            ModelState.SetModelValue("Estado", new ValueProviderResult("true"));
            CargarAutores();
            return Page();
        }

        var autorDto = new AutorDto
        {
            AutorId = id,
            Nombres = Nombres,
            Apellidos = Apellidos,
            Nacionalidad = Nacionalidad,
            FechaNacimiento = FechaNacimiento,
            Estado = true,
            UsuarioSesionId = ObtenerUsuarioSesionId()
        };

        var result = _autorServicio.Update(autorDto);

        if (result.IsFailure)
            AgregarError(result.Error);

        if (!ModelState.IsValid)
        {
            CargarAutores();
            return Page();
        }

        return RedirectToPage();
    }

    private void ValidarAutorCrear()
    {
        ValidarCampoObligatorio("AutorDto.Nombres", AutorDto.Nombres, "Ingrese el nombre del autor.");
        ValidarCampoObligatorio("AutorDto.Apellidos", AutorDto.Apellidos, "Ingrese los apellidos del autor.");
        ValidarCampoObligatorio("AutorDto.Nacionalidad", AutorDto.Nacionalidad, "Seleccione una nacionalidad.");

        ValidarSoloLetras("AutorDto.Nombres", AutorDto.Nombres, "El nombre solo debe contener letras y espacios.");
        ValidarSoloLetras("AutorDto.Apellidos", AutorDto.Apellidos, "Los apellidos solo deben contener letras y espacios.");
        ValidarSoloLetras("AutorDto.Nacionalidad", AutorDto.Nacionalidad, "La nacionalidad solo debe contener letras y espacios.");

        ValidarFecha("AutorDto.FechaNacimiento", AutorDto.FechaNacimiento);
    }

    private void ValidarAutorEditar(string nombres, string? apellidos, string? nacionalidad, DateTime? fechaNacimiento)
    {
        ValidarCampoObligatorio("Nombres", nombres, "Ingrese el nombre del autor.");
        ValidarCampoObligatorio("Apellidos", apellidos, "Ingrese los apellidos del autor.");
        ValidarCampoObligatorio("Nacionalidad", nacionalidad, "Seleccione una nacionalidad.");

        ValidarSoloLetras("Nombres", nombres, "El nombre solo debe contener letras y espacios.");
        ValidarSoloLetras("Apellidos", apellidos, "Los apellidos solo deben contener letras y espacios.");
        ValidarSoloLetras("Nacionalidad", nacionalidad, "La nacionalidad solo debe contener letras y espacios.");

        ValidarFecha("FechaNacimiento", fechaNacimiento);
    }

    private void ValidarCampoObligatorio(string key, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            ModelState.AddModelError(key, message);
    }

    private void ValidarFechaObligatoria(string key, DateTime? value)
    {
        if (!value.HasValue)
            ModelState.AddModelError(key, "Ingrese la fecha de nacimiento.");
    }

    private void ValidarSoloLetras(string key, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var regex = new Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$");

        if (!regex.IsMatch(value))
            ModelState.AddModelError(key, message);
    }

    private void ValidarEspaciosInternosIncorrectos(string key, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            return;

        var partes = value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partes.Length >= 3 && partes.Any(parte => parte.Length <= 2))
            ModelState.AddModelError(key, message);
    }

    private void ValidarFecha(string key, DateTime? fechaNacimiento)
    {
        if (fechaNacimiento.HasValue && fechaNacimiento.Value.Date > DateTime.Today)
            ModelState.AddModelError(key, "La fecha de nacimiento no puede ser futura.");
    }

    private static string LimpiarTexto(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return Regex.Replace(value.Trim(), @"\s+", " ");
    }

    private void AgregarError(Error error, bool esCrear = false)
    {
        var key = error.Code.Split('.').LastOrDefault() ?? string.Empty;

        if (esCrear)
            ModelState.AddModelError($"AutorDto.{key}", error.Message);
        else
            ModelState.AddModelError(key, error.Message);
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

    private int? ObtenerUsuarioSesionId()
    {
        var usuarioSesion = HttpContext.Session.GetString(SessionKeys.UsuarioId);

        if (int.TryParse(usuarioSesion, out var usuarioId))
            return usuarioId;

        return null;
    }
}
