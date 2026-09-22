namespace PowerNode.DesignSuite.Domain.Norma;

public enum TipoAnotacion
{
    Nota,
    Excepcion
}

/// <summary>Nota o excepción colgada de una sección (el corpus las trae ya separadas por tipo).</summary>
public class SeccionAnotacion
{
    public int Id { get; set; }
    public string SeccionId { get; set; } = string.Empty;
    public Seccion Seccion { get; set; } = null!;

    public TipoAnotacion Tipo { get; set; }
    public string? Etiqueta { get; set; }
    public string Texto { get; set; } = string.Empty;
    public int Secuencia { get; set; }
}
