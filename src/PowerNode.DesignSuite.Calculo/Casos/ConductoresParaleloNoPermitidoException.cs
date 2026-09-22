namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>310-10(h)(1): conductores en paralelo solo se permiten a partir de 1/0 AWG (53.49 mm²).</summary>
public sealed class ConductoresParaleloNoPermitidoException(string mensaje) : Exception(mensaje);
