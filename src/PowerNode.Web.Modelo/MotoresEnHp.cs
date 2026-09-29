using System.Globalization;
using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.DesignSuite.Normativa;

namespace PowerNode.Web.Modelo;

/// <summary>Cómo se captura un motor — I-74: por sus caballos, o por sus amperes si la placa no trae HP.</summary>
public enum CapturaDeMotor
{
    Hp,
    Amperes,

    /// <summary>
    /// Varios motores, o motores y otras cargas, en el circuito — 430-53, I-115. Cada uno es un aparato
    /// del desglose; solo en el circuito, nunca en un aparato.
    /// </summary>
    Grupo,

    /// <summary>
    /// Un motor con variador de velocidad — 430 Parte J, I-119: la corriente de entrada del variador y la
    /// protección máxima de su fabricante, no los HP del motor. Solo en el circuito.
    /// </summary>
    Variador,
}

/// <summary>
/// <b>Un motor marcado en amperes, llevado a la tabla</b> — 430-6(a)(1): «se debe asumir que su
/// potencia en caballos de fuerza es la correspondiente a los valores dados en las Tablas […],
/// interpolando si fuera necesario». Interpolados, esos caballos tienen en la tabla justo esa
/// corriente: <see cref="Amperes"/> es la FLC.
/// </summary>
/// <param name="Hp">Los caballos que le corresponden.</param>
/// <param name="DesdeHp">El renglón de la tabla de abajo; 0 si es menor que el motor más chico: ahí no
/// hay renglón de abajo, y se interpola desde 0 HP y 0 A.</param>
/// <param name="HastaHp">El renglón de arriba. Igual a <paramref name="DesdeHp"/> si la corriente es
/// la de un renglón.</param>
public sealed record MotorEnAmperes(decimal Amperes, decimal Hp, decimal DesdeHp, decimal DesdeA, decimal HastaHp, decimal HastaA)
{
    public bool EsDeUnRenglon => DesdeHp == HastaHp;
}

/// <summary>
/// <b>Un renglón de tipo Motor</b> — I-15, I-74. Se calcula como circuito derivado de un motor
/// (Art. 430): la corriente sale de la tabla (430-6(a)), el conductor al 125 % de ella (430-22) y la
/// protección con el porcentaje de la Tabla 430-52. Se captura en HP, o en amperes
/// (<see cref="DeAmperes"/>).
///
/// <para>
/// <b>Lo que no se captura sale del tablero</b>, y aquí está escrito una sola vez:
/// </para>
/// <list type="bullet">
/// <item><b>Monofásico o trifásico</b>: por los polos. 1 polo es fase-neutro; 2 polos, un motor
/// monofásico entre fases; 3 polos, trifásico. La Tabla 430-249 (dos fases, a 90°) no aplica a
/// estos sistemas.</item>
/// <item><b>La columna de la tabla</b>: la tensión del circuito. Las tablas dicen que sus corrientes
/// se permiten para sistemas de 110 a 120 V y de 220 a 240 V: 220 V se lee en la columna de 230 V.
/// 127 V tiene columna propia en la 430-248.</item>
/// <item><b>El dispositivo</b>: interruptor automático de tiempo inverso, que es lo que se monta en
/// un tablero. <b>El tipo de motor</b>: monofásico o de jaula de ardilla. Los dos dan 250 % en la
/// Tabla 430-52; rotor devanado y corriente continua (150 %) no caben en un tablero de derivados.</item>
/// </list>
/// </summary>
public static class MotoresEnHp
{
    /// <summary>
    /// Los caballos normalizados (NEMA) que traen las tablas. Cuáles se ofrecen en un renglón no lo
    /// decide esta lista: lo decide la tabla, que no trae, por ejemplo, un monofásico de 15 HP.
    /// </summary>
    public static readonly IReadOnlyList<decimal> Normalizados =
    [
        1m / 6m, 0.25m, 1m / 3m, 0.5m, 0.75m, 1m, 1.5m, 2m, 3m, 5m, 7.5m, 10m, 15m, 20m, 25m, 30m, 40m, 50m, 60m, 75m,
        100m, 125m, 150m, 200m, 250m, 300m, 350m, 400m, 450m, 500m,
    ];

