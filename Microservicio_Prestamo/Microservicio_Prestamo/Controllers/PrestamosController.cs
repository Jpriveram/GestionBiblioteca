using Microservicio_Prestamo.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio_Prestamo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PrestamosController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<PrestamoRecord>> GetAll([FromQuery] bool todos = false)
    {
        return Ok(PrestamoDataStore.ObtenerPrestamos(todos));
    }

    [HttpGet("{id}")]
    public ActionResult<PrestamoRecord> GetById(int id)
    {
        var prestamo = PrestamoDataStore.ObtenerPrestamo(id);
        return prestamo is null ? NotFound() : Ok(prestamo);
    }

    [HttpGet("activos/count/{lectorId}")]
    public ActionResult<int> CountActivos(int lectorId)
    {
        return Ok(PrestamoDataStore.ContarPrestamosActivos(lectorId));
    }

    [HttpPost]
    public ActionResult<int> Create([FromBody] CrearPrestamoRequest request)
    {
        if (request.Ejemplares.Count == 0)
        {
            return BadRequest(new { error = "Debe seleccionar al menos un ejemplar." });
        }

        var prestamoId = PrestamoDataStore.CrearPrestamo(request);
        return CreatedAtAction(nameof(GetById), new { id = prestamoId }, prestamoId);
    }

    [HttpPost("{id}/anular")]
    public ActionResult Anular(int id, [FromBody] AnularPrestamoRequest request)
    {
        var ok = PrestamoDataStore.AnularPrestamo(id, request.UsuarioSesionId, request.Motivo);
        return ok ? Ok(new { message = "Préstamo anulado correctamente." }) : NotFound();
    }
}