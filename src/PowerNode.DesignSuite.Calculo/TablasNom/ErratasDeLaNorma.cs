namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>
/// Una celda de una tabla de la norma cuyo <b>valor publicado en el DOF es incorrecto</b>, y el
/// valor con el que este programa calcula en su lugar.
/// </summary>
/// <param name="TablaId">La tabla, con el id que usa el corpus: "430-250".</param>
/// <param name="ClaveFila">Cómo se identifica la fila dentro de la tabla — el Hp en las de FLC.</param>
/// <param name="Columna">Índice de columna en la rejilla del corpus (el mismo que usa el lector).</param>
/// <param name="ValorPublicado">Lo que dice el DOF. Se <b>verifica</b> antes de corregir: ver <see cref="ErratasDeLaNorma"/>.</param>
/// <param name="ValorCorregido">Con lo que se calcula.</param>
/// <param name="Descripcion">Qué celda es, en palabras, para la memoria de cálculo.</param>
/// <param name="Sustento">Por qué el valor publicado no puede ser el correcto. Va al papel.</param>
public sealed record ErrataDeCelda(
    string TablaId,
    string ClaveFila,
    int Columna,
    decimal ValorPublicado,
    decimal ValorCorregido,
    string Descripcion,
    string Sustento);

/// <summary>
/// Una <b>fila</b> de una tabla de la norma cuyo <b>rótulo publicado en el DOF es incorrecto</b>, y
/// el intervalo con el que este programa la interpreta en su lugar.
///
/// <para>
/// <b>Es la misma clase de cosa que <see cref="ErrataDeCelda"/> y por eso vive junto a ella</b>
/// (unificado el 2026-08-20). Antes había dos capas de erratas con dos criterios distintos: ésta
/// estaba escondida en un diccionario privado de <c>RangoNorma</c>, no verificaba nada y no llegaba
/// al papel. <b>Gana el criterio de la capa nueva</b> — se verifica antes de corregir y se declara
/// en la memoria—, porque el otro es exactamente el defecto que esta capa existe para no cometer.
/// </para>
/// </summary>
/// <param name="TablaId">La tabla, con el id que usa el corpus.</param>
/// <param name="RotuloPublicado">El texto tal como lo imprime el DOF. La errata <b>solo</b> se aplica a ese texto exacto.</param>
/// <param name="Min">Límite inferior del intervalo con el que se calcula, inclusivo.</param>
/// <param name="Max">Límite superior, inclusivo.</param>
/// <param name="Descripcion">Qué fila es, en palabras, para la memoria de cálculo.</param>
/// <param name="Sustento">Por qué el rótulo publicado no puede ser el correcto. Va al papel.</param>
public sealed record ErrataDeRotulo(
    string TablaId,
    string RotuloPublicado,
    decimal Min,
    decimal Max,
    string Descripcion,
    string Sustento);

/// <summary>
/// <b>Las erratas conocidas del texto publicado de la NOM-001-SEDE-2012</b>, y la única puerta por
/// la que este programa se aparta de lo que dice el DOF.
///
/// <para>
/// <b>Por qué existe esta capa y no se corrige el dato de origen.</b> El corpus del repo público
/// <c>NOM-001-SEDE-2012</c> es una <b>reproducción fiel</b> del PDF del DOF —su propio README dice
/// que "ante cualquier discrepancia prevalece el texto del Diario Oficial de la Federación"— y esa
/// fidelidad es justamente lo que lo hace útil como referencia. Cambiar el número allá haría creer a
/// quien lo consulte que el DOF dice otra cosa. La corrección vive aquí, donde se calcula, declarada
/// y con su sustento, en vez de escondida en un dato.
/// </para>
///
/// <para>
/// <b>Se verifica antes de corregir.</b> Una errata solo se aplica si la celda trae <b>exactamente</b>
/// el valor publicado que la errata declara. Si ya trae el corregido, se entiende que la fuente se
/// arregló río arriba y no se hace nada. Si trae un tercer valor, se lanza excepción: significa que
/// la tabla cambió de forma y la errata apunta a otra celda — corregir a ciegas ahí sería
/// exactamente el error que esta capa existe para no cometer.
/// </para>
///
/// <para>
/// <b>Toda errata que se aplique tiene que llegar al papel.</b> El motor emite una cita cuando usa
/// un valor corregido, y esa cita entra al mismo rastro que sustenta el resto de la memoria. Una
/// memoria que se aparta del DOF sin decirlo no se puede verificar, y entonces no sirve para lo que
/// se entrega.
/// </para>
///
/// <para>
/// <b>El listón para agregar una errata es alto</b>, y no es "el número se ve raro": hace falta que
/// el valor publicado sea imposible por sus propios vecinos de tabla y que la física lo contradiga.
/// Ante la duda, no se corrige — prevalece el DOF.
/// </para>
/// </summary>
public static class ErratasDeLaNorma
{
    /// <summary>
    /// <b>Tabla 430-250, motor trifásico de 10 hp, columna de 575 V: el DOF publica 44 A.</b>
    ///
    /// <para>
    /// No es un error de transcripción: se verificó glifo por glifo contra el PDF del DOF (página
    /// 325) y ahí está el "44". Es la norma publicada la que trae el dato mal.
    /// </para>
    ///
    /// <para>
    /// <b>Que 44 es imposible lo dice la propia tabla</b>, por tres caminos independientes:
    /// </para>
    /// <list type="number">
    /// <item>La columna de 575 V tiene que ser <b>menor</b> que la de 460 V para el mismo motor —más
    /// tensión, menos corriente— y la de 460 V vale 14 A. Un 44 la triplica.</item>
    /// <item>La columna de 575 V crece con los Hp: 7½ hp → 9 A, <b>10 hp → 44 A</b>, 15 hp → 17 A.
    /// Es el único quiebre de monotonía en las tres tablas de FLC (se barrieron todas).</item>
    /// <item>El escalado entre columnas de la misma fila da el valor: 14 A × 460/575 = <b>11.2 A</b>,
    /// que redondea a los 11 A que trae la tabla equivalente del NEC (430.250).</item>
    /// </list>
    ///
    /// <para>
    /// <b>Consecuencia si no se corrige:</b> un motor de 10 hp en 575 V sale con conductor y
    /// protección <b>cuatro veces</b> más grandes de lo que toca. No es inseguro —sobra cobre— pero
    /// es un proyecto que no pasa una revisión y una memoria que no cuadra consigo misma.
    /// </para>
    /// </summary>
    public static readonly ErrataDeCelda Flc10HpEn575V = new(
        TablaId: "430-250",
        ClaveFila: "10",
        Columna: 7,
        ValorPublicado: 44m,
        ValorCorregido: 11m,
        Descripcion: "Tabla 430-250, 10 hp, columna de 575 V",
        Sustento: "el DOF publica 44 A, que excede la columna de 460 V de la misma fila (14 A) y "
            + "rompe la monotonía de su propia columna (7½ hp = 9 A, 15 hp = 17 A); "
            + "14 A × 460/575 = 11.2 A, que es el valor de la tabla equivalente del NEC 430.250");

