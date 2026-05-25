using ServicioLibroEjemplar.Application.Dtos;
using ServicioLibroEjemplar.Application.Interfaces;
using ServicioLibroEjemplar.Domain.Entities;
using ServicioLibroEjemplar.Domain.Errors;
using ServicioLibroEjemplar.Domain.Validations;
using ServicioLibroEjemplar.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ServicioLibroEjemplar.Application.Services;

public class EjemplarService : IEjemplarService
{
    private readonly EjemplarRepository _repositorio;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EjemplarService(EjemplarRepository repositorio, IHttpContextAccessor httpContextAccessor)
    {
        _repositorio = repositorio;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<List<EjemplarDto>> GetAllAsync()
    {
        var ejemplares = _repositorio.GetAll()
            .Where(e => e.Estado)
            .Select(MapToDto)
            .ToList();
        return Task.FromResult(ejemplares);
    }

    public Task<EjemplarDto?> GetByIdAsync(int id)
    {
        var ejemplar = _repositorio.GetById(id);
        return Task.FromResult(ejemplar != null ? MapToDto(ejemplar) : null);
    }

    public Task<EjemplarDto?> GetByCodigoInventarioAsync(string codigoInventario)
    {
        var normalizedCodigo = NormalizeCodigoInventario(codigoInventario);
        var ejemplar = _repositorio.GetByCodigoInventario(normalizedCodigo);
        return Task.FromResult(ejemplar != null ? MapToDto(ejemplar) : null);
    }

    public Task<List<EjemplarDto>> GetByLibroIdAsync(int libroId)
    {
        var ejemplares = _repositorio.GetByLibroId(libroId)
            .Where(e => e.Estado)
            .Select(MapToDto)
            .ToList();
        return Task.FromResult(ejemplares);
    }

    public Task<List<EjemplarDto>> GetDisponiblesAsync()
    {
        var ejemplares = _repositorio.GetAll()
            .Where(e => e.Estado && e.Disponible && !e.DadoDeBaja)
            .Select(MapToDto)
            .ToList();
        return Task.FromResult(ejemplares);
    }

    public Task<EjemplarDto> CreateAsync(CreateEjemplarDto dto)
    {
        ValidateCreateDto(dto);

        var codigoInventario = NormalizeCodigoInventario(dto.CodigoInventario);
        if (_repositorio.GetByCodigoInventario(codigoInventario) != null)
        {
            throw new InvalidOperationException(EjemplarErrors.CodigoInventarioDuplicado.Message);
        }

        var ejemplar = new Ejemplar
        {
            LibroId = dto.LibroId,
            CodigoInventario = codigoInventario,
            EstadoConservacion = NormalizeOptional(dto.EstadoConservacion),
            Disponible = true,
            DadoDeBaja = false,
            Ubicacion = NormalizeOptional(dto.Ubicacion),
            Estado = true,
            FechaRegistro = DateTime.UtcNow,
            UsuarioSesionId = GetCurrentUserId()
        };

        _repositorio.Insert(ejemplar);
        _repositorio.SaveChanges();

        return Task.FromResult(MapToDto(ejemplar));
    }

    public Task<EjemplarDto?> UpdateAsync(int id, UpdateEjemplarDto dto)
    {
        var ejemplar = _repositorio.GetById(id);
        if (ejemplar == null)
            return Task.FromResult<EjemplarDto?>(null);

        ValidateUpdateDto(dto);

        var codigoInventario = NormalizeCodigoInventario(dto.CodigoInventario);
        var duplicate = _repositorio.GetByCodigoInventario(codigoInventario);
        if (duplicate != null && duplicate.EjemplarId != id)
        {
            throw new InvalidOperationException(EjemplarErrors.CodigoInventarioDuplicado.Message);
        }

        ejemplar.CodigoInventario = codigoInventario;
        ejemplar.EstadoConservacion = NormalizeOptional(dto.EstadoConservacion);
        ejemplar.Disponible = dto.Disponible;
        ejemplar.DadoDeBaja = dto.DadoDeBaja;
        ejemplar.MotivoBaja = NormalizeOptional(dto.MotivoBaja);
        ejemplar.Ubicacion = NormalizeOptional(dto.Ubicacion);
        ejemplar.Estado = dto.Estado;
        ejemplar.UltimaActualizacion = DateTime.UtcNow;
        ejemplar.UsuarioSesionId = GetCurrentUserId();

        _repositorio.Update(ejemplar);
        _repositorio.SaveChanges();

        return Task.FromResult<EjemplarDto?>(MapToDto(ejemplar));
    }

    public Task<bool> DeleteAsync(int id)
    {
        var ejemplar = _repositorio.GetById(id);
        if (ejemplar == null)
            return Task.FromResult(false);

        if (!ejemplar.Estado)
            return Task.FromResult(false);

        ejemplar.Estado = false;
        ejemplar.UltimaActualizacion = DateTime.UtcNow;
        ejemplar.UsuarioSesionId = GetCurrentUserId();

        _repositorio.Update(ejemplar);
        _repositorio.SaveChanges();
        return Task.FromResult(true);
    }

    private static EjemplarDto MapToDto(Ejemplar ejemplar)
    {
        return new EjemplarDto
        {
            EjemplarId = ejemplar.EjemplarId,
            LibroId = ejemplar.LibroId,
            CodigoInventario = ejemplar.CodigoInventario,
            EstadoConservacion = ejemplar.EstadoConservacion,
            Disponible = ejemplar.Disponible,
            DadoDeBaja = ejemplar.DadoDeBaja,
            MotivoBaja = ejemplar.MotivoBaja,
            Ubicacion = ejemplar.Ubicacion,
            Estado = ejemplar.Estado,
            FechaRegistro = ejemplar.FechaRegistro,
            UltimaActualizacion = ejemplar.UltimaActualizacion,
            UsuarioSesionId = ejemplar.UsuarioSesionId
        };
    }

    private int? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var sub = httpContext.User.FindFirst("sub")?.Value
              ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(sub, out var id))
            return id;

        return null;
    }

    private static void ValidateCreateDto(CreateEjemplarDto dto)
    {
        if (dto is null)
            throw new InvalidOperationException(EjemplarErrors.DatosObligatorios.Message);

        if (dto.LibroId <= 0)
            throw new InvalidOperationException(EjemplarErrors.DatosObligatorios.Message);

        ValidateCodigoInventario(dto.CodigoInventario);
    }

    private static void ValidateUpdateDto(UpdateEjemplarDto dto)
    {
        if (dto is null)
            throw new InvalidOperationException(EjemplarErrors.DatosObligatorios.Message);

        ValidateCodigoInventario(dto.CodigoInventario);
    }

    private static void ValidateCodigoInventario(string codigoInventario)
    {
        if (string.IsNullOrWhiteSpace(codigoInventario))
            throw new InvalidOperationException(EjemplarErrors.DatosObligatorios.Message);

        if (!ValidadorEntrada.CodigoInventarioValido(codigoInventario))
            throw new InvalidOperationException(EjemplarErrors.DatosInvalidos.Message);
    }

    private static string NormalizeRequired(string value)
    {
        return ValidadorEntrada.NormalizarEspacios(value);
    }

    private static string NormalizeCodigoInventario(string? value)
    {
        return ValidadorEntrada.NormalizarAMayusculas(value);
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalizado = ValidadorEntrada.NormalizarEspacios(value);
        return string.IsNullOrWhiteSpace(normalizado) ? null : normalizado;
    }
}
