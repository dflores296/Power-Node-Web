namespace PowerNode.DesignSuite.Calculo.Tableros;

/// <summary>
/// Cómo se pide la <b>conexión de fases</b> de un derivado I-Line: es un <b>sufijo del número de
/// catálogo</b>, no una consecuencia de dónde se engarza el interruptor.
///
/// <para>
/// <b>Fuentes: Tabla 9.121</b> (<i>"Phase Options Suffix Numbers for B/Q-frame Circuit Breakers"</i>,
/// pág. 9-61) y <b>Tabla 9.133</b> (<i>"H/J/L-Frame Circuit Breaker/Switch Phase Options"</i>,
/// pág. 9-65), las dos del Digest 178 sección 09, que está en el repo. Las dos publican <b>el mismo
/// mapa</b> para 2 y 3 polos; el de un polo solo lo trae la 9.121, porque de marco H para arriba no
/// hay versión de un polo.
/// </para>
///
/// <para>
/// <b>Esto cierra —y reformula— el hueco que A9 dejó abierto.</b> El plan decía: <i>"falta la
/// secuencia de fases del bus por altura; sin eso no se puede derivar a qué barra cae un interruptor
/// según dónde se engarza"</i>. Esa pregunta <b>no tiene respuesta porque está mal planteada</b>: la
/// fase de un derivado I-Line <b>no se deriva, se pide</b>. Por eso <c>CircuitoDerivado.Fase</c> declarada en
/// I-Line era la decisión correcta, y por eso <c>AsignacionBarrasService</c> no debe escribirla ahí.
/// </para>
///
/// <para>
/// <b>Lo que esta clase deliberadamente NO hace: decodificar un número de catálogo.</b> Es tentador
/// —"el último dígito es el sufijo"— y es una trampa: <c>BDA14015</c> es un 15 A sin sufijo y
/// <c>BDA140155</c> es el mismo 15 A con sufijo 5 (fase C), pero <c>BDA14015</c> <b>también termina
/// en 5</b>. La frontera entre el amperaje y el sufijo depende del largo de la base, que cambia de
/// familia en familia (<c>QBA32070</c>, <c>HDA26150</c>, <c>JDA34250WU31X</c>). Adivinarla produciría
/// una conexión de fases inventada, que es exactamente lo que este proyecto no hace con los datos de
/// catálogo. La conexión se <b>arma</b> aquí (base + sufijo), nunca se lee de vuelta a ciegas.
/// </para>
/// </summary>
public static class OpcionesFaseILine
{
    /// <summary>
    /// El mapa impreso, por número de polos. Se guarda como lista y no como diccionario para que el
    /// <b>orden</b> del renglón se conserve tal cual: la tabla ofrece <c>AB</c> y <c>BA</c> como
    /// opciones <b>distintas</b>, y esa distinción es justo lo que se perdería al normalizar.
    /// </summary>
    private static readonly (int Polos, char Sufijo, string Conexion)[] Opciones =
    [
        // Tabla 9.121, un polo. Solo marco B: de H para arriba no hay derivados de un polo.
        (1, '1', "A"),
        (1, '3', "B"),
        (1, '5', "C"),

        // Tablas 9.121 y 9.133, dos polos. Coinciden dígito por dígito en las dos tablas.
        (2, '1', "AB"),
        (2, '2', "AC"),
        (2, '3', "BA"),
        (2, '4', "BC"),
        (2, '5', "CA"),
        (2, '6', "CB"),

        // Tres polos: la única opción con sufijo es la inversa. Ver EsEstandarDeTresPolos.
        (3, '6', "CBA"),
    ];

    /// <summary>
    /// La conexión que trae un interruptor de tres polos <b>sin sufijo</b>.
    ///
    /// <b>Es texto de la norma del fabricante, no un supuesto.</b> La nota [43] de la Tabla 9.121 lo
    /// dice literal: <i>"The absence of a phase option number after a 3-pole catalog number will
    /// result in an ABC phase connection."</i> Y la nota [46] de la 9.122 lo repite para el marco Q.
    /// </summary>
    public const string EstandarTresPolos = "ABC";

    /// <summary>Las conexiones que el catálogo ofrece para ese número de polos, en el orden impreso.</summary>
    public static IReadOnlyList<string> ConexionesDisponibles(int polos) =>
        polos == 3
            ? [EstandarTresPolos, .. Opciones.Where(o => o.Polos == 3).Select(o => o.Conexion)]
            : [.. Opciones.Where(o => o.Polos == polos).Select(o => o.Conexion)];

