namespace PowerNode.DesignSuite.Calculo.Magnitudes;

/// <summary>
/// Prefijos SI usados en cálculo eléctrico (de pico a giga). Todo el motor de cálculo
/// trabaja internamente en unidad base sin prefijo (V, A, W, VA, VAR, Ω) — estos
/// helpers son la única puerta de entrada/salida para convertir hacia/desde un valor
/// "de placa" o "de captura" que venga en kV, kVA, mA, etc., para que esa conversión
/// pase por un solo lugar y no se repita (ni se equivoque) en cada caso de cálculo.
/// </summary>
public enum PrefijoSI
{
    Pico,
    Nano,
    Micro,
    Mili,
    Unidad,
    Kilo,
    Mega,
    Giga,
}

public static class PrefijoSIExtensions
{
    public static decimal Multiplicador(this PrefijoSI prefijo) => prefijo switch
    {
        PrefijoSI.Pico => 0.000000000001m,
        PrefijoSI.Nano => 0.000000001m,
        PrefijoSI.Micro => 0.000001m,
        PrefijoSI.Mili => 0.001m,
        PrefijoSI.Unidad => 1m,
        PrefijoSI.Kilo => 1000m,
        PrefijoSI.Mega => 1000000m,
        PrefijoSI.Giga => 1000000000m,
        _ => throw new ArgumentOutOfRangeException(nameof(prefijo)),
    };

    /// <summary>Convierte un valor expresado con el prefijo dado (p.ej. 13.2 kV) a la unidad base (13200 V).</summary>
    public static decimal AUnidadBase(this decimal valor, PrefijoSI prefijo) => valor * prefijo.Multiplicador();

    /// <summary>Convierte un valor en unidad base (p.ej. 13200 V) a la magnitud que tendría con el prefijo dado (13.2 kV).</summary>
    public static decimal DesdeUnidadBase(this decimal valorBase, PrefijoSI prefijo) => valorBase / prefijo.Multiplicador();
}
