namespace Microservicio_Autor.Domain.Entities;

public class Autor
{
    public int AutorId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Nacionalidad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public DateTime? UltimaActualizacion { get; set; }
    public int? UsuarioSesionId { get; set; }
}