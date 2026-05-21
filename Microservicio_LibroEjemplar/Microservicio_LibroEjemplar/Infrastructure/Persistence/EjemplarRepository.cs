using ServicioLibroEjemplar.Domain.Entities;
using ServicioLibroEjemplar.Domain.Ports;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ServicioLibroEjemplar.Infrastructure.Configuration;

namespace ServicioLibroEjemplar.Infrastructure.Persistence;

public class EjemplarRepository : IRepository<Ejemplar, int>
{
    public EjemplarRepository()
    {
    }

    public EjemplarRepository(IConfiguration configuration)
    {
        _ = configuration;
    }

    public Ejemplar? GetById(int id)
    {
        Ejemplar? ejemplar = null;
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM ejemplar WHERE EjemplarId = @EjemplarId LIMIT 1;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EjemplarId", id);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ejemplar = MapReaderToEjemplar(reader);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener ejemplar por ID: {ex.Message}");
        }
        return ejemplar;
    }

    public IEnumerable<Ejemplar> GetAll()
    {
        List<Ejemplar> ejemplares = new();
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM ejemplar WHERE Estado = true ORDER BY FechaRegistro DESC;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ejemplares.Add(MapReaderToEjemplar(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener ejemplares: {ex.Message}");
        }
        return ejemplares;
    }

    public Ejemplar? GetByCodigoInventario(string codigoInventario)
    {
        Ejemplar? ejemplar = null;
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM ejemplar WHERE CodigoInventario = @CodigoInventario LIMIT 1;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CodigoInventario", codigoInventario);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            ejemplar = MapReaderToEjemplar(reader);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener ejemplar por código: {ex.Message}");
        }
        return ejemplar;
    }

    public IEnumerable<Ejemplar> GetByLibroId(int libroId)
    {
        List<Ejemplar> ejemplares = new();
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM ejemplar WHERE LibroId = @LibroId AND Estado = true ORDER BY FechaRegistro DESC;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LibroId", libroId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ejemplares.Add(MapReaderToEjemplar(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener ejemplares por LibroId: {ex.Message}");
        }
        return ejemplares;
    }

    public void Insert(Ejemplar entity)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO ejemplar 
                    (LibroId, CodigoInventario, EstadoConservacion, Disponible, DadoDeBaja, MotivoBaja, Ubicacion, Estado, FechaRegistro, UsuarioSesionId) 
                    VALUES 
                    (@LibroId, @CodigoInventario, @EstadoConservacion, @Disponible, @DadoDeBaja, @MotivoBaja, @Ubicacion, @Estado, @FechaRegistro, @UsuarioSesionId);";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LibroId", entity.LibroId);
                    command.Parameters.AddWithValue("@CodigoInventario", entity.CodigoInventario);
                    command.Parameters.AddWithValue("@EstadoConservacion", entity.EstadoConservacion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Disponible", entity.Disponible);
                    command.Parameters.AddWithValue("@DadoDeBaja", entity.DadoDeBaja);
                    command.Parameters.AddWithValue("@MotivoBaja", entity.MotivoBaja ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Ubicacion", entity.Ubicacion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Estado", entity.Estado);
                    command.Parameters.AddWithValue("@FechaRegistro", entity.FechaRegistro);
                    command.Parameters.AddWithValue("@UsuarioSesionId", entity.UsuarioSesionId ?? (object)DBNull.Value);
                    
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al insertar ejemplar: {ex.Message}");
        }
    }

    public void Update(Ejemplar entity)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = @"UPDATE ejemplar SET 
                    LibroId = @LibroId, CodigoInventario = @CodigoInventario, EstadoConservacion = @EstadoConservacion, 
                    Disponible = @Disponible, DadoDeBaja = @DadoDeBaja, MotivoBaja = @MotivoBaja, 
                    Ubicacion = @Ubicacion, Estado = @Estado, UltimaActualizacion = @UltimaActualizacion, 
                    UsuarioSesionId = @UsuarioSesionId 
                    WHERE EjemplarId = @EjemplarId;";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EjemplarId", entity.EjemplarId);
                    command.Parameters.AddWithValue("@LibroId", entity.LibroId);
                    command.Parameters.AddWithValue("@CodigoInventario", entity.CodigoInventario);
                    command.Parameters.AddWithValue("@EstadoConservacion", entity.EstadoConservacion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Disponible", entity.Disponible);
                    command.Parameters.AddWithValue("@DadoDeBaja", entity.DadoDeBaja);
                    command.Parameters.AddWithValue("@MotivoBaja", entity.MotivoBaja ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Ubicacion", entity.Ubicacion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Estado", entity.Estado);
                    command.Parameters.AddWithValue("@UltimaActualizacion", entity.UltimaActualizacion ?? DateTime.UtcNow);
                    command.Parameters.AddWithValue("@UsuarioSesionId", entity.UsuarioSesionId ?? (object)DBNull.Value);
                    
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar ejemplar: {ex.Message}");
        }
    }

    public void Delete(Ejemplar entity)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM ejemplar WHERE EjemplarId = @EjemplarId;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@EjemplarId", entity.EjemplarId);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar ejemplar: {ex.Message}");
        }
    }

    public void SaveChanges()
    {
        // Not needed for ADO.NET, but kept for interface contract
    }

    private static Ejemplar MapReaderToEjemplar(MySqlDataReader reader)
    {
        return new Ejemplar
        {
            EjemplarId = reader.GetInt32("EjemplarId"),
            LibroId = reader.GetInt32("LibroId"),
            CodigoInventario = reader.GetString("CodigoInventario"),
            EstadoConservacion = GetNullableString(reader, "EstadoConservacion"),
            Disponible = reader.GetBoolean("Disponible"),
            DadoDeBaja = reader.GetBoolean("DadoDeBaja"),
            MotivoBaja = GetNullableString(reader, "MotivoBaja"),
            Ubicacion = GetNullableString(reader, "Ubicacion"),
            Estado = reader.GetBoolean("Estado"),
            FechaRegistro = reader.GetDateTime("FechaRegistro"),
            UltimaActualizacion = GetNullableDateTime(reader, "UltimaActualizacion"),
            UsuarioSesionId = GetNullableInt32(reader, "UsuarioSesionId")
        };
    }

    private static string? GetNullableString(MySqlDataReader reader, string columnName)
        => reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetString(columnName);

    private static int? GetNullableInt32(MySqlDataReader reader, string columnName)
        => reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetInt32(columnName);

    private static DateTime? GetNullableDateTime(MySqlDataReader reader, string columnName)
        => reader.IsDBNull(reader.GetOrdinal(columnName)) ? null : reader.GetDateTime(columnName);
}
