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

        var autor = new Autor
        {
            Nombres = NormalizeRequired(dto.Nombres),
            PrimerApellido = NormalizeApellidoOptional(dto.Apellidos),
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

        autor.Nombres = NormalizeRequired(dto.Nombres);
        autor.PrimerApellido = NormalizeApellidoOptional(dto.Apellidos);
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
            throw new InvalidOperationException("Ingrese el nombre del autor.");

        if (!ValidadorEntrada.TextoValido(nombres, 100))
            throw new InvalidOperationException("El nombre no debe superar los 100 caracteres.");

        if (!ValidadorEntrada.SoloLetrasYEspacios(nombres))
            throw new InvalidOperationException("El nombre solo debe contener letras y espacios.");

        if (ValidadorEntrada.TieneLetrasSeparadas(nombres))
            throw new InvalidOperationException("No se permiten letras separadas por espacios.");

        if (!string.IsNullOrWhiteSpace(apellidos))
        {
            if (!ValidadorEntrada.TextoValido(apellidos, 200))
                throw new InvalidOperationException("Los apellidos no deben superar los 200 caracteres.");

            if (!ValidadorEntrada.SoloLetrasYEspacios(apellidos))
                throw new InvalidOperationException("Los apellidos solo deben contener letras y espacios.");

            if (ValidadorEntrada.TieneLetrasSeparadas(apellidos))
                throw new InvalidOperationException("No se permiten letras separadas por espacios en los apellidos.");
        }

        if (!string.IsNullOrWhiteSpace(nacionalidad))
        {
            if (!ValidadorEntrada.TextoValido(nacionalidad, 100))
                throw new InvalidOperationException("La nacionalidad no debe superar los 100 caracteres.");

            if (!ValidadorEntrada.SoloLetrasYEspacios(nacionalidad))
                throw new InvalidOperationException("La nacionalidad solo debe contener letras y espacios.");

            if (ValidadorEntrada.TieneLetrasSeparadas(nacionalidad))
                throw new InvalidOperationException("No se permiten letras separadas por espacios en la nacionalidad.");
        }

        if (!fechaNacimiento.HasValue)
            throw new InvalidOperationException("Ingrese la fecha de nacimiento.");

        if (fechaNacimiento.Value.Date > DateTime.Today)
            throw new InvalidOperationException("La fecha de nacimiento no puede ser futura.");

        if (!ValidadorEntrada.EsMayorDeEdad(fechaNacimiento.Value))
            throw new InvalidOperationException("El autor debe ser mayor de edad.");
    }

    private static string NormalizeRequired(string value)
    {
        return ValidadorEntrada.FormatearNombrePropio(value);
    }

    private static string? NormalizeOptional(string? value)
    {
        return ValidadorEntrada.FormatearTextoOpcional(value);
    }

    private static string? NormalizeApellidoOptional(string? value)
    {
        return ValidadorEntrada.FormatearApellidoOpcional(value);
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