using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServicioLibroEjemplar.Application.Dtos;
using ServicioLibroEjemplar.Application.Interfaces;
using ServicioLibroEjemplar.Infrastructure.Persistence;

namespace ServicioLibroEjemplar.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EjemplaresController : ControllerBase
{
    private readonly IEjemplarService _ejemplarService;
    private readonly EjemplarRepository _ejemplarRepo;

    public EjemplaresController(IEjemplarService ejemplarService, EjemplarRepository ejemplarRepo)
    {
        _ejemplarService = ejemplarService;
        _ejemplarRepo = ejemplarRepo;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EjemplarDto>>> GetAllAsync()
    {
        var ejemplares = await _ejemplarService.GetAllAsync();
        return Ok(ejemplares);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EjemplarDto>> GetByIdAsync(int id)
    {
        var ejemplar = await _ejemplarService.GetByIdAsync(id);
        if (ejemplar == null)
            return NotFound(new { message = "Ejemplar no encontrado" });

        return Ok(ejemplar);
    }

    [HttpGet("codigo/{codigoInventario}")]
    public async Task<ActionResult<EjemplarDto>> GetByCodigoInventarioAsync(string codigoInventario)
    {
        var ejemplar = await _ejemplarService.GetByCodigoInventarioAsync(codigoInventario);
        if (ejemplar == null)
            return NotFound(new { message = "Ejemplar no encontrado" });

        return Ok(ejemplar);
    }

    [HttpGet("libro/{libroId}")]
    public async Task<ActionResult<IEnumerable<EjemplarDto>>> GetByLibroIdAsync(int libroId)
    {
        var ejemplares = await _ejemplarService.GetByLibroIdAsync(libroId);
        return Ok(ejemplares);
    }

    [HttpGet("disponibles")]
    public async Task<ActionResult<IEnumerable<EjemplarDto>>> GetDisponiblesAsync()
    {
        var ejemplares = await _ejemplarService.GetDisponiblesAsync();
        return Ok(ejemplares);
    }

    [HttpPost("reservar-lote")]
    [AllowAnonymous]
    public IActionResult ReservarLote([FromBody] ReservarLoteRequest request)
    {
        try
        {
            foreach (var id in request.EjemplarIds)
            {
                var ejemplar = _ejemplarRepo.GetById(id);
                if (ejemplar is null || !ejemplar.Disponible)
                    return BadRequest(new { message = $"Ejemplar {id} no disponible" });

                ejemplar.Disponible = false;
                _ejemplarRepo.Update(ejemplar);
            }
            return Ok(new { message = "Ejemplares reservados" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpPost]
    public async Task<ActionResult<EjemplarDto>> CreateAsync([FromBody] CreateEjemplarDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var ejemplar = await _ejemplarService.CreateAsync(dto);
            return Ok(ejemplar);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EjemplarDto>> UpdateAsync(int id, [FromBody] UpdateEjemplarDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var ejemplar = await _ejemplarService.UpdateAsync(id, dto);
            if (ejemplar == null)
                return NotFound(new { message = "Ejemplar no encontrado" });

            return Ok(ejemplar);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteAsync(int id)
    {
        var resultado = await _ejemplarService.DeleteAsync(id);
        if (!resultado)
            return NotFound(new { message = "Ejemplar no encontrado" });

        return NoContent();
    }
}

public class ReservarLoteRequest
{
    public List<int> EjemplarIds { get; set; } = new();
    public int? UsuarioSesionId { get; set; }
}
