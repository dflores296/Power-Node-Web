using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Todo lo que necesita un circuito de Fuerza (Art. 430) para calcularse.
/// TensionNominalMotorV es la clase de placa del motor (para las tablas 430-247/248/249/250 y
/// 430-52) -- distinta de TensionFaseNeutroV/TensionFaseFaseV, que es la tensión REAL del tablero
/// y se usa solo para la caída de tensión. Ver el docstring de DatosMotor.
/// </summary>
public sealed record DatosEntradaCircuitoDerivadoMotor(
    decimal Hp,
    TipoAlimentacionMotor TipoAlimentacion,
    TipoMotor TipoMotor,
    TipoDispositivoProteccionMotor TipoDispositivoProteccion,
    decimal TensionNominalMotorV,
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

    /// <summary>
    /// Tope práctico de conductores en paralelo por fase que el motor puede alcanzar solo, cuando el
    /// catálogo se agota antes de cumplir la caída de tensión. <b>La norma no fija un máximo</b>: es
    /// criterio de diseño, y sale de <c>ConfiguracionProyecto.MaxConductoresParaleloAutomatico</c>.
    /// </summary>
    int MaxConductoresParaleloAutomatico = SeleccionConductor.MaxNParaleloAutoResueltoPorOmision,

    /// <summary>
    /// El proyectista declara que las terminales del circuito —interruptor y equipo— están
    /// aprobadas e identificadas para 75 °C: 110-14(c)(1)a.(3). Solo cambia algo en 100 A o menos.
    /// Falso por omisión: sin declaración, 60 °C, que es la regla general. Igual que en
    /// <see cref="DatosEntradaCircuitoDerivadoNoMotor"/> (M-06); aquí faltaba — I-15.
    /// </summary>
    bool TerminalesMarcadas75C = false,

    /// <summary>
    /// <b>Un motor marcado en amperes y no en caballos</b> — 430-6(a)(1): «se debe asumir que su
    /// potencia en caballos de fuerza es la correspondiente a los valores dados en las Tablas
    /// 430-247, 430-248, 430-249 y 430-250, interpolando si fuera necesario». Interpolado, el motor de
    /// esos caballos tiene en la tabla justo esa corriente: es la FLC. <see cref="Hp"/> lleva los
    /// caballos que resultaron, solo para la cita. <c>null</c> = motor en HP, FLC de la tabla
    /// (Power Node Web, I-74).
    /// </summary>
    decimal? FlcMarcadaEnAmperesA = null,

    /// <summary>
    /// <b>Un motor de servicio no continuo</b> — 430-22(e): el conductor va al porcentaje de la Tabla
    /// 430-22(e) sobre la corriente de <b>placa</b>, no al 125 % de la FLC. La protección sigue en
    /// 430-52 con la FLC de tabla. <c>null</c> = servicio continuo (Power Node Web, I-120).
    /// </summary>
    ServicioNoContinuo? Servicio = null)
{
    /// <summary>1 (monofásico o CD), 2 o 3 -- deriva de TipoAlimentacion, no se captura aparte.</summary>
    public int NumeroFases => TipoAlimentacion switch
    {
        TipoAlimentacionMotor.Trifasico => 3,
        TipoAlimentacionMotor.DosFases => 2,
        TipoAlimentacionMotor.Monofasico => 1,
        TipoAlimentacionMotor.CorrienteContinua => 1,
        _ => throw new ArgumentOutOfRangeException(nameof(TipoAlimentacion)),
    };
}

/// <summary>
/// El servicio de un motor que no trabaja continuo — 430-22(e), I-120: la clase, para cuánto tiempo está
/// especificado, el porcentaje que da la Tabla 430-22(e) para esos dos y la corriente de placa del motor.
/// </summary>
public sealed record ServicioNoContinuo(
    TablasNom.ServicioDeMotor Clase,
    TablasNom.EspecificacionDeTiempo Especificacion,
    decimal PorcentajeTabla,
    decimal CorrientePlacaA);
