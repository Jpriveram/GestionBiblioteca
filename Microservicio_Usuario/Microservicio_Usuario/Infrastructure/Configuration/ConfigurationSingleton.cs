using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace ServicioUsuario.Infrastructure.Configuration;

public class ConfigurationSingleton
{
    private static readonly ConfigurationSingleton _instancia = new();
    private string _connectionString = "Server=localhost;Database=gestion_biblioteca;User Id=root;Password=;";

    public static ConfigurationSingleton Instancia => _instancia;

    public static void Initialize(IConfiguration configuration)
    {
        _instancia._connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=gestion_biblioteca;User Id=root;Password=;";
    }

    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
