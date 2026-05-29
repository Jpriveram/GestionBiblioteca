namespace Frontend.Dtos;

public class PrestamoDetalleDTO
{
    public int PrestamoId { get; set; }
    public int EjemplarId { get; set; }
    public int LectorId { get; set; }
    
    // Información del Libro
    public string TituloLibro { get; set; } = string.Empty;
    public string CodigoInventario { get; set; } = string.Empty;
    public List<string> Libros { get; set; } = new();
    public List<string> Codigos { get; set; } = new();
    public List<string> ObservacionesPorLibro { get; set; } = new();
    
    // Información del Lector
    public string NombreLector { get; set; } = string.Empty;
    
    // Fechas
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    
    // Observaciones
    public string? ObservacionesSalida { get; set; }
    public string? ObservacionesEntrada { get; set; }
    
    // Estado
    public int Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    
    public bool EstaDevuelto => FechaDevolucionReal.HasValue;
}
