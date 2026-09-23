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
    /// del mínimo de 15/20 A de alumbrado y contactos: si difiere de la protección, fue el mínimo.</param>
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
        IReadOnlyList<Cita> citas)
    {
        var proteccion = new List<string>
        {
            $"In = {iContinuaA:N2} A (continua) + {iNoContinuaA:N2} A (no continua) = {iContinuaA + iNoContinuaA:N2} A",
            $"Capacidad mínima = {factorContinua * 100m:0} % × {iContinuaA:N2} A + {iNoContinuaA:N2} A = {d.CapacidadMinimaA:N2} A — {articuloProteccion}",
            $"Protección: {proteccionSinMinimo:N0} A, el primer tamaño que alcanza en «{datos.SerieInterruptores.Nombre()}» — 240-6(a)",
        };
        if (proteccionA != proteccionSinMinimo)
            proteccion.Add($"Sube a {proteccionA:N0} A por el mínimo de protección del tipo de carga");

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

        // Las dos revisiones de 210-19(a)(1) / 215-2(a)(1), por separado: el 125 % contra la tabla
        // SIN factores (en la columna de la terminal), la carga al 100 % contra la corregida.
        var carga = iContinuaA + iNoContinuaA;
        if (deTablaTerminal is { } tabla)
        {
            var tablaTotal = tabla * conductoresPorFase;
            conductor.Add(
                $"Antes de factores: {tablaTotal:N2} A a {d.TemperaturaTerminalesC} °C ≥ capacidad mínima {d.CapacidadMinimaA:N2} A " +
                $"{(tablaTotal >= d.CapacidadMinimaA ? "✔" : "✘")} — {articuloConductor}");
        }
        conductor.Add(
            $"Con factores: {d.AmpacidadConductorA:N2} A ≥ carga {carga:N2} A {(d.AmpacidadConductorA >= carga ? "✔" : "✘")} — {articuloConductor}");

        // Por qué el conductor terminó más grueso de lo que pedía la capacidad: lo dice el motor en
        // sus citas, y aquí solo se repite en corto.
        foreach (var cita in citas)
        {
            if (cita.Referencia == "240-4(b)")
                conductor.Add($"Protección {proteccionA:N0} A sobre {d.AmpacidadConductorA:N2} A: permitido, el estándar inmediato superior — 240-4(b)");
            else if (cita.Referencia == "240-4")
                conductor.Add($"Subió para que la protección de {proteccionA:N0} A proteja al conductor — 240-4");
            else if (cita.Referencia == "240-4(d)")
                conductor.Add($"Subió por el tope de protección de calibres pequeños — 240-4(d)");
            else if (cita.Referencia == "Tabla 9" && cita.Descripcion.Contains("excedía"))
                conductor.Add($"Subió por caída de tensión — {cita.Descripcion}");
        }

        return new DesgloseDeSeleccion(proteccion, conductor);
    }

    private static string PorQueLaTerminal(decimal proteccionA, bool marcadas75C, int terminalC) =>
        proteccionA > 100m ? "por ser de más de 100 A — 110-14(c)(1)b."
        : !marcadas75C ? "por ser de 100 A o menos — 110-14(c)(1)a."
        : terminalC == 75 ? "por equipo marcado 75 °C — 110-14(c)(1)a.(3)"
        : "aunque el equipo esté marcado 75 °C: el conductor es de 60 °C — 110-14(c)(1)a.(1)";

    /// <summary>El texto de un tooltip: una línea por paso.</summary>
    public static string ComoTexto(IEnumerable<string> lineas) => string.Join("\n", lineas);
}
