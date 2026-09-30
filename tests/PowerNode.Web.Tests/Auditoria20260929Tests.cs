using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Auditoría NOM-001-SEDE-2012 del 2026-09-29</b> (caja negra contra <c>55c120b</c>). Cada prueba
/// lleva el número del hallazgo que reproduce. Los valores esperados salen de la norma recalculada a
/// mano en la auditoría, no de la implementación. Condiciones base: 3F-4H 220/127 V, cobre THHN,
/// terminales de 60 °C hasta 100 A, lugar seco, 30 °C, centro de carga NEMA, 20 m, EMT.
/// </summary>
public class Auditoria20260929Tests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static readonly MotorNom Motor = new(Json);

    private static CuadroDeCarga Nuevo(int espacios = 12, int fases = 3, int hilos = 4, decimal tension = 220m)
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

    // ---- P1-1 · 430-52(c)(1) Excepción 1 contra la lista de 240-6(a) --------------------------------

    [Theory]
    // 1 HP monofásico a 127 V: FLC 14 A (Tabla 430-248), 250 % = 35 A, valor normalizado → 32 A en riel DIN.
    [InlineData(1, 1, 14, 35, 32)]
    // 10 HP trifásico a 220 V: FLC 28 A (Tabla 430-250, col. 230 V), 250 % = 70 A, normalizado → 63 A.
    [InlineData(10, 3, 28, 70, 63)]
    public void P1_1_EnRielDinElMotorNoSubeSiElTechoEsNormalizado(decimal hp, int polos, decimal flc, decimal techo, decimal esperado)
    {
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        var c = ConMotor(cuadro, 1, hp, polos);

        Assert.Null(c.Error);
        Assert.Equal(flc, c.FlcA);
        Assert.Equal(esperado, c.Resultado!.ProteccionA);
        var p = cuadro.ProteccionDelMotor(c);
        Assert.Equal(techo, p.TechoA);
        Assert.Equal(techo, p.MaximoA);
        Assert.False(p.UsaExcepcion1);
        // La Excepción 1 no se cita: no hubo redondeo.
        Assert.DoesNotContain(c.Resultado.Citas, x => x.Descripcion.Contains("Excepción 1"));
        Assert.DoesNotContain(cuadro.Desglose(c)!.Proteccion, l => l.Contains("Excepción 1"));
    }

    [Fact]
    public void P1_1_EnCentroDeCargaUnTechoNormalizadoNoCitaLaExcepcion1()
    {
        // 10 HP a 220 V: 70 A → 70 A. Antes la memoria decía «70 A → 70 A (Excepción 1: redondeo…)».
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 10m, 3);

        Assert.Equal(70m, c.Resultado!.ProteccionA);
        Assert.DoesNotContain(c.Resultado.Citas, x => x.Descripcion.Contains("Excepción 1"));
        var hoja = MemoriaDeCalculo.DeCircuito(cuadro, c);
        Assert.DoesNotContain(hoja.Equipo!.Proteccion, f => f.Rotulo.Contains("Excepción 1"));
    }

    [Fact]
    public void P1_1_UnTechoQueNoEsNormalizadoSubeAlSiguienteYCitaLaExcepcion1()
    {
        // Regresión 7: 10 HP a 208 V, FLC 30.8 A (col. 208 V), 250 % = 77 A → 80 A, Excepción 1.
        var cuadro = Nuevo(tension: 208m);
        var c = ConMotor(cuadro, 1, 10m, 3);

        Assert.Equal(30.8m, c.FlcA);
        Assert.Equal(80m, c.Resultado!.ProteccionA);
        Assert.True(cuadro.ProteccionDelMotor(c).UsaExcepcion1);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "430-52" && x.Descripcion.Contains("Excepción 1"));
        var hoja = MemoriaDeCalculo.DeCircuito(cuadro, c);
        Assert.Contains(hoja.Equipo!.Proteccion, f => f.Rotulo == "Protección seleccionada — 430-52(c)(1) Excepción 1");
    }

    [Fact]
    public void P1_1_ElTechoDe430_62HeredaLaProteccionCorregida()
    {
        // El techo del alimentador por 430-62(a) parte de la protección del derivado del motor mayor:
        // con el motor de 10 HP en riel DIN, 63 A y no 80.
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        ConMotor(cuadro, 1, 10m, 3);

        var techo = MemoriaDeCalculo.DelAlimentador(cuadro)!.Techo430_62A;
        Assert.NotNull(techo);
        Assert.Equal(63m, techo!.Value);
    }

    [Fact]
    public void P1_1_EnRielDinArribaDe125ANoSeTomaElMayorDeLaSerie()
    {
        // 60 HP a 220 V: FLC 154 A, 250 % = 385 A → 400 A de la lista de 240-6(a). El mayor de riel DIN
        // (125 A) no aguantaría el motor: arriba de la serie se usa la NOM y se avisa.
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        var c = ConMotor(cuadro, 1, 60m, 3);

        Assert.Equal(154m, c.FlcA);
        Assert.Equal(400m, c.Resultado!.ProteccionA);
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.Contains("En riel DIN no hay interruptores de más de 125 A"));
    }

    [Fact]
    public void P1_1_EnRielDinUnMotorChicoNoSubeA16A()
    {
        // ½ HP monofásico a 220 V (2 polos): FLC 4.9 A (Tabla 430-248, col. 230 V), 250 % = 12.25 A →
        // máximo 15 A (Excepción 1). Riel DIN empieza en 16 A, que ya excede el máximo: 15 A de la NOM, con aviso.
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        var c = ConMotor(cuadro, 1, 0.5m, 2);

        var p = cuadro.ProteccionDelMotor(c);
        Assert.Equal(4.9m, c.FlcA);
        Assert.Equal(15m, p.MaximoA);
        Assert.True(p.FueraDeLaSerie);
        Assert.Equal(15m, c.Resultado!.ProteccionA);
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.StartsWith("En riel DIN el interruptor más chico es de 16 A") && a.Contains("circuito 1 (15 A)"));
    }

    // ---- P1-2 · Lugar seco, húmedo y mojado ------------------------------------------------------

    /// <summary>
    /// Un circuito de 2 polos a 220 V, «Aparatos · Otra carga», 6 380 VA no continua (29 A), XHHW a
    /// 45 °C. Mojado: XHHW es de 75 °C (Tabla 310-104(a)); 10 AWG da 35 A × 0.82 = 28.7 A &lt; 29 A → 8
    /// AWG. Húmedo: 90 °C, 40 A × 0.87 = 34.8 A → 10 AWG. Antes «húmedo o mojado» daba 10 AWG en los dos.
    /// </summary>
    [Theory]
    [InlineData(LugarDeInstalacion.Humedo, "10", 90)]
    [InlineData(LugarDeInstalacion.Mojado, "8", 75)]
    public void P1_2_XhhwEnMojadoVaA75Grados(LugarDeInstalacion lugar, string calibre, int temperatura)
    {
        var cuadro = Nuevo();
        cuadro.Datos.TipoAislamiento = "XHHW";
        cuadro.Datos.TemperaturaAmbienteC = 45m;
        cuadro.Datos.Lugar = lugar;
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        Assert.Null(cuadro.CambiarPolos(c, 2));
        var a = c.AgregarCarga();
        a.Subtipo = SubtipoDeCarga.OtraCargaEspecifica;
        a.Unidad = UnidadConsumo.VoltAmperes;
        a.CargaUnitaria = 6380m;
        cuadro.Recalcular();

        Assert.Null(c.Error);
        Assert.Equal(29m, c.Resultado!.CorrienteDisenoA);
        Assert.Equal(temperatura, c.Resultado.Detalle!.TemperaturaAislamientoC);
        Assert.Equal(calibre, c.Resultado.CalibreFase.Designacion);
        Assert.Contains($"lugar {lugar.Nombre()}", MemoriaDeCalculo.Aislamiento(cuadro.Datos));
    }

    [Theory]
    // 310-10(c)(2) no los nombra: no se permiten en lugar mojado.
    [InlineData("RHH")]
    [InlineData("XHH")]
    [InlineData("THHN")]
    public void P1_2_EnMojadoSeBloqueanLosQue310_10cNoNombra(string tipo)
    {
        var cuadro = Nuevo();
        Espacio(cuadro, 1).NoContinua = 1000m;
        cuadro.Datos.TipoAislamiento = tipo;
        cuadro.Datos.Lugar = LugarDeInstalacion.Mojado;
        cuadro.Recalcular();

        Assert.Null(Espacio(cuadro, 1).Resultado);
        Assert.StartsWith($"{tipo} no se permite en lugar mojado", cuadro.AvisoAislamientoDelLugar);

        cuadro.Datos.Lugar = LugarDeInstalacion.Humedo;
        cuadro.Recalcular();
        Assert.Null(cuadro.AvisoAislamientoDelLugar);
        Assert.NotNull(Espacio(cuadro, 1).Resultado);
    }

    [Fact]
    public void P1_2_UnArchivoConHumedoOMojadoAbreComoMojadoYLoDice()
    {
        var cuadro = Nuevo();
        cuadro.Datos.TipoAislamiento = "XHHW";
        var texto = PowerNode.Web.Modelo.Archivo.ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now)
            .Replace("\"lugar\": \"Seco\"", "\"lugarSeco\": false")
            .Replace("\"version\": 11", "\"version\": 10");
        Assert.Contains("\"lugarSeco\": false", texto);

        var apertura = PowerNode.Web.Modelo.Archivo.ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Error);
        Assert.Equal(LugarDeInstalacion.Mojado, apertura.Cuadro!.Datos.Lugar);
        Assert.Contains(apertura.Avisos, a => a.Contains("«húmedo o mojado»") && a.Contains("mojado, la más estricta"));
    }
}
