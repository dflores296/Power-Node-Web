using PowerNode.DesignSuite.Domain.Proyectos;

namespace PowerNode.DesignSuite.Domain.Advertencias;

/// <summary>
/// Una advertencia del cálculo, ya desarmada en sus tres partes: de qué elemento salió, qué artículo
/// la sustenta y qué dice.
/// </summary>
/// <param name="Elemento">Nombre del elemento que la produjo — "Tablero TG", "TR-1", "CCM-A".</param>
/// <param name="Tipo">Qué clase de elemento es, para que el nombre no quede huérfano en el listado.</param>
/// <param name="Referencia">El artículo entre corchetes que el motor antepone. Vacío si el aviso no traía.</param>
/// <param name="Mensaje">El texto del aviso, ya sin el corchete ni la marca del subelemento.</param>
/// <param name="Subelemento">
/// <b>Dentro del elemento, quién exactamente</b> — "Circuito 12", "Interruptor QOB220". Vacío cuando
/// el aviso es del elemento entero y no de una parte suya.
/// </param>
/// <summary>
/// De qué clase de verificación viene un aviso. <b>No es decoración: decide dónde se cuenta y dónde
/// se imprime.</b>
/// </summary>
public enum ClaseAviso
{
    /// <summary>
    /// Del cálculo eléctrico — capacidad interruptiva, 110-14, 430-62, desbalanceo, caída acumulada.
    /// Son los que viajan a la memoria y al cuadro de carga.
    /// </summary>
    Calculo,

    /// <summary>
    /// Un dictamen de coordinación que <b>encontró algo</b>: selectividad parcial o nula, o un
    /// conductor que la protección no alcanza a proteger.
    /// </summary>
    Coordinacion,

    /// <summary>
    /// <b>Falta un dato para poder dictaminar.</b> Se separa de <see cref="Coordinacion"/> porque
    /// «no sé» y «no cumple» nunca se colapsan — es la regla que sostiene todo el motor. En un
    /// proyecto a medio capturar esto es el estado <i>normal</i>, no un defecto, y pintarlo como
    /// advertencia haría que lo normal se viera como error.
    /// </summary>
    Pendiente,
}

public readonly record struct Aviso(
    string Elemento,
    string Tipo,
    string Referencia,
    string Mensaje,
    string Subelemento = "",
    ClaseAviso Clase = ClaseAviso.Calculo)
{
    /// <summary>
    /// La marca con la que el motor señala de qué parte del elemento habla un aviso: comillas
    /// españolas justo después del corchete del artículo.
    ///
    /// <para>
    /// <b>Por qué una marca en el texto y no un campo de la base.</b> Las advertencias se guardan
    /// como un solo <c>string</c> por cálculo, un aviso por renglón — eso es lo que hay y cambiarlo
    /// sería otra migración por una ganancia que no se ve. Lo que sí importa es que el subelemento
    /// deje de ser <b>parte de la frase</b> («Circuito 12: el conductor no cabe…»), porque así nadie
    /// puede agruparlo, ordenarlo ni saltar a él: era prosa, no dato.
    /// </para>
    ///
    /// <para>
    /// Se eligieron «» y no corchetes ni llaves porque el corchete ya está tomado por el artículo y
    /// la llave aparece en textos técnicos. Un aviso sin marca es legítimo y frecuente: el
    /// desbalanceo, por ejemplo, es del tablero entero.
    /// </para>
    /// </summary>
    public const char AbreSubelemento = '«';

    /// <inheritdoc cref="AbreSubelemento"/>
    public const char CierraSubelemento = '»';

    /// <summary>
    /// El renglón como se imprime en papel: referencia, subelemento y mensaje. <b>El elemento va
    /// aparte</b> (los entregables lo usan de encabezado y agrupan por él), pero el subelemento sí
    /// entra aquí — en el papel no hay a dónde saltar con un clic, así que si no se imprime, se
    /// pierde de qué circuito hablaba.
    /// </summary>
    public string Texto
    {
        get
        {
            var cuerpo = string.IsNullOrEmpty(Subelemento) ? Mensaje : $"{Subelemento}: {Mensaje}";
            return string.IsNullOrEmpty(Referencia) ? cuerpo : $"[{Referencia}] {cuerpo}";
        }
    }

    /// <summary>
    /// Cómo el motor marca un aviso que es de una parte del elemento. La marca va <b>después</b> del
    /// corchete del artículo, para que la referencia siga siendo lo primero que se lee.
    /// </summary>
    public static string Marcar(string subelemento, string linea)
    {
        if (string.IsNullOrWhiteSpace(subelemento))
            return linea;

        var marca = $"{AbreSubelemento}{subelemento.Trim()}{CierraSubelemento} ";

        return linea.StartsWith('[') && linea.IndexOf(']') is var cierre && cierre > 1
            ? linea[..(cierre + 1)] + " " + marca + linea[(cierre + 1)..].TrimStart()
            : marca + linea;
    }
}

