using Microsoft.AspNetCore.Mvc;

namespace ServicioLibroEjemplar.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Health()
    {
        return Ok(new
        {
            service = "ServicioLibroEjemplar",
            status = "running",
            version = "1.0.0",
            timestamp = DateTime.UtcNow,
            endpoints = new
            {
                libros = "/api/Libros",
                libroById = "/api/Libros/{id}",
                libroByIsbn = "/api/Libros/isbn/{isbn}",
                libroByAutor = "/api/Libros/autor/{autorId}",
                ejemplares = "/api/Ejemplares",
                ejemplarById = "/api/Ejemplares/{id}",
                ejemplarByCodigo = "/api/Ejemplares/codigo/{codigoInventario}",
                ejemplarByLibro = "/api/Ejemplares/libro/{libroId}",
                ejemplaresDisponibles = "/api/Ejemplares/disponibles",
                openapi = "/openapi/v1.json"
            }
        });
    }
}
