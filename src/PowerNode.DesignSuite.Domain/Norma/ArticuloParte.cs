namespace PowerNode.DesignSuite.Domain.Norma;

/// <summary>Una parte (A, B, C...) dentro de un artículo, p.ej. "Parte B. Instalación".</summary>
public class ArticuloParte
{
    public int Id { get; set; }
    public int ArticuloNum { get; set; }
    public Articulo Articulo { get; set; } = null!;

    public string Letra { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public int? Pagina { get; set; }
}
