using ServicioLibroEjemplar.Application.Dtos;
using ServicioLibroEjemplar.Application.Interfaces;
using ServicioLibroEjemplar.Domain.Entities;
using ServicioLibroEjemplar.Domain.Errors;
using ServicioLibroEjemplar.Domain.Validations;
using ServicioLibroEjemplar.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ServicioLibroEjemplar.Application.Services;

public class LibroService : ILibroService
{
    private readonly LibroRepository _repositorio;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LibroService(LibroRepository repositorio, IHttpContextAccessor httpContextAccessor)
    {
        _repositorio = repositorio;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<List<LibroDto>> GetAllAsync(bool todos = false)
    {
        var libros = _repositorio.GetAll(todos)
            .Select(MapToDto)
            .ToList();
        return Task.FromResult(libros);
    }

    public Task<LibroDto?> GetByIdAsync(int id)
    {
        var libro = _repositorio.GetById(id);
        return Task.FromResult(libro != null ? MapToDto(libro) : null);
    }

    public Task<LibroDto?> GetByIsbnAsync(string isbn)
    {
        var normalizedIsbn = NormalizeIsbn(isbn);
        var libro = _repositorio.GetByIsbn(normalizedIsbn);
        return Task.FromResult(libro != null ? MapToDto(libro) : null);
    }

    public Task<List<LibroDto>> GetByAutorIdAsync(int autorId)
    {
        var libros = _repositorio.GetByAutorId(autorId)
            .Where(l => l.Estado)
            .Select(MapToDto)
            .ToList();
        return Task.FromResult(libros);
    }

    public Task<LibroDto> CreateAsync(CreateLibroDto dto)
    {
        ValidateCreateDto(dto);

        var isbn = NormalizeIsbn(dto.ISBN);
        if (!string.IsNullOrWhiteSpace(isbn) && _repositorio.GetByIsbn(isbn) != null)
        {
            throw new InvalidOperationException(LibroErrors.ISBNDuplicado.Message);
        }

        var libro = new Libro
        {
            AutorId = dto.AutorId,
            Titulo = NormalizeRequired(dto.Titulo),
            ISBN = string.IsNullOrWhiteSpace(isbn) ? null : isbn,
            Editorial = NormalizeOptional(dto.Editorial),
            Genero = NormalizeOptional(dto.Genero),
            Edicion = NormalizeOptional(dto.Edicion),
            AñoPublicacion = dto.AñoPublicacion,
            NumeroPaginas = dto.NumeroPaginas,
            Idioma = NormalizeOptional(dto.Idioma),
            PaisPublicacion = NormalizeOptional(dto.PaisPublicacion),
            Descripcion = NormalizeOptional(dto.Descripcion),
            Estado = true,
            FechaRegistro = DateTime.Now,
            UsuarioSesionId = GetCurrentUserId()
        };

        _repositorio.Insert(libro);
        _repositorio.SaveChanges();

        return Task.FromResult(MapToDto(libro));
    }

    public Task<LibroDto?> UpdateAsync(int id, UpdateLibroDto dto)
    {
        var libro = _repositorio.GetById(id);
        if (libro == null)
            return Task.FromResult<LibroDto?>(null);

        ValidateUpdateDto(dto);

        var isbn = NormalizeIsbn(dto.ISBN);
        if (!string.IsNullOrWhiteSpace(isbn))
        {
            var duplicate = _repositorio.GetByIsbn(isbn);
            if (duplicate != null && duplicate.LibroId != id)
            {
                throw new InvalidOperationException(LibroErrors.ISBNDuplicado.Message);
            }
        }

        libro.AutorId = dto.AutorId;
        libro.Titulo = NormalizeRequired(dto.Titulo);
        libro.ISBN = string.IsNullOrWhiteSpace(isbn) ? null : isbn;
        libro.Editorial = NormalizeOptional(dto.Editorial);
        libro.Genero = NormalizeOptional(dto.Genero);
        libro.Edicion = NormalizeOptional(dto.Edicion);
        libro.AñoPublicacion = dto.AñoPublicacion;
        libro.NumeroPaginas = dto.NumeroPaginas;
        libro.Idioma = NormalizeOptional(dto.Idioma);
        libro.PaisPublicacion = NormalizeOptional(dto.PaisPublicacion);
        libro.Descripcion = NormalizeOptional(dto.Descripcion);
        libro.Estado = dto.Estado;
        libro.UltimaActualizacion = DateTime.Now;
        libro.UsuarioSesionId = GetCurrentUserId();

        _repositorio.Update(libro);
        _repositorio.SaveChanges();

        return Task.FromResult<LibroDto?>(MapToDto(libro));
    }

    public Task<bool> DeleteAsync(int id)
    {
        var libro = _repositorio.GetById(id);
        if (libro == null)
            return Task.FromResult(false);

        if (!libro.Estado)
            return Task.FromResult(false);

        libro.Estado = false;
        libro.UltimaActualizacion = DateTime.Now;
        libro.UsuarioSesionId = GetCurrentUserId();

        _repositorio.Update(libro);
        _repositorio.SaveChanges();
        return Task.FromResult(true);
    }

    private static LibroDto MapToDto(Libro libro)
    {
        return new LibroDto
        {
            LibroId = libro.LibroId,
            AutorId = libro.AutorId,
            Titulo = libro.Titulo,
            ISBN = libro.ISBN,
            Editorial = libro.Editorial,
            Genero = libro.Genero,
            Edicion = libro.Edicion,
            AñoPublicacion = libro.AñoPublicacion,
            NumeroPaginas = libro.NumeroPaginas,
            Idioma = libro.Idioma,
            PaisPublicacion = libro.PaisPublicacion,
            Descripcion = libro.Descripcion,
            Estado = libro.Estado,
            FechaRegistro = libro.FechaRegistro,
            UltimaActualizacion = libro.UltimaActualizacion,
            UsuarioSesionId = libro.UsuarioSesionId
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

    private static void ValidateCreateDto(CreateLibroDto dto)
    {
        if (dto is null)
            throw new InvalidOperationException(LibroErrors.DatosObligatorios.Message);

        ValidateCommonFields(dto.AutorId, dto.Titulo, dto.ISBN, dto.AñoPublicacion, dto.NumeroPaginas);
    }

    private static void ValidateUpdateDto(UpdateLibroDto dto)
    {
        if (dto is null)
            throw new InvalidOperationException(LibroErrors.DatosObligatorios.Message);

        ValidateCommonFields(dto.AutorId, dto.Titulo, dto.ISBN, dto.AñoPublicacion, dto.NumeroPaginas);
    }

    private static void ValidateCommonFields(int autorId, string titulo, string? isbn, int? añoPublicacion, int? numeroPaginas)
    {
        if (autorId <= 0 || string.IsNullOrWhiteSpace(titulo))
            throw new InvalidOperationException(LibroErrors.DatosObligatorios.Message);

        if (!string.IsNullOrWhiteSpace(isbn) && !ValidadorEntrada.ISBNValido(isbn))
            throw new InvalidOperationException(LibroErrors.DatosInvalidos.Message);

        if (añoPublicacion.HasValue && (añoPublicacion.Value < 1000 || añoPublicacion.Value > DateTime.Now.Year))
            throw new InvalidOperationException(LibroErrors.DatosInvalidos.Message);

        if (numeroPaginas.HasValue && numeroPaginas.Value <= 0)
            throw new InvalidOperationException(LibroErrors.DatosInvalidos.Message);
    }

    private static string NormalizeRequired(string value)
    {
        return ValidadorEntrada.NormalizarEspacios(value);
    }

    private static string? NormalizeOptional(string? value)
    {
        var normalizado = ValidadorEntrada.NormalizarEspacios(value);
        return string.IsNullOrWhiteSpace(normalizado) ? null : normalizado;
    }

    private static string NormalizeIsbn(string? value)
    {
        var normalizado = ValidadorEntrada.NormalizarAMayusculas(value);
        return string.IsNullOrWhiteSpace(normalizado) ? string.Empty : normalizado;
    }
}
