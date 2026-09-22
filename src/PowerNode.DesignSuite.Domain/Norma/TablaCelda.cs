namespace PowerNode.DesignSuite.Domain.Norma;

public class TablaCelda
{
    public int Id { get; set; }
    public int TablaFilaId { get; set; }
    public TablaFila TablaFila { get; set; } = null!;

    /// <summary>Índice de columna donde inicia la celda (0-based).</summary>
    public int Indice { get; set; }
    public string Texto { get; set; } = string.Empty;

    /// <summary>Rowspan/colspan tal cual venían en el PDF original (celdas combinadas de encabezado).</summary>
    public int RowSpan { get; set; } = 1;
    public int ColSpan { get; set; } = 1;
}
