using Frontend.Dtos;
using Frontend.Helpers;

namespace Frontend.Adapters;

public interface IMultaServicio
{
    Task<IEnumerable<MultaDto>> SelectAsync(int? usuarioId = null);
    Task<MultaDto?> GetByIdAsync(string id);
    Task<Result<MultaDto>> CreateAsync(MultaDto dto);
    Task<Result<MultaDto>> UpdateAsync(MultaDto dto);
    Task<Result> DeleteAsync(string id);
}
