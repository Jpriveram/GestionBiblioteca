using System.Data;
using ServicioUsuario.Domain.Entities;
using ServicioUsuario.Domain.Ports;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;
using ServicioUsuario.Infrastructure.Configuration;
using System.Collections.Generic;

namespace ServicioUsuario.Infrastructure.Persistence;

public class UsuarioRepository : IRepository<Usuario, int>
{
    private readonly IConfiguration? _configuration;

    public UsuarioRepository()
    {
    }

    public UsuarioRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Usuario? GetByCi(string ci)
    {
        Usuario? usuario = null;
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM usuario WHERE CI = @CI LIMIT 1;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CI", ci);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = MapReaderToUsuario(reader);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener usuario por CI: {ex.Message}");
        }
        return usuario;
    }

    public IEnumerable<Usuario> GetAll()
    {
        var usuarios = new List<Usuario>();
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM usuario;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            usuarios.Add(MapReaderToUsuario(reader));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener todos los usuarios: {ex.Message}");
        }
        return usuarios;
    }

    public Usuario? GetById(int id)
    {
        Usuario? usuario = null;
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM usuario WHERE UsuarioId = @UsuarioId;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UsuarioId", id);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = MapReaderToUsuario(reader);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener usuario por ID: {ex.Message}");
        }
        return usuario;
    }

    public Usuario? GetByNombreUsuario(string nombreUsuario)
    {
        Usuario? usuario = null;
        try
        {
            var normalizedUserName = nombreUsuario?.Trim() ?? string.Empty;

            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT * FROM usuario WHERE LOWER(TRIM(NombreUsuario)) = LOWER(TRIM(@NombreUsuario)) LIMIT 1;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NombreUsuario", normalizedUserName);
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = MapReaderToUsuario(reader);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al obtener usuario por nombre: {ex.Message}");
        }
        return usuario;
    }

    public bool ExisteNombreUsuario(string nombreUsuario)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM usuario WHERE NombreUsuario = @NombreUsuario;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NombreUsuario", nombreUsuario);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar nombre de usuario: {ex.Message}");
            return false;
        }
    }

    public bool ExisteEmail(string email)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM usuario WHERE Email = @Email;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar email: {ex.Message}");
            return false;
        }
    }

    public bool ExisteCi(string ci)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "SELECT COUNT(1) FROM usuario WHERE CI = @CI;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CI", ci);
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al verificar CI: {ex.Message}");
            return false;
        }
    }

    public string JoinCiComp(string ci, string complemento)
    {
        if (string.IsNullOrWhiteSpace(complemento))
            return ci;
        return $"{ci}-{complemento}";
    }

    public void Insert(Usuario entity)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = @"INSERT INTO usuario (Nombres, PrimerApellido, SegundoApellido, Email, NombreUsuario, PasswordHash, Rol, Estado, CI, UsuarioSesionId) 
                                VALUES (@Nombres, @PrimerApellido, @SegundoApellido, @Email, @NombreUsuario, @PasswordHash, @Rol, @Estado, @CI, @UsuarioSesionId);";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Nombres", entity.Nombres);
                    command.Parameters.AddWithValue("@PrimerApellido", entity.PrimerApellido);
                    command.Parameters.AddWithValue("@SegundoApellido", entity.SegundoApellido ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", entity.Email);
                    command.Parameters.AddWithValue("@NombreUsuario", entity.NombreUsuario ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Rol", entity.Rol);
                    command.Parameters.AddWithValue("@Estado", entity.Estado);
                    command.Parameters.AddWithValue("@CI", entity.CI ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UsuarioSesionId", entity.UsuarioSesionId ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                    entity.UsuarioId = Convert.ToInt32(command.LastInsertedId);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al insertar usuario: {ex.Message}");
            throw;
        }
    }

    public void Update(Usuario entity)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = @"UPDATE usuario SET Nombres = @Nombres, PrimerApellido = @PrimerApellido, SegundoApellido = @SegundoApellido, 
                                Email = @Email, NombreUsuario = @NombreUsuario, PasswordHash = @PasswordHash, Rol = @Rol, Estado = @Estado, CI = @CI, UsuarioSesionId = @UsuarioSesionId 
                                WHERE UsuarioId = @UsuarioId;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UsuarioId", entity.UsuarioId);
                    command.Parameters.AddWithValue("@Nombres", entity.Nombres);
                    command.Parameters.AddWithValue("@PrimerApellido", entity.PrimerApellido);
                    command.Parameters.AddWithValue("@SegundoApellido", entity.SegundoApellido ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Email", entity.Email);
                    command.Parameters.AddWithValue("@NombreUsuario", entity.NombreUsuario ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Rol", entity.Rol);
                    command.Parameters.AddWithValue("@Estado", entity.Estado);
                    command.Parameters.AddWithValue("@CI", entity.CI ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UsuarioSesionId", entity.UsuarioSesionId ?? (object)DBNull.Value);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al actualizar usuario: {ex.Message}");
        }
    }

    public void Delete(Usuario entity)
    {
        try
        {
            using (var connection = (MySqlConnection)ConfigurationSingleton.Instancia.GetConnection())
            {
                connection.Open();
                string query = "DELETE FROM usuario WHERE UsuarioId = @UsuarioId;";
                using (MySqlCommand command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UsuarioId", entity.UsuarioId);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al eliminar usuario: {ex.Message}");
        }
    }

    public void SaveChanges()
    {
        // En MySQL con MySqlCommand, los cambios se aplican automáticamente con ExecuteNonQuery()
        // Este método es un placeholder para mantener compatibilidad con la interfaz IRepository
    }

    private Usuario MapReaderToUsuario(MySqlDataReader reader)
    {
        return new Usuario
        {
            UsuarioId = reader.GetInt32("UsuarioId"),
            Nombres = reader.GetString("Nombres"),
            PrimerApellido = reader.GetString("PrimerApellido"),
            SegundoApellido = reader.IsDBNull("SegundoApellido") ? null : reader.GetString("SegundoApellido"),
            Email = reader.GetString("Email"),
            NombreUsuario = reader.IsDBNull("NombreUsuario") ? null : reader.GetString("NombreUsuario"),
            PasswordHash = reader.IsDBNull("PasswordHash") ? null : reader.GetString("PasswordHash"),
            Rol = reader.GetString("Rol"),
            Estado = reader.GetBoolean("Estado"),
            CI = reader.IsDBNull("CI") ? null : reader.GetString("CI"),
            UsuarioSesionId = reader.IsDBNull("UsuarioSesionId") ? null : reader.GetInt32("UsuarioSesionId")
        };
    }
}
