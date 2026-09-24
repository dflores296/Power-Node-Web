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
/// Un circuito que no se asigna a ninguna va en su <b>canalización propia</b>, que se guarda con el
/// circuito y se configura en su fila, igual que una compartida (David, 2026-09-24). Si un circuito
/// pasa por varias, se le asigna la del tramo más desfavorable: manda la ampacidad menor —
/// 310-15(a)(2).
/// </para>
///
/// <para>
/// <b>Toda canalización nace como tubo conduit EMT</b> y el ingeniero decide en cada una. Ya no hay
/// una «canalización por omisión» en Condiciones de cálculo (David, 2026-09-24).
/// </para>
/// </summary>
public sealed class CanalizacionDelTablero(string id, bool esPropia = false)
{
    /// <summary>La clave con la que un circuito la referencia: «T1», «T2»… en las compartidas; «Circuito 3» en una propia; «Alimentador».</summary>
    public string Id { get; } = id;

    /// <summary>La canalización propia de un circuito: se guarda con él y no se comparte.</summary>
    public bool EsPropia { get; } = esPropia;

    public bool EsAlimentador => Id == "Alimentador";

    /// <summary>
    /// El nombre que ve el ingeniero, editable: «T1» o «Propia» al nacer; «Tubo pasillo», «Ducto
    /// azotea»… después. La clave sigue siendo <see cref="Id"/>. Borrado, regresa al de nacimiento.
    /// </summary>
    public string Nombre
    {
        get => nombre;
        set => nombre = string.IsNullOrWhiteSpace(value) ? NombreAlNacer : value.Trim();
    }

    private string nombre = esPropia ? "Propia" : id;

    private string NombreAlNacer => EsPropia ? "Propia" : Id;

    /// <summary>Se le cambió el nombre de nacimiento.</summary>
    public bool TieneNombre => Nombre != NombreAlNacer;

    public TipoCanalizacion Tipo { get; set; } = TipoCanalizacion.TuboConduit;

    /// <summary>El bloque de la Tabla 4. Solo cuenta en tubo y niple. EMT al nacer.</summary>
    public TipoTuboConduit Tubo { get; set; } = TipoTuboConduit.Emt;

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

    /// <summary>
    /// El tamaño en mm: la designación métrica de un tubo («21 mm»), las medidas de un ducto o canal
    /// («100 × 100 mm») o, sin ellas, el área interior mínima («mín. 706 mm²»). «2 ×» antes cuando
    /// van canalizaciones iguales, una por juego en paralelo. «—» sin tamaño.
    /// </summary>
    public string TamanoMm
    {
        get
        {
            var tamano = Ocupacion?.Tamano?.Milimetros
                ?? (Tipo.EsDuctoOCanal() && AnchoMm is { } a && AltoMm is { } h ? $"{a:N0} × {h:N0} mm" : null)
                ?? (Ocupacion?.AreaMinimaMm2 is { } min ? $"mín. {min:N0} mm²" : null);
            return tamano is null ? "—" : Veces(tamano);
        }
    }

    /// <summary>El tamaño comercial en pulgadas («¾ in»). Solo en tubo; «—» en lo demás.</summary>
    public string TamanoIn => Ocupacion?.Tamano?.Pulgadas is { } pulgadas ? Veces(pulgadas) : "—";

    /// <summary>«21 mm / ¾ in», o solo los mm cuando no hay pulgadas.</summary>
    public string TamanoRotulo => TamanoIn == "—" ? TamanoMm : $"{TamanoMm} / {Ocupacion!.Tamano!.Pulgadas}";

    private string Veces(string tamano) => CanalizacionesIguales > 1 ? $"{CanalizacionesIguales} × {tamano}" : tamano;

    /// <summary>La columna de la Tabla 9 con la que se lee la reactancia.</summary>
    public MaterialCanalizacion MaterialParaTabla9 => TiposDeCanalizacion.MaterialParaTabla9(Tipo, Tubo, MetalAluminio);

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
