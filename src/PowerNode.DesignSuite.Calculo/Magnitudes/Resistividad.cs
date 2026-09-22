namespace PowerNode.DesignSuite.Calculo.Magnitudes;

/// <summary>
/// Resistividad (ρ): propiedad <b>intrínseca del material</b> — no depende de la forma
/// ni del tamaño del conductor. Es distinta de la Resistencia (R), que sí depende de
/// longitud y área: R = ρ · L / A. Confundirlas es un error común porque comparten
/// unidad "ohm" en el nombre coloquial, pero ρ trae unidades de longitud extra
/// (Ω·m en SI puro). Las tablas de cableado (y la Tabla 9 de la norma) casi nunca
/// trabajan en Ω·m — usan Ω·mm²/m (o directamente Ω/km ya combinado con un calibre
/// específico), así que este tipo trae ambas unidades explícitas y una conversión
/// clara entre ellas, para que nunca se mezclen en silencio.
/// </summary>
public readonly record struct Resistividad
{
    /// <summary>Convención de tablas de cableado (NOM/IEC): ohm·mm² por metro.</summary>
    public decimal OhmMm2PorMetro { get; }

    private Resistividad(decimal ohmMm2PorMetro) => OhmMm2PorMetro = ohmMm2PorMetro;

    public static Resistividad DesdeOhmMm2PorMetro(decimal valor) => new(valor);

    /// <summary>1 Ω·m (SI puro) = 10⁶ Ω·mm²/m, porque 1 m² = 10⁶ mm².</summary>
    public static Resistividad DesdeOhmMetro(decimal ohmMetro) => new(ohmMetro * 1_000_000m);

    public decimal OhmMetro => OhmMm2PorMetro / 1_000_000m;

    /// <summary>R = ρ · L / A — resistencia de un conductor de longitud L (m) y área A (mm²).</summary>
    public decimal ResistenciaDe(decimal longitudM, decimal areaMm2)
    {
        if (areaMm2 <= 0)
            throw new ArgumentOutOfRangeException(nameof(areaMm2), "El área de un conductor real no puede ser cero o negativa.");

        return OhmMm2PorMetro * longitudM / areaMm2;
    }
}
