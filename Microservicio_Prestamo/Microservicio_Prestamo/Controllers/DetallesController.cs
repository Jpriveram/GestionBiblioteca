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

    [HttpGet]
    public IActionResult GetAll()
    {
        var prestamos = _repo.GetAll(false).ToList();
        var allDetalles = new List<object>();
        foreach (var p in prestamos)
        {
            var detalles = _repo.GetDetallesByPrestamoId(p.PrestamoId);
            allDetalles.AddRange(detalles.Select(d => new
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
        return Ok(allDetalles);
    }

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
