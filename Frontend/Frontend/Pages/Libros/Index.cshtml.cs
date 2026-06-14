using Frontend.Dtos;
using Frontend.Adapters;
using Frontend.Dtos;
using Frontend.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Libros;

public class IndexModel : PageModel
{
    private readonly ILibroServicio _libroServicio;

    public IEnumerable<LibroDto> Libros { get; private set; } = new List<LibroDto>();

    public IndexModel(ILibroServicio libroServicio)
    {
        _libroServicio = libroServicio;
    }

    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        Libros = _libroServicio.Select(todos: EsAdmin());
        return Page();
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
