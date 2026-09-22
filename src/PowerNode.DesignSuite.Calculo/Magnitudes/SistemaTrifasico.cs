namespace PowerNode.DesignSuite.Calculo.Magnitudes;

/// <summary>
/// Cómo están conectados los tres devanados/bobinados de un sistema trifásico —
/// aplica igual a un transformador (primario/secundario, cada lado con su propia
/// conexión) que a un motor (los devanados del estator). Determina la relación entre
/// magnitud de línea y de fase, tanto para tensión como para corriente.
/// </summary>
public enum ConexionSistema
{
    /// <summary>Y / estrella: los tres devanados comparten un punto común (neutro). V_línea = √3·V_fase; I_línea = I_fase. Puede traer neutro afuera (4 hilos) o no (3 hilos).</summary>
    Estrella,

    /// <summary>Δ / delta: los devanados forman un anillo cerrado, sin punto común. V_línea = V_fase; I_línea = √3·I_fase. Nunca tiene neutro.</summary>
    Delta,
}

/// <summary>
/// Relaciones línea↔fase de un sistema trifásico balanceado, y las fórmulas de
/// potencia/corriente que de ahí se derivan. La raíz de 3 no es una constante mágica:
/// sale de la geometría de sumar tres fasores desfasados 120° entre sí (por eso solo
/// aparece del lado que tiene ese desfase — línea en estrella, o corriente en delta —
/// y nunca del lado que no lo tiene).
/// </summary>
public static class SistemaTrifasico
{
    public static readonly decimal Raiz3 = (decimal)Math.Sqrt(3.0);

    public static decimal TensionLineaDesdeFase(decimal tensionFaseV, ConexionSistema conexion) =>
        conexion == ConexionSistema.Estrella ? tensionFaseV * Raiz3 : tensionFaseV;

    public static decimal TensionFaseDesdeLinea(decimal tensionLineaV, ConexionSistema conexion) =>
        conexion == ConexionSistema.Estrella ? tensionLineaV / Raiz3 : tensionLineaV;

    public static decimal CorrienteLineaDesdeFase(decimal corrienteFaseA, ConexionSistema conexion) =>
        conexion == ConexionSistema.Delta ? corrienteFaseA * Raiz3 : corrienteFaseA;

    public static decimal CorrienteFaseDesdeLinea(decimal corrienteLineaA, ConexionSistema conexion) =>
        conexion == ConexionSistema.Delta ? corrienteLineaA / Raiz3 : corrienteLineaA;

    /// <summary>
    /// S = √3 · V_línea · I_línea (trifásico balanceado). Es la forma práctica de la
    /// fórmula porque V e I casi siempre se capturan en línea (lo que mide una pinza
    /// amperimétrica o un multímetro entre dos fases en campo), sin importar si el
    /// sistema por dentro es estrella o delta — la conexión ya quedó absorbida en
    /// cómo se definen V_línea e I_línea.
    /// </summary>
    public static decimal PotenciaAparenteTrifasicaVA(decimal tensionLineaV, decimal corrienteLineaA) =>
        Raiz3 * tensionLineaV * corrienteLineaA;

    public static decimal CorrienteLineaDesdePotencia(decimal potenciaAparenteVA, decimal tensionLineaV)
    {
        if (tensionLineaV <= 0)
            throw new ArgumentOutOfRangeException(nameof(tensionLineaV), "La tensión de línea debe ser mayor a cero.");

        return potenciaAparenteVA / (Raiz3 * tensionLineaV);
    }

    /// <summary>Monofásico: S = V · I directo, sin raíz de 3 — no hay tres fasores a 120° que combinar, solo una onda.</summary>
    public static decimal CorrienteMonofasicaDesdePotencia(decimal potenciaAparenteVA, decimal tensionV)
    {
        if (tensionV <= 0)
            throw new ArgumentOutOfRangeException(nameof(tensionV), "La tensión debe ser mayor a cero.");

        return potenciaAparenteVA / tensionV;
    }
}
