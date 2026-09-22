namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>
/// Sección 240-6(a) — los valores de corriente normalizados para fusibles e interruptores de tiempo
/// inverso. En la norma esto vive como texto dentro de la sección, no como una Tabla estructurada.
/// </summary>
public interface ITablaProteccionEstandar
{
    IReadOnlyList<decimal> ValoresEstandar { get; }

    /// <summary>El primer valor estándar &gt;= amperes.</summary>
    decimal SiguienteEstandar(decimal amperes);

    /// <summary>
    /// El mayor valor estándar &lt;= amperes -- para los techos de protección que NO traen el
    /// permiso explícito de redondear hacia arriba (p.ej. Tabla 450-3(b): el 125% sí lo trae -- nota
    /// 1 -- pero 167%/300%/250% no lo mencionan, así que ahí el valor elegido no debe exceder el
    /// techo calculado). Null si ni el valor estándar más chico cabe (techo por debajo de él).
    /// </summary>
    decimal? AnteriorEstandar(decimal amperes);
}
