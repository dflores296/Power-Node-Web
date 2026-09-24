using PowerNode.DesignSuite.Calculo.Tableros;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// La cabecera del cuadro de carga: <b>quién es el tablero, de qué sistema cuelga y con qué
/// condiciones se calcula</b>.
///
/// <para>
/// <b>Los campos salen del Excel</b> (<c>CC_NOM_2012_nuevo_nube.xlsx</c>, hoja «Cuadro de Carga»,
/// bloque de identificación de las filas 19 a 26), no de una idea de qué debería llevar un cuadro
/// de carga: TABLERO, CLAVE, UBICACIÓN, PROYECTO, CLIENTE, quién diseñó / revisó / aprobó, la
/// fecha y la revisión a la izquierda y a la derecha; MONTAJE, MATERIAL DE BARRAS y GABINETE NEMA
/// en medio; y TENSIÓN F-F, TENSIÓN F-N, FRECUENCIA, No. DE FASES y No. DE HILOS como el sistema.
/// </para>
///
/// <para>
/// <b>Lo que el Excel sacaba del catálogo no está</b> —CATÁLOGO, MARCA, INTERIOR, CAJA, FRENTE,
/// DIMENSIONES, CAPACIDAD—: ese bloque son ocho <c>INDEX/MATCH</c> contra la base de tableros
/// Square D, y este repo es público. Ver <c>docs/decisiones/sin-catalogo-square-d.md</c>. Lo único
/// que sobrevive de ahí es <see cref="CapacidadBarraA"/>, y se captura a mano porque sin ella el
/// 408-36 no se puede verificar.
/// </para>
/// </summary>
public sealed class DatosDelTablero
{
    // ---- Identificación (Excel B19:T26 y BJ19:BM26) --------------------------------------------

