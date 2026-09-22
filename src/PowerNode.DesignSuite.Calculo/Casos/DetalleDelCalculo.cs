namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Los números intermedios de un cálculo, tal como los produce el motor: los factores aplicados, la
/// capacidad mínima antes y después de corregir, la ampacidad del conductor elegido y la impedancia
/// con la que salió la caída de tensión.
///
/// <para>
/// <b>Es lo que vuelve verificable una memoria de cálculo.</b> Quien la revisa toma la corriente, los
/// factores y el calibre impresos, aplica la fórmula del artículo citado y tiene que llegar al mismo
/// número. Hasta el 2026-08-19 estos valores solo existían <b>dentro de una frase</b> del texto: el
/// motor los calculaba, los redactaba y los tiraba.
/// </para>
///
/// <para>
/// Va aparte del resultado y no como diez campos sueltos porque son <b>un solo concepto</b> —el
/// desglose del cálculo— y porque el mismo juego lo necesitan el circuito derivado y el alimentador.
/// </para>
/// </summary>
/// <param name="CapacidadMinimaCorregidaA">Icm = In / (FT × FA × hilos por fase).</param>
public sealed record DetalleDelCalculo(
    decimal CapacidadMinimaA,
    decimal FactorTemperatura,
    decimal FactorAgrupamiento,
    decimal CapacidadMinimaCorregidaA,
    decimal AmpacidadConductorA,
    int TemperaturaTerminalesC,
    int TemperaturaAislamientoC,
    decimal ResistenciaOhmKm,
    decimal ReactanciaOhmKm,
    decimal CaidaTensionV);
