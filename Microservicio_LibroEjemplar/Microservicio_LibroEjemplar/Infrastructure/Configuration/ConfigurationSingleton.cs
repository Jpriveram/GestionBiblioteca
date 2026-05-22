using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace ServicioLibroEjemplar.Infrastructure.Configuration;

public class ConfigurationSingleton
{
    private static readonly ConfigurationSingleton _instancia = new();
    private string _connectionString = "Server=localhost;Database=libro_ejemplar_db;User Id=root;Password=root;";

    public static ConfigurationSingleton Instancia => _instancia;

    public static void Initialize(IConfiguration configuration)
    {
        _instancia._connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=libro_ejemplar_db;User Id=root;Password=root;";
    }

    public MySqlConnection GetConnection()
    {
        return new MySqlConnection(_connectionString);
    }
}
