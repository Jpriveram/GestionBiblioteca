namespace Microservicio_Prestamo.Domain.Entities;

public class Prestamo
{
    public int PrestamoId { get; set; }
    public int LectorId { get; set; }
    public DateTime FechaPrestamo { get; set; } = DateTime.Now;
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    public string? ObservacionesSalida { get; set; }
    public string? ObservacionesEntrada { get; set; }
    public int Estado { get; set; } = 1;
    public int? UsuarioSesionId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public DateTime? UltimaActualizacion { get; set; }
    public ICollection<Detalle> Detalles { get; set; } = new List<Detalle>();
}

public class Detalle
{
    public int DetalleId { get; set; }
    public int PrestamoId { get; set; }
    public int EjemplarId { get; set; }
    public byte EstadoDetalle { get; set; } = 1;
    public DateTime? FechaDevolucionReal { get; set; }
    public string? ObservacionesSalida { get; set; }
    public string? ObservacionesEntrada { get; set; }
    public int? UsuarioSesionId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public DateTime? UltimaActualizacion { get; set; }
}

public class OutboxMessage
{
    public long Id { get; set; }
    public string MessageId { get; set; } = Guid.NewGuid().ToString();
    public string EventType { get; set; } = "";
    public string Payload { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool Processed { get; set; } = false;
}
