namespace PowerNode.DesignSuite.Calculo.Diagramas;

/// <summary>
/// Un elemento a acomodar: su identidad, de quién cuelga (<c>null</c> = raíz) y, si el usuario lo
/// movió a mano, dónde lo dejó (<c>null</c> = que lo acomode el automático).
/// </summary>
public readonly record struct ElementoAAcomodar(int Id, int? PadreId, double? X = null, double? Y = null)
{
    public bool EstaAnclado => X is not null && Y is not null;
}

/// <summary>Dónde quedó la caja de un elemento. Origen arriba-izquierda, como en WPF.</summary>
public readonly record struct CajaDiagrama(int Id, double X, double Y, double Ancho, double Alto)
{
    public double CentroX => X + Ancho / 2;
    public double CentroY => Y + Alto / 2;
    public double Abajo => Y + Alto;
}

/// <summary>
/// Un tramo de línea del dibujo. Horizontal o vertical mientras el acomodo mande, que es el trazo de
/// un unifilar; solo sale en diagonal cuando el usuario arrastró un elemento arriba de quien lo
/// alimenta y ya no hay por dónde bajarle.
/// </summary>
/// <param name="ElementoDestinoId">
/// A qué elemento LLEGA este tramo, cuando el tramo es suyo y de nadie más. Es lo que permite darle
/// clic a un cable en el diagrama y llegar a su alimentador — un elemento tiene a lo más un
/// alimentador entrante, así que el destino lo identifica sin ambigüedad.
///
/// <b><c>null</c> en los tramos COMPARTIDOS</b>, y es la parte que importa: la bajada del padre a la
/// barra común y la barra horizontal las usan todos los hermanos a la vez. Atribuírselas a uno haría
/// que darle clic a la barra abriera el cable de un hermano cualquiera — el primero que se hubiera
/// dibujado—, que es peor que no responder.
/// </param>
public readonly record struct SegmentoDiagrama(
    double X1, double Y1, double X2, double Y2, int? ElementoDestinoId = null);

/// <summary>El dibujo completo, ya resuelto: las cajas, las líneas y cuánto mide el lienzo.</summary>
public sealed record PlanoUnifilar(
    IReadOnlyList<CajaDiagrama> Cajas,
    IReadOnlyList<SegmentoDiagrama> Segmentos,
    double Ancho,
    double Alto);

/// <summary>Las medidas del dibujo. Se pasan como dato para que la prueba no dependa de la interfaz.</summary>
public sealed record MedidasDiagrama(
    double AnchoCaja = 168,
    double AltoCaja = 56,
    double SeparacionHorizontal = 22,
    double SeparacionVertical = 52,
    double Margen = 24);

