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
}
