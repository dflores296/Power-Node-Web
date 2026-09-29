using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Por qué salió esa protección y ese calibre</b>, renglón por renglón y con el artículo de cada
/// paso. Lo muestra la pantalla en un tooltip —sin columnas nuevas— y la memoria en su sección 4.
///
/// <para>
/// <b>No decide nada.</b> Relee lo que el motor ya calculó (<see cref="DetalleDelCalculo"/>, las
/// citas) y lo pone con sus números. Lo único que consulta por su cuenta es la ampacidad de tabla
/// del calibre elegido en cada columna, que el resultado no trae: sin ella no se ve por qué la
/// ampacidad utilizable es la que es.
/// </para>
/// </summary>
/// <param name="Proteccion">Corriente de diseño → capacidad mínima → tamaño estándar.</param>
/// <param name="Conductor">Columna del aislamiento con sus factores, tope de la terminal y las revisiones.</param>
public sealed record DesgloseDeSeleccion(IReadOnlyList<string> Proteccion, IReadOnlyList<string> Conductor)
{
    /// <summary>
    /// El grupo de motores de 430-24, ya redactado: «125 % × 15.20 A + 20.00 A». Si el grupo solo trae
    /// equipos de A/C con MCA no hay mayor que lleve el 125 %: su MCA ya trae el de su motor mayor,
    /// 440-4(b) — M-13.
    /// </summary>
    public static string Grupo430_24(AgregadoMotores motores) =>
        motores.MayorFlcA is > 0m and { } mayor
            ? $"125 % × {mayor:N2} A + {motores.SumaRestoFlcA:N2} A"
            : $"{motores.SumaRestoFlcA:N2} A al 100 % (la MCA ya trae el 125 % de su motor mayor — 440-4(b))";

    /// <summary>
    /// Arma el desglose de un tramo ya calculado, sea circuito derivado o alimentador.
    /// </summary>
    /// <param name="iContinuaA">Corriente de la parte continua, como entró al cálculo.</param>
    /// <param name="iNoContinuaA">Corriente de la parte no continua.</param>
    /// <param name="articuloProteccion">«210-20(a)» en un derivado, «215-3» en un alimentador.</param>
    /// <param name="proteccionSinMinimo">El tamaño estándar que tocaba por la capacidad mínima, antes
    /// de la protección mínima del circuito: si difiere de la protección, fue el mínimo.</param>
    /// <param name="referenciaMinimo">De dónde sale esa protección mínima — «210-11(c)(1)».</param>
    internal static DesgloseDeSeleccion De(
        ITablaAmpacidad ampacidad,
        DatosDelTablero datos,
        decimal iContinuaA,
        decimal iNoContinuaA,
        decimal factorContinua,
        string articuloProteccion,
        string articuloConductor,
        decimal proteccionA,
        decimal proteccionSinMinimo,
        Calibre calibre,
        int conductoresPorFase,
        DetalleDelCalculo d,
        IReadOnlyList<Cita> citas,
        string? referenciaMinimo = null,
        AgregadoMotores motores = default)
    {
        // Con motores (I-15), la corriente y la capacidad llevan su parte: la FLC de todos al 100 %
        // y 430-24 aparte, sin el 125 % de la continua.
        var conMotores = motores.MayorFlcA is not null;
        var corriente = iContinuaA + iNoContinuaA + motores.CorrienteRealA;
        var proteccion = new List<string>
        {
            $"In = {iContinuaA:N2} A (continua) + {iNoContinuaA:N2} A (no continua)" +
                (conMotores ? $" + {motores.CorrienteRealA:N2} A (motores)" : "") + $" = {corriente:N2} A",
            conMotores
                ? $"Capacidad mínima = {factorContinua * 100m:0} % × {iContinuaA:N2} A + {iNoContinuaA:N2} A + " +
                  $"({Grupo430_24(motores)}) = {d.CapacidadMinimaA:N2} A — {articuloProteccion}, 430-24"
                : $"Capacidad mínima = {factorContinua * 100m:0} % × {iContinuaA:N2} A + {iNoContinuaA:N2} A = {d.CapacidadMinimaA:N2} A — {articuloProteccion}",
            $"Protección: {proteccionSinMinimo:N0} A, primer tamaño ≥ capacidad mínima en «{datos.SerieInterruptores.Nombre()}» — 240-6(a)",
        };
        if (proteccionA != proteccionSinMinimo)
            proteccion.Add($"Protección mínima del circuito: {proteccionA:N0} A — {referenciaMinimo}");

        var conductor = LineasDelConductor(ampacidad, datos, proteccionA, calibre, conductoresPorFase, d);

        // Las dos revisiones de 210-19(a)(1) / 215-2(a)(1), por separado: el 125 % contra la tabla
        // SIN factores (en la columna de la terminal), la carga al 100 % contra la corregida. Con
        // motores, 430-24 es un piso de ampacidad con su propia regla: contra la corregida.
        if (conMotores)
            conductor.Add(
                $"Con factores: {d.AmpacidadConductorA:N2} A ≥ capacidad mínima {d.CapacidadMinimaA:N2} A " +
                $"{(d.AmpacidadConductorA >= d.CapacidadMinimaA ? "✔" : "✘")} — {articuloConductor}, 430-24");
        else
        {
            if (ampacidad.Ampacidad(calibre, datos.MaterialConductor, (TemperaturaAislamiento)d.TemperaturaTerminalesC) is { } tabla)
            {
                var tablaTotal = tabla * conductoresPorFase;
                conductor.Add(
                    $"Antes de factores: {tablaTotal:N2} A a {d.TemperaturaTerminalesC} °C ≥ capacidad mínima {d.CapacidadMinimaA:N2} A " +
                    $"{(tablaTotal >= d.CapacidadMinimaA ? "✔" : "✘")} — {articuloConductor}");
            }
            conductor.Add(
                $"Con factores: {d.AmpacidadConductorA:N2} A ≥ carga {corriente:N2} A {(d.AmpacidadConductorA >= corriente ? "✔" : "✘")} — {articuloConductor}");
        }

        conductor.AddRange(PorQueCrecio(citas, proteccionA, d));
        return new DesgloseDeSeleccion(proteccion, conductor);
    }

