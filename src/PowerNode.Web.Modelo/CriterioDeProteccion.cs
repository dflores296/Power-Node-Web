using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Cómo se escoge la protección de un derivado dentro de su rango</b> — M-20, decisión
/// <c>proteccion-de-motores-por-rango.md</c>. 430-52(c)(1), 440-22(a), 440-4(b) y 110-3(b) piden un valor
/// «que no exceda»: es un techo. En un motor solo, un equipo de A/C (salvo el de habitación) y un variador
/// (fase 2, David, 2026-10-03); un grupo sigue con el mayor que no excede su máximo.
/// </summary>
public enum CriterioDeProteccion
{
    /// <summary>
    /// El de un circuito nuevo. En un motor (David, 2026-10-03, pregunta 1: opción C): prioridad al conductor
    /// hasta 1 HP, el máximo arriba — el corte que ya hace 430-32 entre (a) y (b); con la Excepción 2
    /// declarada, el máximo. En un equipo de A/C y en un variador, el máximo (fase 2): el de 440-22(a) está
    /// pensado para el arranque del motocompresor, y el de la placa o del fabricante lo marca quien lo hizo.
    /// </summary>
    Automatico,

    /// <summary>El mayor valor del rango que protege al conductor — 240-4.</summary>
    Conductor,

    /// <summary>El mayor que no excede el máximo de la Tabla 430-52. Lo que hacía la app antes de M-20.</summary>
    Maximo430_52,

    /// <summary>Un valor fijo del rango, que escoge el proyectista.</summary>
    Manual,
}

/// <summary>Lo que se dice de cada criterio, y cómo llega al motor de cálculo.</summary>
public static class CriteriosDeProteccion
{
    /// <summary>
    /// Hasta aquí, el automático protege al conductor — 430-32(a) es «de más de 746 watts (1 hp)»; (b) y
    /// (d), «de 746 watts (1 hp) o menos».
    /// </summary>
    public const decimal CorteDelAutomaticoHp = 1m;

    /// <summary>El criterio que se calcula: el automático se resuelve con los HP del motor y la Excepción 2.</summary>
    public static CriterioProteccionMotor ParaElCalculo(this CriterioDeProteccion criterio, decimal hp, bool noArrancaConLaTabla) =>
        criterio switch
        {
            CriterioDeProteccion.Conductor => CriterioProteccionMotor.Conductor,
            CriterioDeProteccion.Maximo430_52 => CriterioProteccionMotor.Maximo430_52,
            CriterioDeProteccion.Manual => CriterioProteccionMotor.Manual,
            _ => hp <= CorteDelAutomaticoHp && !noArrancaConLaTabla
                ? CriterioProteccionMotor.Conductor
                : CriterioProteccionMotor.Maximo430_52,
        };

    /// <summary>El criterio que se calcula en un equipo de A/C o un variador: el automático es el máximo (fase 2).</summary>
    public static CriterioProteccionMotor ParaElCalculo(this CriterioDeProteccion criterio) =>
        criterio switch
        {
            CriterioDeProteccion.Conductor => CriterioProteccionMotor.Conductor,
            CriterioDeProteccion.Manual => CriterioProteccionMotor.Manual,
            _ => CriterioProteccionMotor.Maximo430_52,
        };

    /// <summary>Por qué el automático de un equipo de A/C o de un variador es el máximo.</summary>
    public static string PorQueAutomaticoSinCorte(RangoDeProteccion r, bool esVariador) =>
        esVariador ? "Automático: variador, la máxima del fabricante — 110-3(b)"
        : r.Regla == "440-4(b)" ? "Automático: equipo de A/C, la protección máxima de su placa — 440-4(b)"
        : "Automático: equipo de A/C, el máximo de 440-22(a), que está pensado para el arranque del motocompresor";

    /// <summary>«Prioridad al conductor», «Máximo 430-52», «Manual».</summary>
    public static string Nombre(this CriterioProteccionMotor criterio) => criterio switch
    {
        CriterioProteccionMotor.Conductor => "Prioridad al conductor",
        CriterioProteccionMotor.Manual => "Manual",
        _ => "Máximo 430-52",
    };

    /// <summary>Por qué el automático quedó en ese criterio, para la memoria y la ayuda.</summary>
    public static string PorQueAutomatico(decimal hp, bool noArrancaConLaTabla) =>
        noArrancaConLaTabla
            ? "Automático: el motor no arranca con la Tabla 430-52 (Excepción 2), así que el máximo"
            : hp <= CorteDelAutomaticoHp
                ? $"Automático: {MotoresEnHp.Texto(hp)} HP, 1 HP o menos (430-32(b)), prioridad al conductor"
                : $"Automático: {MotoresEnHp.Texto(hp)} HP, más de 1 HP (430-32(a)), máximo 430-52";

    /// <summary>Por qué se puede escoger, según la regla que pone el techo — M-20 y su fase 2.</summary>
    public static string PorQueSePuedeEscogerEn(RangoDeProteccion r) => r.Regla switch
    {
        "440-22(a)" =>
            "Equipo de A/C: 440-22(a) pide una protección que «no exceda» el 175 % de su corriente (225 % si se declara que no " +
            "arranca) — es un techo, no el valor obligatorio. Cumple cualquier valor de la serie entre el 125 % de la corriente (lo " +
            "que lleva el conductor, 440-32) y ese techo. Automático: el máximo, porque el techo está pensado para el arranque del " +
            "motocompresor. Con menos, verificar que el interruptor conduzca el arranque (440-22(a)). La sobrecarga la da el " +
            "protector del motocompresor (440-52).",
        "440-4(b)" =>
            "Equipo de A/C por placa: la protección «no debe exceder» la máxima que marca la placa (440-4(b)) — es un techo. Cumple " +
            "cualquier valor de la serie entre la ampacidad mínima de la placa y esa máxima. Automático: la máxima de placa. Con " +
            "menos, verificar que el interruptor conduzca el arranque del equipo (440-22(b)). La sobrecarga la da el protector del " +
            "motocompresor (440-52).",
        "110-3(b)" =>
            "Variador: la protección no debe exceder la máxima que marca su fabricante (110-3(b)) — es un techo. Cumple cualquier " +
            "valor de la serie entre el 125 % de la corriente de entrada (lo que lleva el conductor, 430-122(a)) y esa máxima. " +
            "Automático: la máxima del fabricante. Con menos, verificar en las instrucciones del variador que el interruptor le " +
            "sirve. La sobrecarga del motor la da el variador si así lo marca (430-124(a)).",
        _ => PorQueSePuedeEscoger,
    };

    /// <summary>
    /// <b>Por qué se puede escoger</b> (David, 2026-10-03: «hay que explicar por qué se permite
    /// seleccionar»). Va en la ayuda de la celda «Protec. (A)» de un motor.
    /// </summary>
    public const string PorQueSePuedeEscoger =
        "Motor: 430-52(c)(1) pide una protección «que no exceda» el valor de la Tabla 430-52 — es un techo, no el " +
        "valor obligatorio. Cumple cualquier valor de la serie entre el 125 % de la FLC (lo que lleva el conductor, " +
        "430-22) y ese techo. Automático: hasta 1 HP, el mayor que protege al conductor (240-4); arriba de 1 HP, el " +
        "máximo. Con menos del máximo, verificar que el interruptor soporte el arranque (430-52(b)). La sobrecarga " +
        "del motor la da su relevador o protector térmico, siempre (430-32).";
}
