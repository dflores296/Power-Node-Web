using PowerNode.DesignSuite.Calculo.Casos;
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
    public void UnCircuitoDeAlumbradoDaLaCorrienteYLaProteccionDelEscritorio()
    {
        var cuadro = Nuevo();
        var circuito = Espacio(cuadro, 1);
        circuito.Continua = 720m;
        circuito.LongitudM = 20m;
        cuadro.Recalcular();

        var r = circuito.Resultado;
        Assert.NotNull(r);
        Assert.Equal(5.67m, r!.CorrienteDisenoA, 2);
        Assert.Equal(15m, r.ProteccionA);
        Assert.Null(circuito.Error);

        // 14 AWG, no 12. El escritorio da 12 porque trae ENCENDIDO el piso práctico de calibre;
        // aquí no hay piso — lo quitó David el 2026-09-22. Ver
        // docs/decisiones/sin-piso-practico-de-calibre.md. 14 AWG es lo que permite la norma para
        // un derivado de 15 A (210-19(a)(4)) y lo que cumple la caída de tensión en este tramo.
        Assert.Equal("14", r.CalibreFase.Designacion);
    }

    [Fact]
    public void ConLaMismaCargaLosTresTiposDanElMismoCalibre()
    {
        // Era lo que el piso práctico rompía: contactos salía en 10 AWG y un equipo con la misma
        // carga por fase en 12, sin que ningún artículo de la norma lo pidiera.
        var cuadro = Nuevo();
        foreach (var (espacio, tipo) in new[] { (1, TipoCarga.Alumbrado), (3, TipoCarga.Contactos), (5, TipoCarga.Equipo) })
        {
            var c = Espacio(cuadro, espacio);
            c.Tipo = tipo;
            c.NoContinua = 1500m;
            c.LongitudM = 20m;
        }
        cuadro.Recalcular();

        var calibres = cuadro.Circuitos
            .Where(c => c.Resultado is not null)
            .Select(c => c.Resultado!.CalibreFase.Designacion)
            .Distinct()
            .ToList();

        Assert.Equal(["12"], calibres);
    }

    [Fact]
    public void LoQueSIGUE_SeparandoAContactosEsElPisoDeProteccionDelMotor()
    {
        // Con cargas chicas, contactos todavía sale con más cobre que alumbrado: no es el piso de
        // calibre (que ya no existe) sino el piso de PROTECCIÓN de 20 A que aplica la calculadora
        // copiada -- el mismo MAX(20, ...) que traía el Excel. Vive en el motor, así que si se
        // quisiera cambiar, se cambia allá primero. Esta prueba existe para que el día que eso
        // pase, se note aquí.
        var cuadro = Nuevo();
        var alumbrado = Espacio(cuadro, 1);
        alumbrado.NoContinua = 500m;
        alumbrado.LongitudM = 10m;

        var contactos = Espacio(cuadro, 3);
        contactos.Tipo = TipoCarga.Contactos;
        contactos.NoContinua = 500m;
        contactos.LongitudM = 10m;

        cuadro.Recalcular();

        Assert.Equal(15m, alumbrado.Resultado!.ProteccionA);
        Assert.Equal("14", alumbrado.Resultado.CalibreFase.Designacion);

        Assert.Equal(20m, contactos.Resultado!.ProteccionA);
        Assert.Equal("12", contactos.Resultado.CalibreFase.Designacion);
    }

    [Fact]
    public void ElCircuitoSeCalculaConSusPolos_NoConLasFasesDelTablero()
    {
        // El bug de la primera versión de la pantalla: pasar las fases del TABLERO repartía la carga
        // de un circuito de 1 polo entre tres fases y daba 1.89 A en vez de 5.67 A.
        var cuadro = Nuevo();
        var circuito = Espacio(cuadro, 1);
        circuito.Continua = 720m;
        cuadro.Recalcular();

        Assert.Equal(720m / cuadro.Datos.TensionFaseNeutroV, circuito.Resultado!.CorrienteDisenoA, 2);
    }

    [Fact]
    public void UnRenglonOcupadoPorUnMultipolarNoAportaCarga()
    {
        var cuadro = Nuevo();
        Espacio(cuadro, 3).Continua = 1000m;   // se va a quedar tapado
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
        Espacio(cuadro, 2).NoContinua = 3000m;
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
        Espacio(cuadro, 1).NoContinua = 2000m; // barra A

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
            Espacio(cuadro, numero).Continua = 1500m;
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
            Espacio(cuadro, numero).Continua = 3000m;
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
            Espacio(cuadro, numero).Continua = 5000m;
        cuadro.Recalcular();

        // Sin el dato no se dictamina nada: un campo vacío no es un incumplimiento.
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("408"));
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("barra del tablero"));

        cuadro.Datos.CapacidadBarraA = 30m;
        cuadro.Recalcular();

        Assert.Contains(cuadro.Alimentador.Avisos, a => a.Contains("barra del tablero"));
    }

    [Fact]
    public void SinMinimoCapturadoNoHayAvisoDeMinimo()
    {
        // Antes salía «el Excel nunca bajaba de 30 A» en cualquier tablero chico. Ahora el mínimo
        // lo pide el proyectista, y si no lo pide no se dice nada.
        var cuadro = Nuevo();
        Espacio(cuadro, 1).Continua = 720m;
        cuadro.Recalcular();

        Assert.Null(cuadro.Datos.MinimoInterruptorPrincipalA);
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("mínimo"));
    }

    [Fact]
    public void ConMinimoCapturadoSoloAvisa_NoSubeElPrincipal()
    {
        var cuadro = Nuevo();
        Espacio(cuadro, 1).Continua = 720m;
        cuadro.Datos.MinimoInterruptorPrincipalA = 30m;
        cuadro.Recalcular();

        Assert.Equal(15m, cuadro.InterruptorPrincipalA); // el calculado, no el mínimo
        Assert.Contains(cuadro.Alimentador.Avisos,
            a => a == "El interruptor principal calculado es de 15 A, menor que el mínimo de 30 A que pediste para este tablero.");

        cuadro.Datos.MinimoInterruptorPrincipalA = 15m;
        cuadro.Recalcular();
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("mínimo"));
    }

    [Fact]
    public void NingunAvisoLeHablaAlUsuarioDelExcel()
    {
        // El caso de los tres aparatos dispara el aviso de «igual que el derivado más grande»
        // (principal y air fryer en 20 A); con mínimo de 30 A dispara también el de mínimo.
        var cuadro = TresAparatos();
        cuadro.Datos.MinimoInterruptorPrincipalA = 30m;
        cuadro.Recalcular();

        Assert.Contains(cuadro.Alimentador.Avisos, a => a.StartsWith("El interruptor principal quedó igual que el derivado más grande (20 A)"));
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.Contains("mínimo de 30 A"));
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("Excel"));
    }

    // ---- La prueba del 2026-09-22: refrigerador, microondas y air fryer --------------------------
    //
    // Tres cargas en un 3F-4H 220/127 V de 6 espacios, cobre THHN en PVC a 30 °C, 3 agrupados,
    // FP 0.9, 20 m, e% máx. 3 %, las tres de tipo Equipo, una por fase. Los derivados se verificaron
    // a mano; el alimentador salió subdimensionado (M-02). Es el caso de regresión de M-02 y M-03.

    private static CuadroDeCarga TresAparatos(bool microondasYFreidoraContinuas = true)
    {
        var cuadro = Nuevo(espacios: 6);
        foreach (var (espacio, va, continua) in new[]
                 {
                     (1, 750m, true),                            // refrigerador, fase A
                     (3, 1500m, microondasYFreidoraContinuas),   // microondas, fase B
                     (5, 1550m, microondasYFreidoraContinuas),   // air fryer, fase C
                 })
        {
            var c = Espacio(cuadro, espacio);
            c.Tipo = TipoCarga.Equipo;
            c.LongitudM = 20m;
            if (continua) c.Continua = va; else c.NoContinua = va;
        }
        cuadro.Recalcular();
        return cuadro;
    }

    // ---- R-01 · Caída del alimentador: 3 % por tramo, 5 % combinada — 215-2(a)(4) NOTA 2 ---------------

    [Fact]
    public void R01_ElAlimentadorAdmite3PorCientoPorOmision() =>
        Assert.Equal(3m, new DatosDelTablero().CaidaMaxAlimentadorPct);

    [Fact]
    public void R01_CasoBaseCon80m_AvisaLaCaidaCombinadaDeCadaCircuito()
    {
        var cuadro = TresAparatos();
        cuadro.Datos.LongitudAlimentadorM = 80m;
        cuadro.Datos.CaidaMaxAlimentadorPct = 5m; // el límite de antes: el alimentador se queda en 12 AWG
        cuadro.Recalcular();

        Assert.Equal(4.62m, cuadro.Alimentador.Resultado!.CaidaTensionPct, 2);
        var airFryer = Espacio(cuadro, 5);
        Assert.Equal(6.94m, airFryer.CaidaCombinadaPct!.Value, 2); // 4.62 % + 2.31 %
        Assert.Equal(
            "Caída combinada del circuito 5: alimentador 4.62 % + circuito 2.31 % = 6.94 %, mayor que el 5 % " +
            "recomendado — 215-2(a)(4) NOTA 2, 210-19(a)(1) NOTA 4.",
            airFryer.AvisoCaidaCombinada);
        Assert.Equal([1, 3, 5], cuadro.ConCaidaCombinadaExcedida.Select(c => c.Espacio));
    }

    [Fact]
    public void R01_CasoBaseCon80m_ConEl3PorCientoElAlimentadorSube()
    {
        var cuadro = TresAparatos();
        cuadro.Datos.LongitudAlimentadorM = 80m;
        cuadro.Recalcular();

        var alimentador = cuadro.Alimentador.Resultado!;
        Assert.Equal("10", alimentador.CalibreFase.Designacion);
        Assert.Equal(2.75m, alimentador.CaidaTensionPct, 2);
        // Aun así, el circuito más largo de la fase C pasa del 5 %: 2.75 % + 2.31 %.
        Assert.Equal([5], cuadro.ConCaidaCombinadaExcedida.Select(c => c.Espacio));
    }

    [Fact]
    public void R01_CasoBaseCon20m_SinAvisoDeCaidaCombinada()
    {
        var cuadro = TresAparatos();

        Assert.Equal(3.47m, Espacio(cuadro, 5).CaidaCombinadaPct!.Value, 2); // 1.16 % + 2.31 %
        Assert.Empty(cuadro.ConCaidaCombinadaExcedida);
    }

    [Fact]
    public void M02_ElAlimentadorSeDimensionaConLaFaseMasCargada()
    {
        // Con la lista completa de la NOM, que es con la que se hizo la prueba del 2026-09-22.
        var cuadro = TresAparatos();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.NomCompleta;
        cuadro.Recalcular();
        var a = cuadro.Alimentador;

        // Con la carga total repartida entre √3·220 daba 9.97 A → principal de 15 A y fase de 14 AWG.
        // La fase C lleva la air fryer: 1550 / 127.02 = 12.20 A continuos → 15.25 A al 125 % (el
        // reporte dice 15.26 porque redondeó la tensión a 127 V; aquí es 220/√3).
        Assert.Equal('C', a.Gobierna!.Fase);
        Assert.Equal(12.20m, a.Resultado!.CorrienteDisenoA, 2);
        Assert.Equal(15.25m, a.Resultado.Detalle!.CapacidadMinimaA, 2);

        // El principal no puede quedar en 15 A: 16 A por la 240-6(a) de la NOM, y 12 AWG.
        Assert.Equal(16m, cuadro.InterruptorPrincipalA);
        Assert.Equal("12", a.Resultado.CalibreFase.Designacion);
    }

    [Fact]
    public void M02_LaCorrienteDeCadaFaseEsLaDelDesbalanceo_NoLosVAEntreTres()
    {
        // Un interruptor de 2 polos a 220 V lleva su corriente completa por cada línea: 2200 / 220 =
        // 10 A en A y en B. Repartir sus VA (1100 por barra / 127 = 8.66 A) se quedaría corto.
        var cuadro = Nuevo();
        Espacio(cuadro, 1).NoContinua = 2200m;
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 1), 2)); // 1-3, barras A y B

        var fases = cuadro.Alimentador.Fases!;
        Assert.Equal(10m, fases.Single(f => f.Fase == 'A').TotalA, 2);
        Assert.Equal(10m, fases.Single(f => f.Fase == 'B').TotalA, 2);
        Assert.Equal(0m, fases.Single(f => f.Fase == 'C').TotalA);
        Assert.Equal(10m, cuadro.Alimentador.Resultado!.CorrienteDisenoA, 2);
    }

    [Fact]
    public void M02_UnTableroBalanceadoDaLoMismoQueAntes()
    {
        // Seis circuitos iguales, dos por fase: la fase más cargada lleva exactamente un tercio, así
        // que el resultado no cambia respecto a la carga total entre √3·220.
        var cuadro = Nuevo();
        foreach (var numero in new[] { 1, 2, 3, 4, 5, 6 })
            Espacio(cuadro, numero).Continua = 1500m;
        cuadro.Recalcular();

        Assert.Equal(23.62m, cuadro.Alimentador.Resultado!.CorrienteDisenoA, 2);
    }

    [Fact]
    public void M02_ElFactorDeDemandaSeAplicaAntesDeElegirLaFase()
    {
        // Fase A: 3000 VA continuos; fase B: 3500 VA no continuos. Sin demanda gobierna A
        // (1.25 × 23.62 = 29.53 A contra 27.56 A); con 0.5 sobre la continua gobierna B.
        var cuadro = Nuevo();
        Espacio(cuadro, 1).Continua = 3000m;
        Espacio(cuadro, 3).NoContinua = 3500m;
        cuadro.Recalcular();
        Assert.Equal('A', cuadro.Alimentador.Gobierna!.Fase);

        cuadro.Datos.FactorDemandaContinua = 0.5m;
        cuadro.Recalcular();
        Assert.Equal('B', cuadro.Alimentador.Gobierna!.Fase);
        Assert.Equal(3500m / cuadro.Datos.TensionFaseNeutroV, cuadro.Alimentador.Resultado!.CorrienteDisenoA, 2);

        // Y la cita del 220-40 habla de la carga del tablero, no de la equivalente de la fase.
        var cita = Assert.Single(cuadro.Alimentador.Resultado.Citas, c => c.Referencia == "220-40");
        Assert.Contains("continua 3000 VA x 0.5 = 1500", cita.Descripcion);
    }

    [Fact]
    public void M03_SeAvisaCuandoElPrincipalEsMenorQueUnDerivado()
    {
        // 500 VA de contactos: el derivado sale en 20 A por el mínimo de contactos, y el
        // alimentador —3.94 A— en 15 A.
        var cuadro = Nuevo();
        var contactos = Espacio(cuadro, 3);
        contactos.Tipo = TipoCarga.Contactos;
        contactos.NoContinua = 500m;
        cuadro.Recalcular();

        Assert.Equal(20m, contactos.Resultado!.ProteccionA);
        Assert.Equal(15m, cuadro.InterruptorPrincipalA);
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.Contains("menor que el derivado más grande") && a.Contains("circuito 3"));
    }

    [Fact]
    public void M03_NoSeAvisaCuandoElPrincipalAlcanzaAlDerivado()
    {
        var cuadro = TresAparatos();

        Assert.Equal(20m, cuadro.InterruptorPrincipalA);
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("menor que el derivado más grande"));
    }

    [Fact]
    public void ElMotorDistingueContinuaDeNoContinua_EnElDerivadoYEnElAlimentador()
    {
        // La prueba pendiente del 2026-09-22: microondas y air fryer como NO continuas (210-19: no
        // operan 3 h seguidas). Valores esperados calculados a mano.
        var cuadro = TresAparatos(microondasYFreidoraContinuas: false);

        var microondas = Espacio(cuadro, 3).Resultado!;
        Assert.Equal(11.81m, microondas.CorrienteDisenoA, 2);
        Assert.Equal(15m, microondas.ProteccionA);
        Assert.Equal("12", microondas.CalibreFase.Designacion); // sube por caída
        Assert.Equal(2.24m, microondas.CaidaTensionPct, 2);

        var freidora = Espacio(cuadro, 5).Resultado!;
        Assert.Equal(12.20m, freidora.CorrienteDisenoA, 2);
        Assert.Equal(15m, freidora.ProteccionA);
        Assert.Equal("12", freidora.CalibreFase.Designacion);
        Assert.Equal(2.31m, freidora.CaidaTensionPct, 2);

        // Y en el alimentador la fase C ya no entra al 125 %: 12.20 A → 15 A.
        Assert.Equal('C', cuadro.Alimentador.Gobierna!.Fase);
        Assert.Equal(12.20m, cuadro.Alimentador.Resultado!.Detalle!.CapacidadMinimaA, 2);
        Assert.Equal(15m, cuadro.InterruptorPrincipalA);
    }

    // ---- I-25: la carga como viene en la placa ------------------------------------------------

    [Fact]
    public void I25_PorOmisionLaCargaEsEnVA()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Continua = 720m;
        cuadro.Recalcular();

        Assert.Equal(UnidadConsumo.VoltAmperes, c.Unidad);
        Assert.Equal(720m, c.ContinuaVA);
    }

    [Fact]
    public void I25_LosWattsSeConviertenConElFactorDePotencia()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 5);
        c.Unidad = UnidadConsumo.Watts;
        c.Continua = 1550m;
        cuadro.Recalcular();

        Assert.Equal(1550m / 0.9m, c.ContinuaVA, 2);
    }

    [Fact]
    public void I25_LosAmperesCapturadosRegresanComoLosMismosAmperes()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Unidad = UnidadConsumo.Amperes;
        c.NoContinua = 8m;
        cuadro.Recalcular();

        Assert.Equal(8m, c.Resultado!.CorrienteDisenoA, 2);
    }

    // ---- El factor de potencia es de cada carga, no del tablero ---------------------------------

    [Fact]
    public void FP_CadaCircuitoNaceCon0_9()
    {
        var cuadro = Nuevo();

        Assert.All(cuadro.Circuitos, c => Assert.Equal(0.9m, c.FactorPotencia));
    }

    [Fact]
    public void FP_LosWattsSeConviertenConElFPDeSuPropiaCarga()
    {
        // El refrigerador de la prueba: 600 W a F.P. 0.8 son 750 VA.
        var cuadro = Nuevo();
        var refrigerador = Espacio(cuadro, 1);
        refrigerador.Unidad = UnidadConsumo.Watts;
        refrigerador.FactorPotencia = 0.8m;
        refrigerador.Continua = 600m;
        cuadro.Recalcular();

        Assert.Equal(750m, refrigerador.ContinuaVA);
    }

    [Fact]
    public void FP_LosKWSonLaSumaDeLosWattsDeCadaCarga()
    {
        // Antes: 3800 VA × 0.9 del tablero = 3.42 kW. La suma real es 600 + 1500 + 1550 = 3.65 kW,
        // sea cual sea el F.P. de cada aparato.
        var cuadro = Nuevo(espacios: 6);
        foreach (var (espacio, watts, fp) in new[] { (1, 600m, 0.8m), (3, 1500m, 0.95m), (5, 1550m, 1m) })
        {
            var c = Espacio(cuadro, espacio);
            c.Tipo = TipoCarga.Equipo;
            c.Unidad = UnidadConsumo.Watts;
            c.FactorPotencia = fp;
            c.Continua = watts;
        }
        cuadro.Recalcular();

        Assert.Equal(3650m, cuadro.Resumen.InstaladaW, 2);
    }

    [Fact]
    public void FP_ElFPEntraEnLaCaidaDeTension_NoEnLaProteccion()
    {
        // La air fryer es resistiva. Con F.P. 1 la caída SUBE: en calibres chicos manda la R, y
        // Ze = R·FP + X·senθ crece con el FP (12 AWG: 6.04 Ω/km a 0.9, 6.60 a 1.0). O sea que el 0.9
        // del tablero subestimaba la caída de una carga resistiva — no era del lado conservador.
        // La protección no cambia: sale de la corriente.
        var cuadro = TresAparatos();
        var freidora = Espacio(cuadro, 5);
        var caidaCon09 = freidora.Resultado!.CaidaTensionPct;
        var proteccion = freidora.Resultado.ProteccionA;

        freidora.FactorPotencia = 1m;
        cuadro.Recalcular();

        Assert.True(freidora.Resultado!.CaidaTensionPct > caidaCon09);
        Assert.Equal(proteccion, freidora.Resultado.ProteccionA);
    }

    [Fact]
    public void FP_ElDelAlimentadorResultaDeLasCargasDeLaFaseQueGobierna()
    {
        // Barra A: 1000 VA a F.P. 1 y 1000 VA a F.P. 0.8. P = 1800 W, Q = 600 VAR,
        // S = √(1800² + 600²) = 1897.4 VA → F.P. 0.9487. No es el promedio (0.9).
        var cuadro = Nuevo();
        Espacio(cuadro, 1).Continua = 1000m;
        Espacio(cuadro, 1).FactorPotencia = 1m;
        Espacio(cuadro, 2).Continua = 1000m;
        Espacio(cuadro, 2).FactorPotencia = 0.8m;
        cuadro.Recalcular();

        Assert.Equal('A', cuadro.Alimentador.Gobierna!.Fase);
        Assert.Equal(0.9487m, cuadro.Alimentador.FactorPotencia, 4);
    }

    [Fact]
    public void FP_ElResultanteDelTableroSumaLosVAComoVectores()
    {
        Assert.Equal(1m, FactorPotenciaCombinado.De([(1000m, 1m), (500m, 1m)]));
        Assert.Equal(0.9487m, FactorPotenciaCombinado.De([(1000m, 1m), (1000m, 0.8m)]), 4);
        Assert.Equal(1m, FactorPotenciaCombinado.De([])); // sin carga no hay ángulo
    }

    // ---- Tamaños de interruptor: centro de carga, riel DIN o la NOM completa ---------------------

    [Fact]
    public void Serie_PorOmisionEsCentroDeCarga_YLaAirFryerQuedaEn20A()
    {
        // La air fryer pide 15.25 A. La NOM completa da 16 A, que no existe en un QO/NQ.
        var cuadro = TresAparatos();

        Assert.Equal(SerieDeInterruptores.CentroDeCargaNema, cuadro.Datos.SerieInterruptores);
        Assert.Equal(20m, Espacio(cuadro, 5).Resultado!.ProteccionA);
        Assert.Equal("12", Espacio(cuadro, 5).Resultado!.CalibreFase.Designacion);
        Assert.Equal(20m, cuadro.InterruptorPrincipalA);
    }

    [Fact]
    public void Serie_LaNomCompletaDa16A()
    {
        var cuadro = TresAparatos();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.NomCompleta;
        cuadro.Recalcular();

        Assert.Equal(16m, Espacio(cuadro, 5).Resultado!.ProteccionA);
        Assert.Equal(16m, cuadro.InterruptorPrincipalA);
    }

    [Fact]
    public void Serie_EnRielDinNoHay15A_YElAlumbradoPide12AWG()
    {
        // 720 VA de alumbrado: 15 A en centro de carga con 14 AWG. En riel DIN el más chico es 16 A,
        // y 240-4(d) no deja proteger un 14 AWG con más de 15 A: sube a 12.
        var cuadro = Nuevo();
        Espacio(cuadro, 1).Continua = 720m;
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        cuadro.Recalcular();

        Assert.Equal(16m, Espacio(cuadro, 1).Resultado!.ProteccionA);
        Assert.Equal("12", Espacio(cuadro, 1).Resultado!.CalibreFase.Designacion);
    }

    [Fact]
    public void Serie_En240_4bManda_ElSiguienteDeLaNorma_NoElDeLaSerie()
    {
        // 53 A: 6 AWG (55 A a 60 °C). La NOM completa da 60 A y 240-4(b) lo permite sobre 55 A. En
        // riel DIN sale 63 A, y el siguiente estándar de la NOM arriba de 55 es 60, no 63: el 6 AWG
        // ya no queda protegido y el conductor sube. Es Alumbrado porque la calculadora copiada solo
        // concede 240-4(b) ahí (en Equipo y Contactos siempre sube el conductor).
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Tipo = TipoCarga.Alumbrado;
        c.NoContinua = 53m * cuadro.Datos.TensionFaseNeutroV;
        c.LongitudM = 5m;

        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.NomCompleta;
        cuadro.Recalcular();
        Assert.Equal(60m, c.Resultado!.ProteccionA);
        Assert.Equal("6", c.Resultado.CalibreFase.Designacion);

        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        cuadro.Recalcular();
        Assert.Equal(63m, c.Resultado!.ProteccionA);
        Assert.Equal("4", c.Resultado.CalibreFase.Designacion);
    }

    [Fact]
    public void Serie_ArribaDe125AEnRielDinSeAvisa()
    {
        var cuadro = Nuevo();
        Espacio(cuadro, 2).NoContinua = 60000m; // 157 A trifásicos
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 2), 3));
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        cuadro.Recalcular();

        Assert.True(Espacio(cuadro, 2).Resultado!.ProteccionA > 125m);
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.Contains("riel DIN") && a.Contains("circuito 2") && a.Contains("el principal"));
    }

    // ---- R-06 · El aviso de riel DIN, en singular o plural -------------------------------------------

    /// <summary>Tablero de riel DIN con un trifásico de <paramref name="va"/> no continuos en cada espacio dado.</summary>
    private static CuadroDeCarga RielDinCon(params (int Espacio, decimal Va)[] trifasicos)
    {
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        foreach (var (espacio, va) in trifasicos)
        {
            Espacio(cuadro, espacio).NoContinua = va;
            Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, espacio), 3));
        }
        cuadro.Recalcular();
        return cuadro;
    }

    private static string? AvisoRielDin(CuadroDeCarga cuadro) =>
        cuadro.Alimentador.Avisos.SingleOrDefault(a => a.StartsWith("En riel DIN"));

    [Fact]
    public void R06_UnCircuitoYElPrincipal() =>
        Assert.Equal(
            "En riel DIN no hay interruptores de más de 125 A. El circuito 2 (175 A) y el principal (175 A) se " +
            "calcularon con la lista completa de 240-6(a); esos tamaños ya no son de riel DIN.",
            AvisoRielDin(RielDinCon((2, 60000m)))); // 157 A

    [Fact]
    public void R06_VariosCircuitosYElPrincipal() =>
        Assert.Equal(
            "En riel DIN no hay interruptores de más de 125 A. Los circuitos 1 y 2 (175 A y 200 A) y el principal " +
            "(350 A) se calcularon con la lista completa de 240-6(a); esos tamaños ya no son de riel DIN.",
            AvisoRielDin(RielDinCon((1, 60000m), (2, 70000m)))); // 157 A y 184 A

    [Fact]
    public void R06_SoloElPrincipal() =>
        Assert.Equal(
            "En riel DIN no hay interruptores de más de 125 A. El principal (225 A) se calculó con la lista " +
            "completa de 240-6(a); ese tamaño ya no es de riel DIN.",
            AvisoRielDin(RielDinCon((1, 40000m), (2, 40000m)))); // 105 A cada uno: 125 A, sí es de riel DIN

    [Fact]
    public void R06_ConZapatasEsLaProteccionDelAlimentador()
    {
        var cuadro = RielDinCon((1, 40000m), (2, 40000m));
        cuadro.Datos.TipoAcometida = TipoAcometidaTablero.ZapatasPrincipales;
        cuadro.Recalcular();

        Assert.StartsWith(
            "En riel DIN no hay interruptores de más de 125 A. La protección del alimentador (225 A) se calculó",
            AvisoRielDin(cuadro));
    }

    [Fact]
    public void R06_SinTamanosFueraDeRielDin_SinAviso() =>
        Assert.Null(AvisoRielDin(RielDinCon((1, 20000m))));

    // ---- El aislamiento decide la columna de la Tabla 310-15(b)(16) ------------------------------

    private static CircuitoDelCuadro VeinteAmperesContinuos(CuadroDeCarga cuadro)
    {
        var c = Espacio(cuadro, 1);
        c.Unidad = UnidadConsumo.Amperes;
        c.Continua = 20m;
        c.LongitudM = 5m;
        return c;
    }

    [Fact]
    public void Aislamiento_PorOmisionEsThhnEnLugarSeco()
    {
        var cuadro = Nuevo();

        Assert.Equal("THHN", cuadro.Datos.TipoAislamiento);
        Assert.True(cuadro.Datos.LugarSeco);
    }

    [Fact]
    public void Aislamiento_ConFactoresUnThwLsPideMasCobreQueUnThhn()
    {
        // 26 A no continuos, 9 conductores agrupados (factor 0.7), protección de 30 A.
        // THHN: 10 AWG, 40 A a 90 °C × 0.7 = 28 A ≥ 26 A; 240-4(b) deja 30 A sobre 28 A.
        // THW-LS: 10 AWG, 35 A a 75 °C × 0.7 = 24.5 A < 26 A → 8 AWG. Fijo en THHN, el programa
        // imprimía 10 AWG para una instalación en THW-LS que necesita 8.
        //
        // (Hasta el punto 3 de la auditoría el caso era 20 A continuos; con las dos revisiones de
        // 210-19(a)(1) ese caso ya cumple con 10 AWG en THW-LS —30 A de tabla ≥ 25 A y 24.5 A ≥
        // 20 A—, así que dejó de demostrar nada.)
        var cuadro = Nuevo();
        cuadro.Datos.ConductoresAgrupados = 9;
        var c = Espacio(cuadro, 1);
        c.Unidad = UnidadConsumo.Amperes;
        c.NoContinua = 26m;
        c.LongitudM = 5m;

        cuadro.Recalcular();
        Assert.Equal("10", c.Resultado!.CalibreFase.Designacion);

        cuadro.Datos.TipoAislamiento = "THW-LS";
        cuadro.Recalcular();
        Assert.Equal("8", c.Resultado!.CalibreFase.Designacion);

        cuadro.Datos.TipoAislamiento = "TW";
        cuadro.Recalcular();
        Assert.Equal("8", c.Resultado!.CalibreFase.Designacion);
    }

    [Fact]
    public void Aislamiento_ThhnNoSePermiteEnLugarMojado_YSeDiceEnElRenglon()
    {
        // Tabla 310-104(a): THHN es «lugares secos».
        var cuadro = Nuevo();
        var c = VeinteAmperesContinuos(cuadro);
        cuadro.Datos.LugarSeco = false;
        cuadro.Recalcular();

        Assert.Null(c.Resultado);
        Assert.Contains("THHN", c.Error);

        cuadro.Datos.TipoAislamiento = "THW-LS";
        cuadro.Recalcular();
        Assert.NotNull(c.Resultado);
    }

    [Fact]
    public void Aislamiento_ElCasoBaseNoCambia()
    {
        // A 30 °C y 3 agrupados los factores valen 1 y la terminal limita a 60 °C: el aislamiento
        // no mueve el calibre. Los tres aparatos siguen igual en THW-LS.
        var cuadro = TresAparatos();
        var antes = cuadro.Circuitos.Where(c => c.Resultado is not null).Select(c => c.Resultado!.CalibreFase.Designacion).ToList();

        cuadro.Datos.TipoAislamiento = "THW-LS";
        cuadro.Recalcular();

        Assert.Equal(antes, cuadro.Circuitos.Where(c => c.Resultado is not null).Select(c => c.Resultado!.CalibreFase.Designacion).ToList());
    }

    // ---- 210-19(a)(1): el 125 % antes de factores, la carga al 100 % después ---------------------

    [Fact]
    public void DosRevisiones_El125PorCientoNoSeMultiplicaConLosFactores()
    {
        // 32 A continuos de alumbrado, 9 agrupados (0.7), THHN. 8 AWG:
        //   antes de factores: 40 A de tabla a 60 °C ≥ 40 A (125 % × 32) ✔
        //   con factores: min(55 × 0.7, 40) = 38.5 A ≥ 32 A ✔
        //   240-4(b): 38.5 A no es estándar, el inmediato superior es 40 A ✔
        // Antes el programa exigía 38.5 ≥ 40 y daba 6 AWG.
        var cuadro = Nuevo();
        cuadro.Datos.ConductoresAgrupados = 9;
        var c = Espacio(cuadro, 1);
        c.Unidad = UnidadConsumo.Amperes;
        c.Continua = 32m;
        c.LongitudM = 5m;
        cuadro.Recalcular();

        Assert.Equal(40m, c.Resultado!.ProteccionA);
        Assert.Equal("8", c.Resultado.CalibreFase.Designacion);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "240-4(b)");

        var conductor = cuadro.Desglose(c)!.Conductor;
        Assert.Contains("Antes de factores: 40.00 A a 60 °C ≥ capacidad mínima 40.00 A ✔ — 210-19(a)(1)", conductor);
        Assert.Contains("Con factores: 38.50 A ≥ carga 32.00 A ✔ — 210-19(a)(1)", conductor);
    }

    [Fact]
    public void DosRevisiones_SinFactoresDaLoMismoQueAntes()
    {
        // 30 °C y 3 agrupados: los factores valen 1 y las dos formas coinciden. 32 A continuos → 8 AWG.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Unidad = UnidadConsumo.Amperes;
        c.Continua = 32m;
        c.LongitudM = 5m;
        cuadro.Recalcular();

        Assert.Equal("8", c.Resultado!.CalibreFase.Designacion);
    }

    // ---- 110-14(c)(1)a.(3): terminales marcadas 75 °C ----------------------------------------------

    private static CircuitoDelCuadro CuarentaYCincoAmperes(CuadroDeCarga cuadro)
    {
        var c = Espacio(cuadro, 1);
        c.Tipo = TipoCarga.Equipo;
        c.Unidad = UnidadConsumo.Amperes;
        c.NoContinua = 45m;
        c.LongitudM = 5m;
        return c;
    }

    [Fact]
    public void Terminales_Marcadas75CUsanLaColumnaDe75()
    {
        // 45 A no continuos, protección de 45 A. A 60 °C el 8 AWG da 40 A < 45 → 6 AWG. Con el
        // equipo marcado 75 °C el 8 AWG da 50 A ≥ 45 → 8 AWG.
        var cuadro = Nuevo();
        var c = CuarentaYCincoAmperes(cuadro);
        cuadro.Recalcular();
        Assert.False(cuadro.Datos.TerminalesMarcadas75C);
        Assert.Equal("6", c.Resultado!.CalibreFase.Designacion);

        cuadro.Datos.TerminalesMarcadas75C = true;
        cuadro.Recalcular();
        Assert.Equal("8", c.Resultado!.CalibreFase.Designacion);
        Assert.Equal(75, c.Resultado.Detalle!.TemperaturaTerminalesC);
        Assert.Contains(cuadro.Desglose(c)!.Conductor, l => l.Contains("110-14(c)(1)a.(3)"));
    }

    [Fact]
    public void Terminales_Marcadas75CConConductorTwSeQuedaEn60_SinRechazarlo()
    {
        // 110-14(c)(1)a.(1): un conductor de 60 °C siempre vale en terminales de 100 A o menos; la
        // columna que manda es la más baja. No debe tronar por «no alcanza los 75 °C».
        var cuadro = Nuevo();
        var c = CuarentaYCincoAmperes(cuadro);
        cuadro.Datos.TerminalesMarcadas75C = true;
        cuadro.Datos.TipoAislamiento = "TW";
        cuadro.Recalcular();

        Assert.Null(c.Error);
        Assert.Equal(60, c.Resultado!.Detalle!.TemperaturaTerminalesC);
        Assert.Equal("6", c.Resultado.CalibreFase.Designacion);
    }

    // ---- 240-4(b) en Equipo, no solo en Alumbrado ----------------------------------------------

    [Fact]
    public void Excepcion240_4b_AplicaEnEquipo_NoEnContactos()
    {
        // 32 A continuos, 9 agrupados: el 8 AWG da 38.5 A con factores y la protección es de 40 A.
        // 240-4(b)(1) solo excluye circuitos de varios contactos: Equipo califica → 8 AWG. Contactos
        // no se sabe cuántos lleva → sube a 6 AWG.
        foreach (var (tipo, calibre) in new[] { (TipoCarga.Equipo, "8"), (TipoCarga.Contactos, "6") })
        {
            var cuadro = Nuevo();
            cuadro.Datos.ConductoresAgrupados = 9;
            var c = Espacio(cuadro, 1);
            c.Tipo = tipo;
            c.Unidad = UnidadConsumo.Amperes;
            c.Continua = 32m;
            c.LongitudM = 5m;
            cuadro.Recalcular();

            Assert.Equal(calibre, c.Resultado!.CalibreFase.Designacion);
        }
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

        Espacio(cuadro, 2).Continua = 3000m;
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 2), 3)); // 2-4-6, con carga

        Assert.Equal(3, cuadro.EspaciosOcupados);
        Assert.Equal(9, cuadro.EspaciosLibres);
    }
}
