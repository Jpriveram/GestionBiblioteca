using Frontend.Dtos;
using Frontend.Helpers;

namespace Frontend.Adapters;

public interface IAutorServicio
{
    IEnumerable<AutorDto> Select(bool todos = false);
    Result<AutorDto> Create(AutorDto dto);
    Result<AutorDto> Update(AutorDto dto);
    Result Delete(int id, int? usuarioSesionId);
    AutorDto? GetById(int id);
    Dictionary<int, string> ObtenerAutoresActivos();
    bool ExisteAutorActivo(int autorId);
}
