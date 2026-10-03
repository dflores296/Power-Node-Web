using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// <b>La protección de un derivado, escogida dentro de su rango</b> — Power Node Web, M-20, decisión
/// <c>proteccion-de-motores-por-rango.md</c> (CONFIRMADA · David · 2026-10-03). Cuando la norma da un
/// techo («no exceda»), la protección no tiene que ser ese techo: el rango va del menor valor de la serie
/// que lleva la capacidad mínima del conductor al mayor que no excede el techo. Lo usan el derivado de un
/// motor (430-52(c)(1)), el de un equipo de A/C (440-22(a), 440-4(b)) y el de un variador (110-3(b)).
/// </summary>
internal static class ProteccionDentroDelRango
{
    /// <summary>La serie que se instala; arriba de su último valor (riel DIN pasa de 125 A), los de la NOM.</summary>
    internal static IReadOnlyList<decimal> Serie(ITablaProteccionEstandar tabla) =>
        tabla.ValoresEstandar.Count == 0
            ? tabla.ValoresDeLaNorma
            : [.. tabla.ValoresEstandar, .. tabla.ValoresDeLaNorma.Where(v => v > tabla.ValoresEstandar[^1])];

    /// <summary>
    /// <b>El rango</b>: del menor valor de la serie que lleva <paramref name="capacidadMinimaA"/> —nunca
    /// arriba del máximo— a <paramref name="maximoA"/>. Con <paramref name="soloArribaDeA"/> —el proyectista
    /// declaró que no arranca con ese valor (430-52(c)(1) Excepción 2, el 225 % de 440-22(a))—, el rango
    /// empieza arriba de él; si no hay un tamaño mayor, es solo el máximo.
    /// </summary>
    internal static (decimal Minimo, IReadOnlyList<decimal> Valores) Rango(
        ITablaProteccionEstandar tabla, decimal capacidadMinimaA, decimal maximoA, decimal? soloArribaDeA = null)
    {
        var serie = Serie(tabla);
        var minimo = soloArribaDeA is { } arriba
            ? serie.Where(v => v > arriba && v <= maximoA).DefaultIfEmpty(maximoA).Min()
            : Math.Min(tabla.SiguienteEstandar(capacidadMinimaA), maximoA);
        return (minimo, [.. serie.Where(v => v >= minimo && v <= maximoA).Append(minimo).Append(maximoA).Distinct().Order()]);
    }

    /// <summary>El valor del rango más cercano a <paramref name="pedidaA"/>; entre dos igual de cerca, el menor.</summary>
    internal static decimal MasCercano(IReadOnlyList<decimal> valores, decimal pedidaA) =>
        valores.OrderBy(v => Math.Abs(v - pedidaA)).ThenBy(v => v).First();

    /// <summary>
    /// <b>El conductor queda protegido por la protección</b> según 240-4: su ampacidad la cubre; o no
    /// es valor normalizado de 240-6(a) y la protección es la inmediata superior, hasta 800 A — 240-4(b);
    /// y sin pasar el tope de los conductores chicos — 240-4(d). La lista de 240-6(a), no la serie.
    /// </summary>
    internal static bool Protege(ITablaProteccionEstandar tabla, decimal proteccionA, decimal ampacidadA, Calibre calibre, MaterialConductor material) =>
        (proteccionA <= ampacidadA
         || (proteccionA <= 800m && !tabla.ValoresDeLaNorma.Contains(ampacidadA) && proteccionA == tabla.SiguienteDeLaNorma(ampacidadA)))
        && (SeleccionConductor.TopeProteccion2404d(calibre, material) is not { } tope || proteccionA <= tope);

    /// <summary>
    /// La protección con que se elige la columna de terminales: con prioridad al conductor, el piso del
    /// rango (la protección se queda en su regla de 110-14(c)(1), paso 8 de M-20); en manual, el valor
    /// escogido; si no, el máximo.
    /// </summary>
    internal static (decimal Inicial, decimal ParaLaColumna) Inicial(
        CriterioProteccionMotor criterio, decimal? elegidaA, decimal minimoA, decimal maximoA, IReadOnlyList<decimal> valores)
    {
        var inicial = criterio == CriterioProteccionMotor.Manual && elegidaA is { } pedida ? MasCercano(valores, pedida) : maximoA;
        return (inicial, criterio == CriterioProteccionMotor.Conductor ? minimoA : inicial);
    }