/// <summary>
/// Acomoda los elementos de un proyecto como se dibuja un diagrama unifilar: <b>quien alimenta va
/// arriba, los alimentados van abajo, y los hermanos van uno al lado del otro colgando de una barra
/// común</b>.
///
/// <b>Por qué existe.</b> El navegador de la topología era un <c>TreeView</c>, y un árbol de Windows
/// apila a los hermanos en vertical con una sangría. Eso <b>miente sobre la instalación</b>: con un
/// transformador que alimenta al tablero G1 y al G2, los dos aparecen uno encima del otro y se lee
/// que G1 alimenta a G2. Lo señaló el usuario. En el dibujo, dos tableros que cuelgan del mismo
/// transformador tienen que verse a la misma altura, colgados de la misma barra — y con eso la
/// pregunta "¿de quién cuelga esto?" se contesta viendo, no contando sangrías.
///
/// <b>Y respeta lo que el usuario movió a mano.</b> Un elemento anclado se planta donde lo dejaron,
/// y lo que cuelga de él lo sigue con el mismo desplazamiento — arrastrar un tablero se lleva a sus
/// derivados, que es lo que uno espera. Todo lo que no está anclado se sigue acomodando solo, así
/// que mover una rama no despeina el resto del proyecto.
///
/// Es geometría pura, sin WPF: entran identificadores y sale dónde va cada caja, así que se puede
/// probar sin abrir la aplicación (que es la mitad del punto: la interfaz no se puede compilar en
/// una sesión de Linux, esto sí).
/// </summary>
public static class AcomodoUnifilar
{
    /// <summary>
    /// Resuelve el dibujo. El orden de <paramref name="elementos"/> se respeta: los hermanos salen
    /// de izquierda a derecha en el orden en que vienen, y lo mismo las raíces.
    ///
    /// Tolera dos topologías malas sin quejarse, porque dibujar es lo último que debería impedir
    /// corregirlas: un padre que no está en la lista se trata como si el elemento fuera raíz, y un
    /// lazo se corta (cada elemento se coloca una sola vez).
    /// </summary>
    public static PlanoUnifilar Acomodar(IEnumerable<ElementoAAcomodar> elementos, MedidasDiagrama? medidas = null)
    {
        var m = medidas ?? new MedidasDiagrama();
        var lista = elementos.ToList();
        var existentes = lista.Select(e => e.Id).ToHashSet();

        // Hijos en el orden de entrada. Un padre que no existe cuenta como "sin padre".
        var hijos = new Dictionary<int, List<int>>();
        var raices = new List<int>();
        foreach (var elemento in lista)
        {
            if (elemento.PadreId is { } padre && padre != elemento.Id && existentes.Contains(padre))
            {
                if (!hijos.TryGetValue(padre, out var lst))
                    hijos[padre] = lst = [];
                lst.Add(elemento.Id);
            }
            else
            {
                raices.Add(elemento.Id);
            }
        }

        var cajas = new Dictionary<int, CajaDiagrama>();
        var segmentos = new List<SegmentoDiagrama>();
        var colocados = new HashSet<int>();
        var profundidades = new Dictionary<int, int>();
        var cursorX = m.Margen;

        // Recursivo y no iterativo a propósito: la profundidad de un unifilar real son unos pocos
        // niveles (acometida - transformador - general - derivados), muy lejos de agotar la pila.
        double Colocar(int id, int profundidad)
        {
            if (!colocados.Add(id))
                return cajas.TryGetValue(id, out var ya) ? ya.CentroX : cursorX;

            profundidades[id] = profundidad;
            var y = m.Margen + profundidad * (m.AltoCaja + m.SeparacionVertical);
            var misHijos = hijos.TryGetValue(id, out var lst) ? lst : [];

            if (misHijos.Count == 0)
            {
                // Hoja: se planta donde va el cursor y lo empuja.
                var caja = new CajaDiagrama(id, cursorX, y, m.AnchoCaja, m.AltoCaja);
                cajas[id] = caja;
                cursorX += m.AnchoCaja + m.SeparacionHorizontal;
                return caja.CentroX;
            }

            // Primero los hijos; el padre se centra sobre ellos. Como todas las cajas miden lo
            // mismo, el centro del bloque es el promedio del primer y el último centro.
            var centrosDeHijos = new List<double>(misHijos.Count);
            foreach (var hijo in misHijos)
                centrosDeHijos.Add(Colocar(hijo, profundidad + 1));

            var centro = (centrosDeHijos[0] + centrosDeHijos[^1]) / 2;
            cajas[id] = new CajaDiagrama(id, centro - m.AnchoCaja / 2, y, m.AnchoCaja, m.AltoCaja);
            return centro;
        }

        foreach (var raiz in raices)
            Colocar(raiz, 0);

        // Los elementos que quedaron fuera por un lazo: se dibujan como raíces sueltas en vez de
        // desaparecer del diagrama. Un elemento invisible es peor que uno mal colocado — no se puede
        // seleccionar para arreglarlo.
        foreach (var elemento in lista.Where(e => !colocados.Contains(e.Id)))
            Colocar(elemento.Id, 0);

        AplicarAnclajes(lista, hijos, cajas, profundidades);
        MeterTodoAlLienzo(cajas, m);

        // ------------------------------------------------------------------ las líneas
        //
        // Se calculan AL FINAL, sobre las posiciones definitivas: si se dibujaran antes de mover lo
        // anclado, las líneas apuntarían a donde las cajas ya no están.
        foreach (var (padre, listaHijos) in hijos)
        {
            if (!cajas.TryGetValue(padre, out var cajaPadre))
                continue;

            var todasLasHijas = listaHijos.Where(cajas.ContainsKey).Select(h => cajas[h]).ToList();

            // Las que quedaron por debajo del padre se cuelgan de una barra común, que es como se
            // dibuja un unifilar. Las que NO -- porque el usuario las arrastró arriba o a un lado --
            // se unen con una línea directa: fea, pero honesta. Antes se les quitaba la línea y el
            // elemento se veía desconectado sin estarlo, que es peor.
            var colgadas = todasLasHijas.Where(c => c.Y > cajaPadre.Abajo).ToList();
            var fueraDeLugar = todasLasHijas.Where(c => c.Y <= cajaPadre.Abajo).ToList();

            foreach (var hija in fueraDeLugar)
                segmentos.Add(new SegmentoDiagrama(
                    cajaPadre.CentroX, cajaPadre.CentroY, hija.CentroX, hija.CentroY, hija.Id));

            if (colgadas.Count == 0)
                continue;

            // La barra: a media altura entre el padre y la hija más alta. Es la que hace legible que
            // los hermanos son hermanos y no una cadena.
            var yBarra = (cajaPadre.Abajo + colgadas.Min(c => c.Y)) / 2;

            // Bajada del padre hasta la barra.
            segmentos.Add(new SegmentoDiagrama(cajaPadre.CentroX, cajaPadre.Abajo, cajaPadre.CentroX, yBarra));

            // La barra horizontal. Con un solo hijo alineado queda de largo cero: no se dibuja,
            // porque un punto en medio de una bajada recta se ve como suciedad.
            var izquierda = Math.Min(colgadas.Min(c => c.CentroX), cajaPadre.CentroX);
            var derecha = Math.Max(colgadas.Max(c => c.CentroX), cajaPadre.CentroX);
            if (derecha - izquierda > 0.5)
                segmentos.Add(new SegmentoDiagrama(izquierda, yBarra, derecha, yBarra));

            // Bajada de la barra a cada hijo.
            foreach (var hijo in colgadas)
                segmentos.Add(new SegmentoDiagrama(hijo.CentroX, yBarra, hijo.CentroX, hijo.Y, hijo.Id));
        }

        var ordenadas = lista
            .Where(e => cajas.ContainsKey(e.Id))
            .Select(e => cajas[e.Id])
            .ToList();

        var ancho = ordenadas.Count == 0 ? 0 : ordenadas.Max(c => c.X + c.Ancho) + m.Margen;
        var alto = ordenadas.Count == 0 ? 0 : ordenadas.Max(c => c.Abajo) + m.Margen;

        return new PlanoUnifilar(ordenadas, segmentos, ancho, alto);
    }

