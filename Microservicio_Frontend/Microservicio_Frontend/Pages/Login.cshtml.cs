using Frontend.Adapters;
using Frontend.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages;

public class LoginModel : PageModel
{
    private readonly IUsuarioServicio _usuarioServicio;

    [BindProperty]
    public string NombreUsuario { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    public string? MensajeError { get; set; }

    public LoginModel(IUsuarioServicio usuarioServicio)
    {
        _usuarioServicio = usuarioServicio;
    }

    public IActionResult OnGet()
    {
        return Redirect("/");
    }

    public IActionResult OnPost()
    {
        var resultado = _usuarioServicio.Login(NombreUsuario, Password);

        if (resultado.IsFailure)
        {
            TempData["LoginError"] = resultado.Error.Message;
            return Redirect("/");
        }

        var UsuarioDto = resultado.Value;

        HttpContext.Session.SetString(SessionKeys.UsuarioId, UsuarioDto.UsuarioId.ToString());
        HttpContext.Session.SetString(SessionKeys.NombreUsuario, UsuarioDto.NombreUsuario ?? string.Empty);
        HttpContext.Session.SetString(SessionKeys.Rol, UsuarioDto.Rol);
        HttpContext.Session.SetString(SessionKeys.DebeCambiarPassword, UsuarioDto.DebeCambiarPassword ? "true" : "false");

        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return Redirect("/");
    }

    public async Task<IActionResult> OnPostCambiarPasswordModalAsync(
        string passwordActualModal,
        string passwordNuevaModal,
        string passwordConfirmacionModal,
        string? returnUrl,
        CancellationToken cancellationToken)
    {
        var usuarioSesion = HttpContext.Session.GetString(SessionKeys.UsuarioId);

        if (!int.TryParse(usuarioSesion, out var usuarioId))
        {
            return Redirect("/");
        }

        var resultado = await _usuarioServicio.CambiarPasswordAsync(
            usuarioId,
            passwordActualModal,
            passwordNuevaModal,
            passwordConfirmacionModal,
            cancellationToken);

        if (resultado.IsFailure)
        {
            TempData["ChangePasswordError"] = resultado.Error.Message;
        }
        else
        {
            HttpContext.Session.SetString(SessionKeys.DebeCambiarPassword, "false");
            TempData["ChangePasswordOk"] = "Contrasena actualizada correctamente.";
        }

        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return Redirect("/");
    }

    public IActionResult OnPostLogout()
    {
        HttpContext.Session.Clear();
        Response.Cookies.Delete(".AspNetCore.Session");

        Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
        Response.Headers["Pragma"] = "no-cache";
        Response.Headers["Expires"] = "0";

        return Redirect("/");
    }
}