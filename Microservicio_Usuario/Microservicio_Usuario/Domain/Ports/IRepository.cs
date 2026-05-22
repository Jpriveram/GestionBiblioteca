using System.Collections.Generic;

namespace ServicioUsuario.Domain.Ports;

public interface IRepository<T, TId> where T : class
{
    IEnumerable<T> GetAll();

    T? GetById(TId id);

    void Insert(T entity);

    void Update(T entity);

    void Delete(T entity);

    void SaveChanges();
}

