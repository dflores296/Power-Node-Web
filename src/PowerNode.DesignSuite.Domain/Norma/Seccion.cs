namespace PowerNode.DesignSuite.Domain.Norma;

/// <summary>
/// Una sección o inciso de la norma (p.ej. "310-15(b)(16)"). El árbol de incisos del
/// corpus original se aplana aquí en una sola tabla auto-referenciada por <see cref="PadreId"/>,
/// para poder consultar cualquier nivel sin recorrer JSON anidado.
/// </summary>
public class Seccion
{
    /// <summary>El id de la norma tal cual, p.ej. "310-15(b)(3)(a)(4)". Es también la cita.</summary>
    public string Id { get; set; } = string.Empty;

    public int ArticuloNum { get; set; }
    public Articulo Articulo { get; set; } = null!;

    public string? PadreId { get; set; }
    public Seccion? Padre { get; set; }
    public List<Seccion> Hijos { get; set; } = [];

    public string? Parte { get; set; }
    public string? Etiqueta { get; set; }
    public string? Tipo { get; set; }
    public int Nivel { get; set; }
    public string? Titulo { get; set; }
    public string? Texto { get; set; }
    public int? Pagina { get; set; }

    public List<SeccionAnotacion> Anotaciones { get; set; } = [];
    public List<SeccionDefinicion> Definiciones { get; set; } = [];
}
