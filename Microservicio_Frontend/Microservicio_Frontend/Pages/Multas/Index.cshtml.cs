using Frontend.Adapters;
using Frontend.Dtos;
using Frontend.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Multas;

public class IndexModel : PageModel
{
    private readonly IMultaServicio _multaServicio;
    private readonly IUsuarioServicio _usuarioServicio;

    public List<MultaDto> Multas { get; set; } = new();
    public List<UsuarioDto> Usuarios { get; set; } = new();
    public string? MensajeError { get; set; }
    public string? MensajeOk { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? UsuarioIdFiltro { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? BusquedaUsuario { get; set; }

    // Create
    [BindProperty] public int UsuarioId { get; set; }
    [BindProperty] public decimal Monto { get; set; }
    [BindProperty] public string Motivo { get; set; } = string.Empty;

    // Edit
    [BindProperty] public string? EditId { get; set; }
    [BindProperty] public decimal EditMonto { get; set; }
    [BindProperty] public string EditMotivo { get; set; } = string.Empty;
    [BindProperty] public bool EditEstado { get; set; }

    public IndexModel(IMultaServicio multaServicio, IUsuarioServicio usuarioServicio)
    {
        _multaServicio = multaServicio;
        _usuarioServicio = usuarioServicio;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!EsBibliotecario()) return LocalRedirect("/");
        await CargarAsync();
        MensajeOk = TempData["MensajeOk"]?.ToString();
        MensajeError = TempData["MensajeError"]?.ToString();
        return Page();
    }

    public async Task<IActionResult> OnPostCrearAsync()
    {
        if (!EsBibliotecario()) return LocalRedirect("/");
        var sid = ObtenerUsuarioSesionId();
        if (!sid.HasValue) return RedirectToPage("/Login");

        var dto = new MultaDto { UsuarioId = UsuarioId, Monto = Monto, Motivo = Motivo, UsuarioSesionId = sid.Value };
        var result = await _multaServicio.CreateAsync(dto);

        if (result.IsFailure) { TempData["MensajeError"] = result.Error?.Message; return RedirectToPage(); }
        TempData["MensajeOk"] = "Multa creada correctamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditarAsync()
    {
        if (!EsBibliotecario()) return LocalRedirect("/");
        var sid = ObtenerUsuarioSesionId();
        if (!sid.HasValue) return RedirectToPage("/Login");

        var dto = new MultaDto { Id = EditId, Monto = EditMonto, Motivo = EditMotivo, Estado = EditEstado, UsuarioSesionId = sid.Value };
        var result = await _multaServicio.UpdateAsync(dto);

        if (result.IsFailure) { TempData["MensajeError"] = result.Error?.Message; return RedirectToPage(); }
        TempData["MensajeOk"] = "Multa actualizada correctamente.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostBajaAsync(string id)
    {
        if (!EsBibliotecario()) return LocalRedirect("/");
        var result = await _multaServicio.DeleteAsync(id);
        TempData[result.IsFailure ? "MensajeError" : "MensajeOk"] = result.IsFailure ? result.Error?.Message : "Multa dada de baja.";
        return RedirectToPage();
    }

    private async Task CargarAsync()
    {
        var tareas = new List<Task>();
        tareas.Add(Task.Run(async () => Multas = (await _multaServicio.SelectAsync(UsuarioIdFiltro)).ToList()));
        tareas.Add(Task.Run(() => { Usuarios = _usuarioServicio.Select().Where(u => u.Estado && u.Rol == "Lector").ToList(); }));
        await Task.WhenAll(tareas);
    }

    private bool EsBibliotecario()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);
        return string.Equals(rol, Roles.Bibliotecario, StringComparison.Ordinal);
    }

    private int? ObtenerUsuarioSesionId()
    {
        var sid = HttpContext.Session.GetString(SessionKeys.UsuarioId);
        return int.TryParse(sid, out var id) ? id : null;
    }
}
