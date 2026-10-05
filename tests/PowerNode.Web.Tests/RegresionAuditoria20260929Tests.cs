using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>«Lo que ya cumple»</b>, de la auditoría NOM del 2026-09-29: los casos que la auditoría recalculó a
/// mano y que la aplicación ya daba bien. Quedan como regresión con los números de la auditoría, no con los
/// de la implementación. Cada prueba lleva el número de su renglón. Condiciones base: 3F-4H 220/127 V,
/// cobre THHN, terminales de 60 °C hasta 100 A, lugar seco, 30 °C, centro de carga NEMA, 20 m, EMT
/// (Tabla 9, conduit de acero).
///
/// <para>
/// Los renglones 15, 18, 21 y 26 no dan todas sus entradas (qué circuitos van en el tubo, qué carga lleva
/// el alimentador); los cubren las pruebas de sus hallazgos: I-39 y los de canalizaciones (15), R-02 y
/// R-01 (21, 26). El 17 (THHN en mojado) está en <see cref="Auditoria20260929Tests"/>, P1-2.
/// </para>
/// </summary>
public class RegresionAuditoria20260929Tests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static readonly MotorNom Motor = new(Json);

    private static CuadroDeCarga Nuevo(int fases = 3, int hilos = 4, decimal tension = 220m)
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = 12;
        cuadro.Datos.Fases = fases;
        cuadro.Datos.Hilos = hilos;
        cuadro.Datos.TensionFaseFaseV = tension;
        cuadro.Recalcular();
        return cuadro;
    }

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int numero) =>
        cuadro.Circuitos.Single(c => c.Espacio == numero);

    /// <summary>Un circuito con una línea de carga en el desplegable, como se captura en la pantalla.</summary>
    private static CircuitoDelCuadro Circuito(
        CuadroDeCarga cuadro, int espacio, CategoriaDeCarga tipo, SubtipoDeCarga subtipo, int cantidad, decimal cargaUnitaria,
        bool continua = false, int polos = 1, UnidadConsumo unidad = UnidadConsumo.VoltAmperes, decimal? fp = null, decimal longitudM = 20m)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = tipo;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        var a = c.AgregarCarga();
        a.Subtipo = subtipo;
        a.Cantidad = cantidad;
        a.Unidad = unidad;
        a.CargaUnitaria = cargaUnitaria;
        a.Continua = continua;
        if (fp is { } f)
            a.FactorPotencia = f;
        c.LongitudM = longitudM;
        cuadro.Recalcular();
        return c;
    }

    private static CircuitoDelCuadro ConMotor(CuadroDeCarga cuadro, int espacio, decimal hp, int polos)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = hp;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        cuadro.Recalcular();
        return c;
    }

    private static string Calibre(CircuitoDelCuadro c) => c.Resultado!.CalibreFase.Designacion;

    // ---- Derivados de alumbrado y contactos --------------------------------------------------------

    [Fact]
    public void R01_AlumbradoContinuo()
    {
        // 10 × 100 VA, 1 polo a 127 V, 20 m: 7.87 A, 125 % = 9.84 A → 15 A, 14 AWG, e = 2.30 %.
        var c = Circuito(Nuevo(), 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 10, 100m, continua: true);

        var r = c.Resultado!;
        Assert.Equal(7.87m, Math.Round(r.CorrienteDisenoA, 2));
        Assert.Equal(15m, r.ProteccionA);
        Assert.Equal("14", r.CalibreFase.Designacion);
        Assert.Equal("14", r.CalibreNeutro.Designacion);
        Assert.Equal("14", r.CalibreTierra.Designacion);
        Assert.Equal(2.30m, Math.Round(r.CaidaTensionPct, 2));
    }

    [Fact]
    public void R02_ContactosSubenA12PorCaida()
    {
        // 8 contactos de 180 VA, 1 polo, 20 m: 11.34 A, 15 A; 14 AWG daría 3.31 % → 12 AWG, 2.16 %.
        var c = Circuito(Nuevo(), 1, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 8, 180m);

        var r = c.Resultado!;
        Assert.Equal(11.34m, Math.Round(r.CorrienteDisenoA, 2));
        Assert.Equal(15m, r.ProteccionA);
        Assert.Equal("12", r.CalibreFase.Designacion);
        Assert.Contains(r.Citas, x => x.Referencia == "Tabla 9" && x.Descripcion.StartsWith("Caída de tensión con 14 excedía"));
        Assert.Equal(2.16m, Math.Round(r.CaidaTensionPct, 2));
    }

    [Fact]
    public void R03_ContactosLejanosConTierraProporcional()
    {
        // Lo mismo a 60 m: 8 AWG, e = 2.57 %, y la tierra crece con la fase — 250-122(b): 8 AWG.
        var c = Circuito(Nuevo(), 1, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 8, 180m, longitudM: 60m);

        var r = c.Resultado!;
        Assert.Equal("8", r.CalibreFase.Designacion);
        Assert.Equal(2.57m, Math.Round(r.CaidaTensionPct, 2));
        Assert.Equal("8", r.CalibreTierra.Designacion);
        Assert.Contains(r.Citas, x => x.Referencia.StartsWith("250-122(b)"));
    }

    [Fact]
    public void R04_ElMinimoPorContactoEs180VA()
    {
        // 5 contactos de 100 VA: cada uno cuenta 180 VA — 220-14(i). 900 VA.
        var c = Circuito(Nuevo(), 1, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 5, 100m);

        Assert.Equal(900m, c.CargaInstaladaVA);
    }

    // ---- Motores (Art. 430) ------------------------------------------------------------------------

    [Fact]
    public void R05_Motor1HpMonofasicoA127V()
    {
        // Tabla 430-248: 14 A. 125 % = 17.5 A → 12 AWG; 250 % = 35 A → 35 A. La tierra de 35 A (10 AWG)
        // no pasa del calibre de fase: 12 AWG — 250-122(a).
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 1m, 1);
        // El máximo de la tabla, como lo calculó la auditoría; desde M-20, 1 HP nace en automático.
        c.CriterioProteccion = CriterioDeProteccion.Maximo430_52;
        cuadro.Recalcular();

        var r = c.Resultado!;
        Assert.Equal(14m, c.FlcA);
        Assert.Equal("12", r.CalibreFase.Designacion);
        Assert.Equal(35m, r.ProteccionA);
        Assert.Equal("12", r.CalibreTierra.Designacion);
    }

    [Fact]
    public void R06_Motor10HpTrifasicoA220V()
    {
        // Tabla 430-250, columna de 230 V: 28 A. 125 % = 35 A → 8 AWG; 250 % = 70 A; tierra 8 AWG; e = 1.06 %.
        var c = ConMotor(Nuevo(), 1, 10m, 3);

        var r = c.Resultado!;
        Assert.Equal(28m, c.FlcA);
        Assert.Equal("8", r.CalibreFase.Designacion);
        Assert.Equal(70m, r.ProteccionA);
        Assert.Equal("8", r.CalibreTierra.Designacion);
        Assert.Equal(1.06m, Math.Round(r.CaidaTensionPct, 2));
    }

    [Fact]
    public void R07_Motor10HpA208V()
    {
        // Columna de 208 V: 30.8 A; 250 % = 77 A → 80 A — 430-52(c)(1) Excepción 1.
        var c = ConMotor(Nuevo(tension: 208m), 1, 10m, 3);

        Assert.Equal(30.8m, c.FlcA);
        Assert.Equal(80m, c.Resultado!.ProteccionA);
    }

    [Fact]
    public void R08_Motor10HpA480V()
    {
        // Columna de 460 V: 14 A; 250 % = 35 A.
        var c = ConMotor(Nuevo(tension: 480m), 1, 10m, 3);

        Assert.Equal(14m, c.FlcA);
        Assert.Equal(35m, c.Resultado!.ProteccionA);
    }

    [Fact]
    public void R09_MotorMonofasicoA277VDaUnErrorClaro()
    {
        // 480Y/277 V, motor de 1 polo: la Tabla 430-248 no trae 277 V.
        var c = ConMotor(Nuevo(tension: 480m), 1, 1m, 1);

        Assert.Null(c.Resultado);
        Assert.NotNull(c.Error);
        Assert.Contains("277", c.Error);
        Assert.Contains("430-248", c.Error);
    }

    [Fact]
    public void R10_MotocompresorPorCorrienteNominal()
    {
        // 16 A nominales, 2 polos a 220 V: conductor al 125 % = 20 A → 12 AWG — 440-32. Protección: no más
        // de 175 % = 28 A → 25 A, el mayor estándar que no lo excede — 440-22(a).
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        c.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
        c.CorrientePlacaA = 16m;
        Assert.Null(cuadro.CambiarPolos(c, 2));
        cuadro.ConSuTension(c);
        cuadro.Recalcular();

        var r = c.Resultado!;
        Assert.Equal("12", r.CalibreFase.Designacion);
        Assert.Equal(25m, r.ProteccionA);
    }

    [Fact]
    public void R11_GrupoDeMotores()
    {
        // 5 HP (15.2 A) + 3 HP (9.6 A), 3 polos: conductor 125 % × 15.2 + 9.6 = 28.6 A → 10 AWG — 430-24;
        // protección: 250 % × 15.2 + 9.6 = 47.6 A → 45 A, el mayor que no lo excede — 430-53(c)(4).
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        foreach (var hp in new[] { 5m, 3m })
        {
            var m = c.AgregarCarga();
            m.Subtipo = SubtipoDeCarga.MotorUsoGeneral;
            m.Hp = hp;
        }
        cuadro.Recalcular();

        var r = c.Resultado!;
        Assert.Equal(28.6m, r.Detalle!.CapacidadMinimaA);
        Assert.Equal("10", r.CalibreFase.Designacion);
        Assert.Equal(45m, r.ProteccionA);
    }

    [Fact]
    public void R12_MotorConVariador()
    {
        // Entrada de 30 A, protección máxima del fabricante 50 A: conductor al 125 % = 37.5 A → 8 AWG —
        // 430-122(a); protección 50 A — 110-3(b); tierra de 50 A: 10 AWG.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.CapturaMotor = CapturaDeMotor.Variador;
        c.CorrienteEntradaVariadorA = 30m;
        c.ProteccionMaximaVariadorA = 50m;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        cuadro.ConSuTension(c);
        cuadro.Recalcular();

        var r = c.Resultado!;
        Assert.Equal("8", r.CalibreFase.Designacion);
        Assert.Equal(50m, r.ProteccionA);
        Assert.Equal("10", r.CalibreTierra.Designacion);
    }

    // ---- Aparatos ----------------------------------------------------------------------------------

    [Fact]
    public void R13_CalentadorDeAguaContinuoPor422_13()
    {
        // 4500 W a 220 V, 2 polos: continuo por 422-13; 30 A y 10 AWG.
        var c = Circuito(Nuevo(), 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.CalentadorDeAgua, 1, 4500m, polos: 2, unidad: UnidadConsumo.Watts);

        var r = c.Resultado!;
        Assert.True(c.ContinuaVA > 0m);
        Assert.Equal(0m, c.NoContinuaVA);
        Assert.Equal(30m, r.ProteccionA);
        Assert.Equal("10", r.CalibreFase.Designacion);
    }

    [Fact]
    public void R14_AlumbradoContinuoDe2400VAConAvisoDe210_23()
    {
        // 24 salidas de 100 VA continuas: 18.9 A; 125 % = 23.6 A → 25 A, 10 AWG. Un circuito de 25 A con
        // varias salidas de alumbrado: 210-23(b) — se revisa en la memoria.
        var cuadro = Nuevo();
        var c = Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 24, 100m, continua: true);

        var r = c.Resultado!;
        Assert.Equal(23.6m, Math.Round(r.Detalle!.CapacidadMinimaA, 1));
        Assert.Equal(25m, r.ProteccionA);
        Assert.Equal("10", r.CalibreFase.Designacion);
        Assert.Contains(c.ReglasDeClase, x => x.Referencia.StartsWith("210-23") && x.Aviso);
    }

    [Fact]
    public void R16_CargaNoLinealHaceContarElNeutroDelAlimentador()
    {
        // 3F-4H con la mayor parte de la carga no lineal: el neutro del alimentador es portador —
        // 310-15(b)(5)(c): 4 portadores, F.A. 0.80.
        var cuadro = Nuevo();
        cuadro.Datos.CargaNoLineal = true;
        foreach (var n in new[] { 1, 3, 5 })
            Circuito(cuadro, n, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 10, 100m, continua: true);
        cuadro.Recalcular();

        var canal = cuadro.Datos.CanalizacionAlimentador;
        Assert.Equal(4, canal.Conteo!.Portadores);
        Assert.Equal(4, canal.Ajuste!.ConductoresParaElMotor);
        Assert.Equal(0.80m, cuadro.Alimentador.Resultado!.Detalle!.FactorAgrupamiento);
    }

    [Fact]
    public void R19_TwA60GradosDaUnErrorClaro()
    {
        // TW es de 60 °C: la Tabla 310-15(b)(2)(a) no da factor para 60 °C de ambiente en esa columna.
        var cuadro = Nuevo();
        cuadro.Datos.TipoAislamiento = "TW";
        cuadro.Datos.TemperaturaAmbienteC = 60m;
        var c = Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 10, 100m);

        Assert.Null(c.Resultado);
        Assert.Contains("310-15(b)(2)(a)", c.Error);
        Assert.Contains("60", c.Error);
    }

    [Fact]
    public void R20_FactorDePotencia0_5()
    {
        // 2000 W con F.P. 0.5, 1 polo: 4000 VA, 31.49 A → 35 A, 8 AWG, e = 1.45 %.
        var c = Circuito(Nuevo(), 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 2000m,
            unidad: UnidadConsumo.Watts, fp: 0.5m);

        var r = c.Resultado!;
        Assert.Equal(4000m, c.CargaInstaladaVA);
        Assert.Equal(31.49m, Math.Round(r.CorrienteDisenoA, 2));
        Assert.Equal(35m, r.ProteccionA);
        Assert.Equal("8", r.CalibreFase.Designacion);
        Assert.Equal(1.45m, Math.Round(r.CaidaTensionPct, 2));
    }

    // ---- Alimentador y vivienda --------------------------------------------------------------------

    [Fact]
    public void R22_BarraMenorQueElPrincipal()
    {
        // Barra de 100 A con un principal de 175 A: aviso de 408-36.
        var cuadro = Nuevo();
        cuadro.Datos.CapacidadBarraA = 100m;
        Circuito(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 60000m, polos: 3);

        Assert.Equal(175m, cuadro.Alimentador.Resultado!.ProteccionA);
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.Contains("408-36"));
    }

    [Fact]
    public void R23_ViviendaContactoDeBanoEn20A()
    {
        // En vivienda, el circuito del baño es de 20 A — 210-11(c)(3); 12 AWG.
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        var c = Circuito(cuadro, 1, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoBano, 2, 180m);

        Assert.Equal(20m, c.Resultado!.ProteccionA);
        Assert.Equal("12", Calibre(c));
    }

    [Fact]
    public void R24_ViviendaAparatosPequenosYLavadoraCon1500VA()
    {
        // 300 VA de aparatos pequeños y 500 VA de lavadora: al alimentador, 1500 VA por circuito — 220-52(a)(b).
        // Faltan 1200 + 1000 = 2200 VA.
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        Circuito(cuadro, 1, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoAparatosPequenos, 1, 300m);
        Circuito(cuadro, 3, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoLavadora, 1, 500m);

        Assert.Equal(2200m, cuadro.Resumen.Minimo220_52VA);
    }

    [Fact]
    public void R25_MinimoDe220_12EnOficina()
    {
        // 120 m² de oficinas con 3400 VA de alumbrado: 39 VA/m² = 4680 VA; faltan 1280 VA — Tabla 220-12.
        var cuadro = Nuevo();
        cuadro.Datos.AreaServidaM2 = 120m;
        Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 34, 100m, continua: true);

        var sp = cuadro.Resumen.Superficie!;
        Assert.Equal(39m, sp.VaPorM2);
        Assert.Equal(4680m, sp.MinimoAlumbradoVA);
        Assert.Equal(1280m, sp.AjusteAlumbradoVA);
    }
}
