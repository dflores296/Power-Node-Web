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
        Assert.Equal("16 mm / ½ in", emt[0].Rotulo);

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

        // M-11: donde el DOF pasa de página, la celda de tipo viene en blanco y continúa el grupo de
        // arriba. Se leía como «sin tipo» y se perdían THHN 4/0 a 300 y XHHW de 250 en adelante.
        Assert.Equal((16.31m, 208.8m), tabla.Aislado("4/0", "THHN"));
        Assert.Equal((18.06m, 256.1m), tabla.Aislado("250", "THWN-2"));
        Assert.Equal((19.46m, 297.3m), tabla.Aislado("300", "THHN"));
        Assert.Equal((17.91m, 251.9m), tabla.Aislado("250", "XHHW"));
        Assert.Equal((37.57m, 1108m), tabla.Aislado("1250", "XHHW-2"));
        Assert.Null(tabla.Aislado("1250", "THHN")); // este sí no lo trae la norma

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

    [Fact]
    public void Tabla310_104a_ThwEnLugarSeco_Por310_10a()
    {
        // El DOF publica THW solo como «75 °C · Lugares mojados»; 310-10(a) permite en lugar seco
        // cualquier tipo de la NOM, y 310-10(b) lo nombra. En seco: los 75 °C que da la tabla.
        var tabla = new TablaAislamientoJson(Fuente);
        Assert.Equal(TemperaturaAislamiento.T75, tabla.TemperaturaMaxima("THW", LugarDeInstalacion.Seco));
        Assert.Equal(TemperaturaAislamiento.T75, tabla.TemperaturaMaxima("THW", LugarDeInstalacion.Mojado));
        // Con renglón propio para seco no cambia nada: THHW, 90 °C seco y 75 °C mojado.
        Assert.Equal(TemperaturaAislamiento.T90, tabla.TemperaturaMaxima("THHW", LugarDeInstalacion.Seco));
        Assert.Equal(TemperaturaAislamiento.T75, tabla.TemperaturaMaxima("THHW", LugarDeInstalacion.Mojado));
        // THHN sigue sin valer en mojado.
        Assert.Null(tabla.TemperaturaMaxima("THHN", LugarDeInstalacion.Mojado));
    }

    /// <summary>
    /// <b>Seco, húmedo y mojado</b> — auditoría del 2026-09-29, P1-2. La temperatura, de la fila de la
    /// Tabla 310-104(a) de ese lugar; el permiso, de 310-10(b) y 310-10(c)(2). Antes «húmedo o
    /// mojado» era una sola opción y tomaba la fila más caliente: XHHW mojado a 90 °C.
    /// </summary>
    [Theory]
    // XHHW: 90 °C secos y húmedos, 75 °C mojados.
    [InlineData("XHHW", LugarDeInstalacion.Seco, 90)]
    [InlineData("XHHW", LugarDeInstalacion.Humedo, 90)]
    [InlineData("XHHW", LugarDeInstalacion.Mojado, 75)]
    // THHW y THHW-LS: 90 °C secos, 75 °C mojados; en húmedo, la de mojado.
    [InlineData("THHW", LugarDeInstalacion.Humedo, 75)]
    [InlineData("THHW-LS", LugarDeInstalacion.Seco, 90)]
    [InlineData("THHW-LS", LugarDeInstalacion.Mojado, 75)]
    // Sin permiso en mojado: 310-10(c)(2) no los nombra.
    [InlineData("THHN", LugarDeInstalacion.Mojado, null)]
    [InlineData("RHH", LugarDeInstalacion.Mojado, null)]
    [InlineData("XHH", LugarDeInstalacion.Mojado, null)]
    // En húmedo sí: 310-10(b) nombra a THHN aunque la tabla lo publique solo para secos.
    [InlineData("THHN", LugarDeInstalacion.Humedo, 90)]
    [InlineData("RHH", LugarDeInstalacion.Humedo, 90)]
    // La tabla los da para «secos y húmedos», pero 310-10(c)(2) los nombra para mojados.
    [InlineData("THWN", LugarDeInstalacion.Mojado, 75)]
    [InlineData("THW-2", LugarDeInstalacion.Mojado, 90)]
    [InlineData("THWN-2", LugarDeInstalacion.Mojado, 90)]
    // Con su fila de mojados.
    [InlineData("TW", LugarDeInstalacion.Mojado, 60)]
    [InlineData("RHW", LugarDeInstalacion.Mojado, 75)]
    [InlineData("RHW-2", LugarDeInstalacion.Mojado, 90)]
    [InlineData("THW-LS", LugarDeInstalacion.Mojado, 75)]
    [InlineData("XHHW-2", LugarDeInstalacion.Mojado, 90)]
    [InlineData("USE-2", LugarDeInstalacion.Mojado, 90)]
    public void Tabla310_104a_SecoHumedoYMojado(string tipo, LugarDeInstalacion lugar, int? esperada)
    {
        var tabla = new TablaAislamientoJson(Fuente);
        Assert.Equal(esperada, (int?)tabla.TemperaturaMaxima(tipo, lugar));
    }

    /// <summary>
    /// <b>Tabla 250-66</b> — auditoría del 2026-09-29, P3-3. Los intervalos de la acometida vienen en texto
    /// y redondeados («33.6 o menor» para 2 AWG, que son 33.62 mm²; «Más de 85.0 a 177» hasta 350 kcmil,
    /// 177.3 mm²): el calibre del borde cae en su renglón.
    /// </summary>
    [Theory]
    [InlineData("2", 1, MaterialConductor.Cobre, "8")]
    [InlineData("1/0", 1, MaterialConductor.Cobre, "6")]
    [InlineData("3/0", 1, MaterialConductor.Cobre, "4")]
    [InlineData("350", 1, MaterialConductor.Cobre, "2")]
    [InlineData("600", 1, MaterialConductor.Cobre, "1/0")]
    [InlineData("1000", 1, MaterialConductor.Cobre, "2/0")]
    [InlineData("600", 3, MaterialConductor.Cobre, "3/0")]      // 3 × 304 = 912 mm²: más de 557.38
    [InlineData("500", 1, MaterialConductor.Aluminio, "1/0")]   // aluminio, más de 250 a 500 kcmil
    public void Tabla250_66_ElConductorDelElectrodo(string acometida, int enParalelo, MaterialConductor material, string esperado)
    {
        var calibres = Calibres();
        var tabla = new TablaElectrodoTierraJson(Fuente, calibres);
        var fase = calibres.BuscarPorDesignacion(acometida)!;

        Assert.Equal(esperado, tabla.CalibreMinimo(fase.AreaMm2 * enParalelo, material, material).Designacion);
    }

    /// <summary>
    /// Tabla 220-12 — M-14. Nace en Power Node Web: no hay versión de escritorio que comparar; los valores
    /// son los del PDF del DOF (verificada 2026-08-12 en el repo de la norma). La llamada de nota («39 (b)»)
    /// no se lee como parte del número, y el renglón de título del bloque de áreas comunes se salta.
    /// </summary>
    [Fact]
    public void Tabla220_12_CargaUnitariaPorInmueble()
    {
        var tabla = new TablaCargaUnitariaJson(Fuente);
        decimal Va(string inmueble) => tabla.Filas.First(f => f.Inmueble.StartsWith(inmueble, StringComparison.Ordinal)).VaPorM2;

        Assert.Equal(39m, Va("Edificios de oficinas"));
        Assert.Equal(39m, Va("Bancos"));
        Assert.Equal(33m, Va("Unidades de vivienda"));
        Assert.Equal(22m, Va("Hospitales"));
        Assert.Equal(6m, Va("Estacionamientos comerciales"));
        Assert.Equal(3m, Va("Bodegas"));
        Assert.DoesNotContain(tabla.Filas, f => f.Inmueble.StartsWith("En cualquiera", StringComparison.Ordinal));
        Assert.Equal(21, tabla.Filas.Count); // 18 inmuebles y 3 áreas comunes
    }
}
