using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Simbologia;

/// <summary>
/// Qué símbolo le toca a un renglón de <b>alumbrado</b>.
///
/// <para>
/// Es el hermano de <see cref="ClaveSimboloContacto"/> y existe por la misma razón: la NMX-J-136-ANCE
/// publica figuras propias para el luminario en pared —4.2.48 (LPI, interior) y 4.2.49 (LPE,
/// exterior)—, los dos arbotantes, y hasta el 2026-08-19 esos dibujos vivían en la librería del
/// programa <b>sin que nada pudiera producirlos</b>: no había campo que los pidiera.
/// </para>
///
/// <para>
/// <b>Aquí no hay precedencias ni caídas a otro símbolo</b>, a diferencia de los contactos: cada
/// valor del eje tiene su figura y no se cruza con nada más. Por eso esta clase es una sola función.
/// </para>
///
/// <para>
/// <b>Nada de esto entra en el cálculo.</b> Los VA salen de lo capturado en el renglón; el tipo de
/// luminaria es dato de plano y de nada más.
/// </para>
/// </summary>
public static class ClaveSimboloAlumbrado
{
    /// <summary>Salida de alumbrado genérica: techo o plafón. Es el caso normal y el valor por omisión.</summary>
    public const string General = "Alumbrado";

    /// <summary>Arbotante interior — NMX 4.2.48 (LPI).</summary>
    public const string ArbotanteInterior = "LuminariaParedInterior";

    /// <summary>Arbotante exterior — NMX 4.2.49 (LPE).</summary>
    public const string ArbotanteExterior = "LuminariaParedExterior";

    /// <summary>
    /// La clave del símbolo. <c>null</c> se lee como <see cref="General"/>: es lo que el programa
    /// dibujaba antes de que este eje existiera, así que ningún proyecto ya capturado cambia.
    /// </summary>
    public static string Para(TipoLuminaria? tipo) => (tipo ?? TipoLuminaria.General) switch
    {
        TipoLuminaria.ArbotanteInterior => ArbotanteInterior,
        TipoLuminaria.ArbotanteExterior => ArbotanteExterior,
        _ => General,
    };
}
