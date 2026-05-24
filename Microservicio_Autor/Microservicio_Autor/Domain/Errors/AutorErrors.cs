using Microservicio_Autor.Domain.Common;

namespace Microservicio_Autor.Domain.Errors;

public static class AutorErrors
{
    public static readonly Error DatosObligatorios = new(
        "Autor.DatosObligatorios",
        "Los datos obligatorios del autor no fueron proporcionados.");

    public static readonly Error DatosInvalidos = new(
        "Autor.DatosInvalidos",
        "Los datos del autor no son válidos.");

    public static readonly Error AutorDuplicado = new(
        "Autor.AutorDuplicado",
        "Ya existe un autor registrado con los mismos datos.");
}