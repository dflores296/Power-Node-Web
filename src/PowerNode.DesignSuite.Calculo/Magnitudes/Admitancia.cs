namespace PowerNode.DesignSuite.Calculo.Magnitudes;

/// <summary>
/// Admitancia Y = 1/Z = G + jB, en siemens. Es el inverso de la impedancia, y sus dos
/// componentes son <b>conductancia</b> (G, parte real) y <b>susceptancia</b> (B, parte
/// imaginaria). Ojo con el error común: G = R/(R²+X²), <b>no</b> es simplemente 1/R —
/// eso solo es cierto en el caso particular de una impedancia puramente resistiva
/// (X = 0). Por eso este tipo se construye siempre desde una Impedancia completa (R y
/// X juntos), nunca desde R solo, para que no se cuele ese error por accidente.
/// </summary>
public readonly record struct Admitancia
{
    public decimal ConductanciaSiemens { get; }
    public decimal SusceptanciaSiemens { get; }

    private Admitancia(decimal conductanciaSiemens, decimal susceptanciaSiemens)
    {
        ConductanciaSiemens = conductanciaSiemens;
        SusceptanciaSiemens = susceptanciaSiemens;
    }

    public static Admitancia DesdeImpedancia(Impedancia z)
    {
        var denominador = z.ResistenciaOhms * z.ResistenciaOhms + z.ReactanciaOhms * z.ReactanciaOhms;
        if (denominador == 0)
            throw new ArgumentException("Una impedancia de cero ohms (cortocircuito ideal) tiene admitancia infinita; no se puede representar.");

        return new Admitancia(
            z.ResistenciaOhms / denominador,
            -z.ReactanciaOhms / denominador);
    }

    public decimal MagnitudSiemens =>
        (decimal)Math.Sqrt((double)(ConductanciaSiemens * ConductanciaSiemens + SusceptanciaSiemens * SusceptanciaSiemens));
}

/// <summary>
/// Conductividad (σ): el inverso de la resistividad (propiedad intrínseca del
/// material, como ρ — no de una pieza de conductor en particular). σ = 1/ρ.
/// </summary>
public readonly record struct Conductividad
{
    /// <summary>Siemens · metro / mm² — inverso directo de Resistividad.OhmMm2PorMetro.</summary>
    public decimal SiemensMetroPorMm2 { get; }

    private Conductividad(decimal siemensMetroPorMm2) => SiemensMetroPorMm2 = siemensMetroPorMm2;

    public static Conductividad DesdeResistividad(Resistividad rho)
    {
        if (rho.OhmMm2PorMetro == 0)
            throw new ArgumentException("Un material con resistividad cero (conductor ideal) tiene conductividad infinita; no se puede representar.");

        return new Conductividad(1m / rho.OhmMm2PorMetro);
    }
}
