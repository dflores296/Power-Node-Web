using System.Text.Json;
using System.Text.Json.Serialization;
using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo.Archivo;

// LA FORMA DEL ARCHIVO (formato 1). Todo es opcional al leer: lo que falta se queda como en un
// tablero nuevo (ArchivoDelCuadro). Los tipos van por su nombre ("Alumbrado", "VoltAmperes"), no por
// número: el archivo se lee a simple vista y reordenar un enum no lo rompe. RENOMBRAR un valor sí lo
// rompe: eso pide subir ArchivoDelCuadro.Version y leer el nombre viejo.

/// <summary>El archivo completo.</summary>
public sealed class ArchivoJson
{
    public string? Formato { get; set; }
    public int? Version { get; set; }
    /// <summary>Cuándo se guardó, con su zona horaria. Solo informativo.</summary>
    public string? Guardado { get; set; }
    public DatosJson? Datos { get; set; }
    public List<CircuitoJson>? Circuitos { get; set; }
    public List<CanalizacionJson>? Canalizaciones { get; set; }
    public CanalizacionJson? CanalizacionAlimentador { get; set; }
}

/// <summary>La cabecera del cuadro — <see cref="DatosDelTablero"/>.</summary>
public sealed class DatosJson
{
    public string? Tablero { get; set; }
    public string? Clave { get; set; }
    public string? Ubicacion { get; set; }
    public string? Proyecto { get; set; }
    public string? Cliente { get; set; }
    public string? Diseno { get; set; }
    public string? Reviso { get; set; }
    public string? Aprobo { get; set; }
    public string? Fecha { get; set; }
    public string? Revision { get; set; }

    public string? Montaje { get; set; }
    public MaterialConductor? MaterialBarras { get; set; }
    public string? GabineteNema { get; set; }
    public int? NumeroEspacios { get; set; }
    public TipoAcometidaTablero? TipoAcometida { get; set; }
    public decimal? CapacidadBarraA { get; set; }
    public bool? EsEquipoDeAcometida { get; set; }
    public TipoDeInmueble? Inmueble { get; set; }
    public SerieDeInterruptores? SerieInterruptores { get; set; }

    public decimal? TensionFaseFaseV { get; set; }
    public int? Fases { get; set; }
    public int? Hilos { get; set; }
    public int? FrecuenciaHz { get; set; }

    public Dictionary<CategoriaDeCarga, decimal>? FactoresDeDemanda { get; set; }
    public Dictionary<CategoriaDeCarga, List<JustificacionFactorDemanda>>? Justificaciones { get; set; }
    public Dictionary<CategoriaDeCarga, string>? JustificacionOtra { get; set; }

    public MaterialConductor? MaterialConductor { get; set; }
    public string? TipoAislamiento { get; set; }
    public bool? LugarSeco { get; set; }
    public bool? TerminalesMarcadas75C { get; set; }
    public decimal? TemperaturaAmbienteC { get; set; }
    public bool? CargaNoLineal { get; set; }
    public Dictionary<string, decimal>? DiametrosFabricante { get; set; }
    public TipoTuboConduit? TuboAlNacer { get; set; }

    public decimal? CaidaMaxDerivadoPct { get; set; }
    public decimal? CaidaMaxAlimentadorPct { get; set; }
    public decimal? LongitudAlimentadorM { get; set; }
    public bool? ConjuntoAprobado100Pct { get; set; }
}

/// <summary>Un renglón del cuadro — <see cref="CircuitoDelCuadro"/>.</summary>
public sealed class CircuitoJson
{
    public int? Espacio { get; set; }
    public string? Descripcion { get; set; }
    public CategoriaDeCarga? Categoria { get; set; }
    public UsoDeContactos? Uso { get; set; }
    public UnidadConsumo? Unidad { get; set; }
    public decimal? Continua { get; set; }
    public decimal? NoContinua { get; set; }
    public decimal? FactorPotencia { get; set; }
    public decimal? LongitudM { get; set; }
    public int? Polos { get; set; }
    public bool? ConNeutro { get; set; }
    public string? Canalizacion { get; set; }
    public List<AparatoJson>? Aparatos { get; set; }

