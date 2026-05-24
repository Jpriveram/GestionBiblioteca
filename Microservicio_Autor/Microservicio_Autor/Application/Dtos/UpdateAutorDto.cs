namespace Microservicio_Autor.Application.Dtos;

public class UpdateAutorDto
{
    public string Nombres { get; set; } = string.Empty;
    public string? Apellidos { get; set; }
    public string? Nacionalidad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public bool Estado { get; set; }
}