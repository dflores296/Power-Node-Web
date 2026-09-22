using PowerNode.DesignSuite.Domain.Norma;

namespace PowerNode.DesignSuite.Domain.Referencia;

/// <summary>
/// Una tabla de factor de demanda de la norma, lista para mostrarse en el panel informativo "para
/// saber más sobre factores de demanda" (bloque 8.5) -- con la prosa de la sección que la introduce
/// y sus excepciones, para que el ingeniero decida a mano qué aplicar. v1 no automatiza el Art. 220
/// (ver PLAN-V1.md, sección "Factor de Demanda"); esto es solo consulta, no participa en ningún
/// cálculo.
/// </summary>
public sealed record EntradaFactorDemanda(
    Tabla Tabla,
    string? TextoSeccion,
    IReadOnlyList<string> Excepciones);
