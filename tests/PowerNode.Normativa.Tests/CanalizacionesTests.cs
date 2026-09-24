using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Tableros;
using PowerNode.DesignSuite.Normativa;
using Xunit;

namespace PowerNode.Normativa.Tests;

/// <summary>
/// El módulo de canalizaciones (nacido en la web, 2026-09-24): conteo de portadores, ajuste según el
/// tipo de canalización y llenado. Decisión: docs/decisiones/canalizaciones-y-agrupamiento.md.
/// </summary>
public class CanalizacionesTests
{
    private static readonly FuenteTablasJson Fuente = new(
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    private static readonly CalculadoraOcupacion Ocupacion = new(
        new TablaOcupacionJson(Fuente), new TablaTuboConduitJson(Fuente), new TablaDimensionesConductorJson(Fuente));

    private static CircuitoEnCanalizacion C(string n, int polos, bool neutro, params char[] barras) =>
        new(n, polos, neutro, barras);

    // ---- Portadores — 310-15(b)(3)(a), (b)(5), (b)(6) ----------------------------------------

    [Fact]
    public void TresMonopolaresConSuNeutro_SonSeisPortadores()
    {
        var conteo = ContadorDePortadores.Contar(
            [C("1", 1, true, 'A'), C("3", 1, true, 'B'), C("5", 1, true, 'C')],
            ConfiguracionTablero.TresFasesCuatroHilos, cargaNoLineal: false, neutroCompartido: false);

        Assert.Equal(6, conteo.Portadores);
        Assert.All(conteo.Desglose, d => Assert.Equal(1, d.Neutros));
    }

    [Fact]
    public void NeutroCompartidoEnTresBarras_SonTresPortadores_YAvisa210_4()
    {
        var conteo = ContadorDePortadores.Contar(
            [C("1", 1, true, 'A'), C("3", 1, true, 'B'), C("5", 1, true, 'C')],
            ConfiguracionTablero.TresFasesCuatroHilos, cargaNoLineal: false, neutroCompartido: true);

        Assert.True(conteo.NeutroCompartidoAplicado);
        Assert.Equal(3, conteo.Portadores); // 3 fases + neutro de desbalance que no cuenta
        Assert.Contains(conteo.Avisos, a => a.Contains("210-4(b)") && a.Contains("210-4(d)"));

        // Con carga no lineal el neutro compartido sí cuenta — (b)(5)(3).
        var noLineal = ContadorDePortadores.Contar(
            [C("1", 1, true, 'A'), C("3", 1, true, 'B'), C("5", 1, true, 'C')],
            ConfiguracionTablero.TresFasesCuatroHilos, cargaNoLineal: true, neutroCompartido: true);
        Assert.Equal(4, noLineal.Portadores);

        // Dos fases de una estrella con un neutro común: cuenta — (b)(5)(2).
        var dos = ContadorDePortadores.Contar(
            [C("1", 1, true, 'A'), C("3", 1, true, 'B')],
            ConfiguracionTablero.TresFasesCuatroHilos, cargaNoLineal: false, neutroCompartido: true);
        Assert.Equal(3, dos.Portadores);
    }

    [Fact]
    public void NeutroCompartidoEnLaMismaBarra_NoSeAplica()
    {
        var conteo = ContadorDePortadores.Contar(
            [C("1", 1, true, 'A'), C("7", 1, true, 'A')],
            ConfiguracionTablero.TresFasesCuatroHilos, cargaNoLineal: false, neutroCompartido: true);

        Assert.False(conteo.NeutroCompartidoAplicado);
        Assert.Equal(4, conteo.Portadores);
        Assert.Contains(conteo.Avisos, a => a.Contains("misma barra"));
    }

    [Fact]
    public void Multipolares_SegunNeutroYSistema()
    {
        int Portadores(int polos, bool neutro, ConfiguracionTablero s, bool noLineal = false) =>
            ContadorDePortadores.Contar([C("x", polos, neutro, 'A', 'B', 'C')], s, noLineal, false).Portadores;

        Assert.Equal(2, Portadores(2, false, ConfiguracionTablero.TresFasesCuatroHilos));
        Assert.Equal(3, Portadores(2, true, ConfiguracionTablero.TresFasesCuatroHilos)); // (b)(5)(2)
        Assert.Equal(2, Portadores(2, true, ConfiguracionTablero.UnaFaseTresHilos));     // (b)(5)(1)
        Assert.Equal(3, Portadores(3, false, ConfiguracionTablero.TresFasesCuatroHilos));
        Assert.Equal(3, Portadores(3, true, ConfiguracionTablero.TresFasesCuatroHilos)); // (b)(5)(1)
        Assert.Equal(4, Portadores(3, true, ConfiguracionTablero.TresFasesCuatroHilos, noLineal: true)); // (b)(5)(3)
    }

    [Fact]
    public void ParaleloEnElMismoTubo_CuentaCadaConductor()
    {
        var conteo = ContadorDePortadores.Contar(
            [new CircuitoEnCanalizacion("Alimentador", 3, true, ['A', 'B', 'C'], JuegosEnEsteTubo: 2)],
            ConfiguracionTablero.TresFasesCuatroHilos, cargaNoLineal: false, neutroCompartido: false);
        Assert.Equal(6, conteo.Portadores);
    }

    // ---- Ajuste según la canalización -------------------------------------------------------

    [Fact]
    public void Ajuste_SegunElTipoDeCanalizacion()
    {
        var tubo = AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.TuboConduit, 6);
        Assert.True(tubo.Aplica);
        Assert.Equal(6, tubo.ConductoresParaElMotor);

        Assert.False(AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.TuboConduit, 3).Aplica);

