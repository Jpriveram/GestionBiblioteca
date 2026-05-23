namespace Microservicio_Autor.Domain.Validations;

public static class ValidadorEntrada
{
    public static string NormalizarEspacios(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Join(" ", value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static bool TextoValido(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return NormalizarEspacios(value).Length <= maxLength;
    }
}