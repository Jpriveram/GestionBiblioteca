using System.Globalization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace ServicioMultas.Infrastructure.Persistence;

public sealed class FlexibleDateTimeSerializer : SerializerBase<DateTime>
{
    private static readonly string[] SupportedFormats =
    {
        "yyyy-MM-dd HH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ssK",
        "yyyy-MM-ddTHH:mm:ss.fffK",
        "O"
    };

    public override DateTime Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();

        return bsonType switch
        {
            BsonType.DateTime => FromBsonDateTime(context.Reader.ReadDateTime()),
            BsonType.String => ParseString(context.Reader.ReadString()),
            _ => throw new BsonSerializationException($"Tipo BSON no soportado para DateTime: {bsonType}")
        };
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime value)
    {
        var utc = EnsureUtc(value);
        context.Writer.WriteDateTime(BsonUtils.ToMillisecondsSinceEpoch(utc));
    }

    private static DateTime FromBsonDateTime(long milliseconds)
    {
        var dt = BsonUtils.ToDateTimeFromMillisecondsSinceEpoch(milliseconds);
        return EnsureUtc(dt);
    }

    private static DateTime ParseString(string value)
    {
        if (DateTime.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out var parsedExact))
        {
            return EnsureUtc(parsedExact);
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out var parsed))
        {
            return EnsureUtc(parsed);
        }

        throw new BsonSerializationException($"No se pudo convertir la fecha '{value}' a DateTime.");
    }

    private static DateTime EnsureUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime()
        };
    }
}

public sealed class FlexibleNullableDateTimeSerializer : SerializerBase<DateTime?>
{
    private static readonly FlexibleDateTimeSerializer Inner = new();

    public override DateTime? Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var bsonType = context.Reader.GetCurrentBsonType();
        if (bsonType == BsonType.Null)
        {
            context.Reader.ReadNull();
            return null;
        }

        return Inner.Deserialize(context, args);
    }

    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateTime? value)
    {
        if (!value.HasValue)
        {
            context.Writer.WriteNull();
            return;
        }

        Inner.Serialize(context, args, value.Value);
    }
}