namespace PowerNode.DesignSuite.Domain.Norma;

/// <summary>Definición de término embebida dentro de una sección puntual (no el glosario general del Artículo 100).</summary>
public class SeccionDefinicion
{
    public int Id { get; set; }
    public string SeccionId { get; set; } = string.Empty;
    public Seccion Seccion { get; set; } = null!;

    public string Termino { get; set; } = string.Empty;
    public string Texto { get; set; } = string.Empty;
}
