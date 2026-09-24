using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Todo lo que necesita un Alimentador (Art. 215) para calcularse. Misma forma que
/// <see cref="DatosEntradaCircuitoDerivadoNoMotor"/> a propósito -- es literalmente "un
/// circuito grande" -- pero sin TipoCarga: un alimentador no es Alumbrado ni
/// Contactos ni Fuerza, es la suma de lo que sea que alimenta del otro lado. Sin mínimo de
/// protección: el derivado tampoco tiene ya el piso de 15/20 A por tipo de carga (2026-09-24).
/// </summary>
public sealed record DatosEntradaAlimentador(
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

    /// <summary>
    /// Factor de demanda de la carga continua acumulada. <b>Aquí es donde va</b> — 220-40: la carga
    /// calculada de un alimentador es la suma de los derivados «después de aplicar cualquier factor
    /// de demanda aplicable». 1.0 = sin reducción.
    /// </summary>
    decimal FactorDemandaContinua,

    /// <summary>Factor de demanda de la carga no continua acumulada. Ver <see cref="FactorDemandaContinua"/>.</summary>
    decimal FactorDemandaNoContinua,

    /// <summary>
    /// <b>De qué lado del dispositivo final cae este tramo</b>, que es lo único que separa un
    /// alimentador de un circuito derivado — Artículo 100. <b>No cambia un solo número</b>: la
    /// fórmula es la misma. Cambia el artículo que la memoria cita, que es lo que se entrega firmado.
    ///
    /// <para>
    /// Por omisión, alimentador. Lo pone en <c>CircuitoDerivadoIndividual</c> la cascada cuando el
    /// destino es una <c>Carga</c>: ahí el interruptor del tablero <b>sí</b> es el final, porque
    /// después de él ya no hay más que el equipo.
    /// </para>
    /// </summary>
    ClaseDeTramo Clase = ClaseDeTramo.Alimentador,
    AgregadoMotores CargaMotores = default,
    string TipoAislamiento = "THHN",
    bool LugarInstalacionSeco = true,
    MetodoInstalacion MetodoInstalacion = MetodoInstalacion.CanalizacionOCable,

    // La excepción del 100 % -- ver CargaContinua100Pct.
    bool ConjuntoAprobado100Pct = false,
    bool? ModeloProteccionEsDe100Pct = null,

    /// <summary>
    /// Tope práctico de conductores en paralelo por fase que el motor puede alcanzar solo, cuando el
    /// catálogo se agota antes de cumplir la caída de tensión. <b>La norma no fija un máximo</b>: es
    /// criterio de diseño, y sale de <c>ConfiguracionProyecto.MaxConductoresParaleloAutomatico</c>.
    /// </summary>
    int MaxConductoresParaleloAutomatico = SeleccionConductor.MaxNParaleloAutoResueltoPorOmision,

    /// <summary>
    /// <b>Este tramo es una derivación de otro alimentador</b> — 240-21(b). Cambia dos cosas del
    /// dimensionamiento, y las dos las manda el texto de la norma:
    ///
    /// <list type="number">
    /// <item><b>Se apaga 240-4(b).</b> El encabezado de 240-21(b) es explícito: <i>«Las disposiciones
    /// de 240-4(b) no se deben permitir para conductores de derivación»</i>. Es el único lugar del
    /// motor donde el redondeo al siguiente tamaño estándar —que el bloque 8.6 enseñó a aplicar en
    /// todas partes— está prohibido.</item>
    /// <item><b>Se dimensiona contra la protección del alimentador PADRE, no.</b> Un conductor de
    /// derivación es, por definición, más chico que la protección aguas arriba: eso es lo que el
    /// inciso permite. Lo que lo protege son las fracciones (1/3, 1/10) y el dispositivo donde
    /// termina, que llegan por <see cref="PisoAmpacidadDerivacionA"/>.</item>
    /// </list>
    /// </summary>
    bool EsDerivacion240_21b = false,

    /// <summary>
    /// Ampacidad mínima que 240-21(b) le exige al conductor de derivación — la calcula
    /// <c>VerificadorDerivacion.AmpacidadMinimaExigidaA</c> con la protección del alimentador padre.
    /// Entra como piso de la capacidad mínima, así que el conductor sube si hace falta.
    /// </summary>
    decimal? PisoAmpacidadDerivacionA = null,
    /// <summary>
    /// El proyectista declara que las terminales del circuito —interruptor y equipo— están
    /// aprobadas e identificadas para 75 °C: 110-14(c)(1)a.(3). Solo cambia algo en 100 A o menos.
    /// Falso por omisión: sin declaración, 60 °C, que es la regla general.
    /// </summary>
    bool TerminalesMarcadas75C = false,

    /// <summary>
    /// <b>La corriente de cada fase</b>, sin factor de demanda — R-04. Con ella el alimentador se
    /// dimensiona con <b>la fase de mayor capacidad requerida</b> (215-2(a)(1), 215-3), no con la carga
    /// total repartida como si estuviera balanceado, y la caída de tensión se calcula fase por fase con
    /// el neutro (<see cref="CaidaPorFase"/>, R-02). <see cref="CargaContinuaVA"/> y
    /// <see cref="CargaNoContinuaVA"/> siguen siendo la carga total: la cita 220-40 habla de ella.
    /// <c>null</c> = el cálculo balanceado de siempre.
    /// </summary>
    IReadOnlyList<CorrienteDeFaseAlimentador>? CorrientesPorFase = null,

    /// <summary>El alimentador lleva neutro. Sin él (3F-3H) la caída se calcula balanceada con la fase que gobierna.</summary>
    bool ConNeutro = true);
