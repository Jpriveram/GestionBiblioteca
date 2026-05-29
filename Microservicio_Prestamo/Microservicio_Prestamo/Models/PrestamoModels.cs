namespace Microservicio_Prestamo.Models;

public sealed class PrestamoRecord
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
}

public sealed class DetalleRecord
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

public sealed class CrearPrestamoRequest
{
    public int LectorId { get; set; }
    public List<CrearPrestamoEjemplarRequest> Ejemplares { get; set; } = new();
    public DateTime FechaDevolucionEsperada { get; set; }
    public int? UsuarioSesionId { get; set; }
}

public sealed class CrearPrestamoEjemplarRequest
{
    public int EjemplarId { get; set; }
    public string? ObservacionesSalida { get; set; }
}

public sealed class AnularPrestamoRequest
{
    public int? UsuarioSesionId { get; set; }
    public string? Motivo { get; set; }
}

public static class PrestamoDataStore
{
    private static readonly object Sync = new();
    private static readonly List<PrestamoRecord> Prestamos = new();
    private static readonly List<DetalleRecord> Detalles = new();
    private static int _prestamoSequence;
    private static int _detalleSequence;

    public static IReadOnlyList<PrestamoRecord> ObtenerPrestamos(bool incluirAnulados = false)
    {
        lock (Sync)
        {
            return Prestamos
                .Where(p => incluirAnulados || p.Estado == 1)
                .OrderByDescending(p => p.FechaPrestamo)
                .ToList();
        }
    }

    public static PrestamoRecord? ObtenerPrestamo(int id)
    {
        lock (Sync)
        {
            return Prestamos.FirstOrDefault(p => p.PrestamoId == id);
        }
    }

    public static IReadOnlyList<DetalleRecord> ObtenerDetalles()
    {
        lock (Sync)
        {
            return Detalles
                .OrderByDescending(d => d.FechaRegistro)
                .ToList();
        }
    }

    public static IReadOnlyList<DetalleRecord> ObtenerDetallesPorPrestamo(int prestamoId)
    {
        lock (Sync)
        {
            return Detalles
                .Where(d => d.PrestamoId == prestamoId)
                .OrderByDescending(d => d.FechaRegistro)
                .ToList();
        }
    }

    public static int ContarPrestamosActivos(int lectorId)
    {
        lock (Sync)
        {
            return Prestamos.Count(p => p.Estado == 1 && p.LectorId == lectorId);
        }
    }

    public static int CrearPrestamo(CrearPrestamoRequest request)
    {
        lock (Sync)
        {
            var prestamoId = ++_prestamoSequence;
            Prestamos.Add(new PrestamoRecord
            {
                PrestamoId = prestamoId,
                LectorId = request.LectorId,
                FechaPrestamo = DateTime.Now,
                FechaDevolucionEsperada = request.FechaDevolucionEsperada,
                Estado = 1,
                UsuarioSesionId = request.UsuarioSesionId,
                ObservacionesSalida = request.Ejemplares.FirstOrDefault()?.ObservacionesSalida,
                FechaRegistro = DateTime.Now
            });

            foreach (var ejemplar in request.Ejemplares)
            {
                Detalles.Add(new DetalleRecord
                {
                    DetalleId = ++_detalleSequence,
                    PrestamoId = prestamoId,
                    EjemplarId = ejemplar.EjemplarId,
                    EstadoDetalle = 1,
                    ObservacionesSalida = ejemplar.ObservacionesSalida,
                    UsuarioSesionId = request.UsuarioSesionId,
                    FechaRegistro = DateTime.Now
                });
            }

            return prestamoId;
        }
    }

    public static bool AnularPrestamo(int prestamoId, int? usuarioSesionId, string? motivo)
    {
        lock (Sync)
        {
            var prestamo = Prestamos.FirstOrDefault(p => p.PrestamoId == prestamoId);
            if (prestamo is null)
            {
                return false;
            }

            prestamo.Estado = 0;
            prestamo.UsuarioSesionId = usuarioSesionId;
            prestamo.ObservacionesEntrada = motivo ?? prestamo.ObservacionesEntrada;
            prestamo.UltimaActualizacion = DateTime.Now;
            return true;
        }
    }
}