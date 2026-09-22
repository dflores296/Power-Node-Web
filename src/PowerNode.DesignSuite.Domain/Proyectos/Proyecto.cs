namespace PowerNode.DesignSuite.Domain.Proyectos;

public class Proyecto
{
    public int Id { get; set; }
    public string Cliente { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Ubicacion { get; set; }
    public string? Ingeniero { get; set; }
    public DateOnly Fecha { get; set; }

    /// <summary>
    /// Todos los elementos de la topología del proyecto, sin importar su tipo — es LA colección
    /// persistida (antes eran dos, <c>Tableros</c> y <c>Transformadores</c>, y cada tipo de elemento
    /// nuevo habría exigido una tercera, una cuarta...). Para agregar un elemento al proyecto, agrégalo
    /// aquí: <c>proyecto.Elementos.Add(tablero)</c>.
    /// </summary>
    public List<ElementoTopologia> Elementos { get; set; } = [];

    /// <summary>
    /// Vista de solo lectura de los elementos que son tableros. NO está mapeada a la base (ver
    /// <c>ProyectoConfiguration</c>) y es <see cref="IEnumerable{T}"/> a propósito, no
    /// <see cref="List{T}"/>: si fuera lista, <c>proyecto.Tableros.Add(x)</c> compilaría y no
    /// haría nada (agregaría a una copia temporal), que es justo el bug silencioso que hay que
    /// evitar. Así, el compilador obliga a usar <see cref="Elementos"/>.
    /// </summary>
    public IEnumerable<Tablero> Tableros => Elementos.OfType<Tablero>();

    /// <summary>Vista de solo lectura de los elementos que son transformadores. Ver <see cref="Tableros"/>.</summary>
    public IEnumerable<Transformador> Transformadores => Elementos.OfType<Transformador>();

    public ConfiguracionProyecto? Configuracion { get; set; }

    /// <summary>
    /// Lo que va en la portada de la memoria de cálculo y no sale de ningún cálculo: quién firma,
    /// para qué trámite es, los folios de cada dependencia. Nulo mientras nadie lo capture — la
    /// memoria se genera igual, con la portada de identificación mínima.
    /// </summary>
    public Entregables.DatosPortada? Portada { get; set; }

    /// <summary>El membrete de quien emite. Ver <see cref="Entregables.DatosDespacho"/>.</summary>
    public Entregables.DatosDespacho? Despacho { get; set; }
}
