namespace PowerNode.DesignSuite.Calculo.Tableros;

/// <summary>
/// Un derivado de un tablero I-Line, reducido a lo que hace falta para revisarlo. Es un dato de
/// entrada, no una entidad: así estas reglas se prueban con objetos armados a mano y no dependen ni
/// de la base ni del dominio.
/// </summary>
/// <param name="Numero">Con qué se le nombra en el aviso. En un I-Line no es un espacio físico —no
/// los hay— sino el consecutivo con el que el usuario identifica al derivado.</param>
/// <param name="FaseDeclarada">Las barras que el usuario declaró, p.ej. "A" o "ABC".</param>
/// <param name="Polos">Polos del interruptor derivado.</param>
/// <param name="ProteccionA">Lo que calculó el motor para este derivado. Null si todavía no se
/// calcula.</param>
/// <param name="ModeloCatalogo">El modelo que eligió el usuario, si eligió alguno. <b>Null es un
/// estado válido</b>, no un pendiente: el proyecto se calcula y se entrega completo sin elegir
/// modelo.</param>
/// <param name="ModeloEsGarraILine">Si ese modelo se monta con garra I-Line. <b>Null = no se pudo
/// saber</b> (el número de catálogo no está sembrado), que no es lo mismo que <c>false</c>.</param>
/// <param name="EspacioMontajePlg">Pulgadas de bus que se come este derivado, de
/// <c>ModeloInterruptor.EspacioMontajeILinePlg</c>. <b>Null = no se sabe</b> —no hay modelo elegido,
/// o su marco no está tabulado—, que <b>no</b> es cero: un derivado sin medida no ocupa cero
/// pulgadas, ocupa una cantidad desconocida.</param>
public readonly record struct DerivadoILine(
    int Numero,
    string? FaseDeclarada,
    int Polos,
    decimal? ProteccionA = null,
    string? ModeloCatalogo = null,
    bool? ModeloEsGarraILine = null,
    decimal? EspacioMontajePlg = null,
    /// <summary>Las pulgadas que capturó el usuario. Solo se usan cuando el catálogo no publica la
    /// medida del modelo — y si las dos existen y no coinciden, se avisa. Ver
    /// <see cref="VerificacionesILine.AvisosDeEspacio"/>.</summary>
    decimal? EspacioCapturadoPlg = null,
    /// <summary>Cómo se nombra en un aviso. <c>null</c> = se usa el número, que es lo normal en un
    /// circuito. Un alimentador saliente no tiene número de espacio, así que va con su nombre.</summary>
    string? Etiqueta = null)
{
    /// <summary>Como aparece en un aviso: el nombre si lo tiene, si no el número.</summary>
    public string Rotulo => Etiqueta ?? Numero.ToString();

    /// <summary>
    /// La medida que se usa para sumar: <b>manda el catálogo</b>, porque es un hecho físico del
    /// aparato; la capturada llena el hueco del derivado sin modelo elegido.
    /// </summary>
    public decimal? EspacioParaSumar => EspacioMontajePlg ?? EspacioCapturadoPlg;

    /// <summary>Las dos medidas existen y no coinciden — hay que decirlo, no escoger en silencio.</summary>
    public bool MedidasEnConflicto =>
        EspacioMontajePlg is { } catalogo && EspacioCapturadoPlg is { } capturado && catalogo != capturado;
}

/// <summary>
/// Lo que se puede revisar de un tablero <see cref="SistemaBarras.ILine"/>, que no es lo mismo que
/// se revisa en uno de espacios numerados.
///
/// <b>En un I-Line la fase es declaración del usuario, no un valor derivado</b>, y desde el
/// 2026-08-18 se sabe <b>por qué</b>: la Tabla 9.121 del Digest 178 la publica como una <b>opción de
/// catálogo</b>, un sufijo del número de parte. La fase es un atributo del <b>aparato que se
/// compra</b>, no de la altura a la que se engarza. Ver <see cref="OpcionesFaseILine"/>.
///
/// Eso jubila la pregunta que este archivo traía anotada —<i>"falta la secuencia de fases del bus por
/// altura"</i>—: estaba mal planteada. No hay secuencia que descubrir; hay un sufijo que pedir. Y
/// confirma que dejar de inventarla en <c>AsignacionBarrasService</c> era lo correcto.
///
/// Por eso aquí <b>se valida la forma de lo declarado y que sea pedible, nunca cuál barra toca</b>:
/// que la cantidad de letras cuadre con los polos, que esas barras existan en el tablero, que no se
/// repitan, y que la combinación exista como opción de catálogo.
///
/// <b>Todo lo de aquí es aviso, no bloqueo</b> — como el resto de las verificaciones del proyecto.
/// </summary>
public static class VerificacionesILine
{
    /// <summary>
    /// Por qué la fase declarada de un derivado no sirve, o <c>null</c> si está bien. El texto se
    /// redacta para caer después del número del derivado ("El derivado 3 …").
    /// </summary>
    public static string? MotivoFaseInvalida(string? faseDeclarada, int polos, SistemaTablero sistema)
    {
        var barras = DistribucionBarras.BarrasDe(sistema);

        if (string.IsNullOrWhiteSpace(faseDeclarada))
            return "no tiene barras declaradas. En un I-Line la fase no sale del espacio: se declara al capturar el derivado.";

        var declarada = faseDeclarada.Trim().ToUpperInvariant();

        var ajenas = declarada.Where(b => !barras.Contains(b)).Distinct().ToList();
        if (ajenas.Count > 0)
            return $"declara la barra '{string.Join("', '", ajenas)}', que no existe en un tablero de " +
                   $"{sistema.Fases} fase(s) y {sistema.Hilos} hilos (las barras son {string.Join(", ", barras)}).";

        var repetidas = declarada.GroupBy(b => b).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (repetidas.Count > 0)
            return $"declara dos veces la barra '{string.Join("', '", repetidas)}'; un interruptor multipolar " +
                   "toca una barra distinta por polo.";

        if (declarada.Length != polos)
            return $"declara {declarada.Length} barra(s) ({declarada}) para un interruptor de {polos} polo(s); " +
                   "tiene que declarar una por polo.";

        return null;
    }

