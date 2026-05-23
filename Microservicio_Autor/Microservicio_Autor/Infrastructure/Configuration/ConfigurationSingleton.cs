using Microsoft.Data.SqlClient;

namespace Microservicio_Autor.Infrastructure.Configuration;

public class ConfigurationSingleton
{
    private static readonly ConfigurationSingleton _instance = new();

    private string _connectionString =
        "Server=localhost;Database=autor_db;Trusted_Connection=True;TrustServerCertificate=True;";

    public static ConfigurationSingleton Instancia => _instance;

    public static void Initialize(IConfiguration configuration)
    {
        _instance._connectionString = configuration.GetConnectionString("DefaultConnection")
                                      ?? "Server=localhost;Database=autor_db;Trusted_Connection=True;TrustServerCertificate=True;";
    }

    public SqlConnection GetConnection()
    {
        return new SqlConnection(_connectionString);
    }
}