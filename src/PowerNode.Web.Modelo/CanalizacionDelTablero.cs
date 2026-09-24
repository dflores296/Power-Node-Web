using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>Una canalización: un tubo, un ducto o un canal, con los circuitos que van por ella</b> —
/// decisión canalizaciones-y-agrupamiento (2026-09-24). Se captura qué circuitos van juntos; el
/// programa cuenta sus portadores, decide si aplica el ajuste por agrupamiento según el tipo y
/// calcula su tamaño.
///
/// <para>
/// Un circuito que no se asigna a ninguna va en su <b>canalización propia</b>, con la configuración
/// de <see cref="DatosDelTablero.CanalizacionPorOmision"/>. Si un circuito pasa por varias, se le
/// asigna la del tramo más desfavorable: manda la ampacidad menor — 310-15(a)(2).
/// </para>
/// </summary>
public sealed class CanalizacionDelTablero(string id)
{
    /// <summary>«T1», «T2»… en las compartidas; «Por omisión», «Alimentador» o el número del circuito en las demás.</summary>
    public string Id { get; } = id;

    public TipoCanalizacion Tipo { get; set; } = TipoCanalizacion.TuboConduit;

    /// <summary>El bloque de la Tabla 4. Solo cuenta en tubo y niple.</summary>
    public TipoTuboConduit Tubo { get; set; } = TipoTuboConduit.PvcCedula40;

    /// <summary>Canalización metálica de aluminio en vez de acero: cambia la columna de reactancia de la Tabla 9.</summary>
    public bool MetalAluminio { get; set; }

    /// <summary>Ductos y canales auxiliares: medidas interiores, para el 20 %.</summary>
    public decimal? AnchoMm { get; set; }
    public decimal? AltoMm { get; set; }

    /// <summary>Superficiales: área interior y número de conductores que marca el fabricante (386-22, 388-22).</summary>
    public decimal? AreaInteriorMm2 { get; set; }
    public int? MaxConductoresFabricante { get; set; }

    /// <summary>Circuitos de 1 polo en barras distintas con un solo neutro — circuito multiconductor, 210-4.</summary>
    public bool NeutroCompartido { get; set; }

    /// <summary>Una sola tierra para todos los circuitos, con la protección mayor — 250-122(c).</summary>
    public bool TierraComun { get; set; }

    /// <summary>Tierra desnuda: su área sale de la Tabla 8 (Nota 8 del Capítulo 10).</summary>
    public bool TierraDesnuda { get; set; }

    /// <summary>Tubo en azotea expuesto al sol: distancia del techo a la base del tubo, en mm. Null = no — 310-15(b)(3)(c).</summary>
    public decimal? AlturaSobreTechoMm { get; set; }

    /// <summary>Designación métrica que fija el diseñador; el programa verifica que alcance. Null = la elige el programa.</summary>
    public int? TamanoFijado { get; set; }

    /// <summary>
    /// Solo el alimentador: los conductores en paralelo van <b>todos</b> en esta canalización, en
    /// vez de un juego por tubo (310-10(h)(3)). Cuenta cada conductor — 310-15(b)(3)(a).
    /// </summary>
    public bool JuegosEnUnTubo { get; set; }

    // ---- Resultados: los llena CuadroDeCarga ------------------------------------------------

    /// <summary>Los circuitos que van por aquí, en orden de espacio.</summary>
    public IReadOnlyList<CircuitoDelCuadro> Circuitos { get; internal set; } = [];

    public ConteoDePortadores? Conteo { get; internal set; }
    public AjusteDeAgrupamiento? Ajuste { get; internal set; }

    /// <summary>°C que suma la Tabla 310-15(b)(3)(c); 0 si no está en azotea.</summary>
    public decimal SumadorAzoteaC { get; internal set; }

    public ResultadoOcupacion? Ocupacion { get; internal set; }

    /// <summary>Cuántas canalizaciones iguales hacen falta: una por juego de conductores en paralelo — 310-10(h)(3).</summary>
    public int CanalizacionesIguales { get; internal set; } = 1;

    public List<string> Avisos { get; } = [];

    /// <summary>El factor que resultó: 1.00 si no aplica.</summary>
    public decimal? FactorAgrupamiento { get; internal set; }

    /// <summary>«PVC 40», «Ducto metálico».</summary>
    public string Rotulo => Tipo switch
    {
        TipoCanalizacion.TuboConduit => Tubo.Corto(),
        TipoCanalizacion.Niple => $"Niple {Tubo.Corto()}",
        _ => Tipo.Nombre(),
    };

    /// <summary>«27 (1)», «× 2», o «—» si todavía no hay tamaño.</summary>
    public string TamanoRotulo
    {
        get
        {
            var tamano = Ocupacion?.Tamano?.Rotulo
                ?? (Ocupacion?.AreaMinimaMm2 is { } min ? $"≥ {min:N0} mm²" : null)
                ?? (Tipo.EsDuctoOCanal() && AnchoMm is { } a && AltoMm is { } h ? $"{a:N0} × {h:N0} mm" : null)
                ?? "—";
            return CanalizacionesIguales > 1 ? $"{CanalizacionesIguales} × {tamano}" : tamano;
        }
    }

    /// <summary>La columna de la Tabla 9 con la que se lee la reactancia.</summary>
    public MaterialCanalizacion MaterialParaTabla9 => TiposDeCanalizacion.MaterialParaTabla9(Tipo, Tubo, MetalAluminio);

    /// <summary>Copia la configuración (no los resultados) de otra: así nace la canalización propia de un circuito.</summary>
    internal void CopiarConfiguracionDe(CanalizacionDelTablero otra)
    {
        Tipo = otra.Tipo;
        Tubo = otra.Tubo;
        MetalAluminio = otra.MetalAluminio;
        AnchoMm = otra.AnchoMm;
        AltoMm = otra.AltoMm;
        AreaInteriorMm2 = otra.AreaInteriorMm2;
        MaxConductoresFabricante = otra.MaxConductoresFabricante;
        TierraDesnuda = otra.TierraDesnuda;
        AlturaSobreTechoMm = otra.AlturaSobreTechoMm;
    }

    internal void Limpiar()
    {
        Circuitos = [];
        Conteo = null;
        Ajuste = null;
        SumadorAzoteaC = 0m;
        Ocupacion = null;
        CanalizacionesIguales = 1;
        FactorAgrupamiento = null;
        Avisos.Clear();
    }
}
