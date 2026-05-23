using Microservicio_Autor.Application.Dtos;
using Microservicio_Autor.Application.Interfaces;
using Microservicio_Autor.Domain.Entities;
using Microservicio_Autor.Domain.Errors;
using Microservicio_Autor.Domain.Validations;
using Microservicio_Autor.Infrastructure.Persistence;

namespace Microservicio_Autor.Application.Services;

public class AutorService : IAutorService
{
    private readonly AutorRepository _repository;

    public AutorService(AutorRepository repository)
    {
        _repository = repository;
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
            PrimerApellido = NormalizeOptional(dto.PrimerApellido),
            SegundoApellido = NormalizeOptional(dto.SegundoApellido),
            Nacionalidad = NormalizeOptional(dto.Nacionalidad),
            FechaNacimiento = dto.FechaNacimiento,
            Biografia = NormalizeOptional(dto.Biografia),
            Estado = true,
            FechaRegistro = DateTime.UtcNow
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
        autor.PrimerApellido = NormalizeOptional(dto.PrimerApellido);
        autor.SegundoApellido = NormalizeOptional(dto.SegundoApellido);
        autor.Nacionalidad = NormalizeOptional(dto.Nacionalidad);
        autor.FechaNacimiento = dto.FechaNacimiento;
        autor.Biografia = NormalizeOptional(dto.Biografia);
        autor.Estado = dto.Estado;
        autor.UltimaActualizacion = DateTime.UtcNow;

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
        autor.UltimaActualizacion = DateTime.UtcNow;

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
            Nacionalidad = autor.Nacionalidad,
            FechaNacimiento = autor.FechaNacimiento,
            Biografia = autor.Biografia,
            Estado = autor.Estado,
            FechaRegistro = autor.FechaRegistro,
            UltimaActualizacion = autor.UltimaActualizacion
        };
    }

    private static void ValidateCreateDto(CreateAutorDto dto)
    {
        if (dto is null)
            throw new InvalidOperationException(AutorErrors.DatosObligatorios.Message);

        ValidateCommonFields(dto.Nombres, dto.PrimerApellido, dto.SegundoApellido, dto.Nacionalidad, dto.Biografia, dto.FechaNacimiento);
    }

    private static void ValidateUpdateDto(UpdateAutorDto dto)
    {
        if (dto is null)
            throw new InvalidOperationException(AutorErrors.DatosObligatorios.Message);

        ValidateCommonFields(dto.Nombres, dto.PrimerApellido, dto.SegundoApellido, dto.Nacionalidad, dto.Biografia, dto.FechaNacimiento);
    }

    private static void ValidateCommonFields(
        string nombres,
        string? primerApellido,
        string? segundoApellido,
        string? nacionalidad,
        string? biografia,
        DateTime? fechaNacimiento)
    {
        if (string.IsNullOrWhiteSpace(nombres))
            throw new InvalidOperationException(AutorErrors.DatosObligatorios.Message);

        if (!ValidadorEntrada.TextoValido(nombres, 100) ||
            !ValidadorEntrada.TextoValido(primerApellido, 100) ||
            !ValidadorEntrada.TextoValido(segundoApellido, 100) ||
            !ValidadorEntrada.TextoValido(nacionalidad, 100) ||
            !ValidadorEntrada.TextoValido(biografia, 1000))
            throw new InvalidOperationException(AutorErrors.DatosInvalidos.Message);

        if (fechaNacimiento.HasValue && fechaNacimiento.Value.Date > DateTime.UtcNow.Date)
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
}