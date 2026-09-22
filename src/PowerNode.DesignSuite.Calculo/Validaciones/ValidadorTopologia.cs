namespace PowerNode.DesignSuite.Calculo.Validaciones;

/// <summary>
/// Identidad de un elemento de la topología. Tablero y Transformador son tablas separadas, así que
/// sus Id numéricos pueden coincidir (Tablero #1 y Transformador #1 no son el mismo elemento) — Tipo
/// los distingue. Tipo es literal "Tablero" o "Transformador".
/// </summary>
public readonly record struct ElementoId(string Tipo, int Id)
{
    public override string ToString() => $"{Tipo} {Id}";
}

/// <summary>Un elemento del grafo (Tablero o Transformador) visto solo por su conexión: quién lo alimenta.</summary>
public sealed record ElementoTopologia(ElementoId Id, ElementoId? OrigenId);

public sealed record ProblemaTopologia(string Codigo, string Descripcion, IReadOnlyList<ElementoId> ElementosInvolucrados);

/// <summary>
/// Valida la topología de un proyecto (tableros y transformadores): los elementos se pueden conectar
/// en cualquier orden, sueltos, pero el sistema debe avisar si algo no cuadra — auto-referencia,
/// un origen que no existe en el proyecto, o un ciclo de alimentación.
/// </summary>
public static class ValidadorTopologia
{
    public static IReadOnlyList<ProblemaTopologia> Validar(IReadOnlyList<ElementoTopologia> elementos)
    {
        var problemas = new List<ProblemaTopologia>();
        var porId = elementos.ToDictionary(n => n.Id);

        foreach (var n in elementos)
        {
            if (n.OrigenId == n.Id)
            {
                problemas.Add(new ProblemaTopologia(
                    "AUTO_REFERENCIA", $"El {n.Id} se alimenta a sí mismo.", [n.Id]));
                continue;
            }

            if (n.OrigenId is ElementoId origenId && !porId.ContainsKey(origenId))
            {
                problemas.Add(new ProblemaTopologia(
                    "ORIGEN_INEXISTENTE",
                    $"El {n.Id} dice que lo alimenta {origenId}, que no está en este proyecto.",
                    [n.Id]));
            }
        }

        // Elementos cuyo camino hacia arriba ya se sabe a dónde da a parar: raíz, origen inexistente, o
        // parte de un ciclo ya reportado. Los auto-referenciados ya se reportaron arriba —
        // sembrarlos aquí evita que el recorrido de ciclos los vuelva a marcar como "CICLO".
        var procesados = new HashSet<ElementoId>(elementos.Where(n => n.OrigenId == n.Id).Select(n => n.Id));

        foreach (var inicio in elementos)
        {
            if (procesados.Contains(inicio.Id)) continue;

            var camino = new List<ElementoId>();
            var posicion = new Dictionary<ElementoId, int>();
            var actual = inicio;

            while (true)
            {
                if (procesados.Contains(actual.Id))
                {
                    // Este elemento ya se resolvió en una pasada anterior (raíz, ciclo ajeno, etc.) —
                    // todo lo que llevamos recorrido en este camino cuelga de algo ya válido.
                    foreach (var id in camino) procesados.Add(id);
                    break;
                }

                if (posicion.TryGetValue(actual.Id, out var idx))
                {
                    var ciclo = camino.Skip(idx).ToList();
                    foreach (var id in camino) procesados.Add(id); // el ciclo y lo que colgaba antes de él
                    problemas.Add(new ProblemaTopologia(
                        "CICLO", $"Ciclo de alimentación: {string.Join(" -> ", ciclo)} -> {actual.Id}.", ciclo));
                    break;
                }

                posicion[actual.Id] = camino.Count;
                camino.Add(actual.Id);

                if (actual.OrigenId is not ElementoId siguienteId || !porId.TryGetValue(siguienteId, out var siguiente))
                {
                    foreach (var id in camino) procesados.Add(id); // llegó a la raíz, o el origen no existe (ya reportado aparte)
                    break;
                }

                actual = siguiente;
            }
        }

        return problemas;
    }
}
