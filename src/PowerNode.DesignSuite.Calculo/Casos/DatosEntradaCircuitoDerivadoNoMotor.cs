using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Todo lo que necesita un circuito derivado de Alumbrado o Contactos (Art. 210) para calcularse.
/// Las tensiones fase-neutro y fase-fase, y el número de fases que ocupa el circuito, ya vienen
/// resueltas por quien arma esto (el tablero conoce su sistema) — esta calculadora no necesita
/// saber si el sistema es estrella o delta, solo la tensión que le toca a este circuito.
/// </summary>
public sealed record DatosEntradaCircuitoDerivadoNoMotor(
    TipoCarga TipoCarga,
    decimal CargaContinuaVA,
    decimal CargaNoContinuaVA,
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
    decimal? PisoPracticoCalibreMm2,
    string TipoAislamiento = "THHN",
    bool LugarInstalacionSeco = true,
    MetodoInstalacion MetodoInstalacion = MetodoInstalacion.CanalizacionOCable,

    // La excepción del 100 % -- ver CargaContinua100Pct. Los dos van al final y con valor por
    // omisión a propósito: sin declaración y sin modelo elegido, el cálculo es exactamente el que
    // era antes del 2026-08-18.
    bool ConjuntoAprobado100Pct = false,
    bool? ModeloProteccionEsDe100Pct = null,

    /// <summary>
    /// Tope práctico de conductores en paralelo por fase que el motor puede alcanzar solo, cuando el
    /// catálogo se agota antes de cumplir la caída de tensión. <b>La norma no fija un máximo</b>: es
    /// criterio de diseño, y sale de <c>ConfiguracionProyecto.MaxConductoresParaleloAutomatico</c>.
    /// </summary>
    int MaxConductoresParaleloAutomatico = SeleccionConductor.MaxNParaleloAutoResueltoPorOmision,
    /// <summary>
    /// El proyectista declara que las terminales del circuito —interruptor y equipo— están
    /// aprobadas e identificadas para 75 °C: 110-14(c)(1)a.(3). Solo cambia algo en 100 A o menos.
    /// Falso por omisión: sin declaración, 60 °C, que es la regla general.
    /// </summary>
    bool TerminalesMarcadas75C = false);
