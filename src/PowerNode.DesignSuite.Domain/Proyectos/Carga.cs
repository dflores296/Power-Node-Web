using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Un equipo colgado directo de un elemento, sin tablero de por medio: un chiller, un elevador, una
/// bomba contra incendio, un manejador de aire, un compresor. Llega su alimentador, entra a su
/// desconectador/arrancador, y ahí se acaba — no reparte nada. Es un elemento <b>terminal</b> del árbol.
///
/// <b>Por qué no alcanzaba <see cref="CircuitoDerivado"/>.</b> Un circuito derivado es *un espacio en la barra de un
/// tablero*: tiene `Numero` (el espacio), `Polos` y `Fase` derivada por la distribución NEMA.
/// Correcto para una pastilla en un NQ; falso para un chiller alimentado desde una subestación con
/// secciones I-Line, que no tiene "espacio número 7". Y para coordinación de protecciones hace
/// falta que sea un elemento del árbol con sus aristas, no un renglón de un cuadro de carga.
///
/// <b>Es MÁS CHICA que un CircuitoDerivado, no más grande.</b> Todo el bloque de cableado (longitud,
/// material, canalización, temperatura, agrupamiento, aislamiento, método de instalación,
/// conductores en paralelo) vive en <see cref="Alimentador"/>, que es justo lo que la alimenta.
/// Para un elemento terminal esos datos van en la <b>arista</b>, no en el elemento.
/// </summary>
public class Carga : ElementoTopologia
{
    /// <summary>Tensión del sistema al que se conecta (la real, no la clase de placa del motor).</summary>
    public decimal TensionV { get; set; }

    /// <summary>1, 2 (bifásico 220Y127) o 3. Misma convención que <see cref="Tablero.Fases"/>.</summary>
    public int Fases { get; set; } = 3;

    public decimal FactorPotencia { get; set; } = 0.9m;

    /// <summary>
    /// Carga continua (215-2/215-3: tres horas o más). En los regímenes de motor NO se usa para
    /// inflar la protección — el 430 y el 440 ya traen sus propios múltiplos, y aplicar ambos sería
    /// contar dos veces el mismo margen.
    /// </summary>
    public bool EsCargaContinua { get; set; }

    /// <summary>
    /// Qué artículo la gobierna. <b>Es el eje de la entidad:</b> la pregunta que define una Carga no
    /// es "¿es motor o no?", sino de dónde sale su corriente.
    /// </summary>
    public RegimenCarga Regimen { get; set; } = RegimenCarga.Generica;

    // ---------------------------------------------------------------- Generica

    /// <summary>Potencia aparente capturada. Solo en <see cref="RegimenCarga.Generica"/>.</summary>
    public decimal PotenciaVa { get; set; }

    // ---------------------------------------------------------------- Motor430

    /// <summary>
    /// Datos de placa del motor. Solo en <see cref="RegimenCarga.Motor430"/>. Es el mismo bloque
    /// (owned) que usa <see cref="CircuitoDerivado"/> — una sola definición en C#.
    /// </summary>
    public DatosMotor? DatosMotor { get; set; }

    // ---------------------------------------------------------------- AireAcondicionado440

    /// <summary>
    /// <b>Corriente de carga nominal de placa</b> (la RLA). Solo en
    /// <see cref="RegimenCarga.AireAcondicionado440"/>, donde es el dato <b>primario</b>: 440-6(a)
    /// manda usar la corriente de la placa y NO sacar la FLC por Hp de las tablas del 430. Un
    /// chiller muchas veces ni trae Hp.
    /// </summary>
    public decimal? CorrienteNominalPlacaA { get; set; }

    /// <summary>
    /// <b>Corriente de selección del circuito derivado</b>, si viene marcada en la placa. La
    /// Excepción 1 de 440-6(a) manda usarla en lugar de la corriente de carga nominal, y el resto
    /// del Artículo repite "de estos dos valores el que sea mayor" en cada regla (440-22(a),
    /// 440-32). Por eso son <b>dos</b> datos de placa, no uno: null = no viene marcada.
    /// </summary>
    public decimal? CorrienteSeleccionCircuitoA { get; set; }

