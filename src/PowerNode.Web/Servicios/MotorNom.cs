using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Normativa;

namespace PowerNode.Web.Servicios;

/// <summary>
/// Arma el motor de cálculo con las tablas de la norma leídas del JSON.
///
/// <para>
/// Es lo único que este proyecto web aporta al cálculo: <b>conectar las piezas</b>. Ni una regla de
/// la NOM vive aquí — todas vienen de <c>Calculo</c> y <c>Normativa</c>, copiadas y verificadas.
/// Si un número sale mal, el defecto está allá, no en esta clase.
/// </para>
/// </summary>
public sealed class MotorNom
{
    private MotorNom(FuenteTablasJson fuente)
    {
        Fuente = fuente;
        Calibres = new CatalogoCalibresJson(fuente);

        NoMotor = new CalculadoraCircuitoDerivadoNoMotor(
            Calibres,
            new TablaAmpacidadJson(fuente, Calibres),
            new TablaProteccionEstandarJson(fuente),
            new TablaCorreccionTemperaturaJson(fuente),
            new TablaAgrupamientoJson(fuente),
            new TablaPuestaTierraJson(fuente, Calibres),
            new TablaImpedanciaJson(fuente, Calibres),
            new TablaAislamientoJson(fuente));
    }

    public FuenteTablasJson Fuente { get; }
    public ICatalogoCalibres Calibres { get; }
    public CalculadoraCircuitoDerivadoNoMotor NoMotor { get; }

    /// <summary>Descarga el JSON de la norma y arma el motor. Se hace una sola vez, al arrancar.</summary>
    public static async Task<MotorNom> CargarAsync(HttpClient http)
    {
        var json = await http.GetStringAsync("datos/tablas-nom.json");
        return new MotorNom(new FuenteTablasJson(json));
    }
}
