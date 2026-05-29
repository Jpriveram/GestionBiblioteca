using Microservicio_Prestamo.Domain.Entities;

namespace Microservicio_Prestamo.Domain.Ports;

public interface IRepository<T, TId> where T : class
{
    IEnumerable<T> GetAll();
    T? GetById(TId id);
    void Insert(T entity);
    void Update(T entity);
    void Delete(T entity);
}

public interface IPrestamoRepository : IRepository<Prestamo, int>
{
    int CrearPrestamoTransaccional(Prestamo prestamo, IEnumerable<Detalle> detalles, int? usuarioSesionId);
    IEnumerable<Prestamo> GetAll(bool activos);
    int CountActivosByLector(int lectorId);
}

public interface IOutboxRepository
{
    void Insert(OutboxMessage message);
    IEnumerable<OutboxMessage> GetPending(int limit);
    void MarkAsProcessed(long id);
}
