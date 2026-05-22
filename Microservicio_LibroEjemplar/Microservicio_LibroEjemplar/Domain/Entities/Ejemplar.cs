namespace ServicioLibroEjemplar.Domain.Entities;

public class Ejemplar
{
    public int EjemplarId { get; set; }
    public int LibroId { get; set; }
    public string CodigoInventario { get; set; } = string.Empty;
    public string? EstadoConservacion { get; set; }
    public bool Disponible { get; set; } = true;
    public bool DadoDeBaja { get; set; } = false;
    public string? MotivoBaja { get; set; }
    public string? Ubicacion { get; set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? UltimaActualizacion { get; set; }
    public int? UsuarioSesionId { get; set; }

    // Navigation property
    public Libro? Libro { get; set; }
}
