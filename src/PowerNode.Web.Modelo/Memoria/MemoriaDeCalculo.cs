using PowerNode.DesignSuite.Calculo.Magnitudes;
using PowerNode.DesignSuite.Calculo.Tableros;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo.Memoria;

/// <summary>
/// Las <b>nueve secciones</b> de la memoria de cálculo, en el orden del Excel original (hoja
/// «Memoria de Cálculo», levantada en <c>docs/referencia/cuadro-de-carga.md §4</c> del repo de
/// escritorio). Es la misma plantilla que emite la versión de escritorio en Word
/// (<c>Exportacion.Word.SeccionesDeMemoria</c>), portada aquí a datos: los mismos títulos, las
/// mismas fórmulas y las mismas notas, sin OpenXml de por medio para que las pinte el navegador.
///
/// <para>
/// <b>La diferencia con el Excel es de dónde salen las referencias.</b> Ahí estaban escritas a mano
/// en la plantilla —por eso decía «tabla 310-15(b)( )» con el paréntesis vacío, a rellenar—, y aquí
/// <b>las produce el cálculo</b>: el motor sabe cuál de las tablas 310-15(b) usó, a qué temperatura
/// quedaron las terminales y con qué factores corrigió.
/// </para>
/// </summary>
public static class MemoriaDeCalculo
{
    /// <summary>Las hojas del documento: una por circuito con carga, y la del alimentador al final.</summary>
    public static IReadOnlyList<HojaDeMemoria> Hojas(CuadroDeCarga cuadro)
    {
        var hojas = cuadro.Circuitos
            .Where(c => c.Resultado is not null)
            .Select(c => DeCircuito(cuadro, c))
            .ToList();

        if (DelAlimentador(cuadro) is { } alimentador)
            hojas.Add(alimentador);

        return hojas;
    }

    public static HojaDeMemoria DeCircuito(CuadroDeCarga cuadro, CircuitoDelCuadro circuito)
    {
        var r = circuito.Resultado
            ?? throw new InvalidOperationException($"El circuito {circuito.Espacio} no tiene cálculo.");
        var datos = cuadro.Datos;

        var nombre = string.IsNullOrWhiteSpace(circuito.Descripcion)
            ? Etiqueta(circuito)
            : circuito.Descripcion.Trim();

        return new HojaDeMemoria(
            Sujeto: $"Circuito {circuito.Espacio} — {nombre}  ·  fase {circuito.Fases}",
            Articulo: "210",
            CargaContinuaVa: circuito.ContinuaVA,
            CargaNoContinuaVa: circuito.NoContinuaVA,
            // La tensión del TRAMO, no la del tablero: un circuito de 1 polo va a fase-neutro.
            TensionV: circuito.Polos == 1 ? datos.TensionFaseNeutroV : datos.TensionFaseFaseV,
            NumeroFases: circuito.Polos,
            NumeroHilos: circuito.Polos + 1,
            FactorPotencia: circuito.FactorPotencia,
            LongitudM: circuito.LongitudM,
            MaterialConductor: Etiqueta(datos.MaterialConductor),
            CorrienteDisenoA: r.CorrienteDisenoA,
            ProteccionA: r.ProteccionA,
            ConductorFase: r.CalibreFase.Designacion,
            ConductorNeutro: r.CalibreNeutro.Designacion,
            ConductorTierra: r.CalibreTierra.Designacion,
            CaidaTensionPct: r.CaidaTensionPct,
            TablaAmpacidadId: r.TablaAmpacidadId,
            ConductoresPorFase: r.NumeroConductoresParalelo,
            Detalle: r.Detalle,
            Citas: r.Citas,
            SerieDeInterruptores: cuadro.Datos.SerieInterruptores.Explicacion(),
            Aislamiento: Aislamiento(cuadro.Datos),
            DesgloseConductor: cuadro.Desglose(circuito)?.Conductor,
            CaidaCombinada: CaidaCombinada(cuadro, circuito),
            NeutroPortador: NeutroPortador(cuadro, circuito.Polos, alimentador: false));
    }

