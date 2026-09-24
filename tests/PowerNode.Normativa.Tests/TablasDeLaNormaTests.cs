using PowerNode.DesignSuite.Calculo.Canalizaciones;
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

    // ---- Capítulo 10: canalizaciones (nacido en la web, 2026-09-24) -------------------------

    [Fact]
    public void Tabla1_PorcentajeDeOcupacion()
    {
        var tabla = new TablaOcupacionJson(Fuente);
        Assert.Equal(53m, tabla.PorcentajeMaximo(1));
        Assert.Equal(31m, tabla.PorcentajeMaximo(2));
        Assert.Equal(40m, tabla.PorcentajeMaximo(3));
        Assert.Equal(40m, tabla.PorcentajeMaximo(40));
    }

    [Fact]
    public void Tabla4_BloquesYTamanos()
    {
        var tabla = new TablaTuboConduitJson(Fuente);

        // EMT: el primer bloque, cuyo título cae en el encabezado de la tabla.
        var emt = tabla.Tamanos(TipoTuboConduit.Emt);
        Assert.Equal(16, emt[0].DesignacionMetrica);
        Assert.Equal("½", emt[0].TamanoComercial);
        Assert.Equal(15.8m, emt[0].DiametroInteriorMm);
        Assert.Equal(78m, emt[0].AreaDisponible(40m));
        Assert.Equal(104m, emt[0].AreaDisponible(53m));
        Assert.Equal("16 (½)", emt[0].Rotulo);

        // PVC cédula 40, 27 (1).
        var pvc40 = tabla.Tamanos(TipoTuboConduit.PvcCedula40).Single(t => t.DesignacionMetrica == 27);
        Assert.Equal("1", pvc40.TamanoComercial);

        // «––»: el ENT no tiene 63 (2½) y el RMC no tiene 12 (⅜).
        Assert.DoesNotContain(tabla.Tamanos(TipoTuboConduit.Ent), t => t.DesignacionMetrica == 63);
        Assert.DoesNotContain(tabla.Tamanos(TipoTuboConduit.Rmc), t => t.DesignacionMetrica == 12);

        // La cédula 80 que se ofrece es la primera: 53 (2) con 48.60 mm, menos que la cédula 40.
        var ced80 = tabla.Tamanos(TipoTuboConduit.PvcCedula80).Single(t => t.DesignacionMetrica == 53);
        Assert.Equal(48.60m, ced80.DiametroInteriorMm);
        Assert.True(ced80.DiametroInteriorMm < tabla.Tamanos(TipoTuboConduit.PvcCedula40).Single(t => t.DesignacionMetrica == 53).DiametroInteriorMm);

        // Los once bloques que se ofrecen existen.
        foreach (var tipo in Enum.GetValues<TipoTuboConduit>())
            Assert.NotEmpty(tabla.Tamanos(tipo));
    }

    [Fact]
    public void Tabla5_ConductoresAisladosPorDesignacion()
    {
        var tabla = new TablaDimensionesConductorJson(Fuente);

        Assert.Equal((2.819m, 6.258m), tabla.Aislado("14", "THHN"));
        Assert.Equal((3.302m, 8.581m), tabla.Aislado("12", "THWN-2"));
        // El bloque THHN trae la columna de mm² corrida en el DOF; por AWG sale el área correcta.
        Assert.Equal(23.61m, tabla.Aislado("8", "THHN")!.Value.AreaMm2);
        Assert.Equal(32.71m, tabla.Aislado("6", "THHN")!.Value.AreaMm2);
        Assert.Equal(8.968m, tabla.Aislado("14", "XHHW-2")!.Value.AreaMm2);
        Assert.NotNull(tabla.Aislado("12", "THW"));
        Assert.NotNull(tabla.Aislado("350", "THHN"));

        // ERRATA del DOF: TW 10 AWG publica 55.68 mm² con 4.470 mm de diámetro (π·d²/4 = 15.69).
        Assert.Equal((4.470m, 15.68m), tabla.Aislado("10", "TW"));
        Assert.Equal(15.68m, tabla.Aislado("10", "THW")!.Value.AreaMm2);
        Assert.Same(ErratasDeLaNorma.AreaTw10Awg, tabla.ErrataAplicada("10", "THHW"));
        Assert.Null(tabla.ErrataAplicada("12", "TW"));

        // Los LS no están: la Nota 5 pide las dimensiones reales.
        Assert.Null(tabla.Aislado("12", "THW-LS"));
        Assert.Null(tabla.Aislado("12", "THHW-LS"));

        // Desnudo, Tabla 8 (trenzado).
        Assert.NotNull(tabla.Desnudo("10"));
    }

    [Fact]
    public void Tabla310_15_b_3_c_SumadorEnAzotea()
    {
        var tabla = new TablaTemperaturaAzoteaJson(Fuente);
        Assert.Equal(33m, tabla.Sumador(0m));
        Assert.Equal(33m, tabla.Sumador(13m));
        Assert.Equal(22m, tabla.Sumador(14m));
        Assert.Equal(17m, tabla.Sumador(300m));
        Assert.Equal(14m, tabla.Sumador(900m));
        Assert.Throws<InvalidOperationException>(() => tabla.Sumador(901m));
    }
}
