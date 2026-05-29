using Microservicio_Prestamo.Domain.Entities;
using Microservicio_Prestamo.Domain.Ports;
using Microservicio_Prestamo.Infrastructure.Configuration;
using MySql.Data.MySqlClient;

namespace Microservicio_Prestamo.Infrastructure.Persistence;

public class OutboxRepository : IOutboxRepository
{
    public void Insert(OutboxMessage msg)
    {
        using var c = ConfigurationSingleton.Instancia.GetConnection();
        c.Open();
        using var cmd = new MySqlCommand(
            "INSERT INTO outbox_messages (MessageId, EventType, Payload, CreatedAt, Processed) VALUES (@Mid, @Type, @Payload, @Created, false);", c);
        cmd.Parameters.AddWithValue("@Mid", msg.MessageId);
        cmd.Parameters.AddWithValue("@Type", msg.EventType);
        cmd.Parameters.AddWithValue("@Payload", msg.Payload);
        cmd.Parameters.AddWithValue("@Created", msg.CreatedAt);
        cmd.ExecuteNonQuery();
    }

    public IEnumerable<OutboxMessage> GetPending(int limit)
    {
        var list = new List<OutboxMessage>();
        using var c = ConfigurationSingleton.Instancia.GetConnection();
        c.Open();
        using var cmd = new MySqlCommand(
            "SELECT Id, MessageId, EventType, Payload, CreatedAt FROM outbox_messages WHERE Processed = false ORDER BY Id LIMIT @Limit;", c);
        cmd.Parameters.AddWithValue("@Limit", limit);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(new OutboxMessage
        {
            Id = r.GetInt64("Id"),
            MessageId = r.GetString("MessageId"),
            EventType = r.GetString("EventType"),
            Payload = r.GetString("Payload"),
            CreatedAt = r.GetDateTime("CreatedAt")
        });
        return list;
    }

    public void MarkAsProcessed(long id)
    {
        using var c = ConfigurationSingleton.Instancia.GetConnection();
        c.Open();
        using var cmd = new MySqlCommand("UPDATE outbox_messages SET Processed = true WHERE Id = @Id;", c);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }
}