    /// <summary>
    /// <b>El derivado de un motor</b> — I-15: la FLC de tabla, el 125 % para el conductor (430-22) y el
    /// porcentaje de la Tabla 430-52 para la protección, con el tamaño inmediato superior que permite
    /// su Excepción 1. La protección puede quedar arriba de la ampacidad del conductor: es protección
    /// contra cortocircuito y falla a tierra; la sobrecarga la cuida el arrancador — 430-32, 240-4(g).
    /// </summary>
    /// <param name="origenFlc">De dónde sale la FLC, ya redactado — <see cref="CuadroDeCarga.OrigenDeLaFlc"/>.</param>
    /// <param name="porcentaje">El de la Tabla 430-52: 250 % con interruptor de tiempo inverso.</param>
    internal static DesgloseDeSeleccion DeMotor(
        ITablaAmpacidad ampacidad,
        DatosDelTablero datos,
        string origenFlc,
        decimal flcA,
        decimal porcentaje,
        decimal proteccionA,
        Calibre calibre,
        int conductoresPorFase,
        DetalleDelCalculo d,
        IReadOnlyList<Cita> citas,
        string? servicio = null)
    {
        var techo = flcA * porcentaje / 100m;
        var proteccion = new List<string>
        {
            origenFlc,
            $"Máximo = {porcentaje:0} % × {flcA:N2} A = {techo:N2} A, interruptor de tiempo inverso — Tabla 430-52",
            $"Protección: {proteccionA:N0} A, " +
                (proteccionA == techo ? "igual al máximo" : "tamaño inmediato superior al máximo") +
                $" en «{datos.SerieInterruptores.Nombre()}» — 430-52(c)(1) Excepción 1",
            "Sobrecarga del motor: relevador en el arrancador o protector del motor — 430-32",
        };

        var conductor = LineasDelConductor(ampacidad, datos, proteccionA, calibre, conductoresPorFase, d);
        // Servicio no continuo (I-120): el % de la Tabla 430-22(e) sobre la placa, en lugar del 125 % de la FLC.
        conductor.Add(servicio is not null
            ? $"Con factores: {d.AmpacidadConductorA:N2} A ≥ {d.CapacidadMinimaA:N2} A {(d.AmpacidadConductorA >= d.CapacidadMinimaA ? "✔" : "✘")} — {servicio}, 430-22(e)"
            : $"Con factores: {d.AmpacidadConductorA:N2} A ≥ 125 % × {flcA:N2} A = {d.CapacidadMinimaA:N2} A " +
              $"{(d.AmpacidadConductorA >= d.CapacidadMinimaA ? "✔" : "✘")} — 430-22");
        conductor.AddRange(PorQueCrecio(citas, proteccionA, d));
        return new DesgloseDeSeleccion(proteccion, conductor);
    }

