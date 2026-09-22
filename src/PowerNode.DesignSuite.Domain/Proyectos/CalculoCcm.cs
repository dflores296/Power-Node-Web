namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Resultado propio de un <see cref="CentroControlMotores"/>. Se calcula igual que el interruptor
/// principal de un <see cref="Tablero"/> —tratar todo lo que cuelga como una sola carga, con el
/// bucket de motores del 430-24 aparte— y encima lleva las dos verificaciones que son exclusivas de
/// la Parte H: la del <b>430-94</b> (la protección no puede exceder el valor nominal de la barra
/// común) y la del <b>430-95</b> (si es equipo de acometida, necesita medio principal de
/// desconexión propio).
/// </summary>
public class CalculoCcm
{
    public int Id { get; set; }
    public int CentroControlMotoresId { get; set; }
    public CentroControlMotores CentroControlMotores { get; set; } = null!;

    /// <summary>Corriente de diseño agregada de todo lo que cuelga del CCM (215-2 + 430-24).</summary>
    public decimal CorrienteDisenoA { get; set; }

    /// <summary>Capacidad mínima antes de redondear al estándar: 125 % de la continua + 100 % de la no continua + 430-24.</summary>
    public decimal CapacidadMinimaA { get; set; }

    /// <summary>
    /// Protección contra sobrecorriente del CCM, ya redondeada al estándar de 240-6(a). Según
    /// <see cref="CentroControlMotores.UbicacionProteccion"/> es el aparato de aguas arriba o el
    /// principal interno — el valor calculado es el mismo, lo que cambia es dónde se instala.
    /// </summary>
    public decimal ProteccionA { get; set; }

    /// <summary>
    /// ¿La protección cabe en el valor nominal de la barra común (430-94)? <b>Null = no se pudo
    /// evaluar</b> porque no se capturó <see cref="CentroControlMotores.CorrienteBarrasA"/>, que es
    /// distinto de false. Misma convención que
    /// <see cref="CalculoTablero.CapacidadInterruptivaSuficiente"/>.
    /// </summary>
    public bool? ProteccionCabeEnBarras { get; set; }

    /// <summary>Corriente de cortocircuito disponible en las barras del CCM, propagada desde la acometida.</summary>
    public decimal? CorrienteFallaDisponibleA { get; set; }

    /// <summary><b>Null = no se pudo evaluar</b>, que es distinto de false.</summary>
    public bool? CapacidadInterruptivaSuficiente { get; set; }

    /// <summary>Avisos del CCM, uno por renglón, cada uno con su referencia a la norma.</summary>
    public string Advertencias { get; set; } = string.Empty;

    public string TextoMemoria { get; set; } = string.Empty;
}
