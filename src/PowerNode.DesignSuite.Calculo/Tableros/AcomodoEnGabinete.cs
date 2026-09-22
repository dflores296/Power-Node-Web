namespace PowerNode.DesignSuite.Calculo.Tableros;

/// <summary>Un interruptor ya montado: dónde empieza y cuántos espacios se come.</summary>
/// <param name="EspacioInicial">El número de espacio donde arranca.</param>
/// <param name="Espacios">Los espacios que se come: sus polos de FASE. El neutro conmutado no cuenta — no muerde barra.</param>
/// <param name="Etiqueta">Cómo se le nombra al reportar un choque.</param>
public readonly record struct MontajeEnGabinete(int EspacioInicial, int Espacios, string Etiqueta);

/// <summary>
/// Si un interruptor cabe donde se le quiere poner. <b>Es la lógica del arrastre</b> del editor de
/// gabinete, y vive aquí —pura y probada— en vez de dentro del ViewModel, por la misma razón que
/// <see cref="DistribucionBarras"/>: mover un circuito de espacio le cambia las barras que toca, y
/// eso alimenta el desbalanceo del tablero y las columnas del cuadro de carga. Un acomodo mal
/// validado no truena: sale impreso.
///
/// <para>
/// <b>Lo que esta clase NO hace es decidir las barras.</b> Solo dice si el hueco existe y está
/// libre; qué fase toca cada espacio lo sigue resolviendo <see cref="DistribucionBarras"/>, que es
/// el único lugar del programa que lo sabe.
/// </para>
/// </summary>
public static class AcomodoEnGabinete
{
    /// <summary>
    /// Por qué NO se puede mover ese interruptor al espacio destino, o <c>null</c> si sí se puede.
    ///
    /// <para>
    /// Devuelve el motivo redactado y no un booleano a propósito: el usuario acaba de arrastrar algo
    /// y soltar sin explicación se lee como que el programa se trabó. Y el motivo es siempre
    /// concreto — qué espacio hace falta, o quién lo está ocupando.
    /// </para>
    /// </summary>
    /// <param name="espacioDestino">El espacio donde se quiere que empiece.</param>
    /// <param name="espacios">Polos de fase del interruptor que se mueve.</param>
    /// <param name="numeroEspacios">Capacidad del tablero.</param>
    /// <param name="ocupados">Lo que ya está montado. El propio interruptor que se mueve NO debe venir aquí.</param>
    public static string? MotivoNoCabe(
        int espacioDestino,
        int espacios,
        int numeroEspacios,
        IEnumerable<MontajeEnGabinete> ocupados)
    {
        if (espacioDestino < 1)
            return "Los espacios se numeran desde 1.";

        if (espacios is < 1 or > 3)
            return $"Un interruptor derivado ocupa de 1 a 3 espacios, no {espacios}.";

        var pedidos = DistribucionBarras.EspaciosQueOcupa(espacioDestino, espacios);

        // El desborde primero: si ni siquiera cabe en el tablero, decir eso y no de quién es el
        // espacio que le falta -- ese espacio no existe.
        if (pedidos[^1] > numeroEspacios)
            return $"Un interruptor de {espacios} polos desde el espacio {espacioDestino} necesita hasta el " +
                   $"{pedidos[^1]}, y el tablero solo tiene {numeroEspacios}.";

        foreach (var otro in ocupados)
        {
            var suyos = DistribucionBarras.EspaciosQueOcupa(otro.EspacioInicial, otro.Espacios);
            var choque = pedidos.Intersect(suyos).OrderBy(n => n).ToList();

            if (choque.Count > 0)
                return choque.Count == 1
                    ? $"El espacio {choque[0]} lo ocupa {otro.Etiqueta}."
                    : $"Los espacios {string.Join(", ", choque)} los ocupa {otro.Etiqueta}.";
        }

        return null;
    }

    /// <summary>
    /// El primer espacio donde ese interruptor sí cabe, o <c>null</c> si no cabe en ninguno.
    ///
    /// <b>Respeta el lado.</b> Un multipolar se monta de dos en dos del mismo lado, así que buscar de
    /// uno en uno mandaría un 3 polos de la columna izquierda a la derecha sin que nadie lo pidiera —
    /// y con eso le cambiarían las barras. Se busca dentro del mismo lado del que salió.
    /// </summary>
    public static int? PrimerHuecoDelMismoLado(
        int espacioActual,
        int espacios,
        int numeroEspacios,
        IEnumerable<MontajeEnGabinete> ocupados)
    {
        var lista = ocupados.ToList();
        var primeroDelLado = espacioActual % 2 == 1 ? 1 : 2;

        for (var n = primeroDelLado; n <= numeroEspacios; n += 2)
            if (MotivoNoCabe(n, espacios, numeroEspacios, lista) is null)
                return n;

        return null;
    }

    /// <summary>
    /// <b>Dónde nace el interruptor principal cuando va montado entre los derivados:</b> lo más abajo
    /// posible de la <b>columna par</b> (la derecha).
    ///
    /// <para>
    /// <b>Es la costumbre de obra, dictada por el usuario</b> — <i>«que ocupe los últimos espacios
    /// pares del lado derecho»</i>, y ya estaba anotada como observación desde el 2026-08-20
    /// (<i>«casi siempre cuando se coloca dentro de los espacios lo ponen hasta abajo»</i>) sin que
    /// nadie la programara: el principal nacía en el <b>primer</b> hueco, o sea arriba del todo,
    /// justo al revés.
    /// </para>
    ///
    /// <para>
    /// En un tablero de 18 espacios, un principal de 3 polos nace en <b>14</b> y ocupa 14‑16‑18. Uno
    /// de 2 polos nace en <b>16</b> y ocupa 16‑18.
    /// </para>
    ///
    /// <para>
    /// <b>Si abajo no cabe, sube por la misma columna</b> (12, 10, 8…) antes que cambiar de lado:
    /// mantener el principal en una sola columna es lo que hace que los derivados conserven la otra
    /// entera. Solo si toda la columna par está ocupada devuelve <c>null</c>, y entonces el espacio
    /// se queda en 0 — sin colocar pero existiendo, para que el editor lo reporte como pendiente en
    /// vez de perderlo.
    /// </para>
    /// </summary>
    /// <param name="espacios">Los espacios que ocupa: los polos del principal, que son las barras del tablero.</param>
    public static int? UltimoHuecoDeLaColumnaPar(
        int espacios,
        int numeroEspacios,
        IEnumerable<MontajeEnGabinete> ocupados)
    {
        var lista = ocupados.ToList();

        // El último arranque PAR que no se desborda: para 3 polos en 18 espacios es el 14, porque
        // necesita hasta el 18. De ahí hacia arriba, de dos en dos, sin cambiar de columna.
        var ultimoArranque = numeroEspacios - 2 * (espacios - 1);
        if (ultimoArranque % 2 == 1)
            ultimoArranque--;

        for (var n = ultimoArranque; n >= 2; n -= 2)
            if (MotivoNoCabe(n, espacios, numeroEspacios, lista) is null)
                return n;

        return null;
    }
}
