using System.Text.RegularExpressions;
using PowerNode.DesignSuite.Calculo.TablasNom;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// Sección 240-6(a) — en la norma esto no es una Tabla, es una lista separada por comas dentro
/// del texto de la sección: "Los valores de corriente normalizados... son: 15, 16, 20, 25...".
/// </summary>
public partial class TablaProteccionEstandarJson : ITablaProteccionEstandar
{
    private const string SeccionId = "240-6(a)";
    private readonly List<decimal> _valores;

    public TablaProteccionEstandarJson(IFuenteTablas fuente)
    {
        // La única de las trece que no lee una rejilla: los valores estandarizados de 240-6(a) son
        // prosa ("...son: 15, 16, 20, 25..."), no una tabla, así que se parsean del texto.
        var texto = fuente.TextoDeSeccion(SeccionId);

        // El texto trae dos listas separadas en la misma sección: la general (esta, que termina en
        // "...5000 y 6000 amperes.") y una segunda oración con valores adicionales solo para
        // fusibles (1, 3, 6, 10 y 601) que no aplican a interruptores. Solo se toma la primera.
        var inicio = texto.IndexOf("son:", StringComparison.Ordinal) + "son:".Length;
        var fin = texto.IndexOf("amperes.", inicio, StringComparison.Ordinal);
        var listaTexto = fin > inicio ? texto[inicio..fin] : texto[inicio..];

        _valores = ExpresionNumeros().Matches(listaTexto)
            .Select(m => decimal.Parse(m.Value, System.Globalization.CultureInfo.InvariantCulture))
            .Distinct()
            .OrderBy(v => v)
            .ToList();

        if (_valores.Count == 0)
            throw new InvalidOperationException($"No se pudieron extraer valores estándar del texto de '{SeccionId}'.");
    }

    public IReadOnlyList<decimal> ValoresEstandar => _valores;

    public decimal SiguienteEstandar(decimal amperes)
    {
        foreach (var v in _valores)
            if (v >= amperes) return v;
        return _valores[^1];
    }

    public decimal? AnteriorEstandar(decimal amperes)
    {
        decimal? candidato = null;
        foreach (var v in _valores)
        {
            if (v > amperes) break;
            candidato = v;
        }
        return candidato;
    }

    [GeneratedRegex(@"\d+")]
    private static partial Regex ExpresionNumeros();
}