    /// <summary>Lo que salió de <see cref="Escoger"/>.</summary>
    internal sealed record Escogida(
        decimal ProteccionA, SeleccionConductor.Resultado Seleccion,
        bool TopadoEn100A, bool SubioElCalibre, Calibre? CalibreProtegido, decimal? AmpacidadProtegidaA);

    /// <summary>
    /// <b>El conductor y, con prioridad al conductor, la protección</b> — M-20, pasos 5 a 7: el mayor valor
    /// del rango que protege al calibre base (el de la ampacidad, antes de subir por caída) según 240-4,
    /// 240-4(b) y 240-4(d). Subir el calibre por caída no sube la protección. Si ninguno lo protege (los
    /// factores lo dejaron abajo del piso), el piso, y el calibre sube hasta quedar protegido por él. Con
    /// terminal de 60 °C, no pasa de 100 A: recalcular con la columna de la protección no termina (30 HP a
    /// 220 V oscila entre 1 AWG con 110 A y 3 AWG con 100 A). Con otro criterio, el conductor de siempre.
    /// </summary>
    /// <param name="seleccionar">El conductor sin protección (<c>null</c>) o protegido por ella según 240-4.</param>
    /// <param name="ampacidadUtilizable">La de un calibre en la columna de terminales, con factores, por conductor.</param>
    internal static Escogida Escoger(
        CriterioProteccionMotor criterio, decimal inicialA, decimal minimoA, IReadOnlyList<decimal> valores,
        TemperaturaAislamiento tempTerminales, ITablaProteccionEstandar tabla, MaterialConductor material,
        Func<decimal?, SeleccionConductor.Resultado> seleccionar, Func<Calibre, decimal?> ampacidadUtilizable)
    {
        var seleccion = seleccionar(null);
        if (criterio != CriterioProteccionMotor.Conductor)
            return new(inicialA, seleccion, false, false, null, null);

        var calibreBase = seleccion.CalibreBase;
        var ampacidadBase = (ampacidadUtilizable(calibreBase) ?? 0m) * seleccion.NumeroConductoresParalelo;
        var tope = tempTerminales == TemperaturaAislamiento.T60 ? 100m : decimal.MaxValue;
        var protegen = valores.Where(v => Protege(tabla, v, ampacidadBase, calibreBase, material)).ToList();
        var topado = protegen.Any(v => v > tope);
        protegen.RemoveAll(v => v > tope);
        var subio = protegen.Count == 0;
        var proteccion = subio ? minimoA : protegen.Max();
        seleccion = seleccionar(proteccion);
        // El que se protegió es el base (si subió por caída, el más grueso sigue protegido).
        var protegidaA = (ampacidadUtilizable(seleccion.CalibreBase) ?? 0m) * seleccion.NumeroConductoresParalelo;
        return new(proteccion, seleccion, topado, subio, seleccion.CalibreBase, protegidaA);
    }

