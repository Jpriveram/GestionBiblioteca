using Microsoft.Extensions.Configuration;
using ServicioLibroEjemplar.Infrastructure.Persistence;

namespace ServicioLibroEjemplar.Infrastructure.Creators;

public class LibroRepositoryCreator
{
    private readonly IConfiguration _configuration;

    public LibroRepositoryCreator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LibroRepository CreateRepository()
    {
        return new LibroRepository(_configuration);
    }
}
