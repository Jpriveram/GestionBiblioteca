using ServicioMultas.Domain.Entities;

namespace ServicioMultas.Domain.Ports;

public interface IMultaRepository : IRepository<Multa, string>
{
    Task<IEnumerable<Multa>> GetByUsuarioIdAsync(int usuarioId);
}
