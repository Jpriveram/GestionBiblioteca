using System.ComponentModel.DataAnnotations;

namespace ServicioLibroEjemplar.Application.Dtos;

public class LibroDto
{
    public int LibroId { get; set; }
    public int AutorId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? ISBN { get; set; }
    public string? Editorial { get; set; }
    public string? Genero { get; set; }
    public string? Edicion { get; set; }
    public int? AñoPublicacion { get; set; }
    public int? NumeroPaginas { get; set; }
    public string? Idioma { get; set; }
    public string? PaisPublicacion { get; set; }
    public string? Descripcion { get; set; }
    public bool Estado { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimaActualizacion { get; set; }
}

public class CreateLibroDto
{
    [Range(1, int.MaxValue, ErrorMessage = "AutorId debe ser mayor a cero.")]
    public int AutorId { get; set; }

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(20)]
    public string? ISBN { get; set; }

    [StringLength(120)]
    public string? Editorial { get; set; }

    [StringLength(80)]
    public string? Genero { get; set; }

    [StringLength(40)]
    public string? Edicion { get; set; }

    [Range(1000, 9999, ErrorMessage = "AñoPublicacion debe tener un valor valido.")]
    public int? AñoPublicacion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "NumeroPaginas debe ser mayor a cero.")]
    public int? NumeroPaginas { get; set; }

    [StringLength(40)]
    public string? Idioma { get; set; }

    [StringLength(80)]
    public string? PaisPublicacion { get; set; }

    [StringLength(500)]
    public string? Descripcion { get; set; }
}

public class UpdateLibroDto
{
    [Range(1, int.MaxValue, ErrorMessage = "AutorId debe ser mayor a cero.")]
    public int AutorId { get; set; }

    [Required]
    [StringLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [StringLength(20)]
    public string? ISBN { get; set; }

    [StringLength(120)]
    public string? Editorial { get; set; }

    [StringLength(80)]
    public string? Genero { get; set; }

    [StringLength(40)]
    public string? Edicion { get; set; }

    [Range(1000, 9999, ErrorMessage = "AñoPublicacion debe tener un valor valido.")]
    public int? AñoPublicacion { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "NumeroPaginas debe ser mayor a cero.")]
    public int? NumeroPaginas { get; set; }

    [StringLength(40)]
    public string? Idioma { get; set; }

    [StringLength(80)]
    public string? PaisPublicacion { get; set; }

    [StringLength(500)]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; }
}
