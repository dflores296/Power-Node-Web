namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Base común (TPH) de todo el equipo que puede colgar en la topología de un proyecto: acometida,
/// tablero, transformador, protección, carga y centro de control de motores.
///
/// <b>Por qué "elemento" y no "nodo".</b> Hasta el 2026-08-15 esto se llamaba <c>NodoTopologia</c>, y
/// era un error de terminología eléctrica que señaló el usuario: en una red, un <b>nodo es un punto
/// de conexión</b> —una barra, un empalme—, no el aparato. Un transformador no es un nodo: tiene
/// dos, el primario y el secundario. Lo que se modela aquí es el equipo, así que se llama elemento,
/// y la palabra "nodo" queda libre para su significado real por si algún día se modela la barra del
/// tablero o los dos lados del transformador.
///
/// Antes de esta clase, <see cref="Alimentador"/> tenía cuatro llaves foráneas anulables
/// (TableroOrigen/TransformadorOrigen/TableroDestino/TransformadorDestino) con la invariante
/// "exactamente una de cada lado" sostenida a mano en cada sitio que armaba o leía un Alimentador.
/// Con TPH todos los tipos comparten tabla y por lo tanto espacio de Id -- un Alimentador solo
/// necesita ElementoOrigenId/ElementoDestinoId, y agregar un tipo de elemento nuevo es heredar de
/// esta clase, no tocar cada sitio que hoy distingue "es Tablero o Transformador" con un ternario.
/// </summary>
public abstract class ElementoTopologia
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;

    /// <summary>El alimentador que lo conecta con quien lo alimenta. Null si es la raíz del proyecto (la acometida).</summary>
    public Alimentador? AlimentadorEntrante { get; set; }

    /// <summary>Los alimentadores que salen de este elemento hacia sus elementos hijos.</summary>
    public List<Alimentador> AlimentadoresSalientes { get; set; } = [];

    // ------------------------------------------------------------------ dónde va en el dibujo
    //
    // NULL = no anclado, o sea "acomódalo tú". Es el estado normal: el diagrama se acomoda solo
    // (quien alimenta arriba, hermanos lado a lado) y no hay nada que capturar ni que mantener.
    //
    // En cuanto el usuario arrastra un elemento a un lugar libre, ESE elemento queda anclado ahí y
    // se guarda su posición; lo que cuelga de él lo sigue, y todo lo que no se tocó sigue fluyendo
    // solo. El botón "Acomodar todo" borra los anclajes.
    //
    // Es a propósito un híbrido y no un lienzo libre: en un lienzo libre un proyecto de treinta
    // elementos se acomoda a mano completo y no hay a quién pedirle que lo ordene. Así, lo que no
    // tocaste nunca se despeina.
    //
    // Son coordenadas del dibujo, no de la pantalla: el zoom no las cambia.

    /// <summary>Posición horizontal fijada a mano. Null = la calcula el acomodo automático.</summary>
    public double? PosicionX { get; set; }

    /// <summary>Posición vertical fijada a mano. Null = la calcula el acomodo automático.</summary>
    public double? PosicionY { get; set; }

    /// <summary>Atajo: ¿este elemento tiene posición propia, o fluye con el acomodo automático?</summary>
    public bool EstaAnclado => PosicionX is not null && PosicionY is not null;
}
