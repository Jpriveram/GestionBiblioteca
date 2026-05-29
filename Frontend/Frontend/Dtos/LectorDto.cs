namespace Frontend.Dtos;

public class LectorDto
{
    public int UsuarioId { get; set; }
    public string? CI { get; set; }
    public string? Complemento { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string PrimerApellido { get; set; } = string.Empty;
    public string? SegundoApellido { get; set; }
    public string Email { get; set; } = string.Empty;
}
