using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

public sealed record ResultadoCircuitoDerivado(
    decimal CorrienteDisenoA,
    decimal ProteccionA,
    Calibre CalibreFase,
    Calibre CalibreNeutro,
    Calibre CalibreTierra,
    decimal CaidaTensionPct,
    string TablaAmpacidadId,
    IReadOnlyList<Cita> Citas,
    int NumeroConductoresParalelo = 1,
    // Los números intermedios, para que la memoria se pueda recalcular. Ver DetalleDelCalculo.
    DetalleDelCalculo? Detalle = null,
    // Ver CargaContinua100Pct: van aparte de las citas porque no sustentan el número, dicen qué
    // revisar en campo.
    IReadOnlyList<string>? AvisosCargaContinua = null);
