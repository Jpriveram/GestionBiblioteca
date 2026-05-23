using Microservicio_Autor.Infrastructure.Persistence;

namespace Microservicio_Autor.Infrastructure.Creators;

public class AutorRepositoryCreator
{
    public AutorRepository CreateRepository()
    {
        return new AutorRepository();
    }
}