namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// <b>De qué lado del dispositivo final de sobrecorriente cae un tramo de cable</b>, que es lo único
/// que la NOM usa para distinguir un alimentador de un circuito derivado. Artículo 100:
///
/// <list type="bullet">
/// <item><b>Alimentador:</b> los conductores entre la fuente y <i>el dispositivo final de protección
/// contra sobrecorriente del circuito derivado</i>.</item>
/// <item><b>Circuito derivado:</b> los conductores <i>desde ese dispositivo final</i> hasta las
/// salidas.</item>
/// </list>
///
/// <para>
/// <b>No es geometría ni tamaño: es qué hay aguas abajo.</b> Un mismo interruptor, en un mismo
/// espacio de un mismo tablero, arranca un alimentador si lo que sigue trae sus propias protecciones
/// (otro tablero, un CCM) y un circuito derivado si lo que sigue son salidas o un equipo de
/// utilización.
/// </para>
///
/// <para>
/// <b>Por qué existe este enum (2026-08-20).</b> El programa calculaba con la misma metodología los
/// dos casos —y está bien, porque <b>la fórmula es idéntica</b>: 125 % de la carga continua más
/// 100 % de la no continua, lo diga el 215 o lo diga el 210—, pero <b>citaba el 215 siempre</b>.
/// Así, el cable que llega a un chiller colgado directo del tablero salía en la memoria como
/// alimentador, cuando por definición es un <b>circuito derivado individual</b>: ese interruptor sí
/// es el dispositivo final, porque después de él ya no hay más que el equipo.
/// </para>
///
/// <para>
/// <b>El número no cambia; el artículo sí</b>, y el artículo es justamente lo que se entrega
/// firmado. Una memoria que sustenta un circuito derivado citando el artículo de alimentadores no
/// está mal por poco: está citando algo que no aplica.
/// </para>
/// </summary>
public enum ClaseDeTramo
{
    /// <summary>De la fuente al dispositivo final. Artículo 215.</summary>
    Alimentador,

    /// <summary>
    /// Del dispositivo final a un solo equipo de utilización. Artículo 210, y el 100 lo llama
    /// <i>circuito derivado individual</i>.
    ///
    /// <para>
    /// <b>Sin el piso de 15/20 A</b>, y eso no es un olvido: el <b>210-3</b> clasifica por el
    /// dispositivo de sobrecorriente y fija el rango de 15 a 50 A <i>«para los circuitos derivados
    /// que no sean individuales»</i>. Un individual se dimensiona por su equipo.
    /// </para>
    /// </summary>
    CircuitoDerivadoIndividual,
}

/// <summary>Los artículos que cita cada clase de tramo. Mismo cálculo, distinta referencia.</summary>
public static class ArticulosDelTramo
{
    /// <summary>El de la corriente de diseño: 215-2 para un alimentador, 210-19(a)(1) para un derivado.</summary>
    public static string CorrienteDeDiseno(this ClaseDeTramo clase) =>
        clase == ClaseDeTramo.Alimentador ? "215-2" : "210-19(a)(1)";

    /// <summary>El de la capacidad mínima del conductor: 215-3 / 210-20(a).</summary>
    public static string CapacidadMinima(this ClaseDeTramo clase) =>
        clase == ClaseDeTramo.Alimentador ? "215-3" : "210-20(a)";

    /// <summary>El de la excepción del ensamble al 100 %, en su versión de cada artículo.</summary>
    public static string Excepcion100Pct(this ClaseDeTramo clase) =>
        clase == ClaseDeTramo.Alimentador ? "215-2(a)(1)" : "210-19(a)(1)";
}
