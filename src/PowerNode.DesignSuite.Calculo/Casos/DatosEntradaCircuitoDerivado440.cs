using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Todo lo que necesita el circuito derivado de un equipo de aire acondicionado o refrigeración con
/// motocompresor hermético (Art. 440) para calcularse. <b>Lo que viene de la placa</b> llega en una
/// de dos formas, según lo que traiga el equipo:
///
/// <list type="bullet">
/// <item><b>La corriente de carga nominal del motocompresor</b> —y la de selección del circuito
/// derivado, si viene marcada—: 440-6(a). Conductor al 125 % (440-32) y protección que no exceda
/// 175 %, o 225 % si no arranca (440-22(a)). Es lo que resuelve <see cref="CalculadoraCarga440"/>.</item>
/// <item><b>La ampacidad mínima del conductor y la protección máxima</b> que marca un equipo con
/// varios motores o de carga combinada: 440-4(b). La ampacidad ya la calculó el fabricante con la
/// Parte D (el 125 % del motor mayor incluido) y la protección no puede exceder la marcada.</item>
/// </list>
///
/// Con <see cref="AmpacidadMinimaPlacaA"/> se usa la segunda; si no, la primera.
/// </summary>
/// <param name="NumeroFases">1 (monofásico, F-N o entre fases) o 3. La caída se calcula con él.</param>
/// <param name="TensionFaseNeutroV">La tensión a la que está conectado un equipo monofásico: F-N en 1
/// polo, F-F en 2. La caída de un monofásico se mide contra ella.</param>
public sealed record DatosEntradaCircuitoDerivado440(
    int NumeroFases,
    decimal TensionFaseNeutroV,
    decimal TensionFaseFaseV,
    decimal LongitudM,
    int NumeroConductoresParalelo,
    int NumeroConductoresAgrupados,
    decimal TemperaturaAmbienteC,
    MaterialConductor MaterialConductor,
    MaterialCanalizacion MaterialCanalizacion,
    decimal FactorPotencia,
    decimal CaidaTensionMaxPct,
    string TipoAislamiento = "THHN",
    bool LugarInstalacionSeco = true,
    MetodoInstalacion MetodoInstalacion = MetodoInstalacion.CanalizacionOCable,
    int MaxConductoresParaleloAutomatico = SeleccionConductor.MaxNParaleloAutoResueltoPorOmision,
    bool TerminalesMarcadas75C = false,

    /// <summary>440-6(a): la corriente de carga nominal de la placa del equipo (o del motocompresor).</summary>
    decimal? CorrienteNominalPlacaA = null,

    /// <summary>440-6(a) Excepción 1: la corriente de selección del circuito derivado, si viene marcada.</summary>
    decimal? CorrienteSeleccionCircuitoA = null,

    /// <summary>
    /// El proyectista declara que la protección al 175 % no conduce la corriente de arranque: el
    /// techo sube a 225 % — 440-22(a). Declaración, no deducción: el programa no tiene el arranque.
    /// </summary>
    bool RequiereArranque = false,

    /// <summary>440-4(b): la ampacidad mínima de los conductores que marca la placa.</summary>
    decimal? AmpacidadMinimaPlacaA = null,

    /// <summary>440-4(b): el valor nominal máximo de la protección que marca la placa.</summary>
    decimal? ProteccionMaximaPlacaA = null)
{
    /// <summary>La placa trae ampacidad mínima y protección máxima (440-4(b)), no la corriente nominal.</summary>
    public bool EsPorAmpacidadYProteccion => AmpacidadMinimaPlacaA is not null;
}
