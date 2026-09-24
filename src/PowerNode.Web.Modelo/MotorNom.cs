using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Normativa;

namespace PowerNode.Web.Modelo;

/// <summary>
/// Arma el motor de cálculo con las tablas de la norma leídas del JSON.
///
/// <para>
/// Es lo único que este proyecto web aporta al cálculo: <b>conectar las piezas</b>. Ni una regla de
/// la NOM vive aquí — todas vienen de <c>Calculo</c> y <c>Normativa</c>, copiadas y verificadas.
/// Si un número sale mal, el defecto está allá, no en esta clase.
/// </para>
///
/// <para>
/// <b>Las dos calculadoras que expone son las dos mitades del cuadro de carga:</b> cada renglón es
/// un circuito derivado (Art. 210) y el renglón de abajo —el alimentador con el interruptor
/// principal del tablero— es un alimentador (Art. 215). El Excel original las trata igual: la fila
/// 79 tiene exactamente las mismas fórmulas que las filas 34-76.
/// </para>
/// </summary>
public sealed class MotorNom
{
    /// <summary>
    /// Arma el motor con el JSON de la norma ya leído. Público a propósito: es lo que deja probar
    /// el cuadro completo sin servidor ni <c>HttpClient</c> de por medio.
    /// </summary>
    public MotorNom(string jsonDeLasTablas) : this(new FuenteTablasJson(jsonDeLasTablas))
    {
    }

    private MotorNom(FuenteTablasJson fuente)
    {
        Fuente = fuente;
        Calibres = new CatalogoCalibresJson(fuente);

        var ampacidad = new TablaAmpacidadJson(fuente, Calibres);
        var proteccion = new TablaProteccionEstandarJson(fuente);
        var temperatura = new TablaCorreccionTemperaturaJson(fuente);
        var agrupamiento = new TablaAgrupamientoJson(fuente);
        var tierra = new TablaPuestaTierraJson(fuente, Calibres);
        var impedancia = new TablaImpedanciaJson(fuente, Calibres);
        var aislamiento = new TablaAislamientoJson(fuente);

        // Un juego de calculadoras por serie de interruptores: la misma norma, elegida dentro de la
        // familia que se instala. Ver SerieDeInterruptores.
        foreach (var serie in Enum.GetValues<SerieDeInterruptores>())
        {
            var deLaSerie = new ProteccionEstandarDeLaSerie(proteccion, serie);
            _noMotor[serie] = new CalculadoraCircuitoDerivadoNoMotor(
                Calibres, ampacidad, deLaSerie, temperatura, agrupamiento, tierra, impedancia, aislamiento);
            _alimentador[serie] = new CalculadoraAlimentador(
                Calibres, ampacidad, deLaSerie, temperatura, agrupamiento, tierra, impedancia, aislamiento);
        }

        ProteccionEstandar = proteccion;
        Ampacidad = ampacidad;
        Dimensiones = new TablaDimensionesConductorJson(fuente);
        Ocupacion = new CalculadoraOcupacion(new TablaOcupacionJson(fuente), new TablaTuboConduitJson(fuente), Dimensiones);
        Aislamiento = aislamiento;
        Azotea = new TablaTemperaturaAzoteaJson(fuente);
        Agrupamiento = agrupamiento;
    }

    private readonly Dictionary<SerieDeInterruptores, CalculadoraCircuitoDerivadoNoMotor> _noMotor = [];
    private readonly Dictionary<SerieDeInterruptores, CalculadoraAlimentador> _alimentador = [];

    public FuenteTablasJson Fuente { get; }
    public ICatalogoCalibres Calibres { get; }
    public ITablaProteccionEstandar ProteccionEstandar { get; }

    /// <summary>La Tabla 310-15(b)(16). La consulta el desglose para enseñar la ampacidad de tabla de cada columna.</summary>
    public ITablaAmpacidad Ampacidad { get; }

    /// <summary>Cada renglón del cuadro: circuito derivado de Alumbrado, Contactos o Equipo (Art. 210).</summary>
    public CalculadoraCircuitoDerivadoNoMotor NoMotor(SerieDeInterruptores serie) => _noMotor[serie];

    /// <summary>
    /// El renglón del alimentador (Art. 215). Su <c>ProteccionA</c> <b>es</b> el interruptor
    /// principal del tablero — el mismo número, no dos cálculos: así lo resuelve la versión de
    /// escritorio (<c>CalculoTablero.BreakerPrincipalA</c>) y así lo resuelve el Excel (la celda
    /// «INT. PPAL.» del encabezado es una referencia a la fila del alimentador).
    /// </summary>
    public CalculadoraAlimentador Alimentador(SerieDeInterruptores serie) => _alimentador[serie];

    /// <summary>
    /// Tabla 310-104(a): los aislamientos que reconoce el motor. El selector de «Aislamiento» sale de
    /// aquí, no de una lista aparte — antes la pantalla ofrecía 7 de los 17.
    /// </summary>
    public ITablaAislamiento Aislamiento { get; }

    /// <summary>Tablas 5 y 8 del Capítulo 10: si un aislamiento trae dimensiones o pide las del fabricante.</summary>
    public ITablaDimensionesConductor Dimensiones { get; }

    /// <summary>El tamaño de cada canalización — Capítulo 10, Tablas 1, 4, 5 y 8.</summary>
    public CalculadoraOcupacion Ocupacion { get; }

    /// <summary>Tabla 310-15(b)(3)(a): el factor que resulta de los portadores de cada canalización.</summary>
    public ITablaAgrupamiento Agrupamiento { get; }

    /// <summary>Tabla 310-15(b)(3)(c): lo que se suma a la temperatura de un tubo en azotea al sol.</summary>
    public ITablaTemperaturaAzotea Azotea { get; }

    /// <summary>Descarga el JSON de la norma y arma el motor. Se hace una sola vez, al arrancar.</summary>
    public static async Task<MotorNom> CargarAsync(HttpClient http) =>
        new(await http.GetStringAsync("datos/tablas-nom.json"));
}
