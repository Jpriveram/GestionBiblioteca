using System.Text.RegularExpressions;

namespace ServicioLibroEjemplar.Domain.Validations;

public static class ValidadorEntrada
{
    public static bool EstaVacio(string valor)
    {
        return string.IsNullOrWhiteSpace(valor);
    }

    public static bool ExcedeLongitud(string valor, int maximo)
    {
        if (valor == null)
        {
            return false;
        }

        return valor.Length > maximo;
    }

    public static string NormalizarEspacios(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        var trimmed = valor.Trim();
        return Regex.Replace(trimmed, "\\s+", " ");
    }

    public static string NormalizarAMayusculas(string? valor)
    {
        var normalizado = NormalizarEspacios(valor);
        if (string.IsNullOrWhiteSpace(normalizado))
        {
            return string.Empty;
        }

        return normalizado.ToUpperInvariant();
    }

    public static bool ISBNValido(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return true;
        }

        var limpio = isbn.Replace("-", string.Empty).Trim().ToUpperInvariant();

        if (limpio.Length == 13)
        {
            return limpio.All(char.IsDigit);
        }

        if (limpio.Length == 10)
        {
            for (int i = 0; i < 9; i++)
            {
                if (!char.IsDigit(limpio[i]))
                {
                    return false;
                }
            }

            return char.IsDigit(limpio[9]) || limpio[9] == 'X';
        }

        return false;
    }

    public static bool CodigoInventarioValido(string? codigoInventario)
    {
        if (string.IsNullOrWhiteSpace(codigoInventario))
        {
            return false;
        }

        foreach (char c in codigoInventario)
        {
            if (!char.IsLetterOrDigit(c) && c != '-')
            {
                return false;
            }
        }

        return true;
    }
}
