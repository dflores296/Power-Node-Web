namespace PowerNode.DesignSuite.Calculo.Magnitudes;

/// <summary>
/// Triángulo de potencias: S² = P² + Q². <b>P</b> (activa, watts) es la que hace
/// trabajo útil — luz, calor, movimiento. <b>Q</b> (reactiva, VAR) es la que un
/// inductor o capacitor intercambia con la fuente sin hacer trabajo neto (entra y
/// sale cada medio ciclo). <b>S</b> (aparente, VA) es la hipotenusa — la que en
/// verdad exige la fuente y la que dimensiona conductor, protección y transformador
/// (no P: un equipo con mal factor de potencia consume la misma P pero exige mucha
/// más S, y por eso jala más corriente de la que "su potencia en watts" sugiere).
/// FP = cos(ángulo) = P/S.
/// </summary>
public readonly record struct TrianguloPotencias
{
    public decimal ActivaW { get; }
    public decimal ReactivaVar { get; }

    private TrianguloPotencias(decimal activaW, decimal reactivaVar)
    {
        ActivaW = activaW;
        ReactivaVar = reactivaVar;
    }

    public decimal AparenteVA =>
        (decimal)Math.Sqrt((double)(ActivaW * ActivaW + ReactivaVar * ReactivaVar));

    public decimal FactorPotencia => AparenteVA == 0 ? 1m : ActivaW / AparenteVA;

    public decimal AnguloGrados =>
        (decimal)(Math.Atan2((double)ReactivaVar, (double)ActivaW) * 180.0 / Math.PI);

    /// <summary>
    /// <b>sen(θ) a partir del factor de potencia</b> — <c>sen θ = √(1 − cos²θ)</c>, la identidad
    /// pitagórica del propio triángulo.
    ///
    /// <para>
    /// Es el número que multiplica a la reactancia en la fórmula exacta de caída de tensión
    /// (<c>e = n·L·I·[R·cos θ + X·sen θ]</c>), y se calculaba <b>a mano en dos lugares</b> —el motor y
    /// la memoria— con guardas distintas: el motor sin proteger la raíz y la memoria sí. Un factor de
    /// potencia capturado arriba de 1 daba <c>NaN</c> en el cálculo y un número en el papel, o sea
    /// una memoria que decía otra cosa que el resultado. Auditoría 2026-08-19, §6.2.
    /// </para>
    ///
    /// <para>
    /// <b>Se topa a [0, 1]</b>: un cos θ fuera de ese rango no es un factor de potencia, y devolver
    /// <c>NaN</c> lo propaga hasta un calibre sin decir dónde nació. Es la guarda que ya tenía la
    /// memoria, ahora en los dos lados.
    /// </para>
    /// </summary>
    public static decimal SenoDelAngulo(decimal factorPotencia)
    {
        var cos = Math.Clamp(Math.Abs(factorPotencia), 0m, 1m);
        return (decimal)Math.Sqrt(Math.Max(0d, 1d - (double)(cos * cos)));
    }

    public static TrianguloPotencias DesdeActivaYReactiva(decimal activaW, decimal reactivaVar) =>
        new(activaW, reactivaVar);

    /// <summary>
    /// Reconstruye P y Q a partir de S y FP — el dato que normalmente sí se captura
    /// (placa de un motor, factor de potencia asumido de una carga). <paramref name="inductivo"/>
    /// indica si Q es positiva (carga inductiva típica: motores, balastros) o negativa
    /// (carga capacitiva: banco de capacitores).
    /// </summary>
    public static TrianguloPotencias DesdeAparenteYFactorPotencia(decimal aparenteVA, decimal factorPotencia, bool inductivo = true)
    {
        if (factorPotencia is < 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(factorPotencia), "El factor de potencia es cos(ángulo): debe estar entre 0 y 1.");

        var activa = aparenteVA * factorPotencia;
        var senoAngulo = (decimal)Math.Sqrt((double)(1m - factorPotencia * factorPotencia));
        var reactiva = aparenteVA * senoAngulo * (inductivo ? 1m : -1m);
        return new TrianguloPotencias(activa, reactiva);
    }

    /// <summary>Potencias de cargas distintas en el mismo circuito/tablero se suman componente por componente (P con P, Q con Q) — nunca S con S directo.</summary>
    public static TrianguloPotencias operator +(TrianguloPotencias a, TrianguloPotencias b) =>
        new(a.ActivaW + b.ActivaW, a.ReactivaVar + b.ReactivaVar);
}
