using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>
/// Tabla 310-15(b)(16) — Ampacidades permisibles, base 30°C, hasta 3 conductores portadores de
/// corriente en canalización/cable/enterrado — y Tabla 310-15(b)(17), mismo criterio para
/// conductores individuales al aire libre (también base 30°C). Ver <see cref="MetodoInstalacion"/>.
/// </summary>
public interface ITablaAmpacidad
{
    /// <summary>Ampacidad del calibre en la columna de temperatura y material dados. Null si ese calibre no tiene valor en esa columna.</summary>
    decimal? Ampacidad(Calibre calibre, MaterialConductor material, TemperaturaAislamiento temperatura, MetodoInstalacion metodo = MetodoInstalacion.CanalizacionOCable);

    /// <summary>El calibre más chico cuya ampacidad (columna dada) sea &gt;= corrienteA.</summary>
    Calibre CalibrePorAmpacidad(decimal corrienteA, MaterialConductor material, TemperaturaAislamiento temperatura, MetodoInstalacion metodo = MetodoInstalacion.CanalizacionOCable);
}
