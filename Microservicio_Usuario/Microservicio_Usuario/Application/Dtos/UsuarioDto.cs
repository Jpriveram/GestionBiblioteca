namespace ServicioUsuario.Application.Dtos;

public class UsuarioDto
{
    public int UsuarioId { get; set; }
    public int? UsuarioSesionId { get; set; }
    public string CI { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? NombreUsuario { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public bool DebeCambiarPassword { get; set; }
}

public class CambiarPasswordDto
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
    public string PasswordConfirmacion { get; set; } = string.Empty;
}

public class CreateUsuarioDto
{
    public int? UsuarioSesionId { get; set; }
    public string CI { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? NombreUsuario { get; set; }
    public string? Password { get; set; }
    public string Rol { get; set; } = string.Empty;
}

public class UpdateUsuarioDto
{
    public int? UsuarioSesionId { get; set; }
    public string CI { get; set; } = string.Empty;
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? NombreUsuario { get; set; }
    public string Rol { get; set; } = string.Empty;
    public bool Estado { get; set; }
}
