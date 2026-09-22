namespace PowerNode.DesignSuite.Calculo.Tableros;

/// <summary>Columna física del tablero. Los espacios nones van a la izquierda y los pares a la derecha.</summary>
public enum LadoTablero
{
    Izquierda,
    Derecha
}

/// <summary>Un espacio físico del interior del tablero, ya resuelto a su lado y su barra.</summary>
public sealed record EspacioBarra(int Numero, LadoTablero Lado, char Fase);

/// <summary>
/// Cómo está construido el bus del tablero. <b>Decide si la geometría de
/// <see cref="DistribucionBarras"/> aplica o no</b>, y por eso existe: no todos los tableros se
/// arman igual, y tratarlos igual inventa conexiones que no existen.
/// </summary>
public enum SistemaBarras
{
    /// <summary>
    /// Dos columnas de espacios numerados, nones a la izquierda y pares a la derecha, con las barras
    /// rotando por pares (convención NEMA). Es lo que traen los tableros de alumbrado y distribución
    /// NQ y NF, y es <b>lo único que sabe representar <see cref="DistribucionBarras"/></b>.
    /// </summary>
    ColumnasNema,

    /// <summary>
    /// Bus vertical en sándwich, de los tableros I-Line. Los interruptores derivados se sujetan con
    /// garra a la altura que se quiera, así que <b>no hay "espacios" numerados</b>: hay pulgadas de
    /// montaje disponibles, y qué fases toca cada derivado depende de a qué altura quedó y de
    /// cuántos polos tiene.
    ///
    /// El número de "circuitos" que publica el catálogo es cuántos derivados caben, no espacios de
    /// un polo. <b>Pasarlo por <see cref="DistribucionBarras"/> produciría un interior inventado</b>
    /// —lados y fases de una cuadrícula que este tablero no tiene—, y saldría igual de convincente
    /// que uno real. Por eso el editor de gabinete lo tiene que rechazar en vez de dibujarlo.
    /// </summary>
    ILine,
}

/// <summary>
/// Cómo se reparten los espacios físicos de un tablero de alumbrado y distribución entre las
/// barras de fase. Es geometría del interior del tablero, no una decisión de diseño: dónde
/// atornillas el interruptor **determina** a qué barra queda conectado.
///
/// Numeración estándar de dos columnas (la que traen las "cintas numeradas pares e impares" de los
/// tableros NQ/NF): los nones a la izquierda y los pares a la derecha, alternando hacia abajo —
/// 1 izq, 2 der, 3 izq, 4 der… Por eso un interruptor multipolar ocupa espacios de dos en dos del
/// MISMO lado (N, N+2, N+4): así se apilan los polos en una columna física.
///
/// Todo el conocimiento de "qué barra toca cada espacio" vive aquí y en ningún otro lado, para que
/// cambiarlo sea cambiar una función y no perseguirlo por la UI, el editor de gabinete y el
/// cálculo de desbalanceo.
/// </summary>
public static class DistribucionBarras
{
    /// <summary>
    /// Las barras energizadas del tablero.
    ///
    /// <para>
    /// <b>Recibe el SISTEMA (fases + hilos), no solo las fases</b>, y ése es el arreglo del
    /// 2026-08-21: las fases por sí solas no dicen cuántas barras hay. Un <c>1F-3H</c> tiene
    /// <b>dos</b> barras y un <c>1F-2H</c> tiene una, y las dos cosas son «1 fase». Antes esta
    /// función devolvía una sola barra para las dos, y con eso 36 modelos del catálogo no admitían
    /// un interruptor de 2 polos. La regla vive en <see cref="SistemaDelTablero"/>.
    /// </para>
    /// </summary>
    public static IReadOnlyList<char> BarrasDe(SistemaTablero sistema) => SistemaDelTablero.Barras(sistema);

    public static LadoTablero LadoDe(int espacio) =>
        espacio % 2 == 1 ? LadoTablero.Izquierda : LadoTablero.Derecha;

    /// <summary>
    /// La barra que toca un espacio, según la convención NEMA: **la rotación avanza por pares de
    /// espacios**, porque los dos espacios que quedan a la misma altura física (uno de cada
    /// columna) muerden la misma barra.
    ///
    /// Trifásico: 1 y 2 → A, 3 y 4 → B, 5 y 6 → C, 7 y 8 → A…
    /// Bifásico:  1 y 2 → A, 3 y 4 → B, 5 y 6 → A…
    ///
    /// Consecuencia: un interruptor de 3 polos del lado izquierdo ocupa los espacios 1, 3 y 5, que
    /// caen en A, B y C — las tres barras. Y uno del lado derecho en 8, 10 y 12 cae en A, B y C
    /// también (es el circuito 03 del unifilar de referencia del usuario, un 3×70 A).
    /// </summary>
    public static char FaseDe(int espacio, SistemaTablero sistema)
    {
        if (espacio < 1)
            throw new ArgumentOutOfRangeException(nameof(espacio), espacio, "Los espacios se numeran desde 1.");

        var barras = BarrasDe(sistema);
        return barras[((espacio - 1) / 2) % barras.Count];
    }

