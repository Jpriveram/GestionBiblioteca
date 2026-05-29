using Microservicio_Prestamo.Models;
using Microsoft.AspNetCore.Mvc;

namespace Microservicio_Prestamo.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DetallesController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<DetalleRecord>> GetAll()
    {
        return Ok(PrestamoDataStore.ObtenerDetalles());
    }

    [HttpGet("prestamo/{prestamoId}")]
    public ActionResult<IEnumerable<DetalleRecord>> GetByPrestamoId(int prestamoId)
    {
        return Ok(PrestamoDataStore.ObtenerDetallesPorPrestamo(prestamoId));
    }
}