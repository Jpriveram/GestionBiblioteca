namespace Microservicio_Reportes.Models;

public sealed class PrestamoDto
{
    public int PrestamoId { get; set; }
    public int LectorId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    public int Estado { get; set; }
}

public sealed class DetalleDto
{
    public int DetalleId { get; set; }
    public int PrestamoId { get; set; }
    public int EjemplarId { get; set; }
    public string? ObservacionesSalida { get; set; }
}

public sealed class UsuarioDto
{
    public int UsuarioId { get; set; }
    public string? Nombres { get; set; }
    public string? PrimerApellido { get; set; }
    public string? SegundoApellido { get; set; }
}

public sealed class EjemplarDto
{
    public int EjemplarId { get; set; }
    public int LibroId { get; set; }
    public string? CodigoInventario { get; set; }
    public string? LibroTitulo { get; set; }
}

public sealed class LibroDto
{
    public int LibroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
}

public sealed class ReporteLibroDto
{
    public string TituloLibro { get; set; } = string.Empty;
    public int CantidadPrestamos { get; set; }
}

public sealed class PrestamoOrdenadoDto
{
    public DateTime FechaPrestamo { get; set; }
    public string NombreLector { get; set; } = string.Empty;
    public string TituloLibro { get; set; } = string.Empty;
    public string CodigoInventario { get; set; } = string.Empty;
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    public string EstadoTexto { get; set; } = string.Empty;
}