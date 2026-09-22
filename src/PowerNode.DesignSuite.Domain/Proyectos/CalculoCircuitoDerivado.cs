namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Resultado del motor de cálculo para un circuito (Paso 2, todavía no implementado).
/// Se deja el esqueleto ya relacionado 1:1 con circuito derivado para no tener que migrar de nuevo
/// cuando se escriba el motor.
/// </summary>
public class CalculoCircuitoDerivado
{
    public int Id { get; set; }
    public int CircuitoDerivadoId { get; set; }
    public CircuitoDerivado CircuitoDerivado { get; set; } = null!;

    public decimal CorrienteDisenoA { get; set; }
    public decimal ProteccionA { get; set; }

    public string? ConductorFase { get; set; }
    public string? ConductorNeutro { get; set; }
    public string? ConductorTierra { get; set; }
    public decimal CaidaTensionPct { get; set; }

    /// <summary>
    /// N de conductores en paralelo por fase que USÓ el cálculo -- puede ser mayor al capturado en
    /// CircuitoDerivado.NumeroConductoresParalelo cuando el bloque 8 lo sube automáticamente (caso
    /// "obligado": el catálogo se agotó con el N capturado). Si difiere, TextoMemoria lo explica;
    /// esto no sobreescribe la captura del usuario, solo reporta qué usó el resultado.
    /// </summary>
    public int NumeroConductoresParalelo { get; set; } = 1;

    /// <summary>Id de Tabla (p.ej. "310-15(b)(16)") que sustentó la selección del conductor por capacidad.</summary>
    public string? TablaAmpacidadId { get; set; }

    public string TextoMemoria { get; set; } = string.Empty;

    /// <summary>
    /// Los números intermedios del cálculo, para que la memoria se pueda <b>recalcular</b>. Nulo en
    /// resultados generados antes del 2026-08-19, cuando estos valores vivían solo dentro del texto.
    /// </summary>
    public Entregables.DetalleMemoria? Detalle { get; set; }

}
