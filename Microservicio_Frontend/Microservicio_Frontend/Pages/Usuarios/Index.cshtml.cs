using System.Collections.Generic;
using Frontend.Dtos;
using Frontend.Adapters;
using Frontend.Dtos;
using Frontend.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Usuarios;

public class IndexModel : PageModel
{
    private readonly IUsuarioServicio _usuarioServicio;
    private readonly RouteTokenService _routeTokenService;

    public List<UsuarioListadoItem> Usuarios { get; set; } = new();

    [BindProperty]
    public UsuarioDto NuevoUsuario { get; set; } = new();

    [BindProperty]
    public string? Complemento { get; set; }

    [BindProperty]
    public string RolNuevoUsuario { get; set; } = Roles.Bibliotecario;

    public string? MensajeError { get; set; }
    public string? MensajeOk { get; set; }

    public bool IsAdmin { get; set; }
    public bool AbrirModalCrear { get; set; }

    public IndexModel(IUsuarioServicio usuarioServicio, RouteTokenService routeTokenService)
    {
        _usuarioServicio = usuarioServicio;
        _routeTokenService = routeTokenService;
    }

    public IActionResult OnGet()
    {
        if (!EsAdmin())
        {
            return LocalRedirect("/");
        }

        IsAdmin = true;

        CargarUsuarios();
        return Page();
    }

    public async Task<IActionResult> OnPostCrearAsync(CancellationToken cancellationToken)
    {
        if (!EsAdmin())
        {
            return LocalRedirect("/");
        }

        IsAdmin = true;

        var usuarioSesionId = ObtenerUsuarioSesionId();
        if (!usuarioSesionId.HasValue)
        {
            return RedirectToPage("/Login");
        }

        NuevoUsuario.Rol = RolNuevoUsuario;
        NuevoUsuario.UsuarioSesionId = usuarioSesionId.Value;
        NuevoUsuario.Nombres = NuevoUsuario.Nombres.ToDisplayName();
        NuevoUsuario.PrimerApellido = NuevoUsuario.PrimerApellido.ToDisplayName();
        NuevoUsuario.SegundoApellido = NuevoUsuario.SegundoApellido.ToDisplayName();
        NuevoUsuario.Complemento = Complemento;

        if (NuevoUsuario.Rol == Roles.Lector)
        {
            var lectorDto = new LectorDto
            {
                CI = NuevoUsuario.CI,
                Complemento = Complemento,
                Nombres = NuevoUsuario.Nombres,
                PrimerApellido = NuevoUsuario.PrimerApellido,
                SegundoApellido = NuevoUsuario.SegundoApellido,
                Email = NuevoUsuario.Email
            };

            var resultadoLector = _usuarioServicio.CrearLector(lectorDto, usuarioSesionId.Value);

            if (resultadoLector.IsFailure)
            {
                AgregarErrorCreacion(resultadoLector.Error);
                CargarUsuarios();
                return Page();
            }

            TempData["MensajeOk"] = "Lector creado correctamente.";
            return RedirectToPage();
        }

        var resultado = await _usuarioServicio.CrearUsuarioAsync(NuevoUsuario, usuarioSesionId.Value, cancellationToken);

        if (resultado.IsFailure)
        {
            AgregarErrorCreacion(resultado.Error);
            CargarUsuarios();
            return Page();
        }

        TempData["MensajeOk"] = "Usuario creado correctamente. Se enviaron credenciales por correo.";
        return RedirectToPage();
    }

    private void AgregarErrorCreacion(Error? error)
    {
        AbrirModalCrear = true;
        if (error is null)
        {
            MensajeError = "No se pudo crear el usuario.";
            return;
        }

        var key = error.Code switch
        {
            "Usuario.CI" => "NuevoUsuario.CI",
            "Usuario.Complemento" => nameof(Complemento),
            "Usuario.Nombres" => "NuevoUsuario.Nombres",
            "Usuario.PrimerApellido" => "NuevoUsuario.PrimerApellido",
            "Usuario.SegundoApellido" => "NuevoUsuario.SegundoApellido",
            "Usuario.Email" => "NuevoUsuario.Email",
            "Usuario.Rol" => nameof(RolNuevoUsuario),
            _ => string.Empty
        };

        if (string.IsNullOrWhiteSpace(key))
        {
            MensajeError = error.Message;
            return;
        }

        ModelState.AddModelError(key, error.Message);
    }

    public IActionResult OnPostBaja(string token)
    {
        if (!EsAdmin())
        {
            return LocalRedirect("/");
        }

        IsAdmin = true;

        var usuarioSesionId = ObtenerUsuarioSesionId();
        if (!usuarioSesionId.HasValue)
        {
            return RedirectToPage("/Login");
        }

        if (!_routeTokenService.TryObtenerId(token, out var usuarioId))
        {
            return NotFound();
        }

        var resultado = _usuarioServicio.DarDeBaja(usuarioId, usuarioSesionId.Value);

        if (resultado.IsFailure)
        {
            ModelState.AddModelError(string.Empty, resultado.Error.Message);
            CargarUsuarios();
            return Page();
        }

        TempData["MensajeOk"] = "Usuario dado de baja correctamente.";
        return RedirectToPage();
    }

    private void CargarUsuarios()
    {
        var usuarios = _usuarioServicio.Select();
        Usuarios = new List<UsuarioListadoItem>();

        foreach (var u in usuarios)
        {
            Usuarios.Add(new UsuarioListadoItem
            {
                UsuarioId = u.UsuarioId,
                UsuarioIdToken = _routeTokenService.CrearToken(u.UsuarioId),
                Nombres = (u.Nombres ?? string.Empty).ToDisplayName(),
                PrimerApellido = (u.PrimerApellido ?? string.Empty).ToDisplayName(),
                SegundoApellido = (u.SegundoApellido ?? string.Empty).ToDisplayName(),
                Email = u.Email ?? string.Empty,
                NombreUsuario = u.NombreUsuario ?? string.Empty,
                Rol = u.Rol ?? string.Empty,
                Estado = u.Estado
            });
        }

        MensajeOk = TempData["MensajeOk"]?.ToString();
    }

    private bool EsAdmin()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);
        return string.Equals(rol, Roles.Admin, StringComparison.Ordinal);
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

    public class UsuarioListadoItem
    {
        public int UsuarioId { get; set; }
        public string UsuarioIdToken { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public string PrimerApellido { get; set; } = string.Empty;
        public string SegundoApellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NombreUsuario { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public bool Estado { get; set; }
    }
}
