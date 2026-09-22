using System.Globalization;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// Parseo de las celdas de las tablas de la norma. El texto viene tal cual del PDF original:
/// "-"/"––" para vacío, designaciones marcadas con "**", y HP en fracciones Unicode (½, ¾, 1⁄6...).
/// </summary>
internal static class NormaParsing
{
    private static readonly Dictionary<char, decimal> FraccionesUnicode = new()
    {
        ['¼'] = 0.25m,
        ['½'] = 0.5m,
        ['¾'] = 0.75m,
        ['⅓'] = 1m / 3m,
        ['⅔'] = 2m / 3m,
        ['⅙'] = 1m / 6m,
        ['⅚'] = 5m / 6m,
        ['⅛'] = 0.125m,
        ['⅜'] = 0.375m,
        ['⅝'] = 0.625m,
        ['⅞'] = 0.875m,
    };

    private const char BarraFraccion = '⁄'; // ⁄ FRACTION SLASH, distinta de '/' normal

    public static decimal? Decimal(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;
        var limpio = texto.Trim();
        if (limpio is "-" or "––" or "—" or "*") return null;
        limpio = limpio.TrimEnd('*', '¹', '²', '³');
        return decimal.TryParse(limpio, NumberStyles.Number, CultureInfo.InvariantCulture, out var v) ? v : null;
    }

    public static string DesignacionLimpia(string texto) => texto.Trim().TrimEnd('*');

    /// <summary>Hp con fracción: "2", "½", "1 ½", "1⁄6".</summary>
    public static decimal Hp(string texto)
    {
        var limpio = texto.Trim();

        var barraIdx = limpio.IndexOf(BarraFraccion);
        if (barraIdx > 0 && barraIdx < limpio.Length - 1)
        {
            var numerador = decimal.Parse(limpio[..barraIdx], CultureInfo.InvariantCulture);
            var denominador = decimal.Parse(limpio[(barraIdx + 1)..], CultureInfo.InvariantCulture);
            return numerador / denominador;
        }

        if (limpio.Length > 0 && FraccionesUnicode.TryGetValue(limpio[^1], out var fraccion))
        {
            var enteroTexto = limpio[..^1].Trim();
            var entero = enteroTexto.Length == 0 ? 0m : decimal.Parse(enteroTexto, CultureInfo.InvariantCulture);
            return entero + fraccion;
        }

        return decimal.Parse(limpio, CultureInfo.InvariantCulture);
    }
}
