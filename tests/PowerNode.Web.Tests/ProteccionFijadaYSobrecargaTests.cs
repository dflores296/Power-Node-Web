using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Archivo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// <b>La protección calculada o fijada (I-182) y la sobrecarga dicha según el equipo (I-183)</b> — CONFIRMADAS
/// · David · 2026-10-03, decisión <c>proteccion-de-motores-por-rango.md</c>. La celda trae solo los valores del
/// rango: llega con el calculado, otro valor queda fijado, y el calculado lo regresa. Un fijado se borra si
/// cambia su rango (otro equipo, la Excepción 2, otra serie), no si cambia el conductor. La protección contra
/// sobrecarga no se pregunta: la NOM la exige en todo circuito de motor, y se dice —«OL»—. Cobre, THHN, seco,
/// 30 °C, PVC, 220/127 V.
/// </summary>
public class ProteccionFijadaYSobrecargaTests
{
    private static readonly MotorNom Motor = new(File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json")));

    private static CuadroDeCarga Nuevo()
    {
        var cuadro = new CuadroDeCarga(Motor);
        cuadro.Datos.NumeroEspacios = 12;
        cuadro.Datos.Fases = 3;
        cuadro.Datos.Hilos = 4;
        cuadro.Datos.TensionFaseFaseV = 220m;
        cuadro.Recalcular();
        return cuadro;
    }

    private static CircuitoDelCuadro Espacio(CuadroDeCarga cuadro, int espacio) =>
        cuadro.Circuitos.Single(x => x.Espacio == espacio);

    private static CircuitoDelCuadro ConMotor(CuadroDeCarga cuadro, int espacio, decimal hp, int polos = 1)
    {
        var c = Espacio(cuadro, espacio);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = hp;
        if (polos > 1)
            Assert.Null(cuadro.CambiarPolos(c, polos));
        cuadro.Recalcular();
        return c;
    }

    /// <summary>La bomba de cisterna de David: ½ HP, 1 polo, 127 V. Rango 15, 20 y 25 A; calculada 15 A sobre 14 AWG.</summary>
    private static (CuadroDeCarga Cuadro, CircuitoDelCuadro Bomba) LaBomba()
    {
        var cuadro = Nuevo();
        return (cuadro, ConMotor(cuadro, 1, 0.5m));
    }

    // ---- I-182: la lista, calculada o fijada ---------------------------------------------------------

    [Fact]
    public void I182_LaListaDeLaBombaEsElRangoYLlegaConLaCalculada()
    {
        var (cuadro, bomba) = LaBomba();

        Assert.Equal([15m, 20m, 25m], bomba.Resultado!.Rango!.Valores);
        Assert.Equal(CriterioDeProteccion.Automatico, bomba.CriterioProteccion);
        Assert.Equal(15m, bomba.Resultado.ProteccionA);
        Assert.False(CuadroDeCarga.ProteccionFijada(bomba));
        Assert.Equal(15m, cuadro.ProteccionCalculada(bomba));
    }

    [Fact]
    public void I182_OtroValorLaFijaYElCalculadoLaRegresa()
    {
        var (cuadro, bomba) = LaBomba();

        cuadro.FijarProteccion(bomba, 20m);
        Assert.Equal(CriterioDeProteccion.Manual, bomba.CriterioProteccion);
        Assert.Equal(20m, bomba.Resultado!.ProteccionA);
        Assert.True(CuadroDeCarga.ProteccionFijada(bomba));
        Assert.NotNull(bomba.HuellaDelFijado);
        Assert.Equal(15m, cuadro.ProteccionCalculada(bomba));

        // Escoger el que da el cálculo la regresa al cálculo: sin botón aparte (David).
        cuadro.FijarProteccion(bomba, 15m);
        Assert.Equal(CriterioDeProteccion.Automatico, bomba.CriterioProteccion);
        Assert.Null(bomba.ProteccionElegidaA);
        Assert.Null(bomba.HuellaDelFijado);
        Assert.Equal(15m, bomba.Resultado!.ProteccionA);
    }

    [Fact]
    public void I182_UnValorQueNoEstaEnLaListaNoLaFija()
    {
        var (cuadro, bomba) = LaBomba();

        cuadro.FijarProteccion(bomba, 30m);

        Assert.Equal(CriterioDeProteccion.Automatico, bomba.CriterioProteccion);
        Assert.Equal(15m, bomba.Resultado!.ProteccionA);
    }

    [Fact]
    public void I182_LaAyudaDiceElRangoYLaCalculada_SinAutoCondNiMax()
    {
        var (cuadro, bomba) = LaBomba();

        var ayuda = cuadro.AyudaDeLaProteccion(bomba)!;

        Assert.Contains("Se permite de 15 a 25 A: 430-52(c)(1) solo pone el techo.", ayuda);
        Assert.Contains("Calculada: 15 A, el mayor que protege a 14 AWG (15 A) — 240-4; criterio para motores de 1 HP o menos.", ayuda);
        Assert.Contains("25 A es el máximo de la Tabla 430-52.", ayuda);
        Assert.Contains("si dispara, subir hasta 25 A o declarar la Excepción 2 — 430-52(b).", ayuda);
        Assert.DoesNotContain(". —", ayuda);
        Assert.EndsWith("Escoger otro valor para fijarlo.", ayuda);
        Assert.DoesNotContain("auto", ayuda);
        Assert.DoesNotContain("cond.", ayuda);
        Assert.DoesNotContain("máx.", ayuda);

        cuadro.FijarProteccion(bomba, 20m);
        var fijada = cuadro.AyudaDeLaProteccion(bomba)!;
        Assert.StartsWith("Fijada: 20 A. El cálculo da 15 A; escogerlo en la lista lo regresa al cálculo.", fijada);
        Assert.DoesNotContain("Escoger otro valor", fijada);
    }

    [Fact]
    public void I182_CambiarElMotorBorraLaFijadaYLoAvisaUnaVez()
    {
        // ½ HP con 20 A fijados → 1 ½ HP (18 A, rango 25 a 45 A): la fijada era para otro rango.
        var (cuadro, bomba) = LaBomba();
        cuadro.FijarProteccion(bomba, 20m);
        Assert.Empty(cuadro.TomarProteccionesQueRegresaron());

        bomba.Hp = 1.5m;
        cuadro.Recalcular();

        Assert.Equal(CriterioDeProteccion.Automatico, bomba.CriterioProteccion);
        Assert.Null(bomba.ProteccionElegidaA);
        Assert.Equal(45m, bomba.Resultado!.ProteccionA);
        var regresaron = cuadro.TomarProteccionesQueRegresaron();
        var una = Assert.Single(regresaron);
        Assert.Equal(new ProteccionQueRegreso(1, 20m, 25m, 45m, 45m), una);
        Assert.Equal("Circuito 1: cambió el rango de la protección (25 a 45 A); la fijada, 20 A, regresó al calculado, 45 A.",
            ProteccionQueRegreso.Aviso(regresaron));
        // Tomarlas las borra: se avisa una vez.
        Assert.Empty(cuadro.TomarProteccionesQueRegresaron());
    }

    [Fact]
    public void I182_UnMaximoFijadoNoSeQuedaComoElMinimoDeOtroMotor()
    {
        // 25 A era el máximo de ½ HP; en 1 ½ HP sería el mínimo. Mismo número, otro significado: se borra.
        var (cuadro, bomba) = LaBomba();
        cuadro.FijarProteccion(bomba, 25m);

        bomba.Hp = 1.5m;
        cuadro.Recalcular();

        Assert.Equal(CriterioDeProteccion.Automatico, bomba.CriterioProteccion);
        Assert.Equal(45m, bomba.Resultado!.ProteccionA);
        Assert.Single(cuadro.TomarProteccionesQueRegresaron());
    }

    [Fact]
    public void I182_LaLongitudMueveElConductorNoElRango_LaFijadaSeQueda()
    {
        var (cuadro, bomba) = LaBomba();
        cuadro.FijarProteccion(bomba, 20m);

        bomba.LongitudM = 60m;
        cuadro.Recalcular();

        Assert.Equal(CriterioDeProteccion.Manual, bomba.CriterioProteccion);
        Assert.Equal(20m, bomba.Resultado!.ProteccionA);
        Assert.Empty(cuadro.TomarProteccionesQueRegresaron());
    }

    [Fact]
    public void I182_LaExcepcion2YOtraSerieBorranLaFijada()
    {
        var (cuadro, bomba) = LaBomba();
        cuadro.FijarProteccion(bomba, 20m);
        bomba.NoArrancaConLaTabla = true;
        cuadro.Recalcular();
        Assert.Equal(CriterioDeProteccion.Automatico, bomba.CriterioProteccion);
        Assert.Single(cuadro.TomarProteccionesQueRegresaron());

        bomba.NoArrancaConLaTabla = false;
        cuadro.Recalcular();
        cuadro.FijarProteccion(bomba, 20m);
        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        cuadro.Recalcular();
        Assert.Equal(CriterioDeProteccion.Automatico, bomba.CriterioProteccion);
        Assert.Single(cuadro.TomarProteccionesQueRegresaron());
    }

    [Fact]
    public void I182_VariasQueRegresanSeAvisanJuntas()
    {
        var cuadro = Nuevo();
        var uno = ConMotor(cuadro, 1, 0.5m);
        var otro = ConMotor(cuadro, 3, 0.5m);
        cuadro.FijarProteccion(uno, 20m);
        cuadro.FijarProteccion(otro, 20m);

        cuadro.Datos.SerieInterruptores = SerieDeInterruptores.RielDinIec;
        cuadro.Recalcular();

        var regresaron = cuadro.TomarProteccionesQueRegresaron();
        Assert.Equal([1, 3], regresaron.Select(x => x.Espacio));
        Assert.Equal("Circuitos 1 y 3: cambió el rango de la protección; las fijadas regresaron al calculado.", ProteccionQueRegreso.Aviso(regresaron));
    }

    [Fact]
    public void I182_UnMaxDeFormato12IgualAlCalculadoAbreCalculado()
    {
        // 5 HP trifásico: la calculada ya es el máximo (40 A). Un «máx.» guardado abre calculado, no fijado.
        const string texto = """
            {
              "formato": "power-node/cuadro-de-carga",
              "version": 12,
              "datos": { "fases": 3, "hilos": 4, "tensionFaseFaseV": 220, "numeroEspacios": 12 },
              "circuitos": [
                { "espacio": 1, "categoria": "Motor", "hp": 5, "polos": 3, "criterioProteccion": "Maximo430_52" }
              ]
            }
            """;

        var apertura = ArchivoDelCuadro.Abrir(texto, Motor);

        Assert.Empty(apertura.Avisos);
        var c = Espacio(apertura.Cuadro!, 1);
        Assert.Equal(CriterioDeProteccion.Automatico, c.CriterioProteccion);
        Assert.Equal(40m, c.Resultado!.ProteccionA);
    }

    // ---- I-183: la sobrecarga, dicha según el equipo -------------------------------------------------

    [Fact]
    public void I183_UnMotorDeMasDe1HpPide430_32a()
    {
        var cuadro = Nuevo();
        var c = ConMotor(cuadro, 1, 5m, 3);

        Assert.Equal("430-32(a)", c.Sobrecarga!.Referencia);
        Assert.True(c.Sobrecarga.Aparte);
        Assert.StartsWith("Requerida aparte del interruptor: relevador en el arrancador ajustado a no más de 125 %", c.Sobrecarga.Texto);
        Assert.Contains("«Protegido térmicamente»", c.Sobrecarga.Texto);
    }

    [Fact]
    public void I183_LaBombaDe1HpOMenosPide430_32b_YVerificarLaPlaca()
    {
        var (_, bomba) = LaBomba();

        Assert.Equal("430-32(b)", bomba.Sobrecarga!.Referencia);
        Assert.True(bomba.Sobrecarga.Aparte);
        Assert.EndsWith("verificarlo en la placa", bomba.Sobrecarga.Texto);
        Assert.StartsWith("Protección contra sobrecarga — 430-32(b): Requerida aparte del interruptor", bomba.Sobrecarga.Titulo);
    }

    [Fact]
    public void I183_UnMotorDeServicioNoContinuoLaPuedeDarElInterruptor_SinOL()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.Hp = 1m;
        c.Servicio = ServicioDeMotor.Intermitente;
        c.CorrientePlacaServicioA = 14m;
        cuadro.Recalcular();

        Assert.Null(c.Error);
        Assert.Equal("430-33", c.Sobrecarga!.Referencia);
        Assert.False(c.Sobrecarga.Aparte);
    }

