using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>La protección del derivado de un motor, dentro de su rango</b> — M-20, decisión
/// <c>proteccion-de-motores-por-rango.md</c>. 430-52(c)(1) pide un valor «que no exceda» el de la Tabla
/// 430-52: es un techo. El rango va del menor valor de la serie que lleva el 125 % de la FLC al techo, y se
/// escoge con un criterio: máximo 430-52 (lo de antes), prioridad al conductor (240-4) o manual.
/// Condiciones de los ejemplos: cobre, THHN, lugar seco, 30 °C, PVC, 5 m (sin subir por caída), terminales
/// por la regla general. Los números salen de las tablas de la NOM recalculados a mano.
/// </summary>
public class ProteccionDeMotoresPorRangoTests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    // ---- El motor de cálculo, directo --------------------------------------------------------------

    private static ResultadoCircuitoDerivado Calcular(
        decimal hp, int polos, decimal tensionMotorV, CriterioProteccionMotor criterio,
        SerieDeInterruptores serie = SerieDeInterruptores.CentroDeCargaNema, decimal? elegida = null,
        int agrupados = 3, decimal temperaturaC = 30m, bool noArranca = false, bool marcadas75 = false,
        decimal? flcEnAmperes = null, decimal longitudM = 5m, bool motor75 = false)
    {
        var vff = tensionMotorV > 300m ? 480m : 220m;
        var vfn = tensionMotorV > 300m ? 277m : 127m;
        return Motor.Motor(serie).Calcular(new DatosEntradaCircuitoDerivadoMotor(
            Hp: hp,
            TipoAlimentacion: MotoresEnHp.Alimentacion(polos),
            TipoMotor: MotoresEnHp.TipoDeMotor(polos),
            TipoDispositivoProteccion: TipoDispositivoProteccionMotor.InterruptorTiempoInverso,
            TensionNominalMotorV: tensionMotorV,
            TensionFaseNeutroV: polos == 1 ? vfn : vff,
            TensionFaseFaseV: vff,
            LongitudM: longitudM,
            NumeroConductoresParalelo: 1,
            NumeroConductoresAgrupados: agrupados,
            TemperaturaAmbienteC: temperaturaC,
            MaterialConductor: MaterialConductor.Cobre,
            MaterialCanalizacion: MaterialCanalizacion.Pvc,
            FactorPotencia: 0.9m,
            CaidaTensionMaxPct: 3m,
            PisoPracticoCalibreMm2: null,
            TerminalesMarcadas75C: marcadas75,
            FlcMarcadaEnAmperesA: flcEnAmperes,
            NoArrancaConLaTabla: noArranca,
            CriterioProteccion: criterio,
            ProteccionElegidaA: elegida,
            MotorYArrancadorMarcados75C: motor75));
    }

    [Theory]
    // HP, polos, V, mínimo (≥ 125 % de la FLC), máximo (Tabla 430-52 con la Excepción 1)
    [InlineData(0.5, 1, 127, 15, 25)]   // 8.90 A: 11.13 A → 15; 22.25 A → 25
    [InlineData(1, 1, 127, 20, 35)]     // 14 A: 17.5 A → 20; 35 A, normalizado
    [InlineData(5, 3, 220, 20, 40)]     // 15.2 A: 19 A → 20; 38 A → 40
    [InlineData(10, 3, 220, 35, 70)]    // 28 A: 35 A; 70 A
    [InlineData(30, 3, 220, 100, 200)]  // 80 A: 100 A; 200 A
    public void M20_ElRangoVaDel125DeLaFlcAlTechoDe430_52(double hp, int polos, int v, int minimo, int maximo)
    {
        var r = Calcular((decimal)hp, polos, v, CriterioProteccionMotor.Maximo430_52).Rango!;

        Assert.Equal(minimo, r.MinimoA);
        Assert.Equal(maximo, r.MaximoA);
        Assert.Equal(minimo, r.Valores[0]);
        Assert.Equal(maximo, r.Valores[^1]);
    }

    [Fact]
    public void M20_ElRangoDe100HpEnLaNomCompleta()
    {
        // 124 A (columna de 460 V): 155 A → 175 A; 310 A → 350 A (Excepción 1).
        var r = Calcular(100m, 3, 480m, CriterioProteccionMotor.Maximo430_52, SerieDeInterruptores.NomCompleta).Rango!;

        Assert.Equal(175m, r.MinimoA);
        Assert.Equal(350m, r.MaximoA);
        Assert.Equal([175m, 200m, 225m, 250m, 300m, 350m], r.Valores);
    }

    [Fact]
    public void M20_PrioridadAlConductor_LaBombaDeMedioHpVaEn15A()
    {
        // El caso de David: bomba de cisterna de 1/2 HP, 1 polo, 127 V. Hoy 25 A sobre 14 AWG; en campo, 15 A.
        var r = Calcular(0.5m, 1, 127m, CriterioProteccionMotor.Conductor);

        Assert.Equal(15m, r.ProteccionA);
        Assert.Equal("14", r.CalibreFase.Designacion);
        Assert.Equal("14", r.CalibreTierra.Designacion);
        Assert.True(r.Rango!.ProtegeAlConductor);
        Assert.False(r.Rango.EsElMaximo);
        Assert.Contains(r.Citas, x => x.Referencia == "240-4" && x.Descripcion.StartsWith("Calculada: 15 A, el mayor valor del rango que protege"));
        // No la elige por el arranque: lo dice — 430-52(b).
        Assert.Contains(r.Citas, x => x.Referencia == "430-52(b)" && x.Descripcion.Contains("25 A"));
    }

    [Theory]
    // HP, polos, V, serie (0 NEMA, 2 NOM completa), protección, calibre, tierra, protección con el máximo
    [InlineData(0.5, 1, 127, 0, 15, "14", "14", 25)]
    [InlineData(1, 1, 127, 0, 20, "12", "12", 35)]
    [InlineData(5, 3, 220, 0, 20, "12", "12", 40)]
    [InlineData(10, 3, 220, 0, 40, "8", "10", 70)]
    [InlineData(30, 3, 220, 0, 100, "1", "8", 200)]
    [InlineData(100, 3, 480, 2, 175, "2/0", "6", 350)]
    public void M20_PrioridadAlConductor_DaLaTablaDeEjemplos(
        double hp, int polos, int v, int serie, int proteccion, string calibre, string tierra, int conElMaximo)
    {
        var s = (SerieDeInterruptores)serie;
        var r = Calcular((decimal)hp, polos, v, CriterioProteccionMotor.Conductor, s);

        Assert.Equal(proteccion, r.ProteccionA);
        Assert.Equal(calibre, r.CalibreFase.Designacion);
        Assert.Equal(tierra, r.CalibreTierra.Designacion);
        Assert.Equal(conElMaximo, Calcular((decimal)hp, polos, v, CriterioProteccionMotor.Maximo430_52, s).ProteccionA);
    }

    [Fact]
    public void M20_ElMaximoDaLoMismoQueAntesDeM20()
    {
        // El criterio por omisión del motor de cálculo es el máximo: un cálculo viejo da lo mismo.
        var r = Calcular(0.5m, 1, 127m, CriterioProteccionMotor.Maximo430_52);

        Assert.Equal(25m, r.ProteccionA);
        Assert.Equal("14", r.CalibreFase.Designacion);
        Assert.True(r.Rango!.EsElMaximo);
        Assert.False(r.Rango.ProtegeAlConductor);
        Assert.Contains(r.Citas, x => x.Referencia == "240-4(g)" && x.Descripcion.Contains("430-32"));
        Assert.DoesNotContain(r.Citas, x => x.Referencia == "430-52(b)");
    }

    [Fact]
    public void M20_ConAgrupamientoSubeElCalibreYNoBajaDePmin()
    {
        // 12 portadores a 40 °C: ×0.91 ×0.50. 1/2 HP: 14 AWG = 25 × 0.455 = 11.38 A, cumple 430-22
        // (11.13 A) y ningún valor de la serie es ≤ 11.38 A. 11.38 no es valor normalizado: 15 A por 240-4(b).
        var medio = Calcular(0.5m, 1, 127m, CriterioProteccionMotor.Conductor, agrupados: 12, temperaturaC: 40m);
        Assert.Equal(15m, medio.ProteccionA);
        Assert.Equal("14", medio.CalibreFase.Designacion);
        Assert.True(medio.Rango!.PorExcepcion240_4b);
        Assert.Contains(medio.Citas, x => x.Referencia == "240-4(b)");

        // 1 HP: 12 AWG = 13.65 A no cumple 430-22 (17.5 A); 10 AWG = 18.2 A, y 240-4(b) da 20 A = Pmín.
        var uno = Calcular(1m, 1, 127m, CriterioProteccionMotor.Conductor, agrupados: 12, temperaturaC: 40m);
        Assert.Equal(20m, uno.ProteccionA);
        Assert.Equal("10", uno.CalibreFase.Designacion);
    }

    [Fact]
    public void M20_SinValorQueLoProtejaQuedaElMinimoYSubeElCalibre()
    {
        // Riel DIN, 7.5 HP a 220 V: 22 A, 125 % = 27.5 A → mínimo 32 A (la serie no trae 30). El calibre por
        // ampacidad es 10 AWG con 30 A: valor normalizado, sin 240-4(b), y 240-4(d) lo topa en 30 A. Ningún
        // valor del rango lo protege: 32 A y el calibre sube a 8 AWG (40 A a 60 °C) — 240-4.
        var r = Calcular(7.5m, 3, 220m, CriterioProteccionMotor.Conductor, SerieDeInterruptores.RielDinIec);

        Assert.Equal(32m, r.ProteccionA);
        Assert.Equal("8", r.CalibreFase.Designacion);
        Assert.True(r.Rango!.SubioElCalibre);
        Assert.True(r.Rango.ProtegeAlConductor);
        Assert.Equal(50m, Calcular(7.5m, 3, 220m, CriterioProteccionMotor.Maximo430_52, SerieDeInterruptores.RielDinIec).ProteccionA);
    }

    [Fact]
    public void M20_SubirPorCaidaNoSubeLaProteccion()
    {
        // La bomba de 1/2 HP a 30 m: 14 AWG por ampacidad (3.9 % de caída), que la caída sube a 12 AWG. Se
        // protege el de ampacidad (15 A); el más grueso sigue protegido — paso 9 de M-20.
        var r = Calcular(0.5m, 1, 127m, CriterioProteccionMotor.Conductor, longitudM: 30m);

        Assert.Equal(15m, r.ProteccionA);
        Assert.Equal("12", r.CalibreFase.Designacion);
        Assert.Equal("14", r.Rango!.CalibreProtegido!.Designacion);
        Assert.True(r.Rango.ProtegeAlConductor);
        Assert.Contains(r.Citas, x => x.Referencia == "240-4" && x.Descripcion.Contains("protege a 14 (15 A)") && x.Descripcion.Contains("12 por caída de tensión sigue protegido"));
    }

    [Fact]
    public void M20_ConPminHasta100ALaProteccionNoPasaDe100A()
    {
        // 30 HP a 220 V: 80 A, mínimo 100 A → terminal de 60 °C → 1 AWG (110 A). 110 A protegería a 1 AWG,
        // pero sacaría el circuito de la regla de 60 °C (y con 75 °C el calibre sería 3 AWG, que 110 A no
        // protege). Recalcular oscila; con el tope, 100 A con 1 AWG.
        var r = Calcular(30m, 3, 220m, CriterioProteccionMotor.Conductor);

        Assert.Equal(100m, r.ProteccionA);
        Assert.Equal("1", r.CalibreFase.Designacion);
        Assert.True(r.Rango!.TopadoEn100A);
        Assert.Contains(r.Citas, x => x.Referencia == "240-4" && x.Descripcion.Contains("110-14(c)(1)a."));
        Assert.Equal(60, r.Detalle!.TemperaturaTerminalesC);

        // Con el tablero marcado 75 °C (110-14(c)(1)a.(3)), el interruptor ya no topa en 100 A, pero el motor y su
        // arrancador, con 1 AWG y sin marcado, siguen en 60 °C, la más baja — M-21: 1 AWG (110 A) con 110 A.
        var marcadas = Calcular(30m, 3, 220m, CriterioProteccionMotor.Conductor, marcadas75: true);
        Assert.Equal(110m, marcadas.ProteccionA);
        Assert.Equal("1", marcadas.CalibreFase.Designacion);
        Assert.Equal(60, marcadas.Detalle!.TemperaturaTerminalesC);
        Assert.False(marcadas.Rango!.TopadoEn100A);

        // Con el motor de diseño B a E y el arrancador marcados 75 °C también, la salida: 3 AWG con 100 A.
        var todo75 = Calcular(30m, 3, 220m, CriterioProteccionMotor.Conductor, marcadas75: true, motor75: true);
        Assert.Equal(100m, todo75.ProteccionA);
        Assert.Equal("3", todo75.CalibreFase.Designacion);
        Assert.False(todo75.Rango!.TopadoEn100A);
    }

    [Fact]
    public void M20_ConTerminalesDe75ElTopeDe240_4dSigueMandando()
    {
        // 1/2 HP con terminales 75 °C: 14 AWG lleva 20 A a 75 °C, pero 240-4(d) lo topa en 15 A.
        var r = Calcular(0.5m, 1, 127m, CriterioProteccionMotor.Conductor, marcadas75: true);

        Assert.Equal(15m, r.ProteccionA);
        Assert.Equal("14", r.CalibreFase.Designacion);
    }

    [Fact]
    public void M20_ElPisoNoPasaDelTecho()
    {
        // Riel DIN, 1/4 HP a 127 V: 5.30 A, 250 % = 13.25 A → 15 A de la lista de 240-6(a), fuera de la serie.
        // El menor de la serie que lleva 6.63 A sería 16 A, arriba del techo: el rango es solo 15 A.
        foreach (var criterio in Enum.GetValues<CriterioProteccionMotor>())
        {
            var r = Calcular(0.25m, 1, 127m, criterio, SerieDeInterruptores.RielDinIec, elegida: 16m);
            Assert.Equal([15m], r.Rango!.Valores);
            Assert.Equal(15m, r.ProteccionA);
            Assert.Equal("14", r.CalibreFase.Designacion);
        }
    }

    [Fact]
    public void M20_ManualSoloOfreceValoresDeLaSerieDentroDelRango()
    {
        // 1 HP a 127 V: de 17.5 A a 35 A.
        Assert.Equal([20m, 25m, 30m, 35m], Calcular(1m, 1, 127m, CriterioProteccionMotor.Manual).Rango!.Valores);
        Assert.Equal([20m, 25m, 32m], Calcular(1m, 1, 127m, CriterioProteccionMotor.Manual, SerieDeInterruptores.RielDinIec).Rango!.Valores);
        Assert.Equal([20m, 25m, 30m, 32m, 35m], Calcular(1m, 1, 127m, CriterioProteccionMotor.Manual, SerieDeInterruptores.NomCompleta).Rango!.Valores);
    }

    [Fact]
    public void M20_ManualFueraDelRangoUsaElMasCercano()
    {
        // Riel DIN, 1 HP: 35 A no es de la serie; el más cercano del rango, 32 A.
        var r = Calcular(1m, 1, 127m, CriterioProteccionMotor.Manual, SerieDeInterruptores.RielDinIec, elegida: 35m);

        Assert.Equal(32m, r.ProteccionA);
        Assert.Equal(35m, r.Rango!.PedidaA);
        Assert.Contains(r.Citas, x => x.Descripcion.Contains("pidió 35 A"));

        // Abajo del mínimo, el mínimo.
        Assert.Equal(20m, Calcular(1m, 1, 127m, CriterioProteccionMotor.Manual, elegida: 15m).ProteccionA);
    }

    [Fact]
    public void M20_ManualArribaDeLaAmpacidadCita240_4gY430_32()
    {
        var arriba = Calcular(1m, 1, 127m, CriterioProteccionMotor.Manual, elegida: 30m);
        Assert.Equal(30m, arriba.ProteccionA);
        Assert.Equal("12", arriba.CalibreFase.Designacion);
        Assert.False(arriba.Rango!.ProtegeAlConductor);
        Assert.Contains(arriba.Citas, x => x.Referencia == "240-4(g)" && x.Descripcion.Contains("430-32"));

        var protegido = Calcular(1m, 1, 127m, CriterioProteccionMotor.Manual, elegida: 20m);
        Assert.True(protegido.Rango!.ProtegeAlConductor);
        Assert.Contains(protegido.Citas, x => x.Referencia == "240-4" && x.Descripcion.Contains("protege a 12"));
    }

    [Fact]
    public void M20_ConLaExcepcion2ElRangoEmpiezaArribaDeLaTabla()
    {
        // 1 HP que no arranca con 35 A: hasta 400 % × 14 = 56 A → 50 A. El rango, arriba de 35 A.
        var maximo = Calcular(1m, 1, 127m, CriterioProteccionMotor.Maximo430_52, noArranca: true);
        Assert.Equal(50m, maximo.ProteccionA);
        Assert.Equal([40m, 45m, 50m], maximo.Rango!.Valores);

        Assert.Equal(45m, Calcular(1m, 1, 127m, CriterioProteccionMotor.Manual, elegida: 45m, noArranca: true).ProteccionA);

        // Prioridad al conductor con la Excepción 2: ninguno protege a 12 AWG; el mínimo (40 A) y 8 AWG.
        var conductor = Calcular(1m, 1, 127m, CriterioProteccionMotor.Conductor, noArranca: true);
        Assert.Equal(40m, conductor.ProteccionA);
        Assert.Equal("8", conductor.CalibreFase.Designacion);
    }

    [Fact]
    public void M20_LaTierraUsaLaProteccionElegidaY430_62aElMaximoPermitido()
    {
        // 250-122(d)(1): «el valor nominal del dispositivo». 100 HP: con 175 A, 6 AWG; con 350 A, 2 AWG.
        var conductor = Calcular(100m, 3, 480m, CriterioProteccionMotor.Conductor, SerieDeInterruptores.NomCompleta);
        Assert.Equal("6", conductor.CalibreTierra.Designacion);
        Assert.Equal("2", Calcular(100m, 3, 480m, CriterioProteccionMotor.Maximo430_52, SerieDeInterruptores.NomCompleta).CalibreTierra.Designacion);

        // 430-62(a): «el valor máximo permitido … de acuerdo con 430-52», sin importar el criterio.
        Assert.Equal(350m, conductor.Rango!.MaximoPermitidoA);
        // En riel DIN, 35 A aunque se instalen 32 (1 HP a 127 V).
        Assert.Equal(35m, Calcular(1m, 1, 127m, CriterioProteccionMotor.Maximo430_52, SerieDeInterruptores.RielDinIec).Rango!.MaximoPermitidoA);
    }

    // ---- En el cuadro: el criterio de cada circuito ----------------------------------------------

    private static CuadroDeCarga Nuevo(SerieDeInterruptores serie = SerieDeInterruptores.CentroDeCargaNema)
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = 12;
        cuadro.Datos.Fases = 3;
        cuadro.Datos.Hilos = 4;
        cuadro.Datos.TensionFaseFaseV = 220m;
        cuadro.Datos.SerieInterruptores = serie;
        cuadro.Recalcular();
        return cuadro;
    }

    private static CircuitoDelCuadro ConMotor(CuadroDeCarga cuadro, int espacio, decimal hp, int polos = 1)
    {
        var c = cuadro.Circuitos.Single(x => x.Espacio == espacio);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = hp;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        cuadro.Recalcular();
        return c;
    }

    [Fact]
    public void M20_UnMotorNuevoNaceEnAutomatico_ConductorHasta1HpYMaximoArriba()
    {
        // Pregunta 1 de David, opción C: el corte de 430-32 entre (a) y (b).
        var cuadro = Nuevo();
        var medio = ConMotor(cuadro, 1, 0.5m);
        var uno = ConMotor(cuadro, 3, 1m);
        var cinco = ConMotor(cuadro, 5, 5m, 3);

        Assert.Equal(CriterioDeProteccion.Automatico, medio.CriterioProteccion);
        Assert.Equal(15m, medio.Resultado!.ProteccionA);   // ½ HP: prioridad al conductor
        Assert.Equal(20m, uno.Resultado!.ProteccionA);     // 1 HP, todavía «1 HP o menos»
        Assert.Equal(40m, cinco.Resultado!.ProteccionA);   // 5 HP: el máximo
        Assert.Equal(CriterioProteccionMotor.Conductor, medio.Resultado.Rango!.Criterio);
        Assert.Equal(CriterioProteccionMotor.Maximo430_52, cinco.Resultado.Rango!.Criterio);

        // Con la Excepción 2 el automático va al máximo: el motor no arranca con menos.
        medio.NoArrancaConLaTabla = true;
        cuadro.Recalcular();
        Assert.Equal(CriterioProteccionMotor.Maximo430_52, medio.Resultado!.Rango!.Criterio);
        Assert.True(medio.Resultado.Rango.Tabla430_52!.UsaExcepcion2);
    }

    [Fact]
    public void M20_UnMotorEnAmperesUsaSusHpInterpolados()
    {
        // 8.9 A a 127 V: ½ HP por interpolación (430-6(a)(1)) → automático, prioridad al conductor.
        var cuadro = Nuevo();
        var c = cuadro.Circuitos[0];
        c.Categoria = CategoriaDeCarga.Motor;
        c.CapturaMotor = CapturaDeMotor.Amperes;
        c.CorrientePlacaA = 8.9m;
        cuadro.Recalcular();

        Assert.Equal(15m, c.Resultado!.ProteccionA);
    }

    [Fact]
    public void M20_ElSelectorSeActivaEnMotorAireYVariador_NoEnGrupoNiHabitacion()
    {
        var cuadro = Nuevo();
        var motor = ConMotor(cuadro, 1, 0.5m);
        Assert.True(motor.ProteccionEscogible);

        var aire = cuadro.Circuitos.Single(x => x.Espacio == 3);
        aire.Categoria = CategoriaDeCarga.AireAcondicionado;
        aire.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
        aire.CorrientePlacaA = 10m;

        var variador = cuadro.Circuitos.Single(x => x.Espacio == 5);
        variador.Categoria = CategoriaDeCarga.Motor;
        variador.CapturaMotor = CapturaDeMotor.Variador;
        variador.CorrienteEntradaVariadorA = 20m;
        variador.ProteccionMaximaVariadorA = 40m;

        var grupo = cuadro.Circuitos.Single(x => x.Espacio == 7);
        grupo.Categoria = CategoriaDeCarga.Motor;
        foreach (var hp in new[] { 0.5m, 1m })
        {
            var m = grupo.AgregarCarga();
            m.Subtipo = SubtipoDeCarga.MotorUsoGeneral;
            m.Hp = hp;
        }

        var alumbrado = cuadro.Circuitos.Single(x => x.Espacio == 9);
        alumbrado.Categoria = CategoriaDeCarga.Alumbrado;
        alumbrado.NoContinua = 900m;
        cuadro.Recalcular();

        // Fase 2 (David, 2026-10-03): A/C y variador, sí; grupos de motores, no.
        Assert.True(aire.ProteccionEscogible);
        Assert.True(variador.ProteccionEscogible);
        Assert.False(grupo.ProteccionEscogible);
        Assert.False(alumbrado.ProteccionEscogible);

        // El acondicionador de habitación ya es el mínimo (440-62(b)): sin rango.
        aire.PlacaAire = PlacaDeAireAcondicionado.Habitacion;
        aire.CorrientePlacaA = 8m;
        cuadro.Recalcular();
        Assert.Null(aire.Error);
        Assert.False(aire.ProteccionEscogible);
    }

    [Fact]
    public void M20_ElSelectorDiceLoQueDaCadaCriterio()
    {
        var cuadro = Nuevo();
        var medio = ConMotor(cuadro, 1, 0.5m);
        var cinco = ConMotor(cuadro, 3, 5m, 3);

        Assert.Equal(15m, cuadro.ProteccionConCriterio(medio, CriterioDeProteccion.Automatico));
        Assert.Equal(15m, cuadro.ProteccionConCriterio(medio, CriterioDeProteccion.Conductor));
        Assert.Equal(25m, cuadro.ProteccionConCriterio(medio, CriterioDeProteccion.Maximo430_52));
        Assert.Equal(40m, cuadro.ProteccionConCriterio(cinco, CriterioDeProteccion.Automatico));
        Assert.Equal(20m, cuadro.ProteccionConCriterio(cinco, CriterioDeProteccion.Conductor));
        Assert.Null(cuadro.ProteccionConCriterio(cinco, CriterioDeProteccion.Manual));
        // Preguntar no cambia el circuito.
        Assert.Equal(40m, cinco.Resultado!.ProteccionA);
    }

    [Fact]
    public void M20_QuitarElEquipoRegresaAlAutomatico()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 1m);
        c.CriterioProteccion = CriterioDeProteccion.Manual;
        c.ProteccionElegidaA = 30m;

        c.QuitarEquipo();

        Assert.Equal(CriterioDeProteccion.Automatico, c.CriterioProteccion);
        Assert.Null(c.ProteccionElegidaA);
    }

    [Fact]
    public void M20_El430_62aUsaElMaximoPermitidoYElA4LaProteccionInstalada()
    {
        // ½ HP con 15 A instalados: el techo de 430-62(a) parte de los 25 A que permite 430-52, no de 15 A
        // (pregunta 6, la lectura literal). El aviso de «principal menor que el derivado» (A-4) compara contra
        // los 15 A que se instalan.
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 0.5m);

        Assert.Equal(15m, c.Resultado!.ProteccionA);
        Assert.Equal(25m, cuadro.Alimentador.Resultado!.TechoProteccion430_62A);
        Assert.DoesNotContain(cuadro.Alimentador.Avisos, a => a.Contains("es menor que la protección del motor"));

        // Con el máximo, 25 A sobre un principal más chico: sí avisa.
        c.CriterioProteccion = CriterioDeProteccion.Maximo430_52;
        cuadro.Recalcular();
        Assert.Equal(25m, c.Resultado!.ProteccionA);
        Assert.Equal(25m, cuadro.Alimentador.Resultado!.TechoProteccion430_62A);
        Assert.Contains(cuadro.Alimentador.Avisos, a => a.Contains("es menor que la protección del motor"));
    }

    [Fact]
    public void M20_LaMemoriaDiceElRangoLaCalculadaYElArranque()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 0.5m);

        var equipo = MemoriaDeCalculo.DeCircuito(cuadro, c).Equipo!;
        var renglones = equipo.Proteccion.ToDictionary(r => r.Rotulo, r => r.Valor);

        Assert.StartsWith("25 A, el valor inmediato superior", renglones["Máximo del rango — 430-52(c)(1) Excepción 1"]);
        Assert.StartsWith("15 A (≥ 125 % de la FLC = 11.13 A) a 25 A", renglones["Rango permitido — 430-52(c)(1)"]);
        // I-182: «calculada», con su porqué; ya no «Criterio».
        Assert.Equal("15 A, el mayor que protege a 14 AWG (15 A) — 240-4; criterio para motores de 1 HP o menos",
            renglones["Protección calculada — 240-4"]);
        Assert.DoesNotContain(renglones.Keys, k => k.StartsWith("Criterio"));
        Assert.Contains(equipo.Notas, n => n.StartsWith("Arranque — 430-52(b)") && n.Contains("subir hasta 25 A"));
        Assert.Contains(equipo.Notas, n => n.Contains("además al conductor según su ampacidad — 240-4"));
        // I-183: la sobrecarga, dicha según el equipo.
        Assert.Contains(equipo.Notas, n => n.StartsWith("Protección contra sobrecarga — 430-32(b): Requerida aparte del interruptor"));

        // Fijada en el máximo: arriba de la ampacidad, por la sobrecarga de 430-32.
        cuadro.FijarProteccion(c, 25m);
        var conMaximo = MemoriaDeCalculo.DeCircuito(cuadro, c).Equipo!;
        Assert.Contains(conMaximo.Proteccion, r => r.Rotulo == "Máximo del rango — 430-52(c)(1) Excepción 1");
        Assert.Contains(conMaximo.Proteccion, r => r.Rotulo == "Protección fijada por el proyectista — 240-4(g)"
            && r.Valor.StartsWith("25 A, dentro del rango; arriba de la ampacidad de 14 AWG: la sobrecarga del motor y del conductor la da la protección que exige 430-32"));
        Assert.DoesNotContain(conMaximo.Notas, n => n.StartsWith("Arranque"));
    }

    [Fact]
    public void M20_ElDesgloseDiceElRangoYLaCalculada()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 0.5m);

        var lineas = cuadro.Desglose(c)!.Proteccion;

        Assert.Contains(lineas, l => l.StartsWith("Máximo del rango: 25 A"));
        Assert.Contains(lineas, l => l.StartsWith("Rango: 15 a 25 A"));
        Assert.Contains(lineas, l => l == "Calculada: 15 A, el mayor que protege a 14 AWG (15 A) — 240-4; criterio para motores de 1 HP o menos");
        Assert.DoesNotContain(lineas, l => l.StartsWith("Criterio"));
        Assert.Contains(lineas, l => l.StartsWith("Arranque:") && l.EndsWith("430-52(b)"));
        Assert.Contains(lineas, l => l.StartsWith("Sobrecarga: Requerida aparte del interruptor") && l.Contains("— 430-32(b). El interruptor protege además al conductor — 240-4"));
    }

    // ---- El archivo: formato 12 ---------------------------------------------------------------------

    [Fact]
    public void M20_ElValorFijoSeGuardaYSeAbre_YUnCondDeFormato12AbreFijado()
    {
        var cuadro = Nuevo();
        var fijo = ConMotor(cuadro, 1, 1m);
        cuadro.FijarProteccion(fijo, 30m);
        // Un «cond.» de M-20, como lo guardaba la pantalla de antes.
        var conductor = ConMotor(cuadro, 3, 5m, 3);
        conductor.CriterioProteccion = CriterioDeProteccion.Conductor;
        var automatico = ConMotor(cuadro, 9, 0.5m);
        cuadro.Recalcular();

        var texto = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        Assert.Contains("\"criterioProteccion\": \"Manual\"", texto);
        Assert.Contains("\"proteccionElegida\": 30", texto);
        Assert.Contains("\"criterioProteccion\": \"Conductor\"", texto);

        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);
        Assert.Empty(apertura.Avisos);
        var abierto = apertura.Cuadro!;
        Assert.Equal(30m, abierto.Circuitos[0].Resultado!.ProteccionA);
        Assert.Equal(CriterioDeProteccion.Manual, abierto.Circuitos[0].CriterioProteccion);
        // I-182: el «cond.» abre fijado con el valor que daba (20 A; la calculada da 40 A).
        Assert.Equal(CriterioDeProteccion.Manual, abierto.Circuitos[2].CriterioProteccion);
        Assert.Equal(20m, abierto.Circuitos[2].ProteccionElegidaA);
        Assert.Equal(20m, abierto.Circuitos[2].Resultado!.ProteccionA);
        Assert.Equal(CriterioDeProteccion.Automatico, abierto.Circuitos[8].CriterioProteccion);
        Assert.Equal(15m, abierto.Circuitos[8].Resultado!.ProteccionA);
        Assert.Empty(abierto.TomarProteccionesQueRegresaron());
        // Guardado otra vez, ya no dice «Conductor».
        Assert.DoesNotContain("\"Conductor\"", ArchivoDelCuadro.Guardar(abierto, DateTimeOffset.Now));
    }

    [Fact]
    public void M20_UnArchivoDeFormato11AbreConElMaximo()
    {
        // La memoria de un tablero entregado no cambia al abrirlo: sin el campo, un motor de formato 11 o
        // anterior abre en el máximo — 25 A, como se entregó. Un renglón que no es motor queda en automático.
        const string texto = """
            {
              "formato": "power-node/cuadro-de-carga",
              "version": 11,
              "datos": { "fases": 3, "hilos": 4, "tensionFaseFaseV": 220, "numeroEspacios": 12 },
              "circuitos": [
                { "espacio": 1, "categoria": "Motor", "hp": 0.5, "polos": 1 },
                { "espacio": 3, "categoria": "Alumbrado", "noContinua": 900 }
              ]
            }
            """;

        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Error);
        var cuadro = apertura.Cuadro!;
        // I-182: el máximo abre fijado (la calculada de ½ HP da 15 A).
        Assert.Equal(CriterioDeProteccion.Manual, cuadro.Circuitos[0].CriterioProteccion);
        Assert.Equal(25m, cuadro.Circuitos[0].ProteccionElegidaA);
        Assert.Equal(25m, cuadro.Circuitos[0].Resultado!.ProteccionA);
        Assert.Equal(CriterioDeProteccion.Automatico, cuadro.Circuitos[2].CriterioProteccion);
        // Guardado otra vez, ya dice el valor fijado.
        var otraVez = ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now);
        Assert.Contains("\"criterioProteccion\": \"Manual\"", otraVez);
        Assert.Contains("\"proteccionElegida\": 25", otraVez);
    }

    [Fact]
    public void M20_UnValorFijoFueraDelRangoAbreCalculadoYAvisa()
    {
        // Riel DIN, 1 HP: el rango es 20, 25 y 32 A. Un archivo con 35 A abre con la calculada (20 A) y lo dice.
        const string texto = """
            {
              "formato": "power-node/cuadro-de-carga",
              "version": 12,
              "datos": { "fases": 3, "hilos": 4, "tensionFaseFaseV": 220, "numeroEspacios": 12, "serieInterruptores": "RielDinIec" },
              "circuitos": [
                { "espacio": 1, "categoria": "Motor", "hp": 1, "polos": 1, "criterioProteccion": "Manual", "proteccionElegida": 35 },
                { "espacio": 3, "categoria": "Motor", "hp": 1, "polos": 1, "criterioProteccion": "Manual" }
              ]
            }
            """;

        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Null(apertura.Error);
        var cuadro = apertura.Cuadro!;
        Assert.Equal(CriterioDeProteccion.Automatico, cuadro.Circuitos[0].CriterioProteccion);
        Assert.Null(cuadro.Circuitos[0].ProteccionElegidaA);
        Assert.Equal(20m, cuadro.Circuitos[0].Resultado!.ProteccionA);
        Assert.Contains(apertura.Avisos, a => a.StartsWith("El circuito 1 trae 35 A de protección") && a.Contains("20 a 32 A") && a.EndsWith("se abrió con la calculada."));
        // «Manual» sin valor: el máximo, fijado, y se dice.
        Assert.Equal(CriterioDeProteccion.Manual, cuadro.Circuitos[2].CriterioProteccion);
        Assert.Equal(32m, cuadro.Circuitos[2].Resultado!.ProteccionA);
        Assert.Contains(apertura.Avisos, a => a.StartsWith("El circuito 3 trae la protección del motor en «manual» sin el valor"));
    }

    // ---- Fase 2: A/C y variador (David, 2026-10-03) ---------------------------------------------------

    private static CircuitoDelCuadro Aire(CuadroDeCarga cuadro, int espacio, decimal nominal, bool arranque = false)
    {
        var c = cuadro.Circuitos.Single(x => x.Espacio == espacio);
        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        Assert.Null(cuadro.CambiarPolos(c, 2));
        c.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
        c.CorrientePlacaA = nominal;
        c.ArranqueAl225 = arranque;
        cuadro.Recalcular();
        return c;
    }

    private static CircuitoDelCuadro AirePorPlaca(CuadroDeCarga cuadro, int espacio, decimal mca, decimal mocp)
    {
        var c = cuadro.Circuitos.Single(x => x.Espacio == espacio);
        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        Assert.Null(cuadro.CambiarPolos(c, 2));
        c.PlacaAire = PlacaDeAireAcondicionado.AmpacidadYProteccion;
        c.AmpacidadMinimaA = mca;
        c.ProteccionMaximaA = mocp;
        cuadro.Recalcular();
        return c;
    }

    private static CircuitoDelCuadro Variador(CuadroDeCarga cuadro, int espacio, decimal entrada, decimal maxima)
    {
        var c = cuadro.Circuitos.Single(x => x.Espacio == espacio);
        c.Categoria = CategoriaDeCarga.Motor;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        c.CapturaMotor = CapturaDeMotor.Variador;
        c.CorrienteEntradaVariadorA = entrada;
        c.ProteccionMaximaVariadorA = maxima;
        cuadro.Recalcular();
        return c;
    }

    [Fact]
    public void M20F2_AirePorCorrienteNominal_ElRangoDe440_22aYLosCriterios()
    {
        // 20 A de placa, 2 polos: conductor al 125 % = 25 A → 10 AWG (30 A a 60 °C) — 440-32. Techo: 175 % =
        // 35 A — 440-22(a). Rango 25, 30 y 35 A.
        var cuadro = Nuevo();
        var c = Aire(cuadro, 1, 20m);
        var r = c.Resultado!.Rango!;

        Assert.Equal("440-22(a)", r.Regla);
        Assert.Equal([25m, 30m, 35m], r.Valores);
        // Automático: el máximo (fase 2), como antes.
        Assert.Equal(CriterioDeProteccion.Automatico, c.CriterioProteccion);
        Assert.Equal(35m, c.Resultado.ProteccionA);
        Assert.Equal("10", c.Resultado.CalibreFase.Designacion);

        // Prioridad al conductor: 30 A, lo que protege a 10 AWG (y su tope de 240-4(d)).
        c.CriterioProteccion = CriterioDeProteccion.Conductor;
        cuadro.Recalcular();
        Assert.Equal(30m, c.Resultado!.ProteccionA);
        Assert.True(c.Resultado.Rango!.ProtegeAlConductor);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "440-22(a)" && x.Descripcion.Contains("corriente de arranque del motocompresor"));

        // Manual, abajo del mínimo: el mínimo.
        c.CriterioProteccion = CriterioDeProteccion.Manual;
        c.ProteccionElegidaA = 20m;
        cuadro.Recalcular();
        Assert.Equal(25m, c.Resultado!.ProteccionA);
    }

    [Fact]
    public void M20F2_AireConEl225_ElRangoEmpiezaArribaDelDel175()
    {
        // 16 A: al 175 % = 28 A → 25 A; declarado que no arranca, 225 % = 36 A → 35 A. El rango, arriba de 25 A.
        var cuadro = Nuevo();
        var c = Aire(cuadro, 1, 16m, arranque: true);

        Assert.Equal([30m, 35m], c.Resultado!.Rango!.Valores);
        Assert.Equal(25m, c.Resultado.Rango.ArribaDeA);
        Assert.Equal(35m, c.Resultado.ProteccionA);
    }

    [Fact]
    public void M20F2_AirePorPlaca_DeLaMcaALaMocp()
    {
        // MCA 18 A → 12 AWG (20 A); MOCP 30 A. Rango 20, 25 y 30 A — 440-4(b).
        var cuadro = Nuevo();
        var c = AirePorPlaca(cuadro, 1, 18m, 30m);

        Assert.Equal("440-4(b)", c.Resultado!.Rango!.Regla);
        Assert.Equal([20m, 25m, 30m], c.Resultado.Rango.Valores);
        Assert.Equal(30m, c.Resultado.ProteccionA);
        Assert.Equal(30m, c.Resultado.Rango.MaximoPermitidoA);

        c.CriterioProteccion = CriterioDeProteccion.Conductor;
        cuadro.Recalcular();
        Assert.Equal(20m, c.Resultado!.ProteccionA);
        Assert.Equal("12", c.Resultado.CalibreFase.Designacion);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "440-22(b)");
    }

    [Fact]
    public void M20F2_Variador_DelCientoVeinticincoDeLaEntradaALaMaximaDelFabricante()
    {
        // Entrada 20 A: conductor 25 A → 10 AWG (30 A) — 430-122(a). Máxima del fabricante 40 A — 110-3(b).
        var cuadro = Nuevo();
        var c = Variador(cuadro, 1, 20m, 40m);

        Assert.Equal("110-3(b)", c.Resultado!.Rango!.Regla);
        Assert.Equal([25m, 30m, 35m, 40m], c.Resultado.Rango.Valores);
        Assert.Equal(40m, c.Resultado.ProteccionA); // automático: la del fabricante
        Assert.Equal(40m, cuadro.ProteccionConCriterio(c, CriterioDeProteccion.Automatico));
        Assert.Equal(30m, cuadro.ProteccionConCriterio(c, CriterioDeProteccion.Conductor));

        cuadro.FijarProteccion(c, 30m);
        Assert.Equal(30m, c.Resultado!.ProteccionA);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "110-3(b)" && x.Descripcion.Contains("instrucciones del variador"));
        Assert.Contains(cuadro.Desglose(c)!.Proteccion, l => l.StartsWith("Máximo del rango: 40 A"));
        Assert.Contains(cuadro.Desglose(c)!.Proteccion, l => l == "Fijada por el proyectista: 30 A, dentro del rango");
    }

    [Fact]
    public void M20F2_ElAutomaticoDeAireYVariadorEsElMaximoYLoDice()
    {
        var cuadro = Nuevo();
        var aire = Aire(cuadro, 1, 20m);
        var variador = Variador(cuadro, 5, 20m, 40m);

        Assert.Equal("Calculada: 35 A, el máximo de 440-22(a), pensado para el arranque del motocompresor", cuadro.CriterioDeLaProteccion(aire));
        Assert.Equal("Calculada: 40 A, la máxima que marca el fabricante del variador — 110-3(b)", cuadro.CriterioDeLaProteccion(variador));
    }

    [Fact]
    public void M20F2_LaMemoriaDelAireDiceElRangoYElArranque()
    {
        var cuadro = Nuevo();
        var c = Aire(cuadro, 1, 20m);
        cuadro.FijarProteccion(c, 30m);

        var equipo = MemoriaDeCalculo.DeCircuito(cuadro, c).Equipo!;
        var renglones = equipo.Proteccion.ToDictionary(r => r.Rotulo, r => r.Valor);

        Assert.StartsWith("35 A", renglones["Máximo del rango — 440-22(a)"]);
        Assert.StartsWith("25 A (≥ 125 % de la corriente = 25 A) a 35 A", renglones["Rango permitido — 440-22(a)"]);
        Assert.Equal("30 A, dentro del rango; protege a 10 AWG (30.00 A)", renglones["Protección fijada por el proyectista — 240-4"]);
        Assert.Contains(equipo.Notas, n => n.StartsWith("Protección contra sobrecarga — 440-52: De fábrica"));
        Assert.Contains(equipo.Notas, n => n.StartsWith("Arranque — 440-22(a)") && n.Contains("subir hasta 35 A"));
    }

    [Fact]
    public void M20F2_El430_62aConElAireUsaElMaximoPermitido()
    {
        // El A/C con 30 A instalados: el techo de 430-62(a) parte de los 35 A que permite 440-22(a).
        var cuadro = Nuevo();
        var c = Aire(cuadro, 1, 20m);
        c.CriterioProteccion = CriterioDeProteccion.Conductor;
        cuadro.Recalcular();

        Assert.Equal(30m, c.Resultado!.ProteccionA);
        Assert.Equal(35m, c.Resultado.Rango!.MaximoPermitidoA);
        Assert.Equal(35m, cuadro.Alimentador.Resultado!.TechoProteccion430_62A);
    }

    [Fact]
    public void M20F2_ElCriterioDelAireYDelVariadorSeGuardaYSeAbre()
    {
        var cuadro = Nuevo();
        var aire = Aire(cuadro, 1, 20m);
        cuadro.FijarProteccion(aire, 25m);
        var variador = Variador(cuadro, 5, 20m, 40m);
        cuadro.FijarProteccion(variador, 30m);

        var abierto = ArchivoDelCuadro.Abrir(ArchivoDelCuadro.Guardar(cuadro, DateTimeOffset.Now), Motor).Cuadro!;

        Assert.Equal(25m, abierto.Circuitos[0].Resultado!.ProteccionA);
        Assert.Equal(30m, abierto.Circuitos[4].Resultado!.ProteccionA);
        Assert.Equal(ArchivoDelCuadro.Huella(cuadro), ArchivoDelCuadro.Huella(abierto));
    }
}