    /// <summary>
    /// <b>El derivado de un variador</b> — I-119: 125 % de la corriente de entrada (430-122(a)) y el mayor
    /// tamaño que no excede la protección máxima del fabricante (110-3(b)).
    /// </summary>
    internal static DesgloseDeSeleccion DeVariador(
        ITablaAmpacidad ampacidad,
        DatosDelTablero datos,
        decimal entradaA,
        decimal maximaA,
        decimal proteccionA,
        Calibre calibre,
        int conductoresPorFase,
        DetalleDelCalculo d,
        IReadOnlyList<Cita> citas)
    {
        var proteccion = new List<string>
        {
            $"Corriente = {entradaA:N2} A, la nominal de entrada del variador — 430-122(a)",
            $"Protección máxima del fabricante: {maximaA:N0} A — 110-3(b)",
            $"Protección: {proteccionA:N0} A, " +
                (proteccionA == maximaA ? "la máxima del fabricante" : "el mayor tamaño estándar que no la excede") +
                $" en «{datos.SerieInterruptores.Nombre()}»",
            "Sobrecarga del motor: la da el variador si así lo marca — 430-124(a)",
        };

        var conductor = LineasDelConductor(ampacidad, datos, proteccionA, calibre, conductoresPorFase, d);
        conductor.Add(
            $"Con factores: {d.AmpacidadConductorA:N2} A ≥ 125 % × {entradaA:N2} A = {d.CapacidadMinimaA:N2} A " +
            $"{(d.AmpacidadConductorA >= d.CapacidadMinimaA ? "✔" : "✘")} — 430-122(a)");
        conductor.AddRange(PorQueCrecio(citas, proteccionA, d));
        return new DesgloseDeSeleccion(proteccion, conductor);
    }

    /// <summary>
    /// <b>Varios motores, o motores y otras cargas, en un circuito</b> — I-115. Cada máquina con su
    /// corriente, el límite de 430-53(c)(4) (o de 440-22(b)) y el mayor tamaño que no lo pasa; el
    /// conductor, contra la capacidad mínima de 430-24.
    /// </summary>
    /// <param name="maquinas">Una línea por máquina, ya redactada: «Extractor: 3 × 8.90 A — ½ HP, Tabla 430-248…».</param>
    /// <param name="capacidad">La suma de 430-24 ya redactada, de la cita del motor.</param>
    internal static DesgloseDeSeleccion DeGrupo(
        ITablaAmpacidad ampacidad,
        DatosDelTablero datos,
        DetalleDelGrupo g,
        IReadOnlyList<string> maquinas,
        decimal otrasContinuaA,
        decimal otrasNoContinuaA,
        string capacidad,
        string articuloCapacidad,
        decimal proteccionA,
        Calibre calibre,
        int conductoresPorFase,
        DetalleDelCalculo d,
        IReadOnlyList<Cita> citas)
    {
        var proteccion = new List<string>(maquinas);
        if (otrasContinuaA + otrasNoContinuaA > 0m)
            proteccion.Add($"Otras cargas: {otrasContinuaA:N2} A (continua) + {otrasNoContinuaA:N2} A (no continua)");

        var partes = new List<string> { $"{g.PorcentajeMayor:0} % × {g.Mayor.CorrienteUnitariaA:N2} A ({g.Mayor.Nombre})" };
        if (g.SumaDemasA > 0m)
            partes.Add($"{g.SumaDemasA:N2} A (las demás máquinas)");
        if (g.OtrasCargasA > 0m)
            partes.Add($"{g.OtrasCargasA:N2} A (otras cargas)");
        proteccion.Add($"Máximo = {string.Join(" + ", partes)} = {g.TechoA:N2} A — {g.Regla}");
        proteccion.Add(g.Limite240_4bA is { } limite
            ? $"Protección: {proteccionA:N0} A, el primer tamaño que lleva la corriente de operación ({g.PisoA:N2} A): el máximo queda abajo de " +
              $"la ampacidad del conductor y se permite subir hasta {limite:N0} A — 430-53(c)(4), 240-4(b)"
            : proteccionA > g.TechoA
                ? $"Protección: {proteccionA:N0} A, no se exige menos — 440-22(a) Excepción"
                : $"Protección: {proteccionA:N0} A, el mayor tamaño estándar que no excede el máximo en «{datos.SerieInterruptores.Nombre()}» — " +
                  $"{g.Regla}, sin el redondeo hacia arriba de 430-52(c)(1) Excepción 1");
        proteccion.Add("Sobrecarga: la de cada motor, con controlador y relevador aprobados para instalación en grupo — 430-53(c), 430-32" +
                       (g.Regla.StartsWith("440") ? ", 440-52" : ""));

        var conductor = LineasDelConductor(ampacidad, datos, proteccionA, calibre, conductoresPorFase, d);
        conductor.Add($"Capacidad mínima = {capacidad} — {articuloCapacidad}");
        conductor.Add(
            $"Con factores: {d.AmpacidadConductorA:N2} A ≥ {d.CapacidadMinimaA:N2} A " +
            $"{(d.AmpacidadConductorA >= d.CapacidadMinimaA ? "✔" : "✘")} — {articuloCapacidad}");
        conductor.AddRange(PorQueCrecio(citas, proteccionA, d));
        return new DesgloseDeSeleccion(proteccion, conductor);
    }

