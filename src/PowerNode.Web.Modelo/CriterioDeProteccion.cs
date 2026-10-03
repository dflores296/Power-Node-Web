using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>La protección de un derivado dentro de su rango: calculada o fijada</b> — M-20 e I-182, decisión
/// <c>proteccion-de-motores-por-rango.md</c>. 430-52(c)(1), 440-22(a), 440-4(b) y 110-3(b) piden un valor
/// «que no exceda»: es un techo. En un motor solo, un equipo de A/C (salvo el de habitación) y un variador;
/// un grupo sigue con el mayor que no excede su máximo. La pantalla solo ofrece los valores del rango: llega
/// con el calculado, y otro valor queda fijado (I-182, CONFIRMADA · David · 2026-10-03).
/// </summary>
public enum CriterioDeProteccion
{
    /// <summary>
    /// <b>Calculada</b>, la de un circuito nuevo. En un motor (David, 2026-10-03, pregunta 1: opción C): la
    /// mayor que protege al conductor hasta 1 HP, el máximo arriba; con la Excepción 2 declarada, el máximo. En
    /// un equipo de A/C y en un variador, el máximo (fase 2, CONFIRMADA): el de 440-22(a) está pensado para el
    /// arranque del motocompresor, y el de la placa o del fabricante lo marca quien hizo el equipo.
    /// </summary>
    Automatico,

    /// <summary>
    /// «cond.» de M-20. Solo para abrir un archivo de formato 12 que lo guardó: al abrir se convierte en
    /// fijada con el valor que daba, o en calculada si coincide (I-182). La pantalla ya no lo ofrece.
    /// </summary>
    Conductor,

    /// <summary>
    /// «máx.» de M-20, y el motor de un archivo de formato 11 o anterior, que abre en el máximo. Igual que
    /// <see cref="Conductor"/>: al abrir se convierte en fijada o en calculada.
    /// </summary>
    Maximo430_52,

    /// <summary><b>Fijada</b>: un valor del rango que escogió el proyectista.</summary>
    Manual,
}

/// <summary>Lo que se dice de la protección calculada, y cómo llega cada criterio al motor de cálculo.</summary>
public static class CriteriosDeProteccion
{
    /// <summary>
    /// Hasta aquí, la calculada protege al conductor. Es criterio, no norma (CONFIRMADO, pregunta 1): 430-32
    /// pide protección contra sobrecarga a los dos lados del corte —(a) arriba de 1 HP, (b) y (d)(1) abajo—.
    /// En un motor chico el interruptor que protege al conductor suele dejarlo arrancar, y el conductor queda
    /// con doble protección; arriba, el margen de arranque vale más.
    /// </summary>
    public const decimal CorteDelAutomaticoHp = 1m;

    /// <summary>El criterio que se calcula en un motor: la calculada se resuelve con los HP y la Excepción 2.</summary>
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

    /// <summary>El criterio que se calcula en un equipo de A/C o un variador: la calculada es el máximo.</summary>
    public static CriterioProteccionMotor ParaElCalculo(this CriterioDeProteccion criterio) =>
        criterio switch
        {
            CriterioDeProteccion.Conductor => CriterioProteccionMotor.Conductor,
            CriterioDeProteccion.Manual => CriterioProteccionMotor.Manual,
            _ => CriterioProteccionMotor.Maximo430_52,
        };

