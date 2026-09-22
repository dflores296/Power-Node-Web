using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

public readonly record struct ImpedanciaConductor(decimal ROhmKm, decimal XOhmKm);

/// <summary>
/// Tabla 9 — resistencia y reactancia en corriente alterna, 600V, 3 fases, 60 Hz. Es la que
/// alimenta la fórmula de caída de tensión: e = n·L·In·(R·cosθ + X·senθ). No es la resistencia de
/// CD de la Tabla 8 — esta ya trae el efecto piel y el material de la canalización.
/// </summary>
public interface ITablaImpedancia
{
    /// <summary>Null si el calibre no tiene valor para esa combinación (algunos calibres chicos no traen aluminio, por ejemplo).</summary>
    ImpedanciaConductor? Impedancia(Calibre calibre, MaterialConductor material, MaterialCanalizacion canalizacion);
}