    public string Tablero { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Proyecto { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Diseno { get; set; } = string.Empty;
    public string Reviso { get; set; } = string.Empty;
    public string Aprobo { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;

    // ---- Gabinete ------------------------------------------------------------------------------

    /// <summary>SOBREPONER o EMPOTRAR, como el Excel (J23). Texto libre: no hay catálogo que validar.</summary>
    public string Montaje { get; set; } = "Sobreponer";

    /// <summary>Material de las barras (J24). Es dato del tablero, no del conductor de los circuitos.</summary>
    public MaterialConductor MaterialBarras { get; set; } = MaterialConductor.Cobre;

    /// <summary>Tipo de envolvente NEMA (J25): 1, 3R, 12… Texto libre por la misma razón que <see cref="Montaje"/>.</summary>
    public string GabineteNema { get; set; } = "1";

    /// <summary>
    /// Espacios de un polo del gabinete. <b>42 es el tope del formato</b>: el Excel trae 21 renglones
    /// de nones y 21 de pares, y para un tablero más chico se ocultaban renglones a mano. Aquí no
    /// hace falta ocultar nada — el cuadro se dibuja con los espacios que se declaren.
    /// </summary>
    public int NumeroEspacios { get; set; } = 24;

    /// <summary>Zapatas principales o interruptor principal propio. Decide qué dice el 408-36.</summary>
    public TipoAcometidaTablero TipoAcometida { get; set; } = TipoAcometidaTablero.InterruptorPrincipal;

    /// <summary>
    /// Capacidad nominal de la barra, en amperes. <b><c>null</c> es válido y no dispara nada</b>:
    /// declarar un incumplimiento por un dato que no se capturó es el error contrario al que la
    /// verificación evita. Ver <see cref="Verificacion408_36"/>.
    /// </summary>
    public decimal? CapacidadBarraA { get; set; }

    /// <summary>
    /// El tablero es el medio de desconexión de la acometida: el primero después del medidor. Solo
    /// entonces aplica el mínimo de 230-79, que depende de <see cref="Inmueble"/> — R-11. Falso por
    /// omisión. Sustituye al «Mínimo del principal (A)», que solo avisaba (David, 2026-09-24).
    /// </summary>
    public bool EsEquipoDeAcometida { get; set; }

    /// <summary>El inmueble que sirve la acometida. Solo cuenta con <see cref="EsEquipoDeAcometida"/>.</summary>
    public TipoDeInmueble Inmueble { get; set; } = TipoDeInmueble.Otro;

    /// <summary>
    /// El mínimo del principal por 230-79, con su referencia. <c>null</c> si el tablero no es equipo de
    /// acometida o si es vivienda unifamiliar, donde la norma dice «según la carga conectada»: sin
    /// número, manda el cálculo.
    /// </summary>
    public (decimal Amperes, string Referencia)? Minimo230_79 => !EsEquipoDeAcometida
        ? null
        : Inmueble switch
        {
            TipoDeInmueble.ViviendaPopular => (30m, "230-79(c)"),
            TipoDeInmueble.Otro => (60m, "230-79(d)"),
            _ => null,
        };

    /// <summary>
    /// La familia de interruptores que se instala, que decide de qué tamaños de la 240-6(a) se
    /// escoge. <b>Centro de carga (NEMA) por omisión</b> — decisión de David del 2026-09-23.
    /// </summary>
    public SerieDeInterruptores SerieInterruptores { get; set; } = SerieDeInterruptores.CentroDeCargaNema;

    // ---- Sistema (Excel T22:T26) ---------------------------------------------------------------

    public decimal TensionFaseFaseV { get; set; } = 220m;
    public int Fases { get; set; } = 3;
    public int Hilos { get; set; } = 4;
    public int FrecuenciaHz { get; set; } = 60;

    /// <summary>
    /// Fases <b>y</b> hilos juntos, que es lo único que dice cuántas barras energizadas hay. Un
    /// 1F-3H tiene dos y un 1F-2H tiene una, y las dos cosas son «1 fase».
    /// </summary>
    public SistemaTablero Sistema => new(Fases, Hilos);

    /// <summary>
    /// La tensión fase-neutro que le corresponde al sistema.
    ///
    /// <para>
    /// <b>No es siempre ÷√3</b>, que es lo que hace el Excel (<c>=ROUND(T22/SQRT(3),1)</c>) y lo que
    /// hacía esta pantalla antes: en un 1F-3H la derivación central es <b>÷2</b> —240 da 120, no
    /// 138.6— y en un 1F-2H la tensión capturada ya <i>es</i> la fase-neutro. Lo resuelve
    /// <see cref="SistemaDelTablero"/>, que es el único lugar del motor donde vive esa regla.
    /// </para>
    /// </summary>
    public decimal TensionFaseNeutroV => SistemaDelTablero.TensionFaseNeutro(TensionFaseFaseV, Sistema);

    /// <summary>Las barras del tablero: 'A', 'B', 'C' según el sistema.</summary>
    public IReadOnlyList<char> Barras => SistemaDelTablero.Barras(Sistema);

    /// <summary>Máximo de polos de un interruptor de este tablero: tantos como barras.</summary>
    public int MaximoPolos => SistemaDelTablero.MaximoPolos(Sistema);

    /// <summary>
    /// Cómo se rotula el sistema en el encabezado del cuadro: «3F-4H (estrella)». Sale de la
    /// configuración que reconoce el motor, no de concatenar fases e hilos, para que un 1F-3H diga
    /// que es derivación central y no se lea como un monofásico cualquiera.
    /// </summary>
    public string EtiquetaSistema => SistemaDelTablero.De(Sistema) switch
    {
        ConfiguracionTablero.UnaFaseDosHilos => "1F-2H",
        ConfiguracionTablero.UnaFaseTresHilos => "1F-3H (derivación central)",
        ConfiguracionTablero.DosFasesDeEstrella => "2F de estrella",
        ConfiguracionTablero.TresFasesTresHilos => "3F-3H (delta, sin neutro)",
        _ => "3F-4H (estrella)",
    };

    public bool UsaInterruptorPrincipal => TipoAcometida == TipoAcometidaTablero.InterruptorPrincipal;

    // ---- Factores de demanda del resumen (Excel AX79 y AZ79) ------------------------------------

    /// <summary>
    /// Factor de demanda de la carga continua acumulada. <b>Va en el alimentador, no en el
    /// derivado</b> — 220-40 lo pone sobre la carga acumulada y 220-42 lo prohíbe expresamente en el
    /// circuito derivado. El Excel lo lleva por renglón <i>y</i> en el total; aquí solo en el total,
    /// que es donde la norma lo admite.
    /// </summary>
    public decimal FactorDemandaContinua { get; set; } = 1m;

    /// <summary>Factor de demanda de la carga no continua acumulada. Ver <see cref="FactorDemandaContinua"/>.</summary>
    public decimal FactorDemandaNoContinua { get; set; } = 1m;

    /// <summary>Algún factor de demanda reduce la carga: entonces la memoria pide justificación — R-12.</summary>
    public bool ReduceCargaPorDemanda => FactorDemandaContinua < 1m || FactorDemandaNoContinua < 1m;

    /// <summary>Con qué se justifica el factor de demanda. Varias a la vez. Solo cuenta con <see cref="ReduceCargaPorDemanda"/>.</summary>
    public HashSet<JustificacionFactorDemanda> Justificaciones { get; } = [];

    /// <summary>El texto de «Otra — criterio del proyectista».</summary>
    public string JustificacionOtra { get; set; } = string.Empty;

    /// <summary>
    /// La justificación como la imprime la memoria, en el orden de la lista. <c>null</c> si falta: sin
    /// ninguna escogida, o solo «Otra» sin texto.
    /// </summary>
    public string? JustificacionDelFactorDeDemanda
    {
        get
        {
            var partes = Enum.GetValues<JustificacionFactorDemanda>()
                .Where(Justificaciones.Contains)
                .Select(j => j == JustificacionFactorDemanda.Otra
                    ? (string.IsNullOrWhiteSpace(JustificacionOtra) ? null : $"Criterio del proyectista: {JustificacionOtra.Trim()}")
                    : j.Nombre())
                .OfType<string>()
                .ToList();
            return partes.Count == 0 ? null : string.Join("; ", partes);
        }
    }

    // ---- Condiciones de cálculo (Excel columnas DA a DR, iguales en todos los renglones) --------

    public MaterialConductor MaterialConductor { get; set; } = MaterialConductor.Cobre;

    /// <summary>
    /// El aislamiento del conductor, con su designación de la Tabla 310-104(a): decide la columna
    /// de temperatura de la Tabla 310-15(b)(16) en la que se aplican los factores de corrección
    /// (110-14(c)). <b>THHN por omisión</b>, que es lo que el programa suponía sin preguntar. Fijo
    /// en THHN, un circuito instalado con THW-LS podía salir con un calibre de menos: con 9
    /// conductores agrupados, 20 A continuos piden 10 AWG en THHN y 8 AWG en THW-LS.
    /// </summary>
    public string TipoAislamiento { get; set; } = "THHN";

    /// <summary>
    /// Lugar seco, o húmedo/mojado. Algunos aislamientos cambian de temperatura según el lugar
    /// (THHW-LS: 90 °C seco, 75 °C mojado) y otros no se permiten fuera de lugar seco (THHN) —
    /// Tabla 310-104(a).
    /// </summary>
    public bool LugarSeco { get; set; } = true;

    /// <summary>
    /// Las terminales del circuito —interruptor y equipo— están aprobadas e identificadas para 75 °C:
    /// 110-14(c)(1)a.(3). Muchos interruptores de centro de carga vienen marcados 60/75 °C. <b>Falso por
    /// omisión</b>: sin declaración, 60 °C hasta 100 A, que es la regla general y lo que se calculaba.
    /// </summary>
    public bool TerminalesMarcadas75C { get; set; }

    public MaterialCanalizacion MaterialCanalizacion { get; set; } = MaterialCanalizacion.Pvc;
    public decimal TemperaturaAmbienteC { get; set; } = 30m;
    public int ConductoresAgrupados { get; set; } = 3;
    // SIN FACTOR DE POTENCIA DEL TABLERO. Un tablero no tiene F.P.: lo tienen sus cargas. Cada
    // circuito lleva el suyo (CircuitoDelCuadro.FactorPotencia) y el del alimentador resulta de
    // combinarlos. Decidido por David el 2026-09-23 — docs/decisiones/factor-de-potencia-por-circuito.md.
    public decimal CaidaMaxDerivadoPct { get; set; } = 3m;

    /// <summary>
    /// Límite de caída del alimentador, por tramo: <b>2 % por omisión</b>, para que con el 3 % del
    /// derivado la suma quede en el 5 % combinado — 215-2(a)(4) NOTA 2, 210-19(a)(1) NOTA 4 (R-15).
    /// Estuvo fijo en 5 % (R-01) y luego en 3 %, que con el derivado sumaba 6 %.
    /// </summary>
    public decimal CaidaMaxAlimentadorPct { get; set; } = 2m;

    /// <summary>
    /// Caída combinada alimentador + derivado hasta la salida más lejana: 5 % — 215-2(a)(4) NOTA 2 y
    /// 210-19(a)(1) NOTA 4. Fijo: lo da la norma, no el proyecto. Solo avisa.
    /// </summary>
    public const decimal CaidaMaxCombinadaPct = 5m;

    /// <summary>
    /// Aviso cuando los dos límites por tramo suman más que el combinado: con ellos, un circuito puede
    /// quedar arriba del 5 % aunque cada tramo cumpla el suyo. <c>null</c> si suman 5 % o menos.
    /// </summary>
    public string? AvisoLimitesDeCaida =>
        CaidaMaxAlimentadorPct + CaidaMaxDerivadoPct > CaidaMaxCombinadaPct
            ? $"Los límites suman {CaidaMaxAlimentadorPct + CaidaMaxDerivadoPct:0.##} %: un circuito puede quedar arriba del " +
              $"{CaidaMaxCombinadaPct:0} % combinado — 210-19(a)(1) NOTA 4, 215-2(a)(4) NOTA 2."
            : null;

    /// <summary>Longitud del alimentador que llega al tablero, en metros — la «L» de la fila 79 del Excel.</summary>
    public decimal LongitudAlimentadorM { get; set; } = 20m;

    /// <summary>
    /// El ensamble (gabinete + interruptor principal) está aprobado para operar al 100 % de su
    /// valor nominal — 215-3. <b>Falso por omisión</b>, que es la regla general: es dato de placa y
    /// de marcado, no una opción de diseño.
    /// </summary>
    public bool ConjuntoAprobado100Pct { get; set; }
}

/// <summary>El inmueble de la acometida, para el mínimo del medio de desconexión — 230-79.</summary>
public enum TipoDeInmueble
{
    /// <summary>230-79(c): según la carga conectada.</summary>
    ViviendaUnifamiliar,

    /// <summary>230-79(c): vivienda popular de hasta 60 m², no menor que 30 A.</summary>
    ViviendaPopular,

    /// <summary>230-79(d): comercio, oficina, industria y todo lo demás, no menor que 60 A.</summary>
    Otro,
}

public static class TiposDeInmueble
{
    public static string Nombre(this TipoDeInmueble inmueble) => inmueble switch
    {
        TipoDeInmueble.ViviendaUnifamiliar => "Vivienda unifamiliar",
        TipoDeInmueble.ViviendaPopular => "Vivienda popular (hasta 60 m²)",
        _ => "Otro (comercio, oficina, industria)",
    };
}
