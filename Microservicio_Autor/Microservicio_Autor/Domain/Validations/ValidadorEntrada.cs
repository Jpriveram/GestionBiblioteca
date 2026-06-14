namespace Microservicio_Autor.Domain.Validations;

public static class ValidadorEntrada
{
    public static string NormalizarEspacios(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return string.Join(" ", value.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }

    public static string FormatearNombrePropio(string? value)
    {
        var texto = NormalizarEspacios(value);

        if (string.IsNullOrWhiteSpace(texto))
            return string.Empty;

        var palabras = texto
            .ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(CapitalizarPalabra);

        return string.Join(" ", palabras);
    }

    public static string? FormatearTextoOpcional(string? value)
    {
        var texto = FormatearNombrePropio(value);
        return string.IsNullOrWhiteSpace(texto) ? null : texto;
    }

    public static string? FormatearApellidoOpcional(string? value)
    {
        var texto = NormalizarEspacios(value);

        if (string.IsNullOrWhiteSpace(texto))
            return null;

        texto = CorregirApellidosCompuestos(texto);

        return FormatearNombrePropio(texto);
    }

    public static bool TextoValido(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        return NormalizarEspacios(value).Length <= maxLength;
    }

    public static bool SoloLetrasYEspacios(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var texto = NormalizarEspacios(value);

        return texto.All(c => char.IsLetter(c) || char.IsWhiteSpace(c));
    }

    public static bool TieneLetrasSeparadas(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var partes = NormalizarEspacios(value)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return partes.Any(parte => parte.Length == 1);
    }

    public static bool EsMayorDeEdad(DateTime fechaNacimiento)
    {
        var hoy = DateTime.Today;
        var edad = hoy.Year - fechaNacimiento.Year;

        if (fechaNacimiento.Date > hoy.AddYears(-edad))
            edad--;

        return edad >= 18;
    }

    private static string CapitalizarPalabra(string palabra)
    {
        if (string.IsNullOrWhiteSpace(palabra))
            return string.Empty;

        if (palabra.Length == 1)
            return palabra.ToUpperInvariant();

        return char.ToUpperInvariant(palabra[0]) + palabra[1..];
    }

    private static string CorregirApellidosCompuestos(string value)
    {
        var limpio = NormalizarEspacios(value);
        var partes = limpio.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var partesCorregidas = partes.Select(CorregirParteApellidoCompuesto);

        return string.Join(" ", partesCorregidas);
    }

    private static string CorregirParteApellidoCompuesto(string value)
    {
        var clave = value.ToLowerInvariant();

        return clave switch
        {
            "delarosa" => "De La Rosa",
            "delafuente" => "De La Fuente",
            "delacruz" => "De La Cruz",
            "delatorre" => "De La Torre",
            "delvalle" => "Del Valle",
            "delrio" => "Del Río",
            "delrío" => "Del Río",
            "delosrios" => "De Los Ríos",
            "delosríos" => "De Los Ríos",
            "delossantos" => "De Los Santos",
            "delcastillo" => "Del Castillo",
            "delcampo" => "Del Campo",
            "delpilar" => "Del Pilar",
            "delmonte" => "Del Monte",
            "delpozo" => "Del Pozo",
            _ => value
        };
    }
}