using System.Text;
using System.Text.Json;
using Microservicio_Prestamo.Domain.Ports;
using RabbitMQ.Client;

namespace Microservicio_Prestamo.Infrastructure.Background;

public class OutboxProcessor : BackgroundService
{
    private readonly IOutboxRepository _outboxRepo;
    private readonly IConnection _connection;

    public OutboxProcessor(IOutboxRepository outboxRepo)
    {
        _outboxRepo = outboxRepo;
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var channel = _connection.CreateModel();
        channel.ExchangeDeclare("saga-events", ExchangeType.Topic, durable: true);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                var mensajes = _outboxRepo.GetPending(10);
                foreach (var msg in mensajes)
                {
                    try
                    {
                        var payload = new
                        {
                            CorrelationId = msg.MessageId,
                            PrestamoId = ExtractPrestamoId(msg.Payload),
                            EjemplarIds = ExtractEjemplarIds(msg.Payload)
                        };

                        var json = JsonSerializer.Serialize(payload);
                        var body = Encoding.UTF8.GetBytes(json);

                        var props = channel.CreateBasicProperties();
                        props.MessageId = msg.MessageId;
                        props.Persistent = true;

                        channel.BasicPublish("saga-events", "prestamo.creado", props, body);
                        _outboxRepo.MarkAsProcessed(msg.Id);
                        System.Diagnostics.Debug.WriteLine($"[Outbox] Publicado: {msg.MessageId}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Outbox] Error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Outbox] Error: {ex.Message}");
            }
            await Task.Delay(5000, ct);
        }
    }

    public override void Dispose()
    {
        _connection?.Dispose();
        base.Dispose();
    }

    private static int ExtractPrestamoId(string payload)
    {
        try { using var doc = JsonDocument.Parse(payload); if (doc.RootElement.TryGetProperty("PrestamoId", out var el)) return el.GetInt32(); }
        catch { }
        return 0;
    }

    private static List<int> ExtractEjemplarIds(string payload)
    {
        try { using var doc = JsonDocument.Parse(payload); if (doc.RootElement.TryGetProperty("EjemplarIds", out var arr)) return arr.EnumerateArray().Select(e => e.GetInt32()).ToList(); }
        catch { }
        return new();
    }
}
