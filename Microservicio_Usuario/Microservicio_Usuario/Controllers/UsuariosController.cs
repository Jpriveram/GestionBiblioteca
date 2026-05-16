using Microsoft.AspNetCore.Mvc;
using ServicioUsuario.Application.Dtos;
using ServicioUsuario.Application.Services;

namespace ServicioUsuario.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;

    public UsuariosController(IUsuarioService usuarioService)
    {
        _usuarioService = usuarioService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetAllAsync()
    {
        var usuarios = await _usuarioService.GetAllAsync();
        return Ok(usuarios);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<UsuarioDto>> GetByIdAsync(int id)
    {
        var usuario = await _usuarioService.GetByIdAsync(id);
        if (usuario == null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(usuario);
    }

    [HttpGet("email/{email}")]
    public async Task<ActionResult<UsuarioDto>> GetByEmailAsync(string email)
    {
        var usuario = await _usuarioService.GetByEmailAsync(email);
        if (usuario == null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(usuario);
    }

    [HttpGet("ci/{ci}")]
    public async Task<ActionResult<UsuarioDto>> GetByCIAsync(string ci)
    {
        var usuario = await _usuarioService.GetByCIAsync(ci);
        if (usuario == null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(usuario);
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> CreateAsync([FromBody] CreateUsuarioDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var usuario = await _usuarioService.CreateAsync(dto);
            return Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UsuarioDto>> UpdateAsync(int id, [FromBody] UpdateUsuarioDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var usuario = await _usuarioService.UpdateAsync(id, dto);
        if (usuario == null)
            return NotFound(new { message = "Usuario no encontrado" });

        return Ok(usuario);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(int id)
    {
        var result = await _usuarioService.DeleteAsync(id);
        if (!result)
            return NotFound(new { message = "Usuario no encontrado" });

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<UsuarioDto>> Login([FromBody] LoginDto dto)
    {
        var usuario = await _usuarioService.LoginAsync(dto.NombreUsuario, dto.Password);
        if (usuario == null)
            return Unauthorized(new { message = "Credenciales inválidas" });
        return Ok(usuario);
    }

    [HttpPost("{id}/cambiar-password")]
    public async Task<IActionResult> CambiarPassword(int id, [FromBody] CambiarPasswordDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            await _usuarioService.CambiarPasswordAsync(id, dto.PasswordActual, dto.PasswordNueva, dto.PasswordConfirmacion);
            return Ok(new { message = "Contrasena actualizada correctamente." });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

public record LoginDto(string NombreUsuario, string Password);
