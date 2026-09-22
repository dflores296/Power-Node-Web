namespace PowerNode.DesignSuite.Domain.Entregables;

/// <summary>
/// A quién se entrega la memoria. <b>Es bandera y no una sola opción</b> porque la misma memoria se
/// usa para varios trámites a la vez —el usuario lo confirmó el 2026-08-19: los cuatro—, y cada uno
/// pide datos distintos en la portada.
/// </summary>
[Flags]
public enum DestinoTramite
{
    /// <summary>Sin trámite: entregable del proyecto para el cliente.</summary>
    Ninguno = 0,

    /// <summary>Unidad de verificación de instalaciones eléctricas. Es quien contrasta contra la NOM-001-SEDE.</summary>
    Uvie = 1,

    /// <summary>Contrato o aumento de carga ante la suministradora.</summary>
    Cfe = 2,

    /// <summary>Licencia de construcción o visto bueno municipal, con DRO y corresponsable.</summary>
    Municipio = 4,

    /// <summary>Entrega al cliente, sin trámite de por medio.</summary>
    Cliente = 8,
}

/// <summary>
/// Lo que va en la <b>portada</b> de la memoria de cálculo y no es resultado de ningún cálculo: quién
/// firma, para qué trámite es, y los folios que pide cada dependencia.
///
/// <para>
/// <b>Todo es opcional a propósito.</b> La portada se arma con <b>bloques que aparecen solo si están
/// llenos</b>: un proyecto para el cliente no imprime el renglón del RPU, y uno que no va al
/// municipio no imprime el del DRO. Imprimir un renglón vacío con puntitos es peor que no
/// imprimirlo — parece un documento incompleto en vez de uno que no necesitaba ese dato.
/// </para>
///
/// <para>
/// <b>Los datos eléctricos de la portada NO están aquí</b>, y es la decisión que más cambia el
/// documento: la carga total instalada, la tensión de servicio, las fases y la corriente de la
/// acometida <b>los produce el cálculo</b>. Capturarlos sería invitar a que la portada diga una cosa
/// y el cuerpo de la memoria otra.
/// </para>
/// </summary>
public class DatosPortada
{
    /// <summary>
    /// Encabezado del documento. Se deja configurable porque cada dependencia lo nombra distinto,
    /// pero el valor por omisión es el que se usa en México para baja tensión.
    /// </summary>
    public string Titulo { get; set; } = "MEMORIA DE CÁLCULO DE LA INSTALACIÓN ELÉCTRICA EN BAJA TENSIÓN";

    /// <summary>Para qué trámite(s) se emite. Decide qué bloques de folios se imprimen.</summary>
    public DestinoTramite Destino { get; set; } = DestinoTramite.Ninguno;

    // ------------------------------------------------------------------ responsable técnico
    //
    // Es el bloque que ninguna dependencia perdona: una memoria sin responsable identificable no se
    // recibe. Van separados —nombre, título, cédula, registro— y no como un texto libre, porque el
    // formato de cada trámite los acomoda distinto y concatenarlos obligaría a re-teclearlos.

    /// <summary>Nombre de quien firma la memoria.</summary>
    public string? ResponsableNombre { get; set; }

    /// <summary>Título profesional: "Ing.", "Ing. Electricista", "M.I."…</summary>
    public string? ResponsableTitulo { get; set; }

    /// <summary>Cédula profesional. La pide la UVIE y el municipio.</summary>
    public string? ResponsableCedula { get; set; }

    /// <summary>
    /// Registro con el que firma, cuando el trámite lo exige: perito responsable, unidad de
    /// verificación aprobada, corresponsable en instalaciones. Se guarda tal cual lo emite la
    /// dependencia, sin interpretarlo.
    /// </summary>
    public string? ResponsableRegistro { get; set; }

    // ------------------------------------------------------------------ folios por dependencia

    /// <summary>Número de servicio o RPU del suministro. Solo aplica al trámite ante la suministradora.</summary>
    public string? NumeroServicioRpu { get; set; }

    /// <summary>Número de licencia o de expediente municipal.</summary>
    public string? NumeroLicencia { get; set; }

    /// <summary>Director responsable de obra, cuando el trámite municipal lo pide.</summary>
    public string? DirectorResponsableObra { get; set; }

    /// <summary>Registro del DRO ante el municipio.</summary>
    public string? RegistroDro { get; set; }

    /// <summary>Nombre de la unidad de verificación, cuando ya está designada.</summary>
    public string? UnidadVerificacion { get; set; }

    /// <summary>Número de aprobación de la unidad de verificación ante la Secretaría de Energía.</summary>
    public string? AprobacionUnidadVerificacion { get; set; }

    // ------------------------------------------------------------------ el inmueble

    /// <summary>
    /// Uso o giro de la instalación: "Nave industrial", "Edificio de oficinas", "Casa habitación".
    /// <b>No es decorativo</b>: es lo primero que un verificador usa para saber qué artículos de
    /// instalaciones especiales debería estar viendo.
    /// </summary>
    public string? UsoOGiro { get; set; }

    /// <summary>Superficie construida, en m². La piden los formatos que calculan carga por unidad de superficie.</summary>
    public decimal? SuperficieM2 { get; set; }

    // ------------------------------------------------------------------ control documental

    /// <summary>Revisión del documento: "0", "A", "1"… Vacío = primera emisión.</summary>
    public string? Revision { get; set; }

    /// <summary>Quién revisó, cuando no es la misma persona que elaboró.</summary>
    public string? Reviso { get; set; }

    /// <summary>Quién aprobó.</summary>
    public string? Aprobo { get; set; }

    /// <summary>Notas al pie de la portada: alcance, exclusiones, vigencia del documento.</summary>
    public string? Notas { get; set; }
}
