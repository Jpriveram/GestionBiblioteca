using System.Data;
using System.Text.Json;
using Microservicio_Prestamo.Domain.Entities;
using Microservicio_Prestamo.Domain.Ports;
using Microservicio_Prestamo.Infrastructure.Configuration;
using MySql.Data.MySqlClient;

namespace Microservicio_Prestamo.Infrastructure.Persistence;

public class PrestamoRepository : IPrestamoRepository
{
    public PrestamoRepository() { }

    public IEnumerable<Prestamo> GetAll() => GetAll(true);

    public IEnumerable<Prestamo> GetAll(bool activos)
    {
        var prestamos = new List<Prestamo>();
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();
        string query = @"SELECT * FROM prestamo " + (activos ? "WHERE Estado = 1 " : "") + "ORDER BY FechaPrestamo DESC;";
        using var cmd = new MySqlCommand(query, connection);
        using var reader = cmd.ExecuteReader();
        while (reader.Read()) prestamos.Add(MapPrestamo(reader));
        return prestamos;
    }

    public Prestamo? GetById(int id)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();
        using var cmd = new MySqlCommand("SELECT * FROM prestamo WHERE PrestamoId = @Id LIMIT 1;", connection);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapPrestamo(reader) : null;
    }

    public int CountActivosByLector(int lectorId)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();
        using var cmd = new MySqlCommand("SELECT COUNT(1) FROM prestamo WHERE LectorId = @Id AND Estado = 1;", connection);
        cmd.Parameters.AddWithValue("@Id", lectorId);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Insert(Prestamo entity) => InsertPrestamo(entity);

    void IRepository<Prestamo, int>.Insert(Prestamo entity) => InsertPrestamo(entity);

    private int InsertPrestamo(Prestamo p)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();
        string query = @"INSERT INTO prestamo (LectorId, FechaPrestamo, FechaDevolucionEsperada, ObservacionesSalida, Estado, UsuarioSesionId, FechaRegistro)
                         VALUES (@LectorId, @FechaPrestamo, @FechaDevolucionEsperada, @Obs, @Estado, @Usr, NOW()); SELECT LAST_INSERT_ID();";
        using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@LectorId", p.LectorId);
        cmd.Parameters.AddWithValue("@FechaPrestamo", p.FechaPrestamo);
        cmd.Parameters.AddWithValue("@FechaDevolucionEsperada", p.FechaDevolucionEsperada);
        cmd.Parameters.AddWithValue("@Obs", p.ObservacionesSalida ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Estado", p.Estado);
        cmd.Parameters.AddWithValue("@Usr", p.UsuarioSesionId ?? (object)DBNull.Value);
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public void Update(Prestamo p)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();
        string query = @"UPDATE prestamo SET Estado = @Estado, ObservacionesEntrada = @ObsEntrada,
                         UsuarioSesionId = @Usr, UltimaActualizacion = NOW() WHERE PrestamoId = @Id;";
        using var cmd = new MySqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@Id", p.PrestamoId);
        cmd.Parameters.AddWithValue("@Estado", p.Estado);
        cmd.Parameters.AddWithValue("@ObsEntrada", p.ObservacionesEntrada ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Usr", p.UsuarioSesionId ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public void Delete(Prestamo p)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();
        using var cmd = new MySqlCommand("UPDATE prestamo SET Estado = 0, UsuarioSesionId = @Usr, UltimaActualizacion = NOW() WHERE PrestamoId = @Id;", connection);
        cmd.Parameters.AddWithValue("@Id", p.PrestamoId);
        cmd.Parameters.AddWithValue("@Usr", p.UsuarioSesionId ?? (object)DBNull.Value);
        cmd.ExecuteNonQuery();
    }

    public int CrearPrestamoTransaccional(Prestamo prestamo, IEnumerable<Detalle> detalles, int? usuarioSesionId)
    {
        using var connection = ConfigurationSingleton.Instancia.GetConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            // 1. Insertar préstamo
            string qP = @"INSERT INTO prestamo (LectorId, FechaPrestamo, FechaDevolucionEsperada, ObservacionesSalida, Estado, UsuarioSesionId, FechaRegistro)
                          VALUES (@LectorId, @Fp, @Fde, @Obs, 1, @Usr, NOW()); SELECT LAST_INSERT_ID();";
            using var cmdP = new MySqlCommand(qP, connection, transaction);
            cmdP.Parameters.AddWithValue("@LectorId", prestamo.LectorId);
            cmdP.Parameters.AddWithValue("@Fp", prestamo.FechaPrestamo);
            cmdP.Parameters.AddWithValue("@Fde", prestamo.FechaDevolucionEsperada);
            cmdP.Parameters.AddWithValue("@Obs", prestamo.ObservacionesSalida ?? (object)DBNull.Value);
            cmdP.Parameters.AddWithValue("@Usr", usuarioSesionId ?? (object)DBNull.Value);
            int prestamoId = Convert.ToInt32(cmdP.ExecuteScalar());

            // 2. Insertar detalles
            foreach (var d in detalles)
            {
                string qD = @"INSERT INTO detalle (PrestamoId, EjemplarId, EstadoDetalle, ObservacionesSalida, UsuarioSesionId, FechaRegistro)
                              VALUES (@Pid, @Eid, 1, @Obs, @Usr, NOW());";
                using var cmdD = new MySqlCommand(qD, connection, transaction);
                cmdD.Parameters.AddWithValue("@Pid", prestamoId);
                cmdD.Parameters.AddWithValue("@Eid", d.EjemplarId);
                cmdD.Parameters.AddWithValue("@Obs", d.ObservacionesSalida ?? (object)DBNull.Value);
                cmdD.Parameters.AddWithValue("@Usr", usuarioSesionId ?? (object)DBNull.Value);
                cmdD.ExecuteNonQuery();
            }

            // 3. Insertar outbox
            var outboxMsg = new OutboxMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                EventType = "PrestamoCreado",
                Payload = JsonSerializer.Serialize(new { 
                    PrestamoId = prestamoId, 
                    LectorId = prestamo.LectorId,
                    EjemplarIds = detalles.Select(d => d.EjemplarId).ToList()
                }),
                CreatedAt = DateTime.UtcNow
            };
            string qO = @"INSERT INTO outbox_messages (MessageId, EventType, Payload, CreatedAt, Processed)
                          VALUES (@Mid, @Type, @Payload, @Created, false);";
            using var cmdO = new MySqlCommand(qO, connection, transaction);
            cmdO.Parameters.AddWithValue("@Mid", outboxMsg.MessageId);
            cmdO.Parameters.AddWithValue("@Type", outboxMsg.EventType);
            cmdO.Parameters.AddWithValue("@Payload", outboxMsg.Payload);
            cmdO.Parameters.AddWithValue("@Created", outboxMsg.CreatedAt);
            cmdO.ExecuteNonQuery();

            transaction.Commit();
            return prestamoId;
        }
        catch { transaction.Rollback(); throw; }
    }

    private static Prestamo MapPrestamo(MySqlDataReader r) => new()
    {
        PrestamoId = r.GetInt32("PrestamoId"),
        LectorId = r.GetInt32("LectorId"),
        FechaPrestamo = r.GetDateTime("FechaPrestamo"),
        FechaDevolucionEsperada = r.GetDateTime("FechaDevolucionEsperada"),
        FechaDevolucionReal = r.IsDBNull("FechaDevolucionReal") ? null : r.GetDateTime("FechaDevolucionReal"),
        ObservacionesSalida = r.IsDBNull("ObservacionesSalida") ? null : r.GetString("ObservacionesSalida"),
        ObservacionesEntrada = r.IsDBNull("ObservacionesEntrada") ? null : r.GetString("ObservacionesEntrada"),
        Estado = r.GetInt32("Estado"),
        UsuarioSesionId = r.IsDBNull("UsuarioSesionId") ? null : r.GetInt32("UsuarioSesionId"),
        FechaRegistro = r.GetDateTime("FechaRegistro"),
        UltimaActualizacion = r.IsDBNull("UltimaActualizacion") ? null : r.GetDateTime("UltimaActualizacion")
    };
}
