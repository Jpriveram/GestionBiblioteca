namespace ServicioMultas.Application.Dtos;

public class MultaDto
{
    public string? Id { get; set; }
    public int UsuarioId { get; set; }
    public int? PrestamoId { get; set; }
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public int? UsuarioSesionId { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
}

public class CreateMultaDto
{
    public int UsuarioId { get; set; }
    public int? PrestamoId { get; set; }
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public int? UsuarioSesionId { get; set; }
}

public class UpdateMultaDto
{
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public bool Estado { get; set; }
    public int? UsuarioSesionId { get; set; }
}
