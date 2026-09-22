namespace PowerNode.DesignSuite.Domain.Norma;

public class TablaFila
{
    public int Id { get; set; }
    public string TablaId { get; set; } = string.Empty;
    public Tabla Tabla { get; set; } = null!;

    /// <summary>Índice de la fila dentro de la tabla, incluyendo las de encabezado (0-based).</summary>
    public int Indice { get; set; }

    public List<TablaCelda> Celdas { get; set; } = [];
}
