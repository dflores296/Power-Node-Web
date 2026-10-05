using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;

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
