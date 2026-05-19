using Frontend.Dtos;
using Frontend.Helpers;
using Frontend.Dtos;

namespace Frontend.Adapters;

public interface ILibroServicio
{
    IEnumerable<LibroDto> Select(bool todos = false);
    LibroDto? GetById(int id);
    Result Create(LibroDto dto, string? nombreAutorNuevo);
    Result Update(LibroDto dto);
    Result Delete(int libroId, int? usuarioSesionId);
    Dictionary<int, string> ObtenerNombresAutores();
    IEnumerable<AutorDto> ObtenerAutoresActivos();
    bool ExisteAutorActivo(int autorId);
    int InsertarAutorYObtenerID(string nombreCompleto, int? usuarioSesionId);
}
