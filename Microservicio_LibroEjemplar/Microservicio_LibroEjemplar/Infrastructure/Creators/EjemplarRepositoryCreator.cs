using Microsoft.Extensions.Configuration;
using ServicioLibroEjemplar.Infrastructure.Persistence;

namespace ServicioLibroEjemplar.Infrastructure.Creators;

public class EjemplarRepositoryCreator
{
    private readonly IConfiguration _configuration;

    public EjemplarRepositoryCreator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public EjemplarRepository CreateRepository()
    {
        return new EjemplarRepository(_configuration);
    }
}
