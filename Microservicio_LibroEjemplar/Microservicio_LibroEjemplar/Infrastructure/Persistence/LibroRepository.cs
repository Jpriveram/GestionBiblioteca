using ServicioLibroEjemplar.Domain.Entities;
using ServicioLibroEjemplar.Domain.Ports;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ServicioLibroEjemplar.Infrastructure.Configuration;

namespace ServicioLibroEjemplar.Infrastructure.Persistence;

public class LibroRepository : IRepository<Libro, int>
{
    public LibroRepository()
    {
    }

    public LibroRepository(IConfiguration configuration)
    {
        _ = configuration;
    }

    public Libro? GetById(int id)
    {
        Libro? libro = null;
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Libro WHERE LibroId = @LibroId LIMIT 1;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LibroId", id);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            libro = MapReaderToLibro(reader);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener libro por ID: {ex.Message}");
        }
        return libro;
    }

    public IEnumerable<Libro> GetAll()
    {
        List<Libro> libros = new();
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Libro WHERE Estado = true ORDER BY FechaRegistro DESC;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            libros.Add(MapReaderToLibro(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener libros: {ex.Message}");
        }
        return libros;
    }

    public Libro? GetByIsbn(string isbn)
    {
        Libro? libro = null;
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Libro WHERE ISBN = @ISBN LIMIT 1;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ISBN", isbn);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            libro = MapReaderToLibro(reader);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener libro por ISBN: {ex.Message}");
        }
        return libro;
    }

    public IEnumerable<Libro> GetByAutorId(int autorId)
    {
        List<Libro> libros = new();
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM Libro WHERE AutorId = @AutorId AND Estado = true ORDER BY FechaRegistro DESC;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AutorId", autorId);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            libros.Add(MapReaderToLibro(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener libros por AutorId: {ex.Message}");
        }
        return libros;
    }

    public void Insert(Libro entity)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO Libro 
                    (AutorId, Titulo, ISBN, Editorial, Genero, Edicion, AñoPublicacion, NumeroPaginas, Idioma, PaisPublicacion, Descripcion, Estado, FechaRegistro, UsuarioSesionId) 
                    VALUES 
                    (@AutorId, @Titulo, @ISBN, @Editorial, @Genero, @Edicion, @AñoPublicacion, @NumeroPaginas, @Idioma, @PaisPublicacion, @Descripcion, @Estado, @FechaRegistro, @UsuarioSesionId);";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AutorId", entity.AutorId);
                    command.Parameters.AddWithValue("@Titulo", entity.Titulo);
                    command.Parameters.AddWithValue("@ISBN", entity.ISBN ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Editorial", entity.Editorial ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Genero", entity.Genero ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Edicion", entity.Edicion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AñoPublicacion", entity.AñoPublicacion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@NumeroPaginas", entity.NumeroPaginas ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Idioma", entity.Idioma ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PaisPublicacion", entity.PaisPublicacion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Descripcion", entity.Descripcion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Estado", entity.Estado);
                    command.Parameters.AddWithValue("@FechaRegistro", entity.FechaRegistro);
                    command.Parameters.AddWithValue("@UsuarioSesionId", entity.UsuarioSesionId ?? (object)DBNull.Value);
                    
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al insertar libro: {ex.Message}");
        }
    }

    public void Update(Libro entity)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = @"UPDATE Libro SET 
                    AutorId = @AutorId, Titulo = @Titulo, ISBN = @ISBN, Editorial = @Editorial, Genero = @Genero, 
                    Edicion = @Edicion, AñoPublicacion = @AñoPublicacion, NumeroPaginas = @NumeroPaginas, 
                    Idioma = @Idioma, PaisPublicacion = @PaisPublicacion, Descripcion = @Descripcion, Estado = @Estado, 
                    UltimaActualizacion = @UltimaActualizacion, UsuarioSesionId = @UsuarioSesionId 
                    WHERE LibroId = @LibroId;";
                
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LibroId", entity.LibroId);
                    command.Parameters.AddWithValue("@AutorId", entity.AutorId);
                    command.Parameters.AddWithValue("@Titulo", entity.Titulo);
                    command.Parameters.AddWithValue("@ISBN", entity.ISBN ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Editorial", entity.Editorial ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Genero", entity.Genero ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Edicion", entity.Edicion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@AñoPublicacion", entity.AñoPublicacion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@NumeroPaginas", entity.NumeroPaginas ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Idioma", entity.Idioma ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PaisPublicacion", entity.PaisPublicacion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Descripcion", entity.Descripcion ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Estado", entity.Estado);
                    command.Parameters.AddWithValue("@UltimaActualizacion", entity.UltimaActualizacion ?? DateTime.Now);
                    command.Parameters.AddWithValue("@UsuarioSesionId", entity.UsuarioSesionId ?? (object)DBNull.Value);
                    
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar libro: {ex.Message}");
        }
    }

    public void Delete(Libro entity)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM Libro WHERE LibroId = @LibroId;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LibroId", entity.LibroId);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar libro: {ex.Message}");
        }
    }

    public void SaveChanges()
    {
        // Not needed for ADO.NET, but kept for interface contract
    }

    private static Libro MapReaderToLibro(MySqlDataReader reader)
    {
        return new Libro
        {
            LibroId = reader.GetInt32("LibroId"),
            AutorId = reader.GetInt32("AutorId"),
            Titulo = reader.GetString("Titulo"),
            ISBN = GetNullableString(reader, "ISBN"),
            Editorial = GetNullableString(reader, "Editorial"),
            Genero = GetNullableString(reader, "Genero"),
            Edicion = GetNullableString(reader, "Edicion"),
            AñoPublicacion = GetNullableInt32(reader, "AñoPublicacion"),
            NumeroPaginas = GetNullableInt32(reader, "NumeroPaginas"),
            Idioma = GetNullableString(reader, "Idioma"),
            PaisPublicacion = GetNullableString(reader, "PaisPublicacion"),
            Descripcion = GetNullableString(reader, "Descripcion"),
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