    /// <summary>
    /// Los avisos de fase de todos los derivados de un tablero I-Line, uno por renglón. Vacío
    /// cuando no hay nada que decir.
    /// </summary>
    public static IReadOnlyList<string> AvisosDeFases(IEnumerable<DerivadoILine> derivados, SistemaTablero sistema) =>
    [
        .. derivados
            .Select(d => (d.Numero, Motivo: MotivoFaseInvalida(d.FaseDeclarada, d.Polos, sistema)))
            .Where(x => x.Motivo is not null)
            .Select(x => $"El derivado {x.Numero} {x.Motivo}")
    ];

    /// <summary>
    /// Las dos verificaciones que el catálogo ya permite hacer sin datos nuevos:
    ///
    /// 1. Que el derivado no pase de lo que admite el tablero
    ///    (<c>ModeloTablero.CapacidadMaximaDerivadaA</c>: 250 A en los I-Line chicos, 400 y 800 en
    ///    los grandes). El dato estaba sembrado desde el 2026-08-15 y nadie lo comparaba con nada.
    ///    <b>Desde el 2026-08-27 la comparación no vive aquí</b>, sino en
    ///    <see cref="VerificacionDerivadoMaximo"/>, porque no tiene nada de I-Line y los tableros de
    ///    espacios numerados la necesitaban igual — allá no se revisaba nada.
    /// 2. Que el modelo de interruptor elegido se monte con <b>garra I-Line</b>. Es la condición
    ///    textual del usuario: <i>"siempre que estos usen conexión I-Line, o sea mordazas
    ///    engarzadas"</i>. Un PowerPacT de zapatas —el mismo marco, con la otra letra en el número de
    ///    parte— no entra a este bus.
    ///
    /// <b>Los dos son aviso, no bloqueo</b>, y los dos callan cuando falta el dato: sin capacidad
    /// máxima sembrada no hay contra qué comparar, y un modelo que no esté en el catálogo no se
    /// declara incompatible — se calla, que no es lo mismo.
    /// </summary>
    public static IReadOnlyList<string> AvisosDeCompatibilidad(
        IEnumerable<DerivadoILine> derivados,
        decimal? capacidadMaximaDerivadaA)
    {
        var lista = derivados.ToList();

        // El tope del derivado NO es cosa de I-Line, así que la comparación vive en su propia clase y
        // la comparte con los tableros de espacios numerados. Aquí solo se llama.
        var avisos = new List<string>(
            VerificacionDerivadoMaximo.Avisos(
                lista.Select(d => (d.Numero, d.ProteccionA)), capacidadMaximaDerivadaA));

        foreach (var d in lista)
        {
            if (d.ModeloEsGarraILine is false)
                avisos.Add(
                    $"El derivado {d.Numero} eligió el modelo {d.ModeloCatalogo}, que no se monta con garra " +
                    "I-Line; en este tablero el derivado se engarza al bus, no se atornilla a zapatas.");
        }

        return avisos;
    }

