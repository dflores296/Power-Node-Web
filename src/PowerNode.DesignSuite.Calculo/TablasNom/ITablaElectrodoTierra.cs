using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>
/// Tabla 250-66 — el conductor del electrodo de puesta a tierra por el tamaño del mayor conductor de
/// entrada a la acometida, o el área equivalente de los conductores en paralelo. La misma tabla fija el
/// mínimo del puente de unión principal — 250-28(d)(1). Nace en Power Node Web (auditoría del
/// 2026-09-29, P3-3); el escritorio no la lee.
/// </summary>
public interface ITablaElectrodoTierra
{
    /// <summary>
    /// El conductor del electrodo, de <paramref name="materialElectrodo"/>, para una acometida de
    /// <paramref name="areaAcometidaMm2"/> (la de un conductor, o la suma de los que van en paralelo) de
    /// <paramref name="materialAcometida"/>.
    /// </summary>
    Calibre CalibreMinimo(decimal areaAcometidaMm2, MaterialConductor materialAcometida, MaterialConductor materialElectrodo);
}
