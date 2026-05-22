using Frontend.Dtos;
using Frontend.Adapters;
using Frontend.Dtos;
using Frontend.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Frontend.Pages.Libros;

public class CreateModel : PageModel
{
    private readonly ILibroServicio _libroServicio;

    [BindProperty]
    public LibroDto Libro { get; set; } = new();

    public CreateModel(ILibroServicio libroServicio)
    {
        _libroServicio = libroServicio;
    }

    public IActionResult OnGet()
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        Libro.Estado = true;
        return Page();
    }

    public IActionResult OnPost()
    {
        if (!EsAdminOBibliotecario())
        {
            return LocalRedirect("/");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = _libroServicio.Create(Libro, null);

        if (result.IsFailure)
        {
            ModelState.AddModelError(string.Empty, result.Error.Message);
            return Page();
        }

        return RedirectToPage("Index");
    }

    private bool EsAdminOBibliotecario()
    {
        var rol = HttpContext.Session.GetString(SessionKeys.Rol);

        return string.Equals(rol, Roles.Admin, StringComparison.OrdinalIgnoreCase)
            || string.Equals(rol, Roles.Bibliotecario, StringComparison.OrdinalIgnoreCase);
    }
}
