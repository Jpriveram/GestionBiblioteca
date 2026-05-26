using ServicioMultas.Application.Dtos;
using ServicioMultas.Application.Interfaces;
using ServicioMultas.Domain.Entities;
using ServicioMultas.Domain.Errors;
using ServicioMultas.Domain.Ports;
using ServicioMultas.Domain.Validations;

namespace ServicioMultas.Application.Services;

public class MultaService : IMultaService
{
    private readonly IMultaRepository _repository;

    public MultaService(IMultaRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<MultaDto>> GetAllAsync(int? usuarioId = null)
    {
        IEnumerable<Multa> multas;

        if (usuarioId.HasValue)
            multas = await _repository.GetByUsuarioIdAsync(usuarioId.Value);
        else
            multas = await _repository.GetAllAsync();

        return multas.Select(MapToDto).ToList();
    }

    public async Task<MultaDto?> GetByIdAsync(string id)
    {
        var multa = await _repository.GetByIdAsync(id);
        return multa is not null ? MapToDto(multa) : null;
    }

    public async Task<MultaDto> CreateAsync(CreateMultaDto dto)
    {
        if (dto.UsuarioId <= 0)
            throw new InvalidOperationException(MultaErrors.UsuarioIdRequerido.Message);

        if (!ValidadorEntrada.ValidarMonto(dto.Monto))
            throw new InvalidOperationException(MultaErrors.MontoInvalido.Message);

        if (!ValidadorEntrada.ValidarMotivo(dto.Motivo))
            throw new InvalidOperationException(MultaErrors.MotivoMuyCorto.Message);

        var multa = new Multa
        {
            UsuarioId = dto.UsuarioId,
            PrestamoId = dto.PrestamoId,
            Monto = dto.Monto,
            Motivo = ValidadorEntrada.NormalizarEspacios(dto.Motivo),
            Estado = true,
            UsuarioSesionId = dto.UsuarioSesionId,
            FechaRegistro = DateTime.UtcNow
        };

        await _repository.InsertAsync(multa);
        return MapToDto(multa);
    }

    public async Task<MultaDto?> UpdateAsync(string id, UpdateMultaDto dto)
    {
        var multa = await _repository.GetByIdAsync(id);
        if (multa is null)
            return null;

        if (!ValidadorEntrada.ValidarMonto(dto.Monto))
            throw new InvalidOperationException(MultaErrors.MontoInvalido.Message);

        if (!ValidadorEntrada.ValidarMotivo(dto.Motivo))
            throw new InvalidOperationException(MultaErrors.MotivoMuyCorto.Message);

        multa.Monto = dto.Monto;
        multa.Motivo = ValidadorEntrada.NormalizarEspacios(dto.Motivo);
        multa.Estado = dto.Estado;
        multa.UsuarioSesionId = dto.UsuarioSesionId;
        multa.UltimaActualizacion = DateTime.UtcNow;

        await _repository.UpdateAsync(multa);
        return MapToDto(multa);
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var multa = await _repository.GetByIdAsync(id);
        if (multa is null)
            return false;

        multa.Estado = false;
        multa.UltimaActualizacion = DateTime.UtcNow;
        await _repository.DeleteAsync(multa);
        return true;
    }

    private static MultaDto MapToDto(Multa m) => new()
    {
        Id = m.Id,
        UsuarioId = m.UsuarioId,
        PrestamoId = m.PrestamoId,
        Monto = m.Monto,
        Motivo = m.Motivo,
        Estado = m.Estado,
        UsuarioSesionId = m.UsuarioSesionId,
        FechaRegistro = m.FechaRegistro,
        UltimaActualizacion = m.UltimaActualizacion
    };
}
