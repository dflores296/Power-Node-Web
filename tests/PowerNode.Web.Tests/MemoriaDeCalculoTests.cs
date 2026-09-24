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
    public void R01_LaSeccion7CitaLaNotaDeSuArticulo_YElDerivadoDaLaCaidaCombinada()
    {
        var cuadro = ConUnCircuito();

        var derivado = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]))[6];
        var alimentador = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DelAlimentador(cuadro)!)[6];

        Assert.StartsWith("210-19(a)(1), NOTA 4", derivado.Renglones.Single(r => r.Rotulo == "Referencia").Valor);
        Assert.StartsWith("215-2(a)(4), NOTA 2", alimentador.Renglones.Single(r => r.Rotulo == "Referencia").Valor);
        Assert.Matches(@"^Alimentador \d+\.\d\d % \+ circuito \d+\.\d\d % = \d+\.\d\d % ≤ 5 %$",
            derivado.Renglones.Single(r => r.Rotulo == "Caída combinada").Valor);
        Assert.DoesNotContain(alimentador.Renglones, r => r.Rotulo == "Caída combinada");
    }

    [Fact]
    public void Vivienda_ElAlimentadorDiceDeDondeSalenLos1500VA()
    {
        var cuadro = new CuadroDeCarga(new MotorNom(Json));
        cuadro.Datos.NumeroEspacios = 6;
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar; // 210-11(c) y 220-52 son de vivienda — I-46
        var cocina = cuadro.Circuitos[0];
        cocina.Tipo = PowerNode.DesignSuite.Calculo.Unidades.TipoCarga.Contactos;
        cocina.Uso = UsoDeContactos.AparatosPequenos;
        cocina.NoContinua = 500m;
        cuadro.Recalcular();

        var seccion1 = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DelAlimentador(cuadro)!)[0];
        Assert.Equal("500 VA", seccion1.Renglones.Single(r => r.Rotulo == "Carga total instalada").Valor);
        Assert.StartsWith("1,000 VA — aparatos pequeños y lavadora", seccion1.Renglones.Single(r => r.Rotulo == "Mínimo 220-52").Valor);
        Assert.Equal("1,500 VA", seccion1.Renglones.Single(r => r.Rotulo == "Carga calculada").Valor);

        var derivado = MemoriaDeCalculo.DeCircuito(cuadro, cocina);
        Assert.Contains("Contactos · Aparatos pequeños (cocina)", derivado.Sujeto);
        Assert.DoesNotContain(MemoriaDeCalculo.Secciones(derivado)[0].Renglones, r => r.Rotulo == "Mínimo 220-52");
    }

    private static string? NotaDeNeutro(HojaDeMemoria hoja) =>
        MemoriaDeCalculo.Secciones(hoja)[1].Renglones.SingleOrDefault(r => r.Rotulo == "Neutro — 310-15(b)(5)(2)")?.Valor;

    private static CuadroDeCarga Sistema(int fases, int hilos, decimal tension)
    {
        var cuadro = new CuadroDeCarga(new MotorNom(Json));
        cuadro.Datos.NumeroEspacios = 6;
        cuadro.Datos.Fases = fases;
        cuadro.Datos.Hilos = hilos;
        cuadro.Datos.TensionFaseFaseV = tension;
        cuadro.Circuitos[0].NoContinua = 1000m;
        cuadro.Recalcular();
        return cuadro;
    }

    [Fact]
    public void R09_En2F3HElAlimentadorCitaElNeutroPortadorY220_61c1()
    {
        var nota = NotaDeNeutro(MemoriaDeCalculo.DelAlimentador(Sistema(2, 3, 220m))!);

        Assert.NotNull(nota);
        Assert.StartsWith("Portador de corriente: en 2 fases + neutro de estrella", nota);
        Assert.EndsWith("no se reduce — 220-61(c)(1).", nota);
    }

    [Theory]
    [InlineData(3, 4, 220)] // 3F-4H: el alimentador es de 3 fases
    [InlineData(1, 3, 240)] // 1F-3H 120/240: el neutro lleva solo el desbalance — (b)(5)(1)
    [InlineData(1, 2, 127)] // 1F-2H
    public void R09_FueraDe2FasesMasNeutroDeEstrella_NoSeCita(int fases, int hilos, decimal tension) =>
        Assert.Null(NotaDeNeutro(MemoriaDeCalculo.DelAlimentador(Sistema(fases, hilos, tension))!));

    [Fact]
    public void R09_UnDerivadoDe2PolosEn3F4HTambienLoCita_SinEl220_61()
    {
        var cuadro = Sistema(3, 4, 220m);
        Assert.Null(cuadro.CambiarPolos(cuadro.Circuitos[0], 2));

        // I-41: un bipolar lleva neutro solo con «+N»; sin él (carga F-F) no hay nota.
        Assert.Null(NotaDeNeutro(MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0])));
        cuadro.Circuitos[0].ConNeutro = true;
        cuadro.Recalcular();

        var nota = NotaDeNeutro(MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]));

        Assert.NotNull(nota);
        Assert.DoesNotContain("220-61", nota);
        Assert.Null(NotaDeNeutro(MemoriaDeCalculo.DelAlimentador(cuadro)!)); // el alimentador es de 3 fases
    }

    [Fact]
    public void R02_LaSeccion6DelAlimentadorEsFaseporFaseConElNeutro()
    {
        var cuadro = new CuadroDeCarga(new MotorNom(Json));
        cuadro.Datos.NumeroEspacios = 6;
        foreach (var (i, va) in new[] { (0, 750m), (2, 1500m), (4, 1550m) }) // fases A, B, C
            cuadro.Circuitos[i].Continua = va;
        cuadro.Datos.LongitudAlimentadorM = 80m;
        cuadro.Datos.CaidaMaxAlimentadorPct = 10m;
        cuadro.Recalcular();

        var secciones = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DelAlimentador(cuadro)!);
        var seccion6 = secciones[5];
        Assert.StartsWith("e_f = Re[ Z × (I_f + I_N) × conj(û_f) ]", seccion6.Formulas[0]);
        Assert.Contains(seccion6.Formulas, f => f.StartsWith("I_N = 6.11 A"));
        Assert.Contains(seccion6.Formulas, f => f.StartsWith("Fase C: I = 12.20 A") && f.EndsWith("(6.77 %)"));
        Assert.Contains("Manda la fase C.", seccion6.Notas[1]);
        Assert.EndsWith("— fase C", secciones[6].Renglones.Single(r => r.Rotulo == "Caída de tensión").Valor);

        // El derivado sigue con su fórmula de ida y vuelta.
        var derivado = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, cuadro.Circuitos[0]))[5];
        Assert.StartsWith("e = 2 × L × In", derivado.Formulas[0]);
    }

    [Fact]
    public void R12_LaMemoriaDelAlimentadorImprimeLosFactoresYSuJustificacion()
    {
        var cuadro = ConUnCircuito(); // un circuito de alumbrado
        cuadro.Datos.Inmueble = TipoDeInmueble.ViviendaUnifamiliar; // la Tabla 220-42 reduce en vivienda
        const string Rotulo = "F.D. alumbrado — 220-40";
        Assert.DoesNotContain(MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DelAlimentador(cuadro)!)[0].Renglones,
            r => r.Rotulo.StartsWith("F.D."));

        cuadro.Datos.FactorDemandaAlumbrado = 0.9m;
        cuadro.Recalcular();
        string Renglon() => MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DelAlimentador(cuadro)!)[0].Renglones
            .Single(r => r.Rotulo == Rotulo).Valor;
        Assert.Equal("0.90 · SIN JUSTIFICACIÓN", Renglon());

        cuadro.Datos.Justificaciones[CategoriaDeCarga.Alumbrado].Add(JustificacionFactorDemanda.AlumbradoGeneral);
        Assert.Equal("0.90 · Tabla 220-42 — alumbrado general (unidades de vivienda)", Renglon());
    }

    [Fact]
    public void I35_LaMemoriaDelCircuitoListaSusAparatos()
    {
        var cuadro = new CuadroDeCarga(new MotorNom(Json));
        cuadro.Datos.NumeroEspacios = 6;
        var c = cuadro.Circuitos[0];
        var estufa = c.AgregarAparato();
        estufa.Descripcion = "Estufa";
        estufa.Unidad = PowerNode.DesignSuite.Calculo.Casos.UnidadConsumo.Watts;
        estufa.CargaUnitaria = 900m;
        estufa.FactorPotencia = 1m;
        cuadro.Recalcular();

        var seccion1 = MemoriaDeCalculo.Secciones(MemoriaDeCalculo.DeCircuito(cuadro, c))[0];
        Assert.Equal("1 × 900 W = 900 VA · no continua · F.P. 1.00",
            seccion1.Renglones.Single(r => r.Rotulo == "Aparato 1: Estufa").Valor);
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
        var c = cuadro.Circuitos[0];
        c.Tipo = PowerNode.DesignSuite.Calculo.Unidades.TipoCarga.Equipo;
        c.Unidad = PowerNode.DesignSuite.Calculo.Casos.UnidadConsumo.Amperes;
        c.Continua = 32m;
        c.LongitudM = 5m;
        Agrupar.EnTubo(cuadro, c, 6);

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
        Assert.StartsWith("Protección: 15 A, primer tamaño ≥ capacidad mínima", proteccion[2]);
    }
}
