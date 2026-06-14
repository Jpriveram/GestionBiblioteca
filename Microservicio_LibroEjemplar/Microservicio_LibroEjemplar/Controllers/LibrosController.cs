using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServicioLibroEjemplar.Application.Dtos;
using ServicioLibroEjemplar.Application.Interfaces;

namespace ServicioLibroEjemplar.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LibrosController : ControllerBase
{
    private readonly ILibroService _libroService;

    public LibrosController(ILibroService libroService)
    {
        _libroService = libroService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<LibroDto>>> GetAllAsync([FromQuery] bool todos = false)
    {
        var libros = await _libroService.GetAllAsync(todos);
        return Ok(libros);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<LibroDto>> GetByIdAsync(int id)
    {
        var libro = await _libroService.GetByIdAsync(id);
        if (libro == null)
            return NotFound(new { message = "Libro no encontrado" });

        return Ok(libro);
    }

    [HttpGet("isbn/{isbn}")]
    public async Task<ActionResult<LibroDto>> GetByIsbnAsync(string isbn)
    {
        var libro = await _libroService.GetByIsbnAsync(isbn);
        if (libro == null)
            return NotFound(new { message = "Libro no encontrado" });

        return Ok(libro);
    }

    [HttpGet("autor/{autorId}")]
    public async Task<ActionResult<IEnumerable<LibroDto>>> GetByAutorIdAsync(int autorId)
    {
        var libros = await _libroService.GetByAutorIdAsync(autorId);
        return Ok(libros);
    }

    [HttpPost]
    public async Task<ActionResult<LibroDto>> CreateAsync([FromBody] CreateLibroDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var libro = await _libroService.CreateAsync(dto);
            return Ok(libro);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<LibroDto>> UpdateAsync(int id, [FromBody] UpdateLibroDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var libro = await _libroService.UpdateAsync(id, dto);
            if (libro == null)
                return NotFound(new { message = "Libro no encontrado" });

            return Ok(libro);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        var resultado = await _libroService.DeleteAsync(id);
        if (!resultado)
            return NotFound(new { message = "Libro no encontrado" });

        return NoContent();
    }
}
