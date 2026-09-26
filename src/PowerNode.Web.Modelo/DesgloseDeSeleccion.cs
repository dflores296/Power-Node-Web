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
                  $"(125 % × {motores.MayorFlcA:N2} A + {motores.SumaRestoFlcA:N2} A) = {d.CapacidadMinimaA:N2} A — {articuloProteccion}, 430-24"
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
    /// <param name="fuente">«Tabla 430-250, columna de 230 V (220 V: intervalo de 220 a 240 V)».</param>
    /// <param name="porcentaje">El de la Tabla 430-52: 250 % con interruptor de tiempo inverso.</param>
    internal static DesgloseDeSeleccion DeMotor(
        ITablaAmpacidad ampacidad,
        DatosDelTablero datos,
        decimal hp,
        decimal flcA,
        string fuente,
        decimal porcentaje,
        decimal proteccionA,
        Calibre calibre,
        int conductoresPorFase,
        DetalleDelCalculo d,
        IReadOnlyList<Cita> citas)
    {
        var techo = flcA * porcentaje / 100m;
        var proteccion = new List<string>
        {
            $"FLC = {flcA:N2} A, {MotoresEnHp.Texto(hp)} HP — {fuente}, 430-6(a)",
            $"Máximo = {porcentaje:0} % × {flcA:N2} A = {techo:N2} A, interruptor de tiempo inverso — Tabla 430-52",
            $"Protección: {proteccionA:N0} A, " +
                (proteccionA == techo ? "igual al máximo" : "tamaño inmediato superior al máximo") +
                $" en «{datos.SerieInterruptores.Nombre()}» — 430-52(c)(1) Excepción 1",
            "Sobrecarga del motor: relevador en el arrancador o protector del motor — 430-32",
        };

        var conductor = LineasDelConductor(ampacidad, datos, proteccionA, calibre, conductoresPorFase, d);
        conductor.Add(
            $"Con factores: {d.AmpacidadConductorA:N2} A ≥ 125 % × {flcA:N2} A = {d.CapacidadMinimaA:N2} A " +
            $"{(d.AmpacidadConductorA >= d.CapacidadMinimaA ? "✔" : "✘")} — 430-22");
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
