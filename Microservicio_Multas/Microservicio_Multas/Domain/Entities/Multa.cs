using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ServicioMultas.Infrastructure.Persistence;

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

    [BsonSerializer(typeof(FlexibleDateTimeSerializer))]
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    [BsonSerializer(typeof(FlexibleNullableDateTimeSerializer))]
    public DateTime? UltimaActualizacion { get; set; }
}
