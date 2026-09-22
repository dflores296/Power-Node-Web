using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Datos de placa de un motor del Art. 430. La corriente a plena carga (FLC) se determina por tabla
/// (430-247/248/249/250) según Hp y TipoAlimentacion, salvo que se capture la de placa.
///
/// **Es un tipo <i>owned</i> de EF, no una tabla propia.** Antes era `DatosMotorCircuito`, amarrado
/// a `CircuitoDerivado` con su propio Id y FK. Se volvió compartido cuando apareció <see cref="Carga"/>:
/// un motor colgado directo de un tablero (una bomba, un elevador) tiene exactamente los mismos
/// datos de placa que un circuito de Fuerza, y duplicar los campos era pedir que se desincronizaran
/// — este proyecto ya se quemó con `MaterialConductor` guardado como texto en una tabla y como
/// número en otra. Al ser owned, sus columnas viven en la tabla de quien lo tenga (`CircuitosDerivados` o
/// `ElementosTopologia`) con una sola definición en C#.
/// </summary>
public class DatosMotor
{
    public decimal Hp { get; set; }
    public TipoAlimentacionMotor TipoAlimentacion { get; set; }
    public TipoMotor TipoMotor { get; set; }
    public TipoDispositivoProteccionMotor TipoDispositivoProteccion { get; set; }

    /// <summary>
    /// Tensión nominal de PLACA del motor (una clase NEMA estándar: 115, 127, 200, 208, 230, 460,
    /// 575, 2300...) -- la que de verdad tienen las columnas de las Tablas 430-247/248/249/250 y
    /// 430-251(a)/(b), NO necesariamente la misma que la tensión del tablero. Un motor en un tablero
    /// de 220V real casi siempre trae placa de 230V (las clases NEMA no coinciden 1:1 con las
    /// tensiones de distribución mexicanas) -- por eso es un campo aparte y no se infiere del
    /// tablero: adivinar el redondeo (¿220 a 230? ¿127 a 115?) sería un error silencioso. La
    /// tensión REAL del sistema se sigue usando para la caída de tensión, que sí depende del
    /// sistema eléctrico real, no de la clase de placa del motor.
    /// </summary>
    public decimal TensionNominalMotorV { get; set; }

    /// <summary>Letra de código (Tabla 430-7(b)), para convertir a corriente de arranque vía Tabla 430-251(a)/(b) — Excepción 2 de 430-52(c)(1).</summary>
    public string? LetraCodigo { get; set; }

    /// <summary>Corriente de plena carga de placa, si se decide usar en vez del valor de tabla.</summary>
    public decimal? CorrientePlenaCargaPlacaA { get; set; }

    /// <summary>
    /// ¿La placa marca <b>factor de servicio de 1.15 o más</b>, o <b>elevación de temperatura de
    /// 40 °C o menos</b>? Decide el porcentaje de la protección contra sobrecarga en 430-32(a)(1):
    /// <b>125 %</b> con cualquiera de esas dos marcas, <b>115 %</b> en "todos los demás motores".
    ///
    /// Por omisión es false — o sea 115 %, el caso conservador. El 125 % exige una marca afirmativa
    /// en la placa, así que suponerlo sin verla sería aflojar la protección sin sustento.
    /// </summary>
    public bool MarcadoPermiteSobrecarga125Pct { get; set; }
}
