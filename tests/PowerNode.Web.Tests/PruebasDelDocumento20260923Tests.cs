using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>El documento «Pruebas de las correcciones del 2026-09-23», automatizado.</b> Cada prueba lleva
/// el número de la prueba manual que reemplaza. Lo que queda manual (tooltips, textos cortados,
/// documento impreso) es solo lo que depende del navegador.
///
/// <para>
/// Los valores esperados NO se copiaron de la implementación: salen del cálculo a mano con
/// V F-N = 220/√3 = 127.02 V y la Tabla 9 (Cu en PVC). La única diferencia con el documento es 4.2
/// con F.P. 0.8: el documento dice 2.01 %, la cuenta da 2.00 %
/// (0.04 km × 11.81 A × (6.6×0.8 + 0.177×0.6) Ω/km = 2.544 V → 2.003 %).
/// </para>
/// </summary>
public class PruebasDelDocumento20260923Tests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    /// <summary>«Recargar la página»: tablero de 6 espacios con los valores iniciales.</summary>
    private static CuadroDeCarga Nuevo()
    {
        var cuadro = new CuadroDeCarga(new MotorNom(Json));
        cuadro.Datos.NumeroEspacios = 6;
        cuadro.Recalcular();
        return cuadro;
    }

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int numero) =>
        cuadro.Circuitos.Single(c => c.Espacio == numero);

    private static CircuitoDelCuadro Capturar(
        CuadroDeCarga cuadro, int espacio, TipoCarga tipo, UnidadConsumo unidad,
        decimal continua, decimal noContinua, decimal fp = 0.9m, decimal longitudM = 20m)
    {
        var c = Espacio(cuadro, espacio);
        c.Tipo = tipo;
        c.Unidad = unidad;
        c.Continua = continua;
        c.NoContinua = noContinua;
        c.FactorPotencia = fp;
        c.LongitudM = longitudM;
        cuadro.Recalcular();
        return c;
    }

    /// <summary>Caso base: refrigerador, microondas y air fryer, Equipo, VA, continua, 20 m.</summary>
    private static CuadroDeCarga CasoBase()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 1, TipoCarga.Equipo, UnidadConsumo.VoltAmperes, 750m, 0m);
        Capturar(cuadro, 3, TipoCarga.Equipo, UnidadConsumo.VoltAmperes, 1500m, 0m);
        Capturar(cuadro, 5, TipoCarga.Equipo, UnidadConsumo.VoltAmperes, 1550m, 0m);
        return cuadro;
    }

    // ---- 1 y 2 · Alimentador e interruptor principal ------------------------------------------------

    [Fact]
    public void P1_1_ElAlimentadorLoGobiernaLaFaseC()
    {
        var cuadro = CasoBase();
        var a = cuadro.Alimentador;

        Assert.Equal('C', a.Gobierna!.Fase);
        Assert.Equal(12.20m, a.Resultado!.CorrienteDisenoA, 2);
        Assert.Equal(20m, a.Resultado.ProteccionA);
        Assert.Equal("12", a.Resultado.CalibreFase.Designacion);
    }

    [Fact]
    public void P1_2_ConNomCompletaElPrincipalEsDe16A()
    {
        var cuadro = CasoBase();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.NomCompleta;
        cuadro.Recalcular();

        Assert.Equal(16m, cuadro.InterruptorPrincipalA);
        Assert.Equal("12", cuadro.Alimentador.Resultado!.CalibreFase.Designacion);
    }

    [Fact]
    public void P1_3_LaMemoriaDiceQueFaseGobierna()
    {
        var seccion3 = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DelAlimentador(CasoBase())!)[2];
        var gobierna = Assert.Single(seccion3.Renglones, r => r.Rotulo == "Fase que gobierna");

        Assert.StartsWith(
            "Fase C, la más cargada: 125 % × 12.20 A (continua) + 0.00 A (no continua) = 15.25 A",
            gobierna.Valor);
    }

    [Fact]
    public void P1_4_UnDosPolosLlevaSuCorrienteCompletaPorCadaLinea()
    {
        var cuadro = Nuevo();
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 1), 2));
        Capturar(cuadro, 1, TipoCarga.Alumbrado, UnidadConsumo.VoltAmperes, 0m, 2200m);

        Assert.Equal(10.00m, cuadro.Alimentador.Resultado!.CorrienteDisenoA, 2);
    }

    [Fact]
    public void P2_1_AvisaCuandoElPrincipalEsMenorQueUnDerivado()
    {
        // Contactos de cocina: 20 A por 210-11(c)(1). Desde el 2026-09-24 no hay mínimo de 20 A en
        // contactos de uso general; el principal cuenta 1500 VA por 220-52(a) → 11.81 A → 15 A.
        var cuadro = Nuevo();
        Espacio(cuadro, 3).Uso = UsoDeContactos.AparatosPequenos;
        var c = Capturar(cuadro, 3, TipoCarga.Contactos, UnidadConsumo.VoltAmperes, 0m, 500m);

        Assert.Equal(20m, c.Resultado!.ProteccionA);
        Assert.Equal(15m, cuadro.InterruptorPrincipalA);
        Assert.Contains(cuadro.Alimentador.Avisos, a =>
            a.StartsWith("El interruptor principal (15 A) es menor que el derivado más grande (20 A, circuito 3)."));
    }

    [Fact]
    public void P2_2_SinAvisoDeMenorEnElCasoBase() =>
        Assert.DoesNotContain(CasoBase().Alimentador.Avisos, a => a.Contains("menor que el derivado más grande"));

    // ---- 3 · Carga en VA, W o A ---------------------------------------------------------------------

    [Fact]
    public void P3_ConvierteVAWYAConElFPDelRenglon()
    {
        var casos = new (UnidadConsumo Unidad, decimal Continua, decimal NoContinua, decimal Fp, decimal Va, decimal In)[]
        {
            (UnidadConsumo.VoltAmperes, 720m, 0m, 0.9m, 720m, 5.67m),   // 3.1
            (UnidadConsumo.Watts, 1550m, 0m, 0.9m, 1722m, 13.56m),      // 3.2
            (UnidadConsumo.Watts, 600m, 0m, 0.8m, 750m, 5.90m),         // 3.3
            (UnidadConsumo.Amperes, 0m, 8m, 0.9m, 1016m, 8.00m),        // 3.4
        };

        foreach (var k in casos)
        {
            var c = Capturar(Nuevo(), 1, TipoCarga.Alumbrado, k.Unidad, k.Continua, k.NoContinua, k.Fp);
            Assert.Equal(k.Va, Math.Round(c.CargaInstaladaVA, 0));
            Assert.Equal(k.In, c.Resultado!.CorrienteDisenoA, 2);
        }
    }

    [Fact]
    public void P3_5_CambiarLaUnidadConservaElValorCapturado()
    {
        var cuadro = Nuevo();
        var c = Capturar(cuadro, 1, TipoCarga.Alumbrado, UnidadConsumo.Watts, 900m, 0m, 0.9m);
        c.Unidad = UnidadConsumo.VoltAmperes;
        cuadro.Recalcular();

        Assert.Equal(900m, c.Continua);
        Assert.Equal(900m, c.CargaInstaladaVA);
    }

    // ---- 4 y 5 · F.P. y kW --------------------------------------------------------------------------

    [Fact]
    public void P4_2_EnVAElFPSoloMueveLaCaida()
    {
        foreach (var (fp, caida) in new[] { (0.8m, 2.00m), (0.9m, 2.24m), (1.0m, 2.45m) })
        {
            var cuadro = CasoBase();
            var microondas = Espacio(cuadro, 3);
            microondas.FactorPotencia = fp;
            cuadro.Recalcular();

            Assert.Equal(11.81m, microondas.Resultado!.CorrienteDisenoA, 2);
            Assert.Equal(15m, microondas.Resultado.ProteccionA);
            Assert.Equal(caida, microondas.Resultado.CaidaTensionPct, 2);
        }
    }

    [Fact]
    public void P5_1_LosKWSonLosRealesYElFPSeCombina()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 1, TipoCarga.Equipo, UnidadConsumo.Watts, 600m, 0m, 0.8m);
        Capturar(cuadro, 3, TipoCarga.Equipo, UnidadConsumo.Watts, 1500m, 0m, 1m);
        Capturar(cuadro, 5, TipoCarga.Equipo, UnidadConsumo.Watts, 1550m, 0m, 1m);

        Assert.Equal(3.65m, cuadro.Resumen.InstaladaW / 1000m, 2);
        Assert.Equal(0.99m, cuadro.Resumen.FactorPotencia, 2);
        Assert.Equal(1.00m, cuadro.Alimentador.FactorPotencia, 2);
    }

    [Fact]
    public void P5_2_ElFPDelAlimentadorEsPEntreS()
    {
        var cuadro = Nuevo();
        Capturar(cuadro, 1, TipoCarga.Equipo, UnidadConsumo.VoltAmperes, 1000m, 0m, 1m);
        Capturar(cuadro, 2, TipoCarga.Equipo, UnidadConsumo.VoltAmperes, 1000m, 0m, 0.8m);

        // P = 1800 W, Q = 600 VAR, S = 1897 VA → 0.949
        Assert.Equal(0.95m, cuadro.Alimentador.FactorPotencia, 2);
    }

    // ---- 7 · Avisos del principal -------------------------------------------------------------------

    [Fact]
    public void P7_LosAvisosDelPrincipal()
    {
        var cuadro = CasoBase();
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.StartsWith("El interruptor principal quedó igual que el derivado más grande (20 A)."));
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("mínimo"));

        cuadro.Datos.MinimoInterruptorPrincipalA = 30m;
        cuadro.Recalcular();
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.Contains("menor que el mínimo de 30 A"));
        Assert.Equal(20m, cuadro.InterruptorPrincipalA); // se reporta, no se aplica

        cuadro.Datos.MinimoInterruptorPrincipalA = 20m;
        cuadro.Recalcular();
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("mínimo"));
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("Excel"));
    }

    // ---- 8 · Tamaños de interruptor -----------------------------------------------------------------

    [Fact]
    public void P8_1_CasoBasePorFamilia()
    {
        foreach (var (serie, esperado) in new[]
                 {
                     (SerieDeInterruptores.CentroDeCargaNema, new[] { 15m, 15m, 20m, 20m }),
                     (SerieDeInterruptores.RielDinIec, new[] { 16m, 16m, 16m, 16m }),
                     (SerieDeInterruptores.NomCompleta, new[] { 15m, 15m, 16m, 16m }),
                 })
        {
            var cuadro = CasoBase();
            cuadro.Datos.SerieInterruptores = serie;
            cuadro.Recalcular();

            Assert.Equal(esperado, new[]
            {
                Espacio(cuadro, 1).Resultado!.ProteccionA,
                Espacio(cuadro, 3).Resultado!.ProteccionA,
                Espacio(cuadro, 5).Resultado!.ProteccionA,
                cuadro.InterruptorPrincipalA,
            });
        }
    }

    [Fact]
    public void P8_2_y_8_3_LaFamiliaMueveProteccionYCalibre()
    {
        var casos = new (SerieDeInterruptores Serie, decimal NoContinua, decimal Continua, decimal L, decimal Prot, string Calibre)[]
        {
            (SerieDeInterruptores.CentroDeCargaNema, 0m, 720m, 20m, 15m, "14"), // 8.2
            (SerieDeInterruptores.RielDinIec, 0m, 720m, 20m, 16m, "12"),        // 8.2 — 240-4(d)
            (SerieDeInterruptores.NomCompleta, 6732m, 0m, 5m, 60m, "6"),        // 8.3 — 240-4(b)
            (SerieDeInterruptores.RielDinIec, 6732m, 0m, 5m, 63m, "4"),         // 8.3
        };

        foreach (var k in casos)
        {
            var cuadro = Nuevo();
            cuadro.Datos.SerieInterruptores = k.Serie;
            var c = Capturar(cuadro, 1, TipoCarga.Alumbrado, UnidadConsumo.VoltAmperes, k.Continua, k.NoContinua, 0.9m, k.L);

            Assert.Equal(k.Prot, c.Resultado!.ProteccionA);
            Assert.Equal(k.Calibre, c.Resultado.CalibreFase.Designacion);
        }
    }

    [Fact]
    public void P8_4_RielDinArribaDe125A()
    {
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 2), 3));
        Capturar(cuadro, 2, TipoCarga.Alumbrado, UnidadConsumo.VoltAmperes, 0m, 60000m);

        Assert.Contains(cuadro.Alimentador.Avisos, a =>
            a.StartsWith("En riel DIN no hay interruptores de más de 125 A") && a.Contains("circuito 2") && a.Contains("principal"));
    }

    // ---- 10 · No continuas ------------------------------------------------------------------------

    [Fact]
    public void P10_1_NoContinuasSinEl125()
    {
        var cuadro = CasoBase();
        var microondas = Capturar(cuadro, 3, TipoCarga.Equipo, UnidadConsumo.VoltAmperes, 0m, 1500m);
        var freidora = Capturar(cuadro, 5, TipoCarga.Equipo, UnidadConsumo.VoltAmperes, 0m, 1550m);

        Assert.Equal(15m, microondas.Resultado!.ProteccionA);
        Assert.Equal("12", microondas.Resultado.CalibreFase.Designacion); // sube por caída
        Assert.Equal(2.24m, microondas.Resultado.CaidaTensionPct, 2);
        Assert.Equal(15m, freidora.Resultado!.ProteccionA);
        Assert.Equal(2.31m, freidora.Resultado.CaidaTensionPct, 2);
        Assert.Equal('C', cuadro.Alimentador.Gobierna!.Fase);
        Assert.Equal(15m, cuadro.InterruptorPrincipalA);
    }

    // ---- 11 a 15 · Selección de conductor ---------------------------------------------------------

    [Fact]
    public void P11_AislamientoYLugar()
    {
        foreach (var (aislamiento, seco, calibre) in new[] { ("THHN", true, "10"), ("THW-LS", true, "8"), ("THHN", false, (string?)null) })
        {
            var cuadro = Nuevo();
            cuadro.Datos.ConductoresAgrupados = 9;
            cuadro.Datos.TipoAislamiento = aislamiento;
            cuadro.Datos.LugarSeco = seco;
            var c = Capturar(cuadro, 1, TipoCarga.Alumbrado, UnidadConsumo.Amperes, 0m, 26m, 0.9m, 5m);

            if (calibre is null)
                Assert.Contains("THHN", c.Error);
            else
                Assert.Equal(calibre, c.Resultado!.CalibreFase.Designacion);
        }
    }

    [Fact]
    public void P13_El125AntesDeFactoresYLaCargaDespues()
    {
        var cuadro = Nuevo();
        cuadro.Datos.ConductoresAgrupados = 9;
        var c = Capturar(cuadro, 1, TipoCarga.Alumbrado, UnidadConsumo.Amperes, 32m, 0m, 0.9m, 5m);
        var conductor = cuadro.Desglose(c)!.Conductor;

        Assert.Equal(40m, c.Resultado!.ProteccionA);
        Assert.Equal("8", c.Resultado.CalibreFase.Designacion);
        Assert.Contains("Antes de factores: 40.00 A a 60 °C ≥ capacidad mínima 40.00 A ✔ — 210-19(a)(1)", conductor);
        Assert.Contains("Con factores: 38.50 A ≥ carga 32.00 A ✔ — 210-19(a)(1)", conductor);
        Assert.Contains("Protección 40 A > 38.50 A: estándar inmediato superior permitido — 240-4(b)", conductor);
    }

    [Fact]
    public void P14_Terminales()
    {
        foreach (var (marcadas75, aislamiento, calibre) in new[] { (false, "THHN", "6"), (true, "THHN", "8"), (true, "TW", "6") })
        {
            var cuadro = Nuevo();
            cuadro.Datos.TerminalesMarcadas75C = marcadas75;
            cuadro.Datos.TipoAislamiento = aislamiento;
            var c = Capturar(cuadro, 1, TipoCarga.Equipo, UnidadConsumo.Amperes, 0m, 45m, 0.9m, 5m);

            Assert.Null(c.Error);
            Assert.Equal(calibre, c.Resultado!.CalibreFase.Designacion);
        }
    }

    [Fact]
    public void P15_240_4b_EnEquipoSiEnContactosNo()
    {
        foreach (var (tipo, calibre) in new[] { (TipoCarga.Equipo, "8"), (TipoCarga.Contactos, "6") })
        {
            var cuadro = Nuevo();
            cuadro.Datos.ConductoresAgrupados = 9;
            var c = Capturar(cuadro, 1, tipo, UnidadConsumo.Amperes, 32m, 0m, 0.9m, 5m);

            Assert.Equal(calibre, c.Resultado!.CalibreFase.Designacion);
        }
    }
}