    public static CircuitoJson De(CircuitoDelCuadro c) => new()
    {
        Espacio = c.Espacio,
        Descripcion = c.Descripcion,
        Categoria = c.Categoria,
        Uso = c.Uso,
        Unidad = c.Unidad,
        Continua = c.Continua,
        NoContinua = c.NoContinua,
        FactorPotencia = c.FactorPotencia,
        LongitudM = c.LongitudM,
        Polos = c.Polos,
        ConNeutro = c.ConNeutro,
        Canalizacion = c.Canalizacion,
        Aparatos = c.Aparatos.Count == 0 ? null : c.Aparatos.Select(AparatoJson.De).ToList(),
    };

    /// <summary>Igual a un espacio recién hecho: no se guarda.</summary>
    [JsonIgnore]
    public bool EsVacio
    {
        get
        {
            var vacio = De(new CircuitoDelCuadro(Espacio ?? 0));
            return JsonSerializer.Serialize(this, ContextoDelArchivo.Legible.CircuitoJson)
                == JsonSerializer.Serialize(vacio, ContextoDelArchivo.Legible.CircuitoJson);
        }
    }

    internal void Aplicar(CircuitoDelCuadro c, DatosDelTablero datos)
    {
        c.Descripcion = Descripcion ?? c.Descripcion;
        c.Categoria = Categoria ?? c.Categoria;
        c.Uso = Uso ?? c.Uso;
        c.Unidad = Unidad ?? c.Unidad;
        c.Continua = Continua ?? c.Continua;
        c.NoContinua = NoContinua ?? c.NoContinua;
        c.FactorPotencia = FactorPotencia ?? c.FactorPotencia;
        c.LongitudM = LongitudM ?? c.LongitudM;
        // Los polos se ponen directo: el recálculo resuelve qué renglones se come cada uno, y gana
        // el que empieza antes, igual que al capturarlo.
        if (Polos is { } polos && polos >= 1 && polos <= datos.MaximoPolos)
            c.Polos = polos;
        c.ConNeutro = ConNeutro ?? c.ConNeutro;
        c.Canalizacion = Canalizacion ?? c.Canalizacion;
        c.Aparatos.Clear();
        foreach (var a in Aparatos ?? [])
            c.Aparatos.Add(a.Crear());
    }
}

/// <summary>Un aparato del desglose — <see cref="AparatoDelCircuito"/>.</summary>
public sealed class AparatoJson
{
    public string? Descripcion { get; set; }
    public int? Cantidad { get; set; }
    public UnidadConsumo? Unidad { get; set; }
    public decimal? CargaUnitaria { get; set; }
    public bool? Continua { get; set; }
    public decimal? FactorPotencia { get; set; }

    public static AparatoJson De(AparatoDelCircuito a) => new()
    {
        Descripcion = a.Descripcion,
        Cantidad = a.Cantidad,
        Unidad = a.Unidad,
        CargaUnitaria = a.CargaUnitaria,
        Continua = a.Continua,
        FactorPotencia = a.FactorPotencia,
    };

    internal AparatoDelCircuito Crear()
    {
        var a = new AparatoDelCircuito();
        a.Descripcion = Descripcion ?? a.Descripcion;
        a.Cantidad = Cantidad is >= 1 ? Cantidad.Value : a.Cantidad;
        a.Unidad = Unidad ?? a.Unidad;
        a.CargaUnitaria = CargaUnitaria ?? a.CargaUnitaria;
        a.Continua = Continua ?? a.Continua;
        a.FactorPotencia = FactorPotencia ?? a.FactorPotencia;
        return a;
    }
}

