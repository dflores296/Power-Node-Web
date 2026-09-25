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
        return EnPvc.Todo(cuadro);
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
    public void SinMinimoPorTipo_ContactosYAlumbradoConLaMismaCargaSalenIguales()
    {
        // Hasta el 2026-09-24 contactos salía en 20 A por un mínimo que la NOM no pide (el MAX(20, ...)
        // del Excel). Ya no: la protección sale de la carga y 240-6(a).
        var cuadro = Nuevo();
        var alumbrado = Espacio(cuadro, 1);
        alumbrado.NoContinua = 500m;
        alumbrado.LongitudM = 10m;

        var contactos = Espacio(cuadro, 3);
        contactos.Tipo = TipoCarga.Contactos;
        contactos.NoContinua = 500m;
        contactos.LongitudM = 10m;

        cuadro.Recalcular();

        foreach (var c in new[] { alumbrado, contactos })
        {
            Assert.Equal(15m, c.Resultado!.ProteccionA);
            Assert.Equal("14", c.Resultado.CalibreFase.Designacion);
        }
        Assert.DoesNotContain(contactos.Resultado!.Citas, c => c.Descripcion.Contains("piso"));
    }

    // ---- 210-11(c) y 220-52 · Contactos de vivienda por uso ------------------------------------------

    [Theory]
    [InlineData(UsoDeContactos.AparatosPequenos, "210-11(c)(1)")]
    [InlineData(UsoDeContactos.Lavadora, "210-11(c)(2)")]
    [InlineData(UsoDeContactos.Bano, "210-11(c)(3)")]
    public void Vivienda_ElUsoPide20AConSuCita(UsoDeContactos uso, string referencia)
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar; // 210-11(c) y 220-52 son de vivienda — I-46
        var c = Espacio(cuadro, 1);
        c.Tipo = TipoCarga.Contactos;
        c.Uso = uso;
        c.NoContinua = 500m;
        cuadro.Recalcular();

        Assert.Equal(20m, c.Resultado!.ProteccionA);
        Assert.Equal("12", c.Resultado.CalibreFase.Designacion); // 240-4(d): 20 A pide 12 AWG
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == referencia);
        Assert.Contains($"Protección mínima del circuito: 20 A — {referencia}", cuadro.Desglose(c)!.Proteccion);
    }

    [Theory]
    [InlineData(TipoDeInmueble.Otro)]
    [InlineData(TipoDeInmueble.Restaurante)]
    [InlineData(TipoDeInmueble.ViviendaPopular)] // 210-11(c) Excepción 1; 220-52, excepción
    public void I46_FueraDeViviendaElUsoNoCuenta(TipoDeInmueble inmueble)
    {
        // La cocina de David (2026-09-24): inmueble «Otro» y contactos con uso «Cocina». 210-11(c) y
        // 220-52 son de unidades de vivienda: ni 20 A, ni 1500 VA en el alimentador, ni aviso.
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = inmueble;
        var c = Espacio(cuadro, 1);
        c.Tipo = TipoCarga.Contactos;
        c.Uso = UsoDeContactos.AparatosPequenos; // se queda capturado, pero no aplica
        c.NoContinua = 500m;
        cuadro.Recalcular();

        Assert.Equal(UsoDeContactos.General, c.UsoEfectivo);
        Assert.Equal(15m, c.Resultado!.ProteccionA);
        Assert.Equal(0m, c.Ajuste220_52VA);
        Assert.Equal(0m, cuadro.Resumen.Minimo220_52VA);
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("aparatos pequeños"));

        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        cuadro.Recalcular();
        Assert.Equal(20m, c.Resultado!.ProteccionA);
        Assert.Equal(1000m, c.Ajuste220_52VA);
    }

    [Fact]
    public void I47_ElFpDelAlimentadorEsElDeLaCorrienteDeLaCaida()
    {
        // 1F-2H: equipo de 1000 VA a F.P. 1 y contactos de 1000 VA a 0.6 con F.D. 0.5. La caída usa
        // la corriente con demanda: 1000 + 500∠−53.13° → 1300 W, 400 var → F.P. 0.9558. Con la
        // carga instalada daba 0.8944, y la pantalla mostraba ese.
        var cuadro = Nuevo(espacios: 6, fases: 1, hilos: 2, tension: 127m);
        var equipo = Espacio(cuadro, 1);
        equipo.Tipo = TipoCarga.Equipo;
        equipo.NoContinua = 1000m;
        equipo.FactorPotencia = 1m;
        var contactos = Espacio(cuadro, 3);
        contactos.Tipo = TipoCarga.Contactos;
        contactos.NoContinua = 1000m;
        contactos.FactorPotencia = 0.6m;
        cuadro.Datos.CambiarFactorDeDemanda(CategoriaDeCarga.Contactos, 0.5m);
        cuadro.Recalcular();

        Assert.Equal(0.9558m, cuadro.Alimentador.FactorPotencia);
        var fase = cuadro.Alimentador.Resultado!.CaidaPorFase!.Single();
        Assert.Equal(0.9558, Math.Cos((double)fase.Corriente.AnguloGrados * Math.PI / 180.0), 3);
    }

    [Fact]
    public void Vivienda_ElUsoSoloCuentaEnContactos()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Tipo = TipoCarga.Equipo;
        c.Uso = UsoDeContactos.AparatosPequenos; // se queda capturado, pero no aplica
        c.NoContinua = 500m;
        cuadro.Recalcular();

        Assert.Equal(15m, c.Resultado!.ProteccionA);
        Assert.Equal(0m, c.Ajuste220_52VA);
    }

    [Fact]
    public void Vivienda_AparatosPequenosYLavadoraCuentan1500VAEnElAlimentador()
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar; // 210-11(c) y 220-52 son de vivienda — I-46
        foreach (var (espacio, uso, va) in new[]
                 {
                     (1, UsoDeContactos.AparatosPequenos, 500m),  // + 1000 VA
                     (3, UsoDeContactos.Lavadora, 1800m),         // ya pasa de 1500: + 0
                     (5, UsoDeContactos.Bano, 300m),              // baño: sin mínimo de carga
                 })
        {
            var c = Espacio(cuadro, espacio);
            c.Tipo = TipoCarga.Contactos;
            c.Uso = uso;
            c.NoContinua = va;
        }
        cuadro.Recalcular();

        Assert.Equal(1000m, Espacio(cuadro, 1).Ajuste220_52VA);
        Assert.Equal(0m, Espacio(cuadro, 3).Ajuste220_52VA);
        Assert.Equal(0m, Espacio(cuadro, 5).Ajuste220_52VA);
        Assert.Equal(2600m, cuadro.Resumen.InstaladaVA);   // lo capturado, renglón por renglón
        Assert.Equal(1000m, cuadro.Resumen.Minimo220_52VA);
        Assert.Equal(3600m, cuadro.Resumen.CalculadaVA);
        // Fase A (espacio 1): 1500 VA / 127 V = 11.81 A. Fase B, la lavadora: 1800 / 127 = 14.17 A.
        Assert.Equal(11.81m, cuadro.Alimentador.Fases!.Single(f => f.Fase == 'A').TotalA, 2);
        Assert.Equal('B', cuadro.Alimentador.Gobierna!.Fase);
        // El derivado NO cambia por 220-52: se calcula con sus 500 VA.
        Assert.Equal(3.94m, Espacio(cuadro, 1).Resultado!.CorrienteDisenoA, 2);
    }

    [Fact]
    public void Vivienda_UnSoloCircuitoDeAparatosPequenosSeAvisa_DosNo()
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar; // 210-11(c) y 220-52 son de vivienda — I-46
        var cocina = Espacio(cuadro, 1);
        cocina.Tipo = TipoCarga.Contactos;
        cocina.Uso = UsoDeContactos.AparatosPequenos;
        cocina.NoContinua = 800m;
        cuadro.Recalcular();

        Assert.Contains(cuadro.Alimentador.Avisos, a =>
            a.StartsWith("Solo el circuito 1 es de aparatos pequeños.") && a.Contains("210-11(c)(1)"));

        var comedor = Espacio(cuadro, 3);
        comedor.Tipo = TipoCarga.Contactos;
        comedor.Uso = UsoDeContactos.AparatosPequenos;
        comedor.NoContinua = 600m;
        cuadro.Recalcular();

        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("aparatos pequeños"));
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

        cuadro.Datos.FactorDemandaAlumbrado = 0.5m; // los seis son de alumbrado, el tipo por omisión
        cuadro.Recalcular();

        Assert.Equal(sinDemanda / 2m, cuadro.Alimentador.Resultado!.CorrienteDisenoA, 2);
        var alumbrado = cuadro.Resumen.PorCategoria!.Single(f => f.Categoria == CategoriaDeCarga.Alumbrado);
        Assert.Equal(18000m, alumbrado.InstaladaVA);
        Assert.Equal(9000m, alumbrado.DemandadaVA);
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

    // ---- R-16 · 240-4(b) en cada calibre, no solo en el de la carga ------------------------------------

    [Fact]
    public void R16_Con60AElAlimentadorSeQuedaEn6AWGPor240_4b()
    {
        // La carga pide 12 AWG; 230-79(d) sube la protección a 60 A. 10 AWG (30 A) y 8 AWG (40 A,
        // valor estándar) no quedan protegidos; 6 AWG (55 A) sí: 60 A es el estándar inmediato superior.
        var r = TresAparatosEnAcometida(TipoDeInmueble.Otro).Alimentador.Resultado!;

        Assert.Equal(60m, r.ProteccionA);
        Assert.Equal("6", r.CalibreFase.Designacion);
        var cita = Assert.Single(r.Citas, c => c.Referencia == "240-4(b)");
        Assert.StartsWith("12 (20 A) no cubre la protección de 60 A -- sube a 6 (55 A)", cita.Descripcion);
        Assert.DoesNotContain(r.Citas, c => c.Referencia == "240-4");
    }

    [Fact]
    public void R16_SiUnCalibreCubreLaProteccionSinExcepcion_NoSeCita240_4b()
    {
        // 30 A (vivienda popular): 10 AWG tiene 30 A a 60 °C, cubre directo.
        var r = TresAparatosEnAcometida(TipoDeInmueble.ViviendaPopular).Alimentador.Resultado!;

        Assert.Equal("10", r.CalibreFase.Designacion);
        Assert.DoesNotContain(r.Citas, c => c.Referencia == "240-4(b)");
        Assert.Contains(r.Citas, c => c.Referencia == "240-4" && c.Descripcion.Contains("sube a 10"));
    }

    // ---- R-12 · El factor de demanda lo decide el proyectista, con justificación --------------------------

    private const string AvisoSinJustificacion = "El factor de demanda de equipo (aparatos) es menor que 1 y no tiene justificación.";

    [Fact]
    public void R12_SinReduccionNoSePideJustificacion()
    {
        var cuadro = TresAparatos();

        Assert.False(cuadro.Datos.ReduceCargaPorDemanda);
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.StartsWith(AvisoSinJustificacion));
    }

    [Fact]
    public void R12_ConReduccionSinJustificacion_SeAvisa()
    {
        var cuadro = TresAparatos(); // tres circuitos de equipo
        cuadro.Datos.FactorDemandaEquipo = 0.5m;
        cuadro.Recalcular();

        Assert.Contains(cuadro.Alimentador.Avisos, a => a.StartsWith(AvisoSinJustificacion));
    }

    [Fact]
    public void R12_VariasJustificaciones_EnElOrdenDeLaLista()
    {
        var cuadro = TresAparatos();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar; // 220-53 es de vivienda (R-19)
        cuadro.Datos.FactorDemandaEquipo = 0.8m;
        cuadro.Datos.Justificaciones[CategoriaDeCarga.Equipo].Add(JustificacionFactorDemanda.CargasNoCoincidentes);
        cuadro.Datos.Justificaciones[CategoriaDeCarga.Equipo].Add(JustificacionFactorDemanda.AparatosFijosVivienda);
        cuadro.Recalcular();

        Assert.Equal(
            "220-53 — cuatro o más aparatos fijos en vivienda, 75 %; 220-60 — cargas no coincidentes",
            cuadro.Datos.JustificacionDe(CategoriaDeCarga.Equipo));
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.StartsWith(AvisoSinJustificacion));
    }

    [Fact]
    public void R12_OtraSinTextoNoJustifica_ConTextoSi()
    {
        var cuadro = TresAparatos();
        cuadro.Datos.FactorDemandaEquipo = 0.7m;
        cuadro.Datos.Justificaciones[CategoriaDeCarga.Equipo].Add(JustificacionFactorDemanda.Otra);
        cuadro.Recalcular();
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.StartsWith(AvisoSinJustificacion));

        cuadro.Datos.JustificacionOtra[CategoriaDeCarga.Equipo] = "Registro de demanda de la planta existente";
        cuadro.Recalcular();
        Assert.Equal("Criterio del proyectista: Registro de demanda de la planta existente", cuadro.Datos.JustificacionDe(CategoriaDeCarga.Equipo));
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.StartsWith(AvisoSinJustificacion));
    }

    // ---- R-17 · Factor de demanda por tipo de carga ---------------------------------------------------

    [Fact]
    public void R17_CadaTipoConSuFactor_MotoresYCalefaccionEn1PorOmision()
    {
        var cuadro = Nuevo(espacios: 12);
        foreach (var (espacio, categoria) in new[]
                 {
                     (1, CategoriaDeCarga.Alumbrado), (2, CategoriaDeCarga.Contactos), (3, CategoriaDeCarga.Equipo),
                     (4, CategoriaDeCarga.MotorOAireAcondicionado), (5, CategoriaDeCarga.CalefaccionFija),
                 })
        {
            Espacio(cuadro, espacio).Categoria = categoria;
            Espacio(cuadro, espacio).NoContinua = 1000m;
        }
        cuadro.Datos.FactorDemandaAlumbrado = 0.5m;
        cuadro.Datos.FactorDemandaContactos = 0.6m;
        cuadro.Datos.FactorDemandaEquipo = 0.75m;
        cuadro.Recalcular();

        decimal Demandada(CategoriaDeCarga c) => cuadro.Resumen.PorCategoria!.Single(f => f.Categoria == c).DemandadaVA;
        Assert.Equal(500m, Demandada(CategoriaDeCarga.Alumbrado));
        Assert.Equal(600m, Demandada(CategoriaDeCarga.Contactos));
        Assert.Equal(750m, Demandada(CategoriaDeCarga.Equipo));
        Assert.Equal(1000m, Demandada(CategoriaDeCarga.MotorOAireAcondicionado)); // 1.0 por omisión
        Assert.Equal(1000m, Demandada(CategoriaDeCarga.CalefaccionFija));
        Assert.Equal(3850m, cuadro.Resumen.DemandadaVA);
    }

    // ---- R-18 · Motores y calefacción también admiten F.D. (430-26, 220-51 Exc.); calefacción, continua

    [Fact]
    public void R18_MotoresYCalefaccionSeReducenConSuJustificacion()
    {
        var cuadro = Nuevo(espacios: 6);
        Espacio(cuadro, 1).Categoria = CategoriaDeCarga.MotorOAireAcondicionado;
        Espacio(cuadro, 1).NoContinua = 1000m;
        Espacio(cuadro, 3).Categoria = CategoriaDeCarga.CalefaccionFija;
        Espacio(cuadro, 3).Continua = 1000m;
        cuadro.Datos.FactorDemandaMotores = 0.7m;
        cuadro.Datos.FactorDemandaCalefaccion = 0.8m;
        cuadro.Recalcular();

        decimal Demandada(CategoriaDeCarga c) => cuadro.Resumen.PorCategoria!.Single(f => f.Categoria == c).DemandadaVA;
        Assert.Equal(700m, Demandada(CategoriaDeCarga.MotorOAireAcondicionado));
        Assert.Equal(800m, Demandada(CategoriaDeCarga.CalefaccionFija));
        Assert.Equal(2, cuadro.Alimentador.Avisos.Count(a => a.Contains("no tiene justificación")));

        Assert.Equal(
            [JustificacionFactorDemanda.MotoresNoSimultaneos, JustificacionFactorDemanda.CargasNoCoincidentes, JustificacionFactorDemanda.Otra],
            CategoriaDeCarga.MotorOAireAcondicionado.JustificacionesPosibles(TipoDeInmueble.Otro));
        Assert.Equal(
            [JustificacionFactorDemanda.CalefaccionPorCiclos, JustificacionFactorDemanda.CargasNoCoincidentes, JustificacionFactorDemanda.Otra],
            CategoriaDeCarga.CalefaccionFija.JustificacionesPosibles(TipoDeInmueble.Otro));

        cuadro.Datos.Justificaciones[CategoriaDeCarga.MotorOAireAcondicionado].Add(JustificacionFactorDemanda.MotoresNoSimultaneos);
        cuadro.Datos.Justificaciones[CategoriaDeCarga.CalefaccionFija].Add(JustificacionFactorDemanda.CalefaccionPorCiclos);
        cuadro.Recalcular();
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("no tiene justificación"));
    }

    [Fact]
    public void R18_LaCalefaccionPasaSolaACargaContinua()
    {
        // 424-3(b): lo capturado como no continua se mueve a continua, y entra al 125 %.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.CalefaccionFija;
        c.NoContinua = 1270m; // 10 A
        cuadro.Recalcular();

        Assert.Equal(1270m, c.Continua);
        Assert.Equal(0m, c.NoContinua);
        Assert.Equal(1270m, c.ContinuaVA);
        Assert.Equal(12.5m, c.Resultado!.Detalle!.CapacidadMinimaA, 2); // 125 % × 10 A
    }

    [Fact]
    public void R18_ElTooltipDelTipoTraeEjemplos()
    {
        Assert.Contains("inverter frío/calor", CategoriaDeCarga.MotorOAireAcondicionado.Descripcion());
        Assert.Contains("calderas eléctricas", CategoriaDeCarga.CalefaccionFija.Descripcion());
    }

    // ---- I-35 · Desglose de aparatos por circuito --------------------------------------------------------

    [Fact]
    public void I35_LaCargaDelCircuitoEsLaSumaDeSusAparatos()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var estufa = c.AgregarAparato();
        estufa.Descripcion = "Estufa";
        estufa.Unidad = UnidadConsumo.Watts;
        estufa.CargaUnitaria = 900m;
        estufa.FactorPotencia = 1m;
        var refri = c.AgregarAparato();
        refri.Descripcion = "Refrigerador";
        refri.CargaUnitaria = 750m;
        refri.Continua = true;
        refri.FactorPotencia = 0.8m;
        var focos = c.AgregarAparato();
        focos.Descripcion = "Focos";
        focos.Cantidad = 2;
        focos.CargaUnitaria = 100m;
        cuadro.Recalcular();

        Assert.Equal(900m, estufa.TotalVA);
        Assert.Equal(200m, focos.TotalVA);        // 2 × 100 VA
        Assert.Equal(UnidadConsumo.VoltAmperes, c.Unidad);
        Assert.Equal(750m, c.ContinuaVA);
        Assert.Equal(1100m, c.NoContinuaVA);
        // P = 900 + 600 + 180 = 1680 W; Q = 450 + 87.2 = 537.2 VAR → 0.9525
        Assert.Equal(0.95m, c.FactorPotencia, 2);
        Assert.NotNull(c.Resultado);
    }

    [Fact]
    public void I35_AlDesglosarSeConservaLoCapturado()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Descripcion = "Cocina";
        c.Continua = 500m;
        c.NoContinua = 800m;
        c.AgregarAparato();
        cuadro.Recalcular();

        Assert.Equal(3, c.Aparatos.Count); // la continua, la no continua y el nuevo
        Assert.Equal(500m, c.ContinuaVA);
        Assert.Equal(800m, c.NoContinuaVA);
    }

    [Fact]
    public void I35_UnContactoSinCargaToma180VA()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Contactos;
        var contactos = c.AgregarAparato();
        contactos.Descripcion = "Contacto doble";
        contactos.Cantidad = 5;
        cuadro.Recalcular();

        Assert.Equal(180m, contactos.CargaUnitaria); // 220-14(i)
        Assert.Equal(900m, c.NoContinuaVA);
    }

    [Fact]
    public void I35_EnCalefaccionTodosLosAparatosSonContinuos()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.CalefaccionFija;
        c.AgregarAparato().CargaUnitaria = 1000m;
        cuadro.Recalcular();

        Assert.True(c.Aparatos.Single().Continua);
        Assert.Equal(1000m, c.ContinuaVA);
        Assert.Equal(0m, c.NoContinuaVA);
    }

    [Fact]
    public void I35_EnUnDosPolosLosAmperesSeConviertenConLaTensionFaseFase()
    {
        var cuadro = Nuevo();
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 1), 2));
        var c = Espacio(cuadro, 1);
        var a = c.AgregarAparato();
        a.Unidad = UnidadConsumo.Amperes;
        a.CargaUnitaria = 10m;
        cuadro.Recalcular();

        Assert.Equal(2200m, a.TotalVA); // 10 A × 220 V
        Assert.Equal(10m, c.Resultado!.CorrienteDisenoA, 2);
    }

    [Fact]
    public void I35_SinAparatosElCircuitoVuelveASerEditable_ConLaUltimaSuma()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        var a = c.AgregarAparato();
        a.CargaUnitaria = 600m;
        cuadro.Recalcular();
        c.Aparatos.Remove(a);
        cuadro.Recalcular();

        Assert.False(c.TieneDesglose);
        Assert.Equal(600m, c.NoContinua);
    }

    // ---- R-19 · Un solo inmueble para 230-79 y para el factor de demanda ------------------------------

    [Theory]
    [InlineData(TipoDeInmueble.ViviendaUnifamiliar, null, "unidades de vivienda", true)]
    [InlineData(TipoDeInmueble.ViviendaPopular, 30, "unidades de vivienda", true)]
    [InlineData(TipoDeInmueble.ViviendaMultifamiliar, 60, "unidades de vivienda", true)]
    [InlineData(TipoDeInmueble.Hospital, 60, "hospitales", false)]
    [InlineData(TipoDeInmueble.HotelOMotel, 60, "hoteles y moteles", false)]
    [InlineData(TipoDeInmueble.Almacen, 60, "almacenes", false)]
    [InlineData(TipoDeInmueble.Escuela, 60, null, false)]
    [InlineData(TipoDeInmueble.Restaurante, 60, null, false)]
    [InlineData(TipoDeInmueble.Otro, 60, null, false)]
    public void R19_CadaInmuebleTieneUnaSolaRespuestaEnCadaCriterio(TipoDeInmueble inmueble, int? minimo230_79, string? fila220_42, bool vivienda)
    {
        Assert.Equal(minimo230_79, (int?)inmueble.Minimo230_79()?.Amperes);
        Assert.Equal(fila220_42, inmueble.FilaTabla220_42());
        Assert.Equal(vivienda, inmueble.EsVivienda());
    }

    [Fact]
    public void R19_LasJustificacionesSeFiltranPorInmueble()
    {
        var enVivienda = CategoriaDeCarga.Equipo.JustificacionesPosibles(TipoDeInmueble.ViviendaUnifamiliar);
        Assert.Contains(JustificacionFactorDemanda.AparatosFijosVivienda, enVivienda);
        Assert.DoesNotContain(JustificacionFactorDemanda.CocinaComercial, enVivienda);

        var enRestaurante = CategoriaDeCarga.Equipo.JustificacionesPosibles(TipoDeInmueble.Restaurante);
        Assert.Contains(JustificacionFactorDemanda.CocinaComercial, enRestaurante);
        Assert.Contains(JustificacionFactorDemanda.RestauranteNuevo, enRestaurante);
        Assert.DoesNotContain(JustificacionFactorDemanda.AparatosFijosVivienda, enRestaurante);

        Assert.DoesNotContain(JustificacionFactorDemanda.ContactosNoVivienda, CategoriaDeCarga.Contactos.JustificacionesPosibles(TipoDeInmueble.ViviendaUnifamiliar));
        Assert.Contains(JustificacionFactorDemanda.ContactosNoVivienda, CategoriaDeCarga.Contactos.JustificacionesPosibles(TipoDeInmueble.Otro));

        // «Todos los demás» va al 100 % en la Tabla 220-42: no justifica reducir el alumbrado.
        Assert.DoesNotContain(JustificacionFactorDemanda.AlumbradoGeneral, CategoriaDeCarga.Alumbrado.JustificacionesPosibles(TipoDeInmueble.Otro));
        Assert.Contains(JustificacionFactorDemanda.AlumbradoGeneral, CategoriaDeCarga.Alumbrado.JustificacionesPosibles(TipoDeInmueble.Hospital));
    }

    [Fact]
    public void R19_LaTabla220_42DiceSuRenglon_YUnaJustificacionQueYaNoAplicaSeDescarta()
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.HotelOMotel;
        cuadro.Datos.FactorDemandaAlumbrado = 0.5m;
        cuadro.Datos.Justificaciones[CategoriaDeCarga.Alumbrado].Add(JustificacionFactorDemanda.AlumbradoGeneral);
        Assert.Equal("Tabla 220-42 — alumbrado general (hoteles y moteles)", cuadro.Datos.JustificacionDe(CategoriaDeCarga.Alumbrado));

        cuadro.Datos.Inmueble = TipoDeInmueble.Otro; // «todos los demás»: la 220-42 ya no justifica
        Assert.Null(cuadro.Datos.JustificacionDe(CategoriaDeCarga.Alumbrado));
    }

    [Fact]
    public void R19_ElInmuebleSeCapturaAunqueNoSeaEquipoDeAcometida()
    {
        var cuadro = TresAparatos();
        cuadro.Datos.Inmueble = TipoDeInmueble.Hospital;
        cuadro.Recalcular();

        Assert.Null(cuadro.Datos.Minimo230_79);
        Assert.Equal(20m, cuadro.InterruptorPrincipalA);

        cuadro.Datos.EsEquipoDeAcometida = true;
        cuadro.Recalcular();
        Assert.Equal(60m, cuadro.InterruptorPrincipalA);
    }

    [Fact]
    public void R17_ElTipoDelMotorDeMotorYCalefaccionEsEquipo()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.MotorOAireAcondicionado;
        Assert.Equal(TipoCarga.Equipo, c.Tipo);

        c.Tipo = TipoCarga.Contactos;
        Assert.Equal(CategoriaDeCarga.Contactos, c.Categoria);
    }

    [Fact]
    public void R17_ElFactorSeAplicaEnLaCorrienteDelAlimentador_NoEnElDerivado()
    {
        var cuadro = TresAparatos(); // tres circuitos de equipo
        var derivado = Espacio(cuadro, 5).Resultado!.CorrienteDisenoA;
        cuadro.Datos.FactorDemandaEquipo = 0.5m;
        cuadro.Recalcular();

        Assert.Equal(derivado, Espacio(cuadro, 5).Resultado!.CorrienteDisenoA); // 220-42: no en el derivado
        Assert.Equal(6.10m, cuadro.Alimentador.Resultado!.CorrienteDisenoA, 2);  // 0.5 × 12.20 A
    }

    // ---- R-11 · Mínimo del principal por 230-79, solo si el tablero es el de la acometida ----------------

    private static CuadroDeCarga TresAparatosEnAcometida(TipoDeInmueble inmueble)
    {
        var cuadro = TresAparatos();
        cuadro.Datos.EsEquipoDeAcometida = true;
        cuadro.Datos.Inmueble = inmueble;
        cuadro.Recalcular();
        return cuadro;
    }

    [Fact]
    public void R11_SinSerEquipoDeAcometida_ElPrincipalEsElCalculado()
    {
        var cuadro = TresAparatos();

        Assert.False(cuadro.Datos.EsEquipoDeAcometida);
        Assert.Null(cuadro.Datos.Minimo230_79);
        Assert.Equal(20m, cuadro.InterruptorPrincipalA);
    }

    [Fact]
    public void R11_OtroInmueble_ElPrincipalSubeA60AYElConductorLoSigue()
    {
        var cuadro = TresAparatosEnAcometida(TipoDeInmueble.Otro);
        var r = cuadro.Alimentador.Resultado!;

        Assert.Equal(60m, r.ProteccionA);
        Assert.Contains(r.Citas, c => c.Referencia == "230-79(d)" && c.Descripcion.Contains("Por carga salía 20 A -> 60 A"));
        // 240-4: el conductor se protege con 60 A. 6 AWG: 55 A a 60 °C, y 240-4(b) permite 60 A (R-16).
        Assert.Equal("6", r.CalibreFase.Designacion);
        Assert.Contains("Protección mínima del circuito: 60 A — 230-79(d)", cuadro.DesgloseDelAlimentador()!.Proteccion);
    }

    [Fact]
    public void R11_ViviendaPopular_30A()
    {
        var r = TresAparatosEnAcometida(TipoDeInmueble.ViviendaPopular).Alimentador.Resultado!;

        Assert.Equal(30m, r.ProteccionA);
        Assert.Contains(r.Citas, c => c.Referencia == "230-79(c)");
    }

    [Fact]
    public void R11_ViviendaUnifamiliar_SinNumeroFijo_ManDaLaCarga() =>
        Assert.Equal(20m, TresAparatosEnAcometida(TipoDeInmueble.ViviendaUnifamiliar).InterruptorPrincipalA);

    [Fact]
    public void R11_SiLaCargaYaPasaDelMinimo_NoCambiaNada()
    {
        var cuadro = Nuevo(espacios: 6);
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 1), 3));
        Espacio(cuadro, 1).NoContinua = 30000m; // 78.7 A → 80 A
        cuadro.Datos.EsEquipoDeAcometida = true;
        cuadro.Recalcular();

        Assert.Equal(80m, cuadro.InterruptorPrincipalA);
        Assert.DoesNotContain(cuadro.Alimentador.Resultado!.Citas, c => c.Referencia.StartsWith("230-79"));
    }

    [Fact]
    public void NingunAvisoLeHablaAlUsuarioDelExcel()
    {
        // El caso de los tres aparatos dispara el aviso de «igual que el derivado más grande»
        // (principal y air fryer en 20 A).
        var cuadro = TresAparatos();

        Assert.Contains(cuadro.Alimentador.Avisos, a => a.StartsWith("El interruptor principal quedó igual que el derivado más grande (20 A)"));
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

    // ---- R-09 · 2F-3H: el neutro lleva ≈ la corriente de fase — 310-15(b)(5)(2) -----------------------

    [Fact]
    public void R09_Alimentador2F3HBalanceado_ElNeutroEsDelCalibreDeLaFase()
    {
        // 2 fases de una estrella 220Y/127, 1270 VA en cada fase: 10 A por fase. El neutro lleva la
        // suma fasorial de dos corrientes iguales a 120°, que es otra vez 10 A.
        var cuadro = Nuevo(espacios: 6, fases: 2, hilos: 3);
        foreach (var espacio in new[] { 1, 3 })
            Espacio(cuadro, espacio).NoContinua = 1270m;
        cuadro.Recalcular();

        Assert.Equal("AB", string.Concat(cuadro.Datos.Barras));
        Assert.Equal(127.02m, cuadro.Datos.TensionFaseNeutroV, 2);
        var a = cuadro.Alimentador;
        Assert.Equal(2, a.Polos);
        Assert.Equal(10.00m, a.Resultado!.CorrienteDisenoA, 2);
        Assert.Equal(a.Resultado.CalibreFase, a.Resultado.CalibreNeutro);
    }

    // ---- R-10 · Candados de 2F-3H 220Y/127: ni 220-61(a) excepción ni 310-15(b)(7) --------------------

    /// <summary>
    /// 2F-3H 220Y/127 con 99.99 A no continuos por fase y terminales marcadas 75 °C: 100 A de principal
    /// y 3 AWG (100 A a 75 °C, Tabla 310-15(b)(16)). Es el caso en que las dos reglas equivocadas sí
    /// cambiarían el cobre: el neutro × 140 % pediría 140 A (1/0 AWG) y la Tabla 310-15(b)(7) daría
    /// 4 AWG para 100 A.
    /// </summary>
    private static ResultadoAlimentador AlimentadorDe100APorFase2F3H()
    {
        var cuadro = Nuevo(espacios: 6, fases: 2, hilos: 3);
        cuadro.Datos.TerminalesMarcadas75C = true;
        cuadro.Datos.LongitudAlimentadorM = 5m; // que la caída no suba el calibre: la prueba es de ampacidad
        foreach (var espacio in new[] { 1, 3 })
            Espacio(cuadro, espacio).NoContinua = 12700m; // 99.99 A a 127.02 V
        cuadro.Recalcular();

        var r = cuadro.Alimentador.Resultado!;
        Assert.Equal(99.99m, r.CorrienteDisenoA, 2);
        Assert.Equal(100m, r.ProteccionA);
        return r;
    }

    [Fact]
    public void R10_En2F3HElNeutroNoSeMultiplicaPor140() // 220-61(a) excepción: solo bifásico a 90°
    {
        var r = AlimentadorDe100APorFase2F3H();

        Assert.Equal("3", r.CalibreFase.Designacion);
        Assert.Equal(r.CalibreFase, r.CalibreNeutro);
        Assert.DoesNotContain(r.Citas, c => c.Referencia.StartsWith("220-61"));
    }

    [Fact]
    public void R10_En2F3HNoSeUsaLaTabla310_15b7() // solo 120/240 V, vivienda
    {
        var r = AlimentadorDe100APorFase2F3H();

        Assert.Equal("310-15(b)(16)", r.TablaAmpacidadId);
        Assert.Equal("3", r.CalibreFase.Designacion); // la (b)(7) daría 4 AWG
        Assert.DoesNotContain(r.Citas, c => c.Referencia.Contains("310-15(b)(7)"));
    }

    // ---- R-02 · Caída del alimentador fase por fase, con el neutro -----------------------------------

    /// <summary>Límite alto para que el calibre no suba y se lea la caída del conductor que salió por ampacidad.</summary>
    private static CuadroDeCarga ConLimiteAlto(CuadroDeCarga cuadro, decimal longitudM)
    {
        cuadro.Datos.LongitudAlimentadorM = longitudM;
        cuadro.Datos.CaidaMaxAlimentadorPct = 10m;
        cuadro.Recalcular();
        return cuadro;
    }

    private static decimal CaidaDeLaFase(CuadroDeCarga cuadro, char fase) =>
        cuadro.Alimentador.Resultado!.CaidaPorFase!.Single(f => f.Fase == fase).CaidaPct;

    [Fact]
    public void R02_CasoBaseCon80m_LaFaseCCaeConSuNeutro()
    {
        // Calculado aparte con fasores: I_N = 6.11 A; A −0.02 %, B 4.59 %, C 6.77 %. El equivalente
        // balanceado de antes daba 4.62 % en la fase C: subestimaba ≈32 %.
        var cuadro = ConLimiteAlto(TresAparatos(), 80m);
        var r = cuadro.Alimentador.Resultado!;

        Assert.Equal("12", r.CalibreFase.Designacion);
        Assert.Equal(6.11m, r.CorrienteNeutro!.Value.Magnitud, 2);
        Assert.Equal(-0.02m, CaidaDeLaFase(cuadro, 'A'), 2);
        Assert.Equal(4.59m, CaidaDeLaFase(cuadro, 'B'), 2);
        Assert.Equal(6.77m, CaidaDeLaFase(cuadro, 'C'), 2);
        Assert.Equal(6.77m, r.CaidaTensionPct, 2); // manda la peor fase
        Assert.Contains(r.Citas, c => c.Referencia == "Tabla 9" && c.Descripcion.Contains("manda la fase C"));
    }

    [Fact]
    public void R02_Balanceado3F4H_SinCorrienteEnElNeutro_DaLoMismoQueLaFormulaBalanceada()
    {
        var cuadro = Nuevo(espacios: 6);
        foreach (var espacio in new[] { 1, 3, 5 })
            Espacio(cuadro, espacio).NoContinua = 1500m;
        ConLimiteAlto(cuadro, 80m);
        var r = cuadro.Alimentador.Resultado!;

        // √3 · L · I · (R cosθ + X senθ) / V_FF, con I = 1500 / 127.02 = 11.81 A.
        var d = r.Detalle!;
        var sen = (decimal)Math.Sqrt(1 - 0.81);
        var esperado = (decimal)Math.Sqrt(3) * 0.08m * (1500m / cuadro.Datos.TensionFaseNeutroV)
                       * (d.ResistenciaOhmKm * 0.9m + d.ReactanciaOhmKm * sen) / 220m * 100m;

        Assert.Equal(0m, r.CorrienteNeutro!.Value.Magnitud, 3);
        Assert.All(r.CaidaPorFase!, f => Assert.Equal(esperado, f.CaidaPct, 3));
    }

    [Fact]
    public void R02_UnaFaseDosHilos_EsLaFormulaDeIdaYVuelta()
    {
        var cuadro = Nuevo(espacios: 6, fases: 1, hilos: 2, tension: 127m);
        Espacio(cuadro, 1).NoContinua = 1270m; // 10 A
        ConLimiteAlto(cuadro, 20m);
        var r = cuadro.Alimentador.Resultado!;

        // 2 · L · I · (R cosθ + X senθ) / V_FN: el neutro regresa la misma corriente.
        var d = r.Detalle!;
        var sen = (decimal)Math.Sqrt(1 - 0.81);
        var esperado = 2m * 0.02m * 10m * (d.ResistenciaOhmKm * 0.9m + d.ReactanciaOhmKm * sen) / 127m * 100m;

        Assert.Equal(10m, r.CorrienteNeutro!.Value.Magnitud, 3);
        Assert.Equal(esperado, r.CaidaTensionPct, 3);
    }

    [Fact]
    public void R02_2F3HBalanceado_ElNeutroCargaUnaFaseMasQueLaOtra()
    {
        // 10 A por fase a 120°: el neutro lleva 10 A. Calculado aparte: A 4.20 %, B 7.17 %. La fórmula
        // de antes, 2 · L · I · Z / V_FF, daba 4.38 % en las dos.
        var cuadro = Nuevo(espacios: 6, fases: 2, hilos: 3);
        foreach (var espacio in new[] { 1, 3 })
            Espacio(cuadro, espacio).NoContinua = 1270.17m;
        ConLimiteAlto(cuadro, 80m);

        Assert.Equal("12", cuadro.Alimentador.Resultado!.CalibreFase.Designacion);
        Assert.Equal(10.00m, cuadro.Alimentador.Resultado.CorrienteNeutro!.Value.Magnitud, 2);
        Assert.Equal(4.20m, CaidaDeLaFase(cuadro, 'A'), 2);
        Assert.Equal(7.17m, CaidaDeLaFase(cuadro, 'B'), 2);
    }

    [Fact]
    public void R02_UnDosPolosEntreFasesNoTocaElNeutro()
    {
        // 1F-3H 240/120, 2400 VA a 240 V: 10 A que salen por A y regresan por B.
        var cuadro = Nuevo(espacios: 6, fases: 1, hilos: 3, tension: 240m);
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 1), 2));
        Espacio(cuadro, 1).NoContinua = 2400m;
        ConLimiteAlto(cuadro, 20m);
        var r = cuadro.Alimentador.Resultado!;

        Assert.Equal(0m, r.CorrienteNeutro!.Value.Magnitud, 3);
        Assert.Equal(CaidaDeLaFase(cuadro, 'A'), CaidaDeLaFase(cuadro, 'B'), 3);
    }

    [Fact]
    public void R02_SinNeutro3F3H_SeQuedaLaCaidaBalanceada()
    {
        var cuadro = Nuevo(espacios: 6, hilos: 3);
        Assert.Null(cuadro.CambiarPolos(Espacio(cuadro, 1), 3));
        Espacio(cuadro, 1).NoContinua = 3000m;
        cuadro.Recalcular();

        Assert.Null(cuadro.Alimentador.Resultado!.CaidaPorFase);
        Assert.Null(cuadro.Alimentador.Resultado.CorrienteNeutro);
    }

    // ---- R-04 · La fase que gobierna la elige el motor ------------------------------------------------

    [Fact]
    public void R04_ElMotorEligeLaFaseQueGobierna()
    {
        var cuadro = TresAparatos();
        cuadro.Datos.FactorDemandaEquipo = 0.8m;
        cuadro.Recalcular();
        var r = cuadro.Alimentador.Resultado!;

        Assert.Equal('C', r.FaseQueGobierna);
        Assert.Equal('C', cuadro.Alimentador.Gobierna!.Fase);
        Assert.Contains(r.Citas, c => c.Referencia == "215-2(a)(1)" && c.Descripcion.StartsWith("Fase que gobierna: C"));
        // 0.8 × 12.20 A de la fase C. El factor ya viene aplicado por tipo (R-17): el motor, con F.D. 1,
        // no escribe su propia cita 220-40 — la escribe la memoria, por tipo.
        Assert.Equal(9.76m, r.CorrienteDisenoA, 2);
        Assert.DoesNotContain(r.Citas, c => c.Referencia == "220-40");
    }

    // ---- R-01 · Caída del alimentador: 3 % por tramo, 5 % combinada — 215-2(a)(4) NOTA 2 ---------------

    [Fact]
    public void R15_PorOmisionLosLimitesSuman5PorCiento_YNoHayAviso()
    {
        var datos = new DatosDelTablero();

        Assert.Equal(2m, datos.CaidaMaxAlimentadorPct);
        Assert.Equal(3m, datos.CaidaMaxDerivadoPct);
        Assert.Null(datos.AvisoLimitesDeCaida);
    }

    [Fact]
    public void R15_SiLosLimitesSumanMasDe5PorCiento_SeAvisaEnLosCampos()
    {
        var datos = new DatosDelTablero { CaidaMaxAlimentadorPct = 3m };

        Assert.Equal(
            "Los límites suman 6 %: un circuito puede quedar arriba del 5 % combinado — 210-19(a)(1) NOTA 4, 215-2(a)(4) NOTA 2.",
            datos.AvisoLimitesDeCaida);
    }

    [Fact]
    public void R15_ConLosLimitesPorOmisionNingunCircuitoPasaDel5PorCiento()
    {
        // Cada tramo queda en su límite o abajo, así que la suma no pasa de 2 % + 3 %.
        var cuadro = TresAparatos();
        cuadro.Datos.LongitudAlimentadorM = 80m;
        cuadro.Recalcular();

        Assert.Equal("6", cuadro.Alimentador.Resultado!.CalibreFase.Designacion);
        Assert.True(cuadro.Alimentador.Resultado.CaidaTensionPct <= 2m);
        Assert.Empty(cuadro.ConCaidaCombinadaExcedida);
    }

    [Fact]
    public void R01_CasoBaseCon80mY5PorCiento_AvisaElCircuitoDeLaFaseC()
    {
        // Desde R-02 la caída del alimentador es por fase, con el neutro: con 12 AWG la fase C daría
        // 6.77 % y pasa del 5 %, así que sube a 10 AWG. A cada circuito se le suma la de SU fase.
        var cuadro = TresAparatos();
        cuadro.Datos.LongitudAlimentadorM = 80m;
        cuadro.Datos.CaidaMaxAlimentadorPct = 5m;
        cuadro.Recalcular();

        var alimentador = cuadro.Alimentador.Resultado!;
        Assert.Equal("10", alimentador.CalibreFase.Designacion);
        Assert.Equal(4.01m, alimentador.CaidaTensionPct, 2); // fase C
        var airFryer = Espacio(cuadro, 5);
        Assert.Equal(6.32m, airFryer.CaidaCombinadaPct!.Value, 2); // 4.01 % + 2.31 %
        Assert.Equal(
            "Caída combinada del circuito 5: alimentador 4.01 % + circuito 2.31 % = 6.32 %, mayor que el 5 % " +
            "recomendado — 215-2(a)(4) NOTA 2, 210-19(a)(1) NOTA 4.",
            airFryer.AvisoCaidaCombinada);
        // Fase A: el neutro casi anula la caída (−0.01 %); fase B: 2.75 % + 2.24 % = 4.99 %.
        Assert.Equal([5], cuadro.ConCaidaCombinadaExcedida.Select(c => c.Espacio));
    }

    [Fact]
    public void R01_CasoBaseCon80m_ConEl3PorCientoElAlimentadorSubeA8AWG()
    {
        var cuadro = TresAparatos();
        cuadro.Datos.LongitudAlimentadorM = 80m;
        cuadro.Datos.CaidaMaxAlimentadorPct = 3m;
        cuadro.Recalcular();

        var alimentador = cuadro.Alimentador.Resultado!;
        Assert.Equal("8", alimentador.CalibreFase.Designacion);
        Assert.Equal(2.64m, alimentador.CaidaTensionPct, 2); // fase C
        Assert.Equal(4.95m, Espacio(cuadro, 5).CaidaCombinadaPct!.Value, 2); // 2.64 % + 2.31 %
        Assert.Empty(cuadro.ConCaidaCombinadaExcedida);
    }

    [Fact]
    public void R01_CasoBaseCon20m_SinAvisoDeCaidaCombinada()
    {
        var cuadro = TresAparatos();

        Assert.Equal(4.00m, Espacio(cuadro, 5).CaidaCombinadaPct!.Value, 2); // fase C 1.69 % + 2.31 %
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
        // Fase A: alumbrado, 3000 VA continuos; fase B: contactos, 3500 VA no continuos. Sin demanda
        // gobierna A (1.25 × 23.62 = 29.53 A contra 27.56 A); con 0.5 en alumbrado gobierna B.
        var cuadro = Nuevo();
        Espacio(cuadro, 1).Continua = 3000m;
        Espacio(cuadro, 3).Tipo = TipoCarga.Contactos;
        Espacio(cuadro, 3).NoContinua = 3500m;
        cuadro.Recalcular();
        Assert.Equal('A', cuadro.Alimentador.Gobierna!.Fase);

        cuadro.Datos.FactorDemandaAlumbrado = 0.5m;
        cuadro.Recalcular();
        Assert.Equal('B', cuadro.Alimentador.Gobierna!.Fase);
        Assert.Equal(3500m / cuadro.Datos.TensionFaseNeutroV, cuadro.Alimentador.Resultado!.CorrienteDisenoA, 2);
    }

    [Fact]
    public void M03_SeAvisaCuandoElPrincipalEsMenorQueUnDerivado()
    {
        // 500 VA de contactos de cocina: el derivado sale en 20 A por 210-11(c)(1), y el
        // alimentador —1500 VA por 220-52(a), 11.81 A— en 15 A.
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar; // 210-11(c) y 220-52 son de vivienda — I-46
        var contactos = Espacio(cuadro, 3);
        contactos.Tipo = TipoCarga.Contactos;
        contactos.Uso = UsoDeContactos.AparatosPequenos;
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
        var c = Espacio(cuadro, 1);
        c.Unidad = UnidadConsumo.Amperes;
        c.NoContinua = 26m;
        c.LongitudM = 5m;
        Agrupar.EnTubo(cuadro, c, 9);

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
        var c = Espacio(cuadro, 1);
        c.Unidad = UnidadConsumo.Amperes;
        c.Continua = 32m;
        c.LongitudM = 5m;
        Agrupar.EnTubo(cuadro, c, 9);

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
            var c = Espacio(cuadro, 1);
            c.Tipo = tipo;
            c.Unidad = UnidadConsumo.Amperes;
            c.Continua = 32m;
            c.LongitudM = 5m;
            Agrupar.EnTubo(cuadro, c, 9);

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

    [Fact]
    public void AlCambiarLasFases_LosHilosQuedanEnUnSistemaQueExiste()
    {
        var datos = new DatosDelTablero(); // 3F-4H

        datos.Fases = 1;
        Assert.Equal([2, 3], datos.HilosValidos);
        Assert.Equal(2, datos.Hilos); // no «1F-4H»

        datos.Hilos = 3; // 1F-3H
        datos.Fases = 1;
        Assert.Equal(3, datos.Hilos); // mismas fases: no se toca

        datos.Fases = 2;
        Assert.Equal(3, datos.Hilos);

        datos.Fases = 3;
        Assert.Equal(4, datos.Hilos); // de regreso a 3F-4H, no a un 3F-3H que nadie pidió
        Assert.Equal([3, 4], datos.HilosValidos);
    }

    [Fact]
    public void I53_AlCambiarDeConfiguracion_LaTensionPasaALaNominalDeLaNom()
    {
        // David, 2026-09-25: con 220 V de un 3F-4H pasó a 1F-2H y el tablero calculó 220 V de fase a
        // neutro; a 1F-3H, 110 V. Ninguna es tensión de la NOM (110-4).
        var datos = new DatosDelTablero(); // 3F-4H, 220 V
        Assert.Null(datos.AvisoTension);

        datos.Fases = 1; // 1F-2H: fase y neutro
        Assert.Equal(127m, datos.TensionFaseNeutroV);

        datos.Hilos = 3; // 1F-3H: 120/240
        Assert.Equal(240m, datos.TensionFaseFaseV);
        Assert.Equal(120m, datos.TensionFaseNeutroV);

        datos.Fases = 2; // 2F-3H: 220/127
        Assert.Equal(220m, datos.TensionFaseFaseV);

        datos.Fases = 3;
        datos.TensionFaseFaseV = 480m; // 480Y/277
        datos.Hilos = 3; // 3F-3H: 480 sigue siendo nominal, se queda
        Assert.Equal(480m, datos.TensionFaseFaseV);
        Assert.Null(datos.AvisoTension);
    }

    [Fact]
    public void I54_1F2H_OfreceDe1a8Espacios_YAjustaAlCambiarDeConfiguracion()
    {
        // David, 2026-09-25: en 1F-2H, centros de carga de 1, 2, 4, 6 y 8 espacios; de 12 en adelante no.
        var datos = new DatosDelTablero(); // 3F-4H, 24 espacios
        datos.Fases = 1; // 1F-2H
        Assert.Equal([1, 2, 4, 6, 8], datos.EspaciosValidos);
        Assert.Equal(8, datos.NumeroEspacios);

        datos.NumeroEspacios = 4;
        datos.Fases = 3; // de regreso a 3F: 4 no existe, pasa a 6
        Assert.Equal([6, 12, 18, 24, 30, 36, 42], datos.EspaciosValidos);
        Assert.Equal(6, datos.NumeroEspacios);

        datos.Fases = 1;
        datos.NumeroEspacios = 8;
        datos.Hilos = 3; // 1F-3H, dos barras: 8 no existe, pasa a 12
        Assert.Equal(12, datos.NumeroEspacios);
    }

    [Fact]
    public void I54_ConUnaSolaBarra_LosRenglonesYElGabineteVanEnOrden()
    {
        var una = Nuevo(espacios: 6, fases: 1, hilos: 2, tension: 127m);
        Assert.True(una.UnaSolaBarra);
        Assert.Equal([1, 2, 3, 4, 5, 6], una.Lados.Single().Select(c => c.Espacio));
        Assert.All(una.Gabinete, b => Assert.Equal(1, b.Columna));
        Assert.Equal([1, 2, 3, 4, 5, 6], una.Gabinete.Select(b => b.Fila));

        var tres = Nuevo(espacios: 6); // 3F-4H: nones arriba, pares abajo, dos columnas
        Assert.Equal(2, tres.Lados.Count);
        Assert.Equal([1, 3, 5], tres.Lados[0].Select(c => c.Espacio));
        Assert.Equal(2, tres.Gabinete.Single(b => b.Circuito.Espacio == 2).Columna);
    }

    [Fact]
    public void I53_UnaTensionQueNoEsDeLaNom_SeAvisa()
    {
        var datos = new DatosDelTablero();
        datos.Fases = 1;
        datos.Hilos = 3;
        datos.TensionFaseFaseV = 220m; // la captura de David en 1F-3H
        Assert.Contains("120/240 V", datos.AvisoTension);
        Assert.Contains("110 V", datos.AvisoTension);

        datos.Hilos = 2; // 1F-2H: 220 no es nominal, pasa a 127
        Assert.Equal(127m, datos.TensionFaseFaseV);
        datos.TensionFaseFaseV = 220m;
        Assert.Contains("1F-2H (fase y neutro): 127 o 120 V", datos.AvisoTension);
    }
}