/// <summary>
/// Recoge <b>todas</b> las advertencias que el motor guardó en un proyecto calculado, para que los
/// entregables las impriman.
///
/// <para>
/// <b>Por qué existe.</b> Hasta la auditoría del 2026-08-19 (§3.1) el motor producía advertencias con
/// nombre y artículo —capacidad interruptiva insuficiente contra la falla disponible, 110-14 con el
/// conductor que no cabe en la terminal, 240-86, 430-94, desbalanceo, conflictos de barras NEMA— las
/// guardaba en <c>CalculoTablero.Advertencias</c>, <c>CalculoCcm.Advertencias</c>,
/// <c>CalculoTransformador.Advertencias</c> y <c>CalculoProteccion.Advertencias</c>, y
/// <b>ningún entregable las imprimía</b>. El documento que se sella salía limpio aunque el cálculo
/// hubiera detectado que un interruptor no aguanta la falla de su punto. El programa lo sabía, lo
/// guardaba, y el papel no lo decía.
/// </para>
///
/// <para>
/// <b>Un solo recolector para los entregables Y PARA LA PANTALLA.</b> La memoria en Word, el cuadro
/// de carga en Excel y el panel de avisos del área de trabajo imprimen lo mismo desde aquí; si mañana
/// un elemento nuevo gana advertencias, se conecta en <see cref="DeElemento"/> y aparece en los tres
/// a la vez. Que un entregable las imprima y el otro no es exactamente el defecto que esto cierra, y
/// que la pantalla dijera algo distinto del papel sería peor que el defecto original.
/// </para>
///
/// <para>
/// <b>Por qué vive en Domain y no en Exportacion, donde nació.</b> No tiene una línea de OOXML ni de
/// Excel: es un recorrido del grafo de dominio y nada más. Se mudó el 2026-08-19 al conectarlo a la
/// interfaz, porque la UI no referencia Exportacion y la alternativa —escribir un segundo recolector
/// para la pantalla— es justo lo que no hay que hacer.
/// </para>
///
/// <para>
/// <b>Formato de entrada.</b> El motor escribe las advertencias como texto, un aviso por renglón, casi
/// siempre con la forma <c>[artículo] mensaje</c> (ver <c>CascadaCalculoService</c>). Aquí se separan
/// por renglón y se le quita el corchete al frente para poder darle formato propio. Un aviso sin
/// corchete se respeta tal cual — no se inventa una referencia.
/// </para>
/// </summary>
public static class AdvertenciasDelProyecto
{
    /// <summary>
    /// Todas las advertencias del proyecto, en el orden de la topología: acometida y raíces primero,
    /// y dentro de cada tipo por nombre, para que dos corridas del mismo proyecto den el mismo
    /// documento.
    /// </summary>
    public static IReadOnlyList<Aviso> Recolectar(Proyecto proyecto) =>
        proyecto.Elementos
            .OrderBy(e => OrdenDelTipo(e))
            .ThenBy(e => e.Nombre, StringComparer.OrdinalIgnoreCase)
            .SelectMany(DeElemento)
            .ToList();

