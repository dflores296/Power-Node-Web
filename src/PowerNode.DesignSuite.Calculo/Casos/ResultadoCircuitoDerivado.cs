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
    IReadOnlyList<string>? AvisosCargaContinua = null,
    // Solo en un circuito con varios motores (430-53(c)(4), 440-22(b)): de dónde sale el límite de
    // la protección. Nació en Power Node Web — ver CalculadoraCircuitoDerivadoGrupo.
    DetalleDelGrupo? Grupo = null,
    // Solo en el derivado de un motor solo: el rango de 430-52(c)(1) y cómo se escogió la protección
    // dentro de él. Nació en Power Node Web (M-20) — ver CalculadoraCircuitoDerivadoMotor.
    RangoDeProteccionMotor? RangoMotor = null);
