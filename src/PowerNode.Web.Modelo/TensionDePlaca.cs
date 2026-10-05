using PowerNode.DesignSuite.Calculo.Tableros;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>La tensión de placa de un equipo de A/C, o la de entrada de un variador</b> — I-189 (AM-11, CONFIRMADA · David ·
/// 2026-10-05). Hasta entonces el circuito nacía en 1 polo (127 V) y nadie lo decidía: un minisplit de placa 220 V
/// se calculaba a 127 V, colgado de una fase y del neutro. Ahora se captura, <b>sin valor por omisión</b>: si falta,
/// el circuito no se calcula y lo dice, como la protección máxima de placa. Los polos salen de ella.
/// </summary>
/// <remarks>
/// Son las tensiones nominales que traen las placas, no las del tablero: un equipo «208/230 V 1F» va entre dos fases
/// de un tablero de 208 a 240 V. Solo se ofrecen las que el tablero puede alimentar (<see cref="TensionesDePlaca.De"/>).
/// </remarks>
public enum TensionDePlaca
{
    /// <summary>127 V, monofásica, fase a neutro: 1 polo.</summary>
    V127Monofasica,

    /// <summary>208/230 V, monofásica, entre dos fases: 2 polos.</summary>
    V208a230Monofasica,

    /// <summary>220 V, trifásica: 3 polos, en un tablero trifásico de 208 a 240 V.</summary>
    V220Trifasica,

    /// <summary>440/460 V, trifásica: 3 polos, en un tablero de 440 a 480 V.</summary>
    V440a460Trifasica,

    /// <summary>
    /// 575/600 V, trifásica: 3 polos. La decisión nombra hasta 440/460 V; la app también calcula tableros de 600 V
    /// (I-196), y sin ella un equipo de A/C no tendría tensión que escoger.
    /// </summary>
    V575a600Trifasica,
}

public static class TensionesDePlaca
{
    /// <summary>Los polos del interruptor que alimenta un equipo de esa placa.</summary>
    public static int Polos(this TensionDePlaca t) => t switch
    {
        TensionDePlaca.V127Monofasica => 1,
        TensionDePlaca.V208a230Monofasica => 2,
        _ => 3,
    };

    /// <summary>«127 V 1F», «208/230 V 1F», «220 V 3F», «440/460 V 3F»: como en la placa.</summary>
    public static string Texto(this TensionDePlaca t) => t switch
    {
        TensionDePlaca.V127Monofasica => "127 V 1F",
        TensionDePlaca.V208a230Monofasica => "208/230 V 1F",
        TensionDePlaca.V220Trifasica => "220 V 3F",
        TensionDePlaca.V440a460Trifasica => "440/460 V 3F",
        _ => "575/600 V 3F",
    };

    /// <summary>El tablero puede alimentar un equipo de esa placa: tiene las fases (y el neutro) y la tensión.</summary>
    public static bool EsDe(this TensionDePlaca t, DatosDelTablero d) => t switch
    {
        TensionDePlaca.V127Monofasica => SistemaDelTablero.TieneNeutro(d.Sistema) && d.TensionFaseNeutroV is >= 100m and <= 140m,
        TensionDePlaca.V208a230Monofasica => d.MaximoPolos >= 2 && d.TensionFaseFaseV is >= 200m and <= 250m,
        TensionDePlaca.V220Trifasica => d.MaximoPolos >= 3 && d.TensionFaseFaseV is >= 200m and <= 250m,
        TensionDePlaca.V440a460Trifasica => d.MaximoPolos >= 3 && d.TensionFaseFaseV is >= 400m and <= 500m,
        _ => d.MaximoPolos >= 3 && d.TensionFaseFaseV is >= 550m and <= 620m,
    };

    /// <summary>Las que se ofrecen en este tablero, de menor a mayor.</summary>
    public static IReadOnlyList<TensionDePlaca> De(DatosDelTablero d) => [.. Enum.GetValues<TensionDePlaca>().Where(t => t.EsDe(d))];

    /// <summary>La de este tablero con esos polos; <c>null</c> si ninguna.</summary>
    public static TensionDePlaca? ConPolos(DatosDelTablero d, int polos) =>
        De(d).Where(t => t.Polos() == polos).Select(t => (TensionDePlaca?)t).FirstOrDefault();
}