/// <summary>Una canalización — <see cref="CanalizacionDelTablero"/>, sin sus resultados.</summary>
public sealed class CanalizacionJson
{
    public string? Id { get; set; }
    public bool? Automatica { get; set; }
    public string? Nombre { get; set; }
    public TipoCanalizacion? Tipo { get; set; }
    public TipoTuboConduit? Tubo { get; set; }
    public bool? MetalAluminio { get; set; }
    public decimal? AnchoMm { get; set; }
    public decimal? AltoMm { get; set; }
    public decimal? AreaInteriorMm2 { get; set; }
    public int? MaxConductoresFabricante { get; set; }
    public bool? NeutroCompartido { get; set; }
    public bool? TierraComun { get; set; }
    public bool? TierraDesnuda { get; set; }
    public decimal? AlturaSobreTechoMm { get; set; }
    public int? TamanoFijado { get; set; }
    public bool? JuegosEnUnTubo { get; set; }

    public static CanalizacionJson De(CanalizacionDelTablero t) => new()
    {
        Id = t.Id,
        Automatica = t.Automatica,
        Nombre = t.TieneNombre ? t.Nombre : null,
        Tipo = t.Tipo,
        Tubo = t.Tubo,
        MetalAluminio = t.MetalAluminio,
        AnchoMm = t.AnchoMm,
        AltoMm = t.AltoMm,
        AreaInteriorMm2 = t.AreaInteriorMm2,
        MaxConductoresFabricante = t.MaxConductoresFabricante,
        NeutroCompartido = t.NeutroCompartido,
        TierraComun = t.TierraComun,
        TierraDesnuda = t.TierraDesnuda,
        AlturaSobreTechoMm = t.AlturaSobreTechoMm,
        TamanoFijado = t.TamanoFijado,
        JuegosEnUnTubo = t.JuegosEnUnTubo,
    };

    internal void Aplicar(CanalizacionDelTablero t)
    {
        if (Nombre is not null)
            t.Nombre = Nombre;
        t.Tipo = Tipo ?? t.Tipo;
        t.Tubo = Tubo ?? t.Tubo;
        t.MetalAluminio = MetalAluminio ?? t.MetalAluminio;
        t.AnchoMm = AnchoMm;
        t.AltoMm = AltoMm;
        t.AreaInteriorMm2 = AreaInteriorMm2;
        t.MaxConductoresFabricante = MaxConductoresFabricante;
        t.NeutroCompartido = NeutroCompartido ?? t.NeutroCompartido;
        t.TierraComun = TierraComun ?? t.TierraComun;
        t.TierraDesnuda = TierraDesnuda ?? t.TierraDesnuda;
        t.AlturaSobreTechoMm = AlturaSobreTechoMm;
        t.TamanoFijado = TamanoFijado;
        t.JuegosEnUnTubo = JuegosEnUnTubo ?? t.JuegosEnUnTubo;
    }
}

/// <summary>
/// El lector y el escritor, generados al compilar: sin reflexión, así que el recorte de la
/// publicación (WebAssembly) no le quita nada. Se usa por <see cref="Legible"/>.
/// </summary>
[JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true,
    ReadCommentHandling = JsonCommentHandling.Skip,
    AllowTrailingCommas = true)]
[JsonSerializable(typeof(ArchivoJson))]
internal sealed partial class ContextoDelArchivo : JsonSerializerContext
{
    /// <summary>
    /// Con los acentos y los signos tal cual: por omisión el escritor los escapa («Baño» salía
    /// «Ba\u00F1o», y el «+» de la zona horaria, «\u002B»), y el archivo ya no se leía a simple vista.
    /// Se puede: el archivo no se incrusta en una página. Perezoso: armado en el inicializador
    /// estático, corría antes que las opciones que genera el compilador y tronaba.
    /// </summary>
    public static ContextoDelArchivo Legible => legible ??= new(new JsonSerializerOptions(Default.Options)
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    });

    private static ContextoDelArchivo? legible;
}