    public static TipoAlimentacionMotor Alimentacion(int polos) =>
        polos == 3 ? TipoAlimentacionMotor.Trifasico : TipoAlimentacionMotor.Monofasico;

    /// <summary>El renglón de la Tabla 430-52.</summary>
    public static TipoMotor TipoDeMotor(int polos) =>
        polos == 3 ? TipoMotor.PolifasicoJaulaArdilla : TipoMotor.Monofasico;

    /// <summary>
    /// La tensión del circuito, en volts enteros: F-N en 1 polo, F-F en 2 y 3. Entera porque así se
    /// buscan las columnas: 208/√3 = 120.09 V quedaría fuera del intervalo de 110 a 120 V.
    /// </summary>
    public static decimal Tension(int polos, decimal tensionFaseNeutroV, decimal tensionFaseFaseV) =>
        Math.Round(polos == 1 ? tensionFaseNeutroV : tensionFaseFaseV, 0, MidpointRounding.AwayFromZero);

    /// <summary>«430-250».</summary>
    public static string Tabla(int polos) => TablaFlcMotorJson.TablaDe(Alimentacion(polos));

    /// <summary>
    /// «Monofásico 127 V», «Monofásico 220 V», «Trifásico 220 V»: cómo se alimenta un motor de estos
    /// polos — I-114. Dos polos a 220 V es monofásico (Tabla 430-248); la Tabla 430-249 es de sistemas
    /// de dos fases, que aquí no hay.
    /// </summary>
    public static string AlimentacionTexto(int polos, decimal tensionV) =>
        $"{(polos == 3 ? "Trifásico" : "Monofásico")} {tensionV:0} V";

    /// <summary>La corriente a plena carga de tabla, o <c>null</c> si la tabla no trae ese motor a esa tensión.</summary>
    public static decimal? Flc(ITablaFlcMotor tabla, decimal hp, int polos, decimal tensionV) =>
        tabla.CorrientePlenaCargaA(hp, Alimentacion(polos), tensionV);

    /// <summary>Los HP que la tabla trae para estos polos y esta tensión: los que ofrece el selector.</summary>
    public static IReadOnlyList<decimal> Disponibles(ITablaFlcMotor tabla, int polos, decimal tensionV) =>
        [.. Normalizados.Where(hp => Flc(tabla, hp, polos, tensionV) is not null)];

    /// <summary>
    /// <b>Los caballos de un motor marcado en amperes</b> — 430-6(a)(1), interpolando entre los dos
    /// renglones de la tabla que lo rodean. <c>null</c> si pasa del motor más grande que trae la tabla
    /// a esta tensión (no hay renglón de arriba), o si no es una corriente.
    /// </summary>
    public static MotorEnAmperes? DeAmperes(ITablaFlcMotor tabla, decimal amperes, int polos, decimal tensionV)
    {
        if (amperes <= 0m)
            return null;

        var (desdeHp, desdeA) = (0m, 0m);
        foreach (var hp in Disponibles(tabla, polos, tensionV))
        {
            var flc = Flc(tabla, hp, polos, tensionV)!.Value;
            if (amperes == flc)
                return new MotorEnAmperes(amperes, hp, hp, flc, hp, flc);
            if (amperes < flc)
                return new MotorEnAmperes(amperes, desdeHp + (amperes - desdeA) / (flc - desdeA) * (hp - desdeHp), desdeHp, desdeA, hp, flc);
            (desdeHp, desdeA) = (hp, flc);
        }
        return null;
    }

    /// <summary>
    /// «Tabla 430-250: 3 HP, 9.60 A; 5 HP, 15.20 A». Los dos renglones con los que se interpoló, para
    /// que el número se pueda encontrar en la norma.
    /// </summary>
    public static string Interpolacion(MotorEnAmperes m, int polos) =>
        m.EsDeUnRenglon
            ? $"Tabla {Tabla(polos)}: {Texto(m.Hp)} HP, {m.Amperes:N2} A"
            : m.DesdeHp == 0m
                ? $"menor que el motor más chico de la Tabla {Tabla(polos)} ({Texto(m.HastaHp)} HP, {m.HastaA:N2} A): se interpola desde 0 HP y 0 A"
                : $"Tabla {Tabla(polos)}: {Texto(m.DesdeHp)} HP, {m.DesdeA:N2} A; {Texto(m.HastaHp)} HP, {m.HastaA:N2} A";

