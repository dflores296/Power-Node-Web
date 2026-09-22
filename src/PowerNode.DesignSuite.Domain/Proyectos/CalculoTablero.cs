namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Resultado propio del tablero, sin importar qué lo alimenta (otro tablero, un transformador, o
/// nada — es el tablero raíz). El interruptor principal sale de tratar la suma de sus circuitos
/// como si fuera uno solo (misma metodología que un circuito derivado No-Motor, 210-19(a)(1)) — no hay
/// factores de demanda escalonados del Art. 220. El desbalanceo es un diagnóstico aparte, no
/// afecta la selección. El conductor del Alimentador que lo conecta hacia arriba vive en
/// CalculoAlimentador, no aquí — un tablero raíz no tiene Alimentador entrante pero sí necesita
/// su propio interruptor principal.
/// </summary>
public class CalculoTablero
{
    public int Id { get; set; }
    public int TableroId { get; set; }
    public Tablero Tablero { get; set; } = null!;

    public decimal BreakerPrincipalA { get; set; }

    /// <summary>(max(fase) - min(fase)) / max(fase) × 100. Diagnóstico, no bloquea el cálculo.</summary>
    public decimal DesbalanceoPct { get; set; }

    /// <summary>
    /// True si <see cref="DesbalanceoPct"/> superó el umbral configurado en
    /// <see cref="ConfiguracionProyecto.DesbalanceoMaxPct"/> (bloque 9) al recalcularse. Es un
    /// aviso -- no bloquea nada, solo le dice al ingeniero que revise el reparto de carga.
    /// </summary>
    public bool DesbalanceoExcedeLimite { get; set; }

    /// <summary>
    /// Corriente de cortocircuito disponible en las barras de este tablero, en Amperes. Se propaga
    /// desde la acometida y la recalcula cada transformador que haya en el camino. Null = no se
    /// pudo determinar (ni dato de la suministradora ni un transformador con %Z aguas arriba).
    /// </summary>
    public decimal? CorrienteFallaDisponibleA { get; set; }

    /// <summary>
    /// ¿Los kA del tablero alcanzan para esa falla? <b>Null significa "no se pudo evaluar"</b>, que
    /// es distinto de false. Se distinguen a propósito: faltar un dato no es lo mismo que estar mal.
    /// </summary>
    public bool? CapacidadInterruptivaSuficiente { get; set; }

    /// <summary>Avisos del tablero, uno por renglón. Hoy solo los de capacidad interruptiva.</summary>
    public string Advertencias { get; set; } = string.Empty;

    public string TextoMemoria { get; set; } = string.Empty;
}