        var niple = AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.Niple, 12);
        Assert.False(niple.Aplica);
        Assert.Equal(1, niple.ConductoresParaElMotor);

        Assert.False(AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.DuctoMetalico, 12).Aplica);
        Assert.False(AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.DuctoMetalico, 30).Aplica);
        Assert.True(AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.DuctoMetalico, 31).Aplica);
        Assert.False(AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.CanalAuxiliarMetalico, 20).Aplica);

        Assert.Equal(12, AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.DuctoNoMetalico, 12).ConductoresParaElMotor);
        Assert.True(AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.CanalAuxiliarNoMetalico, 6).Aplica);

        Assert.False(AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.SuperficialMetalica, 12, 3000m, 15m).Aplica);
        Assert.True(AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.SuperficialMetalica, 12, 2000m, 15m).Aplica);
        Assert.True(AjusteDeAgrupamientoPorCanalizacion.Evaluar(TipoCanalizacion.SuperficialNoMetalica, 6).Aplica);
    }

    // ---- Llenado — Capítulo 10 ---------------------------------------------------------------

    private static ConductorEnCanalizacion Thhn(string awg, int cantidad) =>
        new("x", PapelConductor.Fase, awg, "THHN", null, cantidad);

    /// <summary>
    /// El Apéndice C (Tabla C-1, EMT) da cuántos conductores iguales caben en cada tamaño: THHN 14 AWG
    /// en 16 (½) = 12, 12 AWG = 9. Con las Tablas 1, 4 y 5 la calculadora tiene que llegar a lo mismo.
    /// </summary>
    [Fact]
    public void Llenado_CoincideConElApendiceC()
    {
        Assert.Equal(16, Ocupacion.Calcular(TipoCanalizacion.TuboConduit, TipoTuboConduit.Emt, [Thhn("14", 12)]).Tamano!.DesignacionMetrica);
        Assert.Equal(21, Ocupacion.Calcular(TipoCanalizacion.TuboConduit, TipoTuboConduit.Emt, [Thhn("14", 13)]).Tamano!.DesignacionMetrica);
        Assert.Equal(16, Ocupacion.Calcular(TipoCanalizacion.TuboConduit, TipoTuboConduit.Emt, [Thhn("12", 9)]).Tamano!.DesignacionMetrica);
        Assert.Equal(21, Ocupacion.Calcular(TipoCanalizacion.TuboConduit, TipoTuboConduit.Emt, [Thhn("12", 10)]).Tamano!.DesignacionMetrica);
    }

    [Fact]
    public void Llenado_TierraDesnudaYMezclaDeCalibres()
    {
        var r = Ocupacion.Calcular(TipoCanalizacion.TuboConduit, TipoTuboConduit.PvcCedula40,
        [
            new("1", PapelConductor.Fase, "10", "THHN", null, 2),
            new("1", PapelConductor.Neutro, "10", "THHN"),
            new("1", PapelConductor.Tierra, "10", null),
        ]);

        Assert.Equal(4, r.NumeroConductores);
        Assert.Equal(40m, r.PorcentajePermitido);
        Assert.Contains(r.Renglones, x => x.Fuente.StartsWith("Tabla 8"));
        Assert.NotNull(r.Tamano);
        Assert.False(r.Excede);
    }

    [Fact]
    public void Llenado_Nota2_TresConductoresQuePuedenAtascarse()
    {
        // Busca un caso real: 3 conductores iguales cuyo tamaño mínimo cae en relación 2.8–3.2.
        var dims = new TablaDimensionesConductorJson(Fuente);
        var tubos = new TablaTuboConduitJson(Fuente);
        var ocup = new TablaOcupacionJson(Fuente);
        foreach (var awg in new[] { "14", "12", "10", "8", "6", "4", "3", "2", "1", "1/0", "2/0", "3/0", "4/0", "250", "300", "350", "400", "500" })
        {
            if (dims.Aislado(awg, "THHN") is not { } d) continue;
            var minimo = tubos.Tamanos(TipoTuboConduit.Emt).FirstOrDefault(t => t.AreaDisponible(ocup.PorcentajeMaximo(3)) >= 3 * d.AreaMm2);
            if (minimo is null || minimo.DiametroInteriorMm / d.DiametroMm is < 2.8m or > 3.2m) continue;

            var r = Ocupacion.Calcular(TipoCanalizacion.TuboConduit, TipoTuboConduit.Emt, [Thhn(awg, 3)]);
            Assert.True(r.Nota2Aplicada);
            Assert.True(r.Tamano!.DesignacionMetrica > minimo.DesignacionMetrica);
            return;
        }
        Assert.Fail("No se encontró un caso de la Nota 2 en EMT con THHN: revisar la prueba.");
    }

    [Fact]
    public void Llenado_SinDiametroDelFabricante_NoHayTamano()
    {
        var r = Ocupacion.Calcular(TipoCanalizacion.TuboConduit, TipoTuboConduit.PvcCedula40,
            [new("1", PapelConductor.Fase, "12", "THW-LS", null, 2)]);
        Assert.Null(r.Tamano);
        Assert.Contains("12 AWG THW-LS", r.Faltantes);

        var conDiametro = Ocupacion.Calcular(TipoCanalizacion.TuboConduit, TipoTuboConduit.PvcCedula40,
            [new("1", PapelConductor.Fase, "12", "THW-LS", 3.9m, 2)]);
        Assert.NotNull(conDiametro.Tamano);
    }

    [Fact]
    public void Llenado_TamanoFijadoQueNoAlcanza_Avisa()
    {
        var r = Ocupacion.Calcular(TipoCanalizacion.TuboConduit, TipoTuboConduit.Emt, [Thhn("14", 20)], tamanoFijado: 16);
        Assert.True(r.Excede);
        Assert.NotEmpty(r.Avisos);
    }

    [Fact]
    public void Llenado_DuctoAl20Pct()
    {
        var sinMedidas = Ocupacion.Calcular(TipoCanalizacion.DuctoMetalico, TipoTuboConduit.Emt, [Thhn("12", 10)]);
        Assert.Equal(85.81m / 0.20m, sinMedidas.AreaMinimaMm2);

        var chico = Ocupacion.Calcular(TipoCanalizacion.DuctoMetalico, TipoTuboConduit.Emt, [Thhn("12", 10)], anchoMm: 20m, altoMm: 20m);
        Assert.True(chico.Excede);
        var bien = Ocupacion.Calcular(TipoCanalizacion.DuctoMetalico, TipoTuboConduit.Emt, [Thhn("12", 10)], anchoMm: 100m, altoMm: 100m);
        Assert.False(bien.Excede);
    }
}
