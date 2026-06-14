using ServicioLibroEjemplar.Application.Dtos;

namespace ServicioLibroEjemplar.Application.Interfaces;

public interface ILibroService
{
    Task<List<LibroDto>> GetAllAsync(bool todos = false);
    Task<LibroDto?> GetByIdAsync(int id);
    Task<LibroDto?> GetByIsbnAsync(string isbn);
    Task<List<LibroDto>> GetByAutorIdAsync(int autorId);
    Task<LibroDto> CreateAsync(CreateLibroDto dto);
    Task<LibroDto?> UpdateAsync(int id, UpdateLibroDto dto);
    Task<bool> DeleteAsync(int id);
}
