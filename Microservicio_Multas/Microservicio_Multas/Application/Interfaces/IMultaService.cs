using ServicioMultas.Application.Dtos;

namespace ServicioMultas.Application.Interfaces;

public interface IMultaService
{
    Task<List<MultaDto>> GetAllAsync(int? usuarioId = null);
    Task<MultaDto?> GetByIdAsync(string id);
    Task<MultaDto> CreateAsync(CreateMultaDto dto);
    Task<MultaDto?> UpdateAsync(string id, UpdateMultaDto dto);
    Task<bool> DeleteAsync(string id);
}
