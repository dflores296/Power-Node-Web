using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Auditoría de motores, Art. 430, del 2026-10-03</b> (David; caja negra contra <c>d9a1ff8</c>). El cálculo
/// coincidió en los 25 circuitos; los hallazgos son de la interfaz. Cada prueba lleva el número del hallazgo.
/// Lo que solo se ve en el navegador (el selector que decía 175 A con el cálculo en 350 A, el botón «Nuevo»)
/// se revisó allí; aquí queda el modelo que lo sostiene.
/// </summary>
public class AuditoriaMotores20261003Tests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

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

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int espacio) =>
        cuadro.Circuitos.Single(x => x.Espacio == espacio);

    private static CargaDelCircuito Linea(
        CuadroDeCarga cuadro, CircuitoDelCuadro c, SubtipoDeCarga subtipo, decimal unitaria,
        string descripcion = "", UnidadConsumo unidad = UnidadConsumo.VoltAmperes)
    {
        var a = c.AgregarCarga();
        a.Subtipo = subtipo;
        a.Descripcion = descripcion;
        a.Unidad = unidad;
        a.CargaUnitaria = unitaria;
        cuadro.Recalcular();
        return a;
    }

    /// <summary>La bomba de 100 HP del caso C2: 440 V, 3F-3H, FLC 124 A (Tabla 430-250, columna de 460 V).</summary>
    private static (CuadroDeCarga Cuadro, CircuitoDelCuadro Bomba) LaBombaDe100Hp()
    {
        var cuadro = Nuevo(440m, 3, 3);
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = 100m;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        cuadro.Recalcular();
        return (cuadro, c);
    }

    // ---- #1 · I-184: la Excepción 2 marcada y desmarcada ----------------------------------------------

    /// <summary>
    /// 250 % × 124 A = 310 A → 350 A (Excepción 1); el rango, de 175 A (≥ 125 % = 155 A) a 350 A, y arriba de
    /// 1 HP la calculada es el máximo. Con la Excepción 2 (FLC &gt; 100 A: 300 % = 372 A → 350 A) solo queda
    /// 350 A. Desmarcarla regresa el rango completo y la calculada sigue en 350 A: el selector decía 175 A
    /// porque reutilizaba la opción por posición, no porque el cálculo cambiara.
    /// </summary>
    [Fact]
    public void I184_LaExcepcion2MarcadaYDesmarcadaDejaLaCalculadaEn350()
    {
        var (cuadro, bomba) = LaBombaDe100Hp();
        Assert.Equal(124m, bomba.FlcA);
        Assert.Equal([175m, 200m, 225m, 250m, 300m, 350m], bomba.Resultado!.Rango!.Valores);
        Assert.Equal(350m, bomba.Resultado.ProteccionA);

        bomba.NoArrancaConLaTabla = true;
        cuadro.Recalcular();
        Assert.Equal([350m], bomba.Resultado!.Rango!.Valores);
        Assert.Equal(350m, bomba.Resultado.ProteccionA);

        bomba.NoArrancaConLaTabla = false;
        cuadro.Recalcular();
        Assert.Equal([175m, 200m, 225m, 250m, 300m, 350m], bomba.Resultado!.Rango!.Valores);
        Assert.Equal(350m, bomba.Resultado.ProteccionA);
        Assert.Contains(bomba.Resultado.ProteccionA, bomba.Resultado.Rango.Valores);
        Assert.Equal(CriterioDeProteccion.Automatico, bomba.CriterioProteccion);
        Assert.Empty(cuadro.TomarProteccionesQueRegresaron());
    }

    /// <summary>
    /// Fijada en 250 A, la Excepción 2 cambia el rango: la fijada regresa a la calculada, con aviso (I-182). Al
    /// desmarcarla, la calculada sigue en 350 A — no cae al mínimo del rango.
    /// </summary>
    [Fact]
    public void I184_UnaFijadaQueLaExcepcion2BorraRegresaALaCalculadaNoAlMinimo()
    {
        var (cuadro, bomba) = LaBombaDe100Hp();
        cuadro.FijarProteccion(bomba, 250m);
        Assert.Equal(250m, bomba.Resultado!.ProteccionA);

        bomba.NoArrancaConLaTabla = true;
        cuadro.Recalcular();
        Assert.Equal("Circuito 1: cambió el rango de la protección (solo 350 A); la fijada, 250 A, regresó al calculado, 350 A.",
            ProteccionQueRegreso.Aviso(cuadro.TomarProteccionesQueRegresaron()));
        Assert.Equal(350m, bomba.Resultado!.ProteccionA);

        bomba.NoArrancaConLaTabla = false;
        cuadro.Recalcular();
        Assert.Equal(350m, bomba.Resultado!.ProteccionA);
        Assert.Equal(CriterioDeProteccion.Automatico, bomba.CriterioProteccion);
    }

    // ---- #2 y #3 · I-185: quitar la única carga ------------------------------------------------------

    /// <summary>
    /// La lavadora del caso A: 1500 VA en Contactos · Lavadora, 20 A por 210-11(c)(2). Antes, quitarla dejaba
    /// «Uso general · carga total» con los mismos 1500 VA y 15 A: la continua y la no continua del renglón eran
    /// la suma de sus líneas y se quedaban. Ahora el circuito queda vacío, como uno nuevo; la descripción, la
    /// longitud y los polos son del espacio y se quedan.
    /// </summary>
    [Fact]
    public void I185_QuitarLaUnicaCargaVaciaElCircuito()
    {
        var cuadro = Nuevo();
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar;
        var c = Espacio(cuadro, 11);
        c.Descripcion = "Lavado";
        c.LongitudM = 15m;
        var lavadora = Linea(cuadro, c, SubtipoDeCarga.ContactoLavadora, 1500m, "Lavadora");
        Assert.Equal(20m, c.Resultado!.ProteccionA);
        Assert.Equal(ClaseDeCircuito.ParaAparatos, c.ClaseDelCircuito);

        c.QuitarCarga(lavadora);
        cuadro.Recalcular();

        Assert.False(c.TieneCarga);
        Assert.Null(c.Resultado);
        Assert.Null(c.ClaseDelCircuito);
        Assert.Empty(c.Cargas);
        Assert.Equal(0m, c.Continua);
        Assert.Equal(0m, c.NoContinua);
        Assert.Equal(0m, c.CargaInstaladaVA);
        Assert.False(c.TipoElegido);
        Assert.Equal("Lavado", c.Descripcion);
        Assert.Equal(15m, c.LongitudM);
        Assert.DoesNotContain(cuadro.Circuitos, x => x.TieneCarga);
    }

    [Fact]
    public void I185_QuitarUnaDeDosDejaLaOtra()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        var primera = Linea(cuadro, c, SubtipoDeCarga.Luminarias, 600m, "Pasillo");
        Linea(cuadro, c, SubtipoDeCarga.Luminarias, 400m, "Escalera");

        c.QuitarCarga(primera);
        cuadro.Recalcular();

        Assert.True(c.TieneCarga);
        Assert.Equal("Escalera", Assert.Single(c.Cargas).Descripcion);
        Assert.Equal(400m, c.CargaInstaladaVA);
    }

    /// <summary>Un alimentador a otro tablero: sin su línea, el recálculo la volvía a armar de la continua que quedaba.</summary>
    [Fact]
    public void I185_QuitarElUnicoTableroVaciaElAlimentador()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Tablero;
        var tablero = c.AgregarCarga();
        tablero.Subtipo = SubtipoDeCarga.TableroAlimentado;
        tablero.CargaUnitaria = 5000m;
        tablero.NoContinua = 3000m;
        cuadro.Recalcular();
        Assert.True(c.TieneCarga);

        c.QuitarCarga(tablero);
        cuadro.Recalcular();

        Assert.False(c.TieneCarga);
        Assert.Empty(c.Cargas);
    }

    /// <summary>
    /// Las líneas de otro tipo que un motor del renglón conserva sin contar (I-113) no son su carga: quitar la
    /// última no le borra el motor.
    /// </summary>
    [Fact]
    public void I185_QuitarUnaLineaQueElMotorConservaSinContarNoLoVacia()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = 0.5m;
        var vieja = new CargaDelCircuito { Descripcion = "Contactos", CargaUnitaria = 360m };
        c.Cargas.Add(vieja);
        cuadro.Recalcular();
        Assert.False(c.TieneDesglose);

        c.QuitarCarga(vieja);
        cuadro.Recalcular();

        Assert.True(c.TieneCarga);
        Assert.Equal(0.5m, c.Hp);
        Assert.Equal(8.9m, c.FlcA);
    }

    // ---- #4 · M-21: la columna de terminales sale de la protección escogida ---------------------------

    /// <summary>
    /// El molino de 25 HP a 220 V: FLC 68 A, 125 % = 85 A, rango de 90 a 175 A. Con 175 A (la calculada, el
    /// máximo arriba de 1 HP) el circuito es de más de 100 A: 75 °C y 4 AWG (85 A) — 110-14(c)(1)b. Fijada en
    /// 100 A: 60 °C y 3 AWG (85 A) — 110-14(c)(1)a. Correcto por la letra; la cita ahora dice el inciso. Si
    /// el conductor debe salir del motor y no de la protección es criterio: por decidir (David).
    /// </summary>
    [Fact]
    public void M21_LaCitaDeTerminalesDiceElInciso()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = 25m;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        cuadro.Recalcular();
        Assert.Equal(68m, c.FlcA);
        Assert.Equal(175m, c.Resultado!.ProteccionA);
        Assert.Equal("4", c.Resultado.CalibreFase.Designacion);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "110-14(c)(1)" && x.Descripcion.EndsWith("terminales a 75°C (110-14(c)(1)b.)", StringComparison.Ordinal));

        cuadro.FijarProteccion(c, 100m);
        Assert.Equal("3", c.Resultado!.CalibreFase.Designacion);
        Assert.Contains(c.Resultado.Citas, x => x.Referencia == "110-14(c)(1)" && x.Descripcion.EndsWith("terminales a 60°C (110-14(c)(1)a.)", StringComparison.Ordinal));
    }

    // ---- #6 · I-197: 430-62(a) con la máxima que permite 430-52 --------------------------------------

    /// <summary>
    /// El techo de 430-62(a) usa la máxima que permite 430-52 al derivado mayor (350 A), no la instalada: la
    /// cita y la memoria lo dicen así; antes, «la mayor protección de derivado».
    /// </summary>
    [Fact]
    public void I197_ElTechoDe430_62aDiceQueEsLaMaximaPermitidaNoLaInstalada()
    {
        var (cuadro, bomba) = LaBombaDe100Hp();
        cuadro.FijarProteccion(bomba, 175m);
        Assert.Equal(175m, bomba.Resultado!.ProteccionA);

        var cita = Assert.Single(cuadro.Alimentador.Resultado!.Citas, x => x.Referencia == "430-62(a)" && x.Descripcion.StartsWith("Techo", StringComparison.Ordinal));
        Assert.Contains("350 A (la máxima que permiten 430-52 o 440-22(a) al mayor derivado del grupo, no la instalada)", cita.Descripcion);
    }

    // ---- #12 · I-190, #18 · I-196: la tensión y la columna del motor ---------------------------------

    [Fact]
    public void I190_LaTensionDeCadaAlimentacionDelMotor()
    {
        var cuadro = Nuevo();
        Assert.Equal(127m, cuadro.TensionDelMotorV(1));
        Assert.Equal(220m, cuadro.TensionDelMotorV(2));
        Assert.Equal(220m, cuadro.TensionDelMotorV(3));
    }

    /// <summary>A 600 V la FLC se lee en la columna de 575 V (Tabla 430-250); el rótulo lo dice, como a 440 V la de 460 V.</summary>
    [Theory]
    [InlineData(600, "Trifásico 600 V · Tabla 430-250, columna de 575 V")]
    [InlineData(440, "Trifásico 440 V · Tabla 430-250, columna de 460 V")]
    [InlineData(460, "Trifásico 460 V · Tabla 430-250")]
    public void I196_ElRotuloDelMotorDiceLaColumnaQueSeLee(int tension, string rotulo)
    {
        var cuadro = Nuevo(tension, 3, 3);
        Assert.Equal(rotulo, cuadro.AlimentacionDelMotor(3));
    }

    // ---- #14 · I-192: centro de carga a más de 240 V -------------------------------------------------

    [Fact]
    public void I192_UnCentroDeCargaA440VAvisa()
    {
        var cuadro = Nuevo(440m, 3, 3);
        Assert.Equal(SerieDeInterruptores.CentroDeCargaNema, cuadro.Datos.SerieInterruptores);
        Assert.Contains("240 V como máximo", cuadro.Datos.AvisoSerie);
        Assert.Contains("110-3(b)", cuadro.Datos.AvisoSerie);

        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.NomCompleta;
        Assert.Null(cuadro.Datos.AvisoSerie);

        Assert.Null(Nuevo().Datos.AvisoSerie); // 220/127 V
        Assert.Null(Nuevo(240m, 1, 3).Datos.AvisoSerie); // 120/240 V
    }

    // ---- #13 · I-191: el nombre de la carga, sin descripción del espacio -----------------------------

    [Fact]
    public void I191_SinDescripcionElCircuitoSeNombraPorSuCarga()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        Linea(cuadro, c, SubtipoDeCarga.OtraCargaEspecifica, 1500m, "Lavadora");
        Assert.Equal("Lavadora", c.NombreDeSusCargas);
        Assert.Equal("Lavadora", MemoriaDeCalculo.Etiqueta(c));
        Assert.Contains("— Lavadora", MemoriaDeCalculo.DeCircuito(cuadro, c).Sujeto);

        Linea(cuadro, c, SubtipoDeCarga.OtraCargaEspecifica, 500m, "Secadora");
        Assert.Equal("Lavadora, Secadora", c.NombreDeSusCargas);

        // Con la descripción del espacio, manda ella.
        c.Descripcion = "Cuarto de lavado";
        Assert.Contains("— Cuarto de lavado", MemoriaDeCalculo.DeCircuito(cuadro, c).Sujeto);
    }

    [Fact]
    public void I191_UnNombreGenericoNoCuentaComoNombre()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        Linea(cuadro, c, SubtipoDeCarga.Luminarias, 600m, $"{SubtipoDeCarga.Luminarias.NombreGenerico()} 1");
        Assert.Null(c.NombreDeSusCargas);
        Assert.Equal("Alumbrado", MemoriaDeCalculo.Etiqueta(c));
    }

    // ---- #16 · I-194: la memoria con dos decimales y las fases en orden ------------------------------

    /// <summary>El refrigerador de 3.5 A a 127 V: 444.5 VA. La memoria decía «1 × 4 A = 445 VA».</summary>
    [Fact]
    public void I194_LaMemoriaNoRedondeaLaCorrienteDePlaca()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        Linea(cuadro, c, SubtipoDeCarga.OtraCargaEspecifica, 3.5m, "Refrigerador", UnidadConsumo.Amperes);

        var renglon = Assert.Single(MemoriaDeCalculo.DeCircuito(cuadro, c).Desglose!, r => r.Rotulo.Contains("Refrigerador"));
        Assert.StartsWith("1 × 3.50 A = 445 VA", renglon.Valor);
    }

    /// <summary>2F-3H: un bipolar en el espacio 3 toca la barra B y luego la A. La memoria decía «fase BA».</summary>
    [Fact]
    public void I194_LasFasesDeUnBipolarVanEnOrden()
    {
        var cuadro = Nuevo(220m, 2, 3);
        var c = Espacio(cuadro, 3);
        Assert.Null(cuadro.CambiarPolos(c, 2));
        Linea(cuadro, c, SubtipoDeCarga.OtraCargaEspecifica, 2000m, "Bomba");
        Assert.Equal("BA", c.Fases);

        var sujeto = MemoriaDeCalculo.DeCircuito(cuadro, c).Sujeto;
        Assert.EndsWith("fases A-B", sujeto);
        Assert.EndsWith("fase A", MemoriaDeCalculo.DeCircuito(cuadro, Linea1(cuadro)).Sujeto);

        static CircuitoDelCuadro Linea1(CuadroDeCarga cuadro)
        {
            var uno = Espacio(cuadro, 1);
            Linea(cuadro, uno, SubtipoDeCarga.Luminarias, 300m, "Pasillo");
            return uno;
        }
    }
}
