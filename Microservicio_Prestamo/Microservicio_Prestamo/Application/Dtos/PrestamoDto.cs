namespace Microservicio_Prestamo.Application.Dtos;

public class PrestamoDto
{
    public int PrestamoId { get; set; }
    public int LectorId { get; set; }
    public string? LectorNombre { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    public string? ObservacionesSalida { get; set; }
    public string? ObservacionesEntrada { get; set; }
    public int Estado { get; set; }
    public int? UsuarioSesionId { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
    public List<PrestamoDetalleDto> Detalles { get; set; } = new();
}

public class PrestamoDetalleDto
{
    public int DetalleId { get; set; }
    public int EjemplarId { get; set; }
    public string? CodigoInventario { get; set; }
    public string? TituloLibro { get; set; }
    public string? ObservacionesSalida { get; set; }
    public byte EstadoDetalle { get; set; }
}

public class CreatePrestamoDto
{
    public int LectorId { get; set; }
    public List<CreatePrestamoEjemplarDto> Ejemplares { get; set; } = new();
    public DateTime? FechaDevolucionEsperada { get; set; }
    public string? ObservacionesSalida { get; set; }
    public int? UsuarioSesionId { get; set; }
}

public class CreatePrestamoEjemplarDto
{
    public int EjemplarId { get; set; }
    public string? ObservacionesSalida { get; set; }
}
