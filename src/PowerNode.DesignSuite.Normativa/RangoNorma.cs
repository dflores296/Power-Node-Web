using System.Globalization;
using PowerNode.DesignSuite.Calculo.TablasNom;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// Un rango como los que usan las tablas de la norma para agrupar filas: "10 o menos", "11-15",
/// "41 y más". Los límites son inclusivos.
/// </summary>
internal sealed class RangoNorma(decimal min, decimal max)
{
    public bool Contiene(decimal valor) => valor >= min && valor <= max;

    /// <summary>La errata del rótulo que se aplicó para leer esta fila, o null. Ver <see cref="ErratasDeLaNorma"/>.</summary>
    public ErrataDeRotulo? Errata { get; private init; }

    /// <summary>
    /// Lee el rótulo de una fila. <b>Necesita la tabla</b> porque una errata de rótulo está atada a
    /// su tabla, no al texto suelto: el mismo "91-75" en otra tabla sería otra cosa.
    ///
    /// <para>
    /// <b>La errata de "91-75" vivía aquí, en un diccionario privado</b>, hasta el 2026-08-20. Se
    /// mudó a <see cref="ErratasDeLaNorma"/> porque eran dos capas de erratas con dos criterios
    /// distintos, y ésta era la floja: no verificaba nada y, sobre todo, <b>no llegaba al papel</b> —
    /// una memoria que se aparta del DOF sin decirlo no se puede verificar.
    /// </para>
    /// </summary>
    public static RangoNorma? Parsear(string texto, string tablaId)
    {
        var limpio = texto.Trim();
        if (limpio.Length == 0) return null;

        if (ErratasDeLaNorma.DelRotulo(tablaId, limpio) is { } errata)
            return new RangoNorma(errata.Min, errata.Max) { Errata = errata };

        if (limpio.Contains("o menos"))
        {
            var n = decimal.Parse(limpio.Split(' ')[0], CultureInfo.InvariantCulture);
            return new RangoNorma(decimal.MinValue, n);
        }

        if (limpio.Contains("y más") || limpio.Contains("y mas"))
        {
            var n = decimal.Parse(limpio.Split(' ')[0], CultureInfo.InvariantCulture);
            return new RangoNorma(n, decimal.MaxValue);
        }

        var partes = limpio.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (partes.Length == 2
            && decimal.TryParse(partes[0], NumberStyles.Number, CultureInfo.InvariantCulture, out var a)
            && decimal.TryParse(partes[1], NumberStyles.Number, CultureInfo.InvariantCulture, out var b)
            && a <= b)
        {
            return new RangoNorma(a, b);
        }

        return null;
    }
}
