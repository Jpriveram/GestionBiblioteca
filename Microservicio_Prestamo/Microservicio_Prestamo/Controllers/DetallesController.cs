using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicio_Prestamo.Application.Dtos;
using Microservicio_Prestamo.Domain.Entities;
using Microservicio_Prestamo.Domain.Ports;

namespace Microservicio_Prestamo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DetallesController : ControllerBase
{
    private readonly IPrestamoRepository _repo;

    public DetallesController(IPrestamoRepository repo) => _repo = repo;

    [HttpGet]
    public ActionResult<IEnumerable<PrestamoDetalleDto>> GetAll()
    {
        var prestamos = _repo.GetAll(false).ToList();
        var result = new List<PrestamoDetalleDto>();
        // Nota: los detalles se obtienen de la tabla detalle via el repositorio
        // Por simplicidad retornamos vacío — el frontend usará los datos del prestamo
        return Ok(result);
    }

    [HttpGet("prestamo/{prestamoId}")]
    public ActionResult<IEnumerable<PrestamoDetalleDto>> GetByPrestamo(int prestamoId)
    {
        return Ok(new List<PrestamoDetalleDto>());
    }
}
