namespace Microservicio_Autor.Application.Dtos;

public class CreateAutorDto
{
    public string Nombres { get; set; } = string.Empty;
    public string? Apellidos { get; set; }
    public string? Nacionalidad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Biografia { get; set; }
}