    /// <summary>
    /// <b>Tabla 310-15(b)(2)(a): el DOF rotula una fila como "91-75".</b>
    ///
    /// <para>
    /// No es un error de transcripción: se verificó contra el PDF del DOF (página 134) y ahí está.
    /// </para>
    ///
    /// <para>
    /// <b>Que "91-75" es imposible lo dice la propia tabla</b>, por dos caminos:
    /// </para>
    /// <list type="number">
    /// <item>Un intervalo no puede empezar en 91 y terminar en 75. <b>No es un rango.</b></item>
    /// <item>La tabla entera avanza en bandas de 5 °C y esa fila va justo entre 66-70 y 76-80, así
    /// que el único hueco que puede llenar es <b>71-75</b>. Leído como está, la tabla deja sin
    /// cobertura las temperaturas de 71 a 74 °C.</item>
    /// </list>
    ///
    /// <para>
    /// <b>Consecuencia si no se corrige:</b> a 72 °C de temperatura ambiente la tabla no devuelve
    /// factor, así que el cálculo se queda sin corrección por temperatura justo en el caso más
    /// caliente. <b>Ésa es la dirección peligrosa</b> — el conductor saldría más chico de lo que
    /// toca, no más grande.
    /// </para>
    /// </summary>
    public static readonly ErrataDeRotulo Banda71a75 = new(
        TablaId: "310-15(b)(2)(a)",
        RotuloPublicado: "91-75",
        Min: 71m,
        Max: 75m,
        Descripcion: "Tabla 310-15(b)(2)(a), la fila rotulada \"91-75\"",
        Sustento: "un intervalo no puede empezar en 91 y terminar en 75, y la fila va entre 66-70 y "
            + "76-80 en una tabla que avanza en bandas de 5 °C, así que el único hueco que puede "
            + "llenar es 71-75; leída como se publica, la tabla deja sin cobertura los 71 a 74 °C");

