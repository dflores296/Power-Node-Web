namespace PowerNode.DesignSuite.Domain.Entregables;

/// <summary>
/// Los <b>números intermedios</b> de un cálculo: los factores que se aplicaron, la capacidad mínima
/// antes y después de corregirla, la ampacidad del conductor elegido y la impedancia con la que se
/// calculó la caída de tensión.
///
/// <para>
/// <b>Existe porque una memoria de cálculo tiene que poder recalcularse.</b> Quien la revisa —una
/// unidad de verificación, el de la suministradora, otro ingeniero— toma la corriente, los factores
/// y el calibre impresos, aplica la fórmula del artículo citado y debe llegar <b>al mismo número</b>.
/// Si no llega, el documento no vale. Y hasta el 2026-08-19 estos valores existían nada más
/// <b>dentro de una frase</b> del texto de la memoria: el motor los calculaba, los redactaba y los
/// tiraba, así que el documento podía enseñar el resultado pero no el camino.
/// </para>
///
/// <para>
/// Es un tipo <b>owned</b> y no una tabla: no tiene vida sin su cálculo, nadie lo consulta por su
/// cuenta y se borra con él.
/// </para>
/// </summary>
public class DetalleMemoria
{
    /// <summary>
    /// Capacidad mínima que exige la norma para la protección — 210-20(a) en un derivado, 215-3 en un
    /// alimentador. Es la carga no continua más la continua afectada por su factor (125 %, o 100 %
    /// cuando el ensamble está aprobado para operación al 100 %).
    /// </summary>
    public decimal CapacidadMinimaA { get; set; }

    /// <summary>Factor por temperatura ambiente — Tabla 310-15(b)(2)(a). 1 = sin corrección.</summary>
    public decimal FactorTemperatura { get; set; } = 1m;

    /// <summary>Factor por agrupamiento — Tabla 310-15(b)(3)(a). 1 = sin ajuste.</summary>
    public decimal FactorAgrupamiento { get; set; } = 1m;

    /// <summary>
    /// La capacidad mínima de conducción <b>por hilo de fase</b>, ya corregida:
    /// <c>Icm = In / (FT × FA × hilos por fase)</c>. Es el número con el que se entra a la tabla de
    /// ampacidades, y la sección 4 de la memoria lo muestra como fracción.
    /// </summary>
    public decimal CapacidadMinimaCorregidaA { get; set; }

    /// <summary>
    /// Ampacidad utilizable del juego que se instala (ya por el número de conductores en paralelo).
    /// <b>0 = no se pudo determinar</b> —el calibre no tiene valor tabulado en la columna de la
    /// terminal— y quien lo lea debe tratarlo como "no se sabe", nunca como cero amperes.
    /// </summary>
    public decimal AmpacidadConductorA { get; set; }

    /// <summary>Temperatura de la terminal que impuso 110-14(c)(1): 60 o 75 °C.</summary>
    public int TemperaturaTerminalesC { get; set; }

    /// <summary>Temperatura del aislamiento capturado — Tabla 310-104(a).</summary>
    public int TemperaturaAislamientoC { get; set; }

    /// <summary>Resistencia del conductor elegido, en ohm/km — Tabla 9.</summary>
    public decimal ResistenciaOhmKm { get; set; }

    /// <summary>Reactancia del conductor elegido, en ohm/km — Tabla 9.</summary>
    public decimal ReactanciaOhmKm { get; set; }

    /// <summary>Caída de tensión en volts. El porcentaje sale de dividirla entre la tensión.</summary>
    public decimal CaidaTensionV { get; set; }
}
