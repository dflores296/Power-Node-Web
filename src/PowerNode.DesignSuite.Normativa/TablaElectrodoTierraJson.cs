using System.Globalization;
using System.Text.RegularExpressions;
using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// Tabla 250-66. Columnas: 0..1 = acometida de cobre (mm², AWG o kcmil), 2..3 = de aluminio, 4..5 =
/// conductor del electrodo de cobre, 6..7 = de aluminio. Las de la acometida son intervalos en texto
/// («33.6 o menor», «42.4 o 53.5», «Más de 85.0 a 177», «Más de 557.38»): de cada renglón se lee el
/// tope en mm² —el número mayor—; el último, «Más de», no tiene tope.
/// </summary>
public partial class TablaElectrodoTierraJson(IFuenteTablas fuente, ICatalogoCalibres catalogo) : ITablaElectrodoTierra
{
    private const string TablaId = "250-66";
    private List<(decimal TopeCuMm2, decimal TopeAlMm2, string ElectrodoCu, string ElectrodoAl)>? _cache;

    private List<(decimal, decimal, string, string)> Filas()
    {
        if (_cache is not null) return _cache;

        var resultado = new List<(decimal, decimal, string, string)>();
        foreach (var f in fuente.FilasDatos(TablaId))
        {
            if (Tope(f.Texto(0)) is not { } cu || Tope(f.Texto(2)) is not { } al)
                continue;
            if (f.Texto(5) is not { Length: > 0 } electrodoCu || f.Texto(7) is not { Length: > 0 } electrodoAl)
                continue;
            resultado.Add((cu, al, NormaParsing.DesignacionLimpia(electrodoCu), NormaParsing.DesignacionLimpia(electrodoAl)));
        }

        if (resultado.Count == 0)
            throw new InvalidOperationException($"No se pudo leer la Tabla {TablaId}.");
        _cache = resultado;
        return _cache;
    }

    /// <summary>El tope del intervalo en mm²; <see cref="decimal.MaxValue"/> para «Más de …» sin «a». Null si no es un intervalo.</summary>
    private static decimal? Tope(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
            return null;
        var numeros = Numeros().Matches(texto).Select(m => decimal.Parse(m.Value, CultureInfo.InvariantCulture)).ToList();
        if (numeros.Count == 0)
            return null;
        var abierto = texto.TrimStart().StartsWith("Más de", StringComparison.OrdinalIgnoreCase) && numeros.Count == 1;
        return abierto ? decimal.MaxValue : numeros.Max();
    }

    public Calibre CalibreMinimo(decimal areaAcometidaMm2, MaterialConductor materialAcometida, MaterialConductor materialElectrodo)
    {
        foreach (var (topeCu, topeAl, electrodoCu, electrodoAl) in Filas())
        {
            // Los topes vienen redondeados (350 kcmil son 177.3 mm² y la tabla dice «177»; 2 AWG, 33.62 y
            // «33.6»): se comparan con 0.5 % de holgura, menos que la distancia entre dos calibres.
            var tope = materialAcometida == MaterialConductor.Cobre ? topeCu : topeAl;
            if (tope != decimal.MaxValue && areaAcometidaMm2 > tope * 1.005m)
                continue;
            var designacion = materialElectrodo == MaterialConductor.Cobre ? electrodoCu : electrodoAl;
            return catalogo.BuscarPorDesignacion(designacion)
                ?? throw new InvalidOperationException($"La Tabla {TablaId} referencia el calibre '{designacion}' que no está en el catálogo de la Tabla 8.");
        }

        throw new InvalidOperationException($"La Tabla {TablaId} no cubre una acometida de {areaAcometidaMm2} mm².");
    }

    [GeneratedRegex(@"\d+(\.\d+)?")]
    private static partial Regex Numeros();
}