    [Fact]
    public void I183_ElVariadorLaDaSiEstaMarcado_YSobretemperatura()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        Assert.Null(cuadro.CambiarPolos(c, 3));
        c.CapturaMotor = CapturaDeMotor.Variador;
        c.CorrienteEntradaVariadorA = 20m;
        c.ProteccionMaximaVariadorA = 40m;
        cuadro.ConSuTension(c);
        cuadro.Recalcular();

        Assert.Equal("430-124(a), 430-126", c.Sobrecarga!.Referencia);
        Assert.True(c.Sobrecarga.Aparte);
        Assert.Contains(c.Resultado!.Citas, x => x.Referencia == "240-4(g)" && x.Descripcion.Contains("por 430-120 a la Parte D"));
    }

    [Fact]
    public void I183_ElAireLaTraeDeFabrica_ElDeHabitacionNo()
    {
        var cuadro = Nuevo();
        var aire = Espacio(cuadro, 1);
        aire.Categoria = CategoriaDeCarga.AireAcondicionado;
        Assert.Null(cuadro.CambiarPolos(aire, 2));
        aire.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
        aire.CorrientePlacaA = 20m;
        var cuarto = Espacio(cuadro, 5);
        cuarto.Categoria = CategoriaDeCarga.AireAcondicionado;
        cuarto.PlacaAire = PlacaDeAireAcondicionado.Habitacion;
        cuarto.CorrientePlacaA = 10m;
        cuadro.ConSuTension(aire);
        cuadro.ConSuTension(cuarto);
        cuadro.Recalcular();

        Assert.Equal("440-52", aire.Sobrecarga!.Referencia);
        Assert.True(aire.Sobrecarga.Aparte);
        Assert.StartsWith("De fábrica", aire.Sobrecarga.Texto);
        Assert.Null(cuarto.Sobrecarga);
    }

    [Fact]
    public void I183_UnGrupoDeMotores_CadaUnoConLaSuya()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Motor;
        c.LongitudM = 10m;
        c.PasarAGrupo();
        foreach (var hp in new[] { 0.5m, 0.5m })
        {
            var m = c.AgregarMotor();
            m.Hp = hp;
        }
        cuadro.Recalcular();

        Assert.Null(c.Error);
        Assert.Equal("430-53", c.Sobrecarga!.Referencia);
        Assert.True(c.Sobrecarga.Aparte);
        Assert.Contains("cada motor, la de 430-32", c.Sobrecarga.Texto);
    }

    [Fact]
    public void I183_UnaCargaQueNoEsMotorNoPideSobrecarga()
    {
        var cuadro = Nuevo();
        var c = Espacio(cuadro, 1);
        c.Categoria = CategoriaDeCarga.Alumbrado;
        c.NoContinua = 900m;
        cuadro.Recalcular();

        Assert.NotNull(c.Resultado);
        Assert.Null(c.Sobrecarga);
    }

    [Fact]
    public void I183_ElDesgloseYLaMemoriaLaDicen_YLaCitaYaNoDiceCriterio()
    {
        var (cuadro, bomba) = LaBomba();

        Assert.Contains(cuadro.Desglose(bomba)!.Proteccion, l => l.StartsWith("Sobrecarga: Requerida aparte del interruptor") && l.Contains("430-32(b)"));
        Assert.Contains(MemoriaDeCalculo.DeCircuito(cuadro, bomba).Equipo!.Notas,
            n => n.StartsWith("Protección contra sobrecarga — 430-32(b): Requerida aparte del interruptor"));
        Assert.DoesNotContain(bomba.Resultado!.Citas, x => x.Descripcion.Contains("Criterio «"));

        // Fijada arriba de la ampacidad: 240-4(g), por la sobrecarga que exige 430-32.
        cuadro.FijarProteccion(bomba, 25m);
        Assert.Contains(bomba.Resultado!.Citas, x => x.Referencia == "240-4(g)" && x.Descripcion.Contains("la protección que exige 430-32"));
        Assert.Contains(bomba.Resultado.Citas, x => x.Descripcion.StartsWith("Fijada por el proyectista: 25 A, dentro del rango"));
    }
}
