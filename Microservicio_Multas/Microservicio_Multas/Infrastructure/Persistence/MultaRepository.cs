using MongoDB.Driver;
using ServicioMultas.Domain.Entities;
using ServicioMultas.Domain.Ports;

namespace ServicioMultas.Infrastructure.Persistence;

public class MultaRepository : IMultaRepository
{
    private readonly IMongoCollection<Multa> _multas;

    public MultaRepository(IMongoDatabase database, string collectionName)
    {
        _multas = database.GetCollection<Multa>(collectionName);
    }

    public async Task<IEnumerable<Multa>> GetAllAsync()
    {
        var filter = Builders<Multa>.Filter.Eq(m => m.Estado, true);
        return await _multas.Find(filter).SortByDescending(m => m.FechaRegistro).ToListAsync();
    }

    public async Task<Multa?> GetByIdAsync(string id)
    {
        var filter = Builders<Multa>.Filter.Eq(m => m.Id, id) &
                     Builders<Multa>.Filter.Eq(m => m.Estado, true);
        return await _multas.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Multa>> GetByUsuarioIdAsync(int usuarioId)
    {
        var filter = Builders<Multa>.Filter.Eq(m => m.UsuarioId, usuarioId) &
                     Builders<Multa>.Filter.Eq(m => m.Estado, true);
        return await _multas.Find(filter).SortByDescending(m => m.FechaRegistro).ToListAsync();
    }

    public async Task InsertAsync(Multa entity)
    {
        entity.FechaRegistro = DateTime.UtcNow;
        entity.UltimaActualizacion = DateTime.UtcNow;
        await _multas.InsertOneAsync(entity);
    }

    public async Task UpdateAsync(Multa entity)
    {
        entity.UltimaActualizacion = DateTime.UtcNow;
        var filter = Builders<Multa>.Filter.Eq(m => m.Id, entity.Id);
        await _multas.ReplaceOneAsync(filter, entity);
    }

    public async Task DeleteAsync(Multa entity)
    {
        entity.Estado = false;
        entity.UltimaActualizacion = DateTime.UtcNow;
        var filter = Builders<Multa>.Filter.Eq(m => m.Id, entity.Id);
        var update = Builders<Multa>.Update
            .Set(m => m.Estado, false)
            .Set(m => m.UltimaActualizacion, DateTime.UtcNow)
            .Set(m => m.UsuarioSesionId, entity.UsuarioSesionId);
        await _multas.UpdateOneAsync(filter, update);
    }
}
