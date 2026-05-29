using Frontend.Dtos;
using Frontend.Helpers;
using Frontend.Dtos;
using System.Collections.Generic;

namespace Frontend.Adapters;

public interface IPrestamoFachada
{
    IEnumerable<KeyValuePair<int, string>> BuscarEjemplaresActivos(string q);
    IEnumerable<KeyValuePair<int, string>> BuscarLectoresPorCi(string q);
    Result<int> CrearPrestamoMultiple(int lectorId, IEnumerable<int> ejemplarIds, DateTime fechaDevolucionEsperada, int? usuarioSesionId = null, string? observacionesSalida = null);
    Result<int> CrearPrestamoMultiple(int lectorId, IEnumerable<(int EjemplarId, string? ObservacionesSalida)> detallesEjemplares, DateTime fechaDevolucionEsperada, int? usuarioSesionId = null);
    Result CrearPrestamo(PrestamoDto PrestamoDto);
    int CountPrestamosActivos(int lectorId);
    PrestamoDto? ObtenerPrestamoPorId(int id);
    EjemplarDto? ObtenerEjemplarPorId(int id);
    string? ObtenerLabelEjemplar(int ejemplarId);
    UsuarioDto? ObtenerUsuarioPorCi(string ci);
    List<object> ObtenerTodosLosLectores();
    Result CrearPrestamos(IEnumerable<PrestamoDto> prestamos);
}

public interface IAnulacionFachada
{
    Result AnularPrestamo(int prestamoId, int? usuarioSesionId, string motivo);
}

public interface IEjemplarDisponibilidadFachada
{
    Result CambiarDisponibilidad(int ejemplarId, bool disponible, int? usuarioSesionId);
}
