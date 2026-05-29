using Frontend.Dtos;
using Frontend.Helpers;
using Frontend.Dtos;

namespace Frontend.Adapters;

public interface IDetalleServicio
{
    IEnumerable<DetalleDto> Select();
    IEnumerable<DetalleDto> ObtenerPorPrestamo(int prestamoId);
    IEnumerable<DetalleDto> ObtenerTodos();
    Result CrearMultiples(IEnumerable<DetalleDto> detalles);
}

public class DetalleDto 
{ 
    public int DetalleId { get; set; }
    public int PrestamoId { get; set; }
    public int EjemplarId { get; set; }
    public string? ObservacionesSalida { get; set; }
    public string? ObservacionesEntrada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
}
