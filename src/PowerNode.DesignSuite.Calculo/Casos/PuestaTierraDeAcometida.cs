using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// <b>La puesta a tierra del equipo de acometida</b> — nace en Power Node Web (auditoría del 2026-09-29,
/// P3-3). Con el tablero como equipo de acometida, sus conductores de alimentación son los de entrada a
/// la acometida, y de ellos salen:
/// <list type="bullet">
/// <item>el <b>conductor del electrodo de puesta a tierra</b>, de la Tabla 250-66 por el mayor conductor de
/// acometida o el área equivalente de los que van en paralelo — 250-66. Si su única conexión es a
/// varilla, tubo o placa, esa porción no tiene que pasar de 6 AWG de cobre (4 AWG de aluminio) —
/// 250-66(a);</item>
/// <item>el <b>puente de unión principal</b>, no menor que la misma tabla; con conductores de más de
/// 557 mm² (1100 kcmil) de cobre o 887 mm² (1750 kcmil) de aluminio, no menor que el 12.5 % de su área —
/// 250-28(d)(1).</item>
/// </list>
/// Los dos del mismo material que la instalación. El de 12.5 % se toma sobre el área equivalente, como
/// la tabla y como 250-24(c)(1) para el conductor puesto a tierra.
/// </summary>
public static class PuestaTierraDeAcometida
{
    /// <summary>557 mm² (1100 kcmil) de cobre; 887 mm² (1750 kcmil) de aluminio.</summary>
    public static decimal UmbralDelPorcentajeMm2(MaterialConductor material) => material == MaterialConductor.Cobre ? 557m : 887m;

    public static ResultadoTierraDeAcometida Calcular(
        ITablaElectrodoTierra tabla, ICatalogoCalibres catalogo, Calibre fase, int conductoresPorFase, MaterialConductor material)
    {
        var n = Math.Max(1, conductoresPorFase);
        var area = fase.AreaMm2 * n;
        var areaTexto = n > 1 ? $"{n} × {fase.DesignacionConUnidad} = {area:0.##} mm² (área equivalente)" : $"{fase.DesignacionConUnidad} ({area:0.##} mm²)";

        var electrodo = tabla.CalibreMinimo(area, material, material);
        var citas = new List<Cita>
        {
            new("250-66", $"Conductor del electrodo de puesta a tierra: {electrodo.DesignacionConUnidad}, por el mayor conductor de acometida, {areaTexto} — Tabla 250-66."),
        };
        var limite = material == MaterialConductor.Cobre ? "6 AWG de cobre" : "4 AWG de aluminio";
        citas.Add(new("250-66(a)", $"Si su única conexión es a electrodos de varilla, tubo o placa, esa porción no tiene que pasar de {limite}."));

        var puente = electrodo;
        var porPorcentaje = area > UmbralDelPorcentajeMm2(material);
        if (porPorcentaje)
        {
            var minimo = 0.125m * area;
            var porArea = catalogo.BuscarPorAreaMinima(minimo);
            if (porArea.AreaMm2 > puente.AreaMm2)
                puente = porArea;
            citas.Add(new("250-28(d)(1)", $"Puente de unión principal: {puente.DesignacionConUnidad}, no menor que el 12.5 % de {area:0.##} mm² = {minimo:0.##} mm² ni que la Tabla 250-66."));
        }
        else
            citas.Add(new("250-28(d)(1)", $"Puente de unión principal: {puente.DesignacionConUnidad}, no menor que la Tabla 250-66."));

        return new ResultadoTierraDeAcometida(electrodo, puente, area, porPorcentaje, citas);
    }
}

/// <param name="ConductorElectrodo">El conductor del electrodo de puesta a tierra — Tabla 250-66.</param>
/// <param name="PuenteDeUnion">El puente de unión principal — 250-28(d)(1).</param>
/// <param name="AreaAcometidaMm2">El área con la que se entró a la tabla: la del conductor, o la equivalente en paralelo.</param>
/// <param name="PuentePorPorcentaje">El puente salió del 12.5 % del área.</param>
public sealed record ResultadoTierraDeAcometida(
    Calibre ConductorElectrodo, Calibre PuenteDeUnion, decimal AreaAcometidaMm2, bool PuentePorPorcentaje, IReadOnlyList<Cita> Citas);
