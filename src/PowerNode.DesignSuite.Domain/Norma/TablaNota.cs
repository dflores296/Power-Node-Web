namespace PowerNode.DesignSuite.Domain.Norma;

public class TablaNota
{
    public int Id { get; set; }
    public string TablaId { get; set; } = string.Empty;
    public Tabla Tabla { get; set; } = null!;

    public string Texto { get; set; } = string.Empty;
    public int Secuencia { get; set; }
}
