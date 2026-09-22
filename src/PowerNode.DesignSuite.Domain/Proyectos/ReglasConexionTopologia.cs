namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Quién puede alimentar a quién en la topología. <b>Un solo lugar</b>, porque a partir de ahora hay
/// dos maneras de conectar dos elementos —el combo de la pestaña Alimentación y arrastrar una caja
/// sobre otra en el diagrama— y dos maneras de decidir lo mismo terminan divergiendo.
///
/// Las reglas no son de estilo: describen la instalación real.
///
/// <list type="bullet">
/// <item><b>De una <see cref="Carga"/> no cuelga nada.</b> Es el final del circuito — el chiller, la
/// bomba, el elevador. Lo dijo el usuario y ya estaba implícito en el modelo: una Carga es UN equipo,
/// no una lista de elementos (docs/estado/03-decisiones-cerradas.md). Colgarle algo produciría un
/// alimentador que en la obra no existe, y la cascada le sumaría carga a un equipo que no
/// distribuye.</item>
/// <item><b>Una <see cref="Acometida"/> no cuelga de nadie.</b> Es el punto de entrega de la
/// suministradora, o sea el principio del proyecto; no hay nada aguas arriba que dibujar. Es la misma
/// regla vista del otro lado.</item>
/// <item><b>Nada se alimenta de sí mismo, ni de su propio descendiente.</b> La cascada ya detectaba
/// los lazos, pero recién al calcular y con una excepción; aquí se rechazan cuando se cometen, que es
/// cuando el usuario puede corregirlos.</item>
/// </list>
///
/// Lo que a propósito <b>no</b> se restringe: qué tipos puede tener adentro un CCM (sus unidades son
/// Cargas, pero nada impide un diseño con una protección en medio) y cuántos hijos admite una
/// Protección. Prohibirlos sería inventar reglas que la norma no pide.
/// </summary>
public static class ReglasConexionTopologia
{
    /// <summary>¿De este elemento puede colgar algo? Falso solo para la <see cref="Carga"/>, que es terminal.</summary>
    public static bool PuedeAlimentar(ElementoTopologia elemento) => elemento is not Carga;

    /// <summary>¿Este elemento puede colgar de alguien? Falso solo para la <see cref="Acometida"/>, que es la raíz.</summary>
    public static bool PuedeSerAlimentado(ElementoTopologia elemento) => elemento is not Acometida;

    /// <summary>
    /// Por qué NO se puede conectar <paramref name="destino"/> aguas abajo de <paramref name="origen"/>,
    /// en una frase que se le puede enseñar al usuario tal cual. <c>null</c> = sí se puede.
    ///
    /// <paramref name="quienAlimentaA"/> resuelve el padre actual de un elemento. Se recibe como
    /// función y no se lee de <see cref="ElementoTopologia.AlimentadorEntrante"/> porque durante la
    /// edición hay cambios que todavía no se guardan: la interfaz apunta el origen por Id y deja la
    /// navegación en null hasta el siguiente Guardar, así que el grafo bueno lo tiene quien edita, no
    /// las entidades.
    /// </summary>
    public static string? PorQueNoSePuedeConectar(
        ElementoTopologia origen,
        ElementoTopologia destino,
        Func<ElementoTopologia, ElementoTopologia?> quienAlimentaA)
    {
        if (ReferenceEquals(origen, destino))
            return $"«{destino.Nombre}» no puede alimentarse de sí mismo.";

        if (!PuedeAlimentar(origen))
            return $"«{origen.Nombre}» es una carga: es el final del circuito y de ella no cuelga nada. " +
                   "Si ahí va a haber varios equipos, lo que corresponde es un tablero.";

        if (!PuedeSerAlimentado(destino))
            return $"«{destino.Nombre}» es la acometida: es el punto de entrega de la suministradora, " +
                   "así que es el principio del proyecto y no cuelga de nada.";

        if (EsDescendiente(origen, destino, quienAlimentaA))
            return $"«{origen.Nombre}» ya se alimenta (directa o indirectamente) de «{destino.Nombre}». " +
                   "Eso haría un lazo en la topología.";

        return null;
    }

    /// <summary>
    /// ¿<paramref name="posibleDescendiente"/> cuelga —directa o indirectamente— de
    /// <paramref name="ancestro"/>? Se camina hacia arriba siguiendo quién alimenta a quién.
    /// </summary>
    private static bool EsDescendiente(
        ElementoTopologia posibleDescendiente,
        ElementoTopologia ancestro,
        Func<ElementoTopologia, ElementoTopologia?> quienAlimentaA)
    {
        // Por referencia: durante la edición hay elementos recién creados que todavía tienen Id 0,
        // así que compararlos por Id los haría ver a todos como el mismo.
        var visitados = new HashSet<ElementoTopologia>(ReferenceEqualityComparer.Instance);
        var actual = posibleDescendiente;

        while (actual is not null)
        {
            if (ReferenceEquals(actual, ancestro))
                return true;

            // Lazo preexistente: no lo introduce este cambio, así que no es este cambio el que hay
            // que rechazar. Se corta para no dar vueltas para siempre.
            if (!visitados.Add(actual))
                return false;

            actual = quienAlimentaA(actual);
        }

        return false;
    }
}
