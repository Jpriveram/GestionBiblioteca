using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ServicioMultas.Domain.Entities;

public class Multa
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int UsuarioId { get; set; }
    public int? PrestamoId { get; set; }
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public bool Estado { get; set; } = true;
    public int? UsuarioSesionId { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    public DateTime? UltimaActualizacion { get; set; }
}
