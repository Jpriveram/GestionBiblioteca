using MySql.Data.MySqlClient;

namespace Microservicio_Prestamo.Infrastructure.Configuration;

public class ConfigurationSingleton
{
    private static readonly ConfigurationSingleton _instancia = new();
    private string _connectionString = "Server=localhost;Database=bibliotecabd;User Id=root;Password=root;";

    public static ConfigurationSingleton Instancia => _instancia;

    public static void Initialize(IConfiguration configuration)
    {
        _instancia._connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=bibliotecabd;User Id=root;Password=root;";
    }

    public MySqlConnection GetConnection() => new(_connectionString);
}
