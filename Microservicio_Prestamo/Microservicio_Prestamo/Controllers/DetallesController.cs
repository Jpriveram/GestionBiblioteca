using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microservicio_Prestamo.Domain.Ports;

namespace Microservicio_Prestamo.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DetallesController : ControllerBase
{
    private readonly IPrestamoRepository _repo;

    public DetallesController(IPrestamoRepository repo) => _repo = repo;

    [HttpGet("prestamo/{prestamoId}")]
    public IActionResult GetByPrestamo(int prestamoId)
    {
        var detalles = _repo.GetDetallesByPrestamoId(prestamoId);
        return Ok(detalles.Select(d => new
        {
            d.DetalleId,
            d.PrestamoId,
            d.EjemplarId,
            d.EstadoDetalle,
            d.ObservacionesSalida,
            d.ObservacionesEntrada,
            d.UsuarioSesionId,
            d.FechaRegistro
        }));
    }
}
