using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Dónde va montada una <see cref="Carga"/> dentro de un <see cref="CentroControlMotores"/>. La NOM
/// les llama <b>"unidades de control de motores"</b> (430-98(b)) — en obra se les dice gaveta o
/// cubículo; aquí se usa el término de la norma.
///
/// <b>Es solo lo físico, a propósito.</b> Lo eléctrico de la unidad (desconectador, protección
/// contra cortocircuito, contactor, relevador de sobrecarga) ya lo resuelve <c>CalculoCarga</c>, que
/// calcula las dos protecciones del 430 sin saber que existe un CCM. Duplicarlo aquí sería tener el
/// mismo motor de arranque capturado en dos lugares.
///
/// Es un bloque <i>owned</i> y <b>nulo cuando la carga no cuelga de un CCM</b> — un chiller colgado
/// directo del tablero general no tiene sección ni altura que declarar.
/// </summary>
public class UnidadControl
{
    /// <summary>
    /// Sección (columna vertical) del gabinete, empezando en 1. 430-96 habla de CCM "que consten de
    /// varias secciones", y 430-97(a) exige que en una sección vertical solo vayan los conductores
    /// que terminan ahí — por eso la sección es dato del proyecto y no adorno.
    /// </summary>
    public int Seccion { get; set; } = 1;

    /// <summary>Posición dentro de la sección, contando de arriba hacia abajo desde 1.</summary>
    public int Posicion { get; set; }

    /// <summary>
    /// Altura que ocupa la unidad, en espacios del gabinete. Las unidades de un CCM son de alturas
    /// distintas (un arrancador chico ocupa una fracción de lo que ocupa un variador), así que sin
    /// esto no se puede saber cuánto espacio queda libre en la sección.
    /// </summary>
    public int AlturaUnidades { get; set; } = 1;

    /// <summary>
    /// Cómo arranca el motor. <b>Descriptivo, no entra en ningún cálculo</b> — ver
    /// <see cref="TipoArranqueMotor"/> para por qué.
    /// </summary>
    public TipoArranqueMotor TipoArranque { get; set; } = TipoArranqueMotor.TensionPlena;
}
