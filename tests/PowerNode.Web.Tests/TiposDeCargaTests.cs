using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>Seis tipos de carga: Motor (Art. 430) y A/C y refrigeración (Art. 440) por separado</b> — I-74,
/// <c>docs/decisiones/tipos-de-carga.md</c>. Los números salen de la norma: FLC de la Tabla 430-250
/// (3 HP a 230 V: 9.6 A; 5 HP: 15.2 A), interpolada para un motor marcado en amperes (430-6(a)(1));
/// en A/C, la placa: 125 % (440-32) y el mayor tamaño estándar que no pase de 175 % o 225 %
/// (440-22(a)), o la ampacidad mínima y la protección máxima (440-4(b)).
/// </summary>
public class TiposDeCargaTests
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

    private static CircuitoDelCuadro Con(CuadroDeCarga cuadro, int espacio, CategoriaDeCarga categoria, int polos, Action<CircuitoDelCuadro> placa)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = categoria;
        placa(c);
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        cuadro.Recalcular();
        return c;
    }

    private static CircuitoDelCuadro MotorEnAmperes(CuadroDeCarga cuadro, int espacio, decimal amperes, int polos) =>
        Con(cuadro, espacio, CategoriaDeCarga.Motor, polos, c => { c.CapturaMotor = CapturaDeMotor.Amperes; c.CorrientePlacaA = amperes; });

    private static CircuitoDelCuadro AireConPlaca(CuadroDeCarga cuadro, int espacio, decimal mca, decimal mocp, int polos) =>
        Con(cuadro, espacio, CategoriaDeCarga.AireAcondicionado, polos, c => { c.AmpacidadMinimaA = mca; c.ProteccionMaximaA = mocp; });

    private static CircuitoDelCuadro AireConNominal(CuadroDeCarga cuadro, int espacio, decimal nominal, int polos, decimal? seleccion = null, bool arranque = false) =>
        Con(cuadro, espacio, CategoriaDeCarga.AireAcondicionado, polos, c =>
        {
            c.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
            c.CorrientePlacaA = nominal;
            c.CorrienteSeleccionA = seleccion;
            c.ArranqueAl225 = arranque;
        });

    // ---- Los tipos -------------------------------------------------------------------------------

    [Fact]
    public void I74_SonSeisTipos_ConMotorYAireAcondicionadoPorSeparado()
    {
        Assert.Equal(
            [CategoriaDeCarga.Alumbrado, CategoriaDeCarga.Contactos, CategoriaDeCarga.Equipo,
             CategoriaDeCarga.Motor, CategoriaDeCarga.AireAcondicionado, CategoriaDeCarga.CalefaccionFija],
            CategoriasDeCarga.Todas);
        Assert.Equal("Motor", CategoriaDeCarga.Motor.Nombre());
        Assert.Equal("A/C y refrig.", CategoriaDeCarga.AireAcondicionado.Nombre());
        Assert.Equal("Aire acondicionado y refrigeración", CategoriaDeCarga.AireAcondicionado.NombreCompleto());
        Assert.True(CategoriaDeCarga.Motor.EsDeMotor());
        Assert.True(CategoriaDeCarga.AireAcondicionado.EsDeMotor());
        Assert.False(CategoriaDeCarga.Equipo.EsDeMotor());
    }

    [Fact]
    public void I74_LaUnidadYaNoDecideElArticulo_ElMismoMotorEnHpYEnAmperesDaLoMismo()
    {
        // El defecto: la misma bomba de 5 HP salía con 40 A en HP y con 20 A capturada en 15.2 A.
        var cuadro = Nuevo();
        var enHp = Con(cuadro, 1, CategoriaDeCarga.Motor, 3, c => c.Hp = 5m);
        var enAmperes = MotorEnAmperes(cuadro, 2, 15.2m, 3);

        Assert.Equal(40m, enHp.Resultado!.ProteccionA);
        Assert.Equal(40m, enAmperes.Resultado!.ProteccionA);
        Assert.Equal(enHp.Resultado.CalibreFase.Designacion, enAmperes.Resultado.CalibreFase.Designacion);
        Assert.Equal(5m, enAmperes.MotorEnAmperes!.Hp); // 15.2 A es un renglón de la tabla
        Assert.True(enAmperes.MotorEnAmperes.EsDeUnRenglon);
    }

    // ---- Motor en amperes: 430-6(a)(1) ---------------------------------------------------------

    [Fact]
    public void I74_UnMotorEnAmperesTomaLosHpDeLaTablaInterpolando()
    {
        var cuadro = Nuevo();
        var c = MotorEnAmperes(cuadro, 1, 12m, 3);

        // Tabla 430-250, columna de 230 V: 3 HP 9.6 A; 5 HP 15.2 A. 3 + (12 − 9.6) / (15.2 − 9.6) × 2 = 3.857 HP.
        var m = c.MotorEnAmperes!;
        Assert.Equal(3m, m.DesdeHp);
        Assert.Equal(5m, m.HastaHp);
        Assert.Equal(3.857m, m.Hp, 3);
        // Interpolado, ese motor tiene en la tabla justo esa corriente: es la FLC.
        Assert.Equal(12m, c.FlcA);
        var r = c.Resultado!;
        Assert.Equal(12m, r.CorrienteDisenoA);
        Assert.Equal(30m, r.ProteccionA);            // 250 % × 12 = 30 A — Tabla 430-52
        Assert.Equal(15m, r.Detalle!.CapacidadMinimaA); // 125 % — 430-22
        Assert.Equal("14", r.CalibreFase.Designacion);  // 15 A a 60 °C
        Assert.Contains(r.Citas, x => x.Referencia == "430-6(a)(1)");
        Assert.DoesNotContain(r.Citas, x => x.Referencia == "430-6(a)");
    }

    [Fact]
    public void I74_UnMotorEnAmperesMenorQueElMasChicoSeInterpolaDesdeCero()
    {
        var cuadro = Nuevo();
        var minimo = cuadro.FlcDe(Espacio(cuadro, 1), 1m / 6m); // 1/6 HP monofásico a 127 V
        var c = MotorEnAmperes(cuadro, 1, minimo / 2m, 1);

        Assert.Equal(0m, c.MotorEnAmperes!.DesdeHp);
        Assert.Equal(1m / 12m, c.MotorEnAmperes.Hp, 4);
        Assert.Equal(minimo / 2m, c.FlcA);
        Assert.NotNull(c.Resultado);
        Assert.Contains("se interpola desde 0 HP y 0 A", MotoresEnHp.Interpolacion(c.MotorEnAmperes, 1));
    }

    [Fact]
    public void I74_UnMotorEnAmperesMayorQueElMasGrandeNoCalculaYDiceHastaDonde()
    {
        var cuadro = Nuevo();
        var c = MotorEnAmperes(cuadro, 1, 500m, 1); // la 430-248 a 127 V llega a 10 HP

        Assert.Null(c.Resultado);
        Assert.NotNull(c.Error);
        Assert.Contains("pasa del más grande", c.Error);
        Assert.Contains("430-6(a)(1)", c.Error);
    }

    [Fact]
    public void I74_ElSelectorDeHpEnseñaLaCorrienteDeTabla()
    {
        var cuadro = Nuevo();
        var c = Con(cuadro, 1, CategoriaDeCarga.Motor, 3, _ => { });
        Assert.Equal(15.2m, cuadro.FlcDe(c, 5m));
        Assert.Equal(0m, cuadro.FlcDe(c, 250m)); // la 430-250 a 230 V no trae 250 HP
    }

    // ---- A/C y refrigeración: Art. 440 ---------------------------------------------------------

    [Fact]
    public void I74_UnAireConMcaYMocpUsaLaPlaca()
    {
        var cuadro = Nuevo();
        var c = AireConPlaca(cuadro, 1, mca: 18m, mocp: 30m, polos: 2);

        Assert.True(c.EsAireAcondicionado);
        Assert.Null(c.Error);
        var r = c.Resultado!;
        Assert.Equal(18m, r.CorrienteDisenoA);
        Assert.Equal(30m, r.ProteccionA);             // la máxima de placa — 440-4(b)
        Assert.Equal(18m, r.Detalle!.CapacidadMinimaA); // la MCA, sin otro 125 %
        Assert.Equal("12", r.CalibreFase.Designacion);  // 14 AWG da 15 A a 60 °C
        Assert.Contains(r.Citas, x => x.Referencia == "440-4(b)");
        Assert.Contains(r.Citas, x => x.Referencia == "240-4(g)");
        // Monofásico entre fases: 18 A × 220 V.
        Assert.Equal(18m * 220m, c.MotorVA);
    }

    [Fact]
    public void I74_UnaMocpQueNoEsTamanoEstandarBajaAlAnterior()
    {
        var cuadro = Nuevo();
        var c = AireConPlaca(cuadro, 1, mca: 18m, mocp: 28m, polos: 2);
        Assert.Equal(25m, c.Resultado!.ProteccionA); // «no debe exceder»: 25, no 30
    }

    [Fact]
    public void I74_SinTamanoEntreMcaYMocpNoCalculaYLoDice()
    {
        var cuadro = Nuevo();
        var c = AireConPlaca(cuadro, 1, mca: 17m, mocp: 16m, polos: 2);
        Assert.Null(c.Resultado);
        Assert.Contains("ningún tamaño estándar", c.Error);
    }

    [Fact]
    public void I74_SinMocpNoCalculaYLoDice()
    {
        var cuadro = Nuevo();
        var c = AireConPlaca(cuadro, 1, mca: 18m, mocp: 0m, polos: 2);
        Assert.True(c.TieneCarga);
        Assert.Contains("falta la protección máxima", c.Error);
    }

    [Fact]
    public void I74_ConCorrienteNominal_LaProteccionNoPasaDe175SinRedondearArriba()
    {
        var cuadro = Nuevo();
        var c = AireConNominal(cuadro, 1, 12.5m, polos: 2);

        var r = c.Resultado!;
        Assert.Equal(12.5m, r.CorrienteDisenoA);
        // 175 % × 12.5 = 21.875 A → 20 A: 440-22(a) dice «no exceda», sin el permiso de 430-52(c)(1).
        Assert.Equal(20m, r.ProteccionA);
        // 125 % × 12.5 = 15.63 A — 440-32: 14 AWG (15 A) no alcanza.
        Assert.Equal(15.625m, r.Detalle!.CapacidadMinimaA);
        Assert.Equal("12", r.CalibreFase.Designacion);
        Assert.Contains(r.Citas, x => x.Referencia == "440-22(a)");
        Assert.Contains(r.Citas, x => x.Referencia == "440-32");
    }

    [Fact]
    public void I74_SiNoArrancaAl175SubeA225()
    {
        var cuadro = Nuevo();
        var c = AireConNominal(cuadro, 1, 12.5m, polos: 2, arranque: true);
        Assert.Equal(25m, c.Resultado!.ProteccionA); // 225 % × 12.5 = 28.13 A → 25 A
    }

    [Fact]
    public void I74_LaCorrienteDeSeleccionMayorManda()
    {
        var cuadro = Nuevo();
        var c = AireConNominal(cuadro, 1, 12.5m, polos: 2, seleccion: 14m);

        Assert.Equal(14m, c.CorrienteDeMotorA);         // 440-6(a) Excepción 1
        Assert.Equal(20m, c.Resultado!.ProteccionA);     // 175 % × 14 = 24.5 A → 20 A
        Assert.Equal(17.5m, c.Resultado.Detalle!.CapacidadMinimaA);
    }

    [Fact]
    public void I74_NoSeExigeProteccionMenorA15A()
    {
        var cuadro = Nuevo();
        var c = AireConNominal(cuadro, 1, 5m, polos: 2); // 175 % × 5 = 8.75 A
        Assert.Equal(15m, c.Resultado!.ProteccionA);      // 440-22(a) Excepción
    }

    [Fact]
    public void I74_UnAireNoSeDesglosa()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Descripcion = "Cocina";
        c.AgregarAparato().CargaUnitaria = 500m;
        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        c.AmpacidadMinimaA = 10m;
        c.ProteccionMaximaA = 15m;
        cuadro.Recalcular();

        Assert.False(c.TieneDesglose);           // los aparatos se conservan, no cuentan
        Assert.Single(c.Aparatos);
        Assert.Equal(10m, c.Resultado!.CorrienteDisenoA);
    }

    // ---- El alimentador ------------------------------------------------------------------------

    [Fact]
    public void I74_MotoresYAireVanAlMismoGrupoDelAlimentador()
    {
        var cuadro = Nuevo();
        Con(cuadro, 1, CategoriaDeCarga.Motor, 3, c => c.Hp = 5m); // 15.2 A
        AireConPlaca(cuadro, 2, mca: 20m, mocp: 35m, polos: 3);    // 20 A, el mayor — 440-7

        var g = cuadro.Alimentador.Gobierna!;
        Assert.Equal(20m, g.Motores.MayorFlcA);
        // 440-33 y 430-24: 125 % × 20 + 15.2 = 40.2 A → 45 A.
        Assert.Equal(40.2m, cuadro.Alimentador.Resultado!.Detalle!.CapacidadMinimaA);
        Assert.Equal(45m, cuadro.Alimentador.Resultado.ProteccionA);
        Assert.Equal("Motores y A/C", cuadro.EtiquetaDeMotores);
        Assert.Equal("430-24, 440-33", cuadro.ReferenciaDeMotores);
    }

    [Fact]
    public void I74_ElResumenAlAlimentadorSumaElTotal()
    {
        var cuadro = Nuevo();
        Espacio(cuadro, 7).Continua = 1000m;
        Espacio(cuadro, 9).Categoria = CategoriaDeCarga.Equipo;
        Espacio(cuadro, 9).NoContinua = 500m;
        Con(cuadro, 1, CategoriaDeCarga.Motor, 1, c => c.Hp = 1m);
        AireConPlaca(cuadro, 2, mca: 10m, mocp: 15m, polos: 2);

        var r = cuadro.Resumen;
        Assert.Equal(r.CalculadaVA, r.ContinuaVA + r.NoContinuaVA + r.Minimo220_52VA + r.MotoresVA);
        Assert.Equal(1000m, r.ContinuaVA);
        Assert.Equal(500m, r.NoContinuaVA);
        Assert.Equal(cuadro.Circuitos[0].MotorVA + cuadro.Circuitos[1].MotorVA, r.MotoresVA);
        Assert.Equal(6, r.PorCategoria!.Count);
    }

    [Fact]
    public void I74_ElPrincipalMenorQueElAireAvisaConSuArticulo()
    {
        // 12.5 A: derivado de 25 A (225 %, no arranca al 175 %); principal de 125 % × 12.5 = 15.63 A → 20 A.
        var cuadro = Nuevo();
        AireConNominal(cuadro, 1, 12.5m, polos: 2, arranque: true);

        Assert.Equal(20m, cuadro.Alimentador.Resultado!.ProteccionA);
        var aviso = Assert.Single(cuadro.Alimentador.Avisos, a => a.Contains("equipo de A/C del circuito 1"));
        Assert.Contains("440-22(a)", aviso);
        Assert.Contains("430-62(a)", aviso);
    }

    // ---- Documentos ----------------------------------------------------------------------------

    [Fact]
    public void I74_LaMemoriaDelAireVaPorElArt440()
    {
        var cuadro = Nuevo();
        var c = AireConNominal(cuadro, 1, 12.5m, polos: 2);

        var hoja = MemoriaDeCalculo.DeCircuito(cuadro, c);
        Assert.Equal("440", hoja.Articulo);
        var secciones = MemoriaDeCalculo.Secciones(hoja);
        Assert.Contains(secciones[0].Renglones, r => r.Rotulo == "Equipo de A/C" && r.Valor.Contains("12.50 A") && r.Valor.Contains("440-6(a)"));
        Assert.DoesNotContain(secciones[0].Renglones, r => r.Rotulo == "Carga continua");
        Assert.Contains(secciones[2].Renglones, r => r.Rotulo == "Capacidad mínima del conductor — 440-32");
        Assert.Contains(secciones[2].Renglones, r => r.Rotulo == "Protección máxima — 440-22(a)" && r.Valor.Contains("175 %"));
        Assert.Contains(secciones[2].Notas, n => n.Contains("440-52"));
    }

    [Fact]
    public void I74_LaMemoriaDelMotorEnAmperesDiceComoSeInterpolo()
    {
        var cuadro = Nuevo();
        var c = MotorEnAmperes(cuadro, 1, 12m, 3);

        var secciones = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, c));
        Assert.Contains(secciones[0].Renglones, r => r.Rotulo == "Motor" && r.Valor.Contains("3 HP, 9.60 A; 5 HP, 15.20 A"));
        Assert.Contains(secciones[2].Renglones, r => r.Rotulo == "Corriente a plena carga (FLC) — 430-6(a)(1)");
    }

    [Fact]
    public void I74_ElDesgloseDelAireCitaSusArticulos()
    {
        var cuadro = Nuevo();
        var nominal = cuadro.Desglose(AireConNominal(cuadro, 1, 12.5m, polos: 2))!;
        Assert.Contains(nominal.Proteccion, x => x.Contains("175 %") && x.Contains("440-22(a)"));
        Assert.Contains(nominal.Conductor, x => x.Contains("440-32") && x.Contains("✔"));

        var placa = cuadro.Desglose(AireConPlaca(cuadro, 2, 18m, 30m, 2))!;
        Assert.Contains(placa.Proteccion, x => x.Contains("440-4(b)"));
        Assert.Contains(placa.Conductor, x => x.Contains("ampacidad mínima de placa") && x.Contains("✔"));
    }

    // ---- El archivo ----------------------------------------------------------------------------

    [Fact]
    public void I74_ElArchivoGuardaLaPlacaDeMotoresYAire()
    {
        var original = Nuevo();
        MotorEnAmperes(original, 1, 12m, 3);
        AireConNominal(original, 2, 12.5m, polos: 2, seleccion: 14m, arranque: true);
        AireConPlaca(original, 7, 18m, 30m, 2);

        var texto = ArchivoDelCuadro.Guardar(original, new DateTimeOffset(2026, 9, 27, 12, 0, 0, TimeSpan.FromHours(-6)));
        Assert.Contains("\"version\": 2", texto);
        Assert.Contains("\"categoria\": \"AireAcondicionado\"", texto);

        var abierto = ArchivoDelCuadro.Abrir(texto, Motor).Cuadro!;
        Assert.Equal(ArchivoDelCuadro.Huella(original), ArchivoDelCuadro.Huella(abierto));
        Assert.Equal(
            original.Circuitos.Select(c => (c.Espacio, c.Resultado?.ProteccionA, c.Resultado?.CalibreFase.Designacion)),
            abierto.Circuitos.Select(c => (c.Espacio, c.Resultado?.ProteccionA, c.Resultado?.CalibreFase.Designacion)));
        var aire = abierto.Circuitos[1];
        Assert.Equal(PlacaDeAireAcondicionado.CorrienteNominal, aire.PlacaAire);
        Assert.Equal(14m, aire.CorrienteSeleccionA);
        Assert.True(aire.ArranqueAl225);
    }

    [Fact]
    public void I74_UnArchivoDeFormato1_MotorConHpPasaAMotor_SinHpAAireConAviso()
    {
        const string formato1 = """
            {
              "formato": "power-node/cuadro-de-carga",
              "version": 1,
              "datos": {
                "fases": 3, "hilos": 4, "tensionFaseFaseV": 220, "numeroEspacios": 12,
                "factoresDeDemanda": { "Alumbrado": 1, "MotorOAireAcondicionado": 0.8 },
                "justificaciones": { "MotorOAireAcondicionado": [ "MotoresNoSimultaneos" ] }
              },
              "circuitos": [
                { "espacio": 1, "categoria": "MotorOAireAcondicionado", "hp": 5, "polos": 3 },
                { "espacio": 2, "categoria": "MotorOAireAcondicionado", "unidad": "Amperes", "noContinua": 10 },
                { "espacio": 4, "categoria": "MotorOAireAcondicionado" }
              ]
            }
            """;

        var apertura = ArchivoDelCuadro.Abrir(formato1, Motor);
        Assert.Null(apertura.Error);
        var cuadro = apertura.Cuadro!;

        var motor = cuadro.Circuitos[0];
        Assert.Equal(CategoriaDeCarga.Motor, motor.Categoria);
        Assert.Equal(40m, motor.Resultado!.ProteccionA);

        var aire = cuadro.Circuitos[1];
        Assert.Equal(CategoriaDeCarga.AireAcondicionado, aire.Categoria);
        Assert.Equal(PlacaDeAireAcondicionado.CorrienteNominal, aire.PlacaAire);
        Assert.Equal(10m, aire.CorrientePlacaA);
        Assert.Single(apertura.Avisos, a => a.Contains("circuito 2") && a.Contains("A/C y refrigeración") && a.Contains("10.00 A"));

        // Sin carga no hay nada que revisar: pasa a A/C sin aviso.
        Assert.Equal(CategoriaDeCarga.AireAcondicionado, cuadro.Circuitos[3].Categoria);
        Assert.DoesNotContain(apertura.Avisos, a => a.Contains("circuito 4"));

        // El factor y la justificación de Motor / A/C van a los dos tipos que salieron de él.
        Assert.Equal(0.8m, cuadro.Datos.FactorDemandaMotores);
        Assert.Equal(0.8m, cuadro.Datos.FactorDemandaAireAcondicionado);
        Assert.Contains(JustificacionFactorDemanda.MotoresNoSimultaneos, cuadro.Datos.Justificaciones[CategoriaDeCarga.AireAcondicionado]);
    }
}
