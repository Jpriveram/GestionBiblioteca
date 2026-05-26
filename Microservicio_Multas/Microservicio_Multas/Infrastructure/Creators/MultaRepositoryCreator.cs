using MongoDB.Driver;
using ServicioMultas.Infrastructure.Persistence;

namespace ServicioMultas.Infrastructure.Creators;

public class MultaRepositoryCreator
{
    private readonly IMongoDatabase _database;
    private readonly string _collectionName;

    public MultaRepositoryCreator(IMongoDatabase database, string collectionName)
    {
        _database = database;
        _collectionName = collectionName;
    }

    public MultaRepository CreateRepository()
    {
        return new MultaRepository(_database, _collectionName);
    }
}
