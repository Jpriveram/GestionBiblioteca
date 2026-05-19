namespace Frontend.Dtos;

public class EjemplarDto
{
    public int EjemplarId { get; set; }
    public int? UsuarioSesionId { get; set; }
    public int LibroId { get; set; }
    public string? LibroTitulo { get; set; }
    public string CodigoInventario { get; set; } = string.Empty;
    public string? EstadoConservacion { get; set; }
    public bool Disponible { get; set; }
    public bool DadoDeBaja { get; set; }
    public string? MotivoBaja { get; set; }
    public string? Ubicacion { get; set; }
    public bool Estado { get; set; }
    public string RouteToken { get; set; } = string.Empty;
}