    /// <summary>
    /// Los espacios que ocupa un interruptor de <paramref name="polos"/> polos montado a partir de
    /// <paramref name="espacioInicial"/>: de dos en dos, del mismo lado.
    ///
    /// <para>
    /// <b>El máximo es 3, y es el número de FASES.</b> Aquí llegó a admitir 4 el 2026-08-20, contando
    /// el neutro conmutado como un espacio más, y estaba mal: lo corrigió el usuario en el momento —
    /// <i>«nunca va un neutro en los espacios de las barras de un gabinete»</i>. Los espacios
    /// numerados de esta clase <b>son</b> la barra de fases; el neutro va a la barra de neutros, que
    /// no se numera ni se reparte. Un interruptor con neutro conmutado ocupa los espacios de sus
    /// fases y ni uno más — el propio catálogo siembra el <c>QO215SWN</c> como 2 polos, 2 espacios.
    /// </para>
    /// </summary>
    public static IReadOnlyList<int> EspaciosQueOcupa(int espacioInicial, int polos)
    {
        if (espacioInicial < 1)
            throw new ArgumentOutOfRangeException(nameof(espacioInicial), espacioInicial, "Los espacios se numeran desde 1.");
        if (polos is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(polos), polos, "Un interruptor derivado es de 1, 2 o 3 polos.");

        return [.. Enumerable.Range(0, polos).Select(i => espacioInicial + 2 * i)];
    }

    /// <summary>
    /// Las barras que toca ese interruptor, en el orden en que las toca. Es lo que consume el
    /// cálculo de desbalanceo: ya no se captura a mano, se deduce de dónde está montado.
    /// </summary>
    public static string FasesQueOcupa(int espacioInicial, int polos, SistemaTablero sistema) =>
        new([.. EspaciosQueOcupa(espacioInicial, polos).Select(e => FaseDe(e, sistema))]);

    /// <summary>
    /// Dibuja el interior completo: cada espacio con su lado y su barra, de 1 a
    /// <paramref name="numeroEspacios"/>.
    /// </summary>
    public static IReadOnlyList<EspacioBarra> Interior(int numeroEspacios, SistemaTablero sistema)
    {
        if (numeroEspacios < 0)
            throw new ArgumentOutOfRangeException(nameof(numeroEspacios), numeroEspacios, "No puede ser negativo.");

        return [.. Enumerable.Range(1, numeroEspacios)
            .Select(n => new EspacioBarra(n, LadoDe(n), FaseDe(n, sistema)))];
    }

    /// <summary>
    /// ¿Esta geometría aplica al tablero? Solo la de dos columnas con rotación por pares. Un I-Line
    /// no tiene espacios numerados que repartir, así que dibujarle un interior con
    /// <see cref="Interior"/> sería inventarle lados y fases.
    ///
    /// Existe para que quien vaya a pintar la cuadrícula tenga que preguntar primero, en vez de
    /// llamar a <see cref="Interior"/> con un número de "circuitos" que significa otra cosa.
    /// </summary>
    public static bool RepresentaElInterior(SistemaBarras sistema) => sistema == SistemaBarras.ColumnasNema;

    /// <summary>
    /// ¿Un interruptor de estos polos tiene sentido en un tablero de estas fases? Un 3 polos en un
    /// tablero bifásico no: solo hay dos barras que tocar, así que repetiría fase. Es un error de
    /// captura, no un caso raro.
    /// </summary>
    public static bool PolosValidos(int polos, SistemaTablero sistema) =>
        polos >= 1 && polos <= BarrasDe(sistema).Count;

    /// <summary>
    /// ¿Cabe ese interruptor en el tablero? Un 3 polos que arranca en el espacio 40 de un tablero
    /// de 42 necesitaría el 44, que no existe.
    /// </summary>
    public static bool CabeEnElTablero(int espacioInicial, int polos, int numeroEspacios) =>
        EspaciosQueOcupa(espacioInicial, polos)[^1] <= numeroEspacios;
}
