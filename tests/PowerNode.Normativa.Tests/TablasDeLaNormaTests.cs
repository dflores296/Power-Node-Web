using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.DesignSuite.Normativa;
using Xunit;

namespace PowerNode.Normativa.Tests;

/// <summary>
/// Las tablas leídas del JSON tienen que dar <b>exactamente</b> los mismos valores que la versión de
/// escritorio leyendo SQL Server. Los números de abajo NO se copiaron de la implementación: son los
/// mismos que <c>TablasNomTests</c> del repo de escritorio ya verificó a mano contra el PDF del DOF.
///
/// <para>
/// Si uno de estos falla, la traducción de SQL Server a JSON perdió o torció un dato — que es
/// justo el riesgo que esta suite existe para atrapar.
/// </para>
/// </summary>
public class TablasDeLaNormaTests
{
    private static readonly FuenteTablasJson Fuente = new(
        File.ReadAllText(Path.Combine(
            AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    private static ICatalogoCalibres Calibres() => new CatalogoCalibresJson(Fuente);

    [Fact]
    public void Tabla8_AreaYResistenciaDeCalibresConocidos()
    {
        var catalogo = Calibres();

        var doceAwg = catalogo.BuscarPorDesignacion("12");
        Assert.NotNull(doceAwg);
        Assert.Equal(3.31m, doceAwg!.AreaMm2);
        Assert.Equal(6.5m, doceAwg.ResistenciaCuNoCubiertoOhmKm);
        Assert.Equal(6.73m, doceAwg.ResistenciaCuRecubiertoOhmKm);
        Assert.False(doceAwg.PermiteParalelo); // < 53.5 mm²

        var unoCero = catalogo.BuscarPorDesignacion("1/0");
        Assert.NotNull(unoCero);
        Assert.Equal(53.49m, unoCero!.AreaMm2);
        Assert.Equal(0.66m, unoCero.ResistenciaAlOhmKm);
        Assert.True(unoCero.PermiteParalelo); // 310-10(h)(1)

        // 36 filas de datos en la Tabla 8, colapsando sólido/trenzado: 30 designaciones distintas.
        Assert.Equal(30, catalogo.Listar().Count);
    }

    [Fact]
    public void Ampacidad_310_15_b_16_ColumnaCorrectaPorMaterialYTemperatura()
    {
        var catalogo = Calibres();
        var ampacidad = new TablaAmpacidadJson(Fuente, catalogo);
        var doceAwg = catalogo.BuscarPorDesignacion("12")!;

        Assert.Equal(20m, ampacidad.Ampacidad(doceAwg, MaterialConductor.Cobre, TemperaturaAislamiento.T60));
        Assert.Equal(25m, ampacidad.Ampacidad(doceAwg, MaterialConductor.Cobre, TemperaturaAislamiento.T75));
        Assert.Equal(30m, ampacidad.Ampacidad(doceAwg, MaterialConductor.Cobre, TemperaturaAislamiento.T90));
    }

    [Fact]
    public void Proteccion_240_6a_ValoresEstandarizados()
    {
        var tabla = new TablaProteccionEstandarJson(Fuente);

        // Los primeros de la lista de 240-6(a), tal cual los enumera el texto de la norma:
        // "15, 16, 20, 25, 30, 32, 35, 40, 45, 50, 60, 63, 70, 80, 90, 100..."
        Assert.Contains(15m, tabla.ValoresEstandar);
        Assert.Contains(20m, tabla.ValoresEstandar);
        Assert.Contains(30m, tabla.ValoresEstandar);

        // ⚠ LA NOM NO ES EL NEC AQUÍ. Su lista incluye los valores IEC —16, 32, 63— que el NEC
        // no tiene. Esta prueba se escribió primero esperando 20 A para 15.1 A, por costumbre del
        // NEC, y falló: el inmediato superior es 16 A. Queda fijado a propósito, porque es
        // exactamente el tipo de diferencia que se cuela sin que nadie la note.
        Assert.Contains(16m, tabla.ValoresEstandar);
        Assert.Contains(32m, tabla.ValoresEstandar);
        Assert.Contains(63m, tabla.ValoresEstandar);

        // El "inmediato superior" es la regla que usa todo el motor al elegir interruptor.
        Assert.Equal(15m, tabla.SiguienteEstandar(7.08m));
        Assert.Equal(16m, tabla.SiguienteEstandar(15.1m));
        Assert.Equal(20m, tabla.SiguienteEstandar(16.1m));
    }

    [Fact]
    public void Impedancia_Tabla9_ResistenciaYReactanciaDe12Awg()
    {
        var catalogo = Calibres();
        var impedancia = new TablaImpedanciaJson(Fuente, catalogo);
        var doceAwg = catalogo.BuscarPorDesignacion("12")!;

        // Los valores que sostienen el cálculo de caída de tensión verificado a mano:
        // e = 2 × L × In × (R·cosθ + X·senθ), con R = 6.6 Ω/km en PVC para 12 AWG de cobre.
        var z = impedancia.Impedancia(doceAwg, MaterialConductor.Cobre, MaterialCanalizacion.Pvc);
        Assert.NotNull(z);
        Assert.Equal(6.6m, z!.Value.ROhmKm);
        Assert.Equal(0.177m, z.Value.XOhmKm);
    }

    [Fact]
    public void PuestaTierra_250_122_CalibreMinimoPorProteccion()
    {
        var catalogo = Calibres();
        var tierra = new TablaPuestaTierraJson(Fuente, catalogo);

        // 250-122: hasta 20 A, cobre 12 AWG.
        Assert.Equal("12", tierra.CalibreMinimo(20m, MaterialConductor.Cobre).Designacion);
    }

    [Fact]
    public void LasCatorceTablasTraenSuFechaDeVerificacion()
    {
        // La procedencia no es adorno: dice que ese número se cotejó celda por celda contra el PDF
        // del DOF, y en qué fecha. Una tabla sin fecha es una tabla en la que no se puede confiar.
        foreach (var id in new[]
                 {
                     "8", "9", "310-15(b)(16)", "310-15(b)(17)", "310-15(b)(2)(a)",
                     "310-15(b)(3)(a)", "310-104(a)", "250-122",
                     "430-247", "430-248", "430-249", "430-250", "430-52", "430-7(b)",
                 })
            Assert.False(string.IsNullOrWhiteSpace(Fuente.VerificadaEl(id)),
                $"La tabla {id} no trae fecha de verificación.");
    }
}
