namespace Frontend.Dtos;

public class LibroDto
{
    public int LibroId { get; set; }
    public int? UsuarioSesionId { get; set; }
    public int AutorId { get; set; }
    public string NombreAutor { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public string? Editorial { get; set; }
    public string? Genero { get; set; }
    public string? Edicion { get; set; }
    public int? AñoPublicacion { get; set; }
    public int? NumeroPaginas { get; set; }
    public string? Idioma { get; set; }
    public string? PaisPublicacion { get; set; }
    public string? Descripcion { get; set; }
    public bool Estado { get; set; }
}
