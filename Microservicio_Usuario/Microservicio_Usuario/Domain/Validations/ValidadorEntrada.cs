using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace ServicioUsuario.Domain.Validations;

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

    public static bool SoloLetras(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        foreach (char c in valor)
        {
            if (!char.IsLetter(c) && c != ' ')
            {
                return false;
            }
        }

        return true;
    }

    public static bool SoloLetrasYNumeros(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        foreach (char c in valor)
        {
            if (!char.IsLetterOrDigit(c) && c != ' ')
            {
                return false;
            }
        }

        return true;
    }

    public static bool CodigoInventarioValido(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return false;
        }

        foreach (char c in valor)
        {
            if (!char.IsLetterOrDigit(c) && c != '-')
            {
                return false;
            }
        }

        return true;
    }

    public static bool FechaNoFutura(DateTime? fecha)
    {
        if (!fecha.HasValue)
        {
            return true;
        }

        return fecha.Value.Date <= DateTime.Today;
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

    public static bool ValidYear(int? year, int minyear = 1000)
    {
        if (!year.HasValue)
        {
            return true;
        }

        int current = DateTime.Now.Year;
        return year.Value >= minyear && year.Value <= current;
    }

    public static bool ISBNValido(string? isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return true;
        }

        var limpio = isbn.Replace("-", "").Trim().ToUpper();

        if (limpio.Length == 13)
        {
            foreach (char c in limpio)
            {
                if (!char.IsDigit(c))
                {
                    return false;
                }
            }

            return true;
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

    public static bool IdiomaPermitido(string? idioma)
    {
        if (string.IsNullOrWhiteSpace(idioma))
        {
            return false;
        }

        var idiomaNormalizado = NormalizarEspacios(idioma);

        string[] idiomasValidos =
        {
            "Español",
            "Inglés",
            "Alemán",
            "Quechua",
            "Aymara",
            "Frances",
            "Chino Mandarin",
            "Portugues",
            "Italiano"
        };

        return idiomasValidos.Contains(idiomaNormalizado);
    }
}