    /// <summary>
    /// R-09. En un tramo de <b>2 fases + neutro de una estrella</b> (220Y/127) el neutro lleva
    /// aproximadamente la corriente de fase: cuenta como portador — 310-15(b)(5)(2) — y en el
    /// alimentador no se reduce — 220-61(c)(1). <c>null</c> en cualquier otro tramo: en 1F-3H 120/240
    /// el neutro lleva solo el desbalance — (b)(5)(1).
    /// </summary>
    private static string? NeutroPortador(CuadroDeCarga cuadro, int fases, bool alimentador)
    {
        var estrella = SistemaDelTablero.De(cuadro.Datos.Sistema)
            is ConfiguracionTablero.DosFasesDeEstrella or ConfiguracionTablero.TresFasesCuatroHilos;
        if (!estrella || fases != 2)
            return null;

        return "Portador de corriente: en 2 fases + neutro de estrella lleva ≈ la corriente de fase. Contarlo en " +
               "«Agrupados». Mismo calibre que la fase" + (alimentador ? "; no se reduce — 220-61(c)(1)." : ".");
    }

    /// <summary>«Alimentador 4.62 % + circuito 2.31 % = 6.94 % — mayor que 5 %». <c>null</c> sin alimentador.</summary>
    private static string? CaidaCombinada(CuadroDeCarga cuadro, CircuitoDelCuadro circuito)
    {
        if (circuito.CaidaCombinadaPct is not { } combinada || circuito.CaidaAlimentadorPct is not { } alimentador)
            return null;

        var limite = DatosDelTablero.CaidaMaxCombinadaPct;
        return $"Alimentador {alimentador:N2} % + circuito {circuito.Resultado!.CaidaTensionPct:N2} % = {combinada:N2} % " +
               (circuito.AvisoCaidaCombinada is null ? $"≤ {limite:N0} %" : $"— mayor que {limite:N0} %");
    }

    public static HojaDeMemoria? DelAlimentador(CuadroDeCarga cuadro)
    {
        if (cuadro.Alimentador.Resultado is not { } r)
            return null;

        var datos = cuadro.Datos;
        var nombre = string.IsNullOrWhiteSpace(datos.Tablero) ? "del tablero" : $"del tablero {datos.Tablero.Trim()}";

        return new HojaDeMemoria(
            Sujeto: $"Alimentador general {nombre}",
            Articulo: "215",
            CargaContinuaVa: cuadro.Resumen.ContinuaVA,
            CargaNoContinuaVa: cuadro.Resumen.NoContinuaVA,
            TensionV: cuadro.Alimentador.Polos == 1 ? datos.TensionFaseNeutroV : datos.TensionFaseFaseV,
            NumeroFases: cuadro.Alimentador.Polos,
            NumeroHilos: datos.Hilos,
            // Resulta de las cargas de la fase que gobierna; no se captura.
            FactorPotencia: cuadro.Alimentador.FactorPotencia,
            LongitudM: datos.LongitudAlimentadorM,
            MaterialConductor: Etiqueta(datos.MaterialConductor),
            CorrienteDisenoA: r.CorrienteDisenoA,
            ProteccionA: r.ProteccionA,
            ConductorFase: r.CalibreFase.Designacion,
            ConductorNeutro: r.CalibreNeutro.Designacion,
            ConductorTierra: r.CalibreTierra.Designacion,
            CaidaTensionPct: r.CaidaTensionPct,
            TablaAmpacidadId: r.TablaAmpacidadId,
            ConductoresPorFase: r.NumeroConductoresParalelo,
            Detalle: r.Detalle,
            Citas: r.Citas,
            FaseQueGobierna: FaseQueGobierna(cuadro),
            SerieDeInterruptores: cuadro.Datos.SerieInterruptores.Explicacion(),
            Aislamiento: Aislamiento(cuadro.Datos),
            DesgloseConductor: cuadro.DesgloseDelAlimentador()?.Conductor,
            Minimo220_52VA: cuadro.Resumen.Minimo220_52VA,
            NeutroPortador: NeutroPortador(cuadro, cuadro.Alimentador.Polos, alimentador: true),
            CaidaPorFase: r.CaidaPorFase,
            CorrienteNeutro: r.CorrienteNeutro,
            TensionFaseNeutroV: datos.TensionFaseNeutroV);
    }

