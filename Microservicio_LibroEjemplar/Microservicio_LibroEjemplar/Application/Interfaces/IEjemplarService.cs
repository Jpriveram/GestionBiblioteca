using ServicioLibroEjemplar.Application.Dtos;

namespace ServicioLibroEjemplar.Application.Interfaces;

public interface IEjemplarService
{
    Task<List<EjemplarDto>> GetAllAsync();
    Task<EjemplarDto?> GetByIdAsync(int id);
    Task<EjemplarDto?> GetByCodigoInventarioAsync(string codigoInventario);
    Task<List<EjemplarDto>> GetByLibroIdAsync(int libroId);
    Task<List<EjemplarDto>> GetDisponiblesAsync();
    Task<EjemplarDto> CreateAsync(CreateEjemplarDto dto);
    Task<EjemplarDto?> UpdateAsync(int id, UpdateEjemplarDto dto);
    Task<bool> DeleteAsync(int id);
}
