namespace Microservicio_Autor.Application.Dtos;

public class AutorDto
{
    public int AutorId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Apellidos { get; set; }
    public string? Nacionalidad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Biografia { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
    public int? UsuarioSesionId { get; set; }
}