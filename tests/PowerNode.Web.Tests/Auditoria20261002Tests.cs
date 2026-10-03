using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Auditoría NOM-001-SEDE-2012, ronda 2, del 2026-10-02</b> (caja negra contra <c>3f53a3f</c>). Cada
/// prueba lleva el número del hallazgo que reproduce; los valores esperados salen de la norma recalculada a
/// mano. Condiciones base: 3F-4H 220/127 V, cobre THHN, lugar seco, 30 °C, 20 m, EMT.
/// </summary>
public class Auditoria20261002Tests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static readonly MotorNom Motor = new(Json);

    private static CuadroDeCarga Nuevo(int espacios = 12)
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = espacios;
        cuadro.Datos.Fases = 3;
        cuadro.Datos.Hilos = 4;
        cuadro.Datos.TensionFaseFaseV = 220m;
        cuadro.Recalcular();
        return cuadro;
    }

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int numero) =>
        cuadro.Circuitos.Single(c => c.Espacio == numero);

    private static CircuitoDelCuadro Carga(
        CuadroDeCarga cuadro, int espacio, CategoriaDeCarga tipo, SubtipoDeCarga subtipo, int polos,
        int cantidad, decimal unitaria, bool continua, UnidadConsumo unidad = UnidadConsumo.VoltAmperes)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = tipo;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        var a = c.AgregarCarga();
        a.Subtipo = subtipo;
        a.Cantidad = cantidad;
        a.Unidad = unidad;
        a.CargaUnitaria = unitaria;
        a.Continua = continua;
        cuadro.Recalcular();
        return c;
    }

    // ---- N-1 · Conductores en paralelo por ampacidad en el alimentador -----------------------------

    /// <summary>
    /// 3 × 90 kVA trifásicos, continuos: 270 000 / (√3 × 220) = 708.57 A, 125 % = 885.71 A → protección de
    /// 1000 A (240-6(a)). Ni 2000 kcmil de cobre a 75 °C (665 A) alcanza: antes, «No hay calibre en el
    /// catálogo…» y el alimentador sin resultado. Con 2 por fase: 442.86 A por conductor → 700 kcmil (460 A),
    /// pero 1000 A pasa de 800 A y 240-4(b) no aplica: el juego tiene que cubrir los 1000 A. 2 × 800 kcmil
    /// (490 A) = 980 A no; 2 × 900 kcmil (520 A) = 1040 A sí. La tierra, de la Tabla 250-122 por 1000 A
    /// (2/0 AWG), una en cada canalización — 250-122(f).
    /// </summary>
    [Fact]
    public void N1_ElAlimentadorSubeAParalelosCuandoLaAmpacidadNoAlcanza()
    {
        var cuadro = Nuevo();
        foreach (var espacio in new[] { 1, 2, 7 })
            Carga(cuadro, espacio, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 3, 1, 90000m, continua: true);

        Assert.Null(cuadro.Alimentador.Error);
        var r = cuadro.Alimentador.Resultado!;
        Assert.Equal(708.57m, Math.Round(r.CorrienteDisenoA, 2));
        Assert.Equal(1000m, r.ProteccionA);
        Assert.Equal(2, r.NumeroConductoresParalelo);
        Assert.Equal("900", r.CalibreFase.Designacion);
        Assert.True(r.CalibreFase.PermiteParalelo); // 310-10(h)(1): 1/0 AWG o mayor.
        Assert.Equal("2/0", r.CalibreTierra.Designacion);
        Assert.Contains(r.Citas, x => x.Referencia == "250-122(f)");
        var cita = Assert.Single(r.Citas, x => x.Referencia == "310-10(h)(1)" && x.Descripcion.Contains("ampacidad"));
        Assert.Contains("2 conductores en paralelo por fase", cita.Descripcion);
        Assert.DoesNotContain("caída de tensión", cita.Descripcion);
        // Una canalización por juego, todas iguales — 310-10(h)(3).
        Assert.Equal(2, cuadro.Datos.CanalizacionAlimentador.CanalizacionesIguales);
    }

    /// <summary>
    /// Los juegos en un solo tubo: cada conductor cuenta como portador (310-15(b)(3)(a)) y entra el factor
    /// de agrupamiento. Con el factor, el calibre o el número de juegos crece, y el alimentador sigue
    /// calculando.
    /// </summary>
    [Fact]
    public void N1_LosJuegosEnUnTuboLlevanElFactorDeAgrupamiento()
    {
        var cuadro = Nuevo();
        foreach (var espacio in new[] { 1, 2, 7 })
            Carga(cuadro, espacio, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 3, 1, 90000m, continua: true);
        var separados = cuadro.Alimentador.Resultado!;

        cuadro.Datos.CanalizacionAlimentador.JuegosEnUnTubo = true;
        cuadro.Recalcular();

        Assert.Null(cuadro.Alimentador.Error);
        var r = cuadro.Alimentador.Resultado!;
        Assert.True(r.NumeroConductoresParalelo >= 2);
        Assert.True(r.Detalle!.FactorAgrupamiento < 1m);
        Assert.True(r.Detalle.AmpacidadConductorA >= r.ProteccionA);
        Assert.True(r.CalibreFase.AreaMm2 * r.NumeroConductoresParalelo >= separados.CalibreFase.AreaMm2 * separados.NumeroConductoresParalelo);
    }

    /// <summary>El de 415.5 A de la auditoría, que ya cumplía, se queda en un conductor por fase.</summary>
    [Fact]
    public void N1_UnAlimentadorQueAlcanzaConUnoNoSeParte()
    {
        var cuadro = Nuevo();
        // 415.5 A en la fase: 158.33 kVA trifásicos no continuos.
        Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 3, 1, 158325m, continua: false);

        var r = cuadro.Alimentador.Resultado!;
        Assert.Equal(415.5m, Math.Round(r.CorrienteDisenoA, 1));
        Assert.Equal(450m, r.ProteccionA);
        Assert.Equal(1, r.NumeroConductoresParalelo);
        Assert.Equal("600", r.CalibreFase.Designacion);
        Assert.Equal("2", r.CalibreTierra.Designacion);
    }

    // ---- N-2 · 210-3: circuito de varias salidas de más de 50 A -----------------------------------

    /// <summary>
    /// Restaurante, 2 polos, Aparatos · Cocina comercial, 3 × 5 000 W: 75.76 A → 80 A. No es individual:
    /// 210-3 limita a 50 A. La excepción (más de 50 A para cargas que no son de alumbrado) es solo
    /// industrial con supervisión calificada; se nombra en el aviso.
    /// </summary>
    [Fact]
    public void N2_UnCircuitoDeVariasSalidasDeMasDe50ALlevaAviso()
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.Restaurante;
        var c = Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.CocinaComercial, 2, 3, 5000m, continua: false, UnidadConsumo.Watts);

        Assert.Equal(ClaseDeCircuito.ParaAparatos, c.ClaseDelCircuito);
        Assert.Equal(80m, c.Resultado!.ProteccionA);
        var regla = Assert.Single(c.ReglasDeClase, x => x.Referencia == "210-3");
        Assert.True(regla.Aviso);
        Assert.StartsWith("Circuito de 3 salidas de 80 A: 210-3 limita a 50 A", regla.Texto);
        Assert.Contains("Separar en circuitos individuales", regla.Texto);
        Assert.Contains("industrial", regla.Texto);
        Assert.Contains(cuadro.AvisosDe(c), x => x.Contains("210-3"));
    }

    [Fact]
    public void N2_UnCircuitoIndividualDeMasDe50ANoLlevaElAviso()
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.Restaurante;
        var c = Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.CocinaComercial, 2, 1, 15000m, continua: false, UnidadConsumo.Watts);

        Assert.Equal(ClaseDeCircuito.Individual, c.ClaseDelCircuito);
        Assert.True(c.Resultado!.ProteccionA > 50m);
        Assert.DoesNotContain(c.ReglasDeClase, x => x.Referencia == "210-3");
    }

    [Fact]
    public void N2_UnCircuitoDeVariasSalidasDe50ANoLlevaElAviso()
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.Restaurante;
        // 2 × 4 000 W a 220 V: 40.40 A → 45 A (o 50), dentro de 210-3.
        var c = Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.CocinaComercial, 2, 2, 4000m, continua: false, UnidadConsumo.Watts);

        Assert.True(c.Resultado!.ProteccionA <= 50m);
        Assert.DoesNotContain(c.ReglasDeClase, x => x.Referencia == "210-3");
    }

    // ---- Observación menor · Interruptor de 1 polo arriba de la familia ----------------------------

    /// <summary>
    /// El subtablero de la auditoría (1 polo, 10 kVA continua + 15 kVA no continua → 225 A) en riel DIN:
    /// la familia llega a 125 A y el aviso del tablero ya lo dice.
    /// </summary>
    [Fact]
    public void Menor_UnSubtableroDe225AEnRielDinSeAvisa()
    {
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Tablero;
        c.Continua = 10000m;
        c.NoContinua = 15000m;
        cuadro.Recalcular();

        Assert.Equal(225m, c.Resultado!.ProteccionA);
        Assert.Contains(cuadro.Alimentador.Avisos, x => x.StartsWith("En riel DIN no hay interruptores de más de 125 A") && x.Contains("circuito 1 (225 A)"));
    }

    // ---- I-160 · Interruptor de 1 polo arriba de la familia (aceptado por David) --------------------

    /// <summary>El subtablero de la auditoría (225 A en 1 polo) en centro de carga: arriba de 70 A, aviso en el renglón.</summary>
    [Fact]
    public void I160_UnSubtableroDe225AEnUnPoloAvisaEnCentroDeCarga()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Tablero;
        c.Continua = 10000m;
        c.NoContinua = 15000m;
        cuadro.Recalcular();

        Assert.Equal(225m, c.Resultado!.ProteccionA);
        var regla = Assert.Single(c.ReglasDeClase, x => x.Referencia == "serie de interruptores");
        Assert.True(regla.Aviso);
        Assert.StartsWith("Interruptor de 1 polo de 225 A: en centro de carga los de 1 polo llegan, por lo común, a 70 A.", regla.Texto);
        Assert.Contains(cuadro.AvisosDe(c), x => x.Contains("1 polo de 225 A"));
        Assert.Contains(cuadro.AvisosDeCircuitos, x => x.StartsWith("Circuito 1: Interruptor de 1 polo de 225 A"));
    }

    [Theory]
    [InlineData(1, 8000, true)]    // 62.99 A → 70 A: el tope, sin aviso.
    [InlineData(1, 10000, false)]  // 78.73 A → 80 A: arriba de 70 A.
    [InlineData(2, 20000, true)]   // 2 polos: no aplica.
    public void I160_ElTopeDe70AEsDeUnPolo(int polos, decimal va, bool sinAviso)
    {
        var cuadro = Nuevo();
        var c = Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, polos, 1, va, continua: false);

        Assert.Equal(sinAviso, !c.ReglasDeClase.Any(x => x.Referencia == "serie de interruptores"));
    }

    [Fact]
    public void I160_ConLaListaCompletaElTopeEs125A()
    {
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.NomCompleta;
        var c = Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 12000m, continua: false);
        Assert.Equal(100m, c.Resultado!.ProteccionA);
        Assert.DoesNotContain(c.ReglasDeClase, x => x.Referencia == "serie de interruptores");

        var d = Carga(cuadro, 2, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 18000m, continua: false);
        Assert.True(d.Resultado!.ProteccionA > 125m);
        Assert.Contains(d.ReglasDeClase, x => x.Referencia == "serie de interruptores" && x.Texto.Contains("en centro de carga y en riel DIN") && x.Texto.Contains("125 A"));
    }

    // ---- I-161 · El neutro del alimentador por 220-61 (aceptado por David) -------------------------

    /// <summary>
    /// 3 × 14 000 VA de 1 polo en la fase A (espacios 1, 2 y 7): 3 × 110.22 = 330.66 A. Sin marcar, el
    /// neutro es igual que la fase. Marcado: lo que pasa de 200 A al 70 % — 220-61(b)(2): 200 + 0.7 ×
    /// 130.66 = 291.46 A → 350 kcmil (310 A a 75 °C), contra la fase de 400 kcmil (335 A).
    /// </summary>
    [Fact]
    public void I161_ElNeutroSeReduceConEl70ArribaDe200A()
    {
        var cuadro = Nuevo();
        foreach (var espacio in new[] { 1, 2, 7 })
            Carga(cuadro, espacio, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 14000m, continua: false);
        var r = cuadro.Alimentador.Resultado!;
        Assert.Equal(r.CalibreFase, r.CalibreNeutro);
        Assert.Null(cuadro.NeutroReducido);
        Assert.Null(cuadro.PorQueNoSeReduceElNeutro);

        cuadro.Datos.NeutroReducido220_61 = true;
        cuadro.Recalcular();

        r = cuadro.Alimentador.Resultado!;
        var nr = cuadro.NeutroReducido!;
        Assert.Equal('A', nr.Fase);
        Assert.Equal(330.66m, Math.Round(nr.DesbalanceA, 2));
        Assert.Equal(291.46m, Math.Round(nr.CorrienteA, 2));
        Assert.True(nr.Con70Pct);
        Assert.Equal("400", r.CalibreFase.Designacion);
        Assert.Equal("350", r.CalibreNeutro.Designacion);
        Assert.Contains(r.Citas, x => x.Referencia == "220-61" && x.Descripcion.Contains("220-61(b)(2)"));
    }

    /// <summary>
    /// 150 kVA trifásicos sin neutro y 2 × 3 000 VA de 1 polo en A: el neutro lleva 47.24 A → 8 AWG por
    /// ampacidad, pero no menor que la tierra de equipos de la protección (Tabla 250-122).
    /// </summary>
    [Fact]
    public void I161_ElNeutroNoBajaDeLaTierraDeEquipos()
    {
        var cuadro = Nuevo();
        Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 3, 1, 150000m, continua: true);
        Assert.False(Espacio(cuadro, 1).LlevaNeutro);
        Carga(cuadro, 2, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        Carga(cuadro, 8, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        cuadro.Datos.NeutroReducido220_61 = true;
        cuadro.Recalcular();

        var r = cuadro.Alimentador.Resultado!;
        var nr = cuadro.NeutroReducido!;
        Assert.Equal(47.24m, Math.Round(nr.DesbalanceA, 2));
        Assert.False(nr.Con70Pct);
        Assert.Equal(r.CalibreTierra.Designacion, r.CalibreNeutro.Designacion);
        Assert.True(r.CalibreNeutro.AreaMm2 < r.CalibreFase.AreaMm2);
    }

    [Fact]
    public void I161_En2FasesDeEstrellaYConCargaNoLinealNoSeReduce()
    {
        var cuadro = Nuevo();
        cuadro.Datos.Fases = 2;
        cuadro.Datos.Hilos = 3;
        cuadro.Recalcular();
        Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        cuadro.Datos.NeutroReducido220_61 = true;
        cuadro.Recalcular();
        Assert.Contains("220-61(c)(1)", cuadro.PorQueNoSeReduceElNeutro);
        Assert.Null(cuadro.NeutroReducido);
        Assert.Equal(cuadro.Alimentador.Resultado!.CalibreFase, cuadro.Alimentador.Resultado.CalibreNeutro);

        var otro = Nuevo();
        Carga(otro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        otro.Datos.CargaNoLineal = true;
        otro.Datos.NeutroReducido220_61 = true;
        otro.Recalcular();
        Assert.Contains("220-61(c)(2)", otro.PorQueNoSeReduceElNeutro);
        Assert.Null(otro.NeutroReducido);
    }

    [Fact]
    public void I161_LaOpcionSeGuardaEnElArchivo()
    {
        var cuadro = Nuevo();
        Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        cuadro.Datos.NeutroReducido220_61 = true;
        cuadro.Recalcular();

        var abierto = PowerNode.Web.Modelo.Archivo.ArchivoDelCuadro.Abrir(
            PowerNode.Web.Modelo.Archivo.ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now), Motor);
        Assert.True(abierto.Cuadro!.Datos.NeutroReducido220_61);
    }

    // ---- I-162 · Conductores por fase del alimentador, fijados por el proyectista ------------------

    private static CuadroDeCarga De708A()
    {
        var cuadro = Nuevo();
        foreach (var espacio in new[] { 1, 2, 7 })
            Carga(cuadro, espacio, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 3, 1, 90000m, continua: true);
        return cuadro;
    }

    private static CuadroDeCarga De415A()
    {
        var cuadro = Nuevo();
        Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 3, 1, 158325m, continua: false);
        return cuadro;
    }

    /// <summary>
    /// 708.57 A, protección de 1000 A: cumplen 2 a 6 por fase. Cada opción con su fase, tierra (2/0 por
    /// 1000 A, una por canalización), ampacidad del juego ≥ 1000 A (240-4, sin 240-4(b) arriba de 800 A),
    /// caída, canalizaciones y cobre = (3 fases + neutro + tierra) × N.
    /// </summary>
    [Fact]
    public void I162_Con708ACumplenDe2a6()
    {
        var cuadro = De708A();

        Assert.Null(cuadro.Datos.ConductoresPorFaseAlimentador);
        Assert.Equal(2, cuadro.ConductoresPorFaseAutomatico);
        Assert.True(cuadro.ConvieneCompararParalelos);
        var o = cuadro.OpcionesDeParalelo;
        Assert.Equal([2, 3, 4, 5, 6], o.Select(x => x.PorFase));
        Assert.Equal(["900", "400", "250", "3/0", "2/0"], o.Select(x => x.Fase.Designacion));
        Assert.All(o, x => Assert.Equal("2/0", x.Tierra.Designacion));
        Assert.All(o, x => Assert.True(x.AmpacidadA >= 1000m));
        Assert.Equal([1040m, 1005m, 1020m, 1000m, 1050m], o.Select(x => x.AmpacidadA));
        Assert.Equal(2, Assert.Single(o, x => x.EsAutomatico).PorFase);
        var tres = o.Single(x => x.PorFase == 3);
        Assert.Equal(3 * (4 * tres.Fase.AreaMm2 + tres.Tierra.AreaMm2), tres.CobreMm2);
        Assert.StartsWith("EMT · 3 × ", tres.Canalizacion);
        // La del tablero se queda con la del automático: las opciones no la tocan.
        Assert.Equal(2, cuadro.Datos.CanalizacionAlimentador.CanalizacionesIguales);
    }

    /// <summary>Fijado en 3: 3 × 400 kcmil, tres canalizaciones, y la cita dice que lo fijó el proyectista.</summary>
    [Fact]
    public void I162_FijadoEn3SeCalculaConTres()
    {
        var cuadro = De708A();
        cuadro.Datos.ConductoresPorFaseAlimentador = 3;
        cuadro.Recalcular();

        Assert.Null(cuadro.Alimentador.Error);
        var r = cuadro.Alimentador.Resultado!;
        Assert.Equal(3, r.NumeroConductoresParalelo);
        Assert.Equal("400", r.CalibreFase.Designacion);
        Assert.Equal("2/0", r.CalibreTierra.Designacion);
        Assert.Equal(3, cuadro.Datos.CanalizacionAlimentador.CanalizacionesIguales);
        Assert.Equal(2, cuadro.ConductoresPorFaseAutomatico);
        var cita = Assert.Single(r.Citas, x => x.Referencia == "310-10(h)(1)" && x.Descripcion.Contains("fijado por el proyectista"));
        Assert.Contains("3 conductores por fase", cita.Descripcion);
        Assert.Contains("el automático daba 2", cita.Descripcion);
        // Sin la cita de que subió solo: no subió, se fijó.
        Assert.DoesNotContain(r.Citas, x => x.Descripcion.Contains("se sube automáticamente"));
        Assert.Equal(2, Assert.Single(cuadro.OpcionesDeParalelo, x => x.EsAutomatico).PorFase);
    }

    /// <summary>De regreso al automático (null): 2 × 900 kcmil otra vez.</summary>
    [Fact]
    public void I162_DeRegresoAlAutomatico()
    {
        var cuadro = De708A();
        cuadro.Datos.ConductoresPorFaseAlimentador = 4;
        cuadro.Recalcular();
        Assert.Equal("250", cuadro.Alimentador.Resultado!.CalibreFase.Designacion);

        cuadro.Datos.ConductoresPorFaseAlimentador = null;
        cuadro.Recalcular();
        Assert.Equal(2, cuadro.Alimentador.Resultado!.NumeroConductoresParalelo);
        Assert.Equal("900", cuadro.Alimentador.Resultado.CalibreFase.Designacion);
        Assert.DoesNotContain(cuadro.Alimentador.Resultado.Citas, x => x.Descripcion.Contains("fijado por el proyectista"));
    }

    /// <summary>708.57 A con 1 por fase no alcanza: error con la ampacidad, y cuáles cumplen.</summary>
    [Fact]
    public void I162_FijadoEnUnoSinAmpacidadDiceCualesCumplen()
    {
        var cuadro = De708A();
        cuadro.Datos.ConductoresPorFaseAlimentador = 1;
        cuadro.Recalcular();

        Assert.Null(cuadro.Alimentador.Resultado);
        Assert.Equal(
            "Con 1 conductor por fase, ni el calibre más grande del catálogo alcanza la ampacidad o queda protegido — 240-4. "
            + "Cumplen 2, 3, 4, 5 o 6 por fase.",
            cuadro.Alimentador.Error);
        Assert.Equal(5, cuadro.OpcionesDeParalelo.Count);
        Assert.Equal(2, cuadro.ConductoresPorFaseAutomatico);
    }

    /// <summary>
    /// 415.5 A, protección de 450 A: cumplen 1 × 600 kcmil, 2 × 4/0 y 3 × 1/0. Con 4 cada conductor saldría
    /// de 2 AWG, menor que 1/0 — 310-10(h)(1).
    /// </summary>
    [Fact]
    public void I162_Con415AFijadoEn4NoLlegaA1_0()
    {
        var cuadro = De415A();
        Assert.Equal([1, 2, 3], cuadro.OpcionesDeParalelo.Select(x => x.PorFase));
        Assert.Equal(["600", "4/0", "1/0"], cuadro.OpcionesDeParalelo.Select(x => x.Fase.Designacion));
        Assert.Equal(1, Assert.Single(cuadro.OpcionesDeParalelo, x => x.EsAutomatico).PorFase);
        Assert.True(cuadro.ConvieneCompararParalelos);

        cuadro.Datos.ConductoresPorFaseAlimentador = 4;
        cuadro.Recalcular();

        Assert.Null(cuadro.Alimentador.Resultado);
        Assert.Equal(
            "Con 4 conductores por fase, cada uno sale de 2 AWG, y 310-10(h)(1) pide 1/0 AWG o mayor en paralelo. Cumplen 1, 2 o 3 por fase.",
            cuadro.Alimentador.Error);
    }

    /// <summary>Un alimentador chico: una sola opción de un calibre delgado, nada que comparar.</summary>
    [Fact]
    public void I162_UnAlimentadorChicoNoOfreceComparar()
    {
        var cuadro = Nuevo();
        Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);

        var opcion = Assert.Single(cuadro.OpcionesDeParalelo);
        Assert.Equal(1, opcion.PorFase);
        Assert.True(opcion.EsAutomatico);
        Assert.False(cuadro.ConvieneCompararParalelos);
    }

    /// <summary>Sin carga no hay opciones; y un N fuera de 1 a 6 regresa al automático.</summary>
    [Fact]
    public void I162_SinCargaNoHayOpcionesYElRangoEsDe1a6()
    {
        var cuadro = Nuevo();
        Assert.Empty(cuadro.OpcionesDeParalelo);
        Assert.Null(cuadro.ConductoresPorFaseAutomatico);

        cuadro.Datos.ConductoresPorFaseAlimentador = 7;
        Assert.Null(cuadro.Datos.ConductoresPorFaseAlimentador);
        cuadro.Datos.ConductoresPorFaseAlimentador = 0;
        Assert.Null(cuadro.Datos.ConductoresPorFaseAlimentador);
    }

    /// <summary>Con los juegos en un tubo, el N fijado cuenta todos los conductores en una canalización.</summary>
    [Fact]
    public void I162_FijadoConLosJuegosEnUnTubo()
    {
        var cuadro = De708A();
        cuadro.Datos.CanalizacionAlimentador.JuegosEnUnTubo = true;
        cuadro.Datos.ConductoresPorFaseAlimentador = 3;
        cuadro.Recalcular();

        Assert.Null(cuadro.Alimentador.Error);
        var r = cuadro.Alimentador.Resultado!;
        Assert.Equal(3, r.NumeroConductoresParalelo);
        Assert.True(r.Detalle!.FactorAgrupamiento < 1m);
        Assert.Equal(1, cuadro.Datos.CanalizacionAlimentador.CanalizacionesIguales);
        // 3 juegos × 3 fases; el neutro de un 3F-4H lineal no cuenta — 310-15(b)(5)(1).
        Assert.Equal(9, cuadro.Datos.CanalizacionAlimentador.Conteo!.Portadores);
    }

    [Fact]
    public void I162_ElNFijadoSeGuardaEnElArchivo()
    {
        var cuadro = De708A();
        cuadro.Datos.ConductoresPorFaseAlimentador = 3;
        cuadro.Recalcular();

        var abierto = PowerNode.Web.Modelo.Archivo.ArchivoDelCuadro.Abrir(
            PowerNode.Web.Modelo.Archivo.ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now), Motor);
        Assert.Equal(3, abierto.Cuadro!.Datos.ConductoresPorFaseAlimentador);
        Assert.Equal("400", abierto.Cuadro.Alimentador.Resultado!.CalibreFase.Designacion);

        cuadro.Datos.ConductoresPorFaseAlimentador = null;
        var automatico = PowerNode.Web.Modelo.Archivo.ArchivoDelCuadro.Abrir(
            PowerNode.Web.Modelo.Archivo.ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now), Motor);
        Assert.Null(automatico.Cuadro!.Datos.ConductoresPorFaseAlimentador);
    }

    // ---- Ronda 3 (verificación sobre f960bcb) -----------------------------------------------------

    /// <summary>
    /// 150 kVA trifásicos continuos y 2 × 3 000 VA de 1 polo en A, 20 m: el neutro lleva 47.24 A y, reducido,
    /// sale de 1 AWG (la tierra de equipos) contra la fase de 1000 kcmil. Antes la caída seguía como si el
    /// neutro fuera de 1000 kcmil. La diferencia de la fase A es solo la del neutro:
    /// Δe = L/1000 × [ (R_N − R_F) × Re(I_N) − (X_N − X_F) × Im(I_N) ], con I_N referida a V_AN — R3-2.
    /// </summary>
    [Fact]
    public void R3_2_ElNeutroReducidoEntraALaCaidaDeTension()
    {
        var cuadro = Nuevo();
        Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 3, 1, 150000m, continua: true);
        Carga(cuadro, 2, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        Carga(cuadro, 8, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        var completo = cuadro.Alimentador.Resultado!;
        Assert.Equal(completo.CalibreFase, completo.CalibreNeutro);
        var antesA = completo.CaidaPorFase!.Single(f => f.Fase == 'A');

        cuadro.Datos.NeutroReducido220_61 = true;
        cuadro.Recalcular();

        var r = cuadro.Alimentador.Resultado!;
        Assert.Equal("1", r.CalibreNeutro.Designacion);
        Assert.Equal(1, r.NumeroConductoresParalelo);
        var canal = cuadro.Datos.CanalizacionAlimentador.MaterialParaTabla9;
        var zf = Motor.Impedancia.Impedancia(r.CalibreFase, MaterialConductor.Cobre, canal)!.Value;
        var zn = Motor.Impedancia.Impedancia(r.CalibreNeutro, MaterialConductor.Cobre, canal)!.Value;
        var iN = r.CorrienteNeutro!.Value;
        var km = cuadro.Datos.LongitudAlimentadorM / 1000m;
        var esperadaV = antesA.CaidaV + km * ((zn.ROhmKm - zf.ROhmKm) * iN.Real - (zn.XOhmKm - zf.XOhmKm) * iN.Imaginario);
        var despuesA = r.CaidaPorFase!.Single(f => f.Fase == 'A');
        Assert.Equal(Math.Round(esperadaV, 3), Math.Round(despuesA.CaidaV, 3));
        Assert.True(despuesA.CaidaPct > antesA.CaidaPct);
        Assert.Equal(r.CaidaPorFase!.Max(f => f.CaidaPct), r.CaidaTensionPct);
        Assert.Equal(despuesA.CaidaV, r.Detalle!.CaidaTensionV);
        Assert.Contains(r.Citas, x => x.Referencia == "Tabla 9" && x.Descripcion.Contains("con el neutro de 1"));
        Assert.Equal(zn, cuadro.NeutroReducido!.Impedancia);
    }

    /// <summary>La memoria dice que el neutro está reducido y pone su Z_N, en vez de «del mismo calibre que la fase».</summary>
    [Fact]
    public void R3_2_LaMemoriaDiceQueElNeutroEstaReducido()
    {
        var cuadro = Nuevo();
        Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 3, 1, 150000m, continua: true);
        Carga(cuadro, 2, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        string Caida() => string.Join("\n", PowerNode.Web.Modelo.Memoria.MemoriaDeCalculo.Secciones(
                PowerNode.Web.Modelo.Memoria.MemoriaDeCalculo.DelAlimentador(cuadro)!)
            .Single(b => b.Titulo.StartsWith("6.")) is var b ? [.. b.Formulas, .. b.Notas] : []);

        Assert.Contains("El neutro es del mismo calibre que la fase.", Caida());
        Assert.DoesNotContain("Z_N", Caida());

        cuadro.Datos.NeutroReducido220_61 = true;
        cuadro.Recalcular();

        var texto = Caida();
        Assert.DoesNotContain("del mismo calibre que la fase", texto);
        Assert.Contains("reducido a su carga de desbalance (220-61)", texto);
        Assert.Contains("Z_N = (", texto);
        Assert.Contains("Z × I_f + Z_N × I_N", texto);
    }

    /// <summary>
    /// A 100 m con 3 por fase, el neutro que pide la carga de desbalance haría pasar la caída del límite: sube
    /// lo necesario, y la cita lo dice. La caída nunca queda arriba del límite por reducir el neutro.
    /// </summary>
    [Fact]
    public void R3_2_SiLaCaidaPasaDelLimiteElNeutroSube()
    {
        var cuadro = Nuevo();
        cuadro.Datos.LongitudAlimentadorM = 100m;
        Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 3, 1, 150000m, continua: true);
        Carga(cuadro, 2, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        Carga(cuadro, 8, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        cuadro.Datos.NeutroReducido220_61 = true;
        cuadro.Recalcular();

        var r = cuadro.Alimentador.Resultado!;
        Assert.True(r.CaidaTensionPct <= cuadro.Datos.CaidaMaxAlimentadorPct);
        Assert.True(r.CalibreNeutro.AreaMm2 < r.CalibreFase.AreaMm2);
        Assert.Contains(r.Citas, x => x.Referencia == "220-61" && x.Descripcion.Contains("para que la caída de tensión"));
    }

    /// <summary>El piso del neutro es la tierra de 250-122 por 215-2(a)(2), no un criterio del proyectista — R3-3.</summary>
    [Fact]
    public void R3_3_ElPisoDelNeutroCita215_2a2()
    {
        var cuadro = Nuevo();
        Carga(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 3, 1, 150000m, continua: true);
        Carga(cuadro, 2, CategoriaDeCarga.Equipo, SubtipoDeCarga.OtraCargaEspecifica, 1, 1, 3000m, continua: false);
        cuadro.Datos.NeutroReducido220_61 = true;
        cuadro.Recalcular();

        var cita = Assert.Single(cuadro.Alimentador.Resultado!.Citas, x => x.Referencia == "220-61");
        Assert.Contains("250-122 — 215-2(a)(2)", cita.Descripcion);
        Assert.DoesNotContain("criterio del proyectista", cita.Descripcion);
    }
}
