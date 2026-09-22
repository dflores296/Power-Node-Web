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
}