    /// <summary>
    /// «Fase C, la más cargada: 125 % × 12.20 A + 0.00 A = 15.25 A». Es lo que explica por qué la
    /// corriente de diseño del alimentador no es la carga total entre √3·V: el alimentador se
    /// dimensiona con la barra que más lleva — M-02.
    /// </summary>
    private static string? FaseQueGobierna(CuadroDeCarga cuadro)
    {
        if (cuadro.Alimentador.Gobierna is not { } g || cuadro.Alimentador.Fases is not { Count: > 1 } fases)
            return null;

        return $"Fase {g.Fase}, la más cargada: {g.FactorContinua * 100m:0} % × {g.ContinuaA:N2} A (continua) + " +
               $"{g.NoContinuaA:N2} A (no continua) = {g.CapacidadA:N2} A. Las demás: " +
               string.Join(", ", fases.Where(f => f.Fase != g.Fase).Select(f => $"fase {f.Fase} {f.CapacidadA:N2} A")) +
               ". El alimentador se dimensiona con la corriente de esta fase, no con la carga total repartida.";
    }

    /// <summary>Las nueve secciones de una hoja, en el orden en que se imprimen.</summary>
    public static IReadOnlyList<BloqueMemoria> Secciones(HojaDeMemoria hoja)
    {
        var d = hoja.Detalle;
        var cargaTotal = hoja.CargaContinuaVa + hoja.CargaNoContinuaVa;
        var senTheta = TrianguloPotencias.SenoDelAngulo(hoja.FactorPotencia);
        var bloques = new List<BloqueMemoria>();

        // ---- 1
        bloques.Add(Seccion("1. DATOS DEL SISTEMA", [
            ("Carga total instalada", $"{cargaTotal:N0} VA"),
            ("Carga continua", $"{hoja.CargaContinuaVa:N0} VA"),
            ("Carga no continua", $"{hoja.CargaNoContinuaVa:N0} VA"),
            ("Mínimo 220-52", hoja.Minimo220_52VA > 0m
                ? $"{hoja.Minimo220_52VA:N0} VA — aparatos pequeños y lavadora a 1,500 VA por circuito, 220-52(a) y (b)"
                : null),
            ("Carga calculada", hoja.Minimo220_52VA > 0m ? $"{cargaTotal + hoja.Minimo220_52VA:N0} VA" : null),
            ("Tensión nominal", $"{hoja.TensionV:N1} V"),
            ("Frecuencia", "60 Hz"),
            ("Factor de potencia", hoja.Articulo == "215"
                ? $"{hoja.FactorPotencia:N2} — resulta de combinar las cargas de la fase que gobierna"
                : $"{hoja.FactorPotencia:N2}"),
            ("Fases / hilos", $"{hoja.NumeroFases} / {hoja.NumeroHilos}")]));

        // ---- 2
        bloques.Add(Seccion("2. CONSIDERACIONES", [
            ("Material del conductor", hoja.MaterialConductor),
            ("Aislamiento — Tabla 310-104(a)", hoja.Aislamiento),
            ("Hilos por fase", hoja.ConductoresPorFase.ToString()),
            ("Longitud del tramo", $"{hoja.LongitudM:N2} m"),
            ("Temperatura del aislamiento", d is null ? null : $"{d.TemperaturaAislamientoC} °C"),
            ("Temperatura de terminales", d is null ? null : $"{d.TemperaturaTerminalesC} °C — 110-14(c)(1)"),
            ("Neutro — 310-15(b)(5)(2)", hoja.NeutroPortador)]));

        // ---- 3
        var articuloProteccion = hoja.Articulo == "215" ? "215-3" : "210-20(a)";
        bloques.Add(Seccion("3. SELECCIÓN DE LA PROTECCIÓN", [
            ("Fase que gobierna", hoja.FaseQueGobierna),
            ("Corriente de diseño (In)", Amperes(hoja.CorrienteDisenoA)),
            ($"Capacidad mínima — {articuloProteccion}", d is null ? null : Amperes(d.CapacidadMinimaA)),
            ("Protección seleccionada — 240-6(a)", Amperes(hoja.ProteccionA, "N0")),
            ("Tamaños de interruptor", hoja.SerieDeInterruptores)]));

        // ---- 4: cómo se llegó a la ampacidad del conductor, paso por paso. Hasta el 2026-09-23 aquí
        // decía «Icm = In / (FT × FA)» y sustituía la CAPACIDAD MÍNIMA, no In; con 6 agrupados
        // imprimía «= 50 A» y luego un conductor de 40 A, sin decir que esos 50 son de la columna
        // del aislamiento y los 40 el tope de la terminal. Ahora se enseñan las dos columnas.
        bloques.Add(new BloqueMemoria(
            "4. CÁLCULO POR CAPACIDAD",
            [],
            hoja.DesgloseConductor ?? [],
            [
                "FT es el factor de corrección por temperatura ambiente (Tabla 310-15(b)(2)(a)) y FA el factor de " +
                "ajuste por agrupamiento (Tabla 310-15(b)(3)(a)). Se aplican en la columna del aislamiento; la " +
                "ampacidad que se usa no pasa de la de la terminal — 110-14(c).",
            ],
            Introduccion: "Ampacidad del conductor elegido:"));

        // ---- 5
        bloques.Add(Seccion("5. CONDUCTOR DE FASE SELECCIONADO", [
            ("Calibre", CalibreDe(hoja.ConductorFase, hoja.ConductoresPorFase)),
            ("Conductor de neutro", CalibreDe(hoja.ConductorNeutro, 1)),
            ("Ampacidad utilizable", d is { AmpacidadConductorA: > 0m } ? $"{d.AmpacidadConductorA:N2} A" : null)]));

        // ---- 6
        if (hoja.CaidaPorFase is { Count: > 0 } porFase && d is not null)
        {
            // R-02: fase por fase con el neutro. Cada renglón se puede recalcular a mano: Z por la
            // suma fasorial de la corriente de la fase y la del neutro, proyectada sobre su tensión.
            var km = hoja.LongitudM / 1000m;
            var peor = porFase.Aggregate((max, f) => f.CaidaPct > max.CaidaPct ? f : max);
            var formulasFase = new List<string>
            {
                "e_f = Re[ Z × (I_f + I_N) × conj(û_f) ],   I_N = suma fasorial de las corrientes de fase",
                $"Z = ( {d.ResistenciaOhmKm:N4} + j {d.ReactanciaOhmKm:N4} ) Ω/km × {km:N3} km / {hoja.ConductoresPorFase}",
            };
            if (hoja.CorrienteNeutro is { } iN)
                formulasFase.Add($"I_N = {iN.Magnitud:N2} A ∠ {iN.AnguloGrados:N1}°");
            formulasFase.AddRange(porFase.Select(f =>
                $"Fase {f.Fase}: I = {f.Corriente.Magnitud:N2} A ∠ {f.Corriente.AnguloGrados:N1}° → e = {f.CaidaV:N2} V ({f.CaidaPct:N2} %)"));
            bloques.Add(new BloqueMemoria("6. CÁLCULO DE CAÍDA DE TENSIÓN", [], formulasFase,
            [
                "R y X en ohm/km, de la Tabla 9 de la NOM-001-SEDE-2012. El neutro es del mismo calibre que la fase.",
                $"Ángulos respecto a V_AN = 0°; cada corriente, atrasada según el F.P. de sus circuitos. Porcentaje sobre " +
                $"V_FN = {hoja.TensionFaseNeutroV:N2} V. Manda la fase {peor.Fase}.",
            ]));
        }
        else
        {
            var formulas6 = new List<string>
            {
                hoja.NumeroFases == 3
                    ? "e = √3 × L × In × [ R × cos(θ) + X × sen(θ) ] / N"
                    : "e = 2 × L × In × [ R × cos(θ) + X × sen(θ) ] / N",
            };
            var notas6 = new List<string>();
            if (d is not null)
            {
                var k = hoja.NumeroFases == 3 ? "√3" : "2";
                formulas6.Add(
                    $"e = {k} × {hoja.LongitudM:N2} m × {hoja.CorrienteDisenoA:N2} A × " +
                    $"[ {d.ResistenciaOhmKm:N4} × {hoja.FactorPotencia:N2} + {d.ReactanciaOhmKm:N4} × {senTheta:N2} ] / " +
                    $"{hoja.ConductoresPorFase} = {d.CaidaTensionV:N2} V");
                notas6.Add(
                    $"R y X en ohm/km, de la Tabla 9 de la NOM-001-SEDE-2012. cos(θ) = {hoja.FactorPotencia:N2}, " +
                    $"sen(θ) = {senTheta:N2}.");
            }
            bloques.Add(new BloqueMemoria("6. CÁLCULO DE CAÍDA DE TENSIÓN", [], formulas6, notas6));
        }

        // ---- 7
        var renglones7 = new List<(string, string)>
        {
            ("Caída de tensión", (d is null ? $"{hoja.CaidaTensionPct:N2} %" : $"{d.CaidaTensionV:N2} V  ({hoja.CaidaTensionPct:N2} %)")
                + (hoja.CaidaPorFase is { Count: > 0 } fs ? $" — fase {fs.Aggregate((m, f) => f.CaidaPct > m.CaidaPct ? f : m).Fase}" : "")),
        };
        if (hoja.CaidaCombinada is { } combinada)
            renglones7.Add(("Caída combinada", combinada));
        renglones7.Add(("Referencia", hoja.Articulo == "215"
            ? "215-2(a)(4), NOTA 2 — caída recomendada: 3 % en el alimentador y 5 % combinada con el derivado"
            : "210-19(a)(1), NOTA 4 — caída recomendada: 3 % en el derivado y 5 % combinada con el alimentador"));
        bloques.Add(Seccion("7. CAÍDA DE TENSIÓN EN EL TRAMO", [.. renglones7]));

        // ---- 8
        bloques.Add(new BloqueMemoria(
            "8. SELECCIÓN DEL CONDUCTOR DE PUESTA A TIERRA",
            [],
            [],
            [
                "Se selecciona de la Tabla 250-122 de la NOM-001-SEDE-2012, entrando con la capacidad del dispositivo " +
                "de protección contra sobrecorriente" + (Amperes(hoja.ProteccionA, "N0") is { } p ? $" ({p})." : "."),
                "Cuando el tamaño nominal de los alimentadores se ajuste para compensar la caída de tensión eléctrica, " +
                "el conductor de puesta a tierra de equipo deberá ajustarse proporcionalmente según el área en mm² de " +
                "su sección transversal. — 250-122(b)",
            ]));

        // ---- 9
        bloques.Add(Seccion("9. CONDUCTOR DE PUESTA A TIERRA SELECCIONADO", [
            ("Calibre", CalibreDe(hoja.ConductorTierra, 1))]));

        return bloques;
    }

