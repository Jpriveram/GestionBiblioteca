using System.ComponentModel.DataAnnotations;

namespace ServicioLibroEjemplar.Application.Dtos;

public class EjemplarDto
{
    public int EjemplarId { get; set; }
    public int LibroId { get; set; }
    public string CodigoInventario { get; set; } = string.Empty;
    public string? EstadoConservacion { get; set; }
    public bool Disponible { get; set; }
    public bool DadoDeBaja { get; set; }
    public string? MotivoBaja { get; set; }
    public string? Ubicacion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
    public int? UsuarioSesionId { get; set; }
}

public class CreateEjemplarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "LibroId debe ser mayor a cero.")]
    public int LibroId { get; set; }

    [Required]
    [StringLength(50)]
    public string CodigoInventario { get; set; } = string.Empty;

    [StringLength(100)]
    public string? EstadoConservacion { get; set; }

    [StringLength(120)]
    public string? Ubicacion { get; set; }
}

public class UpdateEjemplarDto
{
    [Required]
    [StringLength(50)]
    public string CodigoInventario { get; set; } = string.Empty;

    [StringLength(100)]
    public string? EstadoConservacion { get; set; }

    public bool Disponible { get; set; }
    public bool DadoDeBaja { get; set; }

    [StringLength(200)]
    public string? MotivoBaja { get; set; }

    [StringLength(120)]
    public string? Ubicacion { get; set; }

    public bool Estado { get; set; }
}
