namespace PowerNode.DesignSuite.Domain.Norma;

/// <summary>Un artículo de la NOM-001-SEDE-2012 (p.ej. Artículo 310).</summary>
public class Articulo
{
    public int Num { get; set; }
    public int Capitulo { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int? Pagina { get; set; }
    public string? Alcance { get; set; }

    public List<ArticuloParte> Partes { get; set; } = [];
    public List<Seccion> Secciones { get; set; } = [];
    public List<Tabla> Tablas { get; set; } = [];
}
