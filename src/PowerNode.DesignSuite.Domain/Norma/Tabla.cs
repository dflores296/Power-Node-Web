namespace PowerNode.DesignSuite.Domain.Norma;

/// <summary>
/// Una tabla de la norma (p.ej. "310-15(b)(16)", ampacidades). El id es la cita misma:
/// el motor de cálculo y la memoria de cálculo citan directo "según Tabla {Id}, NOM-001-SEDE-2012".
/// </summary>
public class Tabla
{
    public string Id { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string? Introduccion { get; set; }

    public int? ArticuloNum { get; set; }
    public Articulo? Articulo { get; set; }

    public bool Informativa { get; set; }
    public int Columnas { get; set; }
    public int FilasEncabezado { get; set; }
    public int? Pagina { get; set; }
    public double? Calidad { get; set; }

    /// <summary>Fecha de verificación celda por celda contra el PDF original (data/tablas_revisadas.json), si existe.</summary>
    public DateOnly? Verificada { get; set; }

    public List<TablaFila> Filas { get; set; } = [];
    public List<TablaNota> Notas { get; set; } = [];
}