    /// <summary>
    /// Las advertencias de un elemento. <b>Es el único lugar que sabe dónde las guarda cada tipo</b>:
    /// conectar un tipo nuevo es agregarle un brazo a este <c>switch</c>, y con eso sale en los dos
    /// entregables.
    ///
    /// <para>
    /// <see cref="Carga"/> y <see cref="Acometida"/> no aparecen porque <c>CalculoCarga</c> no tiene
    /// campo de advertencias y la acometida no tiene cálculo propio. No es un olvido: no hay nada que
    /// leer todavía.
    /// </para>
    /// </summary>
    public static IReadOnlyList<Aviso> DeElemento(ElementoTopologia elemento)
    {
        var (tipo, texto) = elemento switch
        {
            Tablero t => ("Tablero", t.Calculo?.Advertencias),
            Transformador t => ("Transformador", t.Calculo?.Advertencias),
            CentroControlMotores c => ("CCM", c.Calculo?.Advertencias),
            _ => (string.Empty, null),
        };

        var avisos = string.IsNullOrWhiteSpace(texto)
            ? new List<Aviso>()
            : texto
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(linea => Desarmar(elemento.Nombre, tipo, linea))
                .ToList();

        // EL DESBALANCEO SE GUARDA COMO bool, NO COMO TEXTO. Lo escribe CalculoTableroService (no la
        // cascada), y por eso nunca cayó en la cadena de Advertencias. Es una advertencia de pleno
        // derecho -- la auditoría §3.1 la lista entre las que no salían en ningún entregable -- así
        // que se arma aquí a partir del dato persistido, en vez de dejar que cada exportador la
        // resuelva por su cuenta con un `if` propio (que es lo que hacía el cuadro de carga).
        //
        // Sin referencia a propósito: LA NORMA NO FIJA UMBRAL de desbalanceo. El límite sale de
        // ConfiguracionProyecto.DesbalanceoMaxPct, que es criterio de diseño, y ponerle un artículo
        // encima sería atribuirle a la NOM algo que no dice.
        if (elemento is Tablero { Calculo: { DesbalanceoExcedeLimite: true } calculoTablero })
            avisos.Add(new Aviso(
                elemento.Nombre,
                tipo,
                string.Empty,
                $"El desbalanceo entre fases es de {calculoTablero.DesbalanceoPct:N2} % y excede el límite " +
                "configurado en el proyecto. Revisa el reparto de carga entre barras."));

        return avisos;
    }

    /// <summary>¿Este proyecto calculado trae alguna advertencia? Atajo para no armar la sección vacía.</summary>
    public static bool HayAlguna(Proyecto proyecto) => Recolectar(proyecto).Count > 0;

    /// <summary>
    /// Separa <c>[240-86] el derivado no coordina</c> en referencia y mensaje. Solo reconoce el
    /// corchete <b>al principio</b> del renglón: un corchete a media frase es parte del mensaje.
    /// </summary>
    private static Aviso Desarmar(string elemento, string tipo, string linea)
    {
        var referencia = string.Empty;

        if (linea.StartsWith('[') && linea.IndexOf(']') is var cierre && cierre > 1)
        {
            referencia = linea[1..cierre].Trim();
            linea = linea[(cierre + 1)..].Trim();
        }

        var subelemento = string.Empty;

        // La marca «Circuito 12» solo cuenta AL PRINCIPIO del mensaje, igual que el corchete: unas
        // comillas a media frase son parte de lo que el aviso dice, no una etiqueta.
        if (linea.StartsWith(Aviso.AbreSubelemento)
            && linea.IndexOf(Aviso.CierraSubelemento) is var fin && fin > 1)
        {
            subelemento = linea[1..fin].Trim();
            linea = linea[(fin + 1)..].Trim();
        }

        return new Aviso(elemento, tipo, referencia, linea, subelemento);
    }

    /// <summary>
    /// El orden de los tipos en el listado: de la acometida hacia abajo, que es como se lee un
    /// unifilar. No hay significado eléctrico en el número, solo estabilidad del documento.
    /// </summary>
    private static int OrdenDelTipo(ElementoTopologia elemento) => elemento switch
    {
        Acometida => 0,
        Transformador => 1,
        Tablero => 2,
        CentroControlMotores => 3,
        Carga => 5,
        _ => 6,
    };
}