    /// <summary>Por qué un motor en amperes no calcula: pasa del más grande de la tabla.</summary>
    public static string SinFilaEnAmperes(ITablaFlcMotor tabla, decimal amperes, int polos, decimal tensionV)
    {
        var tipo = polos == 3 ? "trifásico" : "monofásico";
        var disponibles = Disponibles(tabla, polos, tensionV);
        if (disponibles.Count == 0)
            return $"La {Fuente(polos, tensionV)} no trae motores {tipo}s a {tensionV:0} V: cambiar los polos.";
        var mayor = disponibles[^1];
        return $"Un motor {tipo} de {amperes:N2} A pasa del más grande de la {Fuente(polos, tensionV)}: {Texto(mayor)} HP, " +
               $"{Flc(tabla, mayor, polos, tensionV):N2} A. No se puede interpolar — 430-6(a)(1). Revisar la corriente o los polos.";
    }

    /// <summary>
    /// «Tabla 430-250, columna de 230 V (220 V: intervalo de 220 a 240 V)». Lo que hace falta para
    /// encontrar el número en la norma.
    /// </summary>
    public static string Fuente(int polos, decimal tensionV)
    {
        var tabla = $"Tabla {Tabla(polos)}";
        if (TablaFlcMotorJson.TensionDeColumna(Alimentacion(polos), tensionV) is not { } columna)
            return tabla;
        return columna == (int)tensionV
            ? $"{tabla}, columna de {columna} V"
            : $"{tabla}, columna de {columna} V ({tensionV:0} V: intervalo de {Intervalo(columna)})";
    }

    private static string Intervalo(int columna) => columna switch
    {
        115 => "110 a 120 V",
        230 => "220 a 240 V",
        460 => "440 a 480 V",
        _ => "550 a 600 V",
    };

    /// <summary>Por qué un motor no calcula: la tabla no lo trae. Con qué sí.</summary>
    public static string SinFila(ITablaFlcMotor tabla, decimal hp, int polos, decimal tensionV)
    {
        var tipo = polos == 3 ? "trifásico" : "monofásico";
        var disponibles = Disponibles(tabla, polos, tensionV);
        if (disponibles.Count == 0)
            return $"La {Fuente(polos, tensionV)} no trae motores {tipo}s a {tensionV:0} V: cambiar los polos o capturar la carga en VA, W o A.";
        return $"La {Fuente(polos, tensionV)} no trae un motor {tipo} de {Texto(hp)} HP a {tensionV:0} V: " +
               $"llega de {Texto(disponibles[0])} a {Texto(disponibles[^1])} HP. Cambiar los polos o el motor.";
    }

    /// <summary>«1/2», «1 1/2», «7 1/2», «10»: como se nombran los motores.</summary>
    public static string Texto(decimal hp)
    {
        var entero = decimal.Truncate(hp);
        var fraccion = hp - entero;
        string? texto = fraccion switch
        {
            < 0.01m => "",
            _ when Math.Abs(fraccion - 1m / 6m) < 0.01m => "1/6",
            _ when Math.Abs(fraccion - 0.25m) < 0.01m => "1/4",
            _ when Math.Abs(fraccion - 1m / 3m) < 0.01m => "1/3",
            _ when Math.Abs(fraccion - 0.5m) < 0.01m => "1/2",
            _ when Math.Abs(fraccion - 0.75m) < 0.01m => "3/4",
            _ => null,
        };
        return texto switch
        {
            null => hp.ToString("0.##", CultureInfo.InvariantCulture),
            "" => entero.ToString("0", CultureInfo.InvariantCulture),
            _ when entero == 0m => texto,
            _ => $"{entero.ToString("0", CultureInfo.InvariantCulture)} {texto}",
        };
    }
}
