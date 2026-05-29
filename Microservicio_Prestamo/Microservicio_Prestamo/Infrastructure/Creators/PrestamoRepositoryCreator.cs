using Microservicio_Prestamo.Infrastructure.Persistence;

namespace Microservicio_Prestamo.Infrastructure.Creators;

public class PrestamoRepositoryCreator
{
    public PrestamoRepository CreateRepository() => new();
}
