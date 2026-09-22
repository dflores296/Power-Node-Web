namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Una combinación en serie <b>probada por el fabricante</b> — 240-86(b). No es una fórmula: es un
/// renglón de una tabla de ensayo, y por eso se transcribe en vez de calcularse.
/// </summary>
/// <param name="PrincipalesAdmitidos">Las abreviaturas de los interruptores de aguas arriba con los
/// que la combinación fue probada. <b>Solo esos</b>: la prueba es del par, no del derivado.</param>
public sealed record CombinacionSerie(
    decimal TensionMaximaV,
    decimal CapacidadCombinadaKa,
    IReadOnlyList<string> PrincipalesAdmitidos,
    string DerivadoAbreviatura,
    int Polos,
    decimal AmperesMin,
    decimal AmperesMax);

/// <summary>
/// <b>240-86 — Valores nominales en serie.</b>
///
/// <b>El hueco que cierra.</b> Hasta ahora la verificación de capacidad interruptiva del programa
/// comparaba la falla disponible contra los kA del interruptor <b>a secas</b>: si el derivado tenía
/// menos, decía "insuficiente". Eso deja fuera un caso que la norma <b>sí permite</b> y que se usa
/// todos los días — un derivado de menor capacidad detrás de un principal mayor, cuando el par está
/// probado y marcado.
///
/// <b>Lo que dice la norma</b> (240-86, textual): <i>"Cuando un interruptor automático se usa en un
/// circuito que tiene una corriente de falla disponible más alta que su capacidad nominal de
/// interrupción marcada, al estar conectado en el lado carga de un dispositivo de protección contra
/// sobrecorriente aceptable que tiene el mayor valor nominal, el interruptor automático debe
/// satisfacer los requisitos que se indican en <b>(a) o (b), y (c)</b>."</i>
///
/// Ese <b>"y (c)"</b> es lo que hace que esto no sea solo una tabla de consulta:
///
/// <list type="bullet">
/// <item><b>(a)</b> — selección bajo supervisión de ingeniería, en instalaciones existentes. <b>Fuera
/// de alcance</b>: es un dictamen de un ingeniero, no algo que un programa pueda conceder.</item>
/// <item><b>(b)</b> — <b>combinaciones probadas</b> y marcadas por el fabricante. Es lo que
/// representa <see cref="CombinacionSerie"/>, y el único camino que este motor evalúa.</item>
/// <item><b>(c)</b> — <b>contribución del motor</b>, y es un veto: <i>"Los valores nominales en serie
/// no se deben usar cuando: (1) Los motores están conectados en el lado carga del dispositivo de
/// sobrecorriente de mayor valor y en el lado línea del dispositivo de sobrecorriente con menor
/// valor. (2) La suma de las corrientes a plena carga del motor excede el 1 por ciento del valor de
/// interrupción del interruptor automático con el menor valor."</i></item>
/// </list>
///
/// <b>La (c)(2) es un número que este programa sí puede verificar</b>, y por eso vale la pena: la
/// cascada ya agrega la suma de FLC de los motores de la rama (<c>AgregadoMotores</c>). Un motor
/// aporta corriente a la falla mientras gira, y esa contribución entra <i>entre</i> los dos
/// dispositivos — justo donde la combinación probada no la vio.
///
/// <b>Esto RELAJA un veredicto de seguridad, así que va al revés que el resto de la casa:</b> aquí
/// no basta con "no hay dato" para conceder. Se concede <b>solo</b> con combinación probada explícita
/// y con (c) pasada; cualquier duda deja el veredicto estricto de antes. Y siempre con la cita del
/// marcado que exige 110-22, porque una combinación en serie sin marcar en campo <b>no cumple</b>.
/// </summary>
public static class CoordinacionSerie
{
    /// <param name="Aplica">Si la coordinación en serie viene al caso. <c>false</c> cuando el derivado
    /// ya aguanta la falla por sí solo — ahí no hay nada que evaluar.</param>
    /// <param name="Permitida">Solo <c>true</c> con combinación probada y (c) pasada.</param>
    public sealed record Resultado(bool Aplica, bool Permitida, string? Motivo, IReadOnlyList<Cita> Citas);

    private static readonly Resultado NoAplica = new(false, false, null, []);