    // ---------------------------------------------------------------- montaje en un CCM

    /// <summary>
    /// Dónde va montada dentro de un <see cref="CentroControlMotores"/>, cuando cuelga de uno.
    /// <b>Null es lo normal:</b> una carga alimentada directo desde un tablero no tiene sección ni
    /// altura que declarar. Ver <see cref="UnidadControl"/> — es solo geometría; lo eléctrico del
    /// arrancador ya vive en <see cref="Calculo"/>.
    /// </summary>
    public UnidadControl? UnidadControl { get; set; }

    // ---------------------------------------------------------------- transformador de control

    /// <summary>
    /// Número de catálogo del <b>transformador de control</b> que alimenta el circuito de control de
    /// esta máquina (Square D serie 9070, tipos T y TF). <c>null</c> es lo normal: una carga que no
    /// lleva control propio, o que todavía no se especifica.
    ///
    /// <para>
    /// <b>Por qué cuelga de la carga y no del árbol</b> (decisión del usuario, 2026-08-19, opción A
    /// del punto 1.8 del reporte de UI). Un transformador de control <b>no es un elemento de la
    /// topología</b>: su secundario alimenta bobinas de contactor, botonería y pilotos — no un
    /// tablero, no una carga de la instalación. Ponerlo como nodo del árbol le colgaría una cascada
    /// que no existe y lo dejaría compitiendo en el selector con los transformadores de distribución,
    /// donde alguien acabaría eligiendo uno de 100 VA para alimentar un tablero. Aquí es lo que de
    /// verdad es: <b>un accesorio de la máquina</b>, y por eso se guarda como referencia de catálogo
    /// junto a ella y no como entidad propia.
    /// </para>
    ///
    /// <para>
    /// <b>NO entra en la carga del circuito, y es a propósito.</b> El motor no le suma sus VA a nada.
    /// Dos razones: se alimenta del circuito derivado del motor y su consumo es de decenas o cientos
    /// de VA contra los miles del motor —abajo del redondeo de cualquier protección estándar—, y
    /// sumarlo en silencio cambiaría el resultado de proyectos ya capturados sin que nadie lo pidiera.
    /// <b>Si algún día se quiere contar, es decisión de diseño y lleva su propia discusión:</b> lo que
    /// el Art. 430 Parte F regula es la protección del circuito de control (430-72), no que su
    /// potencia se agregue a la del motor.
    /// </para>
    /// </summary>
    public string? ModeloTransformadorControl { get; set; }

    // ----------------------------------------------------------------

    /// <summary>
    /// El cálculo propio de la carga. <b>Existe para que ninguna carga quede jamás sin protección en
    /// el modelo:</b> igual que un Tablero calcula la protección de su alimentador entrante
    /// (`CalculoTablero.BreakerPrincipalA`), una Carga calcula la suya. Colocar un elemento
    /// <see cref="Proteccion"/> aparte sigue siendo opcional, para cuando es un aparato físico
    /// distinto (el desconectador a la vista, o un interruptor en gabinete propio).
    /// </summary>
    public CalculoCarga? Calculo { get; set; }
}

/// <summary>
/// De dónde sale la corriente de una <see cref="Carga"/> — y por lo tanto qué artículo la gobierna.
/// </summary>
public enum RegimenCarga
{
    /// <summary>VA capturados. Tablero de control, carga resistiva, cualquier cosa que no sea motor.</summary>
    Generica,

    /// <summary>Hp → Tablas 430-247/248/249/250. Bomba, elevador.</summary>
    Motor430,

    /// <summary>
    /// Corriente de placa (440-6(a)). Chiller, manejadora con motocompresor hermético. El 440-3(a)
    /// dice que sus disposiciones "son adicionales o modifican las del Artículo 430", y 440-8
    /// considera un sistema de A/C <b>una sola máquina</b> aunque traiga varios motores adentro.
    /// </summary>
    AireAcondicionado440
}
