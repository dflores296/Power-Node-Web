using System.Text.RegularExpressions;
using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Normativa;

/// <summary>
/// Tabla 310-104(a). Columnas: 0=nombre genérico, 1=tipo (designación), 2=temperatura máxima,
/// 3=aplicaciones previstas (el lugar: secos/húmedos/mojados), 4=aislamiento, 5=recubrimiento.
/// Cada designación puede traer 1 o 2 filas (rowspan) cuando tiene rating distinto según el lugar
/// -- p.ej. THHW es 75°C en lugares mojados pero 90°C en lugares secos, misma designación, dos filas.
/// </summary>
public partial class TablaAislamientoJson(IFuenteTablas fuente) : ITablaAislamiento
{
    private const string TablaId = "310-104(a)";

    /// <summary>
    /// Designaciones curadas para v1 -- las de uso común en alambrado de baja tensión (60-90°C).
    /// Fuera a propósito: aislamientos especiales de 150-250°C (FEP, FEPB, MI, PFA, PFAH, SA, TFE,
    /// Z, ZW, ZW-2...) que casi nunca aparecen en un tablero/circuito de edificio normal, y que este
    /// motor no puede representar (<see cref="TemperaturaAislamiento"/> solo cubre 60/75/90°C) --
    /// ver PLAN-V1.md.
    /// </summary>
    private static readonly HashSet<string> Curadas = new(StringComparer.OrdinalIgnoreCase)
    {
        "TW", "THW", "THW-2", "THW-LS", "THHW", "THHW-LS", "THHN", "THWN", "THWN-2",
        "XHH", "XHHW", "XHHW-2", "RHH", "RHW", "RHW-2", "USE", "USE-2",
    };

    private Dictionary<string, List<(decimal TempC, string Condicion)>>? _cache;

    private Dictionary<string, FamiliaAislamiento>? _familias;

    public IReadOnlyList<string> DesignacionesReconocidas => Curadas.OrderBy(d => d, StringComparer.Ordinal).ToList();

    private Dictionary<string, List<(decimal TempC, string Condicion)>> Entradas()
    {
        if (_cache is not null) return _cache;

        var resultado = new Dictionary<string, List<(decimal, string)>>(StringComparer.OrdinalIgnoreCase);
        string? designacionActual = null;

        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var designacionCelda = f.Texto(1);
            if (!string.IsNullOrWhiteSpace(designacionCelda))
                designacionActual = designacionCelda.Trim();

            if (designacionActual is null || !Curadas.Contains(designacionActual))
                continue;

            var tempCelda = f.Texto(2);
            if (string.IsNullOrWhiteSpace(tempCelda)) continue;

            var match = ExpresionTemperatura().Match(tempCelda);
            if (!match.Success) continue;
            var temp = decimal.Parse(match.Value, System.Globalization.CultureInfo.InvariantCulture);

            var condicion = f.Texto(3) ?? "";

            if (!resultado.TryGetValue(designacionActual, out var lista))
                resultado[designacionActual] = lista = [];
            lista.Add((temp, condicion));
        }