    /// <summary>
    /// Que la conexión declarada <b>se pueda pedir</b>, según las Tablas 9.121 y 9.133 del Digest.
    ///
    /// <para>
    /// <b>No es lo mismo que <see cref="MotivoFaseInvalida"/>, y por eso son dos avisos.</b> Aquel
    /// revisa la <b>forma</b> contra el tablero: cuántas letras, que existan, que no se repitan.
    /// Éste revisa el <b>catálogo</b>: <c>AC</c> y <c>CA</c> pasan los dos la revisión de forma —dos
    /// letras distintas, las dos existen— y los dos se pueden pedir, pero son <b>productos
    /// distintos</b>, y hay combinaciones bien formadas que sencillamente no se fabrican.
    /// </para>
    ///
    /// <para>
    /// <b>El orden importa y no se normaliza.</b> El catálogo ofrece <c>AB</c> y <c>BA</c> como
    /// opciones separadas porque cambia qué polo cae en qué fase. Si este programa "ordenara
    /// alfabéticamente" la fase declarada —que es la simplificación obvia— borraría la mitad del
    /// catálogo y haría ordenar el aparato equivocado.
    /// </para>
    /// </summary>
    public static IReadOnlyList<string> AvisosDeConexionPedible(IEnumerable<DerivadoILine> derivados) =>
    [
        .. derivados
            .Select(d => (d.Numero, Motivo: OpcionesFaseILine.MotivoConexionNoPedible(d.FaseDeclarada, d.Polos)))
            .Where(x => x.Motivo is not null)
            .Select(x => $"El derivado {x.Numero} {x.Motivo}")
    ];

    /// <summary>
    /// Si los derivados caben en el bus. <b>Es la verificación que un I-Line necesita y un NQ no.</b>
    ///
    /// <para>
    /// En un tablero de columnas NEMA se cuentan <b>espacios</b>, todos del mismo tamaño. En un
    /// I-Line se cuentan <b>pulgadas</b>, porque en el mismo bus se mezclan marcos de tamaños muy
    /// distintos: un marco R de 3 polos (15 plg) se come lo que tres marcos H de 3 polos (4.5 plg
    /// cada uno). Contar derivados contra <c>ModeloTablero.NumeroEspacios</c> daría verde a un
    /// tablero que no cierra físicamente.
    /// </para>
    ///
    /// <para>
    /// <b>De dónde sale el "número de circuitos" del catálogo, confirmado.</b> Los I-Line se publican
    /// por circuitos (8, 10, 14, 16, 18, 20, 22) y el sembrado los traduce a pulgadas multiplicando
    /// por <b>4.5</b>. La Tabla 9.109 confirma de dónde sale ese 4.5: son las pulgadas de un marco
    /// H o J de <b>3 polos</b>, o sea el derivado típico. Un "circuito" del catálogo es un espacio de
    /// 4.5 plg, no un interruptor cualquiera.
    /// </para>
    ///
    /// <para>
    /// <b>Un derivado sin medida no vale cero.</b> Si algún derivado no trae pulgadas —no eligió
    /// modelo, o su marco no está tabulado— la suma queda incompleta, y en vez de dar un veredicto
    /// falso el aviso <b>dice cuántos faltan</b>. Misma regla que el resto de la casa: null no es
    /// cero, y el silencio se declara.
    /// </para>
    /// </summary>
    public static IReadOnlyList<string> AvisosDeEspacio(
        IEnumerable<DerivadoILine> derivados,
        decimal? espacioMontajePlg)
    {
        var lista = derivados.ToList();
        if (lista.Count == 0) return [];

        var sinMedida = lista.Where(d => d.EspacioParaSumar is null).ToList();
        var ocupado = lista.Sum(d => d.EspacioParaSumar ?? 0m);

        var avisos = new List<string>();

        if (espacioMontajePlg is not { } disponible)
        {
            // Sin bus tabulado no hay contra qué comparar. Se calla, igual que la capacidad máxima
            // de derivado cuando el modelo de tablero no la publica.
            return avisos;
        }

        if (ocupado > disponible)
            avisos.Add(
                $"Los derivados ocupan {ocupado:0.##} plg de bus y este tablero tiene {disponible:0.##} plg " +
                $"(se pasa por {ocupado - disponible:0.##} plg). En un I-Line no se cuentan espacios: se " +
                "cuentan pulgadas, y un marco grande se come varios espacios de los chicos.");

        // Las dos medidas para el mismo derivado y distintas: manda el catálogo, y se dice cuál se
        // usó. Callarlo dejaría al usuario creyendo que su número es el que cuenta.
        foreach (var d in lista.Where(x => x.MedidasEnConflicto))
            avisos.Add(
                $"El derivado {d.Rotulo} tiene {d.EspacioCapturadoPlg:0.##} plg capturadas y su modelo " +
                $"({d.ModeloCatalogo}) mide {d.EspacioMontajePlg:0.##} plg según el catálogo. Se sumó la del " +
                "catálogo, que es la medida física del aparato; corrige la captura o cambia el modelo.");

        if (sinMedida.Count > 0)
            avisos.Add(
                $"La cuenta de pulgadas está incompleta: {sinMedida.Count} derivado(s) " +
                $"({string.Join(", ", sinMedida.Take(5).Select(d => d.Rotulo))}) no tienen medida de montaje, " +
                $"así que las {ocupado:0.##} plg contadas son un mínimo, no el total.");

        return avisos;
    }
}
