using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Motores en HP</b> — I-15, Art. 430. Los números salen de las tablas de la norma, no de la
/// implementación: FLC de la Tabla 430-250 (5 HP a 230 V: 15.2 A; 3 HP: 9.6 A) y de la 430-248 (1 HP
/// a 127 V: 14 A; a 230 V: 8 A); 250 % de la Tabla 430-52 para interruptor de tiempo inverso; 125 %
/// de 430-22; y en el alimentador, 125 % del mayor más la suma de los demás (430-24), por fase.
/// </summary>
public class MotoresTests
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
        return EnPvc.Todo(cuadro);
    }

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int numero) =>
        cuadro.Circuitos.Single(c => c.Espacio == numero);

    /// <summary>Un motor en HP en el espacio <paramref name="espacio"/>, con sus polos.</summary>
    private static CircuitoDelCuadro ConMotor(CuadroDeCarga cuadro, int espacio, decimal hp, int polos)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.MotorOAireAcondicionado;
        c.Hp = hp;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        cuadro.Recalcular();
        return c;
    }

    // ---- El derivado --------------------------------------------------------------------------

    [Fact]
    public void I15_UnMotorTrifasicoSeCalculaConElArt430()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 5m, 3);

        Assert.True(c.EsMotor);
        Assert.Equal(TipoCarga.Fuerza, c.Tipo);
        Assert.Null(c.Error);
        var r = c.Resultado!;
        // Tabla 430-250, columna de 230 V: 220 V está en el intervalo de 220 a 240 V.
        Assert.Equal(15.2m, c.FlcA);
        Assert.Equal(15.2m, r.CorrienteDisenoA);
        // 250 % × 15.2 = 38 A → 40 A, el tamaño inmediato superior — 430-52(c)(1) Excepción 1.
        Assert.Equal(40m, r.ProteccionA);
        // 125 % × 15.2 = 19 A: 14 AWG da 15 A a 60 °C, 12 AWG 20 A — 430-22. El interruptor de 40 A
        // no sube el conductor: protege contra cortocircuito, no contra sobrecarga — 240-4(g).
        Assert.Equal(19m, r.Detalle!.CapacidadMinimaA);
        Assert.Equal("12", r.CalibreFase.Designacion);
        Assert.Contains(r.Citas, x => x.Referencia == "430-22");
        Assert.Contains(r.Citas, x => x.Referencia == "430-52");
    }

    [Fact]
    public void I15_LaCargaDelMotorEsSuFlcPorLaTensionDelCircuito()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 5m, 3);

        // 15.2 A × 220 V × √3 = 5,791.98 VA. No es continua ni no continua: va aparte, por 430-24.
        Assert.Equal(5791.98m, Math.Round(c.MotorVA, 2));
        Assert.Equal(c.MotorVA, c.CargaInstaladaVA);
        Assert.Equal(0m, c.ContinuaVA);
        Assert.Equal(0m, cuadro.Resumen.ContinuaVA);
        Assert.Equal(c.MotorVA, cuadro.Resumen.MotoresVA);
        Assert.Equal(c.MotorVA, cuadro.Resumen.InstaladaVA);
        // En las tres barras, un tercio cada una.
        Assert.Equal(Math.Round(c.MotorVA / 3m, 2), Math.Round(cuadro.Resumen.CargaPorFaseVA['A'], 2));
    }

    [Fact]
    public void I15_UnMotorDeUnPoloSeLeeEnLaColumnaDe127V()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 1m, 1);

        // Tabla 430-248, columna de 127 V (propia de la NOM): 14 A. 250 % = 35 A → 35 A.
        Assert.Equal(14m, c.FlcA);
        Assert.Equal(35m, c.Resultado!.ProteccionA);
        // 125 % × 14 = 17.5 A → 12 AWG (20 A a 60 °C).
        Assert.Equal("12", c.Resultado.CalibreFase.Designacion);
        Assert.True(c.LlevaNeutro);
    }

    [Fact]
    public void I15_UnMotorMonofasicoDeDosPolosVaEntreFases()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 1m, 2);

        // Tabla 430-248, columna de 230 V: 8 A. 250 % = 20 A → 20 A. 125 % = 10 A → 14 AWG.
        Assert.Equal(8m, c.FlcA);
        Assert.Equal(20m, c.Resultado!.ProteccionA);
        Assert.Equal("14", c.Resultado.CalibreFase.Designacion);

        // La caída, a 220 V y no a 127 V: 2 × 20 m × 8 A × (10.2 × 0.9 + 0.19 × 0.436) ÷ 1000 = 2.96 V,
        // 1.35 % de 220 V (2.33 % si se midiera contra 127 V).
        Assert.InRange(c.Resultado.CaidaTensionPct, 1.2m, 1.5m);
    }

    [Fact]
    public void I15_ConTerminalesMarcadas75CElConductorUsaLaColumnaDe75()
    {
        var cuadro = Nuevo();
        cuadro.Datos.TerminalesMarcadas75C = true;
        var c = ConMotor(cuadro, 1, 5m, 3);

        // 19 A: 14 AWG da 20 A a 75 °C — 110-14(c)(1)a.(3). Antes el derivado de motor la ignoraba.
        Assert.Equal("14", c.Resultado!.CalibreFase.Designacion);
    }

    [Fact]
    public void I15_UnMotorQueLaTablaNoTraeDiceConQueSi()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 15m, 1);

        // Los monofásicos de la Tabla 430-248 llegan a 10 HP.
        Assert.True(c.TieneCarga);
        Assert.Null(c.Resultado);
        Assert.Contains("430-248", c.Error);
        Assert.Contains("15 HP", c.Error);
        Assert.Contains("10 HP", c.Error);
    }

    [Fact]
    public void I15_ElSelectorOfreceLosHpDeLaTabla()
    {
        var monofasicos = MotoresEnHp.Disponibles(Motor.FlcMotor, polos: 1, tensionV: 127m);
        var trifasicos = MotoresEnHp.Disponibles(Motor.FlcMotor, polos: 3, tensionV: 220m);

        Assert.Equal(1m / 6m, monofasicos[0]);
        Assert.Equal(10m, monofasicos[^1]);
        // A 230 V la Tabla 430-250 va de 1/2 a 200 HP; 250 HP ya no trae valor en esa columna.
        Assert.Equal(0.5m, trifasicos[0]);
        Assert.Equal(200m, trifasicos[^1]);
        Assert.Equal("7 1/2", MotoresEnHp.Texto(7.5m));
        Assert.Equal("1/6", MotoresEnHp.Texto(1m / 6m));
    }

    [Fact]
    public void I15_En208Y120UnMotorDeUnPoloSeLeeEnLaColumnaDe115V()
    {
        // 208 / √3 = 120.09 V: fuera del intervalo de 110 a 120 V si no se redondea.
        var cuadro = Nuevo(tension: 208m);
        var c = ConMotor(cuadro, 1, 1m, 1);

        Assert.Equal(16m, c.FlcA);
        Assert.Equal("Tabla 430-248, columna de 115 V (120 V: intervalo de 110 a 120 V)", MotoresEnHp.Fuente(1, 120m));
    }

    [Fact]
    public void I15_HpSoloCuentaEnMotorYSeConservaAlCambiarDeTipo()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 5m, 3);
        c.Continua = 1000m;

        c.Categoria = CategoriaDeCarga.Equipo;
        cuadro.Recalcular();
        Assert.False(c.EsMotor);
        Assert.Equal(1000m, c.CargaInstaladaVA);

        c.Categoria = CategoriaDeCarga.MotorOAireAcondicionado;
        cuadro.Recalcular();
        Assert.True(c.EsMotor);
        Assert.Equal(15.2m, c.Resultado!.CorrienteDisenoA);
    }

    // ---- El alimentador -----------------------------------------------------------------------

    [Fact]
    public void I15_ElAlimentadorLlevaElMayorAl125YLosDemasAl100()
    {
        var cuadro = Nuevo();
        ConMotor(cuadro, 1, 5m, 3); // 15.2 A, 40 A
        ConMotor(cuadro, 2, 3m, 3); // 9.6 A, 250 % = 24 A → 25 A

        var a = cuadro.Alimentador.Resultado!;
        // 430-24: 125 % × 15.2 + 9.6 = 28.6 A → 30 A.
        Assert.Equal(28.6m, a.Detalle!.CapacidadMinimaA);
        Assert.Equal(30m, a.ProteccionA);
        Assert.Equal(24.8m, a.CorrienteDisenoA);
        // 430-62(a): la mayor protección de motor (40 A) + la FLC del otro (9.6 A).
        Assert.Equal(49.6m, a.TechoProteccion430_62A);
        Assert.False(a.ProteccionExcedeTecho430_62);
    }

    [Fact]
    public void I15_ElPrincipalMenorQueElMotorAvisaHastaDondeDejaSubir430_62()
    {
        var cuadro = Nuevo();
        ConMotor(cuadro, 1, 5m, 3);
        ConMotor(cuadro, 2, 3m, 3);

        var avisos = cuadro.Alimentador.Avisos;
        Assert.Contains(avisos, x => x.Contains("protección del motor del circuito 1") && x.Contains("430-62(a)"));
        Assert.DoesNotContain(avisos, x => x.Contains("revisa la carga capturada"));
    }

    [Fact]
    public void I15_LosMotoresSeSumanPorFaseNoTodosEnLaQueGobierna()
    {
        var cuadro = Nuevo();
        // Tres motores de 1 HP a 127 V, uno por barra: 14 A cada uno.
        ConMotor(cuadro, 1, 1m, 1); // A
        ConMotor(cuadro, 3, 1m, 1); // B
        ConMotor(cuadro, 5, 1m, 1); // C

        var a = cuadro.Alimentador.Resultado!;
        // Cada fase lleva un solo motor: 125 % × 14 = 17.5 A → 20 A. Todos en la misma fase darían
        // 125 % × 14 + 28 = 45.5 A → 50 A.
        Assert.Equal(17.5m, a.Detalle!.CapacidadMinimaA);
        Assert.Equal(20m, a.ProteccionA);
        Assert.Equal(17.5m, cuadro.Alimentador.Gobierna!.CapacidadA);

        // La caída, fase por fase con el neutro, ya con los motores: tres corrientes iguales a 120°.
        Assert.NotNull(a.CaidaPorFase);
        Assert.Equal(14m, Math.Round(a.CaidaPorFase!.Single(f => f.Fase == 'A').Corriente.Magnitud, 2));
        Assert.True(a.CorrienteNeutro!.Value.Magnitud < 0.01m);
    }

    [Fact]
    public void I15_LaFaseQueGobiernaCuentaLosMotores()
    {
        var cuadro = Nuevo();
        var motor = ConMotor(cuadro, 1, 3m, 1); // 1 polo, 127 V: 31 A → 125 % = 38.75 A en la fase A
        var alumbrado = Espacio(cuadro, 3); // fase B
        alumbrado.Continua = 3000m; // 23.62 A → 125 % = 29.52 A
        cuadro.Recalcular();

        Assert.Equal(31m, motor.FlcA);
        Assert.Equal('A', cuadro.Alimentador.Gobierna!.Fase);
        Assert.Equal(38.75m, cuadro.Alimentador.Resultado!.Detalle!.CapacidadMinimaA);
    }

    [Fact]
    public void I15_ElFactorDeDemandaDeMotoresReduceSuFlcEnElAlimentador()
    {
        var cuadro = Nuevo();
        ConMotor(cuadro, 1, 5m, 3);
        cuadro.Datos.CambiarFactorDeDemanda(CategoriaDeCarga.MotorOAireAcondicionado, 0.5m);
        cuadro.Recalcular();

        // 430-26: 0.5 × 15.2 = 7.6 A; 125 % = 9.5 A. El derivado no cambia.
        Assert.Equal(9.5m, cuadro.Alimentador.Resultado!.Detalle!.CapacidadMinimaA);
        Assert.Equal(40m, Espacio(cuadro, 1).Resultado!.ProteccionA);
        Assert.Equal(Math.Round(Espacio(cuadro, 1).MotorVA / 2m, 2), Math.Round(cuadro.Resumen.MotoresDemandadaVA, 2));
    }

    // ---- Documentos ---------------------------------------------------------------------------

    [Fact]
    public void I15_ElDesgloseDelMotorCitaLasTablasDelArt430()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 5m, 3);

        var d = cuadro.Desglose(c)!;
        Assert.Contains(d.Proteccion, x => x.Contains("Tabla 430-250, columna de 230 V") && x.Contains("15.20"));
        Assert.Contains(d.Proteccion, x => x.Contains("250 %") && x.Contains("Tabla 430-52"));
        Assert.Contains(d.Proteccion, x => x.Contains("430-32"));
        Assert.Contains(d.Conductor, x => x.Contains("430-22") && x.Contains("✔"));
    }

    [Fact]
    public void I15_LaMemoriaDelMotorVaPorElArt430()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 5m, 3);

        var hoja = MemoriaDeCalculo.DeCircuito(cuadro, c);
        Assert.Equal("430", hoja.Articulo);
        var secciones = MemoriaDeCalculo.Secciones(hoja);
        var s1 = secciones[0];
        Assert.Contains(s1.Renglones, r => r.Rotulo == "Motor" && r.Valor.Contains("5 HP") && r.Valor.Contains("trifásico"));
        Assert.DoesNotContain(s1.Renglones, r => r.Rotulo == "Carga continua");
        var s3 = secciones[2];
        Assert.Contains(s3.Renglones, r => r.Rotulo.Contains("430-22"));
        Assert.Contains(s3.Renglones, r => r.Rotulo.Contains("Tabla 430-52"));
        Assert.Contains(s3.Notas, n => n.Contains("430-32"));
    }

    [Fact]
    public void I15_LaMemoriaDelAlimentadorDice430_24Y430_62()
    {
        var cuadro = Nuevo();
        ConMotor(cuadro, 1, 5m, 3);
        ConMotor(cuadro, 2, 3m, 3);

        var s3 = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DelAlimentador(cuadro)!)[2];
        Assert.Contains(s3.Renglones, r => r.Rotulo == "Motores — 430-24");
        Assert.Contains(s3.Renglones, r => r.Rotulo.Contains("430-62(a)"));
    }

    [Fact]
    public void I15_ElArchivoGuardaLosHp()
    {
        var original = Nuevo();
        ConMotor(original, 1, 7.5m, 3);

        var texto = ArchivoDelCuadro.Guardar(original, new DateTimeOffset(2026, 9, 26, 12, 0, 0, TimeSpan.FromHours(-6)));
        Assert.Contains("\"hp\"", texto, StringComparison.OrdinalIgnoreCase);

        var abierto = ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!;
        var c = abierto.Circuitos[0];
        Assert.Equal(7.5m, c.Hp);
        Assert.True(c.EsMotor);
        Assert.Equal(ArchivoDelCuadro.Huella(original), ArchivoDelCuadro.Huella(abierto));
    }

    // ---- M-09 ---------------------------------------------------------------------------------

    [Fact]
    public void M09_ElTechoDe430_63SumaLaOtraCargaComoLaPide215_3()
    {
        // 40 A continuos a 127 V (5,080 VA) y un motor de 5.3 A con su derivado de 15 A.
        // Capacidad mínima: 125 % × 40 + 125 % × 5.3 = 56.63 A → 60 A. El techo: 15 A + lo que 215-3
        // pide para la otra carga, 125 % × 40 = 50 A → 65 A. Al 100 % daba 55 A y «excedía».
        var r = CalculadoraProteccionAlimentador.Calcular(
            Motor.ProteccionEstandar,
            cargaContinuaVA: 5080m,
            cargaNoContinuaVA: 0m,
            numeroFases: 1,
            tensionFaseNeutroV: 127m,
            tensionFaseFaseV: 220m,
            cargaMotores: AgregadoMotores.DeUnMotor(5.3m, 15m));

        Assert.Equal(60m, r.ProteccionA);
        Assert.Equal(65m, r.TechoProteccion430_62A);
        Assert.False(r.ProteccionExcedeTecho430_62);
    }
}
