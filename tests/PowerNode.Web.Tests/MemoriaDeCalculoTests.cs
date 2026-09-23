using PowerNode.Web.Modelo;
using PowerNode.Web.Modelo.Memoria;

namespace PowerNode.Web.Tests;

/// <summary>
/// La memoria de cálculo: <b>las nueve secciones del Excel original, en su orden</b>, con las
/// fórmulas ya sustituidas. Es la misma plantilla que emite la versión de escritorio en Word.
/// </summary>
public class MemoriaDeCalculoTests
{
    private static readonly string Json = File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "datos", "tablas-nom.json"));

    private static CuadroDeCarga ConUnCircuito()
    {
        var cuadro = new CuadroDeCarga(new MotorNom(Json));
        cuadro.Datos.NumeroEspacios = 6;
        cuadro.Datos.Tablero = "TA-1";
        cuadro.Circuitos[0].Descripcion = "Alumbrado planta baja";
        cuadro.Circuitos[0].Continua = 720m;
        cuadro.Recalcular();
        return cuadro;
    }

    [Fact]
    public void LasNueveSeccionesSalenEnElOrdenDelExcel()
    {
        var cuadro = ConUnCircuito();
        var hoja = MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]);

        var titulos = MemoriaDeCalculo.Secciones(hoja).Select(s => s.Titulo).ToList();

        Assert.Equal(
        [
            "1. DATOS DEL SISTEMA",
            "2. CONSIDERACIONES",
            "3. SELECCIÓN DE LA PROTECCIÓN",
            "4. CÁLCULO POR CAPACIDAD",
            "5. CONDUCTOR DE FASE SELECCIONADO",
            "6. CÁLCULO DE CAÍDA DE TENSIÓN",
            "7. CAÍDA DE TENSIÓN EN EL TRAMO",
            "8. SELECCIÓN DEL CONDUCTOR DE PUESTA A TIERRA",
            "9. CONDUCTOR DE PUESTA A TIERRA SELECCIONADO",
        ], titulos);
    }

    [Fact]
    public void UnDerivadoCitaEl210YElAlimentadorEl215()
    {
        var cuadro = ConUnCircuito();

        var derivado = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]));
        var alimentador = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DelAlimentador(cuadro)!);

        Assert.Contains(derivado[2].Renglones, r => r.Rotulo.Contains("210-20(a)"));
        Assert.Contains(alimentador[2].Renglones, r => r.Rotulo.Contains("215-3"));
    }

    [Fact]
    public void LasFormulasVienenConSusNumerosSustituidos()
    {
        var cuadro = ConUnCircuito();
        var secciones = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]));

        // Cómo se llegó a la ampacidad, con los números: quien revisa tiene que poder recalcularla.
        // 720 VA continuos → 7.09 A de capacidad mínima → 15 A → 14 AWG: 25 A a 90 °C, topado a los
        // 15 A de la terminal de 60 °C.
        var capacidad = secciones[3];
        Assert.Contains(capacidad.Formulas, f => f.StartsWith("14 AWG/kcmil a 90 °C: 25 A × FT 1.00 × FA 1.00 = 25.00 A"));
        Assert.Contains(capacidad.Formulas, f => f.StartsWith("Tope de la terminal: 15 A a 60 °C"));
        Assert.Contains("Ampacidad utilizable: 15.00 A", capacidad.Formulas);
        Assert.DoesNotContain(capacidad.Formulas, f => f.Contains("Icm = In"));

        var caida = secciones[5];
        Assert.StartsWith("e = 2 × L × In ×", caida.Formulas[0]); // 1 polo: no lleva √3
        Assert.Contains("ohm/km", caida.Notas[0]);
    }

    [Fact]
    public void UnaHojaNoImprimeCerosComoSiFueranResultados()
    {
        var cuadro = ConUnCircuito();
        var secciones = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]));

        Assert.DoesNotContain(
            secciones.SelectMany(s => s.Renglones),
            r => r.Valor is "0 A" or "0.00 A");
    }

    [Fact]
    public void ElDocumentoTraeUnaHojaPorCircuitoCalculadoYLaDelAlimentadorAlFinal()
    {
        var cuadro = ConUnCircuito();
        cuadro.Circuitos[1].NoContinua = 1500m;
        cuadro.Recalcular();

        var hojas = MemoriaDeCalculo.Hojas(cuadro);

        Assert.Equal(3, hojas.Count);
        Assert.StartsWith("Circuito 1 — Alumbrado planta baja", hojas[0].Sujeto);
        Assert.StartsWith("Circuito 2 —", hojas[1].Sujeto);
        Assert.Equal("Alimentador general del tablero TA-1", hojas[2].Sujeto);
        Assert.Equal("215", hojas[2].Articulo);
    }

    [Fact]
    public void LaHojaDeUnCircuitoDeUnPoloUsaLaTensionFaseNeutro()
    {
        var cuadro = ConUnCircuito();

        var hoja = MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]);

        Assert.Equal(cuadro.Datos.TensionFaseNeutroV, hoja.TensionV);
        Assert.Equal(1, hoja.NumeroFases);
    }

    [Fact]
    public void LaMemoriaDelAlimentadorDiceQueFaseGobierna()
    {
        // M-02, con el caso de la prueba del 2026-09-22: la air fryer en la fase C.
        var cuadro = new CuadroDeCarga(new MotorNom(Json));
        cuadro.Datos.NumeroEspacios = 6;
        cuadro.Circuitos[0].Continua = 750m;
        cuadro.Circuitos[2].Continua = 1500m;
        cuadro.Circuitos[4].Continua = 1550m;
        cuadro.Recalcular();

        var seccion3 = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DelAlimentador(cuadro)!)[2];
        var gobierna = Assert.Single(seccion3.Renglones, r => r.Rotulo == "Fase que gobierna");

        Assert.StartsWith("Fase C, la más cargada: 125 % × 12.20 A", gobierna.Valor);
    }

    [Fact]
    public void UnDerivadoNoLlevaFaseQueGobierna()
    {
        var cuadro = ConUnCircuito();
        var seccion3 = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]))[2];

        Assert.DoesNotContain(seccion3.Renglones, r => r.Rotulo == "Fase que gobierna");
    }

    [Fact]
    public void LaMemoriaDiceConQueAislamientoSeCalculo()
    {
        var cuadro = ConUnCircuito();
        cuadro.Datos.TipoAislamiento = "THW-LS";
        cuadro.Recalcular();

        var seccion2 = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]))[1];
        var renglon = Assert.Single(seccion2.Renglones, r => r.Rotulo.StartsWith("Aislamiento"));
        Assert.Equal("THW-LS · lugar seco", renglon.Valor);
    }

    [Fact]
    public void LaMemoriaDiceDeQueListaSalioElInterruptor()
    {
        var cuadro = ConUnCircuito();
        var seccion3 = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]))[2];

        var renglon = Assert.Single(seccion3.Renglones, r => r.Rotulo == "Tamaños de interruptor");
        Assert.StartsWith("Centro de carga (NEMA): de la lista de 240-6(a) se omiten 16, 32 y 63 A", renglon.Valor);
    }

    [Fact]
    public void LaSeccion4CuadraConElConductorElegido()
    {
        // 32 A continuos, 6 agrupados. Antes la memoria decía «Icm = 50 A» y luego elegía un 8 AWG
        // de 40 A. Los 50 A eran de la columna de 90 °C; ahora se ven las dos columnas.
        var cuadro = new CuadroDeCarga(new MotorNom(Json));
        cuadro.Datos.NumeroEspacios = 6;
        cuadro.Datos.ConductoresAgrupados = 6;
        var c = cuadro.Circuitos[0];
        c.Tipo = PowerNode.DesignSuite.Calculo.Unidades.TipoCarga.Equipo;
        c.Unidad = PowerNode.DesignSuite.Calculo.Casos.UnidadConsumo.Amperes;
        c.Continua = 32m;
        c.LongitudM = 5m;
        cuadro.Recalcular();

        var formulas = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, c))[3].Formulas;

        Assert.Contains(formulas, f => f.StartsWith("8 AWG/kcmil a 90 °C: 55 A × FT 1.00 × FA 0.80 = 44.00 A"));
        Assert.Contains(formulas, f => f.StartsWith("Tope de la terminal: 40 A a 60 °C"));
        Assert.Contains("Ampacidad utilizable: 40.00 A", formulas);
        Assert.Contains("Antes de factores: 40.00 A a 60 °C ≥ capacidad mínima 40.00 A ✔ — 210-19(a)(1)", formulas);
        Assert.Contains("Con factores: 40.00 A ≥ carga 32.00 A ✔ — 210-19(a)(1)", formulas);
    }

    [Fact]
    public void ElDesgloseDeLaProteccionDiceDeDondeSaleCadaNumero()
    {
        var cuadro = ConUnCircuito();
        var proteccion = cuadro.Desglose(cuadro.Circuitos[0])!.Proteccion;

        Assert.Equal("In = 5.67 A (continua) + 0.00 A (no continua) = 5.67 A", proteccion[0]);
        Assert.Equal("Capacidad mínima = 125 % × 5.67 A + 0.00 A = 7.09 A — 210-20(a)", proteccion[1]);
        Assert.StartsWith("Protección: 15 A, el primer tamaño que alcanza", proteccion[2]);
    }
}
