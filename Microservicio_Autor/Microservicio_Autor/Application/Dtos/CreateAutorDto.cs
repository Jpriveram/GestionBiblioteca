namespace Microservicio_Autor.Application.Dtos;

public class CreateAutorDto
{
    public string Nombres { get; set; } = string.Empty;
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
    public string? Nacionalidad { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Biografia { get; set; }
}