    /// <param name="capacidadDerivadoKa">Los kA marcados en el interruptor de aguas abajo. <c>null</c>
    /// = no se conoce, y entonces no se concede nada.</param>
    /// <param name="motoresEntreLosDosDispositivos">240-86(c)(1): hay motores colgados entre el
    /// principal y el derivado.</param>
    /// <param name="sumaFlcMotoresA">240-86(c)(2): suma de corrientes a plena carga de esos motores.</param>
    public static Resultado Evaluar(
        decimal corrienteFallaKa,
        decimal? capacidadDerivadoKa,
        CombinacionSerie? combinacion,
        bool motoresEntreLosDosDispositivos = false,
        decimal sumaFlcMotoresA = 0m)
    {
        // Sin capacidad marcada no se evalúa nada: no se sabe si hace falta la serie ni contra qué
        // medir el 1 % de (c)(2).
        if (capacidadDerivadoKa is not decimal derivadoKa)
            return NoAplica;

        // El derivado aguanta solo. 240-86 es para cuando NO aguanta.
        if (derivadoKa >= corrienteFallaKa)
            return NoAplica;

        var citas = new List<Cita>();

        if (combinacion is null)
            return new Resultado(true, false,
                $"El derivado está marcado para {derivadoKa:0.##} kA y la falla disponible es {corrienteFallaKa:0.##} kA. " +
                "No hay una combinación en serie probada para este par, así que 240-86(b) no aplica y la capacidad " +
                "interruptiva sigue siendo insuficiente.", citas);

        if (combinacion.CapacidadCombinadaKa < corrienteFallaKa)
            return new Resultado(true, false,
                $"La combinación en serie probada llega a {combinacion.CapacidadCombinadaKa:0.##} kA y la falla disponible es " +
                $"{corrienteFallaKa:0.##} kA. Ni siquiera con la combinación alcanza.", citas);

        // ---- 240-86(c): el veto de la contribución del motor. Va DESPUÉS de confirmar la
        // combinación porque la norma dice "(a) o (b), Y (c)": (c) no sustituye a (b), la limita.
        if (motoresEntreLosDosDispositivos)
        {
            var motivo =
                "Hay motores conectados entre el interruptor principal y el derivado. 240-86(c)(1) prohíbe usar valores " +
                "nominales en serie en ese caso, sin importar que la combinación esté probada: el motor aporta corriente " +
                "de falla en un punto que el ensayo del fabricante no cubre.";
            citas.Add(new Cita("240-86(c)(1)", motivo));
            return new Resultado(true, false, motivo, citas);
        }

        var unoPorCiento = derivadoKa * 1000m * 0.01m;
        if (sumaFlcMotoresA > unoPorCiento)
        {
            var motivo =
                $"La suma de corrientes a plena carga de los motores ({sumaFlcMotoresA:0.##} A) excede el 1 % de la capacidad " +
                $"interruptiva del derivado ({derivadoKa:0.##} kA -> límite {unoPorCiento:0.##} A). 240-86(c)(2) prohíbe usar " +
                "valores nominales en serie en ese caso.";
            citas.Add(new Cita("240-86(c)(2)", motivo));
            return new Resultado(true, false, motivo, citas);
        }

        citas.Add(new Cita("240-86(b)",
            $"Combinación en serie probada: principal {string.Join("/", combinacion.PrincipalesAdmitidos)} con derivado " +
            $"{combinacion.DerivadoAbreviatura} de {combinacion.AmperesMin:0.##}–{combinacion.AmperesMax:0.##} A, " +
            $"{combinacion.Polos} polo(s), hasta {combinacion.CapacidadCombinadaKa:0.##} kA a {combinacion.TensionMaximaV:0.##} V. " +
            $"Cubre la falla de {corrienteFallaKa:0.##} kA aunque el derivado esté marcado para {derivadoKa:0.##} kA."));

        if (sumaFlcMotoresA > 0m)
            citas.Add(new Cita("240-86(c)(2)",
                $"Contribución de motores {sumaFlcMotoresA:0.##} A, dentro del 1 % permitido ({unoPorCiento:0.##} A)."));

        // El marcado NO es un trámite: sin él la combinación no cumple, por más que esté bien elegida.
        citas.Add(new Cita("110-22",
            "El equipo debe quedar marcado en campo: \"PRECAUCIÓN - SISTEMA COMBINADO EN SERIE. CORRIENTE NOMINAL ___ AMPERES. " +
            "SE REQUIEREN COMPONENTES DE REEMPLAZO IDÉNTICOS\"."));

        return new Resultado(true, true, null, citas);
    }
}
