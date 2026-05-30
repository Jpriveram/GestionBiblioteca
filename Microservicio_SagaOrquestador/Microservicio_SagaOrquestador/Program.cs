using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient("ServicioLibroEjemplar", c =>
    c.BaseAddress = new Uri("http://localhost:5101/"));

builder.Services.AddHostedService<SagaConsumer>();

var app = builder.Build();
app.Run();

// ── Consumer ──
public class SagaConsumer : BackgroundService
{
    private readonly IHttpClientFactory _httpFactory;
    private IConnection? _connection;

    public SagaConsumer(IHttpClientFactory httpFactory)
    {
        _httpFactory = httpFactory;
    }

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
        var channel = _connection.CreateModel();

        channel.ExchangeDeclare("saga-events", ExchangeType.Topic, durable: true);
        channel.QueueDeclare("prestamo-creado-queue", durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind("prestamo-creado-queue", "saga-events", "prestamo.creado");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (_, ea) =>
        {
            var body = Encoding.UTF8.GetString(ea.Body.ToArray());
            var msg = JsonSerializer.Deserialize<PrestamoMessage>(body);

            if (msg is null) return;

            Console.WriteLine($"[Saga] Recibido Prestamo {msg.PrestamoId}, Ejemplares: {string.Join(",", msg.EjemplarIds)}");

            try
            {
                var http = _httpFactory.CreateClient("ServicioLibroEjemplar");
                var response = await http.PostAsJsonAsync("api/ejemplares/reservar-lote", new
                {
                    EjemplarIds = msg.EjemplarIds,
                    UsuarioSesionId = 0
                });

                if (response.IsSuccessStatusCode)
                {
                    channel.BasicAck(ea.DeliveryTag, false);
                    Console.WriteLine($"[Saga] Prestamo {msg.PrestamoId} COMPLETADO");
                }
                else
                {
                    channel.BasicNack(ea.DeliveryTag, false, true);
                    Console.WriteLine($"[Saga] Prestamo {msg.PrestamoId} FALLIDO — reencolado");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Saga] Error: {ex.Message}");
                channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        channel.BasicConsume("prestamo-creado-queue", false, consumer);
        Console.WriteLine("[SagaOrquestador] Esperando mensajes...");
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _connection?.Dispose();
        base.Dispose();
    }
}

public record PrestamoMessage(Guid CorrelationId, int PrestamoId, List<int> EjemplarIds);
