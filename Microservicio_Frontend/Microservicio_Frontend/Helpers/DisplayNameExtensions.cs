using System.Globalization;

namespace Frontend.Helpers;

public static class DisplayNameExtensions
{
    private static readonly TextInfo TextInfoEs = CultureInfo.GetCultureInfo("es-ES").TextInfo;

    public static string ToDisplayName(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var compactado = string.Join(' ', value
            .Trim()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

        return TextInfoEs.ToTitleCase(TextInfoEs.ToLower(compactado));
    }
}
