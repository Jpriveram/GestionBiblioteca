using Microsoft.AspNetCore.Mvc;
using ServicioMultas.Application.Dtos;
using ServicioMultas.Application.Interfaces;

namespace ServicioMultas.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MultasController : ControllerBase
{
    private readonly IMultaService _multaService;

    public MultasController(IMultaService multaService)
    {
        _multaService = multaService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MultaDto>>> GetAllAsync([FromQuery] int? usuarioId)
    {
        var multas = await _multaService.GetAllAsync(usuarioId);
        return Ok(multas);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MultaDto>> GetByIdAsync(string id)
    {
        var multa = await _multaService.GetByIdAsync(id);
        if (multa is null)
            return NotFound(new { message = "Multa no encontrada" });

        return Ok(multa);
    }

    [HttpPost]
    public async Task<ActionResult<MultaDto>> CreateAsync([FromBody] CreateMultaDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var multa = await _multaService.CreateAsync(dto);
            return Ok(multa);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<MultaDto>> UpdateAsync(string id, [FromBody] UpdateMultaDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var multa = await _multaService.UpdateAsync(id, dto);
            if (multa is null)
                return NotFound(new { message = "Multa no encontrada" });

            return Ok(multa);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        var result = await _multaService.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = "Multa no encontrada" });

        return NoContent();
    }
}
