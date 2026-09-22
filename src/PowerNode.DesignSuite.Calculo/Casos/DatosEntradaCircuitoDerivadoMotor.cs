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
    int MaxConductoresParaleloAutomatico = SeleccionConductor.MaxNParaleloAutoResueltoPorOmision)
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
