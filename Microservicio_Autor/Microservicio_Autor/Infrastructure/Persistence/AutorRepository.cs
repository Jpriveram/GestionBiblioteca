using Microsoft.Data.SqlClient;
using Microservicio_Autor.Domain.Entities;
using Microservicio_Autor.Domain.Ports;
using Microservicio_Autor.Infrastructure.Configuration;

namespace Microservicio_Autor.Infrastructure.Persistence;

public class AutorRepository : IRepository<Autor, int>
{
    public Autor? GetById(int id)
    {
        Autor? autor = null;

        try
        {
            using var connection = ConfigurationSingleton.Instancia.GetConnection();
            connection.Open();

            const string query = "SELECT TOP 1 * FROM Autor WHERE AutorId = @AutorId;";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AutorId", id);

            using var reader = command.ExecuteReader();
            if (reader.Read())
                autor = MapReaderToAutor(reader);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener autor por ID: {ex.Message}");
        }

        return autor;
    }

    public IEnumerable<Autor> GetAll()
    {
        List<Autor> autores = new();

        try
        {
            using var connection = ConfigurationSingleton.Instancia.GetConnection();
            connection.Open();

            const string query = """
                SELECT * FROM Autor
                WHERE Estado = 1
                ORDER BY PrimerApellido ASC, SegundoApellido ASC, Nombres ASC;
                """;

            using var command = new SqlCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
                autores.Add(MapReaderToAutor(reader));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener autores: {ex.Message}");
        }

        return autores;
    }

    public void Insert(Autor entity)
    {
        try
        {
            using var connection = ConfigurationSingleton.Instancia.GetConnection();
            connection.Open();

            const string query = """
                INSERT INTO Autor
                (Nombres, PrimerApellido, SegundoApellido, Nacionalidad, FechaNacimiento, Estado, FechaRegistro, UsuarioSesionId)
                OUTPUT INSERTED.AutorId
                VALUES
                (@Nombres, @PrimerApellido, @SegundoApellido, @Nacionalidad, @FechaNacimiento, @Estado, @FechaRegistro, @UsuarioSesionId);
                """;

            using var command = new SqlCommand(query, connection);
            AddCommonParameters(command, entity);

            var insertedId = command.ExecuteScalar();
            if (insertedId is not null)
                entity.AutorId = Convert.ToInt32(insertedId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al insertar autor: {ex.Message}");
        }
    }

    public void Update(Autor entity)
    {
        try
        {
            using var connection = ConfigurationSingleton.Instancia.GetConnection();
            connection.Open();

            const string query = """
                UPDATE Autor SET
                    Nombres = @Nombres,
                    PrimerApellido = @PrimerApellido,
                    SegundoApellido = @SegundoApellido,
                    Nacionalidad = @Nacionalidad,
                    FechaNacimiento = @FechaNacimiento,
                    Estado = @Estado,
                    UltimaActualizacion = @UltimaActualizacion,
                    UsuarioSesionId = @UsuarioSesionId
                WHERE AutorId = @AutorId;
                """;

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AutorId", entity.AutorId);
            AddCommonParameters(command, entity);
            command.Parameters.AddWithValue("@UltimaActualizacion", entity.UltimaActualizacion ?? DateTime.Now);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar autor: {ex.Message}");
        }
    }

    public void Delete(Autor entity)
    {
        try
        {
            using var connection = ConfigurationSingleton.Instancia.GetConnection();
            connection.Open();

            const string query = "DELETE FROM Autor WHERE AutorId = @AutorId;";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@AutorId", entity.AutorId);

            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar autor: {ex.Message}");
        }
    }

    public void SaveChanges()
    {
    }

    private static void AddCommonParameters(SqlCommand command, Autor entity)
    {
        command.Parameters.AddWithValue("@Nombres", entity.Nombres);
        command.Parameters.AddWithValue("@PrimerApellido", entity.PrimerApellido ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@SegundoApellido", entity.SegundoApellido ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Nacionalidad", entity.Nacionalidad ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@FechaNacimiento", entity.FechaNacimiento ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Estado", entity.Estado);
        command.Parameters.AddWithValue("@FechaRegistro", entity.FechaRegistro);
        command.Parameters.AddWithValue("@UsuarioSesionId", entity.UsuarioSesionId ?? (object)DBNull.Value);
    }

    private static Autor MapReaderToAutor(SqlDataReader reader)
    {
        return new Autor
        {
            AutorId = reader.GetInt32(reader.GetOrdinal("AutorId")),
            Nombres = reader.GetString(reader.GetOrdinal("Nombres")),
            PrimerApellido = GetNullableString(reader, "PrimerApellido"),
            SegundoApellido = GetNullableString(reader, "SegundoApellido"),
            Nacionalidad = GetNullableString(reader, "Nacionalidad"),
            FechaNacimiento = GetNullableDateTime(reader, "FechaNacimiento"),
            Estado = reader.GetBoolean(reader.GetOrdinal("Estado")),
            FechaRegistro = reader.GetDateTime(reader.GetOrdinal("FechaRegistro")),
            UltimaActualizacion = GetNullableDateTime(reader, "UltimaActualizacion"),
            UsuarioSesionId = GetNullableInt32(reader, "UsuarioSesionId")
        };
    }

    private static string? GetNullableString(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    private static int? GetNullableInt32(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    private static DateTime? GetNullableDateTime(SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }
}