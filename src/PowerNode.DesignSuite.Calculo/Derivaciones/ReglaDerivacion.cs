namespace PowerNode.DesignSuite.Calculo.Derivaciones;

/// <summary>
/// Los <b>cinco</b> casos de 240-21(b) bajo los que se permite derivar conductores de un alimentador
/// <b>sin protección contra sobrecorriente en la derivación</b>.
///
/// <para>
/// <b>Por qué esto existe (2026-08-25).</b> Hasta esta fecha el modelo daba por hecho que de cada
/// interruptor del tablero salía exactamente un destino: <c>Alimentador.ElementoDestino</c> es uno a
/// uno. En obra no es así — lo dijo el usuario: <i>«ese alimentador puede alimentar varias cargas, no
/// solo un tablero hijo»</i> —, y la norma no solo lo permite: le dedica un inciso entero con cinco
/// juegos de condiciones. Sin esto, un proyecto con derivaciones había que capturarlo mintiendo
/// (dos interruptores donde hay uno) y el papel describía una instalación que no es la construida.
/// </para>
///
/// <para>
/// <b>La regla que abre el inciso y que ninguna de las cinco puede saltarse:</b> «Las disposiciones
/// de 240-4(b) no se deben permitir para conductores de derivación». O sea que el redondeo al
/// siguiente tamaño estándar de protección —el que el motor sí aplica en un circuito normal desde el
/// bloque 8.6— <b>está prohibido aquí</b>. Un conductor de derivación se dimensiona por su ampacidad
/// mínima exigida y por nada más.
/// </para>
/// </summary>
public enum ReglaDerivacion
{
    /// <summary>240-21(b)(1) — derivaciones no mayores a 3.00 m.</summary>
    TresMetros,

    /// <summary>240-21(b)(2) — derivaciones no mayores a 8.00 m.</summary>
    OchoMetros,

    /// <summary>240-21(b)(3) — derivaciones que alimentan un transformador (primario + secundario ≤ 8.00 m).</summary>
    Transformador,

    /// <summary>240-21(b)(4) — derivaciones de más de 8.00 m en naves industriales de gran altura.</summary>
    NaveIndustrial,

    /// <summary>240-21(b)(5) — conductores localizados en el exterior del edificio o estructura.</summary>
    Exterior,
}

/// <summary>
/// En qué quedó una de las condiciones de una regla. <b>«No se puede verificar» no es «no cumple»</b>
/// — es la misma regla que ya rige en cortocircuito y en el catálogo de I-Line: un dato que no se
/// capturó no se declara incumplido, se declara faltante.
/// </summary>
public enum EstadoCondicion
{
    /// <summary>La condición se revisó contra datos capturados y se cumple.</summary>
    Cumple,

    /// <summary>La condición se revisó y <b>no</b> se cumple.</summary>
    NoCumple,

    /// <summary>Falta el dato que haría falta para revisarla. No cuenta como incumplimiento.</summary>
    NoVerificable,
}

/// <summary>Una condición de 240-21(b), con la cláusula exacta que la exige y en qué quedó.</summary>
/// <param name="Clausula">El identificador de la norma, p.ej. <c>240-21(b)(2)(1)</c>.</param>
/// <param name="Texto">Qué se revisó, redactado para que se pueda leer en pantalla y en la memoria.</param>
/// <param name="Estado">Cumple, no cumple, o falta el dato.</param>
public sealed record CondicionDerivacion(string Clausula, string Texto, EstadoCondicion Estado);
