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

    /// <summary>
    /// La Excepción 2 no se aplica sola: la declara el proyectista. 1 HP a 127 V en riel DIN: 32 A por
    /// la tabla; declarado que no arranca, hasta 400 % × 14 A = 56 A → 50 A, el mayor de riel DIN que
    /// no lo excede — 430-52(c)(1) Excepción 2(3).
    /// </summary>
    [Fact]
    public void P1_1_LaExcepcion2SoloDeclarada()
    {
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        var c = ConMotor(cuadro, 1, 1m, 1);
        Assert.Equal(32m, c.Resultado!.ProteccionA);
        Assert.DoesNotContain(c.Resultado.Citas, x => x.Referencia.Contains("Excepción 2"));

        c.NoArrancaConLaTabla = true;
        cuadro.Recalcular();

        var p = cuadro.ProteccionDelMotor(c);
        Assert.Equal(400m, p.PorcentajeExcepcion2);
        Assert.Equal(56m, p.TechoExcepcion2A);
        Assert.Equal(50m, c.Resultado!.ProteccionA);
        Assert.Equal(p.SeleccionadaA, c.Resultado.ProteccionA);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "430-52(c)(1) Excepción 2");
        Assert.Contains(cuadro.Desglose(c)!.Proteccion, l => l.Contains("Excepción 2(3)"));
        var hoja = MemoriaDeCalculo.DeCircuito(cuadro, c);
        Assert.Contains(hoja.Equipo!.Proteccion, f => f.Rotulo == "Protección seleccionada — 430-52(c)(1) Excepción 2(3)");

        // Se guarda en el archivo y se abre igual.
        var texto = PowerNode.Web.Modelo.Archivo.ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        var abierto = PowerNode.Web.Modelo.Archivo.ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!;
        Assert.Equal(50m, Espacio(abierto, 1).Resultado!.ProteccionA);
    }

    [Fact]
    public void P1_1_LaExcepcion2TambienConElMotorCapturadoEnElDesplegable()
    {
        // Como se captura en la pantalla (I-128): el motor es la línea del desplegable, un grupo de un
        // solo motor que se calcula como motor.
        var cuadro = Nuevo();
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        var motor = c.AgregarCarga();
        motor.Subtipo = SubtipoDeCarga.MotorUsoGeneral;
        motor.Hp = 1m;
        cuadro.Recalcular();

        Assert.True(c.EsGrupo);
        Assert.True(c.EsMotorSolo);
        Assert.Equal(32m, c.Resultado!.ProteccionA);

        c.NoArrancaConLaTabla = true;
        cuadro.Recalcular();
        Assert.Equal(50m, c.Resultado!.ProteccionA);
        Assert.Contains(cuadro.Desglose(c)!.Proteccion, l => l.Contains("Excepción 2(3)"));

        // Con otra carga ya no es un motor solo: 430-53, sin Excepción 2.
        var otra = c.AgregarCarga();
        otra.Subtipo = SubtipoDeCarga.Luminarias;
        otra.CargaUnitaria = 100m;
        cuadro.Recalcular();
        Assert.False(c.EsMotorSolo);
    }

    [Fact]
    public void P1_1_LaExcepcion2ArribaDe100AEsAl300()
    {
        // 50 HP a 220 V: FLC 130 A (Tabla 430-250, col. 230 V), más de 100 A → 300 % = 390 A → 350 A.
        // Por la tabla: 250 % = 325 A → 350 A (Excepción 1). La Excepción 2 no da nada mayor.
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 50m, 3);
        c.NoArrancaConLaTabla = true;
        cuadro.Recalcular();

        var p = cuadro.ProteccionDelMotor(c);
        Assert.Equal(130m, c.FlcA);
        Assert.Equal(300m, p.PorcentajeExcepcion2);
        Assert.Equal(350m, p.ProteccionA);
        Assert.False(p.UsaExcepcion2);
        Assert.Equal(350m, c.Resultado!.ProteccionA);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "430-52(c)(1) Excepción 2" && x.Descripcion.Contains("Se queda 350 A"));
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

    // ---- P2-2 · Calibres sin R ni X en la Tabla 9 ---------------------------------------------------

    /// <summary>
    /// 150 kVA continua a 220 V trifásica: 393.65 A, 125 % = 492.06 A → 900 kcmil a 75 °C (520 A; 800
    /// da 490), protección de 500 A. La Tabla 9 no trae R ni X para 900 kcmil: antes se saltaba a 1000
    /// «porque la caída excedía» y la tierra subía de 2 a 1 AWG por 250-122(b). Con la R y la X de 750
    /// kcmil —que dan más caída— 900 kcmil cumple de sobra: se queda, y la tierra también.
    /// </summary>
    [Fact]
    public void P2_2_UnHuecoDeLaTabla9NoSeAtribuyeALaCaida()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        var a = c.AgregarCarga();
        a.Subtipo = SubtipoDeCarga.OtraCargaEspecifica;
        a.Unidad = UnidadConsumo.VoltAmperes;
        a.CargaUnitaria = 150000m;
        a.Continua = true;
        cuadro.Recalcular();

        var r = c.Resultado!;
        Assert.Equal(393.65m, Math.Round(r.CorrienteDisenoA, 2));
        Assert.Equal(500m, r.ProteccionA);
        Assert.Equal("900", r.CalibreFase.Designacion);
        Assert.Equal("2", r.CalibreTierra.Designacion);
        Assert.DoesNotContain(r.Citas, x => x.Descripcion.Contains("excedía"));
        Assert.DoesNotContain(r.Citas, x => x.Referencia.StartsWith("250-122(b)"));
        var cita = Assert.Single(r.Citas, x => x.Referencia == "Tabla 9");
        Assert.StartsWith("La Tabla 9 no trae R ni X para 900", cita.Descripcion);
        Assert.Contains("las de 750", cita.Descripcion);
        Assert.True(r.CaidaTensionPct < 1m);
        // La memoria imprime la R y la X que se usaron, y dice de dónde salen.
        Assert.True(r.Detalle!.ResistenciaOhmKm > 0m);
        var hoja = MemoriaDeCalculo.DeCircuito(cuadro, c);
        Assert.Contains(MemoriaDeCalculo.Secciones(hoja), b => b.Notas.Any(n => n.StartsWith("La Tabla 9 no trae R ni X para 900")));
    }

    // ---- P1-3 · «Congelamiento» al reducir espacios o fases ---------------------------------------

    /// <summary>
    /// Los cuatro casos de la auditoría. No había ciclo: en el navegador, cada uno abría una pregunta
    /// con <c>window.confirm</c>, que detiene la página hasta que se contesta —vista desde una
    /// herramienta que no ve el diálogo nativo, una pestaña congelada—; contestada, el cambio tardaba
    /// 84 ms. La pregunta es ahora un diálogo dentro de la página (<c>archivo.js</c>). Aquí: que cada
    /// caso sí pregunta, y que aplicar el cambio termina con el tablero consistente.
    /// </summary>
    [Theory]
    [InlineData(24, 7, 3, 6)]    // (a) 3 polos en 7-9-11, Espacios 24 → 6
    [InlineData(24, 13, 3, 12)]  // (b) 3 polos en 13-15-17, Espacios 24 → 12
    [InlineData(24, 15, 1, 12)]  // (c) 1 polo en el 15, Espacios 24 → 12
    public void P1_3_ReducirLosEspaciosPreguntaYTermina(int espacios, int espacio, int polos, int nuevos)
    {
        var cuadro = Nuevo(espacios: espacios);
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        c.NoContinua = 900m;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        cuadro.Recalcular();

        var aviso = cuadro.AvisoAlCambiar(nuevos, cuadro.Datos.MaximoPolos);
        Assert.NotNull(aviso);
        Assert.Contains($"circuito {espacio}", aviso);

        cuadro.Datos.NumeroEspacios = nuevos;
        cuadro.Recalcular();

        Assert.Equal(nuevos, cuadro.Circuitos.Count);
        Assert.DoesNotContain(cuadro.Circuitos, x => x.TieneCarga);
        Assert.Null(cuadro.AvisoAlCambiar(nuevos, cuadro.Datos.MaximoPolos));
    }

    [Fact]
    public void P1_3_BajarLasFasesConUnBipolarPreguntaYTermina()
    {
        // (d) Un circuito de 2 polos y Fases 3 → 1: pasa a 1 polo, y se dice antes.
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        c.NoContinua = 2000m;
        Assert.Null(cuadro.CambiarPolos(c, 2));
        cuadro.Recalcular();

        var (etiqueta, espacios, maximoPolos) = cuadro.Datos.AlCambiar(1);
        var aviso = cuadro.AvisoAlCambiar(espacios, maximoPolos, etiqueta);
        Assert.NotNull(aviso);
        Assert.Contains("pasa de 2 a 1 polo", aviso);

        cuadro.Datos.Fases = 1;
        cuadro.Recalcular();

        Assert.Equal(1, c.Polos);
        Assert.NotNull(c.Resultado);
    }

    // ---- P2-1 · 3F-3H (delta) sin neutro ---------------------------------------------------------

    [Fact]
    public void P2_1_EnDeltaUnCircuitoDeUnPoloNoSeCalcula()
    {
        // 500 VA de alumbrado en 1 polo de un 3F-3H: sin neutro no hay regreso. Antes salía a 220 V
        // (2.27 A) con fase y tierra. En 2 polos sí: entre fases, 500 / 220 = 2.27 A.
        var cuadro = Nuevo(hilos: 3);
        Assert.False(cuadro.SistemaConNeutro);
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        c.NoContinua = 500m;
        cuadro.Recalcular();

        Assert.Null(c.Resultado);
        Assert.Equal(CuadroDeCarga.MensajeUnPoloSinNeutro, c.Error);

        Assert.Null(cuadro.CambiarPolos(c, 2));
        cuadro.Recalcular();
        Assert.Null(c.Error);
        Assert.Equal(2.27m, Math.Round(c.Resultado!.CorrienteDisenoA, 2));
    }

    // ---- P3-4 · ICFT — 210-8 -------------------------------------------------------------------

    private static CircuitoDelCuadro Contactos(CuadroDeCarga cuadro, int espacio, SubtipoDeCarga subtipo, int cantidad = 2)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.Contactos;
        var a = c.AgregarCarga();
        a.Subtipo = subtipo;
        a.Cantidad = cantidad;
        a.CargaUnitaria = 180m;
        cuadro.Recalcular();
        return c;
    }

    [Theory]
    [InlineData(TipoDeInmueble.ViviendaUnifamiliar, SubtipoDeCarga.ContactoBano, "210-8(a)(1)")]
    [InlineData(TipoDeInmueble.Otro, SubtipoDeCarga.ContactoBano, "210-8(b)(1)")]
    [InlineData(TipoDeInmueble.ViviendaUnifamiliar, SubtipoDeCarga.ContactoAparatosPequenos, "210-8(a)(6)")]
    public void P3_4_LosContactosDeBanoYCocinaRequierenIcft(TipoDeInmueble inmueble, SubtipoDeCarga subtipo, string referencia)
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = inmueble;
        var c = Contactos(cuadro, 1, subtipo);

        var regla = Assert.Single(c.ReglasDeClase, x => x.Referencia.StartsWith("210-8"));
        Assert.Equal(referencia, regla.Referencia);
        Assert.False(regla.Aviso); // es requisito, no incumplimiento
        Assert.Contains("ICFT", regla.Texto);
        // En la memoria, con las demás reglas del circuito.
        Assert.Contains(MemoriaDeCalculo.DeCircuito(cuadro, c).ReglasDeClase!, x => x.Referencia == referencia);
    }

    [Fact]
    public void P3_4_UsoGeneralYAfciNoSeExigen()
    {
        // Contactos de uso general: 210-8 no los nombra. Y 210-12(a) de la NOM es «se podrán proteger»: no hay
        // regla de ICFA en ningún circuito.
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        var c = Contactos(cuadro, 1, SubtipoDeCarga.ContactoUsoGeneral, 6);

        Assert.DoesNotContain(c.ReglasDeClase, x => x.Referencia.StartsWith("210-8") || x.Referencia.StartsWith("210-12"));
    }

    // ---- Riesgo 6 · Contactos a 277 V -----------------------------------------------------------

    [Theory]
    [InlineData(TipoDeInmueble.Otro, "210-6(c)(6)", false)]
    [InlineData(TipoDeInmueble.ViviendaUnifamiliar, "210-6(a)(2)", true)]
    public void R6_LosContactosA277VLlevanSuNota(TipoDeInmueble inmueble, string referencia, bool aviso)
    {
        // 480Y/277 V, contactos de uso general en 1 polo: 277 V a tierra.
        var cuadro = Nuevo(tension: 480m);
        cuadro.Datos.Inmueble = inmueble;
        var c = Contactos(cuadro, 1, SubtipoDeCarga.ContactoUsoGeneral, 4);

        var regla = Assert.Single(c.ReglasDeClase, x => x.Referencia.StartsWith("210-6"));
        Assert.Equal(referencia, regla.Referencia);
        Assert.Equal(aviso, regla.Aviso);
        Assert.Contains("277 V", regla.Texto);

        // A 127 V no hay nada que decir.
        var comun = Contactos(Nuevo(), 1, SubtipoDeCarga.ContactoUsoGeneral, 4);
        Assert.DoesNotContain(comun.ReglasDeClase, x => x.Referencia.StartsWith("210-6"));
    }
}
