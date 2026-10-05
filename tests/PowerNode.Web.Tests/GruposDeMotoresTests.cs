using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Varios motores, o motores y otras cargas, en un circuito</b> — I-115, 430-53. Los números salen de
/// la norma, no de la implementación: FLC de la Tabla 430-248 (½ HP a 127 V: 8.9 A; 1 HP: 14 A) y de la
/// 430-250 (5 HP a 230 V: 15.2 A; ½ HP: 2.2 A); conductor por 430-24 (125 % del mayor + los demás +
/// 125 % de la continua); protección por 430-53(c)(4): el mayor tamaño estándar que no exceda el 250 %
/// del motor mayor más los demás y las otras cargas (decisión de David, 2026-09-29).
/// </summary>
public class GruposDeMotoresTests
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

    /// <summary>Un circuito de Motor en «Varios», con sus polos y 10 m para que la caída no mueva el calibre.</summary>
    private static CircuitoDelCuadro Grupo(CuadroDeCarga cuadro, int espacio, int polos)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.Motor;
        c.LongitudM = 10m;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        c.PasarAGrupo();
        return c;
    }

    private static CargaDelCircuito MotorHp(CircuitoDelCuadro c, string nombre, decimal hp, int cantidad = 1)
    {
        var a = c.AgregarMotor();
        a.Descripcion = nombre;
        a.Hp = hp;
        a.Cantidad = cantidad;
        return a;
    }

    private static decimal Flc(decimal hp, int polos, decimal tension) =>
        Motor.FlcMotor.CorrientePlenaCargaA(hp, MotoresEnHp.Alimentacion(polos), tension)!.Value;

    [Fact]
    public void I115_TresMotoresDeMedioHpEnUnCircuito()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 1);
        MotorHp(c, "Extractor", 0.5m, cantidad: 3);
        cuadro.Recalcular();

        Assert.Null(c.Error);
        Assert.True(c.EsGrupo && c.TieneDesglose);
        Assert.Equal(8.9m, c.Cargas[0].CorrienteUnitariaA);
        Assert.Equal(26.7m, c.CorrienteDeMotorA);
        var r = c.Resultado!;
        // 430-24: 125 % × 8.9 + 8.9 + 8.9 = 28.925 A → 10 AWG (30 A a 60 °C).
        Assert.Equal(28.925m, r.Detalle!.CapacidadMinimaA);
        Assert.Equal("10", r.CalibreFase.Designacion);
        // 430-53(c)(4): 250 % × 8.9 + 8.9 + 8.9 = 40.05 A → 40 A, el mayor que no lo excede.
        Assert.Equal(40.05m, r.Grupo!.TechoA);
        Assert.Equal("430-53(c)(4)", r.Grupo.Regla);
        Assert.Equal(40m, r.ProteccionA);
        Assert.Equal(26.7m, r.CorrienteDisenoA);
        Assert.Contains(r.Citas, x => x.Referencia == "430-24");
        Assert.Contains(r.Citas, x => x.Referencia == "240-4(g)");
        Assert.Contains(r.Citas, x => x.Referencia == "430-53(c)");
        // 8.9 A pasa de los 6 A de 430-53(a)(1): esa regla no aplica.
        Assert.DoesNotContain(r.Citas, x => x.Referencia == "430-53(a)");
    }

    [Fact]
    public void I115_UnMotorGrandeYUnoChicoTrifasicos()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 3);
        MotorHp(c, "Compresor de aire", 5m);
        MotorHp(c, "Ventilador", 0.5m);
        cuadro.Recalcular();

        var r = c.Resultado!;
        Assert.Null(c.Error);
        // 430-24: 125 % × 15.2 + 2.2 = 21.2 A → 10 AWG (12 AWG da 20 A a 60 °C).
        Assert.Equal(21.2m, r.Detalle!.CapacidadMinimaA);
        Assert.Equal("10", r.CalibreFase.Designacion);
        // 430-53(c)(4): 250 % × 15.2 + 2.2 = 40.2 A → 40 A.
        Assert.Equal(40.2m, r.Grupo!.TechoA);
        Assert.Equal("Compresor de aire", r.Grupo.Mayor.Nombre);
        Assert.Equal(40m, r.ProteccionA);
    }

    [Fact]
    public void I115_UnGrupoDeUnSoloMotorSeCalculaComoMotor()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Descripcion = "Bomba";
        c.Hp = 5m;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        cuadro.Recalcular();
        Assert.Equal(40m, c.Resultado!.ProteccionA);

        // El motor capturado pasa a ser el primero del grupo, y el resultado no cambia: 250 % × 15.2 = 38 A
        // sube a 40 A por 430-52(c)(1) Excepción 1, que 430-53(c)(4) no trae (daría 35 A).
        c.PasarAGrupo();
        cuadro.Recalcular();

        Assert.True(c.EsGrupo);
        var motor = Assert.Single(c.Cargas);
        Assert.Equal(ClaseDeAparato.Motor, motor.Clase);
        Assert.Equal("Bomba", motor.Descripcion);
        Assert.Equal(5m, motor.Hp);
        Assert.Null(c.Resultado!.Grupo);
        Assert.Equal(40m, c.Resultado.ProteccionA);
        Assert.Contains("FLC = 15.20 A", cuadro.OrigenDeLaFlc(c));
    }

    [Fact]
    public void I115_UnMotorYAlumbradoEnElMismoCircuito()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 1);
        MotorHp(c, "Extractor", 1m);
        var luz = c.AgregarCarga();
        luz.Descripcion = "Alumbrado";
        luz.CargaUnitaria = 1000m;
        luz.Continua = true;
        cuadro.Recalcular();

        var r = c.Resultado!;
        Assert.Null(c.Error);
        var iLuz = 1000m / cuadro.Datos.TensionFaseNeutroV; // 220 / √3 = 127.02 V
        // 430-24: 125 % × 14 + 125 % × 7.874 = 27.34 A.
        Assert.Equal(1.25m * 14m + 1.25m * iLuz, r.Detalle!.CapacidadMinimaA);
        // 430-53(c)(4): 250 % × 14 + 7.874 = 42.87 A → 40 A.
        Assert.Equal(35m + iLuz, r.Grupo!.TechoA);
        Assert.Equal(40m, r.ProteccionA);
        // La otra carga lleva su propia protección si el interruptor del grupo pasa sus derivaciones.
        Assert.Contains(r.Citas, x => x.Referencia == "430-53(c)(6)");
        Assert.Equal(1000m, c.ContinuaVA);
        Assert.Equal(14m * cuadro.Datos.TensionFaseNeutroV, c.MotorVA);
    }

    [Fact]
    public void I115_MotoresChicosCumplenTambien430_53a()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 1);
        MotorHp(c, "Ventilador", 1m / 6m, cantidad: 2);
        cuadro.Recalcular();

        var flc = Flc(1m / 6m, 1, 127m);
        Assert.True(flc <= 6m);
        var r = c.Resultado!;
        Assert.Equal(3.5m * flc, r.Grupo!.TechoA);
        Assert.True(r.ProteccionA <= 20m);
        Assert.Contains(r.Citas, x => x.Referencia == "430-53(a)");
    }

    [Fact]
    public void I115_SiElLimiteNoLlevaLaCargaSubeHasta240_4b()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 1);
        MotorHp(c, "Ventilador", 1m / 6m);
        var luz = c.AgregarCarga();
        luz.Descripcion = "Alumbrado";
        luz.CargaUnitaria = 3300m;
        luz.Continua = true;
        cuadro.Recalcular();

        var flc = Flc(1m / 6m, 1, 127m);
        var iLuz = 3300m / cuadro.Datos.TensionFaseNeutroV;
        var r = c.Resultado!;
        Assert.Null(c.Error);
        var g = r.Grupo!;
        // 250 % × FLC + 25.98 A queda abajo de lo que el alumbrado continuo pide al interruptor
        // (FLC + 125 % × 25.98 A): el límite no deja un tamaño que lo lleve, y el conductor sí.
        Assert.Equal(2.5m * flc + iLuz, g.TechoA);
        Assert.Equal(flc + 1.25m * iLuz, g.PisoA);
        Assert.NotNull(g.Limite240_4bA);
        Assert.True(r.ProteccionA >= g.PisoA);
        Assert.True(r.ProteccionA <= g.Limite240_4bA);
        Assert.Contains(r.Citas, x => x.Referencia == "430-53(c)(4)" && x.Descripcion.Contains("240-4(b)"));
    }

    [Fact]
    public void I115_UnMotorQueSoloTieneAlumbradoPasaAAlumbrado()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 1);
        var luz = c.AgregarCarga();
        luz.Subtipo = SubtipoDeCarga.Luminarias; // capturada en el desplegable — I-123
        luz.CargaUnitaria = 500m;
        cuadro.Recalcular();

        // Desde captura-en-el-desplegable.md el tipo del circuito sigue a sus cargas: sin motores, ya no es
        // Motor; es un circuito de alumbrado.
        Assert.Equal(CategoriaDeCarga.Alumbrado, c.Categoria);
        Assert.False(c.EsGrupo);
        Assert.Null(c.Error);
        Assert.Equal(500m, c.CargaInstaladaVA);
    }

    [Fact]
    public void I115_UnMotorQueLaTablaNoTraeLoDiceConSuNombre()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 1);
        MotorHp(c, "Extractor", 0.5m);
        MotorHp(c, "Bomba grande", 50m); // la 430-248 no trae monofásicos de 50 HP
        cuadro.Recalcular();

        Assert.Null(c.Resultado);
        Assert.StartsWith("Bomba grande:", c.Error);
    }

    [Fact]
    public void I115_ElAlimentadorCuentaCadaMotorDelGrupo()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 1);
        MotorHp(c, "Extractor", 0.5m, cantidad: 3);
        cuadro.Recalcular();

        // En la fase A, tres motores de 8.9 A: 125 % del mayor + los otros dos, igual que si fueran
        // tres circuitos — 430-24. El 125 % no es de los 26.7 A del circuito.
        var fase = cuadro.Alimentador.Fases.Single(f => f.Fase == 'A');
        Assert.Equal(8.9m, fase.Motores.MayorFlcA);
        Assert.Equal(17.8m, fase.Motores.SumaRestoFlcA);
        Assert.Equal(28.925m, fase.Motores.CapacidadMinimaA);
        // 430-62(a): la protección del grupo cubre a sus tres motores; ninguno queda en «los demás».
        Assert.Equal(40m, fase.Motores.MayorProteccionDerivadoA);
        Assert.Equal(26.7m, fase.Motores.FlcDelMayorProteccionA);
        Assert.Equal(40m, cuadro.Alimentador.Resultado!.TechoProteccion430_62A);
    }

    [Fact]
    public void I115_LasOtrasCargasDelGrupoVanAlAlimentadorComoCarga()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 1);
        MotorHp(c, "Extractor", 1m);
        var luz = c.AgregarCarga();
        luz.CargaUnitaria = 1000m;
        luz.Continua = true;
        cuadro.Recalcular();

        var fase = cuadro.Alimentador.Fases.Single(f => f.Fase == 'A');
        var iLuz = 1000m / cuadro.Datos.TensionFaseNeutroV;
        Assert.Equal(iLuz, fase.ContinuaA);
        Assert.Equal(14m, fase.Motores.MayorFlcA);
        // 125 % × 7.874 + 125 % × 14.
        Assert.Equal(1.25m * iLuz + 17.5m, fase.CapacidadA);
    }

    [Fact]
    public void I115_ElDesgloseYLaMemoriaDicenElGrupo()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 3);
        MotorHp(c, "Compresor de aire", 5m);
        MotorHp(c, "Ventilador", 0.5m);
        cuadro.Recalcular();

        var d = cuadro.Desglose(c)!;
        Assert.Contains(d.Proteccion, x => x.StartsWith("Compresor de aire: 15.20 A"));
        Assert.Contains(d.Proteccion, x => x.Contains("250 % × 15.20 A (Compresor de aire) + 2.20 A") && x.EndsWith("= 40.20 A — 430-53(c)(4)"));
        Assert.Contains(d.Proteccion, x => x.StartsWith("Protección: 40 A, el mayor tamaño estándar"));
        Assert.Contains(d.Conductor, x => x.StartsWith("Capacidad mínima = 125 % × 15.2 A") && x.EndsWith("— 430-24"));

        var hoja = MemoriaDeCalculo.DeCircuito(cuadro, c);
        Assert.Equal("Grupo de motores", hoja.Equipo!.Rotulo);
        Assert.Contains(hoja.Equipo.Proteccion, x => x.Rotulo == "Protección máxima — 430-53(c)(4)" && x.Valor.EndsWith("= 40.2 A"));
        Assert.Contains(hoja.Equipo.Proteccion, x => x.Rotulo == "Capacidad mínima del conductor — 430-24" && x.Valor.EndsWith("= 21.2 A"));
        Assert.Contains(hoja.Desglose!, x => x.Rotulo == "1. Motores: Compresor de aire" && x.Valor.StartsWith("1 × 5 HP · 15.20 A"));
        var texto = string.Join("\n", MemoriaDeCalculo.Secciones(hoja).SelectMany(s => s.Renglones).Select(x => $"{x.Rotulo} {x.Valor}"));
        Assert.Contains("Compresor de aire", texto);
        Assert.Contains("430-53(c)(4)", texto);
    }

    [Fact]
    public void I115_MoverYDeshacerUnGrupoLoConserva()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 1);
        MotorHp(c, "Extractor", 0.5m, cantidad: 3);
        cuadro.Recalcular();

        Assert.True(cuadro.MoverCircuito(1, 5).Movio);
        var movido = Espacio(cuadro, 5);
        Assert.True(movido.EsGrupo);
        Assert.Equal(3, movido.Cargas.Single().Cantidad);
        Assert.Equal(40m, movido.Resultado!.ProteccionA);

        cuadro.Deshacer();
        Assert.True(Espacio(cuadro, 1).EsGrupo);
        Assert.Equal(40m, Espacio(cuadro, 1).Resultado!.ProteccionA);
    }

    // ---- Aparato con motor en un desglose de carga: 220-18(a) — I-118 ---------------------------

    [Fact]
    public void I118_UnAparatoConMotorYOtrasCargasLlevaElMotorAl125()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var luz = c.AgregarCarga();
        luz.Descripcion = "Alumbrado";
        luz.CargaUnitaria = 1000m;
        var lavadora = c.AgregarCarga();
        lavadora.Descripcion = "Lavadora";
        lavadora.Clase = ClaseDeAparato.Motor;
        lavadora.CapturaMotor = CapturaDeMotor.Amperes;
        lavadora.CorrientePlacaA = 5m; // entre ⅙ HP (4.0 A) y ¼ HP (5.3 A) a 127 V: más de ⅛ hp
        cuadro.Recalcular();

        Assert.Null(c.Error);
        var v = cuadro.Datos.TensionFaseNeutroV;
        Assert.Same(lavadora, c.MotorAl125);
        // El motor mayor, como continua (125 %); el alumbrado, no continua (100 %).
        Assert.Equal(5m * v, c.ContinuaVA);
        Assert.Equal(1000m, c.NoContinuaVA);
        // 210-19(a)(1): 125 % × 5 A + 1000 VA ÷ V = 6.25 + 7.87 A.
        Assert.Equal(Math.Round(6.25m + 1000m / v, 10), Math.Round(c.Resultado!.Detalle!.CapacidadMinimaA, 10));
        Assert.Contains(cuadro.Desglose(c)!.Proteccion, x => x.StartsWith("Lavadora, el motor mayor: 5.00 A") && x.EndsWith("— 220-18(a)"));
        var hoja = MemoriaDeCalculo.DeCircuito(cuadro, c);
        Assert.Contains("220-18(a)", hoja.NotaDelMotor);
        Assert.Contains(MemoriaDeCalculo.Secciones(hoja)[2].Renglones, r => r.Rotulo == "Aparato con motor — 220-18(a)");
    }

    [Fact]
    public void I118_DosMotoresSoloElMayorVaAl125()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var luz = c.AgregarCarga();
        luz.CargaUnitaria = 500m;
        var chico = c.AgregarCarga();
        chico.Clase = ClaseDeAparato.Motor;
        chico.Hp = 0.25m;
        var grande = c.AgregarCarga();
        grande.Clase = ClaseDeAparato.Motor;
        grande.Hp = 0.5m;
        grande.Cantidad = 2;
        cuadro.Recalcular();

        var v = cuadro.Datos.TensionFaseNeutroV;
        var flcChico = Flc(0.25m, 1, 127m);
        Assert.Same(grande, c.MotorAl125);
        // Una unidad de ½ HP al 125 %; la otra y el de ¼ HP, al 100 %.
        Assert.Equal(8.9m * v, c.ContinuaVA);
        Assert.Equal(Math.Round(500m + (8.9m + flcChico) * v, 10), Math.Round(c.NoContinuaVA, 10));
    }

    [Fact]
    public void I118_SoloMotoresEnUnDesgloseDeEquipoVanPorElArt430()
    {
        // Antes (I-118) avisaba y no calculaba. Desde I-123 el grupo sale de lo que lleva el circuito: un
        // aparato con motor solo se calcula por el Art. 430 — 220-18(a), 422-10(a).
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var motor = c.AgregarCarga();
        motor.Clase = ClaseDeAparato.Motor;
        motor.Hp = 0.5m;
        cuadro.Recalcular();

        Assert.True(c.EsGrupo);
        Assert.Null(c.Error);
        Assert.Equal(motor.CorrienteUnitariaA, c.Resultado!.CorrienteDisenoA);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia.StartsWith("430-52", StringComparison.Ordinal));
    }

    [Fact]
    public void I118_UnMotorDeHastaUnOctavoDeHpNoSubeAl125()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Equipo;
        var luz = c.AgregarCarga();
        luz.CargaUnitaria = 500m;
        var ventilador = c.AgregarCarga();
        ventilador.Clase = ClaseDeAparato.Motor;
        ventilador.CapturaMotor = CapturaDeMotor.Amperes;
        ventilador.CorrientePlacaA = 2m; // interpolado desde 0: 2 A ÷ 4 A × ⅙ HP = 0.083 HP
        cuadro.Recalcular();

        Assert.Null(c.MotorAl125);
        Assert.Equal(0m, c.ContinuaVA);
        Assert.Equal(500m + 2m * cuadro.Datos.TensionFaseNeutroV, c.NoContinuaVA);
    }

    // ---- A/C en grupo: 440-22(b), 440-33 — I-116 -----------------------------------------------

    private static CircuitoDelCuadro AireEnGrupo(CuadroDeCarga cuadro, int espacio, int polos)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        c.LongitudM = 10m;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        c.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
        c.PasarAGrupoDeAire();
        return c;
    }

    private static CargaDelCircuito Compresor(CircuitoDelCuadro c, decimal rla, decimal? seleccion = null)
    {
        var a = c.AgregarMotor(ClaseDeAparato.Motocompresor);
        a.Descripcion = "Compresor";
        a.CorrientePlacaA = rla;
        a.CorrienteSeleccionA = seleccion;
        return a;
    }

    [Fact]
    public void I116_CompresorYVentiladorSinMcaDeConjunto()
    {
        var cuadro = Nuevo();
        var c = AireEnGrupo(cuadro, 1, 2);
        Compresor(c, 12m);
        var ventilador = c.AgregarMotor();
        ventilador.Descripcion = "Ventilador";
        ventilador.CapturaMotor = CapturaDeMotor.Amperes;
        ventilador.CorrientePlacaA = 1.2m;
        cuadro.Recalcular();

        Assert.Null(c.Error);
        Assert.True(c.EsGrupo);
        var r = c.Resultado!;
        // 440-33: 12 A + 1.2 A + 25 % × 12 A = 16.2 A → 12 AWG (14 AWG da 15 A a 60 °C).
        Assert.Equal(16.2m, r.Detalle!.CapacidadMinimaA);
        Assert.Equal("12", r.CalibreFase.Designacion);
        Assert.Contains(r.Citas, x => x.Referencia == "440-33");
        // 440-22(b)(1): el motocompresor es la carga más grande: 175 % × 12 + 1.2 = 22.2 A → 20 A.
        Assert.Equal("440-22(b)(1)", r.Grupo!.Regla);
        Assert.Equal(22.2m, r.Grupo.TechoA);
        Assert.Equal(20m, r.ProteccionA);

        // No arranca al 175 %: 225 % × 12 + 1.2 = 28.2 A → 25 A.
        c.ArranqueAl225 = true;
        cuadro.Recalcular();
        Assert.Equal(28.2m, c.Resultado!.Grupo!.TechoA);
        Assert.Equal(25m, c.Resultado.ProteccionA);
    }

    [Fact]
    public void I116_UnSoloMotocompresorVaPor440_22a()
    {
        var cuadro = Nuevo();
        var c = AireEnGrupo(cuadro, 1, 2);
        Compresor(c, 15m, seleccion: 16m);
        cuadro.Recalcular();

        var r = c.Resultado!;
        // La corriente de selección es mayor: cuenta ella — 440-6(a) Exc. 1. 125 % × 16 = 20 A — 440-32.
        Assert.Equal(20m, r.Detalle!.CapacidadMinimaA);
        Assert.Contains(r.Citas, x => x.Referencia == "440-32");
        // 440-22(a): 175 % × 16 = 28 A → 25 A, sin redondear hacia arriba.
        Assert.Equal("440-22(a)", r.Grupo!.Regla);
        Assert.Equal(25m, r.ProteccionA);
    }

    [Fact]
    public void I116_SiLaCargaMasGrandeNoEsElMotocompresor()
    {
        var cuadro = Nuevo();
        var c = AireEnGrupo(cuadro, 1, 2);
        Compresor(c, 5m);
        var resistencia = c.AgregarCarga();
        resistencia.Descripcion = "Resistencia";
        resistencia.Unidad = UnidadConsumo.Amperes;
        resistencia.CargaUnitaria = 20m;
        cuadro.Recalcular();

        var r = c.Resultado!;
        Assert.Null(c.Error);
        // 440-34: 5 A + 25 % × 5 A + 20 A = 26.25 A.
        Assert.Equal(26.25m, r.Detalle!.CapacidadMinimaA);
        // 440-22(b)(2), solo con cargas que no son de motor: 5 A + lo de 240-4 para 20 A (20 A) = 25 A.
        Assert.Equal("440-22(b)(2)", r.Grupo!.Regla);
        Assert.Equal(25m, r.Grupo.TechoA);
        Assert.Equal(25m, r.ProteccionA);
    }

    [Fact]
    public void I116_ElAlimentadorCuentaCadaMotocompresor()
    {
        var cuadro = Nuevo();
        var c = AireEnGrupo(cuadro, 1, 3);
        Compresor(c, 10m);
        Compresor(c, 10m);
        cuadro.Recalcular();

        var fase = cuadro.Alimentador.Fases.Single(f => f.Fase == 'A');
        Assert.Equal(10m, fase.Motores.MayorFlcA);
        Assert.Equal(10m, fase.Motores.SumaRestoFlcA);
        Assert.Equal(c.Resultado!.ProteccionA, fase.Motores.MayorProteccionDerivadoA);
        Assert.Equal(20m, fase.Motores.FlcDelMayorProteccionA);
    }

    // ---- Acondicionador de habitación: 440 Parte G — I-117 -------------------------------------

    private static CircuitoDelCuadro DeCuarto(CuadroDeCarga cuadro, int espacio, int polos, decimal corriente)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        c.LongitudM = 10m;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        c.PlacaAire = PlacaDeAireAcondicionado.Habitacion;
        c.CorrientePlacaA = corriente;
        cuadro.Recalcular();
        return c;
    }

    [Theory]
    // 440-62(b): 12 A ÷ 0.8 = 15 A → 15 A; el conductor la cubre: 14 AWG (15 A).
    [InlineData(12, 15, "14")]
    // 13 A ÷ 0.8 = 16.25 A → 20 A; 12 AWG (20 A).
    [InlineData(13, 20, "12")]
    public void I117_ElDeHabitacionNoPasaDel80PorCientoDelCircuito(decimal corriente, decimal proteccion, string calibre)
    {
        var cuadro = Nuevo();
        var c = DeCuarto(cuadro, 1, 1, corriente);

        Assert.Null(c.Error);
        var r = c.Resultado!;
        Assert.Equal(proteccion, r.ProteccionA);
        Assert.Equal(calibre, r.CalibreFase.Designacion);
        Assert.True(r.Detalle!.AmpacidadConductorA >= r.ProteccionA);
        Assert.Contains(r.Citas, x => x.Referencia == "440-62(b)");
        Assert.DoesNotContain(r.Citas, x => x.Referencia == "240-4(g)");
        Assert.Contains(cuadro.Desglose(c)!.Proteccion, x => x.EndsWith("— 440-62(b)"));
        Assert.Contains(MemoriaDeCalculo.DeCircuito(cuadro, c).Equipo!.Proteccion, x => x.Rotulo == "Circuito mínimo — 440-62(b)");
    }

    [Fact]
    public void I117_UnoTrifasicoODeMasDe40ANoEsDeLaParteG()
    {
        var cuadro = Nuevo();
        Assert.Contains("440-60", DeCuarto(cuadro, 1, 3, 12m).Error);
        Assert.Contains("440-62(a)(2)", DeCuarto(cuadro, 2, 1, 45m).Error);
    }

    [Fact]
    public void I117_EnUnCircuitoDeContactosNoPasaDel50PorCiento()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Contactos;
        var contactos = c.AgregarCarga();
        contactos.Descripcion = "Contacto";
        contactos.Cantidad = 4;
        var ventana = c.AgregarCarga();
        ventana.Descripcion = "A/C de ventana";
        ventana.Clase = ClaseDeAparato.AireDeHabitacion;
        ventana.CorrientePlacaA = 12m;
        cuadro.Recalcular();

        Assert.Null(c.Error);
        // 720 VA + 12 A a 127 V: 17.67 A → 20 A; 12 A pasa del 50 % (10 A) — 440-62(c).
        Assert.Equal(20m, c.Resultado!.ProteccionA);
        Assert.Contains("440-62(c)", c.AvisoAireDeHabitacion);
        Assert.Contains(cuadro.AvisosDeCircuitos, x => x.StartsWith("Circuito 1: el acondicionador de habitación (12 A)"));
        Assert.Equal(12m * cuadro.Datos.TensionFaseNeutroV, ventana.TotalVA);

        // Solo, 13 A en un circuito de 15 A: pasa del 80 % — 440-62(b).
        c.Cargas.Remove(contactos);
        ventana.CorrientePlacaA = 13m;
        cuadro.Recalcular();
        Assert.Equal(15m, c.Resultado!.ProteccionA);
        Assert.Contains("440-62(b)", c.AvisoAireDeHabitacion);
        Assert.Contains("«De hab.»", c.AvisoAireDeHabitacion); // I-127
    }

    // ---- El archivo: formato 4 -----------------------------------------------------------------

    [Fact]
    public void I116_I117_ElAireEnGrupoYElDeCuartoSeGuardanYAbrenIgual()
    {
        var cuadro = Nuevo();
        var grupo = AireEnGrupo(cuadro, 1, 2);
        Compresor(grupo, 12m, seleccion: 13m);
        grupo.ArranqueAl225 = true;
        DeCuarto(cuadro, 2, 1, 12m); // el 3 es del 2 polos del 1
        var contactos = Espacio(cuadro, 4);
        contactos.Categoria = CategoriaDeCarga.Contactos;
        var ventana = contactos.AgregarCarga();
        ventana.Clase = ClaseDeAparato.AireDeHabitacion;
        ventana.CorrientePlacaA = 8m;
        cuadro.Recalcular();

        var texto = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Error);
        Assert.Empty(apertura.Avisos);
        Assert.Equal(ArchivoDelCuadro.Huella(cuadro), ArchivoDelCuadro.Huella(apertura.Cuadro!));
        Assert.Equal(
            cuadro.Circuitos.Select(c => (c.Espacio, c.Resultado?.ProteccionA, c.Resultado?.CalibreFase.Designacion)),
            apertura.Cuadro!.Circuitos.Select(c => (c.Espacio, c.Resultado?.ProteccionA, c.Resultado?.CalibreFase.Designacion)));
        Assert.Equal(PlacaDeAireAcondicionado.Habitacion, Espacio(apertura.Cuadro, 2).PlacaAire);
        Assert.Equal(ClaseDeAparato.AireDeHabitacion, Espacio(apertura.Cuadro, 4).Cargas.Single().Clase);
        Assert.True(Espacio(apertura.Cuadro, 1).ArranqueAl225);
    }

    // ---- El archivo: formato 3 -----------------------------------------------------------------

    [Fact]
    public void I115_ElGrupoSeGuardaYAbreIgual()
    {
        var cuadro = Nuevo();
        var c = Grupo(cuadro, 1, 3);
        MotorHp(c, "Compresor de aire", 5m);
        var ventilador = c.AgregarMotor();
        ventilador.Descripcion = "Ventilador";
        ventilador.CapturaMotor = CapturaDeMotor.Amperes;
        ventilador.CorrientePlacaA = 2m;
        var luz = c.AgregarCarga();
        luz.Descripcion = "Alumbrado";
        luz.CargaUnitaria = 300m;
        cuadro.Recalcular();

        var texto = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        Assert.Contains("\"version\": 13", texto);
        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Error);
        Assert.Empty(apertura.Avisos);
        var abierto = Espacio(apertura.Cuadro!, 1);
        Assert.True(abierto.EsGrupo);
        Assert.Equal(
            c.Cargas.Select(a => (a.Descripcion, a.Clase, a.CapturaMotor, a.Hp, a.CorrientePlacaA, a.CargaUnitaria)),
            abierto.Cargas.Select(a => (a.Descripcion, a.Clase, a.CapturaMotor, a.Hp, a.CorrientePlacaA, a.CargaUnitaria)));
        Assert.Equal(c.Resultado!.ProteccionA, abierto.Resultado!.ProteccionA);
        Assert.Equal(c.Resultado.Detalle!.CapacidadMinimaA, abierto.Resultado.Detalle!.CapacidadMinimaA);
        Assert.Equal(ArchivoDelCuadro.Huella(cuadro), ArchivoDelCuadro.Huella(apertura.Cuadro!));
    }

    [Fact]
    public void I115_UnArchivoDeFormato2AbreConSusAparatosComoCargas()
    {
        const string texto = """
            {
              "formato": "power-node/cuadro-de-carga",
              "version": 2,
              "circuitos": [
                { "espacio": 1, "categoria": "Equipo",
                  "aparatos": [ { "descripcion": "Horno", "cantidad": 1, "cargaUnitaria": 1500 } ] }
              ]
            }
            """;

        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Error);
        var a = Assert.Single(apertura.Cuadro!.Circuitos[0].Cargas);
        Assert.Equal(ClaseDeAparato.Carga, a.Clase);
        Assert.Equal(1500m, apertura.Cuadro.Circuitos[0].NoContinuaVA);
    }

    /// <summary>
    /// Un formato más nuevo con un valor que esta versión no conoce: pide recargar la página en vez de
    /// decir que el archivo no es de Power Node.
    /// </summary>
    [Fact]
    public void I115_UnFormatoMasNuevoConValoresDesconocidosPideRecargar()
    {
        const string texto = """
            {
              "formato": "power-node/cuadro-de-carga",
              "version": 14,
              "circuitos": [ { "espacio": 1, "capturaMotor": "AlgoNuevo" } ]
            }
            """;

        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Cuadro);
        Assert.Contains("versión más nueva", apertura.Error);
    }
}
