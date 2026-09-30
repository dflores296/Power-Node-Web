namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>
/// <b>Tabla 220-12</b>: la carga mínima de alumbrado general por metro cuadrado, según el tipo de inmueble.
/// Nace en Power Node Web (M-14).
/// </summary>
public interface ITablaCargaUnitaria
{
    /// <summary>Los renglones de la tabla, en su orden: el tipo de inmueble y sus VA/m².</summary>
    IReadOnlyList<(string Inmueble, decimal VaPorM2)> Filas { get; }
}
