using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;

namespace PowerNode.Web.Tests;

/// <summary>
/// El cuadro de carga armado: <b>qué barra toca cada espacio, cómo se reparte la carga y qué sale
/// del alimentador</b>.
///
/// <para>
/// Los números de los circuitos NO se copiaron de la implementación: son los que la versión de
/// escritorio ya produce para el mismo caso, anotados en <c>docs/estado/TABLERO.md</c> — 720 VA de
/// alumbrado a 127 V dan 5.67 A, protección de 15 A, calibre 12 AWG.
/// </para>
/// </summary>
public class CuadroDeCargaTests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static CuadroDeCarga Nuevo(int espacios = 12, int fases = 3, int hilos = 4, decimal tension = 220m)
    {
        var cuadro = new CuadroDeCarga(new MotorNom(Json));
        cuadro.Datos.NumeroEspacios = espacios;
        cuadro.Datos.Fases = fases;
        cuadro.Datos.Hilos = hilos;
        cuadro.Datos.TensionFaseFaseV = tension;
        cuadro.Recalcular();
        return cuadro;
    }

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int numero) =>
        cuadro.Circuitos.Single(c => c.Espacio == numero);

    // ---- La geometría del tablero --------------------------------------------------------------

    [Fact]
    public void LasBarrasRotanPorPares_NonesALaIzquierdaYParesALaDerecha()
    {
        var cuadro = Nuevo();

        // Convención NEMA: los dos espacios a la misma altura muerden la misma barra.
        Assert.Equal("A", Espacio(cuadro, 1).Fases);
        Assert.Equal("A", Espacio(cuadro, 2).Fases);
        Assert.Equal("B", Espacio(cuadro, 3).Fases);
        Assert.Equal("B", Espacio(cuadro, 4).Fases);
        Assert.Equal("C", Espacio(cuadro, 5).Fases);
        Assert.Equal("C", Espacio(cuadro, 6).Fases);
        Assert.Equal("A", Espacio(cuadro, 7).Fases);

        Assert.Equal(6, cuadro.Nones.Count());
        Assert.Equal(6, cuadro.Pares.Count());
    }

    [Fact]
    public void UnTrifasicoOcupaTresEspaciosDelMismoLadoYTocaLasTresBarras()
    {
        var cuadro = Nuevo();
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 2), 3));

        Assert.Equal("ABC", Espacio(cuadro, 2).Fases);
        Assert.Equal(2, Espacio(cuadro, 4).ContinuacionDe);
        Assert.Equal(2, Espacio(cuadro, 6).ContinuacionDe);

        // El de enfrente no se toca: un multipolar se apila en SU columna.
        Assert.Null(Espacio(cuadro, 3).ContinuacionDe);
    }

    [Fact]
    public void NoSePuedeMontarUnInterruptorEncimaDeOtro()
    {
        var cuadro = Nuevo();
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 1), 3));

        var motivo = cuadro.CambiarPolos(Espacio(cuadro, 5), 2);

        Assert.NotNull(motivo);
        Assert.Contains("circuito 1", motivo);
        Assert.Equal(1, Espacio(cuadro, 5).Polos); // y no se aplicó
    }

    [Fact]
    public void NoSePuedeMontarUnInterruptorQueNoCabeEnElTablero()
    {
        var cuadro = Nuevo(espacios: 12);

        var motivo = cuadro.CambiarPolos(Espacio(cuadro, 11), 3);

        Assert.NotNull(motivo);
        Assert.Contains("solo tiene 12", motivo);
    }

    [Fact]
    public void UnTableroDeDosBarrasNoAdmiteTresPolos()
    {
        var cuadro = Nuevo(fases: 1, hilos: 3, tension: 240m);

        Assert.Equal(2, cuadro.Datos.MaximoPolos);
        Assert.NotNull(cuadro.CambiarPolos(Espacio(cuadro, 1), 3));
    }

    [Fact]
    public void AchicarElTableroRecortaLoQueYaNoCabe()
    {
        var cuadro = Nuevo(espacios: 12);
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 9), 2)); // ocupa 9 y 11

        cuadro.Datos.NumeroEspacios = 6;
        cuadro.Recalcular();

        Assert.Equal(6, cuadro.Circuitos.Count);
        Assert.Equal(1, Espacio(cuadro, 5).Polos);
    }

    // ---- La tensión del sistema ----------------------------------------------------------------

    [Fact]
    public void LaTensionFaseNeutroNoEsSiempreEntreRaizDeTres()
    {
        // Estrella: 220 da 127.
        Assert.Equal(127.02m, Nuevo().Datos.TensionFaseNeutroV, 2);

        // Derivación central: 240 da 120, no 138.6 — que es lo que daría el ÷√3 del Excel.
        Assert.Equal(120m, Nuevo(fases: 1, hilos: 3, tension: 240m).Datos.TensionFaseNeutroV);

        // 1F-2H: la tensión capturada YA ES la fase-neutro.
        Assert.Equal(127m, Nuevo(fases: 1, hilos: 2, tension: 127m).Datos.TensionFaseNeutroV);
    }

    // ---- El cálculo de un renglón --------------------------------------------------------------

    [Fact]
    public void UnCircuitoDeAlumbradoDaLosMismosNumerosQueElEscritorio()
    {
        var cuadro = Nuevo();
        var circuito = Espacio(cuadro, 1);
        circuito.ContinuaVA = 720m;
        circuito.LongitudM = 20m;
        cuadro.Recalcular();

        var r = circuito.Resultado;
        Assert.NotNull(r);
        Assert.Equal(5.67m, r!.CorrienteDisenoA, 2);
        Assert.Equal(15m, r.ProteccionA);
        Assert.Equal("12", r.CalibreFase.Designacion);
        Assert.Null(circuito.Error);
    }

    [Fact]
    public void ElCircuitoSeCalculaConSusPolos_NoConLasFasesDelTablero()
    {
        // El bug de la primera versión de la pantalla: pasar las fases del TABLERO repartía la carga
        // de un circuito de 1 polo entre tres fases y daba 1.89 A en vez de 5.67 A.
        var cuadro = Nuevo();
        var circuito = Espacio(cuadro, 1);
        circuito.ContinuaVA = 720m;
        cuadro.Recalcular();

        Assert.Equal(720m / cuadro.Datos.TensionFaseNeutroV, circuito.Resultado!.CorrienteDisenoA, 2);
    }

    [Fact]
    public void UnRenglonOcupadoPorUnMultipolarNoAportaCarga()
    {
        var cuadro = Nuevo();
        Espacio(cuadro, 3).ContinuaVA = 1000m;   // se va a quedar tapado
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 1), 2)); // ocupa 1 y 3

        Assert.True(Espacio(cuadro, 3).EsContinuacion);
        Assert.Null(Espacio(cuadro, 3).Resultado);
        Assert.Equal(0m, cuadro.Resumen.InstaladaVA);
    }

    // ---- Balanceo y desbalanceo ----------------------------------------------------------------

    [Fact]
    public void UnTrifasicoRepartesuCargaEntreLasTresFases()
    {
        var cuadro = Nuevo();
        Espacio(cuadro, 2).NoContinuaVA = 3000m;
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 2), 3));

        foreach (var barra in cuadro.Datos.Barras)
            Assert.Equal(1000m, cuadro.Resumen.CargaPorFaseVA[barra]);

        // Y no desbalancea: toca las tres barras por igual.
        Assert.Equal(0m, cuadro.Resumen.DesbalanceoPct);
    }

    [Fact]
    public void UnMonofasicoSoloEnUnaBarraDesbalancea()
    {
        var cuadro = Nuevo();
        Espacio(cuadro, 1).NoContinuaVA = 2000m; // barra A

        cuadro.Recalcular();

        Assert.Equal(2000m, cuadro.Resumen.CargaPorFaseVA['A']);
        Assert.Equal(0m, cuadro.Resumen.CargaPorFaseVA['B']);
        Assert.Equal(100m, cuadro.Resumen.DesbalanceoPct);
    }

    // ---- Alimentador e interruptor principal ---------------------------------------------------

    [Fact]
    public void ElInterruptorPrincipalEsLaProteccionDelAlimentador()
    {
        var cuadro = Nuevo();
        foreach (var numero in new[] { 1, 2, 3, 4, 5, 6 })
            Espacio(cuadro, numero).ContinuaVA = 1500m;
        cuadro.Recalcular();

        var alimentador = cuadro.Alimentador.Resultado;
        Assert.NotNull(alimentador);
        Assert.Equal(alimentador!.ProteccionA, cuadro.InterruptorPrincipalA);
        Assert.True(cuadro.InterruptorPrincipalA > 0m);

        // 9 000 VA trifásicos a 220 V: In = 9000 / (√3 × 220) = 23.62 A.
        Assert.Equal(23.62m, alimentador.CorrienteDisenoA, 2);
    }

    [Fact]
    public void SinCargaNoHayAlimentador()
    {
        var cuadro = Nuevo();

        Assert.Null(cuadro.Alimentador.Resultado);
        Assert.Equal(0m, cuadro.InterruptorPrincipalA);
    }

    [Fact]
    public void ElFactorDeDemandaBajaLaCargaDelAlimentador_YQuedaEscritoEnLaMemoria()
    {
        var cuadro = Nuevo();
        foreach (var numero in new[] { 1, 2, 3, 4, 5, 6 })
            Espacio(cuadro, numero).ContinuaVA = 3000m;
        cuadro.Recalcular();
        var sinDemanda = cuadro.Alimentador.Resultado!.CorrienteDisenoA;

        cuadro.Datos.FactorDemandaContinua = 0.5m;
        cuadro.Recalcular();

        Assert.Equal(sinDemanda / 2m, cuadro.Alimentador.Resultado!.CorrienteDisenoA, 2);
        Assert.Contains(cuadro.Alimentador.Resultado.Citas, c => c.Referencia == "220-40");
    }

    [Fact]
    public void ElAvisoDel408_36SaleSoloCuandoHayCapacidadDeBarraCapturada()
    {
        var cuadro = Nuevo();
        foreach (var numero in new[] { 1, 2, 3, 4, 5, 6 })
            Espacio(cuadro, numero).ContinuaVA = 5000m;
        cuadro.Recalcular();

        // Sin el dato no se dictamina nada: un campo vacío no es un incumplimiento.
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("408"));
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("barra del tablero"));

        cuadro.Datos.CapacidadBarraA = 30m;
        cuadro.Recalcular();

        Assert.Contains(cuadro.Alimentador.Avisos, a => a.Contains("barra del tablero"));
    }

    [Fact]
    public void LosCriteriosDelExcelSeReportan_NoSeAplican()
    {
        var cuadro = Nuevo();
        Espacio(cuadro, 1).ContinuaVA = 720m;
        cuadro.Recalcular();

        // El Excel nunca bajaba de 30 A el principal. El motor dice lo que dice la 240-6(a), y el
        // criterio de diseño se avisa.
        Assert.True(cuadro.InterruptorPrincipalA < 30m);
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.Contains("30 A"));
    }

    // ---- El interior del gabinete ---------------------------------------------------------------

    [Fact]
    public void ElGabineteDibujaUnBloquePorInterruptor_NoUnoPorEspacio()
    {
        var cuadro = Nuevo(espacios: 12);
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 1), 3)); // ocupa 1-3-5

        var trifasico = cuadro.Gabinete.Single(b => b.Circuito.Espacio == 1);

        Assert.Equal("1-3-5", trifasico.Numeros);
        Assert.Equal("ABC", trifasico.Barras);
        Assert.Equal(3, trifasico.Espacios);
        Assert.Equal(1, trifasico.Fila);
        Assert.Equal(1, trifasico.Columna);

        // Los espacios 3 y 5 ya no son bloques propios: se los comió el de arriba.
        Assert.DoesNotContain(cuadro.Gabinete, b => b.Circuito.Espacio is 3 or 5);
    }

    [Fact]
    public void NonesALaIzquierdaYParesALaDerecha_ConSuRenglon()
    {
        var cuadro = Nuevo(espacios: 12);

        foreach (var (espacio, fila, columna) in new[] { (1, 1, 1), (2, 1, 2), (3, 2, 1), (4, 2, 2), (11, 6, 1), (12, 6, 2) })
        {
            var bloque = cuadro.Gabinete.Single(b => b.Circuito.Espacio == espacio);
            Assert.Equal(fila, bloque.Fila);
            Assert.Equal(columna, bloque.Columna);
        }
    }

    [Fact]
    public void LaOcupacionCuentaPolos_YSoloLoQueTieneCarga()
    {
        var cuadro = Nuevo(espacios: 12);
        Assert.Equal(0, cuadro.EspaciosOcupados);
        Assert.Equal(12, cuadro.EspaciosLibres);

        Espacio(cuadro, 2).ContinuaVA = 3000m;
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 2), 3)); // 2-4-6, con carga

        Assert.Equal(3, cuadro.EspaciosOcupados);
        Assert.Equal(9, cuadro.EspaciosLibres);
    }
}
