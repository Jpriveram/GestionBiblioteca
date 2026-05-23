using ServicioMultas.Domain.Common;

namespace ServicioMultas.Domain.Errors;

public static class MultaErrors
{
    public static readonly Error MontoInvalido = new("Multa.Monto", "El monto debe ser mayor a 0.");
    public static readonly Error MotivoMuyCorto = new("Multa.Motivo", "El motivo debe tener al menos 3 caracteres.");
    public static readonly Error MotivoMuyLargo = new("Multa.Motivo", "El motivo no puede exceder los 500 caracteres.");
    public static readonly Error UsuarioIdRequerido = new("Multa.UsuarioId", "El UsuarioId es obligatorio.");
    public static readonly Error NoEncontrada = new("Multa.Id", "La multa no fue encontrada.");
    public static readonly Error DatosObligatorios = new("Multa.Datos", "Completa todos los campos obligatorios.");
}
