using System.Text.Json;
using System.Text.Json.Serialization;
using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo.Archivo;

// LA FORMA DEL ARCHIVO (formato 2). Todo es opcional al leer: lo que falta se queda como en un
// tablero nuevo (ArchivoDelCuadro). Los tipos van por su nombre ("Alumbrado", "VoltAmperes"), no por
// número: el archivo se lee a simple vista y reordenar un enum no lo rompe. RENOMBRAR un valor sí lo
// rompe: eso pide subir ArchivoDelCuadro.Version y leer el nombre viejo. Así pasó con el tipo de
// carga (formato 2, I-74): «MotorOAireAcondicionado» se partió en «Motor» y «AireAcondicionado», y el
// tipo va como texto para poder leer el nombre viejo (CategoriasDeCarga.DelArchivo).

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
    public MontajeDelPrincipal? MontajePrincipal { get; set; }
    public int? EspacioDelPrincipal { get; set; }
    public decimal? CapacidadBarraA { get; set; }
    public bool? EsEquipoDeAcometida { get; set; }
    public TipoDeInmueble? Inmueble { get; set; }
    public SerieDeInterruptores? SerieInterruptores { get; set; }

    public decimal? TensionFaseFaseV { get; set; }
    public int? Fases { get; set; }
    public int? Hilos { get; set; }
    public int? FrecuenciaHz { get; set; }

    // Por el nombre del tipo, como texto: ver CircuitoJson.Categoria.
    public Dictionary<string, decimal>? FactoresDeDemanda { get; set; }
    public Dictionary<string, List<JustificacionFactorDemanda>>? Justificaciones { get; set; }
    public Dictionary<string, string>? JustificacionOtra { get; set; }

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
    /// <summary>
    /// El tipo, por su nombre y como texto: así se lee también el de un archivo de formato 1,
    /// «MotorOAireAcondicionado», que ya no existe — <see cref="CategoriasDeCarga.DelArchivo"/>.
    /// </summary>
    public string? Categoria { get; set; }
    public UsoDeContactos? Uso { get; set; }
    public UnidadConsumo? Unidad { get; set; }
    /// <summary>Los HP de un motor — I-15. En formato 1, sin él un Motor / A/C era carga de placa.</summary>
    public decimal? Hp { get; set; }
    // Motor y A/C y refrigeración — I-74. Solo se escriben si no son los de un renglón nuevo.
    public CapturaDeMotor? CapturaMotor { get; set; }
    public decimal? CorrientePlaca { get; set; }
    public decimal? CorrienteSeleccion { get; set; }
    public bool? ArranqueAl225 { get; set; }
    public PlacaDeAireAcondicionado? PlacaAire { get; set; }
    public decimal? AmpacidadMinima { get; set; }
    public decimal? ProteccionMaxima { get; set; }
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
        Categoria = c.TipoElegido ? c.Categoria.AlArchivo() : null, // sin elegir, sin tipo (I-111)
        Uso = c.Uso,
        Unidad = c.Unidad,
        Hp = c.Hp,
        CapturaMotor = c.CapturaMotor == CapturaDeMotor.Hp ? null : c.CapturaMotor,
        CorrientePlaca = c.CorrientePlacaA == 0m ? null : c.CorrientePlacaA,
        CorrienteSeleccion = c.CorrienteSeleccionA,
        ArranqueAl225 = c.ArranqueAl225 ? true : null,
        PlacaAire = c.PlacaAire == PlacaDeAireAcondicionado.AmpacidadYProteccion ? null : c.PlacaAire,
        AmpacidadMinima = c.AmpacidadMinimaA == 0m ? null : c.AmpacidadMinimaA,
        ProteccionMaxima = c.ProteccionMaximaA == 0m ? null : c.ProteccionMaximaA,
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

    internal void Aplicar(CircuitoDelCuadro c, DatosDelTablero datos, List<string> avisos)
    {
        c.Descripcion = Descripcion ?? c.Descripcion;
        var categoria = CategoriasDeCarga.DelArchivo(Categoria, out var eraMotorOAire);
        if (Categoria is not null && categoria is null && !eraMotorOAire)
            avisos.Add($"El circuito {Espacio} trae un tipo de carga que no se conoce («{Categoria}»); se abrió sin tipo.");
        if (categoria is { } elegida)
            c.Categoria = elegida;
        c.Uso = Uso ?? c.Uso;
        c.Unidad = Unidad ?? c.Unidad;
        c.Hp = Hp is >= 0m ? Hp : null;
        c.CapturaMotor = CapturaMotor ?? c.CapturaMotor;
        c.CorrientePlacaA = CorrientePlaca is >= 0m ? CorrientePlaca.Value : c.CorrientePlacaA;
        c.CorrienteSeleccionA = CorrienteSeleccion is > 0m ? CorrienteSeleccion : null;
        c.ArranqueAl225 = ArranqueAl225 ?? c.ArranqueAl225;
        c.PlacaAire = PlacaAire ?? c.PlacaAire;
        c.AmpacidadMinimaA = AmpacidadMinima is >= 0m ? AmpacidadMinima.Value : c.AmpacidadMinimaA;
        c.ProteccionMaximaA = ProteccionMaxima is >= 0m ? ProteccionMaxima.Value : c.ProteccionMaximaA;
        c.Continua = NoNegativo(Continua, c.Continua, $"El circuito {Espacio}, en la carga continua,", avisos);
        c.NoContinua = NoNegativo(NoContinua, c.NoContinua, $"El circuito {Espacio}, en la carga no continua,", avisos);
        if (FactorPotencia is { } fp)
        {
            if (CircuitoDelCuadro.FactorPotenciaValido(fp))
                c.FactorPotencia = fp;
            else
                avisos.Add(FueraDeRango($"El circuito {Espacio}", fp, c.FactorPotencia));
        }
        c.LongitudM = NoNegativo(LongitudM, c.LongitudM, $"El circuito {Espacio}, en la longitud,", avisos);
        // Los polos se ponen directo: el recálculo resuelve qué renglones se come cada uno, y gana
        // el que empieza antes, igual que al capturarlo.
        if (Polos is { } polos && polos >= 1 && polos <= datos.MaximoPolos)
            c.Polos = c.PolosElegidos = polos;
        c.ConNeutro = ConNeutro ?? c.ConNeutro;
        c.Canalizacion = Canalizacion ?? c.Canalizacion;
        c.Aparatos.Clear();
        foreach (var (a, n) in (Aparatos ?? []).Select((a, i) => (a, i + 1)))
            c.Aparatos.Add(a.Crear($"El aparato {n} del circuito {Espacio}", avisos));

        if (eraMotorOAire)
            DeMotorOAire(c, datos, avisos);
    }

    /// <summary>
    /// I-84: una carga o una longitud negativa no se abre —en la pantalla ya no entra—; se avisa y
    /// queda la de omisión. Una longitud de −30 m daba una caída de −3.45 %.
    /// </summary>
    internal static decimal NoNegativo(decimal? valor, decimal actual, string donde, List<string> avisos)
    {
        if (valor is not { } v)
            return actual;
        if (v >= 0m)
            return v;
        avisos.Add($"{donde} trae {v:0.##}, que no puede ser negativo; se abrió con {actual:0.##}.");
        return actual;
    }

    /// <summary>I-80: un F.P. fuera de 0.1 a 1 no se abre; se avisa y queda el de omisión.</summary>
    internal static string FueraDeRango(string quien, decimal fp, decimal queda) =>
        $"{quien} trae F.P. {fp:0.##}, fuera de {CircuitoDelCuadro.FactorPotenciaMinimo:0.0} a 1; se abrió con {queda:0.00}.";

    /// <summary>
    /// <b>Un «Motor / A/C» de formato 1</b> — I-74, decisión <c>tipos-de-carga.md</c>. Con HP era un
    /// motor (I-15): pasa a Motor, igual. Sin HP era carga de placa en VA, W o A: pasa a A/C y
    /// refrigeración, con esa carga en amperes como corriente de carga nominal, y un aviso para
    /// revisarlo — pudo ser un motor capturado en amperes.
    /// </summary>
    private void DeMotorOAire(CircuitoDelCuadro c, DatosDelTablero datos, List<string> avisos)
    {
        if (Hp is not null)
        {
            c.Categoria = CategoriaDeCarga.Motor;
            c.CapturaMotor = CapturaDeMotor.Hp;
            return;
        }

        c.Categoria = CategoriaDeCarga.AireAcondicionado;
        c.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
        var polos = Polos is >= 1 and <= 3 ? Polos.Value : 1;
        var va = ConsumoDePlaca.AVoltAmperes(
            c.Continua + c.NoContinua, c.Unidad, datos.TensionFaseNeutroV, datos.TensionFaseFaseV, polos, c.FactorPotencia);
        if (va <= 0m)
            return;

        c.CorrientePlacaA = Math.Round(va / TensionDeCalculo.Divisor(polos, datos.TensionFaseNeutroV, datos.TensionFaseFaseV), 2);
        avisos.Add(
            $"El circuito {Espacio} era «Motor / A/C» en VA, W o A. Ahora Motor y A/C son dos tipos: se abrió como " +
            $"«A/C y refrigeración» con {c.CorrientePlacaA:N2} A de corriente de carga nominal (440-6(a)). Revísalo: si es un " +
            "motor, cámbialo a «Motor»; si la placa trae ampacidad mínima y protección máxima, captúralas en «MCA».");
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

    internal AparatoDelCircuito Crear(string quien, List<string> avisos)
    {
        var a = new AparatoDelCircuito();
        a.Descripcion = Descripcion ?? a.Descripcion;
        a.Cantidad = Cantidad is >= 1 ? Cantidad.Value : a.Cantidad;
        a.Unidad = Unidad ?? a.Unidad;
        a.CargaUnitaria = CircuitoJson.NoNegativo(CargaUnitaria, a.CargaUnitaria, $"{quien}, en la carga,", avisos);
        a.Continua = Continua ?? a.Continua;
        if (FactorPotencia is { } fp)
        {
            if (CircuitoDelCuadro.FactorPotenciaValido(fp))
                a.FactorPotencia = fp;
            else
                avisos.Add(CircuitoJson.FueraDeRango(quien, fp, a.FactorPotencia));
        }
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