    /// <summary>
    /// El rango armado, con lo que se escogió en él y si el conductor instalado queda protegido.
    /// <paramref name="arranque"/> recibe la protección escogida y el máximo, y dice qué verificar cuando
    /// la protección queda abajo del máximo; <c>null</c> si no hay nada que verificar.
    /// </summary>
    internal static RangoDeProteccion Armar(
        string regla, string techo, string piso, string sobrecarga, decimal capacidadMinimaA,
        decimal minimoA, decimal maximoA, decimal maximoPermitidoA, IReadOnlyList<decimal> valores,
        CriterioProteccionMotor criterio, decimal? elegidaA, decimal? soloArribaDeA, Escogida e,
        ITablaProteccionEstandar tabla, MaterialConductor material, Func<decimal, decimal, Cita> arranque,
        ProteccionDeMotor? tabla430_52 = null)
    {
        var s = e.Seleccion;
        var protege = Protege(tabla, e.ProteccionA, s.AmpacidadUtilizableTotalA, s.CalibreFase, material);
        return new RangoDeProteccion(
            Regla: regla, Techo: techo, Piso: piso, Sobrecarga: sobrecarga,
            CapacidadMinimaA: capacidadMinimaA, MinimoA: minimoA, MaximoA: maximoA, MaximoPermitidoA: maximoPermitidoA,
            Valores: valores, Criterio: criterio, ProteccionA: e.ProteccionA,
            ProtegeAlConductor: protege,
            PorExcepcion240_4b: protege && e.ProteccionA > s.AmpacidadUtilizableTotalA,
            ArribaDeA: soloArribaDeA,
            TopadoEn100A: e.TopadoEn100A,
            SubioElCalibre: e.SubioElCalibre,
            PedidaA: criterio == CriterioProteccionMotor.Manual && elegidaA is { } p && p != e.ProteccionA ? p : null,
            CalibreProtegido: e.CalibreProtegido,
            AmpacidadProtegidaA: e.AmpacidadProtegidaA,
            Tabla430_52: tabla430_52,
            Arranque: e.ProteccionA < maximoA ? arranque(e.ProteccionA, maximoA) : null);
    }

    /// <summary>Las citas del rango, del criterio, de la protección del conductor y del arranque — M-20.</summary>
    internal static IEnumerable<Cita> Citas(RangoDeProteccion r, Calibre calibre, decimal ampacidadA)
    {
        yield return new Cita(r.Regla, r.Valores.Count == 1
            ? $"Rango permitido: solo {r.MaximoA:0.##} A" + (r.ArribaDeA is not null
                ? $" (se declaró que no arranca con menos)"
                : $" (ningún valor de la serie entre el {r.Piso} y el máximo)")
            : $"Rango permitido: {r.MinimoA:0.##} A a {r.MaximoA:0.##} A — " + (r.ArribaDeA is { } arriba
                ? $"arriba de {arriba:0.##} A, con que se declaró que no arranca, al mayor que no excede {r.Techo}"
                : $"del menor de la serie que lleva el {r.Piso} al mayor que no excede {r.Techo}") +
              ". La protección «no debe exceder» ese máximo: cualquiera del rango cumple");

        yield return r.Criterio switch
        {
            CriterioProteccionMotor.Conductor => new Cita("240-4",
                r.SubioElCalibre
                    ? $"Criterio «prioridad al conductor»: ningún valor del rango protegía al calibre por ampacidad; {r.ProteccionA:0.##} A, el mínimo, y el calibre sube hasta quedar protegido"
                    : $"Criterio «prioridad al conductor»: {r.ProteccionA:0.##} A, el mayor valor del rango que protege a {r.CalibreProtegido ?? calibre} " +
                      $"({r.AmpacidadProtegidaA ?? ampacidadA:0.##} A), el calibre por ampacidad" +
                      (r.CalibreProtegido is { } cp && cp.Designacion != calibre.Designacion ? $"; {calibre} por caída de tensión sigue protegido" : "") +
                      (r.TopadoEn100A ? "; sin pasar de 100 A: la terminal se queda en 60 °C — 110-14(c)(1)a." : "")),
            CriterioProteccionMotor.Manual => new Cita(r.Regla,
                $"Criterio «manual»: {r.ProteccionA:0.##} A, la que escogió el proyectista dentro del rango" +
                (r.PedidaA is { } pedida ? $" (pidió {pedida:0.##} A, fuera del rango o de la serie: la más cercana)" : "")),
            _ => new Cita(r.Regla, $"Criterio «máximo»: {r.ProteccionA:0.##} A, el mayor del rango"),
        };

        yield return r.ProtegeAlConductor
            ? new Cita(r.PorExcepcion240_4b ? "240-4(b)" : "240-4",
                $"El interruptor de {r.ProteccionA:0.##} A protege a {calibre} ({ampacidadA:0.##} A) según su ampacidad" +
                (r.PorExcepcion240_4b ? ": un escalón arriba de una ampacidad que no es valor normalizado" : ""))
            : new Cita("240-4(g)",
                $"{r.ProteccionA:0.##} A pasa la ampacidad de {calibre} ({ampacidadA:0.##} A): lo permite 240-4(g) porque {r.Sobrecarga}");

        if (r.Arranque is { } arranque)
            yield return arranque;
    }
}

