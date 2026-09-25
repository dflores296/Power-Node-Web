namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Dónde va el interruptor principal</b> dentro del gabinete — decisión
/// <c>montaje-del-interruptor-principal.md</c> (David, 2026-09-25). Solo cuenta con interruptor
/// principal; con una sola barra (1F-2H) no hay zócalo y siempre va en un espacio.
/// </summary>
public enum MontajeDelPrincipal
{
    /// <summary>En su zócalo, abajo de las barras: no ocupa espacios numerados. Por omisión.</summary>
    Zocalo,

    /// <summary>En espacios del gabinete, como un derivado: se come tantos espacios como polos.</summary>
    EnEspacios,
}