    /// <summary>
    /// <b>Tabla 5 del Capítulo 10, TW/THHW/THW/THW-2 de 10 AWG: el DOF publica 55.68 mm².</b>
    /// Encontrada el 2026-09-24 al dimensionar canalizaciones (nacido en la web).
    ///
    /// <para><b>Que 55.68 es imposible lo dice la propia fila</b>, por tres caminos:</para>
    /// <list type="number">
    /// <item>La misma fila da el diámetro: 4.470 mm. El área de un círculo de 4.470 mm es
    /// π × 4.470² / 4 = <b>15.69 mm²</b>. En las demás filas de la Tabla 5 el área y el diámetro
    /// cuadran a menos del 3 % (se barrieron todas).</item>
    /// <item>Rompe la monotonía del bloque: 12 AWG = 11.68, <b>10 AWG = 55.68</b>, 8 AWG = 28.19. Un
    /// conductor más delgado no puede ocupar el doble que el siguiente.</item>
    /// <item>La tabla equivalente del NEC (Chapter 9, Table 5) da 15.68 mm² para TW 10 AWG.</item>
    /// </list>
    ///
    /// <para><b>Consecuencia si no se corrige:</b> cada conductor de 10 AWG en TW, THW o THHW cuenta
    /// 3.5 veces su área y el tubo sale uno o dos tamaños más grande.</para>
    /// </summary>
    public static readonly ErrataDeCelda AreaTw10Awg = new(
        TablaId: "5",
        ClaveFila: "TW,THHW,THW,THW-2|10",
        Columna: 4,
        ValorPublicado: 55.68m,
        ValorCorregido: 15.68m,
        Descripcion: "Tabla 5 del Capítulo 10, TW/THHW/THW/THW-2 de 10 AWG, área",
        Sustento: "el DOF publica 55.68 mm², pero la misma fila da 4.470 mm de diámetro, que son "
            + "π × 4.470² / 4 = 15.69 mm²; además rompe su bloque (12 AWG = 11.68, 8 AWG = 28.19), y "
            + "la tabla equivalente del NEC (Chapter 9, Table 5) da 15.68 mm²");

    /// <summary>Todas las erratas de celda conocidas.</summary>
    public static readonly IReadOnlyList<ErrataDeCelda> Todas = [Flc10HpEn575V, AreaTw10Awg];

    /// <summary>Todas las erratas de rótulo conocidas. Hoy es una.</summary>
    public static readonly IReadOnlyList<ErrataDeRotulo> TodasLasDeRotulo = [Banda71a75];

    /// <summary>
    /// La errata de rótulo que aplica a esa fila, o null si no hay ninguna — que es el caso de casi
    /// todas las filas de casi todas las tablas.
    ///
    /// <para>
    /// <b>La verificación aquí es el rótulo exacto</b>, y es el equivalente de "se verifica antes de
    /// corregir" de las erratas de celda: si el corpus se arregla río arriba, el texto deja de
    /// coincidir, la errata deja de aplicar sola y la fila se lee como cualquier otra.
    /// </para>
    /// </summary>
    public static ErrataDeRotulo? DelRotulo(string tablaId, string rotulo) =>
        TodasLasDeRotulo.FirstOrDefault(e =>
            e.TablaId == tablaId
            && string.Equals(e.RotuloPublicado, rotulo.Trim(), StringComparison.OrdinalIgnoreCase));

    /// <summary>Las erratas que aplican a una tabla. Vacío en la inmensa mayoría de las tablas.</summary>
    public static IEnumerable<ErrataDeCelda> DeLaTabla(string tablaId) =>
        Todas.Where(e => e.TablaId == tablaId);

    /// <summary>
    /// La errata que aplica a una celda concreta, o null si esa celda no tiene ninguna.
    /// </summary>
    public static ErrataDeCelda? DeLaCelda(string tablaId, string claveFila, int columna) =>
        Todas.FirstOrDefault(e =>
            e.TablaId == tablaId
            && e.Columna == columna
            && string.Equals(e.ClaveFila, claveFila, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Devuelve el valor con el que hay que calcular esa celda. Ver la nota de "se verifica antes de
    /// corregir" en <see cref="ErratasDeLaNorma"/>: un valor inesperado es excepción, no corrección.
    /// </summary>
    /// <exception cref="ErrataDesalineadaException">
    /// La celda no trae ni el valor publicado ni el corregido: la errata apunta a otra celda.
    /// </exception>
    public static decimal? Aplicar(string tablaId, string claveFila, int columna, decimal? valorLeido)
    {
        if (DeLaCelda(tablaId, claveFila, columna) is not { } errata) return valorLeido;
        if (valorLeido is null) return null;

        if (valorLeido == errata.ValorPublicado) return errata.ValorCorregido;

        // La fuente ya se corrigió río arriba. La errata sobra, pero no estorba: el resultado es el
        // mismo y no hay nada que avisar.
        if (valorLeido == errata.ValorCorregido) return valorLeido;

        throw new ErrataDesalineadaException(errata, valorLeido.Value);
    }
}

/// <summary>
/// Una errata dejó de coincidir con la celda a la que apunta. <b>Es un error de programa, no de
/// captura del usuario</b>: significa que la tabla del corpus cambió de forma y la corrección está
/// apuntando a otro lado. Se lanza en vez de corregir a ciegas.
/// </summary>
public class ErrataDesalineadaException(ErrataDeCelda errata, decimal valorEncontrado)
    : InvalidOperationException(
        $"La errata de {errata.Descripcion} esperaba encontrar {errata.ValorPublicado} en la celda "
        + $"(tabla {errata.TablaId}, fila '{errata.ClaveFila}', columna {errata.Columna}) y encontró "
        + $"{valorEncontrado}. La tabla del corpus cambió de forma: revisa ErratasDeLaNorma antes de "
        + "volver a calcular; corregir a ciegas una celda equivocada es peor que no corregir nada.")
{
    public ErrataDeCelda Errata { get; } = errata;
    public decimal ValorEncontrado { get; } = valorEncontrado;
}
