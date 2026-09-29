using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// Variador (I-119), servicio no continuo (I-120), cargas no simultáneas (I-121) y medio de desconexión
/// (I-122). Los números salen de la norma: 125 % de la entrada del variador (430-122(a)); Tabla
/// 430-22(e) (intermitente, 30 y 60 min: 90 %); 5 HP a 230 V: 15.2 A (Tabla 430-250); 115 % de la
/// corriente para el desconectador (430-110(a), 430-128).
/// </summary>
public class MotoresFase4Tests
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

    private static CircuitoDelCuadro Trifasico(CuadroDeCarga cuadro, int espacio)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.Motor;
        c.LongitudM = 10m;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        return c;
    }

    private static string Memoria(CuadroDeCarga cuadro, CircuitoDelCuadro c) =>
        string.Join("\n", MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, c))
            .SelectMany(s => s.Renglones).Select(r => $"{r.Rotulo}: {r.Valor}"));

    // ---- Variador — I-119 ------------------------------------------------------------------------

    [Fact]
    public void I119_ElVariadorVaConSuCorrienteDeEntradaYLaProteccionDelFabricante()
    {
        var cuadro = Nuevo();
        var c = Trifasico(cuadro, 1);
        c.CapturaMotor = CapturaDeMotor.Variador;
        c.CorrienteEntradaVariadorA = 20m;
        c.ProteccionMaximaVariadorA = 40m;
        cuadro.Recalcular();

        Assert.Null(c.Error);
        var r = c.Resultado!;
        // 430-122(a): 125 % × 20 = 25 A → 10 AWG (12 AWG da 20 A a 60 °C).
        Assert.Equal(25m, r.Detalle!.CapacidadMinimaA);
        Assert.Equal("10", r.CalibreFase.Designacion);
        // 110-3(b): la máxima del fabricante, 40 A. No el 250 % de la Tabla 430-52.
        Assert.Equal(40m, r.ProteccionA);
        Assert.Equal(20m, c.CorrienteDeMotorA);
        Assert.Contains(r.Citas, x => x.Referencia == "430-122(a)");
        Assert.Contains(cuadro.Desglose(c)!.Conductor, x => x.EndsWith("— 430-122(a)"));
        var memoria = Memoria(cuadro, c);
        Assert.Contains("Medio de desconexión — 430-128: 115 % × 20.00 A = 23.00 A", memoria);
        // En el alimentador, como un motor de 20 A.
        Assert.Equal(20m, cuadro.Alimentador.Fases!.Single(f => f.Fase == 'A').Motores.MayorFlcA);
    }

    [Fact]
    public void I119_SinProteccionMaximaONingunTamanoEntreLasDosNoCalcula()
    {
        var cuadro = Nuevo();
        var c = Trifasico(cuadro, 1);
        c.CapturaMotor = CapturaDeMotor.Variador;
        c.CorrienteEntradaVariadorA = 20m;
        cuadro.Recalcular();
        Assert.Contains("110-3(b)", c.Error);

        c.ProteccionMaximaVariadorA = 18m;
        cuadro.Recalcular();
        Assert.Contains("ningún tamaño estándar", c.Error);
    }

    // ---- Servicio no continuo — I-120 --------------------------------------------------------------

    [Fact]
    public void I120_LaTabla430_22eSeLeeDeLaNorma()
    {
        Assert.Equal(90m, Motor.ServicioMotor.Porcentaje(ServicioDeMotor.Intermitente, EspecificacionDeTiempo.Minutos30y60));
        Assert.Equal(200m, Motor.ServicioMotor.Porcentaje(ServicioDeMotor.Variable, EspecificacionDeTiempo.Continuo));
        Assert.Null(Motor.ServicioMotor.Porcentaje(ServicioDeMotor.CortaDuracion, EspecificacionDeTiempo.Continuo));
    }

    [Fact]
    public void I120_UnaBombaIntermitenteVaAlPorcentajeDeSuPlaca()
    {
        var cuadro = Nuevo();
        var c = Trifasico(cuadro, 1);
        c.Hp = 5m;
        c.Servicio = ServicioDeMotor.Intermitente;
        c.EspecificacionServicio = EspecificacionDeTiempo.Minutos30y60;
        c.CorrientePlacaServicioA = 14m;
        cuadro.Recalcular();

        Assert.Null(c.Error);
        var r = c.Resultado!;
        // 430-22(e): 90 % × 14 A (placa) = 12.6 A, en lugar de 125 % × 15.2 = 19 A.
        Assert.Equal(12.6m, r.Detalle!.CapacidadMinimaA);
        Assert.Equal("14", r.CalibreFase.Designacion);
        // La protección sigue en 430-52 con la FLC de tabla: 250 % × 15.2 = 38 → 40 A.
        Assert.Equal(40m, r.ProteccionA);
        Assert.Contains(r.Citas, x => x.Referencia == "430-22(e)");
        Assert.Contains("Capacidad mínima del conductor — 430-22(e)", Memoria(cuadro, c));
        // 430-24 Excepción 1: al alimentador con el valor de 430-22(e), sin el 125 %.
        var fase = cuadro.Alimentador.Fases!.Single(f => f.Fase == 'A');
        Assert.Equal(12.6m, fase.Motores.CapacidadMinimaA);
    }

    [Fact]
    public void I120_SinPlacaOCortaDuracionContinuaDiceQueFalta()
    {
        var cuadro = Nuevo();
        var c = Trifasico(cuadro, 1);
        c.Hp = 5m;
        c.Servicio = ServicioDeMotor.Intermitente;
        cuadro.Recalcular();
        Assert.Contains("corriente de placa", c.Error);

        c.CorrientePlacaServicioA = 14m;
        c.Servicio = ServicioDeMotor.CortaDuracion;
        c.EspecificacionServicio = EspecificacionDeTiempo.Continuo;
        cuadro.Recalcular();
        Assert.Contains("corta duración", c.Error);
    }

    // ---- No simultáneo — I-121 ---------------------------------------------------------------------

    [Fact]
    public void I121_DosBombasQueAlternanCuentanUnaEnElAlimentador()
    {
        var cuadro = Nuevo();
        var b1 = Trifasico(cuadro, 1);
        b1.Hp = 5m;
        var b2 = Trifasico(cuadro, 2);
        b2.Hp = 5m;
        cuadro.Recalcular();
        // Las dos: 125 % × 15.2 + 15.2 = 34.2 A.
        Assert.Equal(34.2m, cuadro.Alimentador.Resultado!.Detalle!.CapacidadMinimaA);

        b1.NoSimultaneoCon = 2;
        b2.NoSimultaneoCon = 1;
        cuadro.Recalcular();

        // Del par, una: 125 % × 15.2 = 19 A — 430-24 Excepción 3, 220-60.
        Assert.Equal(19m, cuadro.Alimentador.Resultado!.Detalle!.CapacidadMinimaA);
        Assert.True(b2.OmitidoPorNoSimultaneo);
        Assert.False(b1.OmitidoPorNoSimultaneo);
        Assert.Contains(cuadro.AvisosDeCircuitos, x => x.StartsWith("Circuito 2: no entra al alimentador") && x.Contains("430-24 Excepción 3"));
        // El derivado de cada una sigue igual.
        Assert.Equal(40m, b2.Resultado!.ProteccionA);
    }

    [Fact]
    public void I121_ElParSigueAlCircuitoQueSeMueveYSeGuarda()
    {
        var cuadro = Nuevo();
        var calefaccion = Espacio(cuadro, 1);
        calefaccion.Categoria = CategoriaDeCarga.CalefaccionFija;
        calefaccion.Continua = 3000m;
        var aire = Espacio(cuadro, 3);
        aire.Categoria = CategoriaDeCarga.AireAcondicionado;
        aire.AmpacidadMinimaA = 10m;
        aire.ProteccionMaximaA = 15m;
        calefaccion.NoSimultaneoCon = 3;
        aire.NoSimultaneoCon = 1;
        cuadro.Recalcular();
        Assert.True(aire.OmitidoPorNoSimultaneo); // 1270 VA contra 3000 VA

        Assert.True(cuadro.MoverCircuito(3, 7).Movio);
        Assert.Equal(7, Espacio(cuadro, 1).NoSimultaneoCon);
        Assert.True(Espacio(cuadro, 7).OmitidoPorNoSimultaneo);

        var abierto = ArchivoDelCuadro.Abrir(ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now), Motor).Cuadro!;
        Assert.Equal(7, Espacio(abierto, 1).NoSimultaneoCon);
        Assert.Equal(1, Espacio(abierto, 7).NoSimultaneoCon);
        Assert.True(Espacio(abierto, 7).OmitidoPorNoSimultaneo);
    }

    [Fact]
    public void I121_UnParConUnCircuitoSinCargaAvisa()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        c.NoContinua = 1000m;
        c.NoSimultaneoCon = 5;
        cuadro.Recalcular();

        Assert.False(c.OmitidoPorNoSimultaneo);
        Assert.Contains(cuadro.AvisosDeCircuitos, x => x.StartsWith("Circuito 1: no simultáneo con el 5"));
    }

    // ---- Medio de desconexión — I-122 --------------------------------------------------------------

    [Fact]
    public void I122_LaMemoriaDiceElMedioDeDesconexionMinimo()
    {
        var cuadro = Nuevo();
        var motor = Trifasico(cuadro, 1);
        motor.Hp = 5m;
        var aire = Espacio(cuadro, 2);
        aire.Categoria = CategoriaDeCarga.AireAcondicionado;
        aire.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
        aire.CorrientePlacaA = 12m;
        var cuarto = Espacio(cuadro, 4);
        cuarto.Categoria = CategoriaDeCarga.AireAcondicionado;
        cuarto.PlacaAire = PlacaDeAireAcondicionado.Habitacion;
        cuarto.CorrientePlacaA = 10m;
        cuadro.Recalcular();

        Assert.Contains("Medio de desconexión — 430-110(a): 115 % × 15.20 A = 17.48 A", Memoria(cuadro, motor));
        Assert.Contains("Medio de desconexión — 440-12(a)(1): 115 % × 12.00 A = 13.80 A", Memoria(cuadro, aire));
        Assert.Contains("Medio de desconexión — 440-63: La clavija y el contacto", Memoria(cuadro, cuarto));
    }
}
