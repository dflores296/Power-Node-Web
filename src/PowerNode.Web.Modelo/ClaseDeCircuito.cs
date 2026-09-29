namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>La clase de un circuito</b>, como la define el Art. 100 — I-123, decisión
/// <c>cargas-y-clases-de-circuito.md</c>. No se captura: sale de sus cargas
/// (<see cref="CircuitoDelCuadro.ClaseDelCircuito"/>). Cambia las reglas del circuito —el valor de los
/// contactos, las cargas permisibles, el tamaño de la protección—, no el factor de demanda.
/// </summary>
public enum ClaseDeCircuito
{
    /// <summary>«Circuito derivado individual»: alimenta a un solo equipo de utilización.</summary>
    Individual,

    /// <summary>«Circuito derivado de uso general»: dos o más salidas para alumbrado y aparatos.</summary>
    UsoGeneral,

    /// <summary>«Circuito derivado para aparatos»: salidas para aparatos, sin alumbrado conectado permanentemente.</summary>
    ParaAparatos,

    /// <summary>«Alimentador»: llega hasta la protección de los derivados de otro tablero (Art. 215).</summary>
    Alimentador,
}

public static class ClasesDeCircuito
{
    /// <summary>El nombre corto, basado en la NOM (David, 2026-09-29).</summary>
    public static string Nombre(this ClaseDeCircuito c) => c switch
    {
        ClaseDeCircuito.Individual => "Individual",
        ClaseDeCircuito.UsoGeneral => "Uso general",
        ClaseDeCircuito.ParaAparatos => "Para aparatos",
        _ => "Alimentador",
    };

    /// <summary>El de la NOM, para la memoria.</summary>
    public static string NombreNom(this ClaseDeCircuito c) => c switch
    {
        ClaseDeCircuito.Individual => "Circuito derivado individual",
        ClaseDeCircuito.UsoGeneral => "Circuito derivado de uso general",
        ClaseDeCircuito.ParaAparatos => "Circuito derivado para aparatos",
        _ => "Alimentador",
    };
}

/// <summary>
/// La parte de la carga de un circuito que es de un tipo — I-123: con ella el alimentador aplica el
/// factor de demanda de cada tipo, no el del circuito. En VA.
/// </summary>
public sealed record PorcionDeCarga(CategoriaDeCarga Tipo, decimal ContinuaVA, decimal NoContinuaVA, decimal MotorVA)
{
    public decimal TotalVA => ContinuaVA + NoContinuaVA + MotorVA;
}
