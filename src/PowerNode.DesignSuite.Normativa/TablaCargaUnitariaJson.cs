using System.Globalization;
using System.Text.RegularExpressions;
using PowerNode.DesignSuite.Calculo.TablasNom;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// Tabla 220-12 — M-14. Columnas: 0=tipo del inmueble, 1=carga unitaria en VA/m², a veces con la
/// llamada de su nota («39 (b)»). Los renglones sin número (el título del bloque de áreas comunes) no
/// se leen. Nace en Power Node Web.
/// </summary>
public partial class TablaCargaUnitariaJson(IFuenteTablas fuente) : ITablaCargaUnitaria
{
    private const string TablaId = "220-12";

    private IReadOnlyList<(string, decimal)>? _cache;

    [GeneratedRegex(@"^\s*(\d+(?:\.\d+)?)")]
    private static partial Regex Numero();

    public IReadOnlyList<(string Inmueble, decimal VaPorM2)> Filas => _cache ??= Leer();

    private List<(string, decimal)> Leer()
    {
        var filas = new List<(string, decimal)>();
        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var inmueble = f.Texto(0)?.Trim();
            var m = Numero().Match(f.Texto(1) ?? "");
            if (string.IsNullOrWhiteSpace(inmueble) || !m.Success)
                continue;
            filas.Add((inmueble, decimal.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)));
        }
        if (filas.Count == 0)
            throw new InvalidOperationException("La Tabla 220-12 no trae renglones con carga unitaria.");
        return filas;
    }
}
