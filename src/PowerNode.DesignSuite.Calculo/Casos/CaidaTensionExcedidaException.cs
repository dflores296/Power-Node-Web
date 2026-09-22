namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Error duro (no advertencia): ningún calibre del catálogo, incluyendo el mayor disponible,
/// cumple el límite de caída de tensión configurado. El usuario decide cómo resolverlo — acortar
/// el circuito, subir a conductores en paralelo, o relajar el límite en Configuración.
/// </summary>
public sealed class CaidaTensionExcedidaException(string mensaje) : Exception(mensaje);