    /// <summary>
    /// <b>Por qué salió la calculada</b>, sin el valor: «el mayor que protege a 14 AWG (15 A) — 240-4; criterio
    /// para motores de 1 HP o menos». Para la ayuda de la celda, el desglose y la memoria — I-182.
    /// </summary>
    /// <param name="r">El rango calculado, con su criterio resuelto.</param>
    /// <param name="calibre">El calibre protegido, ya redactado con su ampacidad.</param>
    /// <param name="esMotor">Un motor (HP o A), no un equipo de A/C ni un variador.</param>
    /// <param name="noArrancaConLaTabla">El proyectista declaró la Excepción 2 de 430-52(c)(1).</param>
    public static string PorQueLaCalculada(RangoDeProteccion r, string calibre, bool esMotor, bool noArrancaConLaTabla) =>
        r.Criterio switch
        {
            CriterioProteccionMotor.Conductor when r.SubioElCalibre =>
                "el mínimo del rango: ninguno protegía al calibre por ampacidad, y el calibre sube hasta quedar protegido — 240-4; " +
                "criterio para motores de 1 HP o menos",
            CriterioProteccionMotor.Conductor =>
                $"el mayor que protege a {calibre} — 240-4" +
                (r.TopadoEn100A ? ", sin pasar de 100 A porque la terminal es de 60 °C — 110-14(c)(1)a." : "") +
                "; criterio para motores de 1 HP o menos",
            _ when !esMotor => r.Regla switch
            {
                "110-3(b)" => "la máxima que marca el fabricante del variador — 110-3(b)",
                "440-4(b)" => "la máxima que marca la placa — 440-4(b)",
                _ => $"el máximo de {r.Regla}, pensado para el arranque del motocompresor",
            },
            _ when noArrancaConLaTabla => "el máximo: el motor no arranca con la Tabla 430-52 (Excepción 2, declarada)",
            _ => "el máximo — criterio para motores de más de 1 HP",
        };

    /// <summary>«de la Tabla 430-52», «que marca la placa»: de dónde sale el máximo del rango.</summary>
    public static string DeDondeElMaximo(RangoDeProteccion r) => r.Regla switch
    {
        "110-3(b)" => "que marca el fabricante del variador (110-3(b))",
        "440-4(b)" => "que marca la placa (440-4(b))",
        "440-22(a)" => "de 440-22(a)",
        _ => "de la Tabla 430-52",
    };
}

/// <summary>
/// <b>Una protección fijada que regresó al cálculo</b> porque cambió su rango — I-182: otro equipo, la
/// Excepción 2 u otra serie. La pantalla lo avisa una vez.
/// </summary>
/// <param name="Espacio">El circuito.</param>
/// <param name="FijadaA">La que estaba fijada.</param>
/// <param name="MinimoA">El menor valor del rango nuevo.</param>
/// <param name="MaximoA">El mayor valor del rango nuevo.</param>
/// <param name="CalculadaA">La calculada con que quedó; <c>null</c> si el circuito ya no se pudo calcular.</param>
public sealed record ProteccionQueRegreso(int Espacio, decimal FijadaA, decimal MinimoA, decimal MaximoA, decimal? CalculadaA = null)
{
    /// <summary>
    /// El aviso, de una o de varias: «Circuito 3: cambió el rango de la protección (25 a 45 A); la fijada, 15 A,
    /// regresó al calculado, 45 A.»
    /// </summary>
    public static string Aviso(IReadOnlyList<ProteccionQueRegreso> regresaron) => regresaron switch
    {
        // Un rango de un solo valor (la Excepción 2 del 100 HP a 440 V) decía «(350 a 350 A)» — I-184.
        [var una] => $"Circuito {una.Espacio}: cambió el rango de la protección ({(una.MinimoA == una.MaximoA ? $"solo {una.MaximoA:N0}" : $"{una.MinimoA:N0} a {una.MaximoA:N0}")} A); " +
                     $"la fijada, {una.FijadaA:N0} A, regresó al calculado" + (una.CalculadaA is { } calc ? $", {calc:N0} A." : "."),
        _ => $"Circuitos {Lista(regresaron.Select(x => x.Espacio).ToList())}: cambió el rango de la protección; " +
             "las fijadas regresaron al calculado.",
    };

    private static string Lista(IReadOnlyList<int> espacios) =>
        espacios.Count == 1 ? $"{espacios[0]}" : $"{string.Join(", ", espacios.Take(espacios.Count - 1))} y {espacios[^1]}";
}
