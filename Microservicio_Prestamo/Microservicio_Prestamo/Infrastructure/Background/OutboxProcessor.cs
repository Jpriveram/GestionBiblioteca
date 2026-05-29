using Microservicio_Prestamo.Domain.Ports;
using Microsoft.Extensions.Hosting;

namespace Microservicio_Prestamo.Infrastructure.Background;

public class OutboxProcessor : BackgroundService
{
    private readonly IOutboxRepository _outboxRepo;

    public OutboxProcessor(IOutboxRepository outboxRepo) => _outboxRepo = outboxRepo;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var mensajes = _outboxRepo.GetPending(10);
                foreach (var msg in mensajes)
                {
                    // Futuro: publicar a RabbitMQ aquí
                    System.Diagnostics.Debug.WriteLine($"[Outbox] Evento pendiente: {msg.EventType} - {msg.MessageId}");
                    _outboxRepo.MarkAsProcessed(msg.Id);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Outbox] Error: {ex.Message}");
            }
            await Task.Delay(5000, ct);
        }
    }
}
