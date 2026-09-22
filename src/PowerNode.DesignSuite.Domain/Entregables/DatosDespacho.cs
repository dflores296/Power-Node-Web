namespace PowerNode.DesignSuite.Domain.Entregables;

/// <summary>
/// El <b>membrete</b>: quién emite la memoria. Va arriba de la portada y —cuando exista— en el
/// encabezado de cada página.
///
/// <para>
/// <b>Vive en el proyecto y no en una configuración global, y es una decisión provisional</b>: lo
/// natural sería que fuera del programa, porque el despacho no cambia entre proyectos. Pero hoy no
/// hay dónde guardar preferencias de instalación —un proyecto es un renglón de la base—, y meterlo
/// en el proyecto tiene una ventaja real mientras tanto: un mismo ingeniero que factura para dos
/// despachos puede emitir con el membrete que le toque a cada obra. Cuando exista el archivo `.nds`
/// y sus preferencias, esto se mueve y se hereda.
/// </para>
/// </summary>
public class DatosDespacho
{
    /// <summary>Razón social o nombre profesional de quien emite.</summary>
    public string? RazonSocial { get; set; }

    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? CorreoElectronico { get; set; }
    public string? SitioWeb { get; set; }

    /// <summary>
    /// El logotipo, <b>guardado como bytes y no como ruta a un archivo</b>. Una ruta se rompe al
    /// cambiar de máquina o al mover la carpeta, y una memoria de cálculo tiene que poder
    /// reproducirse igual años después: si el logo vive fuera, el documento deja de ser el mismo.
    /// PNG o JPG; el exportador lo incrusta en el .docx.
    /// </summary>
    public byte[]? Logo { get; set; }

    /// <summary>Ancho con el que se dibuja el logo, en centímetros. El alto sale de la proporción real de la imagen.</summary>
    public decimal LogoAnchoCm { get; set; } = 4m;
}
