namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>Capacidad del transformador a la altitud del sitio, y por qué.</summary>
public sealed record ResultadoAltitud(
    decimal AltitudMsnm,
    decimal AltitudDisenoMsnm,
    decimal TemperaturaAmbienteC,
    decimal TemperaturaPermisibleC,
    decimal FactorCapacidad,
    decimal CapacidadCorregidaKva,
    bool HayDerrateo,
    string Explicacion);

/// <summary>
/// Derrateo de capacidad por altitud, según la <b>NMX-J-116</b> (5.1.3 y "Efecto de la altitud en la
/// elevación de la temperatura"). **No viene de la NOM-001-SEDE-2012**: se verificó que la NOM solo
/// menciona altitud en el Art. 922, para separaciones de líneas aéreas, no para capacidad.
///
/// La regla real tiene tres partes, y la primera implementación de este archivo se equivocaba en
/// las tres. Se corrigió cuando el usuario consiguió el texto de la norma:
///
/// <list type="number">
/// <item><b>La altitud de referencia no siempre es 1 000 m.</b> El 5.1.3 exige que un transformador
/// destinado a operar entre 1 000 y 2 300 m <i>se diseñe para 2 300 m</i>, y el derrateo se cuenta
/// "en exceso a los 1 000 m s.n.m. <i>o 2 300 m s.n.m. según sea el caso</i>". Un transformador
/// diseñado para 2 300 m instalado en la Ciudad de México (2 240 m) <b>no derratea nada</b>. La
/// versión anterior le quitaba casi 5 % de capacidad.</item>
///
/// <item><b>El porcentaje es único: 0,4 % por cada 100 m.</b> No cambia con el tipo de
/// enfriamiento. La versión anterior usaba 0,3 / 0,4 / 0,5 % según el enfriamiento, tomados de otra
/// norma y marcados entonces como "por confirmar".</item>
///
/// <item><b>El derrateo es condicional.</b> Textual: el transformador "puede operarse a capacidad
/// nominal, a mayores altitudes, siempre que la temperatura ambiente promedio máxima no exceda de
/// los valores indicados en la tabla 1". Solo si la excede se reduce la capacidad. La versión
/// anterior derrateaba siempre que se pasara de 1 000 m.</item>
/// </list>
/// </summary>
public static class CalculadoraAltitudTransformador
{
    /// <summary>Altitud de diseño mínima que exige el 5.1.3, incluso para sitios a nivel del mar.</summary>
    public const decimal AltitudDisenoBase = 1000m;

    /// <summary>La otra altitud de diseño normalizada, obligatoria para sitios de 1 000 a 2 300 m.</summary>
    public const decimal AltitudDisenoIntermedia = 2300m;

    /// <summary>Pérdida de capacidad por cada 100 m en exceso de la altitud de diseño.</summary>
    public const decimal PerdidaPorCada100mPct = 0.4m;

    /// <summary>
    /// Tabla 1: temperatura ambiente promedio permisible del aire refrigerante, en un período de
    /// 24 h, para operar a capacidad nominal a cada altitud.
    /// </summary>
    private static readonly (decimal Altitud, decimal TemperaturaC)[] Tabla1 =
        [(1000m, 30m), (2000m, 28m), (3000m, 25m), (4000m, 23m)];

    /// <summary>
    /// La altitud de diseño que exige el 5.1.3 para un sitio dado: 1 000 m hasta los 1 000 m,
    /// 2 300 m de ahí y hasta los 2 300, y la altitud misma más arriba ("para altitudes mayores
    /// debe especificarse y diseñarse para la [altitud de operación]").
    /// </summary>
    public static decimal AltitudDisenoRequerida(decimal altitudSitioMsnm) => altitudSitioMsnm switch
    {
        <= AltitudDisenoBase => AltitudDisenoBase,
        <= AltitudDisenoIntermedia => AltitudDisenoIntermedia,
        _ => altitudSitioMsnm,
    };

