namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Qué trae la placa de un equipo de A/C o refrigeración</b> — I-74, Art. 440. Son dos formas, y se
/// captura la que venga:
/// </summary>
public enum PlacaDeAireAcondicionado
{
    /// <summary>
    /// Ampacidad mínima de los conductores y protección máxima (MCA y MOCP) — 440-4(b). Es lo que trae
    /// un equipo con varios motores o de carga combinada: minisplit, paquete, condensadora. Por
    /// omisión, porque es lo más común en un tablero de derivados.
    /// </summary>
    AmpacidadYProteccion,

    /// <summary>
    /// La corriente de carga nominal del motocompresor y, si viene, la de selección del circuito
    /// derivado — 440-6(a). Un compresor suelto, una cámara de refrigeración.
    /// </summary>
    CorrienteNominal,
}

/// <summary>
/// <b>Un renglón de tipo A/C y refrigeración</b> — I-74. Se calcula como circuito derivado de un
/// motocompresor hermético (Art. 440), con la placa y no con los HP: la corriente de las tablas del
/// 430 no aplica (440-6(a)). El cálculo es <c>CalculadoraCircuitoDerivado440</c>; aquí, los textos.
/// </summary>
public static class AireAcondicionadoDePlaca
{
    /// <summary>Para el selector de unidad del renglón.</summary>
    public static string Nombre(this PlacaDeAireAcondicionado p) => p switch
    {
        PlacaDeAireAcondicionado.AmpacidadYProteccion => "MCA",
        _ => "A",
    };

    /// <summary>
    /// La corriente con la que el equipo entra al alimentador y a la caída: la mayor entre la de carga
    /// nominal y la de selección (440-6(a) y su Excepción 1), o la ampacidad mínima de placa, del lado
    /// seguro: ya trae el 25 % de su motor mayor. <c>null</c> sin placa.
    /// </summary>
    public static decimal? Corriente(CircuitoDelCuadro c) => c.PlacaAire switch
    {
        PlacaDeAireAcondicionado.AmpacidadYProteccion => c.AmpacidadMinimaA > 0m ? c.AmpacidadMinimaA : null,
        _ => c.CorrientePlacaA > 0m ? Math.Max(c.CorrientePlacaA, c.CorrienteSeleccionA ?? 0m) : null,
    };

    /// <summary>«MCA 18.00 A · MOCP 30 A», «Nominal 12.50 A · selección 14.00 A»: la placa en corto.</summary>
    /// <remarks>
    /// Lo de placa se captura: va como se capturó, con hasta dos decimales (I-49). Antes «MCA 18.00 A ·
    /// MOCP 30 A» mezclaba dos formatos en la misma celda — I-92.
    /// </remarks>
    public static string Texto(CircuitoDelCuadro c) => c.PlacaAire switch
    {
        PlacaDeAireAcondicionado.AmpacidadYProteccion => $"MCA {c.AmpacidadMinimaA:#,0.##} A · MOCP {c.ProteccionMaximaA:#,0.##} A",
        _ => $"Nominal {c.CorrientePlacaA:#,0.##} A" + (c.CorrienteSeleccionA is > 0m and { } s ? $" · selección {s:#,0.##} A" : ""),
    };
}
