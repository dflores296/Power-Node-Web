using PowerNode.DesignSuite.Calculo.Casos;

namespace PowerNode.Web.Modelo;

/// <summary>
/// Qué es un aparato del desglose — I-115. En un grupo de motores (430-53) decide cómo se captura y con
/// qué artículo cuenta; fuera de un grupo solo cuenta <see cref="Carga"/>.
/// </summary>
public enum ClaseDeAparato
{
    /// <summary>Una carga de placa: VA, W o A, continua o no — lo de siempre.</summary>
    Carga,

    /// <summary>Un motor de uso general: HP o amperes, con su FLC de tabla — 430-6(a).</summary>
    Motor,

    /// <summary>Un motocompresor hermético: corriente de carga nominal de placa — 440-6(a).</summary>
    Motocompresor,

    /// <summary>
    /// Un acondicionador de aire para habitación con cordón y clavija, en un circuito de contactos o de
    /// equipo: su corriente total de placa — 440-62(b), (c) (I-117). No es máquina de un grupo.
    /// </summary>
    AireDeHabitacion,
}

/// <summary>
/// <b>Un aparato dentro de un circuito</b> — I-35. Un espacio del tablero es un circuito, no un
/// aparato: un circuito puede alimentar uno solo o varios. Con el desglose, la carga del circuito es
/// la suma de sus aparatos, y la memoria dice qué alimenta.
///
/// <para>
/// Es opcional: un circuito sin aparatos se captura como siempre, con su total.
/// </para>
/// </summary>
public sealed class CargaDelCircuito
{
    /// <summary>La carga de un contacto, sencillo o múltiple en un mismo yugo — 220-14(i).</summary>
    public const decimal VAPorContacto = 180m;

    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Cuántos aparatos iguales. Mínimo 1.</summary>
    public int Cantidad { get; set; } = 1;

    public UnidadConsumo Unidad { get; set; } = UnidadConsumo.VoltAmperes;

    /// <summary>La carga de <b>uno</b>, como la dice su placa, en <see cref="Unidad"/>.</summary>
    public decimal CargaUnitaria { get; set; }

    /// <summary>Opera 3 h o más: entra al 125 % — 210-19(a)(1).</summary>
    public bool Continua { get; set; }

    public decimal FactorPotencia { get; set; } = CircuitoDelCuadro.FactorPotenciaSupuesto;

    /// <summary>Los VA de todos (cantidad × carga), convertidos con la tensión y los polos del circuito.</summary>
    public decimal TotalVA { get; internal set; }

    /// <summary>«Contacto», «contactos dobles»…: se llena con 180 VA si no trae carga — 220-14(i).</summary>
    internal bool EsContactoSinCarga =>
        Clase == ClaseDeAparato.Carga && CargaUnitaria == 0m && Descripcion.Trim().StartsWith("contacto", StringComparison.OrdinalIgnoreCase);

    // ---- EN UN GRUPO DE MOTORES (I-115) ---------------------------------------------------------
    // Los datos de cada clase van en campos propios: al cambiar de clase no se leen los VA de una carga
    // como amperes de un motor, y al regresar se recupera lo capturado.

    /// <summary>Carga, motor o motocompresor. Ver <see cref="ClaseDeAparato"/>.</summary>
    public ClaseDeAparato Clase { get; set; } = ClaseDeAparato.Carga;

    /// <summary>Motor o motocompresor: su corriente cuenta en 430-24 y 430-53, no como carga de placa.</summary>
    public bool EsMaquina => Clase is ClaseDeAparato.Motor or ClaseDeAparato.Motocompresor;

    /// <summary>Un motor se captura en HP o en amperes — 430-6(a)(1). Solo HP o amperes, nunca «Varios».</summary>
    public CapturaDeMotor CapturaMotor { get; set; } = CapturaDeMotor.Hp;

    /// <summary>Los caballos de placa de un motor en HP.</summary>
    public decimal? Hp { get; set; }

    /// <summary>La corriente de placa: la de un motor marcado en amperes, o la de carga nominal de un motocompresor.</summary>
    public decimal CorrientePlacaA { get; set; }

    /// <summary>Motocompresor: la corriente de selección del circuito derivado, si la placa la trae — 440-6(a) Exc. 1.</summary>
    public decimal? CorrienteSeleccionA { get; set; }

    /// <summary>Lo capturado de una máquina: HP, o su corriente.</summary>
    public bool TieneCapturaDeMaquina =>
        Clase == ClaseDeAparato.Motor && CapturaMotor == CapturaDeMotor.Hp ? Hp > 0m : CorrientePlacaA > 0m;

    /// <summary>
    /// La corriente de <b>una</b> máquina: la FLC de tabla (430-6(a)) o la de 440-6(a). 0 si no es
    /// máquina, no se capturó o la tabla no la trae (<see cref="Error"/>). La pone <see cref="CuadroDeCarga"/>.
    /// </summary>
    public decimal CorrienteUnitariaA { get; internal set; }

    /// <summary>Un motor en amperes, llevado a la tabla — 430-6(a)(1). La pone <see cref="CuadroDeCarga"/>.</summary>
    public MotorEnAmperes? MotorEnAmperes { get; internal set; }

    /// <summary>Por qué la máquina no tiene corriente, ya redactado. La pone <see cref="CuadroDeCarga"/>.</summary>
    public string? Error { get; internal set; }
}
