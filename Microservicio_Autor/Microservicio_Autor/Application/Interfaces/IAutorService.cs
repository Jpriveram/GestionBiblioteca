using Microservicio_Autor.Application.Dtos;

namespace Microservicio_Autor.Application.Interfaces;

public interface IAutorService
{
    Task<List<AutorDto>> GetAllAsync();
    Task<AutorDto?> GetByIdAsync(int id);
    Task<AutorDto> CreateAsync(CreateAutorDto dto);
    Task<AutorDto?> UpdateAsync(int id, UpdateAutorDto dto);
    Task<bool> DeleteAsync(int id);
}