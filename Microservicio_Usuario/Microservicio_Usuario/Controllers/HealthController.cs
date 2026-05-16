using Microsoft.AspNetCore.Mvc;

namespace ServicioUsuario.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Health()
    {
        return Ok(new
        {
            service = "ServicioUsuario",
            status = "running",
            version = "1.0.0",
            timestamp = DateTime.UtcNow,
            endpoints = new
            {
                usuarios = "/api/Usuarios",
                usuarioById = "/api/Usuarios/{id}",
                usuarioByEmail = "/api/Usuarios/email/{email}",
                usuarioByCi = "/api/Usuarios/ci/{ci}",
                openapi = "/openapi/v1.json"
            }
        });
    }
}
