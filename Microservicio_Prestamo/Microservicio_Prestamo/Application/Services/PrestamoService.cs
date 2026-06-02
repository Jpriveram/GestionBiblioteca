using Microservicio_Prestamo.Application.Dtos;
using Microservicio_Prestamo.Application.Interfaces;
using Microservicio_Prestamo.Domain.Entities;
using Microservicio_Prestamo.Domain.Errors;
using Microservicio_Prestamo.Domain.Ports;

namespace Microservicio_Prestamo.Application.Services;

public class PrestamoService : IPrestamoService
{
    private readonly IPrestamoRepository _repo;

    public PrestamoService(IPrestamoRepository repo) => _repo = repo;

    public List<PrestamoDto> GetAll(int? lectorId = null, bool incluirAnulados = false)
    {
        var prestamos = _repo.GetAll(!incluirAnulados);
        if (lectorId.HasValue)
            prestamos = prestamos.Where(p => p.LectorId == lectorId.Value);
        return prestamos.Select(MapToDto).ToList();
    }

    public PrestamoDto? GetById(int id)
    {
        var p = _repo.GetById(id);
        return p is not null ? MapToDto(p) : null;
    }

    public PrestamoDto Create(CreatePrestamoDto dto)
    {
        if (dto.LectorId <= 0 || dto.Ejemplares.Count == 0)
            throw new InvalidOperationException(PrestamoErrors.DatosObligatorios.Message);

        if (dto.Ejemplares.Count > 5)
            throw new InvalidOperationException(PrestamoErrors.MaxEjemplares.Message);

        if (_repo.CountActivosByLector(dto.LectorId) >= 3)
            throw new InvalidOperationException(PrestamoErrors.MaxActivos.Message);

        var prestamo = new Prestamo
        {
            LectorId = dto.LectorId,
            FechaPrestamo = DateTime.Now,
            FechaDevolucionEsperada = dto.FechaDevolucionEsperada ?? DateTime.Now.AddDays(7),
            ObservacionesSalida = dto.ObservacionesSalida,
            Estado = 1,
            UsuarioSesionId = dto.UsuarioSesionId
        };

        var detalles = dto.Ejemplares.Select(e => new Detalle
        {
            EjemplarId = e.EjemplarId,
            ObservacionesSalida = e.ObservacionesSalida,
            EstadoDetalle = 1,
            UsuarioSesionId = dto.UsuarioSesionId
        }).ToList();

        prestamo.PrestamoId = _repo.CrearPrestamoTransaccional(prestamo, detalles, dto.UsuarioSesionId);
        return MapToDto(prestamo);
    }

    public void Anular(int id, int? usuarioSesionId, string? motivo = null)
    {
        var prestamo = _repo.GetById(id)
            ?? throw new InvalidOperationException(PrestamoErrors.NoEncontrado.Message);

        if (prestamo.Estado == 0)
            throw new InvalidOperationException(PrestamoErrors.YaAnulado.Message);

        prestamo.Estado = 0;
        prestamo.UsuarioSesionId = usuarioSesionId;
        prestamo.ObservacionesEntrada = motivo;
        prestamo.UltimaActualizacion = DateTime.Now;
        _repo.Update(prestamo);
    }

    public int CountActivosByLector(int lectorId) => _repo.CountActivosByLector(lectorId);

    private static PrestamoDto MapToDto(Prestamo p) => new()
    {
        PrestamoId = p.PrestamoId,
        LectorId = p.LectorId,
        FechaPrestamo = p.FechaPrestamo,
        FechaDevolucionEsperada = p.FechaDevolucionEsperada,
        FechaDevolucionReal = p.FechaDevolucionReal,
        ObservacionesSalida = p.ObservacionesSalida,
        ObservacionesEntrada = p.ObservacionesEntrada,
        Estado = p.Estado,
        UsuarioSesionId = p.UsuarioSesionId,
        FechaRegistro = p.FechaRegistro,
        UltimaActualizacion = p.UltimaActualizacion
    };
}