    /// <summary>
    /// Mueve a su lugar lo que el usuario ancló, y arrastra con ello lo que cuelga.
    ///
    /// Se procesa de lo más superficial a lo más profundo, y el desplazamiento de cada anclado se
    /// aplica a TODA su descendencia. Si un descendiente también está anclado, le toca su turno
    /// después y se planta en su propio lugar, pisando el desplazamiento que traía — que es
    /// justamente lo que significa anclarlo: "este va aquí, pase lo que pase arriba".
    /// </summary>
    private static void AplicarAnclajes(
        List<ElementoAAcomodar> lista,
        Dictionary<int, List<int>> hijos,
        Dictionary<int, CajaDiagrama> cajas,
        Dictionary<int, int> profundidades)
    {
        var anclados = lista
            .Where(e => e.EstaAnclado && cajas.ContainsKey(e.Id))
            .OrderBy(e => profundidades.GetValueOrDefault(e.Id))
            .ToList();

        foreach (var anclado in anclados)
        {
            var actual = cajas[anclado.Id];
            var dx = anclado.X!.Value - actual.X;
            var dy = anclado.Y!.Value - actual.Y;

            if (Math.Abs(dx) < 0.001 && Math.Abs(dy) < 0.001)
                continue;

            foreach (var id in ConElDescendencia(anclado.Id, hijos))
                if (cajas.TryGetValue(id, out var caja))
                    cajas[id] = caja with { X = caja.X + dx, Y = caja.Y + dy };
        }
    }

    /// <summary>El elemento y todo lo que cuelga de él. Corta si hay lazo.</summary>
    private static IEnumerable<int> ConElDescendencia(int raiz, Dictionary<int, List<int>> hijos)
    {
        var vistos = new HashSet<int>();
        var pendientes = new Stack<int>([raiz]);

        while (pendientes.Count > 0)
        {
            var id = pendientes.Pop();
            if (!vistos.Add(id))
                continue;

            yield return id;

            if (hijos.TryGetValue(id, out var suyos))
                foreach (var hijo in suyos)
                    pendientes.Push(hijo);
        }
    }

    /// <summary>
    /// Si algo quedó con coordenada negativa —se arrastró más arriba o más a la izquierda del
    /// origen— se corre todo el dibujo para que vuelva a caber. Sin esto, una caja anclada fuera del
    /// lienzo se vuelve invisible y no hay forma de traerla de regreso.
    /// </summary>
    private static void MeterTodoAlLienzo(Dictionary<int, CajaDiagrama> cajas, MedidasDiagrama m)
    {
        if (cajas.Count == 0)
            return;

        var minX = cajas.Values.Min(c => c.X);
        var minY = cajas.Values.Min(c => c.Y);

        var dx = minX < m.Margen ? m.Margen - minX : 0;
        var dy = minY < m.Margen ? m.Margen - minY : 0;

        if (dx == 0 && dy == 0)
            return;

        foreach (var id in cajas.Keys.ToList())
            cajas[id] = cajas[id] with { X = cajas[id].X + dx, Y = cajas[id].Y + dy };
    }
}
