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

        // La columna 3 («Aplicaciones previstas») también viene combinada: RHW trae «Lugares secos y
        // mojados» con RowSpan = 2, que cubre a RHW-2; THWN, «Lugares secos y húmedos», que cubre a
        // THWN-2. Se hereda con el RowSpan, igual que el material (ver Familias). Antes RHW-2 y THWN-2
        // quedaban «sin lugar» y valían en todos (auditoría del 2026-09-29, P1-2).
        string? condicionHeredada = null;
        var filasQueFaltan = 0;

        foreach (var f in fuente.FilasDatos(TablaId))
        {
            var designacionCelda = f.Texto(1);
            if (!string.IsNullOrWhiteSpace(designacionCelda))
                designacionActual = designacionCelda.Trim();

            string condicion;
            var span = f.RowSpan(3);
            if (span > 0)
            {
                condicion = f.Texto(3) ?? "";
                condicionHeredada = condicion;
                filasQueFaltan = span - 1;
            }
            else if (filasQueFaltan > 0)
            {
                condicion = condicionHeredada ?? "";
                filasQueFaltan--;
            }
            else
            {
                condicion = "";
            }

            if (designacionActual is null || !Curadas.Contains(designacionActual))
                continue;

            var tempCelda = f.Texto(2);
            if (string.IsNullOrWhiteSpace(tempCelda)) continue;

            var match = ExpresionTemperatura().Match(tempCelda);
            if (!match.Success) continue;
            var temp = decimal.Parse(match.Value, System.Globalization.CultureInfo.InvariantCulture);

            if (!resultado.TryGetValue(designacionActual, out var lista))
                resultado[designacionActual] = lista = [];
            lista.Add((temp, condicion));
        }

        _cache = resultado;
        return _cache;
    }

    /// <summary>
    /// Los tipos que nombran 310-10(b) (lugares secos y húmedos) y 310-10(c)(2) (lugares mojados),
    /// leídos del texto de la sección como la lista de 240-6(a).
    ///
    /// <para>
    /// ⚠ <b>La tabla y el texto no dicen lo mismo.</b> La Tabla 310-104(a) publica THWN, THWN-2 y THW-2
    /// solo para «lugares secos y húmedos», pero 310-10(c)(2) los nombra para lugares mojados; y
    /// THHN solo para «lugares secos», pero 310-10(b) lo nombra para húmedos. Los «Usos permitidos»
    /// son los de 310-10; la tabla da la temperatura. Hallazgo de la auditoría del 2026-09-29 (P1-2),
    /// que pedía bloquear THWN y THW-2 en mojado leyendo solo la tabla.
    /// </para>
    /// </summary>
    private HashSet<string> Permitidos(string seccionId) =>
        seccionId == "310-10(b)"
            ? _humedos ??= TiposDeLaSeccion(seccionId)
            : _mojados ??= TiposDeLaSeccion(seccionId);

    private HashSet<string>? _humedos;
    private HashSet<string>? _mojados;

    private HashSet<string> TiposDeLaSeccion(string seccionId)
    {
        var texto = fuente.TextoDeSeccion(seccionId);
        var inicio = texto.IndexOf("tipos", StringComparison.OrdinalIgnoreCase);
        var lista = inicio >= 0 ? texto[(inicio + "tipos".Length)..] : texto;
        return new HashSet<string>(
            ExpresionTipo().Matches(lista).Select(m => m.Value),
            StringComparer.OrdinalIgnoreCase);
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

    /// <summary>
    /// La temperatura del aislamiento en el lugar:
    /// <list type="bullet">
    /// <item><b>Seco</b>: la fila de lugares secos; sin ella, la que tenga — 310-10(a) admite
    /// cualquier tipo en lugar seco (THW, que el DOF publica solo para mojados, M-08).</item>
    /// <item><b>Húmedo</b>: la fila de húmedos; sin ella, la de mojados (lo que aguanta mojado aguanta
    /// húmedo: THHW, 75 °C); sin ninguna, la que tenga si 310-10(b) lo nombra (THHN, 90 °C).</item>
    /// <item><b>Mojado</b>: la fila de mojados (XHHW, 75 °C); sin ella, la de húmedos si 310-10(c)(2) lo
    /// nombra (THWN, 75 °C; THW-2, 90 °C). Si no, no se permite (THHN, RHH, XHH).</item>
    /// </list>
    /// Una fila con el lugar en blanco vale en los tres. USE («Ver el Artículo 340») no nombra lugar ni
    /// está en 310-10(b) o (c)(2): solo en seco, por 310-10(a), como antes.
    /// </summary>
    public TemperaturaAislamiento? TemperaturaMaxima(string designacion, LugarDeInstalacion lugar)
    {
        var tipo = designacion.Trim();
        if (!Entradas().TryGetValue(tipo, out var entradas))
            return null;

        List<(decimal TempC, string Condicion)> Filas(string palabra) =>
            entradas.Where(e => string.IsNullOrWhiteSpace(e.Condicion) || e.Condicion.Contains(palabra, StringComparison.OrdinalIgnoreCase)).ToList();

        var compatibles = lugar switch
        {
            LugarDeInstalacion.Humedo => Filas("húmedo") is { Count: > 0 } humedo ? humedo
                : Filas("mojado") is { Count: > 0 } mojado ? mojado
                : Permitidos("310-10(b)").Contains(tipo) ? entradas
                : [],
            LugarDeInstalacion.Mojado => Filas("mojado") is { Count: > 0 } mojado ? mojado
                : Permitidos("310-10(c)(2)").Contains(tipo) ? Filas("húmedo")
                : [],
            _ => Filas("seco") is { Count: > 0 } seco ? seco : entradas,
        };
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

    [GeneratedRegex(@"[A-Z][A-Z0-9]*(-[A-Z0-9]+)*")]
    private static partial Regex ExpresionTipo();

    [GeneratedRegex(@"\d+(\.\d+)?")]
    private static partial Regex ExpresionTemperatura();
}