    /// <summary>
    /// <b>El derivado de un equipo de A/C o refrigeración</b> — I-74, Art. 440. Con la corriente de
    /// placa: 125 % para el conductor (440-32) y el mayor tamaño estándar que no pase de 175 % —o
    /// 225 % si no arranca— para la protección (440-22(a)), sin redondear hacia arriba. Con la
    /// ampacidad mínima y la protección máxima de la placa (440-4(b)): esas dos, tal cual.
    /// </summary>
    internal static DesgloseDeSeleccion DeAireAcondicionado(
        ITablaAmpacidad ampacidad,
        DatosDelTablero datos,
        CircuitoDelCuadro c,
        decimal proteccionA,
        Calibre calibre,
        int conductoresPorFase,
        DetalleDelCalculo d,
        IReadOnlyList<Cita> citas)
    {
        List<string> proteccion;
        string requisito;
        if (c.PlacaAire == PlacaDeAireAcondicionado.Habitacion)
        {
            // 440 Parte G — I-117.
            var i = c.CorrienteDeMotorA;
            proteccion =
            [
                $"Corriente total de placa = {i:N2} A: acondicionador de habitación, una sola unidad de motor — 440-62(a)",
                $"Sin otras cargas, no más del 80 % del circuito: {i:N2} A ÷ 0.8 = {i / 0.8m:N2} A — 440-62(b)",
                $"Protección: {proteccionA:N0} A, el primer tamaño estándar que lo cumple en «{datos.SerieInterruptores.Nombre()}»; " +
                    "no excede la ampacidad del conductor ni el valor del contacto — 440-62(a)(4)",
            ];
            requisito = $"125 % × {i:N2} A = {d.CapacidadMinimaA:N2} A y la protección, {proteccionA:N0} A " +
                        $"{(d.AmpacidadConductorA >= Math.Max(d.CapacidadMinimaA, proteccionA) ? "✔" : "✘")} — 440-32, 440-62(a)(4)";
        }
        else if (c.PlacaAire == PlacaDeAireAcondicionado.AmpacidadYProteccion)
        {
            proteccion =
            [
                $"Placa: ampacidad mínima {c.AmpacidadMinimaA:N2} A, protección máxima {c.ProteccionMaximaA:N0} A — 440-4(b)",
                $"Protección: {proteccionA:N0} A, " +
                    (proteccionA == c.ProteccionMaximaA ? "la máxima de placa" : "el mayor tamaño estándar que no excede la máxima de placa") +
                    $" en «{datos.SerieInterruptores.Nombre()}»",
            ];
            requisito = $"ampacidad mínima de placa {d.CapacidadMinimaA:N2} A {(d.AmpacidadConductorA >= d.CapacidadMinimaA ? "✔" : "✘")} — 440-4(b)";
        }
        else
        {
            var baseA = c.CorrienteDeMotorA;
            var pct = c.ArranqueAl225 ? CalculadoraCarga440.TechoProteccionArranquePct : CalculadoraCarga440.TechoProteccionPct;
            proteccion =
            [
                c.CorrienteSeleccionA is > 0m and { } sel && sel > c.CorrientePlacaA
                    ? $"Corriente = {baseA:N2} A, la de selección del circuito (mayor que la nominal, {c.CorrientePlacaA:N2} A) — 440-6(a) Excepción 1"
                    : $"Corriente = {baseA:N2} A, la de carga nominal de la placa — 440-6(a)",
                $"Máximo = {pct:0} % × {baseA:N2} A = {baseA * pct / 100m:N2} A" + (c.ArranqueAl225 ? ", porque al 175 % no arranca" : "") + " — 440-22(a)",
                $"Protección: {proteccionA:N0} A, " +
                    (proteccionA == CalculadoraCarga440.ProteccionMinimaA && baseA * pct / 100m < proteccionA
                        ? "no se exige menos — 440-22(a) Excepción"
                        : $"el mayor tamaño estándar que no excede el máximo en «{datos.SerieInterruptores.Nombre()}» — 440-22(a) no permite redondear hacia arriba"),
            ];
            requisito = $"125 % × {baseA:N2} A = {d.CapacidadMinimaA:N2} A {(d.AmpacidadConductorA >= d.CapacidadMinimaA ? "✔" : "✘")} — 440-32";
        }
        proteccion.Add("Sobrecarga del motocompresor: su protector o el relevador del equipo — 440-52");

        var conductor = LineasDelConductor(ampacidad, datos, proteccionA, calibre, conductoresPorFase, d);
        conductor.Add($"Con factores: {d.AmpacidadConductorA:N2} A ≥ {requisito}");
        conductor.AddRange(PorQueCrecio(citas, proteccionA, d));
        return new DesgloseDeSeleccion(proteccion, conductor);
    }

