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

    /// <summary>
    /// La lista <b>completa</b> de 240-6(a), aunque la tabla elija dentro de una serie más corta.
    ///
    /// <para>
    /// Existe desde el 2026-09-23 (Power Node Web, hallazgo I-29): la web deja escoger la serie de
    /// interruptores que se instala —centro de carga NEMA, riel DIN IEC— y elige el tamaño dentro de
    /// ella. Pero la excepción 240-4(b) habla del «siguiente valor nominal <b>estándar</b> superior»
    /// —el de la norma, no el del catálogo—, así que esa verificación no se puede hacer contra la serie
    /// recortada: en riel DIN aceptaría 63 A sobre un conductor de 55 A cuando la NOM solo deja
    /// subir a 60. Por omisión es la misma lista, y nada cambia.
    /// </para>
    /// </summary>
    IReadOnlyList<decimal> ValoresDeLaNorma => ValoresEstandar;

    /// <summary>El siguiente valor de la lista completa de 240-6(a). Ver <see cref="ValoresDeLaNorma"/>.</summary>
    decimal SiguienteDeLaNorma(decimal amperes) => SiguienteEstandar(amperes);
}
