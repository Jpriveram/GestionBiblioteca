using System.Text.RegularExpressions;

namespace ServicioMultas.Domain.Validations;

public static class ValidadorEntrada
{
    public static bool EstaVacio(string? valor) => string.IsNullOrWhiteSpace(valor);

    public static bool ExcedeLongitud(string? valor, int maximo)
    {
        if (valor == null) return false;
        return valor.Length > maximo;
    }

    public static string NormalizarEspacios(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return string.Empty;
        var trimmed = valor.Trim();
        return Regex.Replace(trimmed, "\\s+", " ");
    }

    public static string NormalizarAMayusculas(string? valor)
    {
        var normalizado = NormalizarEspacios(valor);
        return string.IsNullOrWhiteSpace(normalizado) ? string.Empty : normalizado.ToUpperInvariant();
    }

    public static bool ValidarMonto(decimal monto) => monto > 0;

    public static bool ValidarMotivo(string? motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo)) return false;
        var normalizado = NormalizarEspacios(motivo);
        return normalizado.Length >= 3 && normalizado.Length <= 500;
    }
}
