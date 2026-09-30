using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>El tipo es de la carga; la clase es del circuito</b> — I-123, I-125, decisión
/// <c>cargas-y-clases-de-circuito.md</c>. Los números salen de la norma: 180 VA por contacto y 90 por
/// contacto de uno múltiple (220-14(i)), 600 VA por portalámparas pesado (220-14(e)), 1200 VA por
/// circuito de anuncios (220-14(f)), 5000 VA por secadora en vivienda (220-54); el F.D. por el tipo de
/// cada carga (220 Parte C); otro tablero sin F.D. (220-40) y citado con 215-2(a)(1) y 215-3.
/// </summary>
public class CargasYClasesTests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static readonly MotorNom Motor = new(Json);

    private static CuadroDeCarga Nuevo()
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = 12;
        cuadro.Datos.Fases = 3;
        cuadro.Datos.Hilos = 4;
        cuadro.Datos.TensionFaseFaseV = 220m;
        return EnPvc.Todo(cuadro);
    }

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int numero) =>
        cuadro.Circuitos.Single(c => c.Espacio == numero);

    private static CargaDelCircuito Linea(CircuitoDelCuadro c, SubtipoDeCarga subtipo, int cantidad, decimal cargaVA, bool continua = false)
    {
        var a = c.AgregarCarga();
        a.Subtipo = subtipo;
        a.Cantidad = cantidad;
        a.Unidad = UnidadConsumo.VoltAmperes;
        a.CargaUnitaria = cargaVA;
        a.Continua = continua;
        return a;
    }

    private static decimal Demandada(CuadroDeCarga cuadro, CategoriaDeCarga tipo) =>
        cuadro.Resumen.PorCategoria!.Single(f => f.Categoria == tipo).DemandadaVA;

    private static decimal Instalada(CuadroDeCarga cuadro, CategoriaDeCarga tipo) =>
        cuadro.Resumen.PorCategoria!.Single(f => f.Categoria == tipo).InstaladaVA;

    // ---- El F.D. por el tipo de cada carga ---------------------------------------------------------

    [Fact]
    public void I123_UnCircuitoCombinadoLlevaElFactorDeCadaCarga()
    {
        // 10 luminarias de 100 VA y 5 contactos: 1000 VA de alumbrado y 900 VA de contactos. F.D. 0.5 solo
        // en alumbrado → 500 + 900 = 1400 VA de demanda. Antes todo iba con el F.D. del tipo del circuito.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        Linea(c, SubtipoDeCarga.Luminarias, 10, 100m, continua: true);
        Linea(c, SubtipoDeCarga.ContactoUsoGeneral, 5, 0m);
        cuadro.Datos.FactorDemandaAlumbrado = 0.5m;
        cuadro.Recalcular();

        Assert.True(c.TieneCargasCombinadas);
        Assert.Equal(1000m, Instalada(cuadro, CategoriaDeCarga.Alumbrado));
        Assert.Equal(900m, Instalada(cuadro, CategoriaDeCarga.Contactos));
        Assert.Equal(500m, Demandada(cuadro, CategoriaDeCarga.Alumbrado));
        Assert.Equal(900m, Demandada(cuadro, CategoriaDeCarga.Contactos));
        Assert.Equal(1400m, cuadro.Resumen.DemandadaVA);
        // El derivado va con la carga plena (220-42 no aplica al circuito) y como Contactos: sin 240-4(b).
        Assert.Equal(1000m, c.ContinuaVA);
        Assert.Equal(900m, c.NoContinuaVA);
        Assert.Equal(ClaseDeCircuito.UsoGeneral, c.ClaseDelCircuito);
        Assert.Equal(15, c.Salidas);
    }

    [Fact]
    public void I123_UnaCargaSinSubtipoTomaElTipoDelCircuito()
    {
        // Lo de hasta el formato 4: sin subtipo, la carga es del tipo del circuito y todo sigue igual.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var a = c.AgregarCarga();
        a.CargaUnitaria = 800m;
        cuadro.Recalcular();

        Assert.Equal(CategoriaDeCarga.Equipo, c.TipoDe(a));
        Assert.False(c.TieneCargasCombinadas);
        Assert.Equal(800m, Instalada(cuadro, CategoriaDeCarga.Equipo));
    }

    [Fact]
    public void I123_UnMotorEntreLasCargasHaceElGrupoYVaConElFactorDeMotores()
    {
        // Alumbrado con un motor de uso general: 430-53 por lo que lleva, sin la unidad «Varios». El motor,
        // con el F.D. de motores; el alumbrado, con el suyo.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        Linea(c, SubtipoDeCarga.Luminarias, 4, 100m);
        var motor = c.AgregarCarga();
        motor.Subtipo = SubtipoDeCarga.MotorUsoGeneral;
        motor.Hp = 0.5m;
        cuadro.Recalcular();

        Assert.True(c.EsGrupo);
        Assert.Equal(ClaseDeAparato.Motor, motor.Clase);
        Assert.Equal(400m, Instalada(cuadro, CategoriaDeCarga.Alumbrado));
        Assert.Equal(c.MotorVA, Instalada(cuadro, CategoriaDeCarga.Motor));
        Assert.True(c.MotorVA > 0m);
        Assert.NotNull(c.Resultado?.Grupo);
    }

    // ---- Los mínimos de cada subtipo — 220-14 ------------------------------------------------------

    [Theory]
    [InlineData(SubtipoDeCarga.ContactoUsoGeneral, 3, 100, 540, "220-14(i)")]   // 3 × 180
    [InlineData(SubtipoDeCarga.ContactoMultiple, 6, 50, 540, "220-14(i)")]      // 6 × 90
    [InlineData(SubtipoDeCarga.PortalamparasPesado, 2, 300, 1200, "220-14(e)")] // 2 × 600
    [InlineData(SubtipoDeCarga.EnsambleDeSalidas, 4, 0, 720, "220-14(h)")]      // 4 tramos × 180
    [InlineData(SubtipoDeCarga.Anuncios, 1, 500, 1200, "220-14(f)")]            // 1200 por circuito
    [InlineData(SubtipoDeCarga.ContactoUsoGeneral, 2, 250, 500, null)]          // ya pasa: se queda
    public void I123_CadaSubtipoLlevaSuMinimo(SubtipoDeCarga subtipo, int cantidad, int vaUnitario, int esperadoVA, string? referencia)
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = subtipo.Tipo();
        var a = Linea(c, subtipo, cantidad, vaUnitario);
        cuadro.Recalcular();

        Assert.Equal(esperadoVA, a.TotalVA);
        Assert.Equal(referencia, a.ReferenciaMinimo);
    }

    [Fact]
    public void I123_LaSecadoraLlevaCincoMilSoloEnVivienda()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var a = Linea(c, SubtipoDeCarga.Secadora, 1, 4000m);
        cuadro.Recalcular();
        Assert.Equal(4000m, a.TotalVA);

        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        cuadro.Recalcular();
        Assert.Equal(5000m, a.TotalVA);
        Assert.Equal("220-54", a.ReferenciaMinimo);
    }

    [Fact]
    public void I123_ElCalentadorDeAguaEsContinuo()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var a = Linea(c, SubtipoDeCarga.CalentadorDeAgua, 1, 1500m);
        cuadro.Recalcular();

        Assert.True(a.Continua);
        Assert.Equal("422-13", a.ReferenciaContinua);
        Assert.Equal(1500m, c.ContinuaVA);
        Assert.Equal(ClaseDeCircuito.Individual, c.ClaseDelCircuito);
    }

    // ---- Otro tablero — I-125 ----------------------------------------------------------------------

    [Fact]
    public void I125_OtroTableroEsUnAlimentadorSinFactorDeDemanda()
    {
        // 12 000 VA continuos y 18 500 no continuos del otro tablero, a 3 polos y 220 V: la capacidad es
        // 1.25 × 12 000 + 18 500 = 33 500 VA → 87.92 A — 215-2(a)(1), 215-3. El F.D. de alumbrado (0.5) no
        // lo toca: entra con sus 30 500 VA — 220-40.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Tablero;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        c.Continua = 12000m;
        c.NoContinua = 18500m;
        c.LongitudM = 10m;
        cuadro.Datos.FactorDemandaAlumbrado = 0.5m;
        cuadro.Datos.CambiarFactorDeDemanda(CategoriaDeCarga.Tablero, 0.5m);
        cuadro.Recalcular();

        Assert.Equal(1m, cuadro.Datos.FactorDeDemanda(CategoriaDeCarga.Tablero));
        Assert.Equal(30500m, Demandada(cuadro, CategoriaDeCarga.Tablero));
        Assert.Equal(ClaseDeCircuito.Alimentador, c.ClaseDelCircuito);
        var r = c.Resultado!;
        Assert.Equal(90m, r.ProteccionA);
        Assert.Contains(r.Citas, x => x.Referencia == "215-2(a)(1)");
        Assert.Contains(r.Citas, x => x.Referencia == "215-3");
        Assert.DoesNotContain(r.Citas, x => x.Referencia == "210-19(a)(1)");
        Assert.Empty(CategoriaDeCarga.Tablero.JustificacionesPosibles(cuadro.Datos.Inmueble));
    }

    [Fact]
    public void I125_OtroTableroNoSeDesglosa()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Tablero;
        c.NoContinua = 1000m;
        c.Cargas.Add(new CargaDelCircuito { CargaUnitaria = 5000m });
        cuadro.Recalcular();

        Assert.False(c.TieneDesglose);
        Assert.Equal(1000m, c.CargaInstaladaVA);
    }

    // ---- La clase del circuito ----------------------------------------------------------------------

    [Theory]
    [InlineData(CategoriaDeCarga.Alumbrado, ClaseDeCircuito.UsoGeneral)]
    [InlineData(CategoriaDeCarga.Contactos, ClaseDeCircuito.UsoGeneral)]
    [InlineData(CategoriaDeCarga.CalefaccionFija, ClaseDeCircuito.ParaAparatos)]
    [InlineData(CategoriaDeCarga.Equipo, ClaseDeCircuito.Individual)]
    public void I123_LaClaseDelRenglon(CategoriaDeCarga tipo, ClaseDeCircuito clase)
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = tipo;
        c.NoContinua = 1000m;
        cuadro.Recalcular();

        Assert.Equal(clase, c.ClaseDelCircuito);
        Assert.Equal(tipo != CategoriaDeCarga.Equipo, c.EsCargaTotal);
    }

    [Fact]
    public void I123_DosAparatosSinAlumbradoEsParaAparatos()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        Linea(c, SubtipoDeCarga.AparatoFijo, 1, 600m);
        Linea(c, SubtipoDeCarga.AparatoFijo, 1, 400m);
        cuadro.Recalcular();

        Assert.Equal(ClaseDeCircuito.ParaAparatos, c.ClaseDelCircuito);
    }

    [Theory]
    [InlineData(UsoDeContactos.AparatosPequenos, ClaseDeCircuito.ParaAparatos)]
    [InlineData(UsoDeContactos.Lavadora, ClaseDeCircuito.ParaAparatos)]
    [InlineData(UsoDeContactos.Refrigerador, ClaseDeCircuito.Individual)]
    [InlineData(UsoDeContactos.General, ClaseDeCircuito.UsoGeneral)]
    public void I123_LosUsosDeViviendaDanSuClase(UsoDeContactos uso, ClaseDeCircuito clase)
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Contactos;
        c.Uso = uso;
        c.NoContinua = 1000m;
        cuadro.Recalcular();

        Assert.Equal(clase, c.ClaseDelCircuito);
    }

    // ---- El renglón como carga ----------------------------------------------------------------------

    [Fact]
    public void I123_AbrirElDesgloseDeUnMotorLoVuelveLaPrimeraLinea()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = 0.5m;
        c.LongitudM = 10m;
        cuadro.Recalcular();
        var antes = c.Resultado!.ProteccionA;

        c.AgregarCarga();
        cuadro.Recalcular();

        Assert.True(c.EsGrupo);
        Assert.Equal(ClaseDeAparato.Motor, c.Cargas[0].Clase);
        Assert.Equal(0.5m, c.Cargas[0].Hp);
        Assert.Equal(antes, c.Resultado!.ProteccionA); // un motor solo sigue siendo un motor — 430-52
    }

    // ---- El archivo, formato 5 -----------------------------------------------------------------------

    [Fact]
    public void I123_ElSubtipoSeGuardaYElTableroTambien()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        Linea(c, SubtipoDeCarga.Luminarias, 2, 100m);
        Linea(c, SubtipoDeCarga.ContactoMultiple, 4, 0m);
        var t = Espacio(cuadro, 2);
        t.Categoria = CategoriaDeCarga.Tablero;
        t.NoContinua = 5000m;
        cuadro.Recalcular();

        var texto = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        Assert.Contains("\"subtipo\": \"ContactoMultiple\"", texto);
        var abierto = ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!;

        Assert.Equal([SubtipoDeCarga.Luminarias, SubtipoDeCarga.ContactoMultiple], Espacio(abierto, 1).Cargas.Select(a => a.Subtipo));
        Assert.Equal(CategoriaDeCarga.Tablero, Espacio(abierto, 2).Categoria);
        Assert.Equal(cuadro.Resumen.DemandadaVA, abierto.Resumen.DemandadaVA);
    }

    [Fact]
    public void I123_UnSubtipoQueSoloVaEnElRenglonNoSeLeeEnUnaCarga()
    {
        const string texto = """
            {
              "formato": "power-node/cuadro-de-carga",
              "version": 5,
              "circuitos": [ { "espacio": 1, "categoria": "Equipo",
                "aparatos": [ { "cargaUnitaria": 500, "subtipo": "TableroAlimentado" } ] } ]
            }
            """;
        var abierto = ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!;

        Assert.Null(Espacio(abierto, 1).Cargas.Single().Subtipo);
    }

    // ---- Las reglas de la clase — I-124 ------------------------------------------------------------

    private static CircuitoDelCuadro Renglon(CuadroDeCarga cuadro, int espacio, CategoriaDeCarga tipo, decimal noContinuaVA)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = tipo;
        c.NoContinua = noContinuaVA;
        c.LongitudM = 5m;
        return c;
    }

    [Fact]
    public void I124_AlumbradoEnUnCircuitoDe25AAvisa210_23b()
    {
        // 3000 VA / 127 V = 23.6 A → 25 A: con alumbrado común no se permite — 210-23(b).
        var cuadro = Nuevo();
        var c = Renglon(cuadro, 1, CategoriaDeCarga.Alumbrado, 3000m);
        cuadro.Recalcular();

        Assert.Equal(25m, c.Resultado!.ProteccionA);
        Assert.Contains(c.ReglasDeClase, x => x.Referencia == "210-23(b)" && x.Aviso);
        Assert.Contains(cuadro.AvisosDeCircuitos, x => x.StartsWith("Circuito 1:") && x.EndsWith("210-23(b)."));
    }

    [Fact]
    public void I124_LosContactosDicenSuValorPorLaTabla210_21b3()
    {
        var cuadro = Nuevo();
        var veinte = Renglon(cuadro, 1, CategoriaDeCarga.Contactos, 2400m);   // 18.9 A → 20 A
        var treinta = Renglon(cuadro, 3, CategoriaDeCarga.Contactos, 3200m);  // 25.2 A → 30 A
        cuadro.Recalcular();

        var nota = Assert.Single(veinte.ReglasDeClase);
        Assert.Equal("Tabla 210-21(b)(3)", nota.Referencia);
        Assert.Contains("de 15 o 20 A", nota.Texto);
        Assert.False(nota.Aviso);
        Assert.Contains(treinta.ReglasDeClase, x => x.Referencia == "Tabla 210-21(b)(3)" && x.Aviso);
    }

    [Fact]
    public void I124_ElEquipoFijoConAlumbradoNoPasaDelCincuentaPorCiento()
    {
        // 600 VA de luminarias y un aparato fijo de 1500 VA: 11.81 A de equipo en un circuito de 20 A
        // (17.3 A) — más de 10 A: 210-23(a)(2).
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        c.LongitudM = 5m;
        Linea(c, SubtipoDeCarga.Luminarias, 6, 100m);
        Linea(c, SubtipoDeCarga.AparatoFijo, 1, 1500m);
        cuadro.Recalcular();

        Assert.Equal(20m, c.Resultado!.ProteccionA);
        Assert.Contains(c.ReglasDeClase, x => x.Referencia == "210-23(a)(2)" && x.Aviso);
    }

    [Fact]
    public void I124_ElRefrigeradorEnSuCircuitoIndividualDice210_21b1()
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        var c = Renglon(cuadro, 1, CategoriaDeCarga.Contactos, 700m);
        c.Uso = UsoDeContactos.Refrigerador;
        cuadro.Recalcular();

        Assert.Equal(ClaseDeCircuito.Individual, c.ClaseDelCircuito);
        Assert.Contains(c.ReglasDeClase, x => x.Referencia == "210-21(b)(1)" && !x.Aviso);
    }

    [Fact]
    public void I124_UnaEstufaDeOchoSetecientosCincuentaPideCuarentaAmperes()
    {
        // 8750 VA a 3 polos y 220 V: 22.96 A → 25 A por carga; en vivienda, 40 A — 210-19(a)(3).
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        c.LongitudM = 5m;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        Linea(c, SubtipoDeCarga.Coccion, 1, 8750m);
        cuadro.Recalcular();

        Assert.Equal(40m, c.Resultado!.ProteccionA);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "210-19(a)(3)");
    }

    [Fact]
    public void I124_UnAparatoSoloDentroDeSusVeinteAmperesNoAvisa422_11e()
    {
        var cuadro = Nuevo();
        var c = Renglon(cuadro, 1, CategoriaDeCarga.Equipo, 1200m); // 9.45 A → 15 A
        cuadro.Recalcular();

        Assert.Equal(ClaseDeCircuito.Individual, c.ClaseDelCircuito);
        Assert.DoesNotContain(c.ReglasDeClase, x => x.Aviso);
    }

    // ---- El mínimo por superficie — M-14 -----------------------------------------------------------

    [Fact]
    public void M14_UnaOficinaNoBajaDelMinimoDeLaTabla220_12()
    {
        // 200 m² de oficinas con 200 VA de LED: la Tabla 220-12 pide 39 VA/m² = 7800 VA → se agregan 7600 VA,
        // continuos. Sin contactos: 220-14(k) pide 11 VA/m² = 2200 VA.
        var cuadro = Nuevo();
        cuadro.Datos.AreaServidaM2 = 200m;
        Renglon(cuadro, 1, CategoriaDeCarga.Alumbrado, 200m);
        cuadro.Recalcular();

        var sp = cuadro.Resumen.Superficie!;
        Assert.StartsWith("Edificios de oficinas", sp.Renglon);
        Assert.Equal(39m, sp.VaPorM2);
        Assert.Equal(7800m, sp.MinimoAlumbradoVA);
        Assert.Equal(7600m, sp.AjusteAlumbradoVA);
        Assert.True(sp.AlumbradoContinuo);
        Assert.Equal(2200m, sp.AjusteContactosVA);
        Assert.Equal(10000m, cuadro.Resumen.CalculadaVA);
        Assert.Equal(10000m, cuadro.Resumen.DemandadaVA);
    }

    [Fact]
    public void M14_EnViviendaLosContactosDeUsoGeneralVanDentro()
    {
        // 100 m² de vivienda: 33 VA/m² = 3300 VA. Capturados 1000 VA de alumbrado y 1500 de contactos de uso
        // general: van dentro (220-14(j)) → faltan 800 VA; no se suman aparte.
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        cuadro.Datos.AreaServidaM2 = 100m;
        Renglon(cuadro, 1, CategoriaDeCarga.Alumbrado, 1000m);
        Renglon(cuadro, 3, CategoriaDeCarga.Contactos, 1500m);
        cuadro.Recalcular();

        var sp = cuadro.Resumen.Superficie!;
        Assert.Equal(33m, sp.VaPorM2);
        Assert.Equal(2500m, sp.AlumbradoCapturadoVA);
        Assert.Equal(800m, sp.AjusteAlumbradoVA);
        Assert.False(sp.AlumbradoContinuo);
        Assert.Null(sp.MinimoContactosVA);
        Assert.Equal(3300m, cuadro.Resumen.CalculadaVA);
    }

    [Fact]
    public void M14_ElMinimoSubeLaCorrienteDelAlimentador()
    {
        var cuadro = Nuevo();
        Renglon(cuadro, 1, CategoriaDeCarga.Alumbrado, 200m);
        cuadro.Recalcular();
        var sin = cuadro.Alimentador.Resultado!.CorrienteDisenoA;
        Assert.Null(cuadro.Resumen.Superficie);

        cuadro.Datos.AreaServidaM2 = 200m;
        cuadro.Datos.UsoTabla220_12 = "Tiendas"; // 33 VA/m², sin 220-14(k)
        cuadro.Recalcular();

        // 6400 VA más, parejos en 3 barras de 127 V: 16.8 A más por barra.
        Assert.Equal(sin + 6400m / (3m * cuadro.Datos.TensionFaseNeutroV), cuadro.Alimentador.Resultado!.CorrienteDisenoA, 2);
    }

    [Fact]
    public void M14_ElAreaSeGuardaEnElArchivo()
    {
        var cuadro = Nuevo();
        cuadro.Datos.AreaServidaM2 = 150m;
        cuadro.Datos.UsoTabla220_12 = "Escuelas";
        Renglon(cuadro, 1, CategoriaDeCarga.Alumbrado, 500m);
        cuadro.Recalcular();

        var abierto = ArchivoDelCuadro.Abrir(ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now), Motor).Cuadro!;
        Assert.Equal(150m, abierto.Datos.AreaServidaM2);
        Assert.Equal("Escuelas", abierto.Datos.UsoTabla220_12);
        Assert.Equal(cuadro.Resumen.CalculadaVA, abierto.Resumen.CalculadaVA);
    }
}
