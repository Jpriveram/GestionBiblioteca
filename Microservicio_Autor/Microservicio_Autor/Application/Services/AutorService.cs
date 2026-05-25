using Microservicio_Autor.Application.Dtos;
using Microservicio_Autor.Application.Interfaces;
using Microservicio_Autor.Domain.Entities;
using Microservicio_Autor.Domain.Errors;
using Microservicio_Autor.Domain.Validations;
using Microservicio_Autor.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Microservicio_Autor.Application.Services;

public class AutorService : IAutorService
{
    private readonly AutorRepository _repository;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AutorService(AutorRepository repository, IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<List<AutorDto>> GetAllAsync()
    {
        var autores = _repository.GetAll()
            .Where(a => a.Estado)
            .Select(MapToDto)
            .ToList();

        return Task.FromResult(autores);
    }

    public Task<AutorDto?> GetByIdAsync(int id)
    {
        var autor = _repository.GetById(id);
        return Task.FromResult(autor != null ? MapToDto(autor) : null);
    }

    public Task<AutorDto> CreateAsync(CreateAutorDto dto)
    {
        ValidateCreateDto(dto);

        var apellidos = NormalizeOptional(dto.Apellidos);

        var autor = new Autor
        {
            Nombres = NormalizeRequired(dto.Nombres),
            PrimerApellido = apellidos,
            SegundoApellido = null,
            Nacionalidad = NormalizeOptional(dto.Nacionalidad),
            FechaNacimiento = dto.FechaNacimiento,
            Estado = true,
            FechaRegistro = DateTime.Now,
            UsuarioSesionId = GetCurrentUserId()
        };

        _repository.Insert(autor);
        _repository.SaveChanges();

        return Task.FromResult(MapToDto(autor));
    }

    public Task<AutorDto?> UpdateAsync(int id, UpdateAutorDto dto)
    {
        var autor = _repository.GetById(id);
        if (autor == null)
            return Task.FromResult<AutorDto?>(null);

        ValidateUpdateDto(dto);

        var apellidos = NormalizeOptional(dto.Apellidos);

        autor.Nombres = NormalizeRequired(dto.Nombres);
        autor.PrimerApellido = apellidos;
        autor.SegundoApellido = null;
        autor.Nacionalidad = NormalizeOptional(dto.Nacionalidad);
        autor.FechaNacimiento = dto.FechaNacimiento;
        autor.Estado = dto.Estado;
        autor.UltimaActualizacion = DateTime.Now;
        autor.UsuarioSesionId = GetCurrentUserId();

        _repository.Update(autor);
        _repository.SaveChanges();

        return Task.FromResult<AutorDto?>(MapToDto(autor));
    }

    public Task<bool> DeleteAsync(int id)
    {
        var autor = _repository.GetById(id);
        if (autor == null || !autor.Estado)
            return Task.FromResult(false);

        autor.Estado = false;
        autor.UltimaActualizacion = DateTime.Now;
        autor.UsuarioSesionId = GetCurrentUserId();

        _repository.Update(autor);
        _repository.SaveChanges();

        return Task.FromResult(true);
    }

    private static AutorDto MapToDto(Autor autor)
    {
        return new AutorDto
        {
            AutorId = autor.AutorId,
            Nombres = autor.Nombres,
            PrimerApellido = autor.PrimerApellido,
            SegundoApellido = autor.SegundoApellido,
            Apellidos = BuildApellidos(autor.PrimerApellido, autor.SegundoApellido),
            Nacionalidad = autor.Nacionalidad,
            FechaNacimiento = autor.FechaNacimiento,
            Estado = autor.Estado,
            FechaRegistro = autor.FechaRegistro,
            UltimaActualizacion = autor.UltimaActualizacion,
            UsuarioSesionId = autor.UsuarioSesionId
        };
    }

    private static void ValidateCreateDto(CreateAutorDto dto)
    {
        if (dto is null)
            throw new InvalidOperationException(AutorErrors.DatosObligatorios.Message);

        ValidateCommonFields(dto.Nombres, dto.Apellidos, dto.Nacionalidad, dto.FechaNacimiento);
    }

    private static void ValidateUpdateDto(UpdateAutorDto dto)
    {
        if (dto is null)
            throw new InvalidOperationException(AutorErrors.DatosObligatorios.Message);

        ValidateCommonFields(dto.Nombres, dto.Apellidos, dto.Nacionalidad, dto.FechaNacimiento);
    }

    private static void ValidateCommonFields(
        string nombres,
        string? apellidos,
        string? nacionalidad,
        DateTime? fechaNacimiento)
    {
        if (string.IsNullOrWhiteSpace(nombres))
            throw new InvalidOperationException(AutorErrors.DatosObligatorios.Message);

        if (!ValidadorEntrada.TextoValido(nombres, 100) ||
            !ValidadorEntrada.TextoValido(apellidos, 200) ||
            !ValidadorEntrada.TextoValido(nacionalidad, 100))
            throw new InvalidOperationException(AutorErrors.DatosInvalidos.Message);

        if (fechaNacimiento.HasValue && fechaNacimiento.Value.Date > DateTime.Now.Date)
            throw new InvalidOperationException(AutorErrors.DatosInvalidos.Message);
    }

    private static string NormalizeRequired(string value)
    {
        return ValidadorEntrada.NormalizarEspacios(value);
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalizedValue = ValidadorEntrada.NormalizarEspacios(value);
        return string.IsNullOrWhiteSpace(normalizedValue) ? null : normalizedValue;
    }

    private static string? BuildApellidos(string? primerApellido, string? segundoApellido)
    {
        var apellidos = $"{primerApellido} {segundoApellido}".Trim();
        return string.IsNullOrWhiteSpace(apellidos) ? null : apellidos;
    }

    private int? GetCurrentUserId()
    {
        var httpContext = _httpContextAccessor?.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
            return null;

        var sub = httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                  ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (int.TryParse(sub, out var id))
            return id;

        return null;
    }
}