    /// <summary>
    /// El sufijo que hay que agregarle al número de catálogo para pedir esa conexión, o <c>null</c>
    /// si el catálogo no la ofrece.
    ///
    /// <para>
    /// <b>Ojo con el caso de tres polos estándar:</b> devuelve <c>null</c> igual que una conexión
    /// inexistente, pero por la razón contraria — no lleva sufijo porque <b>es</b> el estándar. Para
    /// distinguirlos está <see cref="EsConexionValida"/>, y por eso quien redacta el número debe usar
    /// <see cref="NumeroConFase"/> en vez de concatenar a mano.
    /// </para>
    /// </summary>
    public static char? SufijoPara(string? conexion, int polos)
    {
        var buscada = Normalizar(conexion);
        if (buscada is null) return null;

        foreach (var o in Opciones)
            if (o.Polos == polos && o.Conexion == buscada)
                return o.Sufijo;

        return null;
    }

    /// <summary>La conexión que pide ese sufijo con ese número de polos, o <c>null</c> si no existe.</summary>
    public static string? ConexionDeSufijo(char sufijo, int polos)
    {
        foreach (var o in Opciones)
            if (o.Polos == polos && o.Sufijo == sufijo)
                return o.Conexion;

        return null;
    }

    /// <summary>
    /// Si esa conexión se puede <b>pedir</b> con ese número de polos. Distinto de que sea una
    /// combinación de letras bien formada: <c>AA</c> está mal formada, pero <c>ABC</c> con dos polos
    /// está bien formada y aun así <b>no se puede ordenar</b>.
    /// </summary>
    public static bool EsConexionValida(string? conexion, int polos)
    {
        var buscada = Normalizar(conexion);
        if (buscada is null) return false;

        return (polos == 3 && buscada == EstandarTresPolos) || SufijoPara(buscada, polos) is not null;
    }

    /// <summary>
    /// El número de catálogo completo para pedir esa conexión, partiendo del número base (el que
    /// aparece impreso en la tabla, sin sufijo de fase). <c>null</c> cuando la conexión no se ofrece.
    ///
    /// <para>
    /// El estándar de tres polos devuelve el número <b>tal cual</b>, sin agregarle nada: eso es lo
    /// que dice la nota [43]. Es el único caso en que la función devuelve su entrada sin cambios, y
    /// es correcto — no es un "no encontré".
    /// </para>
    /// </summary>
    public static string? NumeroConFase(string? numeroBase, string? conexion, int polos)
    {
        if (string.IsNullOrWhiteSpace(numeroBase)) return null;

        var buscada = Normalizar(conexion);
        if (buscada is null) return null;

        if (polos == 3 && buscada == EstandarTresPolos)
            return numeroBase.Trim();

        return SufijoPara(buscada, polos) is { } sufijo ? numeroBase.Trim() + sufijo : null;
    }

    /// <summary>
    /// Por qué esa conexión no se puede pedir con ese número de polos, o <c>null</c> si sí se puede.
    /// El texto se redacta para caer después del número del derivado ("El derivado 3 …"), igual que
    /// el resto de <see cref="VerificacionesILine"/>.
    ///
    /// <b>Es aviso, no bloqueo</b>, y <b>calla cuando no hay nada declarado</b>: de eso ya se queja
    /// <see cref="VerificacionesILine.MotivoFaseInvalida"/>, y dos avisos por la misma omisión es
    /// ruido.
    /// </summary>
    public static string? MotivoConexionNoPedible(string? conexion, int polos)
    {
        var buscada = Normalizar(conexion);
        if (buscada is null) return null;

        if (EsConexionValida(buscada, polos)) return null;

        var disponibles = ConexionesDisponibles(polos);
        if (disponibles.Count == 0)
            return $"declara la conexión {buscada}, y el catálogo no publica opciones de fase para " +
                   $"interruptores de {polos} polo(s) en I-Line.";

        return $"declara la conexión {buscada}, que el catálogo no ofrece para {polos} polo(s). " +
               $"Las que sí se pueden pedir son: {string.Join(", ", disponibles)}.";
    }

    /// <summary>
    /// A mayúsculas y sin espacios, o <c>null</c> si viene vacío. <b>No reordena las letras</b>: en
    /// esta tabla <c>AB</c> y <c>BA</c> son productos distintos.
    /// </summary>
    private static string? Normalizar(string? conexion) =>
        string.IsNullOrWhiteSpace(conexion) ? null : conexion.Trim().ToUpperInvariant();
}
