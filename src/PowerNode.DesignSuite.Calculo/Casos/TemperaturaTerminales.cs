using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// La columna de ampacidad que impone la terminal del equipo — 110-14(c)(1).
///
/// Vive aparte porque la usaban las tres calculadoras con la misma línea copiada, y **la línea
/// estaba mal en el borde**: decía <c>breaker &lt; 100</c> cuando el texto es
/// <i>"circuitos de <b>100 amperes o menos</b>"</i> (110-14(c)(1)a.) contra <i>"circuitos con un
/// valor nominal <b>mayor que</b> 100 amperes"</i> (110-14(c)(1)b.). Un circuito de exactamente
/// 100 A —valor estándar de 240-6(a), y de los más usados— se calculaba con la columna de 75 °C,
/// que sobrestima la ampacidad y puede dejar el conductor corto.
///
/// Tres calculadoras copiando una regla es tres lugares donde equivocarse; por eso ahora es una.
/// </summary>
public static class TemperaturaTerminales
{
    /// <summary>
    /// 100 A o menos → 60 °C; más de 100 A → 75 °C.
    ///
    /// <b>Lo que este método NO hace</b>, a propósito:
    /// <list type="bullet">
    /// <item>La otra mitad del inciso —"o marcadas para conductores 14 AWG a 1 AWG"— necesita el
    /// marcado del equipo, que no se captura. Ir por amperes es el criterio de siempre y es el
    /// conservador.</item>
    /// <item>110-14(c)(1)a.(4), que permite 75 °C en motores de diseño B, C, D o E aunque el
    /// circuito sea de 100 A o menos. No usarlo sobredimensiona, no incumple.</item>
    /// </list>
    /// </summary>
    public static TemperaturaAislamiento Para(decimal proteccionA) =>
        proteccionA <= 100m ? TemperaturaAislamiento.T60 : TemperaturaAislamiento.T75;

    /// <summary>
    /// Con la declaración del proyectista de que el equipo está <b>aprobado e identificado para
    /// 75 °C</b> — 110-14(c)(1)a.(3). Existe desde el 2026-09-23 (Power Node Web): los interruptores
    /// de centro de carga suelen venir marcados 60/75 °C, y sin esta declaración el programa
    /// siempre usaba la columna de 60 °C hasta 100 A.
    ///
    /// <list type="bullet">
    /// <item>Más de 100 A: 75 °C, como siempre — la declaración no cambia nada.</item>
    /// <item>100 A o menos, marcado 75 °C, con conductor de 75 °C o más: 75 °C.</item>
    /// <item>100 A o menos, marcado 75 °C, con conductor de 60 °C (TW): 60 °C. 110-14(c)(1)a.(1)
    /// permite siempre conductores de 60 °C en estas terminales, y la columna que manda es la más
    /// baja de las dos — 110-14(c). No es un rechazo: el conductor vale, con su ampacidad de 60 °C.</item>
    /// </list>
    /// </summary>
    /// <param name="aislamiento">La temperatura del aislamiento; <c>null</c> si no se reconoció (el llamador lo rechaza aparte).</param>
    public static TemperaturaAislamiento Para(decimal proteccionA, bool equipoMarcado75C, TemperaturaAislamiento? aislamiento)
    {
        if (proteccionA > 100m || !equipoMarcado75C)
            return Para(proteccionA);

        return aislamiento == TemperaturaAislamiento.T60 ? TemperaturaAislamiento.T60 : TemperaturaAislamiento.T75;
    }

    /// <summary>La cita de 110-14(c)(1) que dice de dónde salió la temperatura de la terminal.</summary>
    public static string Explicacion(decimal proteccionA, bool equipoMarcado75C, TemperaturaAislamiento terminal) =>
        proteccionA > 100m || !equipoMarcado75C
            ? $"Protección {proteccionA} A -> terminales a {(int)terminal}°C"
            : terminal == TemperaturaAislamiento.T75
                ? $"Protección {proteccionA} A, equipo aprobado e identificado para 75°C -> terminales a 75°C (110-14(c)(1)a.(3))"
                : $"Protección {proteccionA} A, equipo marcado 75°C pero conductor de 60°C -> se usa la columna de 60°C (110-14(c)(1)a.(1))";
}
