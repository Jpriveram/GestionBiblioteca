using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicio_Prestamo.Application.Dtos;
using Microservicio_Prestamo.Application.Interfaces;

namespace Microservicio_Prestamo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PrestamosController : ControllerBase
{
    private readonly IPrestamoService _service;

    public PrestamosController(IPrestamoService service) => _service = service;

    [HttpGet]
    public ActionResult<IEnumerable<PrestamoDto>> GetAll([FromQuery] int? lectorId, [FromQuery] bool incluirAnulados = false)
    {
        return Ok(_service.GetAll(lectorId, incluirAnulados));
    }

    [HttpGet("{id}")]
    public ActionResult<PrestamoDto> GetById(int id)
    {
        var p = _service.GetById(id);
        return p is not null ? Ok(p) : NotFound(new { message = "Préstamo no encontrado" });
    }

    [HttpGet("activos/count/{lectorId}")]
    public ActionResult<int> CountActivos(int lectorId)
    {
        return Ok(_service.CountActivosByLector(lectorId));
    }

    [HttpPost]
    public ActionResult<PrestamoDto> Create([FromBody] CreatePrestamoDto dto)
    {
        try
        {
            var result = _service.Create(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/anular")]
    public IActionResult Anular(int id, [FromBody] AnularRequest? request)
    {
        try
        {
            _service.Anular(id, request?.UsuarioSesionId, request?.Motivo);
            return Ok(new { message = "Préstamo anulado correctamente" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public class AnularRequest
{
    public int? UsuarioSesionId { get; set; }
    public string? Motivo { get; set; }
}
