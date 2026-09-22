namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Resultado propio del transformador -- corriente nominal de placa (Ley de Ohm,
/// <see cref="Calculo.Casos.CalculadoraTransformador"/>) y su protección contra sobrecorriente
/// (Art. 450-3(b), <see cref="Calculo.Casos.CalculadoraProteccionTransformador"/>). Antes del
/// bloque 10 esto se calculaba al vuelo dentro de la cascada solo para alimentar el Alimentador
/// adyacente y nunca se guardaba como resultado propio del transformador.
/// </summary>
public class CalculoTransformador
{
    public int Id { get; set; }
    public int TransformadorId { get; set; }
    public Transformador Transformador { get; set; } = null!;

    public decimal CorrientePrimariaA { get; set; }
    public decimal CorrienteSecundariaA { get; set; }

    public decimal ProteccionPrimarioA { get; set; }

    /// <summary>Null cuando el esquema es "solo primario" -- el secundario no lleva protección propia.</summary>
    public decimal? ProteccionSecundarioA { get; set; }

    /// <summary>Capacidad de placa corregida por la altitud del proyecto. Igual a la de placa hasta los 1000 msnm.</summary>
    public decimal CapacidadCorregidaKva { get; set; }

    /// <summary>
    /// Altitud de diseño con la que se evaluó (1 000 o 2 300 msnm, normalmente). Se guarda porque
    /// es lo que explica que un transformador en la Ciudad de México no derratee: está diseñado
    /// para 2 300 y el sitio está a 2 240.
    /// </summary>
    public decimal AltitudDisenoMsnm { get; set; } = 1000m;

    /// <summary>Temperatura que la Tabla 1 permite a esa altitud, en °C. Es el umbral que decide si hay derrateo.</summary>
    public decimal TemperaturaPermisibleC { get; set; } = 30m;

    /// <summary>Factor de derrateo por altitud aplicado (1 = sin derrateo).</summary>
    public decimal FactorAltitud { get; set; } = 1m;

    /// <summary>
    /// Corriente de cortocircuito disponible en el secundario, suponiendo barra infinita. Null si
    /// no se capturó el %Z. Es el número contra el que se compara la capacidad interruptiva del
    /// tablero que cuelga del transformador.
    /// </summary>
    public decimal? CorrienteCortocircuitoSecundarioA { get; set; }

    /// <summary>Regulación de tensión a plena carga, en por ciento. Null si no se capturó el %Z.</summary>
    public decimal? RegulacionPct { get; set; }

    /// <summary>
    /// Qué tabla se usó para la protección: "450-3(a)" (más de 600 V) o "450-3(b)" (600 V o menos).
    /// Se guarda porque antes NO se elegía: se aplicaba la (b) a todo, incluido un transformador de
    /// acometida de 13 200 V, y no había forma de notarlo revisando el resultado.
    /// </summary>
    public string TablaProteccion { get; set; } = "450-3(b)";

    /// <summary>
    /// Avisos de la NMX-J-116 sobre las condiciones de servicio, uno por renglón, cada uno con su
    /// referencia. Vacío = el transformador está dentro de lo normalizado. Son avisos, no errores:
    /// salirse es decisión del ingeniero, pero tiene que quedar dicho.
    /// </summary>
    public string Advertencias { get; set; } = string.Empty;

    public string TextoMemoria { get; set; } = string.Empty;
}
