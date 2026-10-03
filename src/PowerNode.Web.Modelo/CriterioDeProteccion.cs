using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Cómo se escoge la protección del derivado de un motor dentro de su rango</b> — M-20, decisión
/// <c>proteccion-de-motores-por-rango.md</c>. 430-52(c)(1) pide un valor «que no exceda» el de la Tabla
/// 430-52: es un techo. Solo en un motor solo (<see cref="CircuitoDelCuadro.EsMotorSolo"/>); A/C, grupos y
/// variador siguen con el mayor que no excede su máximo.
/// </summary>
public enum CriterioDeProteccion
{
    /// <summary>
    /// El de un circuito nuevo (David, 2026-10-03, pregunta 1: opción C): prioridad al conductor hasta 1 HP,
    /// el máximo arriba — el corte que ya hace 430-32 entre (a) y (b). Con la Excepción 2 declarada, el
    /// máximo: el motor no arranca con menos.
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