    /// <summary>La columna del aislamiento con sus factores, el tope de la terminal y la ampacidad utilizable.</summary>
    private static List<string> LineasDelConductor(
        ITablaAmpacidad ampacidad, DatosDelTablero datos, decimal proteccionA, Calibre calibre, int conductoresPorFase, DetalleDelCalculo d)
    {
        var tAislamiento = (TemperaturaAislamiento)d.TemperaturaAislamientoC;
        var tTerminal = (TemperaturaAislamiento)d.TemperaturaTerminalesC;
        var deTablaAislamiento = ampacidad.Ampacidad(calibre, datos.MaterialConductor, tAislamiento);
        var deTablaTerminal = ampacidad.Ampacidad(calibre, datos.MaterialConductor, tTerminal);
        var porFase = conductoresPorFase > 1 ? $" (× {conductoresPorFase} por fase)" : "";

        var conductor = new List<string>
        {
            $"{datos.TipoAislamiento} {d.TemperaturaAislamientoC} °C · terminal {d.TemperaturaTerminalesC} °C " + PorQueLaTerminal(proteccionA, datos.TerminalesMarcadas75C, d.TemperaturaTerminalesC),
        };

        if (deTablaAislamiento is { } a)
        {
            var corregida = a * d.FactorTemperatura * d.FactorAgrupamiento;
            conductor.Add(
                $"{calibre.Designacion} AWG/kcmil a {d.TemperaturaAislamientoC} °C: {a:N0} A × FT {d.FactorTemperatura:N2} × FA {d.FactorAgrupamiento:N2} = {corregida:N2} A — Tabla 310-15(b)(16), 310-15(b)(2)(a), 310-15(b)(3)(a)");
            if (tAislamiento != tTerminal && deTablaTerminal is { } t)
                conductor.Add($"Tope de la terminal: {t:N0} A a {d.TemperaturaTerminalesC} °C, sin factores — 110-14(c)(1)");
        }

        conductor.Add($"Ampacidad utilizable: {d.AmpacidadConductorA:N2} A{porFase}");
        return conductor;
    }

    /// <summary>
    /// Por qué el conductor terminó más grueso de lo que pedía la capacidad: lo dice el motor en sus
    /// citas, y aquí solo se repite en corto.
    /// </summary>
    private static IEnumerable<string> PorQueCrecio(IReadOnlyList<Cita> citas, decimal proteccionA, DetalleDelCalculo d)
    {
        foreach (var cita in citas)
        {
            if (cita.Referencia == "240-4(b)")
                yield return $"Protección {proteccionA:N0} A > {d.AmpacidadConductorA:N2} A: estándar inmediato superior permitido — 240-4(b)";
            else if (cita.Referencia == "240-4")
                yield return $"Conductor aumentado para quedar protegido por {proteccionA:N0} A — 240-4";
            else if (cita.Referencia == "240-4(d)")
                yield return $"Conductor aumentado por el tope de protección de calibres pequeños — 240-4(d)";
            else if (cita.Referencia == "Tabla 9" && cita.Descripcion.Contains("excedía"))
                yield return $"Conductor aumentado por caída de tensión — {cita.Descripcion}";
        }
    }

    private static string PorQueLaTerminal(decimal proteccionA, bool marcadas75C, int terminalC) =>
        proteccionA > 100m ? "(protección > 100 A) — 110-14(c)(1)b."
        : !marcadas75C ? "(protección ≤ 100 A) — 110-14(c)(1)a."
        : terminalC == 75 ? "(equipo marcado 75 °C) — 110-14(c)(1)a.(3)"
        : "(conductor de 60 °C con equipo marcado 75 °C) — 110-14(c)(1)a.(1)";

    /// <summary>El texto de un tooltip: una línea por paso.</summary>
    public static string ComoTexto(IEnumerable<string> lineas) => string.Join("\n", lineas);
}
