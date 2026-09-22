namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Resultado del cálculo de un Alimentador — mismo criterio que un circuito derivado No-Motor (210-19(a)(1)/
/// 215-2(a)(1)): la carga es el rollup de lo que hay aguas abajo, tratado como un circuito grande.
/// </summary>
public class CalculoAlimentador
{
    public int Id { get; set; }
    public int AlimentadorId { get; set; }
    public Alimentador Alimentador { get; set; } = null!;

    /// <summary>
    /// Corriente de diseño del tramo. <b>Faltaba hasta el 2026-08-19</b> —el circuito derivado sí la
    /// guardaba— y la memoria de cálculo la necesita: es el <c>In</c> de la fórmula de caída de
    /// tensión, o sea el número con el que arranca media hoja del documento.
    /// </summary>
    public decimal CorrienteDisenoA { get; set; }

    /// <summary>
    /// La carga continua <b>agregada</b> que este tramo alimenta: la de todo lo que cuelga aguas
    /// abajo, no la de los circuitos propios del elemento destino.
    ///
    /// <para>
    /// <b>Por qué se persiste y no se recalcula al exportar</b> (auditoría 2026-08-19, §3.3). La
    /// memoria imprimía la carga sumando <c>tablero.CircuitosDerivados</c> —solo los propios— mientras el
    /// <c>In</c> de la sección 3 salía de <see cref="CorrienteDisenoA"/>, que es el agregado. En un
    /// tablero general que solo alimenta otros tableros el documento decía <b>"Carga total instalada:
    /// 0 VA"</b> y un <c>In</c> mayor que cero en la misma hoja: una memoria que no se puede
    /// recalcular, que es justo lo que la memoria existe para permitir.
    /// </para>
    ///
    /// <para>
    /// Rehacer el recorrido del árbol dentro del exportador habría sido peor: la regla de agregación
    /// no es una suma —un transformador es frontera y aporta su placa, un CCM y una protección son
    /// transparentes, y los motores van en un cubo aparte en Amperes— así que una segunda
    /// implementación se separaría de la primera en la primera regla que cambiara. Se guarda el
    /// número que el cálculo <b>usó</b>.
    /// </para>
    /// </summary>
    public decimal CargaContinuaVa { get; set; }

    /// <summary>La carga no continua agregada del tramo. Ver <see cref="CargaContinuaVa"/>.</summary>
    public decimal CargaNoContinuaVa { get; set; }

    /// <summary>
    /// La corriente de los motores que alimenta el tramo (suma de FLC del 430-24), en <b>Amperes</b>.
    /// Va aparte de los VA a propósito: el Art. 430 dimensiona en corriente de tabla, no en potencia
    /// aparente, y sumarla a los VA sería inventar un número que ninguna norma respalda. La memoria la
    /// declara como renglón propio para que la hoja cuadre consigo misma.
    /// </summary>
    public decimal CargaMotoresA { get; set; }

    /// <summary>Id de la Tabla de ampacidad que sustentó la selección: "310-15(b)(16)" o "(b)(17)".</summary>
    public string? TablaAmpacidadId { get; set; }

    public string? ConductorFase { get; set; }
    public string? ConductorNeutro { get; set; }
    public string? ConductorTierra { get; set; }
    public decimal CaidaTensionPct { get; set; }

    /// <summary>N de conductores en paralelo por fase que USÓ el cálculo -- ver el mismo campo en CalculoCircuitoDerivado.</summary>
    public int NumeroConductoresParalelo { get; set; } = 1;

    public string TextoMemoria { get; set; } = string.Empty;

    /// <summary>
    /// Los números intermedios del cálculo, para que la memoria se pueda <b>recalcular</b>. Nulo en
    /// resultados generados antes del 2026-08-19, cuando estos valores vivían solo dentro del texto.
    /// </summary>
    public Entregables.DetalleMemoria? Detalle { get; set; }

    // =================================================================================
    // VEREDICTO DE 240-21(b), cuando el tramo es una derivación. Agregado el 2026-08-25.
    // =================================================================================

    /// <summary>
    /// Bajo cuál de los cinco casos de 240-21(b) quedó permitida la derivación, o <c>null</c> si
    /// ninguno se cumple — y entonces <b>la derivación necesita su propia protección</b> en el punto
    /// de conexión (encabezado de 240-21) y deja de ser derivación.
    ///
    /// <para>
    /// Nulo también, y sin significar nada malo, en un alimentador normal: no es una derivación.
    /// Se distingue por <see cref="Alimentador.EsDerivacion"/>, no por este campo.
    /// </para>
    /// </summary>
    public Calculo.Derivaciones.ReglaDerivacion? ReglaDerivacion { get; set; }

    /// <summary>
    /// La ampacidad mínima que 240-21(b) le exigió a este conductor de derivación — el mayor de los
    /// criterios que le aplican (la carga calculada, el dispositivo donde termina, 1/3 o 1/10 de la
    /// protección del alimentador padre).
    ///
    /// <para>
    /// <b>Se persiste porque no se puede recalcular al exportar</b>, por la misma razón que
    /// <see cref="CargaContinuaVa"/>: depende de la protección del alimentador padre y de las
    /// condiciones declaradas, y rehacer ese recorrido dentro del exportador sería una segunda
    /// implementación de la regla.
    /// </para>
    /// </summary>
    public decimal? AmpacidadMinimaDerivacionA { get; set; }

    /// <summary>
    /// Las condiciones de 240-21(b) que quedaron abiertas —incumplidas o sin capturar— con su
    /// cláusula, un renglón por condición. Vacío cuando la derivación cumple limpio.
    ///
    /// <para>
    /// Es un <b>texto de resultado</b>, no un dato de captura: es lo que el aviso enseña en pantalla
    /// y lo que la memoria imprime, y por eso se guarda con el cálculo que lo produjo. Recalcular
    /// borra el anterior.
    /// </para>
    /// </summary>
    public string? CondicionesDerivacionAbiertas { get; set; }
}