        _cache = resultado;
        return _cache;
    }

    /// <summary>
    /// La columna 4 de la tabla real —el material del aislamiento—, leída una vez.
    ///
    /// <para>
    /// ⚠ <b>Aquí manda el <c>RowSpan</c>, no el «arrastra hasta que cambie la designación» que usa la
    /// columna de temperatura.</b> Y la diferencia es visible en la tabla real: <c>RHW</c> trae el
    /// material con <c>RowSpan = 2</c>, así que <b>cubre también a <c>RHW-2</c></b> —dos designaciones
    /// distintas, un solo material—; mientras que <c>RHH</c>, que está justo arriba, trae su celda
    /// <b>presente y vacía</b>, o sea que la norma no declara su material.
    /// </para>
    ///
    /// <para>
    /// Un arrastre a secas confundiría los dos casos: le daría a RHH el material de la fila anterior
    /// (inventando un dato) o dejaría a RHW-2 sin el suyo (tirando uno que sí está). <b>Lo cachó la
    /// prueba, no la lectura</b> — el primer intento reiniciaba por designación y dejaba RHW-2 en
    /// <c>null</c>.
    /// </para>
    /// </summary>
    private Dictionary<string, FamiliaAislamiento> Familias()
    {
        if (_familias is not null) return _familias;

        var resultado = new Dictionary<string, FamiliaAislamiento>(StringComparer.OrdinalIgnoreCase);
        string? designacionActual = null;

        // Lo que la celda combinada de arriba sigue cubriendo, y por cuántas filas más.
        FamiliaAislamiento? heredada = null;
        var filasQueFaltan = 0;

        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var designacionCelda = f.Texto(1);
            if (!string.IsNullOrWhiteSpace(designacionCelda))
                designacionActual = designacionCelda.Trim();

            var span = f.RowSpan(4);
            if (span > 0)
            {
                // Celda propia: sustituye a lo heredado aunque venga vacía (RHH).
                heredada = FamiliaDelTexto(f.Texto(4));
                filasQueFaltan = span - 1;
            }
            else if (filasQueFaltan > 0)
            {
                filasQueFaltan--;
            }
            else
            {
                heredada = null;
            }

            if (designacionActual is null || !Curadas.Contains(designacionActual)) continue;
            if (resultado.ContainsKey(designacionActual)) continue;

            if (heredada is { } familia)
                resultado[designacionActual] = familia;
        }

        _familias = resultado;
        return _familias;
    }

    /// <summary>
    /// <b>«Termofijo» es como la NOM traduce <i>thermoset</i></b>, que es lo mismo que termoestable —
    /// el nombre que usa la Tabla A.54.4 de la IEC.
    ///
    /// <para>
    /// ⚠ <b>Se lee la tabla como está impresa, sin corregirla.</b> La NOM rotula <c>XHH</c> como
    /// «Termoplástico» aunque la X sea de <i>cross-linked</i> (reticulado ⇒ termofijo). No se
    /// enmienda aquí por dos razones: **no es una errata verificada** —para eso está
    /// <c>ErratasDeLaNorma</c>, y sus entradas se comprobaron una por una contra el DOF—, y el efecto
    /// de tomarla al pie de la letra es <b>conservador</b>: termoplástico tiene la <c>k</c> menor, así
    /// que el conductor sale menos protegido, no más. Si alguien la confirma como errata, el lugar de
    /// corregirla es <c>ErratasDeLaNorma</c>, no este método.
    /// </para>
    /// </summary>
    private static FamiliaAislamiento? FamiliaDelTexto(string? texto)
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;

        var t = texto.TrimStart();

        if (t.StartsWith("Termoplástico", StringComparison.OrdinalIgnoreCase) ||
            t.StartsWith("Termoplastico", StringComparison.OrdinalIgnoreCase))
            return FamiliaAislamiento.Termoplastico;

        if (t.StartsWith("Termofijo", StringComparison.OrdinalIgnoreCase) ||
            t.StartsWith("Termoestable", StringComparison.OrdinalIgnoreCase))
            return FamiliaAislamiento.Termoestable;

        // Hule silicón, papel, óxido de magnesio, "resistente a la humedad" a secas... No se fuerza a
        // ninguna de las dos familias: sin material declarado no hay k, y no hay curva de daño.
        return null;
    }

    public FamiliaAislamiento? FamiliaDe(string designacion) =>
        Familias().TryGetValue(designacion.Trim(), out var familia) ? familia : null;

    public TemperaturaAislamiento? TemperaturaMaxima(string designacion, bool lugarSeco)
    {
        if (!Entradas().TryGetValue(designacion.Trim(), out var entradas))
            return null;

        // Entre las filas de esta designación compatibles con el lugar pedido, la de temperatura más
        // alta -- normalmente solo hay una compatible (p.ej. THHN nada más aplica a "secos"), pero si
        // el texto no especifica lugar (celda en blanco, común en los tipos "-2") se trata como
        // válido para ambos.
        var compatibles = entradas.Where(e => EsCompatible(e.Condicion, lugarSeco)).ToList();
        if (compatibles.Count == 0) return null;

        var tempMaxima = compatibles.Max(e => e.TempC);
        return tempMaxima switch
        {
            60m => TemperaturaAislamiento.T60,
            75m => TemperaturaAislamiento.T75,
            90m => TemperaturaAislamiento.T90,
            _ => null, // aislamiento especial (150-250°C) -- fuera de alcance de v1.
        };
    }

    private static bool EsCompatible(string condicion, bool lugarSeco)
    {
        if (string.IsNullOrWhiteSpace(condicion)) return true; // sin lugar especificado -- válido para ambos.

        var esSeco = condicion.Contains("seco", StringComparison.OrdinalIgnoreCase);
        var esHumedoOMojado = condicion.Contains("húmedo", StringComparison.OrdinalIgnoreCase)
            || condicion.Contains("mojado", StringComparison.OrdinalIgnoreCase);

        return lugarSeco ? esSeco : esHumedoOMojado;
    }

    [GeneratedRegex(@"\d+(\.\d+)?")]
    private static partial Regex ExpresionTemperatura();
}
