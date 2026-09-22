namespace PowerNode.DesignSuite.Domain.Norma;

/// <summary>
/// Una arista del grafo de referencias cruzadas de la norma (data/grafo.json): de qué
/// sección/artículo/tabla a cuál otra. "To" no es FK estricta: puede apuntar a
/// "tabla:110-28", "cap:10", etc., no solo a otra Seccion.
/// </summary>
public class ReferenciaCruzada
{
    public int Id { get; set; }
    public string Origen { get; set; } = string.Empty;
    public string Destino { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}
