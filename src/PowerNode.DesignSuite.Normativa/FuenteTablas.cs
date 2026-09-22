using System.Text.Json;
using System.Text.Json.Serialization;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>Una fila de datos (ya sin las filas de encabezado) de una tabla de la norma.</summary>
///
/// <remarks>
/// Copiada tal cual de <c>Data/TablasNom/TablaGridReader.cs</c> del repo de escritorio. Es la
/// <b>costura</b> de todo este proyecto: las trece implementaciones de las interfaces de
/// <c>Calculo/TablasNom</c> sólo hablan con este tipo, nunca con la base de datos. Cambiar de dónde
/// salen las filas —de SQL Server allá, de un JSON aquí— no les toca una línea.
/// </remarks>
public sealed class FilaTabla(
    int indice,
    IReadOnlyDictionary<int, string> celdas,
    IReadOnlyDictionary<int, int>? rowSpans = null)
{
    public int Indice { get; } = indice;

    public string? Texto(int columna) => celdas.TryGetValue(columna, out var t) ? t : null;

    /// <summary>
    /// Cuántas filas abarca esta celda hacia abajo. <b>1 cuando no hay combinación, y 0 cuando la
    /// celda ni siquiera existe</b> — que es lo que distingue «celda vacía a propósito» de «esta fila
    /// la cubre la de arriba».
    ///
    /// <para>
    /// Esa diferencia importa de verdad: en la Tabla 310-104(a), <c>RHH</c> trae la celda de
    /// aislamiento <b>presente y vacía</b> (no se sabe el material) mientras que <c>RHW-2</c>
    /// <b>no trae celda</b> porque la de <c>RHW</c> la cubre con <c>RowSpan = 2</c>. Leer las dos como
    /// «vacío» le quita a RHW-2 un dato que la norma sí da.
    /// </para>
    /// </summary>
    public int RowSpan(int columna) =>
        rowSpans is not null && rowSpans.TryGetValue(columna, out var n) ? Math.Max(n, 1)
        : celdas.ContainsKey(columna) ? 1
        : 0;
}

/// <summary>De dónde salen las tablas de la norma. Implementarla es todo lo que hace falta para
/// alimentar el motor desde una fuente nueva.</summary>
public interface IFuenteTablas
{
    IReadOnlyList<FilaTabla> FilasDatos(string tablaId);

    /// <summary>El texto de una sección de la norma. Sólo 240-6(a) lo usa: sus valores
    /// estandarizados son prosa, no una rejilla.</summary>
    string TextoDeSeccion(string seccionId);
}

/// <summary>
/// Las tablas leídas del JSON que genera <c>tools/extraer_tablas.py</c> desde el repo público
/// <c>dflores296/NOM-001-SEDE-2012</c>.
///
/// <para>
/// <b>El JSON viaja crudo, con las celdas tal como las publica la norma</b> (<c>t</c>/<c>rs</c>/<c>cs</c>),
/// y la expansión de rowspan/colspan a índices de columna se hace aquí, con
/// <see cref="ResolverGrid"/> — que es el mismo algoritmo, copiado verbatim, del importador de la
/// versión de escritorio. Se hizo así a propósito: si la expansión viviera en el script de Python,
/// habría dos implementaciones del mismo algoritmo en dos lenguajes, y el día que una cambiara la
/// otra leería la misma tabla distinto sin que nada avisara.
/// </para>
/// </summary>
public sealed class FuenteTablasJson : IFuenteTablas
{
    private sealed record CeldaJson(
        [property: JsonPropertyName("t")] string? T,
        [property: JsonPropertyName("rs")] int? Rs,
        [property: JsonPropertyName("cs")] int? Cs);

    private sealed record TablaJson(
        [property: JsonPropertyName("verificada")] string? Verificada,
        [property: JsonPropertyName("header_rows")] int HeaderRows,
        [property: JsonPropertyName("rows")] List<List<CeldaJson>> Rows);

    private sealed record ArchivoJson(
        [property: JsonPropertyName("tablas")] Dictionary<string, TablaJson> Tablas,
        [property: JsonPropertyName("secciones")] Dictionary<string, string> Secciones);

    private readonly ArchivoJson _archivo;
    private readonly Dictionary<string, IReadOnlyList<FilaTabla>> _cache = [];

    public FuenteTablasJson(string json)
    {
        _archivo = JsonSerializer.Deserialize<ArchivoJson>(json)
            ?? throw new InvalidOperationException("El JSON de tablas de la norma vino vacío o ilegible.");
    }

    /// <summary>La fecha en que esa tabla se cotejó celda por celda contra el PDF del DOF, según el
    /// repo de la norma. Es la procedencia del dato: sin ella, un número aquí no se distingue de uno
    /// tecleado a mano.</summary>
    public string? VerificadaEl(string tablaId) =>
        _archivo.Tablas.TryGetValue(tablaId, out var t) ? t.Verificada : null;

    public IReadOnlyList<FilaTabla> FilasDatos(string tablaId)
    {
        if (_cache.TryGetValue(tablaId, out var cacheado))
            return cacheado;

        if (!_archivo.Tablas.TryGetValue(tablaId, out var tabla))
            throw new InvalidOperationException(
                $"No se encontró la tabla '{tablaId}' en los datos de la norma. " +
                "Si el cálculo la necesita, agrégala a TABLAS en tools/extraer_tablas.py y regenera.");

        var porFila = ResolverGrid(tabla.Rows)
            .GroupBy(x => x.Fila)
            .Where(g => g.Key >= tabla.HeaderRows)
            .OrderBy(g => g.Key)
            .Select(g => new FilaTabla(
                g.Key,
                g.ToDictionary(x => x.Col, x => x.Celda.T ?? string.Empty),
                g.ToDictionary(x => x.Col, x => x.Celda.Rs is > 0 ? x.Celda.Rs.Value : 1)))
            .ToList();

        _cache[tablaId] = porFila;
        return porFila;
    }

    public string TextoDeSeccion(string seccionId) =>
        _archivo.Secciones.TryGetValue(seccionId, out var texto)
            ? texto
            : throw new InvalidOperationException(
                $"No se encontró la sección '{seccionId}' en los datos de la norma. " +
                "Agrégala a SECCIONES en tools/extraer_tablas.py y regenera.");

    /// <summary>
    /// Resuelve el índice de columna real de cada celda dado su rowspan/colspan, igual que lo
    /// haría un navegador al dibujar una tabla HTML: las columnas ocupadas por un rowspan de una
    /// fila anterior se saltan al numerar las celdas de las filas siguientes.
    /// </summary>
    /// <remarks>Copiado verbatim de <c>NomDataImporter.ResolverGrid</c> del repo de escritorio.</remarks>
    private static List<(int Fila, int Col, CeldaJson Celda)> ResolverGrid(List<List<CeldaJson>> filas)
    {
        var resultado = new List<(int, int, CeldaJson)>();
        var bloqueadas = new Dictionary<int, int>();

        for (var r = 0; r < filas.Count; r++)
        {
            var col = 0;
            foreach (var celda in filas[r])
            {
                while (bloqueadas.TryGetValue(col, out var restante) && restante > 0)
                    col++;

                var rs = celda.Rs is > 0 ? celda.Rs.Value : 1;
                var cs = celda.Cs is > 0 ? celda.Cs.Value : 1;

                resultado.Add((r, col, celda));

                for (var c = col; c < col + cs; c++)
                    bloqueadas[c] = rs;

                col += cs;
            }

            foreach (var key in bloqueadas.Keys.ToList())
                bloqueadas[key] = Math.Max(0, bloqueadas[key] - 1);
        }

        return resultado;
    }
}
