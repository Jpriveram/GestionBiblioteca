using ServicioLibroEjemplar.Domain.Common;

namespace ServicioLibroEjemplar.Domain.Errors;

public static class EjemplarErrors
{
    public static readonly Error EjemplarNoEncontrado = new("Ejemplar.Id", "El ejemplar no existe.");
    public static readonly Error CodigoInventarioDuplicado = new("Ejemplar.CodigoInventario", "Ya existe un ejemplar registrado con ese código de inventario.");
    public static readonly Error DatosObligatorios = new("Ejemplar.Datos", "Completa todos los campos obligatorios.");
    public static readonly Error DatosInvalidos = new("Ejemplar.Datos", "Los datos ingresados no son válidos.");
}
