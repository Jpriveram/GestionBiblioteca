using Microservicio_Prestamo.Domain.Common;

namespace Microservicio_Prestamo.Domain.Errors;

public static class PrestamoErrors
{
    public static readonly Error MaxEjemplares = new("Prestamo.Ejemplares", "Máximo 5 ejemplares por préstamo.");
    public static readonly Error MaxActivos = new("Prestamo.Activos", "El lector ya tiene el máximo de préstamos activos (3).");
    public static readonly Error YaAnulado = new("Prestamo.Anulado", "El préstamo ya fue anulado.");
    public static readonly Error NoEncontrado = new("Prestamo.Id", "El préstamo no fue encontrado.");
    public static readonly Error EjemplarNoDisponible = new("Prestamo.Ejemplar", "Uno o más ejemplares no están disponibles.");
    public static readonly Error DatosObligatorios = new("Prestamo.Datos", "Complete todos los campos obligatorios.");
}
