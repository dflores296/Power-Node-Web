namespace PowerNode.DesignSuite.Calculo.Validaciones;

/// <summary>La corriente de diseño de un circuito ya calculado y las fases que ocupa ("A", "AB", "ABC"...).</summary>
public sealed record CorrientePorCircuito(string Fases, decimal CorrienteA);

/// <summary>
/// Desbalanceo entre fases de un tablero: (max-min)/max × 100. Es un diagnóstico del reparto de
/// carga, no un criterio de selección de nada — no tiene artículo que lo exija, es la misma
/// fórmula que ya traía el Excel original (fila de totales, columna "DESBALANCEO %").
/// </summary>
public static class CalculadoraDesbalanceo
{
    public static decimal Porcentaje(IReadOnlyList<CorrientePorCircuito> circuitos, IReadOnlyList<char> fasesTablero)
    {
        if (fasesTablero.Count < 2) return 0m; // un tablero monofásico no tiene noción de desbalance

        var porFase = CorrientePorFase(circuitos, fasesTablero);

        var max = porFase.Values.Max();
        if (max == 0m) return 0m; // tablero sin carga todavía, nada que reportar

        var min = porFase.Values.Min();
        return (max - min) / max * 100m;
    }

    /// <summary>
    /// <b>La corriente que lleva cada barra</b>: la de cada circuito sumada, completa, en cada barra
    /// que toca. En un interruptor de 3 polos de 20 A circulan 20 A por cada línea, no un tercio por
    /// cada una — por eso se suma en corriente y no en VA.
    ///
    /// <para>
    /// Estaba dentro de <see cref="Porcentaje"/> y salió el 2026-09-23 (Power Node Web, hallazgo
    /// M-02), cuando el alimentador necesitó la corriente de la fase más cargada: <b>es la misma
    /// regla, y vive en un solo lugar</b>.
    /// </para>
    /// </summary>
    public static IReadOnlyDictionary<char, decimal> CorrientePorFase(
        IReadOnlyList<CorrientePorCircuito> circuitos, IReadOnlyList<char> fasesTablero)
    {
        var porFase = fasesTablero.ToDictionary(f => f, _ => 0m);
        foreach (var c in circuitos)
            foreach (var fase in c.Fases)
                if (porFase.ContainsKey(fase))
                    porFase[fase] += c.CorrienteA;
        return porFase;
    }

    /// <summary>
    /// Las barras entre las que se reparte la carga.
    ///
    /// <para>
    /// <b>Sale del SISTEMA (fases + hilos), no de las fases solas.</b> Tenía una copia propia de la
    /// tabla —<c>1 => ['A']</c>— con el mismo defecto que <c>DistribucionBarras.BarrasDe</c>: un
    /// <c>1F-3H</c> tiene DOS barras, y con una sola el desbalanceo de los 36 tableros monofásicos del
    /// catálogo salía <b>siempre 0</b>, porque todo caía en la barra A. Ahora delega en
    /// <see cref="Tableros.SistemaDelTablero"/>, que es el único lugar donde vive esa regla.
    /// </para>
    /// </summary>
    public static IReadOnlyList<char> FasesDe(Tableros.SistemaTablero sistema) =>
        Tableros.SistemaDelTablero.Barras(sistema);
}
