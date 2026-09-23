using PowerNode.DesignSuite.Calculo.TablasNom;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Qué familia de interruptores se va a instalar</b>, que decide de qué lista se escogen los
/// tamaños. La 240-6(a) de la NOM-001-SEDE-2012 mezcla dos familias en una sola lista: los de
/// centro de carga (15, 30, 35, 45, 60…) y los de riel DIN (16, 32, 63). Decisión de David del
/// 2026-09-23 — <c>docs/decisiones/serie-de-interruptores.md</c>.
///
/// <para>
/// <b>No es catálogo</b> (no rompe <c>sin-catalogo-square-d.md</c>): no dice qué modelos ni de qué
/// marca, solo qué tamaños de la propia lista de la norma existen en cada familia.
/// </para>
/// </summary>
public enum SerieDeInterruptores
{
    /// <summary>Enchufables o atornillables a la barra, tipo NEMA (QO, NQ, QP, BR…). Sin 16, 32 ni 63 A. Por omisión.</summary>
    CentroDeCargaNema,

    /// <summary>Modulares en riel de 35 mm, tipo IEC. Solo los tamaños de la NOM que existen ahí: 16 a 125 A.</summary>
    RielDinIec,

    /// <summary>La lista de la 240-6(a) tal cual, con las dos familias mezcladas.</summary>
    NomCompleta,
}

public static class SeriesDeInterruptores
{
    /// <summary>Los tamaños de la 240-6(a) que no existen en centro de carga: vienen de la serie IEC.</summary>
    public static readonly IReadOnlyList<decimal> SoloIec = [16m, 32m, 63m];

    /// <summary>Los tamaños de la 240-6(a) que existen en riel DIN (serie IEC 60898 hasta 125 A).</summary>
    public static readonly IReadOnlyList<decimal> RielDin = [16m, 20m, 25m, 32m, 40m, 50m, 63m, 80m, 100m, 125m];

    /// <summary>El mayor interruptor de riel DIN. Arriba de él, se usa el siguiente de la NOM y se avisa.</summary>
    public const decimal MaximoRielDinA = 125m;

    public static bool Admite(this SerieDeInterruptores serie, decimal amperes) => serie switch
    {
        SerieDeInterruptores.CentroDeCargaNema => !SoloIec.Contains(amperes),
        SerieDeInterruptores.RielDinIec => RielDin.Contains(amperes),
        _ => true,
    };

    public static string Nombre(this SerieDeInterruptores serie) => serie switch
    {
        SerieDeInterruptores.CentroDeCargaNema => "Centro de carga (NEMA)",
        SerieDeInterruptores.RielDinIec => "Riel DIN (IEC)",
        _ => "NOM completa (centro de carga y riel DIN)",
    };

    /// <summary>Lo que la memoria dice de la lista que se usó.</summary>
    public static string Explicacion(this SerieDeInterruptores serie) => serie switch
    {
        SerieDeInterruptores.CentroDeCargaNema =>
            "Centro de carga (NEMA): de la lista de 240-6(a) se omiten 16, 32 y 63 A, que no existen en esta familia. Criterio del proyectista.",
        SerieDeInterruptores.RielDinIec =>
            "Riel DIN (IEC): de la lista de 240-6(a) solo se usan 16, 20, 25, 32, 40, 50, 63, 80, 100 y 125 A, que son los que existen en esta familia. Criterio del proyectista.",
        _ => "Lista completa de 240-6(a).",
    };
}

/// <summary>
/// La 240-6(a) vista a través de una serie: <b>elige</b> dentro de la serie, pero la verificación de
/// 240-4(b) sigue contra la lista completa (<see cref="ITablaProteccionEstandar.ValoresDeLaNorma"/>).
/// Si ningún tamaño de la serie alcanza —un principal de 150 A en riel DIN—, devuelve el siguiente de
/// la NOM: mejor un número que se puede comprar en otra familia que ninguno.
/// </summary>
public sealed class ProteccionEstandarDeLaSerie : ITablaProteccionEstandar
{
    private readonly ITablaProteccionEstandar _nom;
    private readonly List<decimal> _serie;

    public ProteccionEstandarDeLaSerie(ITablaProteccionEstandar nom, SerieDeInterruptores serie)
    {
        _nom = nom;
        _serie = [.. nom.ValoresEstandar.Where(v => serie.Admite(v))];
    }

    public IReadOnlyList<decimal> ValoresEstandar => _serie;

    public IReadOnlyList<decimal> ValoresDeLaNorma => _nom.ValoresEstandar;

    public decimal SiguienteDeLaNorma(decimal amperes) => _nom.SiguienteEstandar(amperes);

    public decimal SiguienteEstandar(decimal amperes)
    {
        foreach (var v in _serie)
            if (v >= amperes) return v;
        return _nom.SiguienteEstandar(amperes);
    }

    public decimal? AnteriorEstandar(decimal amperes)
    {
        decimal? candidato = null;
        foreach (var v in _serie)
        {
            if (v > amperes) break;
            candidato = v;
        }
        return candidato;
    }
}
