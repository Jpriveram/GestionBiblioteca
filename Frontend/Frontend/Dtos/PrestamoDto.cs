using System;

namespace Frontend.Dtos;

public class PrestamoDto
{
    public int PrestamoId { get; set; }
    public int LectorId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    public string? ObservacionesSalida { get; set; }
    public string? ObservacionesEntrada { get; set; }
    public int Estado { get; set; }
    public int? UsuarioSesionId { get; set; }

    // Additional fields for grid display
    public string UsuarioNombre { get; set; } = string.Empty;
}
