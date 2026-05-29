using Microservicio_Prestamo.Application.Dtos;

namespace Microservicio_Prestamo.Application.Interfaces;

public interface IPrestamoService
{
    List<PrestamoDto> GetAll(int? lectorId = null, bool incluirAnulados = false);
    PrestamoDto? GetById(int id);
    PrestamoDto Create(CreatePrestamoDto dto);
    void Anular(int id, int? usuarioSesionId, string? motivo = null);
    int CountActivosByLector(int lectorId);
}