/// <summary>
/// <b>El rango de la protección de un derivado y lo que se escogió en él</b> — Power Node Web, M-20. Ver
/// <see cref="ProteccionDentroDelRango"/>.
/// </summary>
/// <param name="Regla">La que pone el techo: 430-52(c)(1), 440-22(a), 440-4(b) o 110-3(b).</param>
/// <param name="Techo">Qué no se debe exceder, en palabras: «el valor de la Tabla 430-52».</param>
/// <param name="Piso">De dónde sale el mínimo: «125 % de la FLC = 11.13 A».</param>
/// <param name="Sobrecarga">Quién da la sobrecarga cuando la protección pasa la ampacidad del conductor (240-4(g)).</param>
/// <param name="CapacidadMinimaA">La del conductor. De ella sale el mínimo.</param>
/// <param name="MinimoA">El menor valor del rango.</param>
/// <param name="MaximoA">El mayor valor del rango: lo que daba la app antes de M-20.</param>
/// <param name="MaximoPermitidoA">
/// <b>Lo que entra a 430-62(a) y 430-63(1)</b>: «el valor máximo permitido … de acuerdo con 430-52 ó
/// 440-22(a)», no la protección escogida (M-20, pregunta 6). El de la lista de 240-6(a) —en riel DIN, 35 A
/// aunque se instalen 32—, o el que marca la placa o el fabricante.
/// </param>
/// <param name="Valores">Los de la serie entre los dos, de menor a mayor: lo que se puede escoger.</param>
/// <param name="ProteccionA">La que quedó.</param>
/// <param name="ProtegeAlConductor">El conductor que se instala queda protegido por ella según 240-4.</param>
/// <param name="PorExcepcion240_4b">Protegido, pero un escalón arriba de su ampacidad — 240-4(b).</param>
/// <param name="ArribaDeA">Se declaró que no arranca con este valor: el rango empieza arriba de él.</param>
/// <param name="TopadoEn100A">Prioridad al conductor: un valor mayor de 100 A lo protegía, pero la terminal es de 60 °C.</param>
/// <param name="SubioElCalibre">Prioridad al conductor: ninguno lo protegía; quedó el mínimo y el calibre subió.</param>
/// <param name="PedidaA">Manual: la que se pidió, si no estaba en el rango.</param>
/// <param name="CalibreProtegido">Prioridad al conductor: el calibre por ampacidad, el que la protección cubre.</param>
/// <param name="AmpacidadProtegidaA">La ampacidad utilizable de <paramref name="CalibreProtegido"/>.</param>
/// <param name="Tabla430_52">Solo en un motor: el techo de la tabla, sus Excepciones y el mayor de la serie que no lo excede.</param>
/// <param name="Arranque">Con la protección abajo del máximo: qué verificar del arranque, y hasta dónde subir.</param>
public sealed record RangoDeProteccion(
    string Regla,
    string Techo,
    string Piso,
    string Sobrecarga,
    decimal CapacidadMinimaA,
    decimal MinimoA,
    decimal MaximoA,
    decimal MaximoPermitidoA,
    IReadOnlyList<decimal> Valores,
    CriterioProteccionMotor Criterio,
    decimal ProteccionA,
    bool ProtegeAlConductor,
    bool PorExcepcion240_4b,
    decimal? ArribaDeA = null,
    bool TopadoEn100A = false,
    bool SubioElCalibre = false,
    decimal? PedidaA = null,
    Calibre? CalibreProtegido = null,
    decimal? AmpacidadProtegidaA = null,
    ProteccionDeMotor? Tabla430_52 = null,
    Cita? Arranque = null)
{
    /// <summary>La que quedó es el máximo del rango.</summary>
    public bool EsElMaximo => ProteccionA == MaximoA;
}
