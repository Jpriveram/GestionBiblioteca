using System.Text;
using System.Text.Json;
using Microservicio_Prestamo.Domain.Ports;
using RabbitMQ.Client;

namespace Microservicio_Prestamo.Infrastructure.Background;

public class OutboxProcessor : BackgroundService
{
    private readonly IOutboxRepository _outboxRepo;
    private IConnection? _connection;
    private IModel? _channel;

    public OutboxProcessor(IOutboxRepository outboxRepo)
    {
        _outboxRepo = outboxRepo;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (_channel is null)
            {
                try
                {
                    var factory = new ConnectionFactory { HostName = "localhost" };
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.ExchangeDeclare("saga-events", ExchangeType.Topic, durable: true);
                    System.Diagnostics.Debug.WriteLine("[Outbox] Conectado a RabbitMQ");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Outbox] RabbitMQ no disponible: {ex.Message}");
                    await Task.Delay(10000, ct);
                    continue;
                }
            }

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
                        var props = _channel.CreateBasicProperties();
                        props.MessageId = msg.MessageId;
                        props.Persistent = true;

                        var routingKey = msg.EventType == "PrestamoAnulado" ? "prestamo.anulado" : "prestamo.creado";
                        _channel.BasicPublish("saga-events", routingKey, props, body);
                        _outboxRepo.MarkAsProcessed(msg.Id);
                        System.Diagnostics.Debug.WriteLine($"[Outbox] Publicado: {msg.MessageId}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[Outbox] Error: {ex.Message}");
                        _channel?.Dispose();
                        _channel = null;
                        break;
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
        _channel?.Dispose();
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
