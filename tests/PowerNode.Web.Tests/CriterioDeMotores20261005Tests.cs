using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Lo de criterio de la auditoría de motores, decidido por David el 2026-10-05</b> — AM-4 (M-21), AM-5 (M-22),
/// AM-7 (M-23) y AM-11 (I-189). Cada decisión está en <c>docs/decisiones/</c>; aquí, los casos con que se tomó.
/// </summary>
public class CriterioDeMotores20261005Tests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int espacio) =>
        cuadro.Circuitos.Single(x => x.Espacio == espacio);

    private static CuadroDeCarga Nuevo(decimal tension = 220m, int fases = 3, int hilos = 4)
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = 12;
        cuadro.Datos.Fases = fases;
        cuadro.Datos.Hilos = hilos;
        cuadro.Datos.TensionFaseFaseV = tension;
        cuadro.Recalcular();
        return cuadro;
    }

    /// <summary>Un motor trifásico solo, en HP, en el espacio 1.</summary>
    private static (CuadroDeCarga Cuadro, CircuitoDelCuadro Motor) UnMotor(decimal hp, decimal tension, int hilos = 4)
    {
        var cuadro = Nuevo(tension, 3, hilos);
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = hp;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        cuadro.Recalcular();
        return (cuadro, c);
    }

    // ---- AM-4 · M-21: la columna, la más baja de las dos terminales --------------------------------------

    /// <summary>
    /// Los tres casos de la decisión. La terminal del interruptor es de 75 °C en los tres (más de 100 A —
    /// 110-14(c)(1)b.); la del motor y su arrancador, sin marcado, de 60 °C con conductor de 14 a 1 AWG y de 75 °C
    /// mayor que 1 AWG. 25 HP a 220 V: 85 A → 3 AWG a 60 °C (antes 4 AWG a 75 °C). 50 HP a 440 V: FLC 65 A
    /// (columna de 460 V), 81.25 A → 3 AWG (antes 4 AWG). 100 HP a 440 V: 124 A, 155 A → a 60 °C sería 3/0, mayor
    /// que 1 AWG, así que el motor también es de 75 °C: 2/0 (175 A), sin cambio.
    /// </summary>
    [Theory]
    [InlineData(25, 220, 4, 175, "3", 60)]
    [InlineData(50, 440, 3, 175, "3", 60)]
    [InlineData(100, 440, 3, 350, "2/0", 75)]
    public void M21_LaColumnaEsLaMasBajaDeLasDosTerminales(int hp, int tension, int hilos, int proteccion, string calibre, int columna)
    {
        var (_, c) = UnMotor(hp, tension, hilos);
        var r = c.Resultado!;
        Assert.Equal(proteccion, r.ProteccionA);
        Assert.Equal(calibre, r.CalibreFase.Designacion);
        Assert.Equal(columna, r.Detalle!.TemperaturaTerminalesC);
        Assert.Contains(r.Citas, x => x.Referencia == "110-14(c)(1)" && x.Descripcion.EndsWith("terminales a 75°C (110-14(c)(1)b.)", StringComparison.Ordinal));
        Assert.Contains(r.Citas, x => x.Referencia == "110-14(c)" && x.Descripcion.StartsWith("Terminal del motor y del arrancador", StringComparison.Ordinal));
    }

    /// <summary>El desglose de la celda dice que la terminal es la del motor, no la de la protección.</summary>
    [Fact]
    public void M21_ElDesgloseDiceQueMandaLaTerminalDelMotor()
    {
        var (cuadro, c) = UnMotor(25m, 220m);
        var cita = Assert.Single(c.Resultado!.Citas, x => x.Referencia == "110-14(c)" && x.Descripcion.StartsWith("Terminal del motor", StringComparison.Ordinal)).Descripcion;
        Assert.Equal(
            "Terminal del motor y del arrancador: con 3 AWG (de 14 a 1 AWG) y sin marcado de 75°C, 60°C (110-14(c)(1)a.). Manda la más " +
            "baja de las terminales del circuito: el conductor va en la columna de 60°C. Con un motor de diseño B, C, D o E y el arrancador " +
            "marcado 75°C se permite 75°C — 110-14(c)(1)a.(3) y a.(4).", cita);
        Assert.Contains(cuadro.Desglose(c)!.Conductor,
            x => x.Contains("terminal 60 °C (el motor y su arrancador, con conductor de 14 a 1 AWG: la más baja de las terminales) — 110-14(c)(1)a., 110-14(c)"));
    }

    /// <summary>
    /// Con la casilla «Motor diseño B a E y arrancador marcado 75 °C», las dos terminales a 75 °C: el 25 HP a 220 V
    /// regresa a 4 AWG (85 A a 75 °C) — 110-14(c)(1)a.(3) y a.(4). Desmarcada por omisión; se guarda en el archivo.
    /// </summary>
    [Fact]
    public void M21_ConLaCasillaElMotorVaA75YSeGuarda()
    {
        var (cuadro, c) = UnMotor(25m, 220m);
        Assert.False(c.MotorYArrancadorMarcados75C);
        c.MotorYArrancadorMarcados75C = true;
        cuadro.Recalcular();
        Assert.Equal("4", c.Resultado!.CalibreFase.Designacion);
        Assert.Equal(75, c.Resultado.Detalle!.TemperaturaTerminalesC);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "110-14(c)(1)a.(4)");

        var texto = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        Assert.Contains("\"motorYArrancador75C\": true", texto);
        var abierto = ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!;
        Assert.True(Espacio(abierto, 1).MotorYArrancadorMarcados75C);
        Assert.Equal("4", Espacio(abierto, 1).Resultado!.CalibreFase.Designacion);
    }

    /// <summary>
    /// La casilla no sube la columna si la terminal del interruptor es de 60 °C (100 A o menos, tablero sin marcar):
    /// manda la más baja, y la cita lo dice. 10 HP a 220 V: 28 A, 35 A → 8 AWG con 60 A de protección.
    /// </summary>
    [Fact]
    public void M21_LaCasillaNoSubeLaColumnaSiElInterruptorEsDe60()
    {
        var (cuadro, c) = UnMotor(10m, 220m);
        var antes = c.Resultado!.CalibreFase.Designacion;
        c.MotorYArrancadorMarcados75C = true;
        cuadro.Recalcular();
        Assert.Equal(antes, c.Resultado!.CalibreFase.Designacion);
        Assert.Equal(60, c.Resultado.Detalle!.TemperaturaTerminalesC);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "110-14(c)" && x.Descripcion.Contains("manda la más baja"));
    }

    /// <summary>
    /// El calibre decide la terminal y la terminal el calibre. 40 HP a 220 V: FLC 104 A, 130 A. A 60 °C haría falta
    /// 2/0 (1/0 da 125 A), mayor que 1 AWG, así que la terminal del motor es de 75 °C; a 75 °C bastaría 1 AWG (130 A),
    /// pero con 1 AWG la terminal volvería a ser de 60 °C (110 A). Queda 1/0 AWG a 75 °C (150 A).
    /// </summary>
    [Fact]
    public void M21_ConConductorMayorQue1AwgElMotorVaA75YNoBajaDe1_0()
    {
        var (_, c) = UnMotor(40m, 220m);
        var r = c.Resultado!;
        Assert.Equal(104m, c.FlcA);
        Assert.Equal("1/0", r.CalibreFase.Designacion);
        Assert.Equal(75, r.Detalle!.TemperaturaTerminalesC);
        Assert.Contains(r.Citas, x => x.Referencia == "110-14(c)" && x.Descripcion.Contains("con 1/0 AWG como mínimo"));
    }

    // ---- AM-7 · M-23: el variador con bypass ---------------------------------------------------------

    /// <summary>Un variador trifásico a 440 V en el espacio 1, como en el caso C2 de la auditoría.</summary>
    private static (CuadroDeCarga Cuadro, CircuitoDelCuadro Variador) UnVariador(decimal entradaA, decimal maximaA, decimal? hp = null, bool bypass = false)
    {
        var cuadro = Nuevo(440m, 3, 3);
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.CapturaMotor = CapturaDeMotor.Variador;
        c.CorrienteEntradaVariadorA = entradaA;
        c.ProteccionMaximaVariadorA = maximaA;
        c.HpMotorDelVariador = hp;
        c.VariadorConBypass = bypass;
        Assert.Null(cuadro.CambiarTensionDePlaca(c, TensionDePlaca.V440a460Trifasica));
        cuadro.Recalcular();
        return (cuadro, c);
    }

    /// <summary>
    /// La prueba de la decisión: bomba de 30 HP a 440 V, entrada 42 A, máxima del fabricante 70 A. Sin bypass:
    /// 52.5 A → 6 AWG, de 60 a 70 A. Con bypass: 125 % de la FLC (40 A, columna de 460 V) = 50 A, menos que 52.5 A; la
    /// Tabla 430-52 permite 250 % × 40 = 100 A, más que 70 A. Sin cambio: 6 AWG, de 60 a 70 A. El bypass pide además
    /// su relevador de sobrecarga (430-124(b)) y la desconexión, 115 % × 42 A = 48.30 A (430-128, 430-110(a)).
    /// </summary>
    [Fact]
    public void M23_ElCasoDeLaAuditoriaNoCambiaConBypass()
    {
        var (_, sin) = UnVariador(42m, 70m);
        var (cuadro, con) = UnVariador(42m, 70m, hp: 30m, bypass: true);
        foreach (var c in new[] { sin, con })
        {
            Assert.Null(c.Error);
            Assert.Equal("6", c.Resultado!.CalibreFase.Designacion);
            Assert.Equal([60m, 70m], c.Resultado.Rango!.Valores);
            Assert.Equal(70m, c.Resultado.ProteccionA);
            Assert.Equal(52.5m, c.Resultado.Detalle!.CapacidadMinimaA);
        }
        var citas = con.Resultado!.Citas;
        Assert.Contains(citas, x => x.Referencia == "430-122(b)" && x.Descripcion.Contains("(30 HP, 40 A) = 50 A. Manda el mayor: 52.5 A"));
        Assert.Contains(citas, x => x.Referencia == "430-52(c)(1)" && x.Descripcion.Contains("250% x 40 A"));
        Assert.Contains(citas, x => x.Referencia == "430-122(b)" && x.Descripcion.StartsWith("Máximo del circuito: 70 A", StringComparison.Ordinal));
        Assert.Contains(citas, x => x.Referencia == "430-128" && x.Descripcion.Contains("= 48.3 A — 430-128, 430-110(a)"));
        Assert.Equal("430-124(a), 430-124(b), 430-126", con.Sobrecarga!.Referencia);
        Assert.True(con.Sobrecarga.Aparte);
        Assert.Empty(cuadro.AvisosDe(con));
    }

    /// <summary>Con un variador de entrada menor que la FLC del motor, el conductor sale del motor: 125 % × 40 A = 50 A → 6 AWG (antes 8 AWG).</summary>
    [Fact]
    public void M23_ConBypassElConductorEsElMayorDeLosDos125()
    {
        var (_, sin) = UnVariador(30m, 60m, hp: 30m);
        var (_, con) = UnVariador(30m, 60m, hp: 30m, bypass: true);
        Assert.Equal(37.5m, sin.Resultado!.Detalle!.CapacidadMinimaA);
        Assert.Equal("8", sin.Resultado.CalibreFase.Designacion);
        Assert.Equal(50m, con.Resultado!.Detalle!.CapacidadMinimaA);
        Assert.Equal("6", con.Resultado.CalibreFase.Designacion);
        // El piso de la protección sigue en el 125 % de la entrada: 40 A.
        Assert.Equal(40m, con.Resultado.Rango!.MinimoA);
    }

    /// <summary>
    /// La Tabla 430-52 limita el rango: motor de 5 HP a 440 V, FLC 7.6 A, 250 % = 19 A → 20 A (Excepción 1). Un
    /// variador de 8 A de entrada y 30 A de máxima: sin bypass, de 15 a 30 A; con bypass, de 15 a 20 A.
    /// </summary>
    [Fact]
    public void M23_ConBypassLaTabla430_52LimitaElMaximo()
    {
        var (_, sin) = UnVariador(8m, 30m, hp: 5m);
        var (cuadro, con) = UnVariador(8m, 30m, hp: 5m, bypass: true);
        Assert.Equal(30m, sin.Resultado!.Rango!.MaximoA);
        Assert.Equal(20m, con.Resultado!.Rango!.MaximoA);
        Assert.Equal([15m, 20m], con.Resultado.Rango.Valores);
        Assert.Equal(20m, con.Resultado.ProteccionA);
        Assert.Equal(20m, con.Resultado.Rango.MaximoPermitidoA); // el que entra a 430-62(a)
        Assert.Contains("la menor de la máxima del fabricante del variador y la de la Tabla 430-52",
            CriteriosDeProteccion.PorQueLaCalculada(con.Resultado.Rango, con.Resultado.CalibreFase.DesignacionConUnidad, esMotor: false, noArrancaConLaTabla: false));
        Assert.Contains(cuadro.Desglose(con)!.Proteccion, x => x.StartsWith("Con bypass, la de la Tabla 430-52 para el motor de 5 HP", StringComparison.Ordinal));
    }

    /// <summary>
    /// Rango vacío: 18 A de entrada (125 % = 22.5 A → 25 A) y el mismo motor de 5 HP (20 A): ningún tamaño queda entre
    /// los dos. Se usa el máximo, 20 A, y se avisa.
    /// </summary>
    [Fact]
    public void M23_SinTamanoEntreElPisoYElMaximoAvisa()
    {
        var (cuadro, c) = UnVariador(18m, 30m, hp: 5m, bypass: true);
        Assert.Null(c.Error);
        Assert.Equal(20m, c.Resultado!.ProteccionA);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "430-122(b)" && x.Descripcion.StartsWith("⚠ Ningún tamaño", StringComparison.Ordinal));
        var aviso = Assert.Single(cuadro.AvisosDe(c));
        Assert.StartsWith("Con el bypass, ningún tamaño queda entre el 125 % de la entrada del variador (22.50 A) y el máximo (20 A", aviso);
    }

    /// <summary>Con bypass y sin los HP del motor no se calcula: falta un dato de la placa.</summary>
    [Fact]
    public void M23_ConBypassSinHpPideLosHp()
    {
        var (_, c) = UnVariador(42m, 70m, bypass: true);
        Assert.Null(c.Resultado);
        Assert.StartsWith("430-122(b): con bypass el conductor lleva también el 125 % de la corriente del motor.", c.Error);
    }

    /// <summary>
    /// El bypass y los HP se guardan en el archivo (formato 13) y pasan con el variador al desplegable y de regreso;
    /// en un grupo no entran al cálculo, y se avisa.
    /// </summary>
    [Fact]
    public void M23_ElBypassSeGuardaYNoSePierdeEnElDesplegable()
    {
        var (cuadro, c) = UnVariador(42m, 70m, hp: 30m, bypass: true);
        var texto = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        Assert.Contains("\"conBypass\": true", texto);
        var abierto = Espacio(ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!, 1);
        Assert.True(abierto.VariadorConBypass);
        Assert.Equal(30m, abierto.HpMotorDelVariador);

        // Otra línea: el variador pasa a su línea, con su bypass, y el grupo lo avisa.
        var luz = c.AgregarCarga();
        luz.Subtipo = SubtipoDeCarga.Luminarias;
        luz.CargaUnitaria = 300m;
        cuadro.Recalcular();
        var variador = Assert.Single(c.Cargas, a => a.Clase == ClaseDeAparato.Variador);
        Assert.True(variador.ConBypass);
        Assert.Equal(30m, variador.HpMotorDelVariador);
        Assert.Contains(cuadro.AvisosDe(c), x => x.Contains("el bypass no entra al cálculo del grupo"));

        // Se quita la otra: regresa al renglón con su bypass.
        c.QuitarCarga(luz);
        cuadro.Recalcular();
        Assert.True(c.EsVariador);
        Assert.True(c.VariadorConBypass);
        Assert.Equal(30m, c.HpMotorDelVariador);
        Assert.Contains(c.Resultado!.Citas, x => x.Referencia == "430-122(b)");
    }

    // ---- AM-11 · I-189: la tensión de placa ----------------------------------------------------------

    /// <summary>El minisplit de 1 TR del caso A: MCA 10.5 A, MOCP 15 A, a 20 m, en un tablero 3F-4H 220/127 V.</summary>
    private static (CuadroDeCarga Cuadro, CircuitoDelCuadro Minisplit) UnMinisplit()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        c.PlacaAire = PlacaDeAireAcondicionado.AmpacidadYProteccion;
        c.AmpacidadMinimaA = 10.5m;
        c.ProteccionMaximaA = 15m;
        c.LongitudM = 20m;
        cuadro.Recalcular();
        return (cuadro, c);
    }

    /// <summary>
    /// Sin valor por omisión: un equipo de A/C sin tensión de placa no se calcula y la pide, como la MOCP. Antes
    /// nacía en 1 polo y se calculaba a 127 V sin que nadie lo decidiera.
    /// </summary>
    [Fact]
    public void I189_SinTensionDePlacaNoSeCalculaYLaPide()
    {
        var (_, c) = UnMinisplit();
        Assert.True(c.PideTensionDePlaca);
        Assert.Null(c.TensionDePlaca);
        Assert.Null(c.Resultado);
        Assert.Equal("Falta la tensión de placa del equipo: escógela en el desplegable (la flecha junto a la descripción). " +
                     "De ella salen los polos del circuito.", c.Error);
    }

    /// <summary>
    /// El minisplit de la auditoría: a 127 V (1 polo) la caída lo subía a 12 AWG; con su placa de 208/230 V el circuito
    /// pasa a 2 polos y queda en 14 AWG.
    /// </summary>
    [Fact]
    public void I189_LosPolosSalenDeLaTensionDePlaca()
    {
        var (cuadro, c) = UnMinisplit();
        Assert.Null(cuadro.CambiarTensionDePlaca(c, TensionDePlaca.V127Monofasica));
        Assert.Equal(1, c.Polos);
        Assert.Equal("12", c.Resultado!.CalibreFase.Designacion);

        Assert.Null(cuadro.CambiarTensionDePlaca(c, TensionDePlaca.V208a230Monofasica));
        Assert.Equal(2, c.Polos);
        Assert.Null(c.Error);
        Assert.Equal("14", c.Resultado!.CalibreFase.Designacion);
        Assert.Equal(15m, c.Resultado.ProteccionA);
    }

    /// <summary>Al cambiar los polos en la columna P, la tensión los sigue; sin tensión capturada, sigue faltando.</summary>
    [Fact]
    public void I189_LaTensionSigueALosPolos()
    {
        var (cuadro, c) = UnMinisplit();
        Assert.Null(cuadro.CambiarPolos(c, 2));
        Assert.Null(c.TensionDePlaca); // no la inventa

        Assert.Null(cuadro.CambiarTensionDePlaca(c, TensionDePlaca.V208a230Monofasica));
        Assert.Null(cuadro.CambiarPolos(c, 3));
        Assert.Equal(TensionDePlaca.V220Trifasica, c.TensionDePlaca);
        Assert.Null(cuadro.CambiarPolos(c, 1));
        Assert.Equal(TensionDePlaca.V127Monofasica, c.TensionDePlaca);
        Assert.Null(c.Error);
    }

    /// <summary>Si los polos no caben, la tensión no cambia y se dice por qué.</summary>
    [Fact]
    public void I189_SiNoCabenLosPolosNoCambia()
    {
        var (cuadro, c) = UnMinisplit();
        Assert.Null(cuadro.CambiarTensionDePlaca(c, TensionDePlaca.V127Monofasica));
        var vecino = Espacio(cuadro, 3);
        vecino.Categoria = CategoriaDeCarga.Alumbrado;
        vecino.NoContinua = 500m;
        cuadro.Recalcular();

        Assert.NotNull(cuadro.CambiarTensionDePlaca(c, TensionDePlaca.V208a230Monofasica));
        Assert.Equal(TensionDePlaca.V127Monofasica, c.TensionDePlaca);
        Assert.Equal(1, c.Polos);
    }

    /// <summary>Solo las que el tablero alimenta: 127 V con neutro; 208/230 V entre fases; trifásica en uno trifásico.</summary>
    [Theory]
    [InlineData(3, 4, 220, "127 V 1F|208/230 V 1F|220 V 3F")]
    [InlineData(3, 3, 220, "208/230 V 1F|220 V 3F")]
    [InlineData(2, 3, 220, "127 V 1F|208/230 V 1F")]
    [InlineData(1, 3, 240, "127 V 1F|208/230 V 1F")]
    [InlineData(3, 3, 440, "440/460 V 3F")]
    [InlineData(3, 4, 480, "440/460 V 3F")]
    public void I189_LasOpcionesSonLasDelTablero(int fases, int hilos, int tension, string opciones)
    {
        var cuadro = Nuevo(tension, fases, hilos);
        Assert.Equal(opciones, string.Join("|", TensionesDePlaca.De(cuadro.Datos).Select(t => t.Texto())));
    }

    /// <summary>
    /// El variador pide su tensión de entrada; el acondicionador de habitación, la suya. Si el tablero cambia y la
    /// placa ya no es de él, se dice.
    /// </summary>
    [Fact]
    public void I189_VariadorYHabitacionTambienYLaPlacaDeOtroTablero()
    {
        var cuadro = Nuevo();
        var variador = Espacio(cuadro, 1);
        variador.Categoria = CategoriaDeCarga.Motor;
        variador.CapturaMotor = CapturaDeMotor.Variador;
        variador.CorrienteEntradaVariadorA = 20m;
        variador.ProteccionMaximaVariadorA = 40m;
        var cuarto = Espacio(cuadro, 2);
        cuarto.Categoria = CategoriaDeCarga.AireAcondicionado;
        cuarto.PlacaAire = PlacaDeAireAcondicionado.Habitacion;
        cuarto.CorrientePlacaA = 10m;
        cuadro.Recalcular();
        Assert.StartsWith("Falta la tensión de entrada del variador:", variador.Error);
        Assert.StartsWith("Falta la tensión de placa del equipo:", cuarto.Error);

        Assert.Null(cuadro.CambiarTensionDePlaca(cuarto, TensionDePlaca.V127Monofasica));
        Assert.Null(cuarto.Error);
        cuadro.Datos.TensionFaseFaseV = 440m;
        cuadro.Recalcular();
        Assert.StartsWith("La tensión de placa (127 V 1F) no es de este tablero", cuarto.Error);
    }

    /// <summary>
    /// Un archivo de antes (formato 12) no traía la tensión: abre con la de sus polos, calcula igual y avisa. La de
    /// formato 13 se guarda y se abre.
    /// </summary>
    [Fact]
    public void I189_UnArchivoAnteriorAbreConLaTensionDeSusPolosYAvisa()
    {
        var (cuadro, c) = UnMinisplit();
        Assert.Null(cuadro.CambiarTensionDePlaca(c, TensionDePlaca.V208a230Monofasica));
        var texto = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        Assert.Contains("\"tensionDePlaca\": \"V208a230Monofasica\"", texto);
        Assert.Equal(TensionDePlaca.V208a230Monofasica, Espacio(ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!, 1).TensionDePlaca);

        var anterior = texto.Replace("\"version\": 13", "\"version\": 12").Replace("\"tensionDePlaca\": \"V208a230Monofasica\",", "");
        Assert.DoesNotContain("tensionDePlaca", anterior);
        var apertura = ArchivoDelCuadro.Abrir(anterior, Motor);
        var abierto = Espacio(apertura.Cuadro!, 1);
        Assert.Equal(TensionDePlaca.V208a230Monofasica, abierto.TensionDePlaca);
        Assert.Null(abierto.Error);
        Assert.Equal("14", abierto.Resultado!.CalibreFase.Designacion);
        Assert.Contains("El circuito 1 no traía la tensión de placa (archivo de formato 12): se tomó la de sus 2 polos, 208/230 V 1F. " +
                        "Verifícala con la placa del equipo.", apertura.Avisos);
    }

    /// <summary>La tensión pasa con el equipo al desplegable y de regreso.</summary>
    [Fact]
    public void I189_LaTensionNoSePierdeEnElDesplegable()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        c.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
        c.CorrientePlacaA = 12m;
        Assert.Null(cuadro.CambiarTensionDePlaca(c, TensionDePlaca.V208a230Monofasica));

        var luz = c.AgregarCarga();
        luz.Subtipo = SubtipoDeCarga.Luminarias;
        luz.CargaUnitaria = 100m;
        cuadro.Recalcular();
        Assert.Equal(TensionDePlaca.V208a230Monofasica, Assert.Single(c.Cargas, a => a.Clase == ClaseDeAparato.Motocompresor).TensionDePlaca);
        Assert.False(c.PideTensionDePlaca); // el grupo va a la de su circuito

        c.QuitarCarga(luz);
        cuadro.Recalcular();
        Assert.True(c.PideTensionDePlaca);
        Assert.Equal(TensionDePlaca.V208a230Monofasica, c.TensionDePlaca);
        Assert.Null(c.Error);
    }

    /// <summary>La memoria dice la tensión de placa del equipo y, en el variador, el bypass con su motor.</summary>
    [Fact]
    public void I189_M23_LaMemoriaDiceLaTensionDePlacaYElBypass()
    {
        var (cuadro, minisplit) = UnMinisplit();
        Assert.Null(cuadro.CambiarTensionDePlaca(minisplit, TensionDePlaca.V208a230Monofasica));
        Assert.StartsWith("Motocompresor hermético · 208/230 V 1F · placa:", MemoriaDeCalculo.DeCircuito(cuadro, minisplit).Equipo!.Descripcion);

        var (otro, variador) = UnVariador(42m, 70m, hp: 30m, bypass: true);
        var hoja = MemoriaDeCalculo.DeCircuito(otro, variador);
        Assert.Equal("Variador de velocidad · entrada 440/460 V 3F · 42.00 A, protección máxima del fabricante 70 A · con bypass: motor de 30 HP, " +
                     "FLC 40.00 A — 430 Parte J", hoja.Equipo!.Descripcion);
        var renglones = MemoriaDeCalculo.Secciones(hoja).SelectMany(x => x.Renglones).Select(x => $"{x.Rotulo}: {x.Valor}").ToList();
        Assert.Contains("Capacidad mínima del conductor — 430-122(a), 430-122(b): el mayor de 125 % × 42.00 A = 52.50 A (entrada) y " +
                        "125 % × 40.00 A = 50.00 A (FLC del motor, por el bypass): 52.50 A", renglones);
        Assert.Contains(renglones, x => x.StartsWith("Protección máxima con bypass — 430-52(c)(1), 430-120: 250 % × 40.00 A = 100", StringComparison.Ordinal));
        Assert.Contains("Medio de desconexión — 430-128, 430-110(a): 115 % × 42.00 A = 48.30 A como mínimo: la mayor de la corriente de entrada " +
                        "del variador y la corriente a plena carga del motor, porque el bypass lo conecta directo", renglones);
        Assert.Contains(hoja.Equipo.Notas, x => x.StartsWith("Con bypass, el motor también trabaja y arranca directo de la línea", StringComparison.Ordinal));
    }

    // ---- AM-5 · M-22: 430-62(b) con el conductor mínimo -------------------------------------------------

    /// <summary>
    /// El caso A de la auditoría: vivienda, 2F-3H 220/127 V, cobre THHN, 20 m, terminales de 60 °C, centro de
    /// carga NEMA. En la fase A: bomba de 1/2 HP a 127 V (8.9 A), hidroneumático de 1 HP a 220 V (8 A), minisplits
    /// de 1 TR (MCA 10.5 A, MOCP 15 A) y de 2 TR (MCA 19.5 A, MOCP 30 A) y la lavadora (1500 VA); en la B, además del
    /// hidroneumático, el refrigerador (3.5 A) y la bomba de la alberca (placa de 10 A, sin HP).
    /// </summary>
    private static CuadroDeCarga ElCasoA(SerieDeInterruptores serie = SerieDeInterruptores.CentroDeCargaNema, decimal longitudM = 20m)
    {
        var cuadro = new CuadroDeCarga(Motor);
        var d = cuadro.Datos;
        d.NumeroEspacios = 20;
        d.Fases = 2;
        d.Hilos = 3;
        d.TensionFaseFaseV = 220m;
        d.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        d.SerieInterruptores = serie;
        d.LongitudAlimentadorM = longitudM;
        cuadro.Recalcular();

        var bomba = Espacio(cuadro, 1);
        bomba.Categoria = CategoriaDeCarga.Motor;
        bomba.Hp = 0.5m;
        var hidroneumatico = Espacio(cuadro, 3);
        hidroneumatico.Categoria = CategoriaDeCarga.Motor;
        hidroneumatico.Hp = 1m;
        Assert.Null(cuadro.CambiarPolos(hidroneumatico, 2));
        foreach (var (espacio, mca, mocp) in new[] { (9, 10.5m, 15m), (13, 19.5m, 30m) })
        {
            var minisplit = Espacio(cuadro, espacio);
            minisplit.Categoria = CategoriaDeCarga.AireAcondicionado;
            minisplit.PlacaAire = PlacaDeAireAcondicionado.AmpacidadYProteccion;
            minisplit.AmpacidadMinimaA = mca;
            minisplit.ProteccionMaximaA = mocp;
            Assert.Null(cuadro.CambiarTensionDePlaca(minisplit, TensionDePlaca.V127Monofasica));
        }
        var refrigerador = Espacio(cuadro, 15);
        refrigerador.Categoria = CategoriaDeCarga.Contactos;
        var r = refrigerador.AgregarCarga();
        r.Subtipo = SubtipoDeCarga.ContactoRefrigerador;
        r.Unidad = UnidadConsumo.Amperes;
        r.CargaUnitaria = 3.5m;
        var lavadora = Espacio(cuadro, 17);
        lavadora.Categoria = CategoriaDeCarga.Contactos;
        var l = lavadora.AgregarCarga();
        l.Subtipo = SubtipoDeCarga.ContactoLavadora;
        l.CargaUnitaria = 1500m;
        var alberca = Espacio(cuadro, 19);
        alberca.Categoria = CategoriaDeCarga.Motor;
        alberca.CapturaMotor = CapturaDeMotor.Amperes;
        alberca.CorrientePlacaA = 10m;
        foreach (var c in cuadro.Circuitos.Where(c => c.TieneCarga))
            c.LongitudM = 20m;
        cuadro.Recalcular();
        return cuadro;
    }

    /// <summary>
    /// 430-24: 125 % × 8.9 A + 38 A = 49.13 A, más 11.81 A de la lavadora = 60.93 A → 70 A (240-6(a)); techo de
    /// 430-62(a) y 430-63: 30 A + 27.4 A + 11.81 A = 69.21 A. El conductor mínimo, 4 AWG, tiene 70 A a 60 °C: más
    /// que los 60.93 A, así que 430-62(b) permite los 70 A (lectura literal, A). La cita dice además las dos cosas
    /// que lo sostienen (B y lo que pidió David): que los 70 A son la ampacidad del 4 AWG y lo protegen (240-4), y que
    /// no hay tamaño estándar entre 60.93 y 69.21 A: el anterior, 60 A, queda debajo de 430-24.
    /// </summary>
    [Fact]
    public void M22_ElCasoADiceQueElConductorQuedaProtegidoYQueNoHayTamanoIntermedio()
    {
        var r = ElCasoA().Alimentador.Resultado!;
        Assert.Equal(60.93m, Math.Round(r.Detalle!.CapacidadMinimaA, 2));
        Assert.Equal(69.21m, Math.Round(r.TechoProteccion430_62A!.Value, 2));
        Assert.Equal(70m, r.ProteccionA);
        Assert.Equal("4", r.CalibreFase.Designacion);
        Assert.False(r.ProteccionExcedeTecho430_62);

        var cita = Assert.Single(r.Citas, x => x.Referencia == "430-62(b)").Descripcion;
        Assert.Contains("El conductor instalado (4 AWG) tiene una ampacidad de 70 A, mayor que los 60.93 A que exigía 430-24", cita);
        Assert.Contains("70 A = ampacidad del 4 AWG: el conductor queda protegido — 240-4.", cita);
        Assert.Contains("No hay tamaño estándar entre 60.93 A y el techo de 69.21 A: el anterior, 60 A, queda debajo de la capacidad que pide 430-24.", cita);
    }

    /// <summary>
    /// Con el conductor más grande por caída de tensión (60 m: 3 AWG, 85 A a 60 °C), los 70 A quedan debajo de su
    /// ampacidad: la cita lo dice con «&lt;» y no con «=».
    /// </summary>
    [Fact]
    public void M22_ConElConductorMasGrandeLaProteccionQuedaDebajoDeSuAmpacidad()
    {
        var r = ElCasoA(longitudM: 60m).Alimentador.Resultado!;
        Assert.Equal(70m, r.ProteccionA);
        Assert.NotEqual("4", r.CalibreFase.Designacion);

        var cita = Assert.Single(r.Citas, x => x.Referencia == "430-62(b)").Descripcion;
        Assert.Contains($"70 A < {r.Detalle!.AmpacidadConductorA:0.##} A, la ampacidad del {r.CalibreFase.DesignacionConUnidad}: el conductor queda protegido — 240-4.", cita);
        Assert.Contains("el anterior, 60 A, queda debajo", cita);
    }

    /// <summary>
    /// Con riel DIN sí hay tamaño entre la capacidad mínima y el techo (63 A): se escoge solo, no excede el techo y
    /// 430-62(b) no hace falta.
    /// </summary>
    [Fact]
    public void M22_ConRielDinHayTamanoDebajoDelTechoY430_62bNoHaceFalta()
    {
        var r = ElCasoA(SerieDeInterruptores.RielDinIec).Alimentador.Resultado!;
        Assert.Equal(63m, r.ProteccionA);
        Assert.False(r.ProteccionExcedeTecho430_62);
        Assert.DoesNotContain(r.Citas, x => x.Referencia == "430-62(b)");
    }
}
