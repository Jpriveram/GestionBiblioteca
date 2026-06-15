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
            autorDto.Nombres = FormatearObligatorio(autorDto.Nombres);
            autorDto.Apellidos = FormatearApellidoOpcional(autorDto.Apellidos);
            autorDto.Nacionalidad = FormatearTextoOpcional(autorDto.Nacionalidad);

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

        AutorDto.Nombres = FormatearObligatorio(AutorDto.Nombres);
        AutorDto.Apellidos = FormatearApellidoOpcional(AutorDto.Apellidos);
        AutorDto.Nacionalidad = FormatearTextoOpcional(AutorDto.Nacionalidad);
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

        Nombres = FormatearObligatorio(Nombres);
        Apellidos = FormatearApellidoOpcional(Apellidos);
        Nacionalidad = FormatearTextoOpcional(Nacionalidad);

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
        if (AgregarErrorSiCampoVacio("AutorDto.Nombres", AutorDto.Nombres, "Ingrese el nombre del autor."))
            return;

        if (AgregarErrorSiNoSonLetras("AutorDto.Nombres", AutorDto.Nombres, "El nombre solo debe contener letras y espacios."))
            return;

        if (AgregarErrorSiTieneLetrasSeparadas("AutorDto.Nombres", AutorDto.Nombres, "No se permiten letras separadas por espacios."))
            return;

        if (AgregarErrorSiNoSonLetras("AutorDto.Apellidos", AutorDto.Apellidos, "Los apellidos solo deben contener letras y espacios."))
            return;

        if (AgregarErrorSiTieneLetrasSeparadas("AutorDto.Apellidos", AutorDto.Apellidos, "No se permiten letras separadas por espacios en los apellidos."))
            return;

        if (AgregarErrorSiNoSonLetras("AutorDto.Nacionalidad", AutorDto.Nacionalidad, "La nacionalidad solo debe contener letras y espacios."))
            return;

        if (AgregarErrorSiTieneLetrasSeparadas("AutorDto.Nacionalidad", AutorDto.Nacionalidad, "No se permiten letras separadas por espacios en la nacionalidad."))
            return;

        AgregarErrorSiFechaInvalida("AutorDto.FechaNacimiento", AutorDto.FechaNacimiento);
    }

    private void ValidarAutorEditar(string nombres, string? apellidos, string? nacionalidad, DateTime? fechaNacimiento)
    {
        if (AgregarErrorSiCampoVacio("Nombres", nombres, "Ingrese el nombre del autor."))
            return;

        if (AgregarErrorSiNoSonLetras("Nombres", nombres, "El nombre solo debe contener letras y espacios."))
            return;

        if (AgregarErrorSiTieneLetrasSeparadas("Nombres", nombres, "No se permiten letras separadas por espacios."))
            return;

        if (AgregarErrorSiNoSonLetras("Apellidos", apellidos, "Los apellidos solo deben contener letras y espacios."))
            return;

        if (AgregarErrorSiTieneLetrasSeparadas("Apellidos", apellidos, "No se permiten letras separadas por espacios en los apellidos."))
            return;

        if (AgregarErrorSiNoSonLetras("Nacionalidad", nacionalidad, "La nacionalidad solo debe contener letras y espacios."))
            return;

        if (AgregarErrorSiTieneLetrasSeparadas("Nacionalidad", nacionalidad, "No se permiten letras separadas por espacios en la nacionalidad."))
            return;

        AgregarErrorSiFechaInvalida("FechaNacimiento", fechaNacimiento);
    }

    private bool AgregarErrorSiCampoVacio(string key, string? value, string message)
    {
        if (!string.IsNullOrWhiteSpace(value))
            return false;

        ModelState.AddModelError(key, message);
        return true;
    }

    private bool AgregarErrorSiNoSonLetras(string key, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var texto = SepararPalabrasPegadasPorMayuscula(value);
        var regex = new Regex(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑüÜ\s]+$");

        if (regex.IsMatch(texto))
            return false;

        ModelState.AddModelError(key, message);
        return true;
    }

    private bool AgregarErrorSiTieneLetrasSeparadas(string key, string? value, string message)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var partes = LimpiarTexto(value)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!partes.Any(parte => parte.Length == 1))
            return false;

        ModelState.AddModelError(key, message);
        return true;
    }

    private bool AgregarErrorSiFechaInvalida(string key, DateTime? fechaNacimiento)
    {
        if (!fechaNacimiento.HasValue)
            return false;

        if (fechaNacimiento.Value.Date > DateTime.Today)
        {
            ModelState.AddModelError(key, "La fecha de nacimiento no puede ser futura.");
            return true;
        }

        var hoy = DateTime.Today;
        var edad = hoy.Year - fechaNacimiento.Value.Year;

        if (fechaNacimiento.Value.Date > hoy.AddYears(-edad))
            edad--;

        if (edad < 18)
        {
            ModelState.AddModelError(key, "El autor debe ser mayor de edad.");
            return true;
        }

        return false;
    }

    private static string LimpiarTexto(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return Regex.Replace(value.Trim(), @"\s+", " ");
    }

    private static string SepararPalabrasPegadasPorMayuscula(string? value)
    {
        var texto = LimpiarTexto(value);

        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        return Regex.Replace(texto, @"(?<=[a-záéíóúñü])(?=[A-ZÁÉÍÓÚÑÜ])", " ");
    }

    private static string? LimpiarTextoOpcional(string? value)
    {
        var texto = LimpiarTexto(value);
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }

    private static string FormatearObligatorio(string? value)
    {
        return FormatearNombrePropio(SepararPalabrasPegadasPorMayuscula(value));
    }

    private static string? FormatearTextoOpcional(string? value)
    {
        var texto = LimpiarTextoOpcional(value);

        if (string.IsNullOrWhiteSpace(texto))
            return null;

        return FormatearNombrePropio(texto);
    }

    private static string? FormatearApellidoOpcional(string? value)
    {
        var texto = LimpiarTextoOpcional(value);

        if (string.IsNullOrWhiteSpace(texto))
            return null;

        texto = SepararPalabrasPegadasPorMayuscula(texto);
        texto = CorregirApellidosCompuestos(texto);

        return FormatearNombrePropio(texto);
    }

    private static string FormatearNombrePropio(string value)
    {
        value = SepararPalabrasPegadasPorMayuscula(value);

        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        var palabras = value
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(CapitalizarPalabra);

        return string.Join(" ", palabras);
    }

    private static string CapitalizarPalabra(string palabra)
    {
        if (string.IsNullOrWhiteSpace(palabra))
            return string.Empty;

        if (palabra.Length == 1)
            return palabra.ToUpperInvariant();

        return char.ToUpperInvariant(palabra[0]) + palabra[1..];
    }

    private static string CorregirApellidosCompuestos(string value)
    {
        var limpio = LimpiarTexto(value);
        var partes = limpio.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var partesCorregidas = partes.Select(CorregirParteApellidoCompuesto);

        return string.Join(" ", partesCorregidas);
    }

    private static string CorregirParteApellidoCompuesto(string value)
    {
        var clave = value.ToLowerInvariant();

        return clave switch
        {
            "delarosa" => "De La Rosa",
            "delafuente" => "De La Fuente",
            "delacruz" => "De La Cruz",
            "delatorre" => "De La Torre",
            "delvalle" => "Del Valle",
            "delrio" => "Del Río",
            "delrío" => "Del Río",
            "delosrios" => "De Los Ríos",
            "delosríos" => "De Los Ríos",
            "delossantos" => "De Los Santos",
            "delcastillo" => "Del Castillo",
            "delcampo" => "Del Campo",
            "delpilar" => "Del Pilar",
            "delmonte" => "Del Monte",
            "delpozo" => "Del Pozo",
            _ => value
        };
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