    private static BloqueMemoria Seccion(string titulo, (string Rotulo, string? Valor)[] renglones) =>
        new(titulo,
            [.. renglones
                .Where(r => !string.IsNullOrWhiteSpace(r.Valor))
                .Select(r => new RenglonMemoria(r.Rotulo, r.Valor!))],
            [],
            []);

    private static string? CalibreDe(string? designacion, int porFase) =>
        designacion is null ? null
        : porFase > 1 ? $"{porFase} × {designacion} AWG/kcmil por fase"
        : $"{designacion} AWG/kcmil";

    /// <summary>
    /// Una corriente, o <c>null</c> si vale cero. <b>Cero amperes no es un resultado</b>: es un
    /// cálculo que no se ha corrido, y en una memoria que se firma «Protección seleccionada: 0 A»
    /// es peor que un renglón ausente — el renglón que falta se ve, el cero se lee como dato.
    /// </summary>
    private static string? Amperes(decimal valor, string formato = "N2") =>
        valor <= 0m ? null : $"{valor.ToString(formato)} A";

    /// <summary>«THHN · lugar seco»: lo que decide la columna de la Tabla 310-15(b)(16).</summary>
    public static string Aislamiento(DatosDelTablero datos) =>
        $"{datos.TipoAislamiento} · lugar {(datos.LugarSeco ? "seco" : "húmedo o mojado")}" +
        (datos.TerminalesMarcadas75C ? " · terminales marcadas 75 °C" : "");

    /// <summary>«Contactos · Aparatos pequeños (cocina)»: el tipo, y el uso si no es general.</summary>
    public static string Etiqueta(CircuitoDelCuadro circuito) =>
        circuito.UsoEfectivo == UsoDeContactos.General
            ? Etiqueta(circuito.Tipo)
            : $"{Etiqueta(circuito.Tipo)} · {circuito.UsoEfectivo.Nombre()}";

    public static string Etiqueta(TipoCarga tipo) => tipo switch
    {
        TipoCarga.Alumbrado => "Alumbrado",
        TipoCarga.Contactos => "Contactos",
        TipoCarga.Fuerza => "Fuerza",
        _ => "Equipo",
    };

    public static string Etiqueta(MaterialConductor material) =>
        material == MaterialConductor.Cobre ? "Cobre" : "Aluminio";

    public static string Etiqueta(MaterialCanalizacion canalizacion) => canalizacion switch
    {
        MaterialCanalizacion.Pvc => "PVC",
        MaterialCanalizacion.Aluminio => "Aluminio",
        _ => "Acero",
    };
}
