using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using Xunit;

namespace PowerNode.Web.Tests;

/// <summary>
/// Canalizaciones en el cuadro — I-39, I-40, I-41 (decisión canalizaciones-y-agrupamiento,
/// 2026-09-24): el agrupamiento sale de qué circuitos van juntos y cada canalización se dimensiona.
/// </summary>
public class CanalizacionesDelCuadroTests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static CuadroDeCarga Nuevo(int fases = 3, int hilos = 4)
    {
        var cuadro = new CuadroDeCarga(new MotorNom(Json));
        cuadro.Datos.NumeroEspacios = 12;
        cuadro.Datos.Fases = fases;
        cuadro.Datos.Hilos = hilos;
        cuadro.Recalcular();
        return cuadro;
    }

    private static CircuitoDelCuadro Carga(CuadroDeCarga cuadro, int espacio, decimal amperes, string? tubo = null)
    {
        var c = cuadro.Circuitos.Single(x => x.Espacio == espacio);
        c.Unidad = UnidadConsumo.Amperes;
        c.NoContinua = amperes;
        c.LongitudM = 5m;
        c.Canalizacion = tubo;
        return c;
    }

    [Fact]
    public void I41_SoloLosMonopolaresLlevanNeutro_SalvoLaCasilla()
    {
        var cuadro = Nuevo();
        var mono = Carga(cuadro, 2, 10m);
        var bipolar = Carga(cuadro, 1, 10m);
        cuadro.CambiarPolos(bipolar, 2);

        Assert.True(mono.LlevaNeutro);
        Assert.False(bipolar.LlevaNeutro);
        Assert.Equal(2, bipolar.CanalizacionEfectiva!.Conteo!.Portadores);

        bipolar.ConNeutro = true;
        cuadro.Recalcular();
        Assert.True(bipolar.LlevaNeutro);
        Assert.Equal(3, bipolar.CanalizacionEfectiva!.Conteo!.Portadores); // (b)(5)(2)

        var delta = Nuevo(fases: 3, hilos: 3);
        var sinNeutro = Carga(delta, 1, 10m);
        delta.CambiarPolos(sinNeutro, 3);
        sinNeutro.ConNeutro = true;
        delta.Recalcular();
        Assert.False(sinNeutro.LlevaNeutro);
    }

    [Fact]
    public void I39_TresMonopolaresEnUnTubo_SubenElCalibre_YElAlimentadorNoSeEntera()
    {
        // TW (60 °C): 12 AWG da 20 A ≥ 19 A solo; con 6 portadores, 20 × 0.80 = 16 A < 19 A → 10 AWG.
        var cuadro = Nuevo();
        cuadro.Datos.TipoAislamiento = "TW";
        var t1 = cuadro.Datos.NuevaCanalizacion();
        var c1 = Carga(cuadro, 1, 19m, t1.Id);
        Carga(cuadro, 3, 19m, t1.Id);
        Carga(cuadro, 5, 19m, t1.Id);
        cuadro.Recalcular();

        Assert.Equal(6, t1.Conteo!.Portadores);
        Assert.Equal(0.80m, t1.FactorAgrupamiento);
        Assert.Equal("10", c1.Resultado!.CalibreFase.Designacion);
        Assert.Equal(3, t1.Circuitos.Count);
        Assert.NotNull(t1.Ocupacion!.Tamano);

        // El alimentador va solo: 3F-4H, neutro de desbalance → 3 portadores, sin ajuste.
        Assert.Equal(3, cuadro.Datos.CanalizacionAlimentador.Conteo!.Portadores);
        Assert.Equal(1m, cuadro.Datos.CanalizacionAlimentador.FactorAgrupamiento);

        // Solo, el mismo circuito es 12 AWG.
        var solo = Nuevo();
        solo.Datos.TipoAislamiento = "TW";
        var c = Carga(solo, 1, 19m);
        solo.Recalcular();
        Assert.Equal("12", c.Resultado!.CalibreFase.Designacion);
    }

    [Fact]
    public void NeutroCompartido_TresPortadores_YAviso210_4()
    {
        var cuadro = Nuevo();
        cuadro.Datos.TipoAislamiento = "TW";
        var t1 = cuadro.Datos.NuevaCanalizacion();
        t1.NeutroCompartido = true;
        var c1 = Carga(cuadro, 1, 19m, t1.Id);
        Carga(cuadro, 3, 19m, t1.Id);
        Carga(cuadro, 5, 19m, t1.Id);
        cuadro.Recalcular();

        Assert.True(t1.Conteo!.NeutroCompartidoAplicado);
        Assert.Equal(3, t1.Conteo.Portadores);
        Assert.Equal("12", c1.Resultado!.CalibreFase.Designacion);
        Assert.Contains(cuadro.AvisosDeCanalizaciones, a => a.Contains("210-4(b)"));
        // Un solo neutro en el llenado: 3 fases + 1 neutro + 3 tierras.
        Assert.Equal(7, t1.Ocupacion!.NumeroConductores);
    }

    [Fact]
    public void DuctoMetalico_SinAjusteHasta30Portadores()
    {
        var cuadro = Nuevo();
        cuadro.Datos.TipoAislamiento = "TW";
        var t1 = cuadro.Datos.NuevaCanalizacion();
        t1.Tipo = TipoCanalizacion.DuctoMetalico;
        var c1 = Carga(cuadro, 1, 19m, t1.Id);
        Carga(cuadro, 3, 19m, t1.Id);
        Carga(cuadro, 5, 19m, t1.Id);
        cuadro.Recalcular();

        Assert.Equal(6, t1.Conteo!.Portadores);
        Assert.False(t1.Ajuste!.Aplica);
        Assert.Equal("12", c1.Resultado!.CalibreFase.Designacion);
        Assert.NotNull(t1.Ocupacion!.AreaMinimaMm2); // sin medidas: el área mínima
    }

    [Fact]
    public void Azotea_SumaTemperatura()
    {
        var cuadro = Nuevo();
        var c = Carga(cuadro, 1, 19m);
        cuadro.Recalcular();
        Assert.Equal(0m, c.CanalizacionEfectiva!.SumadorAzoteaC);

        c.CanalizacionPropia.AlturaSobreTechoMm = 50m; // más de 13 hasta 90 → +22 °C
        cuadro.Recalcular();
        Assert.Equal(22m, c.CanalizacionEfectiva!.SumadorAzoteaC);
        Assert.Contains(c.Resultado!.Citas, x => x.Referencia == "310-15(b)(2)(a)" && x.Descripcion.Contains("52"));
    }

    [Fact]
    public void AislamientoLs_PideElDiametroDelFabricante()
    {
        var cuadro = Nuevo();
        cuadro.Datos.TipoAislamiento = "THW-LS";
        var c = Carga(cuadro, 1, 10m);
        cuadro.Recalcular();

        var canal = c.CanalizacionEfectiva!;
        Assert.Null(canal.Ocupacion!.Tamano);
        Assert.NotEmpty(canal.Ocupacion.Faltantes);

        var calibre = c.Resultado!.CalibreFase.Designacion;
        cuadro.Datos.DiametrosFabricante[DatosDelTablero.ClaveDiametro("THW-LS", calibre)] = 3.9m;
        cuadro.Datos.DiametrosFabricante[DatosDelTablero.ClaveDiametro("THW-LS", c.Resultado.CalibreTierra.Designacion)] = 3.9m;
        cuadro.Recalcular();
        Assert.NotNull(c.CanalizacionEfectiva!.Ocupacion!.Tamano);
    }

    [Fact]
    public void TodaCanalizacionNaceEmt_YLaPropiaSeGuardaConElCircuito()
    {
        // David, 2026-09-24: ya no hay canalización por omisión en Condiciones; cada tubo nace EMT y
        // el ingeniero decide en cada uno. La propia es del circuito: sobrevive al recálculo y a
        // pasar por un tubo compartido y regresar.
        var cuadro = Nuevo();
        var c = Carga(cuadro, 1, 10m);
        cuadro.Recalcular();
        Assert.Same(c.CanalizacionPropia, c.CanalizacionEfectiva);
        var t1 = cuadro.Datos.NuevaCanalizacion();
        Assert.Equal(TipoTuboConduit.Emt, c.CanalizacionPropia.Tubo);
        Assert.Equal(TipoTuboConduit.Emt, t1.Tubo);
        Assert.Equal(TipoTuboConduit.Emt, cuadro.Datos.CanalizacionAlimentador.Tubo);
        Assert.Equal(MaterialCanalizacion.Acero, c.CanalizacionPropia.MaterialParaTabla9);

        c.CanalizacionPropia.Nombre = "Bajada cocina";
        c.CanalizacionPropia.Tubo = TipoTuboConduit.PvcCedula40;
        c.CanalizacionPropia.TierraDesnuda = true;
        c.Canalizacion = t1.Id;
        cuadro.Recalcular();
        Assert.Same(t1, c.CanalizacionEfectiva);

        c.Canalizacion = null;
        cuadro.Recalcular();
        Assert.Same(c.CanalizacionPropia, c.CanalizacionEfectiva);
        Assert.Equal("Bajada cocina", c.CanalizacionPropia.Nombre);
        Assert.Equal(TipoTuboConduit.PvcCedula40, c.CanalizacionPropia.Tubo);
        Assert.True(c.CanalizacionPropia.TierraDesnuda);
        Assert.Equal(MaterialCanalizacion.Pvc, c.CanalizacionPropia.MaterialParaTabla9);
    }

    [Fact]
    public void ElNombreEsEditable_YLaClaveNoCambia()
    {
        var cuadro = Nuevo();
        var t1 = cuadro.Datos.NuevaCanalizacion();
        t1.Nombre = "Tubo pasillo";
        var c = Carga(cuadro, 1, 10m, t1.Id);
        cuadro.Recalcular();

        Assert.Equal("T1", t1.Id);
        Assert.Same(t1, c.CanalizacionEfectiva);
        var hojas = PowerNode.Web.Modelo.Memoria.MemoriaDeCalculo.Canalizaciones(cuadro);
        Assert.Contains(hojas, h => h.Sujeto.StartsWith("Canalización Tubo pasillo"));
    }

    [Fact]
    public void QuitarUnaCanalizacion_RegresaSusCircuitosASuPropia()
    {
        var cuadro = Nuevo();
        var t1 = cuadro.Datos.NuevaCanalizacion();
        var c = Carga(cuadro, 1, 10m, t1.Id);
        cuadro.Recalcular();
        Assert.Same(t1, c.CanalizacionEfectiva);

        cuadro.QuitarCanalizacion(t1);
        Assert.Null(c.Canalizacion);
        Assert.NotSame(t1, c.CanalizacionEfectiva);
        Assert.Equal("T1", cuadro.Datos.NuevaCanalizacion().Id); // el número se reutiliza
    }

    [Fact]
    public void AlimentadorEnParalelo_UnJuegoPorTubo_OTodosJuntos()
    {
        // El motor pasa a conductores en paralelo solo cuando ni el calibre mayor cumple la caída
        // (SeleccionConductor, caso obligado): cuatro trifásicos de 60 A con un alimentador de 400 m.
        var cuadro = Nuevo();
        cuadro.Datos.LongitudAlimentadorM = 400m;
        foreach (var espacio in new[] { 1, 2, 7, 8 })
        {
            var c = cuadro.Circuitos.Single(x => x.Espacio == espacio);
            c.Tipo = PowerNode.DesignSuite.Calculo.Unidades.TipoCarga.Equipo;
            c.Unidad = UnidadConsumo.Amperes;
            c.Continua = 60m;
            c.LongitudM = 5m;
            Assert.Null(cuadro.CambiarPolos(c, 3));
        }
        cuadro.Recalcular();

        Assert.Null(cuadro.Alimentador.Error);
        var a = cuadro.Alimentador.Resultado!;
        var canal = cuadro.Datos.CanalizacionAlimentador;
        Assert.True(a.NumeroConductoresParalelo > 1);
        Assert.Equal(a.NumeroConductoresParalelo, canal.CanalizacionesIguales);
        Assert.Equal(3, canal.Conteo!.Portadores);

        canal.JuegosEnUnTubo = true;
        cuadro.Recalcular();
        var b = cuadro.Alimentador.Resultado!;
        Assert.Equal(1, canal.CanalizacionesIguales);
        Assert.Equal(3 * b.NumeroConductoresParalelo, canal.Conteo!.Portadores);
    }

    [Fact]
    public void Memoria_Seccion4DiceLaCanalizacion_YCadaUnaTieneSuHoja()
    {
        var cuadro = Nuevo();
        cuadro.Datos.TipoAislamiento = "TW";
        var t1 = cuadro.Datos.NuevaCanalizacion();
        var c1 = Carga(cuadro, 1, 19m, t1.Id);
        Carga(cuadro, 3, 19m, t1.Id);
        Carga(cuadro, 5, 19m, t1.Id);
        cuadro.Recalcular();

        var seccion4 = PowerNode.Web.Modelo.Memoria.MemoriaDeCalculo.Secciones(PowerNode.Web.Modelo.Memoria.MemoriaDeCalculo.DeCircuito(cuadro, c1))[3];
        Assert.Contains(seccion4.Renglones, r => r.Valor.Contains("Canalización T1") && r.Valor.Contains("6 portadores"));
        Assert.Contains(seccion4.Renglones, r => r.Rotulo.StartsWith("Factor de agrupamiento") && r.Valor.StartsWith("0.80"));

        var hojas = PowerNode.Web.Modelo.Memoria.MemoriaDeCalculo.Canalizaciones(cuadro);
        var t1Hoja = hojas.Single(h => h.Sujeto.StartsWith("Canalización T1"));
        Assert.Contains(t1Hoja.Bloques[2].Formulas, f => f.StartsWith("Capítulo 10, Tabla 4"));
        // TW 10 AWG: la errata de la Tabla 5 llega al papel.
        Assert.Contains(t1Hoja.Bloques[0].Renglones, r => r.Valor.Contains("con errata"));
        Assert.Single(t1Hoja.Bloques[2].Formulas, f => f.StartsWith("ERRATA Tabla 5"));
        Assert.Contains(hojas, h => h.Sujeto.StartsWith("Canalización del alimentador"));
    }

    [Fact]
    public void TodosLosAislamientosDelMotor_CalculanYSeDimensionan()
    {
        // El selector ofrece los 17 de la Tabla 310-104(a). Cada uno calcula el circuito, y el
        // llenado sale de la Tabla 5 o pide el diámetro del fabricante (Nota 5) — nunca revienta.
        var motor = new MotorNom(Json);
        Assert.Equal(17, motor.Aislamiento.DesignacionesReconocidas.Count);
        foreach (var aislamiento in motor.Aislamiento.DesignacionesReconocidas)
        {
            var cuadro = new CuadroDeCarga(motor);
            cuadro.Datos.NumeroEspacios = 6;
            cuadro.Datos.TipoAislamiento = aislamiento;
            var c = Carga(cuadro, 1, 10m);
            cuadro.Recalcular();

            Assert.True(c.Resultado is not null, $"{aislamiento}: {c.Error}");
            var ocupacion = c.CanalizacionEfectiva!.Ocupacion!;
            var enTabla5 = motor.Dimensiones.Aislado("12", aislamiento) is not null;
            if (enTabla5) Assert.NotNull(ocupacion.Tamano);
            else Assert.NotEmpty(ocupacion.Faltantes);
        }
    }
}
