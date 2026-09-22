namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// 110-14(c): la temperatura nominal de operación del conductor no debe exceder la temperatura
/// nominal más baja de la terminal a la que se conecta. Se lanza cuando el tipo de aislamiento
/// capturado no se reconoce, no es válido para el lugar de instalación capturado (p.ej. THHN en
/// lugar mojado), o su temperatura máxima queda por debajo de lo que exige la terminal del equipo
/// (60/75°C según 110-14(c)(1), según el amperaje de la protección).
/// </summary>
public sealed class AislamientoIncompatibleException(string mensaje) : Exception(mensaje);
