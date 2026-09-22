namespace PowerNode.DesignSuite.Calculo.Magnitudes;

/// <summary>
/// Impedancia Z = R + jX, en ohms. <b>R</b> (resistencia) es la parte real: disipa
/// potencia como calor. <b>X</b> (reactancia neta) es la parte imaginaria: intercambia
/// energía con un campo (magnético si es inductiva, eléctrico si es capacitiva) sin
/// disipar nada — por convención X = X_L − X_C (inductiva positiva, capacitiva resta).
/// El ángulo de Z es, precisamente, el desfase entre V e I cuando esa impedancia es
/// toda la carga: por eso cos(ángulo de Z) = factor de potencia de una carga puramente
/// pasiva (R+L o R+C), y de ahí sale FactorPotencia sin necesitar un dato aparte.
/// </summary>
public readonly record struct Impedancia
{
    public decimal ResistenciaOhms { get; }
    public decimal ReactanciaOhms { get; }

    public Impedancia(decimal resistenciaOhms, decimal reactanciaOhms)
    {
        ResistenciaOhms = resistenciaOhms;
        ReactanciaOhms = reactanciaOhms;
    }

    public decimal MagnitudOhms =>
        (decimal)Math.Sqrt((double)(ResistenciaOhms * ResistenciaOhms + ReactanciaOhms * ReactanciaOhms));

    public decimal AnguloGrados =>
        (decimal)(Math.Atan2((double)ReactanciaOhms, (double)ResistenciaOhms) * 180.0 / Math.PI);

    /// <summary>cos(ángulo de Z). Positivo por convención cuando la reactancia neta es inductiva (retraso de I respecto a V).</summary>
    public decimal FactorPotencia => ResistenciaOhms == 0 && ReactanciaOhms == 0
        ? 1m
        : (decimal)Math.Cos((double)AnguloGrados * Math.PI / 180.0);

    /// <summary>Impedancias en serie: se suman directo, componente por componente.</summary>
    public static Impedancia operator +(Impedancia a, Impedancia b) =>
        new(a.ResistenciaOhms + b.ResistenciaOhms, a.ReactanciaOhms + b.ReactanciaOhms);
}

/// <summary>
/// Fórmulas de reactancia a partir de los componentes físicos (inductancia/capacitancia)
/// y la frecuencia del sistema. La reactancia inductiva <b>crece</b> con la frecuencia
/// (un inductor se opone más a los cambios rápidos de corriente); la capacitiva
/// <b>decrece</b> con la frecuencia (un capacitor se "ve" más como corto a alta
/// frecuencia). Nunca son la misma magnitud ni se deben confundir/sumar con el signo
/// equivocado — por eso viven como dos fórmulas separadas y no una sola con un booleano.
/// </summary>
public static class Reactancia
{
    /// <summary>X_L = 2π·f·L. Reactancia inductiva, en ohms (f en Hz, L en henrios).</summary>
    public static decimal Inductiva(decimal frecuenciaHz, decimal inductanciaHenrios) =>
        2m * (decimal)Math.PI * frecuenciaHz * inductanciaHenrios;

    /// <summary>X_C = 1 / (2π·f·C). Reactancia capacitiva, en ohms (f en Hz, C en faradios).</summary>
    public static decimal Capacitiva(decimal frecuenciaHz, decimal capacitanciaFaradios)
    {
        if (frecuenciaHz == 0 || capacitanciaFaradios == 0)
            throw new ArgumentException("La reactancia capacitiva es infinita a frecuencia o capacitancia cero (un capacitor en DC estable es circuito abierto).");

        return 1m / (2m * (decimal)Math.PI * frecuenciaHz * capacitanciaFaradios);
    }
}
