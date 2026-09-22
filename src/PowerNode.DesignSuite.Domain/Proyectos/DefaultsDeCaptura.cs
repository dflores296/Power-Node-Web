using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// <b>Los valores con los que nace un elemento nuevo de este proyecto</b> — el nivel intermedio que
/// faltaba (auditoría 2026-08-19, §4.2).
///
/// <para>
/// <b>El hueco que cierra.</b> Los criterios que se capturan por elemento —factor de potencia,
/// temperatura ambiente, material del conductor, aislamiento…— tenían su valor por omisión escrito
/// en la clase de C#:
/// </para>
/// <code>
/// public decimal FactorPotencia { get; set; } = 0.9m;
/// public MaterialConductor MaterialConductor { get; set; } = MaterialConductor.Cobre;
/// </code>
/// <para>
/// Eso significaba que <b>un proyecto en aluminio, o en un sitio a 40 °C, obligaba a corregir cada
/// circuito a mano</b> — y basta olvidar uno para que el cuadro de carga mezcle criterios sin avisar.
/// El error no salta: el circuito olvidado calcula bien, con el criterio equivocado.
/// </para>
///
/// <para>
/// <b>Los tres niveles, y por qué ninguno se mueve de lugar:</b>
/// </para>
/// <list type="number">
/// <item><b>Límites del proyecto</b> (caída máxima, desbalanceo, pisos prácticos) — ya existían en
/// <see cref="ConfiguracionProyecto"/>, son topes contra los que el motor verifica.</item>
/// <item><b>Defaults de captura</b> — esto. No son topes ni los usa el motor: solo dicen con qué
/// valor <i>nace</i> un elemento.</item>
/// <item><b>El campo del elemento</b> — sigue mandando, y sigue siendo editable uno por uno. El
/// motor lee de ahí y de ningún otro lado.</item>
/// </list>
///
/// <para>
/// <b>El sistema propone, el campo se rellena solo, y nunca se bloquea.</b> Es la misma política que
/// ya se decidió para el %Z del transformador. Los defaults se aplican <b>al crear</b, una vez
/// (<see cref="AplicarDefaults"/>), no en cada guardado: si se aplicaran siempre, editar un circuito
/// a un criterio distinto sería imposible — el proyecto se lo comería en el siguiente <c>Save</c>.
/// </para>
///
/// <para>
/// <b>Cada valor por omisión de aquí es idéntico al que tenía la clase.</b> No es coincidencia: es
/// la condición para que ningún proyecto ya capturado cambie de calibres al actualizar. Hay una
/// prueba que compara los dos juegos campo por campo y falla si alguien mueve uno solo.
/// </para>
/// </summary>
public class DefaultsDeCaptura
{
    /// <summary>Factor de potencia (cos θ) con el que nace un circuito o alimentador. Ver <see cref="CircuitoDerivado.FactorPotencia"/>.</summary>
    public decimal FactorPotencia { get; set; } = 0.9m;

    /// <summary>
    /// Temperatura ambiente de la corrida — Tabla 310-15(b)(2)(a). 30 °C es la base de las Tablas
    /// 310-15(b)(16) y (b)(17), o sea factor 1 y sin corrección.
    ///
    /// <para>
    /// <b>Es el default que más se gana con este nivel:</b> una obra en un sitio caliente lo captura
    /// una vez y todos sus circuitos nacen corregidos, en vez de corregirse de a uno y arriesgar que
    /// se escape el que nadie tocó.
    /// </para>
    /// </summary>
    public decimal TemperaturaAmbienteC { get; set; } = 30m;

    /// <summary>Conductores portadores de corriente que comparten canalización — Tabla 310-15(b)(3)(a). 3 = sin ajuste.</summary>
    public int ConductoresAgrupados { get; set; } = 3;

    /// <summary>Cobre o aluminio. El otro default que se captura una vez por obra y no circuito por circuito.</summary>
    public MaterialConductor MaterialConductor { get; set; } = MaterialConductor.Cobre;

    public MaterialCanalizacion MaterialCanalizacion { get; set; } = MaterialCanalizacion.Pvc;

    /// <summary>Designación del aislamiento — Tabla 310-104(a). Ver <see cref="CircuitoDerivado.TipoAislamiento"/>.</summary>
    public string TipoAislamiento { get; set; } = "THHN";

    /// <summary>Canalización/cable (310-15(b)(16)) o al aire libre (310-15(b)(17)) — ambas base 30 °C.</summary>
    public MetodoInstalacion MetodoInstalacion { get; set; } = MetodoInstalacion.CanalizacionOCable;

    /// <summary>Lugar seco (true) o húmedo/mojado — Tabla 310-104(a).</summary>
    public bool LugarInstalacionSeco { get; set; } = true;

    /// <summary>
    /// Conductores en paralelo por fase con los que nace una corrida. <b>1 casi siempre</b>, y
    /// cambiarlo de proyecto tiene sentido en obras donde todas las corridas grandes van partidas por
    /// criterio de montaje. Solo es válido a partir de 1/0 AWG (310-10(h)(1)); el motor lo verifica
    /// igual, este número no le gana a la norma.
    /// </summary>
    public int NumeroConductoresParalelo { get; set; } = 1;

    /// <summary>
    /// Factor de demanda de la carga continua. <b>1.0 = sin reducción</b>, que es la decisión ya
    /// tomada para v1: el Factor de Demanda se queda libre a criterio de diseño y no se automatiza el
    /// Art. 220 (hay un panel de referencia con sus 10 tablas para consultarlas). Este default no
    /// cambia esa decisión — solo evita teclear el mismo criterio en treinta circuitos.
    /// </summary>
    public decimal FactorDemandaContinua { get; set; } = 1m;

    /// <summary>Factor de demanda de la carga no continua. Ver <see cref="FactorDemandaContinua"/>.</summary>
    public decimal FactorDemandaNoContinua { get; set; } = 1m;

    /// <summary>
    /// <b>Declaración del proyectista</b> de que el ensamble está aprobado para operar al 100 % de su
    /// valor nominal (215-2(a)(1)/215-3), y por lo tanto no se le aplica el 125 % de la carga continua.
    ///
    /// <para>
    /// <b>Falso por omisión, y conviene pensarlo dos veces antes de cambiarlo aquí.</b> Es dato de
    /// placa y de marcado del <i>ensamble</i>, no una preferencia: hay gabinetes marcados
    /// <i>"Not rated for 100% rated circuit breakers"</i>. Ponerlo en verdadero a nivel de proyecto
    /// declara que <b>todo</b> el equipo de la obra lo está. El programa lo contradice cuando el
    /// modelo elegido del catálogo es de 80 %, pero no puede contradecir lo que no tiene modelo.
    /// </para>
    /// </summary>
    public bool ConjuntoAprobado100Pct { get; set; }

    /// <summary>
    /// Relación X/R con la que nace un transformador, para la corriente de falla y la regulación del
    /// %Z. 3.0 es el valor típico de un transformador de distribución en baja tensión; el dato real
    /// viene de la ficha del fabricante y por eso sigue siendo editable en cada transformador.
    /// </summary>
    public decimal RelacionXRTransformador { get; set; } = 3m;
}
