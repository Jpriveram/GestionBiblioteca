using Frontend.Dtos;
using Frontend.Helpers;
using Frontend.Dtos;

namespace Frontend.Adapters;

public interface IPrestamoServicio
{
    IEnumerable<PrestamoDto> Select(bool todos = false);
    Result<PrestamoDto> Create(PrestamoDto dto);
    Result<PrestamoDto> Update(PrestamoDto dto);
    Result Delete(PrestamoDto dto);
    PrestamoDto? GetById(int id);
    Result ValidarPrestamo(PrestamoDto PrestamoDto);
    int CountPrestamosActivos(int lectorId);
    int InsertAndReturnId(PrestamoDto PrestamoDto);
}
