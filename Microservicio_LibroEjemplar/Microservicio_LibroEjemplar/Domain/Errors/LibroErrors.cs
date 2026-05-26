using ServicioLibroEjemplar.Domain.Common;

namespace ServicioLibroEjemplar.Domain.Errors;

public static class LibroErrors
{
    public static readonly Error LibroNoEncontrado = new("Libro.Id", "El libro no existe.");
    public static readonly Error ISBNDuplicado = new("Libro.ISBN", "Ya existe un libro registrado con ese ISBN.");
    public static readonly Error DatosObligatorios = new("Libro.Datos", "Completa todos los campos obligatorios.");
    public static readonly Error DatosInvalidos = new("Libro.Datos", "Los datos ingresados no son válidos.");
}
