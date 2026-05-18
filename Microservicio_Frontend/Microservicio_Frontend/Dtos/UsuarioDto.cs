namespace Frontend.Dtos;

public class UsuarioDto
{
    public int UsuarioId { get; set; }
    public string? CI { get; set; }
    public int? UsuarioSesionId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? NombreUsuario { get; set; }
    public string? PasswordHash { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public bool DebeCambiarPassword { get; set; }
}