    /// <summary>
    /// Temperatura ambiente permisible a una altitud, interpolando linealmente entre los cuatro
    /// puntos de la Tabla 1.
    ///
    /// **La interpolación es una lectura, no algo que diga la norma:** la tabla solo da 1 000,
    /// 2 000, 3 000 y 4 000 m. Interpolar es la práctica normal con una tabla de este tipo y es
    /// más fiel que saltar de escalón, pero conviene saber que es interpretación. Fuera del rango
    /// se topa al extremo más cercano en vez de extrapolar.
    /// </summary>
    public static decimal TemperaturaPermisible(decimal altitudMsnm)
    {
        if (altitudMsnm <= Tabla1[0].Altitud)
            return Tabla1[0].TemperaturaC;
        if (altitudMsnm >= Tabla1[^1].Altitud)
            return Tabla1[^1].TemperaturaC;

        for (var i = 1; i < Tabla1.Length; i++)
        {
            var (altAlta, tempAlta) = Tabla1[i];
            if (altitudMsnm > altAlta)
                continue;

            var (altBaja, tempBaja) = Tabla1[i - 1];
            var avance = (altitudMsnm - altBaja) / (altAlta - altBaja);
            return Math.Round(tempBaja + avance * (tempAlta - tempBaja), 2);
        }

        return Tabla1[^1].TemperaturaC;
    }

    /// <param name="altitudDisenoMsnm">
    /// Para qué altitud se diseñó el transformador (1 000 o 2 300 m, normalmente). Si es null se
    /// asume la que exige el 5.1.3 para el sitio, que es lo que debería haberse especificado al
    /// comprarlo.
    /// </param>
    /// <param name="temperaturaAmbiente24hC">
    /// Temperatura ambiente promedio máxima del sitio en 24 h. Es la que se compara contra la
    /// Tabla 1 para decidir si hay derrateo.
    /// </param>
    public static ResultadoAltitud Calcular(
        decimal capacidadKva,
        decimal altitudMsnm,
        decimal temperaturaAmbiente24hC,
        decimal? altitudDisenoMsnm = null)
    {
        if (capacidadKva <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacidadKva), "La capacidad debe ser mayor a cero.");

        var diseno = altitudDisenoMsnm ?? AltitudDisenoRequerida(altitudMsnm);
        var permisible = TemperaturaPermisible(altitudMsnm);

        // Condición (a): a capacidad nominal mientras el ambiente no exceda la Tabla 1.
        if (temperaturaAmbiente24hC <= permisible)
        {
            return new ResultadoAltitud(
                altitudMsnm, diseno, temperaturaAmbiente24hC, permisible,
                FactorCapacidad: 1m, capacidadKva, HayDerrateo: false,
                $"A {altitudMsnm:N0} msnm la Tabla 1 permite hasta {permisible:N1} °C de promedio en 24 h, " +
                $"y el sitio está en {temperaturaAmbiente24hC:N1} °C. El transformador opera a capacidad " +
                "nominal (NMX-J-116, operación a capacidad nominal).");
        }

        // Condición (b): el exceso se cuenta desde la altitud de DISEÑO, no desde 1 000 m.
        var exceso = altitudMsnm - diseno;
        if (exceso <= 0)
        {
            return new ResultadoAltitud(
                altitudMsnm, diseno, temperaturaAmbiente24hC, permisible,
                FactorCapacidad: 1m, capacidadKva, HayDerrateo: false,
                $"El ambiente ({temperaturaAmbiente24hC:N1} °C) excede los {permisible:N1} °C que permite la " +
                $"Tabla 1, pero el transformador está diseñado para {diseno:N0} msnm y el sitio está a " +
                $"{altitudMsnm:N0}: no hay altitud en exceso que derratear.");
        }

        var perdidaPct = exceso / 100m * PerdidaPorCada100mPct;
        var factor = Math.Max(0.5m, 1m - perdidaPct / 100m);
        var corregida = Math.Round(capacidadKva * factor, 2);

        return new ResultadoAltitud(
            altitudMsnm, diseno, temperaturaAmbiente24hC, permisible, factor, corregida, HayDerrateo: true,
            $"El ambiente ({temperaturaAmbiente24hC:N1} °C) excede los {permisible:N1} °C que la Tabla 1 " +
            $"permite a {altitudMsnm:N0} msnm, así que aplica capacidad reducida. Sobre la altitud de diseño " +
            $"({diseno:N0} msnm) hay {exceso:N0} m de exceso: {PerdidaPorCada100mPct:N1} % por cada 100 m = " +
            $"{perdidaPct:N2} % menos. De {capacidadKva:N1} kVA a {corregida:N1} kVA (NMX-J-116, operación a " +
            "capacidad reducida).");
    }
}
