using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>«Pendiente de probar»</b>, de la auditoría NOM del 2026-09-29: lo que la auditoría no alcanzó a
/// correr. Cada escenario se recalculó a mano con la NOM (NOM-001-SEDE-2012) antes de quedar aquí; los
/// números son los de ese cálculo. Condiciones base: cobre THHN, terminales de 60 °C hasta 100 A, lugar
/// seco, 30 °C, centro de carga NEMA, 20 m, EMT.
///
/// <para>
/// De la misma revisión salieron I-154 a I-157: las pruebas con ese prefijo fallaban antes de su
/// corrección.
/// </para>
/// </summary>
public class PendientesDeProbar20260930Tests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    private static CuadroDeCarga Nuevo(int fases = 3, int hilos = 4, decimal tension = 220m, int espacios = 12)
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = espacios;
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

    /// <summary>Un motor solo, en la línea del renglón: HP de la tabla.</summary>
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

    /// <summary>Un motor capturado como línea del desplegable: un grupo de un solo motor.</summary>
    private static CircuitoDelCuadro MotorEnLinea(CuadroDeCarga cuadro, int espacio, decimal hp, int polos, int cantidad = 1)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.Motor;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        var m = c.AgregarCarga();
        m.Subtipo = SubtipoDeCarga.MotorUsoGeneral;
        m.Hp = hp;
        m.Cantidad = cantidad;
        cuadro.Recalcular();
        return c;
    }

    private static CircuitoDelCuadro AireNominal(CuadroDeCarga cuadro, int espacio, decimal rla, int polos = 2)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        c.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
        c.CorrientePlacaA = rla;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        cuadro.Recalcular();
        return c;
    }

    private static CircuitoDelCuadro AirePorPlaca(CuadroDeCarga cuadro, int espacio, decimal mca, decimal mop, int polos)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        c.PlacaAire = PlacaDeAireAcondicionado.AmpacidadYProteccion;
        c.AmpacidadMinimaA = mca;
        c.ProteccionMaximaA = mop;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        cuadro.Recalcular();
        return c;
    }

    private static CanalizacionDelTablero UnTubo(CuadroDeCarga cuadro, params int[] espacios)
    {
        var t = cuadro.Datos.NuevaCanalizacion();
        foreach (var e in espacios)
            Espacio(cuadro, e).Canalizacion = t.Id;
        cuadro.Recalcular();
        return t;
    }

    private static string Calibre(CircuitoDelCuadro c) => c.Resultado!.CalibreFase.Designacion;

    private static ResultadoAlimentador Alimentador(CuadroDeCarga cuadro) => cuadro.Alimentador.Resultado!;

    private static decimal Capacidad(CuadroDeCarga cuadro) => Alimentador(cuadro).Detalle!.CapacidadMinimaA;

    // ---- Sistemas monofásicos ----------------------------------------------------------------------

    [Fact]
    public void Monofasico1F2H_ElAlimentadorSubeA8PorCaida()
    {
        // 1000 VA continuos: 7.87 A → 15 A, 14 AWG, 2.30 %. 1440 VA: 11.34 A → 15 A, 12 AWG por caída.
        // Alimentador: 1.25 × 7.87 + 11.34 = 21.18 A → 25 A; 10 AWG da 2.18 % > 2 %: 8 AWG.
        var cuadro = Nuevo(1, 2, 127m, 8);
        var alumbrado = Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 10, 100m, continua: true);
        var contactos = Circuito(cuadro, 2, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 8, 180m);

        Assert.Equal(7.87m, alumbrado.Resultado!.CorrienteDisenoA, 2);
        Assert.Equal(15m, alumbrado.Resultado.ProteccionA);
        Assert.Equal(2.30m, alumbrado.Resultado.CaidaTensionPct, 2);
        Assert.Equal("12", Calibre(contactos));
        Assert.Equal(21.18m, Capacidad(cuadro), 2);
        Assert.Equal(25m, cuadro.InterruptorPrincipalA);
        Assert.Equal("8", Alimentador(cuadro).CalibreFase.Designacion);
    }

    [Fact]
    public void Monofasico1F3H_ElNeutroSumaSoloLasCargasFaseNeutro()
    {
        // 1000 VA + 1440 VA en la misma fase a 120 V: 8.33 + 12 = 20.33 A por el neutro. El calentador de
        // 4500 W entre fases (240 V) no pasa por él.
        var cuadro = Nuevo(1, 3, 240m);
        Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 10, 100m, continua: true);
        Circuito(cuadro, 2, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 8, 180m);
        var calentador = Circuito(cuadro, 3, CategoriaDeCarga.Equipo, SubtipoDeCarga.CalentadorDeAgua, 1, 4500m, polos: 2, unidad: UnidadConsumo.Watts);

        Assert.Equal(20.33m, Alimentador(cuadro).CorrienteNeutro!.Value.Magnitud, 2);
        // 4500 W / 0.9 = 5000 VA / 240 = 20.83 A, continuo por 422-13: 26.04 A → 30 A, 10 AWG.
        Assert.Equal(26.04m, calentador.Resultado!.Detalle!.CapacidadMinimaA, 2);
        Assert.Equal(30m, calentador.Resultado.ProteccionA);
    }

    [Fact]
    public void Monofasico1F3H_ConUnaSolaFaseCargadaLaOtraSubePorElNeutro()
    {
        // 2000 VA solo en la fase A: el neutro regresa toda la corriente y levanta la tensión de B
        // (corrimiento del neutro). La caída de B sale negativa: la fase A es la que gobierna.
        var cuadro = Nuevo(1, 3, 240m);
        Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 20, 100m);

        var porFase = Alimentador(cuadro).CaidaPorFase!;
        Assert.True(porFase.Single(f => f.Fase == 'B').CaidaPct < 0m);
        Assert.True(porFase.Single(f => f.Fase == 'A').CaidaPct > 0m);
    }

    [Fact]
    public void Motor1HpA120YA240V()
    {
        // Tabla 430-248, 1 HP: 16 A a 115 V y 8 A a 230 V. 430-52: 250 % → 40 A y 20 A.
        var cuadro = Nuevo(1, 3, 240m);
        var a120 = MotorEnLinea(cuadro, 1, 1m, 1);
        var a240 = MotorEnLinea(cuadro, 3, 1m, 2);
        // El máximo de la tabla; desde M-20, 1 HP nace en automático (prioridad al conductor).
        a120.CriterioProteccion = a240.CriterioProteccion = CriterioDeProteccion.Maximo430_52;
        cuadro.Recalcular();

        Assert.Equal(16m, a120.Resultado!.CorrienteDisenoA);
        Assert.Equal(40m, a120.Resultado.ProteccionA);
        Assert.Equal(8m, a240.Resultado!.CorrienteDisenoA);
        Assert.Equal(20m, a240.Resultado.ProteccionA);
    }

    // ---- Tablero alimentado y A/A en un alimentador ------------------------------------------------

    [Fact]
    public void TableroAlimentado_ContinuaAl125()
    {
        // 20 kVA continuos y 10 kVA no continuos a 220 V, 3 fases: 52.49 × 1.25 + 26.24 = 91.85 A → 100 A.
        // 2 AWG (95 A a 60 °C) con 100 A por 240-4(b); tierra de 8 AWG (Tabla 250-122, 100 A).
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Tablero;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        var t = c.AgregarCarga();
        t.Subtipo = SubtipoDeCarga.TableroAlimentado;
        t.CargaUnitaria = 20000m;
        t.NoContinua = 10000m;
        cuadro.Recalcular();

        Assert.Equal(91.85m, c.Resultado!.Detalle!.CapacidadMinimaA, 2);
        Assert.Equal(100m, c.Resultado.ProteccionA);
        Assert.Equal("2", Calibre(c));
        Assert.Equal("8", c.Resultado.CalibreTierra.Designacion);
    }

    [Fact]
    public void TresAiresEnUnAlimentador_440_33()
    {
        // 16, 12 y 8 A de corriente nominal: 175 % → 25, 20 y 15 A (440-22(a), mínimo 15 A).
        // 440-33 en la fase más cargada: la suma más el 25 % del mayor, 32 A → 35 A, 8 AWG.
        var cuadro = Nuevo();
        var a = AireNominal(cuadro, 1, 16m);
        var b = AireNominal(cuadro, 5, 12m);
        var c = AireNominal(cuadro, 9, 8m);

        Assert.Equal(25m, a.Resultado!.ProteccionA);
        Assert.Equal(20m, b.Resultado!.ProteccionA);
        Assert.Equal(15m, c.Resultado!.ProteccionA);
        Assert.Equal(32m, Capacidad(cuadro), 2);
        Assert.Equal(35m, cuadro.InterruptorPrincipalA);
        Assert.Equal("8", Alimentador(cuadro).CalibreFase.Designacion);
    }

    [Fact]
    public void AireConPlacaMasAireYMotor()
    {
        // MCA 25 A (3 polos) + A/A de 16 A (2 polos, A-B) + motor de 5 HP (15.2 A): en la fase A,
        // 25 + 16 + 15.2 + 25 % × 16 = 60.20 A → 70 A, 4 AWG. La MCA ya trae el 25 % de su motor (440-4(b)).
        var cuadro = Nuevo();
        AirePorPlaca(cuadro, 1, 25m, 40m, 3);
        AireNominal(cuadro, 2, 16m);
        MotorEnLinea(cuadro, 7, 5m, 3);

        Assert.Equal(60.20m, Capacidad(cuadro), 2);
        Assert.Equal(70m, cuadro.InterruptorPrincipalA);
        Assert.Equal("4", Alimentador(cuadro).CalibreFase.Designacion);
    }

    // ---- Canalizaciones ----------------------------------------------------------------------------

    [Fact]
    public void TuboDePvc_UsaLaColumnaDePvcDeLaTabla9()
    {
        // 12 AWG en PVC: R = 6.6 Ω/km, X = 0.177 Ω/km (en acero, 0.223).
        var cuadro = Nuevo();
        foreach (var e in new[] { 1, 3, 5 })
            Circuito(cuadro, e, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 8, 180m);
        var tubo = UnTubo(cuadro, 1, 3, 5);
        tubo.Tubo = TipoTuboConduit.PvcCedula40;
        cuadro.Recalcular();

        Assert.Equal(0.177m, Espacio(cuadro, 1).Resultado!.Detalle!.ReactanciaOhmKm);
        Assert.Equal(6, tubo.Conteo!.Portadores);
    }

    [Theory]
    [InlineData(TipoCanalizacion.TuboConduit, true, 40)]
    [InlineData(TipoCanalizacion.Niple, false, 60)]
    [InlineData(TipoCanalizacion.DuctoMetalico, false, 20)]
    public void DiezPortadores_SegunLaCanalizacion(TipoCanalizacion tipo, bool ajusta, decimal llenado)
    {
        // 5 circuitos de 1 polo con neutro: 10 portadores. En tubo, Tabla 310-15(b)(3)(a); en niple de 60 cm
        // o menos, sin ajuste — 310-15(b)(3)(a)(2), y 60 % — Capítulo 10, Nota 4; en ducto metálico, sin
        // ajuste hasta 30 portadores — 376-22(b), y 20 % — 376-22(a).
        var cuadro = Nuevo();
        foreach (var e in new[] { 1, 3, 5, 7, 9 })
            Circuito(cuadro, e, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 10, 150m, continua: true);
        var canal = UnTubo(cuadro, 1, 3, 5, 7, 9);
        canal.Tipo = tipo;
        if (tipo == TipoCanalizacion.DuctoMetalico)
        {
            canal.AnchoMm = 65;
            canal.AltoMm = 65;
        }
        cuadro.Recalcular();

        Assert.Equal(10, canal.Conteo!.Portadores);
        Assert.Equal(ajusta, canal.Ajuste!.Aplica);
        Assert.Equal(llenado, canal.Ocupacion!.PorcentajePermitido);
    }

    [Fact]
    public void Multiconductor_ElNeutroCompartidoNoCuentaSalvoConCargaNoLineal()
    {
        // Tres circuitos de 1 polo en A, B y C con un neutro: el neutro solo lleva el desbalance —
        // 310-15(b)(5)(1): 3 portadores. Con carga mayormente no lineal cuenta — (b)(5)(3): 4.
        // Tierra común: un solo conductor para los tres (250-122(c)); 5 conductores en el tubo.
        var cuadro = Nuevo();
        foreach (var e in new[] { 1, 3, 5 })
            Circuito(cuadro, e, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 10, 150m, continua: true);
        var tubo = UnTubo(cuadro, 1, 3, 5);
        tubo.NeutroCompartido = true;
        tubo.TierraComun = true;
        cuadro.Recalcular();

        Assert.Equal(3, tubo.Conteo!.Portadores);
        Assert.Equal(5, tubo.Ocupacion!.NumeroConductores);
        Assert.Contains(tubo.Avisos, a => a.Contains("210-4(b)"));

        cuadro.Datos.CargaNoLineal = true;
        cuadro.Recalcular();
        Assert.Equal(4, tubo.Conteo!.Portadores);
    }

    [Fact]
    public void TierraDesnudaYAzotea()
    {
        // La tierra desnuda toma su área de la Tabla 8 (12 AWG: 4.25 mm²), no de la Tabla 5.
        // A 20 mm del techo, al sol: +22 °C — Tabla 310-15(b)(3)(c).
        var cuadro = Nuevo();
        foreach (var e in new[] { 1, 3 })
            Circuito(cuadro, e, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 8, 180m);
        var tubo = UnTubo(cuadro, 1, 3);
        tubo.TierraDesnuda = true;
        tubo.AlturaSobreTechoMm = 20m;
        cuadro.Recalcular();

        Assert.Equal(22m, tubo.SumadorAzoteaC);
        var tierras = tubo.Ocupacion!.Renglones.Where(r => r.Conductor.Papel == PapelConductor.Tierra).ToList();
        Assert.Equal(2, tierras.Count);
        Assert.All(tierras, r => Assert.Equal(4.25m, r.AreaUnitariaMm2));
    }

    // ---- Vivienda ----------------------------------------------------------------------------------

    [Fact]
    public void ViviendaPopular55m2()
    {
        // 1F-2H 127 V: 800 + 1800 + 720 + 180 VA = 3500 VA → 27.56 A → 30 A (y 30 A de 230-79(c)).
        // 10 AWG da 3.1 % en 20 m: 6 AWG (1.34 %). En vivienda popular no hay 220-52 ni 210-11(c).
        var cuadro = Nuevo(1, 2, 127m, 8);
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaPopular;
        cuadro.Datos.AreaServidaM2 = 55m;
        cuadro.Datos.EsEquipoDeAcometida = true;
        Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 8, 100m);
        Circuito(cuadro, 2, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 10, 180m);
        var cocina = Circuito(cuadro, 3, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoAparatosPequenos, 4, 180m);
        var bano = Circuito(cuadro, 4, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoBano, 1, 180m);

        Assert.Equal(27.56m, Alimentador(cuadro).CorrienteDisenoA, 2);
        Assert.Equal(30m, cuadro.InterruptorPrincipalA);
        Assert.Equal("6", Alimentador(cuadro).CalibreFase.Designacion);
        Assert.Equal(0m, cuadro.Resumen.Minimo220_52VA);
        Assert.Equal(0m, cuadro.Resumen.Superficie!.AjusteAlumbradoVA);
        Assert.Equal(15m, cocina.Resultado!.ProteccionA);
        Assert.Equal(15m, bano.Resultado!.ProteccionA);
        Assert.Contains(bano.ReglasDeClase, r => r.Referencia == "210-8(a)(1)" && r.Texto.Contains("vivienda popular"));
    }

    [Fact]
    public void ViviendaMediaConBomba()
    {
        // 2F-3H 220/127 V, 120 m². Fase A: continua 22.73 (calentador), no continua 11.81 + 11.81 + 22.73
        // + 11.81 = 58.16 (alumbrado, aparatos pequeños y lavadora con 220-52, secadora), bomba 8.90.
        // Capacidad: 1.25 × 22.73 + 58.16 + 1.25 × 8.90 = 97.69 A → 100 A, 1 AWG (60 °C), tierra 8 AWG.
        var cuadro = Nuevo(2, 3, 220m, 16);
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        cuadro.Datos.AreaServidaM2 = 120m;
        cuadro.Datos.EsEquipoDeAcometida = true;
        Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 15, 100m);
        Circuito(cuadro, 3, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 12, 180m);
        Circuito(cuadro, 5, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoAparatosPequenos, 4, 180m);
        Circuito(cuadro, 7, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoAparatosPequenos, 4, 180m);
        var lavadora = Circuito(cuadro, 9, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoLavadora, 1, 500m);
        Circuito(cuadro, 11, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoBano, 2, 180m);
        var calentador = Circuito(cuadro, 2, CategoriaDeCarga.Equipo, SubtipoDeCarga.CalentadorDeAgua, 1, 4500m, polos: 2, unidad: UnidadConsumo.Watts);
        var secadora = Circuito(cuadro, 6, CategoriaDeCarga.Equipo, SubtipoDeCarga.Secadora, 1, 3000m, polos: 2, unidad: UnidadConsumo.Watts);
        var bomba = MotorEnLinea(cuadro, 10, 0.5m, 1);

        // 220-54: 5000 VA aunque la placa diga 3000 W → 22.73 A → 25 A, 10 AWG.
        Assert.Equal(5000m, secadora.CargaInstaladaVA);
        Assert.Equal(25m, secadora.Resultado!.ProteccionA);
        Assert.Equal(30m, calentador.Resultado!.ProteccionA);
        Assert.Equal(20m, lavadora.Resultado!.ProteccionA);
        // Tabla 430-248, ½ HP a 127 V: 8.9 A; 250 % = 22.25 → 25 A de máximo. Desde M-20 la bomba nace en
        // automático: 1 HP o menos, prioridad al conductor — 15 A sobre 14 AWG, el caso de David.
        Assert.Equal(8.9m, bomba.Resultado!.CorrienteDisenoA);
        Assert.Equal(15m, bomba.Resultado.ProteccionA);
        Assert.Equal(25m, bomba.Resultado.RangoMotor!.MaximoA);

        // 220-52: 780 + 780 + 1000 VA. 220-12: 33 × 120 = 3960 VA contra 1500 + 2160 + 360 capturados.
        Assert.Equal(2560m, cuadro.Resumen.Minimo220_52VA);
        Assert.Equal(0m, cuadro.Resumen.Superficie!.AjusteAlumbradoVA);
        Assert.Equal(97.69m, Capacidad(cuadro), 2);
        Assert.Equal(100m, cuadro.InterruptorPrincipalA);
        Assert.Equal("1", Alimentador(cuadro).CalibreFase.Designacion);
        Assert.Equal("8", Alimentador(cuadro).CalibreTierra.Designacion);
        Assert.Null(cuadro.Datos.Minimo230_79); // 230-79(c): según la carga
    }

    [Fact]
    public void ViviendaGrandeConMinisplits()
    {
        // 3F-4H, 250 m². Estufa de 12 kW (F.P. 0.9): 13 333 VA, 60.61 A → 70 A, 4 AWG. Minisplits con placa
        // MCA 10 / MOP 15: 15 A, 14 AWG. 220-12: 8250 − 6360 = 1890 VA, 630 por barra. Fase A: 9.45 + 14.17
        // + 11.81 + 60.61 + 4.96 = 100.99 no continua; A/A y bomba 28 A y 25 % de la bomba: 130.99 A → 150 A,
        // 1/0 AWG (75 °C arriba de 100 A), tierra 6 AWG.
        var cuadro = Nuevo(3, 4, 220m, 30);
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        cuadro.Datos.AreaServidaM2 = 250m;
        cuadro.Datos.EsEquipoDeAcometida = true;
        Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 12, 100m);
        Circuito(cuadro, 3, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 12, 100m);
        Circuito(cuadro, 5, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 10, 180m);
        Circuito(cuadro, 7, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 10, 180m);
        Circuito(cuadro, 9, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoAparatosPequenos, 3, 180m);
        Circuito(cuadro, 11, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoAparatosPequenos, 3, 180m);
        Circuito(cuadro, 13, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoLavadora, 1, 180m);
        Circuito(cuadro, 15, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoBano, 2, 180m);
        var estufa = Circuito(cuadro, 2, CategoriaDeCarga.Equipo, SubtipoDeCarga.Coccion, 1, 12000m, polos: 2, unidad: UnidadConsumo.Watts);
        var minisplits = new[] { 17, 21, 25 }.Select(e => AirePorPlaca(cuadro, e, 10m, 15m, 2)).ToList();
        MotorEnLinea(cuadro, 6, 1m, 2);

        Assert.Equal(60.61m, estufa.Resultado!.CorrienteDisenoA, 2);
        Assert.Equal(70m, estufa.Resultado.ProteccionA);
        Assert.Equal("4", Calibre(estufa));
        Assert.All(minisplits, m => Assert.Equal(15m, m.Resultado!.ProteccionA));
        Assert.All(minisplits, m => Assert.Equal("14", Calibre(m)));
        Assert.Equal(1890m, cuadro.Resumen.Superficie!.AjusteAlumbradoVA);
        Assert.Equal(3240m, cuadro.Resumen.Minimo220_52VA);
        Assert.Equal(130.99m, Capacidad(cuadro), 2);
        Assert.Equal(150m, cuadro.InterruptorPrincipalA);
        Assert.Equal("1/0", Alimentador(cuadro).CalibreFase.Designacion);
        Assert.Equal("6", Alimentador(cuadro).CalibreTierra.Designacion);
    }

    // ---- Comercio ----------------------------------------------------------------------------------

    private static CuadroDeCarga Restaurante()
    {
        // 150 m², 220/127 V: alumbrado 2 × 2000 VA continuo, contactos 2 × 1440 VA, seis equipos de cocina
        // de 3 polos (8, 6, 5, 4, 3 y 2 kW, F.P. 0.9), campana de 1 HP y A/A con MCA 35 / MOP 50.
        var cuadro = Nuevo(3, 4, 220m, 42);
        cuadro.Datos.Inmueble = TipoDeInmueble.Restaurante;
        cuadro.Datos.AreaServidaM2 = 150m;
        cuadro.Datos.EsEquipoDeAcometida = true;
        Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 20, 100m, continua: true);
        Circuito(cuadro, 3, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 20, 100m, continua: true);
        Circuito(cuadro, 5, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 8, 180m);
        Circuito(cuadro, 7, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 8, 180m);
        var kw = new[] { 8000m, 6000m, 5000m, 4000m, 3000m, 2000m };
        for (var i = 0; i < kw.Length; i++)
            Circuito(cuadro, 2 + 6 * i, CategoriaDeCarga.Equipo, SubtipoDeCarga.CocinaComercial, 1, kw[i], polos: 3, unidad: UnidadConsumo.Watts);
        MotorEnLinea(cuadro, 9, 1m, 3);
        AirePorPlaca(cuadro, 15, 35m, 50m, 3);
        return cuadro;
    }

    [Fact]
    public void Restaurante_SinFactorDeDemanda()
    {
        // Fase A: 15.75 continua; 11.34 + 81.65 (cocina) no continua; 4.2 (Tabla 430-250, 1 HP a 230 V) + 35
        // (MCA). Capacidad: 19.69 + 92.98 + 39.2 + 25 % × 4.2 = 152.91 A → 175 A, 2/0 AWG, tierra 6 AWG.
        var cuadro = Restaurante();

        Assert.Equal(25m, Espacio(cuadro, 2).Resultado!.ProteccionA);
        Assert.Equal(50m, Espacio(cuadro, 15).Resultado!.ProteccionA);
        Assert.Equal(152.91m, Capacidad(cuadro), 2);
        Assert.Equal(175m, cuadro.InterruptorPrincipalA);
        Assert.Equal("2/0", Alimentador(cuadro).CalibreFase.Designacion);
        Assert.Equal(0m, cuadro.Resumen.Superficie!.AjusteAlumbradoVA); // 22 × 150 = 3300 ≤ 4000
    }

    [Fact]
    public void Restaurante_CocinaConLaTabla220_56()
    {
        // Seis equipos o más: 65 %. 31 111 × 0.65 = 20 222 VA, más que los dos mayores (8889 + 6667).
        // Fase A: 19.69 + 11.34 + 0.65 × 81.65 + 40.25 = 124.34 A → 125 A, 1 AWG. Sin aviso de 220-56.
        var cuadro = Restaurante();
        cuadro.Datos.CambiarFactorDeDemanda(CategoriaDeCarga.Equipo, 0.65m);
        cuadro.Datos.Justificaciones[CategoriaDeCarga.Equipo].Add(JustificacionFactorDemanda.CocinaComercial);
        cuadro.Recalcular();

        Assert.Equal(20222m, cuadro.Resumen.PorCategoria!.Single(k => k.Categoria == CategoriaDeCarga.Equipo).DemandadaVA, 0);
        Assert.Equal(124.34m, Capacidad(cuadro), 2);
        Assert.Equal(125m, cuadro.InterruptorPrincipalA);
        Assert.Equal("1", Alimentador(cuadro).CalibreFase.Designacion);
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("220-56"));
    }

    [Fact]
    public void Taller_ConTresMotores()
    {
        // 200 m² a 22 VA/m² = 4400 VA contra 2000 capturados: 2400 VA continuos, 6.30 A por barra.
        // Fase A: 1.25 × (15.75 + 6.30) + 15.2 + 9.6 + 6.8 + 25 % × 15.2 = 62.96 A → 70 A, 4 AWG.
        var cuadro = Nuevo(3, 4, 220m, 24);
        cuadro.Datos.AreaServidaM2 = 200m;
        cuadro.Datos.UsoTabla220_12 = "Edificios industriales";
        cuadro.Datos.EsEquipoDeAcometida = true;
        Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 20, 100m, continua: true);
        Circuito(cuadro, 3, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 8, 180m);
        var m5 = MotorEnLinea(cuadro, 2, 5m, 3);
        MotorEnLinea(cuadro, 8, 3m, 3);
        MotorEnLinea(cuadro, 14, 2m, 3);

        Assert.Equal(40m, m5.Resultado!.ProteccionA);
        Assert.Equal(2400m, cuadro.Resumen.Superficie!.AjusteAlumbradoVA);
        Assert.Equal(62.96m, Capacidad(cuadro), 2);
        Assert.Equal(70m, cuadro.InterruptorPrincipalA);
        Assert.Equal("4", Alimentador(cuadro).CalibreFase.Designacion);
    }

    // ---- Serie «NOM completa» ----------------------------------------------------------------------

    [Theory]
    [InlineData(SerieDeInterruptores.CentroDeCargaNema, 35, 35, 70)]
    [InlineData(SerieDeInterruptores.NomCompleta, 32, 32, 63)]
    [InlineData(SerieDeInterruptores.RielDinIec, 32, 32, 63)]
    public void Serie_32Y63A(SerieDeInterruptores serie, int noContinua, int continua, int tresPolos)
    {
        // 30.5 A no continuos; 25 A continuos (31.25 A); 61 A en 3 polos. 240-6(a) trae 32 y 63 A; en
        // centro de carga no existen.
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = serie;
        var a = Circuito(cuadro, 1, CategoriaDeCarga.Equipo, SubtipoDeCarga.AparatoFijo, 1, 30.5m * 220m, polos: 2, longitudM: 5m);
        var b = Circuito(cuadro, 5, CategoriaDeCarga.Equipo, SubtipoDeCarga.AparatoFijo, 1, 25m * 220m, continua: true, polos: 2, longitudM: 5m);
        var c = Circuito(cuadro, 2, CategoriaDeCarga.Equipo, SubtipoDeCarga.AparatoFijo, 1, 61m * 220m * 1.7320508m, polos: 3, longitudM: 5m);

        Assert.Equal(noContinua, a.Resultado!.ProteccionA);
        Assert.Equal(continua, b.Resultado!.ProteccionA);
        Assert.Equal(tresPolos, c.Resultado!.ProteccionA);
        Assert.Equal("8", Calibre(a));
        Assert.Equal("4", Calibre(c));

        // Con terminales de 75 °C, 10 AWG (35 A) alcanza los 30.5 A, pero 240-4(d) no lo deja pasar de
        // 30 A: sigue en 8 AWG con 32 o 35 A. Con 61 A, 6 AWG (65 A) sí: 63 A, o 70 A por 240-4(b).
        cuadro.Datos.TerminalesMarcadas75C = true;
        cuadro.Recalcular();
        Assert.Equal("8", Calibre(a));
        Assert.Equal("6", Calibre(c));
    }

    // ---- Servicio de los motores — Tabla 430-22(e) -------------------------------------------------

    [Theory]
    [InlineData(ServicioDeMotor.Intermitente, EspecificacionDeTiempo.Minutos15, 22.95, "10")]
    [InlineData(ServicioDeMotor.Periodico, EspecificacionDeTiempo.Minutos15, 24.30, "10")]
    [InlineData(ServicioDeMotor.Variable, EspecificacionDeTiempo.Continuo, 54.00, "6")]
    [InlineData(ServicioDeMotor.CortaDuracion, EspecificacionDeTiempo.Minutos5, 29.70, "10")]
    [InlineData(ServicioDeMotor.CortaDuracion, EspecificacionDeTiempo.Minutos30y60, 40.50, "6")]
    public void ServicioNoContinuo_ElPorcentajeDeLaTablaSobreLaPlaca(
        ServicioDeMotor servicio, EspecificacionDeTiempo tiempo, decimal capacidad, string calibre)
    {
        // Motor de 10 HP con 27 A de placa: 85 %, 90 %, 200 %, 110 % y 150 %. La protección sigue en 430-52
        // con la FLC de tabla: 250 % × 28 = 70 A.
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 10m, 3);
        c.Servicio = servicio;
        c.EspecificacionServicio = tiempo;
        c.CorrientePlacaServicioA = 27m;
        cuadro.Recalcular();

        Assert.Null(c.Error);
        Assert.Equal(capacidad, c.Resultado!.Detalle!.CapacidadMinimaA, 2);
        Assert.Equal(calibre, Calibre(c));
        Assert.Equal(70m, c.Resultado.ProteccionA);
        Assert.Equal(capacidad, c.CorrienteDeServicioA, 2);
    }

    [Fact]
    public void ServicioNoContinuo_ElAlimentadorConLaExcepcion1De430_24()
    {
        // 430-24 Excepción 1: el motor intermitente entra con su 22.95 A; el continuo de 5 HP con su 125 %.
        // 22.95 + 15.2 + 3.8 = 41.95 A → 45 A.
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 10m, 3);
        c.Servicio = ServicioDeMotor.Intermitente;
        c.EspecificacionServicio = EspecificacionDeTiempo.Minutos15;
        c.CorrientePlacaServicioA = 27m;
        ConMotor(cuadro, 2, 5m, 3);

        Assert.Equal(41.95m, Capacidad(cuadro), 2);
        Assert.Equal(45m, cuadro.InterruptorPrincipalA);
    }

    // ---- Hallazgos ---------------------------------------------------------------------------------

    [Fact]
    public void I154_ElAvisoDelPrincipalCitaLaReglaConQueSeCalculoElMotor()
    {
        // Un motor de 10 HP capturado en el desplegable es un grupo de un solo motor, pero se calcula por
        // 430-52 (70 A). El principal (35 A por 430-24) queda abajo: el aviso citaba 430-53(c)(4).
        var cuadro = Nuevo();
        MotorEnLinea(cuadro, 1, 10m, 3);

        var aviso = Assert.Single(cuadro.Alimentador.Avisos, a => a.Contains("dimensiona para el arranque"));
        Assert.Contains("que 430-52 dimensiona", aviso);
        Assert.DoesNotContain("430-53(c)(4)", aviso);

        // Dos motores en el circuito sí son grupo: 430-53(c)(4).
        var grupo = Nuevo();
        MotorEnLinea(grupo, 1, 10m, 3, cantidad: 2);
        Assert.Contains(grupo.Alimentador.Avisos, a => a.Contains("que 430-53(c)(4) dimensiona"));
    }

    private static CuadroDeCarga CocinaDeTresEquipos(decimal factor)
    {
        var cuadro = Nuevo(3, 4, 220m, 24);
        cuadro.Datos.Inmueble = TipoDeInmueble.Restaurante;
        Circuito(cuadro, 2, CategoriaDeCarga.Equipo, SubtipoDeCarga.CocinaComercial, 1, 8000m, polos: 3, unidad: UnidadConsumo.Watts, fp: 1m);
        Circuito(cuadro, 8, CategoriaDeCarga.Equipo, SubtipoDeCarga.CocinaComercial, 1, 6000m, polos: 3, unidad: UnidadConsumo.Watts, fp: 1m);
        Circuito(cuadro, 14, CategoriaDeCarga.Equipo, SubtipoDeCarga.CocinaComercial, 1, 1000m, polos: 3, unidad: UnidadConsumo.Watts, fp: 1m);
        cuadro.Datos.CambiarFactorDeDemanda(CategoriaDeCarga.Equipo, factor);
        cuadro.Datos.Justificaciones[CategoriaDeCarga.Equipo].Add(JustificacionFactorDemanda.CocinaComercial);
        cuadro.Recalcular();
        return cuadro;
    }

    [Fact]
    public void I155_LaCocinaComercialNoBajaDeLosDosEquiposMasGrandes()
    {
        // Tres equipos, 90 % por la Tabla 220-56: 15 000 × 0.90 = 13 500 VA. 220-56: «en ningún caso» menos
        // que la suma de los dos equipos más grandes, 8000 + 6000 = 14 000 VA.
        var cuadro = CocinaDeTresEquipos(0.90m);

        var aviso = Assert.Single(cuadro.Alimentador.Avisos, a => a.Contains("220-56"));
        Assert.Contains("13,500", aviso);
        Assert.Contains("14,000", aviso);

        // Con 0.94, 14 100 VA: ya no.
        Assert.DoesNotContain(CocinaDeTresEquipos(0.94m).Alimentador.Avisos, a => a.Contains("220-56"));
    }

    [Fact]
    public void I156_ElCircuitoDelAnuncioEsDe20AYContinuo()
    {
        // 600-5(a): la salida para anuncios va en un circuito de cuando menos 20 A que no alimenta otras
        // cargas; 600-5(b): es carga continua. 400 VA → 1200 VA (220-14(f)), continuos: 9.45 A × 1.25 → 20 A.
        var cuadro = Nuevo();
        var anuncio = Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Anuncios, 1, 400m);

        Assert.Equal(1200m, anuncio.ContinuaVA);
        Assert.Equal(0m, anuncio.NoContinuaVA);
        Assert.Equal("600-5(b)", anuncio.Cargas.Single().ReferenciaContinua);
        Assert.Equal(20m, anuncio.Resultado!.ProteccionA);
        Assert.Equal("12", Calibre(anuncio));
    }

    [Fact]
    public void I156_ElAnuncioConOtrasCargasOMasDe20AAvisa()
    {
        // Con alumbrado en el mismo circuito: 600-5(a). 3000 VA continuos: 23.62 A × 1.25 → 30 A, arriba de
        // los 20 A de 600-5(b)(2) (30 A solo en neón, (b)(1)).
        var cuadro = Nuevo();
        var mezclado = Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Anuncios, 1, 400m);
        var luminarias = mezclado.AgregarCarga();
        luminarias.Subtipo = SubtipoDeCarga.Luminarias;
        luminarias.CargaUnitaria = 300m;
        var grande = Circuito(cuadro, 3, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Anuncios, 1, 3000m);
        cuadro.Recalcular();

        Assert.Contains(mezclado.ReglasDeClase, r => r.Referencia == "600-5(a)" && r.Aviso);
        Assert.Equal(30m, grande.Resultado!.ProteccionA);
        Assert.Contains(grande.ReglasDeClase, r => r.Referencia == "600-5(b)(2)" && r.Aviso);
    }

    [Fact]
    public void I157_El220_12CuentaSoloElAlumbradoGeneral()
    {
        // Tienda de 100 m² × 33 VA/m² = 3300 VA. Anuncio (1200 VA, 220-14(f)), aparador (1500 VA, 220-14(g))
        // y portalámparas de trabajo pesado (600 VA, 220-14(e)) no son alumbrado general: con 1000 VA de
        // luminarias faltan 2300 VA.
        var cuadro = Nuevo();
        cuadro.Datos.AreaServidaM2 = 100m;
        cuadro.Datos.UsoTabla220_12 = "Tiendas";
        Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Anuncios, 1, 400m);
        Circuito(cuadro, 3, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Aparador, 1, 1500m);
        Circuito(cuadro, 5, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.PortalamparasPesado, 1, 600m);
        Circuito(cuadro, 7, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 10, 100m, continua: true);

        var sp = cuadro.Resumen.Superficie!;
        Assert.Equal(1000m, sp.AlumbradoCapturadoVA);
        Assert.Equal(2300m, sp.AjusteAlumbradoVA);
        Assert.Equal(3300m, sp.AlumbradoNoGeneralVA);

        // La memoria dice qué se dejó fuera.
        var renglon = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DelAlimentador(cuadro)!)
            .SelectMany(b => b.Renglones)
            .Single(r => r.Rotulo.StartsWith("Mínimo de alumbrado general"));
        Assert.Contains("sin 3,300 VA de anuncios, aparadores o portalámparas de trabajo pesado", renglon.Valor);
        Assert.Contains("se agregan 2,300 VA, continuos", renglon.Valor);
    }

    [Fact]
    public void Tienda_ConAnuncioYAire()
    {
        // 100 m² de tienda: 3300 VA de alumbrado general contra 2400 capturados (el anuncio no cuenta): 900 VA.
        // A/A de 12 A: 175 % = 21 → 20 A, 14 AWG (440-32, 15 A). Equipo de acometida: 60 A de 230-79(d),
        // 6 AWG (55 A a 60 °C, 240-4(b)).
        var cuadro = Nuevo(3, 4, 220m, 24);
        cuadro.Datos.AreaServidaM2 = 100m;
        cuadro.Datos.UsoTabla220_12 = "Tiendas";
        cuadro.Datos.EsEquipoDeAcometida = true;
        Circuito(cuadro, 1, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 12, 100m, continua: true);
        Circuito(cuadro, 3, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Luminarias, 12, 100m, continua: true);
        var anuncio = Circuito(cuadro, 5, CategoriaDeCarga.Alumbrado, SubtipoDeCarga.Anuncios, 1, 400m, continua: true);
        Circuito(cuadro, 7, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 6, 180m);
        Circuito(cuadro, 9, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoUsoGeneral, 6, 180m);
        var refrigerador = Circuito(cuadro, 11, CategoriaDeCarga.Contactos, SubtipoDeCarga.ContactoRefrigerador, 1, 600m);
        var aire = AireNominal(cuadro, 2, 12m);

        Assert.Equal(20m, anuncio.Resultado!.ProteccionA);
        Assert.Equal(20m, aire.Resultado!.ProteccionA);
        Assert.Equal("14", Calibre(aire));
        Assert.Contains(refrigerador.ReglasDeClase, r => r.Referencia == "210-21(b)(1)");
        Assert.Equal(900m, cuadro.Resumen.Superficie!.AjusteAlumbradoVA);
        Assert.Equal(60m, cuadro.InterruptorPrincipalA);
        Assert.Equal("6", Alimentador(cuadro).CalibreFase.Designacion);
    }
}
