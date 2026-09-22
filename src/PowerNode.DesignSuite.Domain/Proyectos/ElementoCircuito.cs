using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Un renglón de carga dentro de un circuito de Alumbrado, Contactos o Equipo — un tipo de dispositivo
/// (luminaria, contacto...) por cantidad. El VA del circuito sale de sumar estos elementos, no se
/// captura agregado (así lo hacía el Excel original: catálogo de cargas típicas por cantidad).
/// No hay catálogo de dispositivos que mantener — el mercado es demasiado amplio para eso; cada
/// elemento se captura con sus datos mínimos directo en el circuito donde va.
/// Fuerza no usa esto — un motor es su propio circuito dedicado, ver DatosMotor.
/// </summary>
public class ElementoCircuito
{
    public int Id { get; set; }
    public int CircuitoDerivadoId { get; set; }
    public CircuitoDerivado CircuitoDerivado { get; set; } = null!;

    /// <summary>Descripción libre, p.ej. "Luminaria LED 40W interior" o "Contacto dúplex baño".</summary>
    public string Descripcion { get; set; } = string.Empty;

    public int Cantidad { get; set; } = 1;

    /// <summary>
    /// Volt-amperes por unidad. <b>Es el valor canónico</b>: el motor de cálculo suma
    /// <see cref="VaTotal"/> y no sabe de otras unidades.
    ///
    /// <para>
    /// Cuando el renglón se capturó en watts o en amperes (ver <see cref="UnidadConsumo"/>), este
    /// campo es <b>derivado</b> y lo reescribe <see cref="CircuitoDerivado.NormalizarConsumoDeRenglones"/>
    /// antes de cada cálculo. No lo escribas a mano en ese caso: lo pisa la normalización.
    /// </para>
    /// </summary>
    public decimal VaUnitario { get; set; }

    /// <summary>
    /// En qué unidad se capturó <see cref="ValorConsumo"/>. Volt-amperes es el caso por omisión y el
    /// de todos los renglones anteriores al 2026-08-21.
    ///
    /// <para>
    /// <b>Existe por los aparatos</b> (<see cref="Calculo.Unidades.TipoCarga.Equipo"/>): una placa
    /// publica watts o amperes, casi nunca VA. Convertirlos a mano antes de capturar es hacer a lápiz
    /// la cuenta que el programa existe para hacer — y con el riesgo de usar la tensión equivocada.
    /// </para>
    /// </summary>
    public UnidadConsumo UnidadConsumo { get; set; } = UnidadConsumo.VoltAmperes;

    /// <summary>
    /// El número tal como viene en la placa, en <see cref="UnidadConsumo"/>. <b>Es lo que el usuario
    /// tecleó</b>, y por eso es lo que se conserva: <see cref="VaUnitario"/> se recalcula desde aquí
    /// cada vez que cambia la tensión del tablero o el factor de potencia del circuito, así que
    /// guardar solo los VA dejaría un número que envejece en silencio.
    ///
    /// <para>
    /// <c>null</c> = se capturó directo en VA (el caso de siempre), y entonces manda
    /// <see cref="VaUnitario"/>.
    /// </para>
    /// </summary>
    public decimal? ValorConsumo { get; set; }

    /// <summary>Multiplicador opcional sobre VaUnitario (p.ej. balastros que consumen más que la potencia nominal de la lámpara). Default 1 (sin ajuste).</summary>
    public decimal FactorMultiplicador { get; set; } = 1m;

    /// <summary>Solo aplica cuando el circuito es de Contactos. Null en Alumbrado.</summary>
    public TipoContacto? TipoContacto { get; set; }

    /// <summary>
    /// Cuántas salidas tiene el dispositivo: sencillo, dúplex o trifásico. Solo aplica cuando el
    /// circuito es de Contactos; null en Alumbrado, igual que <see cref="TipoContacto"/>.
    ///
    /// Es un eje aparte del tipo, no un sustituto: un contacto puede ser «GFCI dúplex» o «GFCI
    /// sencillo». Juntos deciden qué símbolo se dibuja — ver
    /// <see cref="Calculo.Simbologia.ClaveSimboloContacto"/>.
    ///
    /// Null se lee como dúplex, que es la configuración por omisión y lo único que el programa sabía
    /// representar antes del 2026-08-19. Así los proyectos ya capturados no cambian de dibujo.
    /// </summary>
    public ConfiguracionContacto? ConfiguracionContacto { get; set; }

    /// <summary>
    /// Dónde va montado el receptáculo — pared, piso o intemperie. Solo aplica en Contactos; null en
    /// Alumbrado, y null se lee como <b>pared</b>, que es lo único que el programa representaba antes
    /// del 2026-08-19: así ningún proyecto ya capturado cambia de dibujo.
    ///
    /// <b>Manda sobre el tipo y la configuración al elegir símbolo</b>, porque la NMX publica una
    /// sola figura por montaje: no hay «GFCI de piso dúplex» dibujado. Cuando eso hace perder
    /// información capturada, el programa lo advierte.
    /// </summary>
    public MontajeContacto? Montaje { get; set; }

    /// <summary>
    /// Qué clase de luminaria es. Solo aplica en Alumbrado; null en Contactos, y null se lee como
    /// <b>general</b> (salida de techo). <b>No entra en ningún cálculo</b>: es dato de plano.
    /// </summary>
    public TipoLuminaria? TipoLuminaria { get; set; }

    public bool EsCargaContinua { get; set; }

    public decimal VaTotal => Cantidad * VaUnitario * FactorMultiplicador;
}
