using Microsoft.AspNetCore.Mvc;
using Microservicio_Autor.Application.Dtos;
using Microservicio_Autor.Application.Interfaces;

namespace Microservicio_Autor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutoresController : ControllerBase
{
    private readonly IAutorService _autorService;

    public AutoresController(IAutorService autorService)
    {
        _autorService = autorService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AutorDto>>> GetAllAsync()
    {
        var autores = await _autorService.GetAllAsync();
        return Ok(autores);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AutorDto>> GetByIdAsync(int id)
    {
        var autor = await _autorService.GetByIdAsync(id);
        if (autor == null)
            return NotFound(new { message = "Autor no encontrado" });

        return Ok(autor);
    }

    [HttpPost]
    public async Task<ActionResult<AutorDto>> CreateAsync([FromBody] CreateAutorDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var autor = await _autorService.CreateAsync(dto);
            return Ok(autor);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AutorDto>> UpdateAsync(int id, [FromBody] UpdateAutorDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var autor = await _autorService.UpdateAsync(id, dto);
            if (autor == null)
                return NotFound(new { message = "Autor no encontrado" });

            return Ok(autor);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        var result = await _autorService.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = "Autor no encontrado" });

        return NoContent();
    }
}