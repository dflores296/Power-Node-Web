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
    // Formato 6 — M-14: el área servida y el renglón de la Tabla 220-12.
    public decimal? AreaServidaM2 { get; set; }
    public string? UsoTabla220_12 { get; set; }
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
    /// <summary>Hasta el formato 10: seco, o «húmedo o mojado». Solo se lee — ver <see cref="Lugar"/>.</summary>
    public bool? LugarSeco { get; set; }
    // Formato 11 — auditoría del 2026-09-29, P1-2: seco, húmedo o mojado.
    public LugarDeInstalacion? Lugar { get; set; }
    public bool? TerminalesMarcadas75C { get; set; }
    public decimal? TemperaturaAmbienteC { get; set; }
    public bool? CargaNoLineal { get; set; }
    public bool? NeutroReducido220_61 { get; set; }
    /// <summary>Null = automático — I-162.</summary>
    public int? ConductoresPorFaseAlimentador { get; set; }
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
    // Formato 11 — P1-1: el motor no arranca con la Tabla 430-52 (430-52(c)(1) Excepción 2).
    public bool? NoArrancaConLaTabla { get; set; }
    public PlacaDeAireAcondicionado? PlacaAire { get; set; }
    public decimal? AmpacidadMinima { get; set; }
    public decimal? ProteccionMaxima { get; set; }
    // Formato 4 — I-119: el motor con variador.
    public decimal? CorrienteEntradaVariador { get; set; }
    public decimal? ProteccionMaximaVariador { get; set; }
    // Formato 10: el nombre del equipo del renglón, aparte del del espacio (David, 2026-09-30).
    public string? DescripcionDelEquipo { get; set; }
    // Formato 4 — I-120, I-121: el servicio de un motor y el par no simultáneo.
    public PowerNode.DesignSuite.Calculo.TablasNom.ServicioDeMotor? Servicio { get; set; }
    public PowerNode.DesignSuite.Calculo.TablasNom.EspecificacionDeTiempo? EspecificacionServicio { get; set; }
    public decimal? CorrientePlacaServicio { get; set; }
    public int? NoSimultaneoCon { get; set; }
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
        NoArrancaConLaTabla = c.NoArrancaConLaTabla ? true : null,
        PlacaAire = c.PlacaAire == PlacaDeAireAcondicionado.AmpacidadYProteccion ? null : c.PlacaAire,
        AmpacidadMinima = c.AmpacidadMinimaA == 0m ? null : c.AmpacidadMinimaA,
        ProteccionMaxima = c.ProteccionMaximaA == 0m ? null : c.ProteccionMaximaA,
        CorrienteEntradaVariador = c.CorrienteEntradaVariadorA == 0m ? null : c.CorrienteEntradaVariadorA,
        ProteccionMaximaVariador = c.ProteccionMaximaVariadorA == 0m ? null : c.ProteccionMaximaVariadorA,
        DescripcionDelEquipo = string.IsNullOrWhiteSpace(c.DescripcionDelEquipo) ? null : c.DescripcionDelEquipo,
        Servicio = c.Servicio,
        EspecificacionServicio = c.EspecificacionServicio == PowerNode.DesignSuite.Calculo.TablasNom.EspecificacionDeTiempo.Continuo ? null : c.EspecificacionServicio,
        CorrientePlacaServicio = c.CorrientePlacaServicioA == 0m ? null : c.CorrientePlacaServicioA,
        NoSimultaneoCon = c.NoSimultaneoCon,
        Continua = c.Continua,
        NoContinua = c.NoContinua,
        FactorPotencia = c.FactorPotencia,
        LongitudM = c.LongitudM,
        Polos = c.Polos,
        ConNeutro = c.ConNeutro,
        Canalizacion = c.Canalizacion,
        Aparatos = c.Cargas.Count == 0 ? null : c.Cargas.Select(AparatoJson.De).ToList(),
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
        c.NoArrancaConLaTabla = NoArrancaConLaTabla ?? c.NoArrancaConLaTabla;
        c.PlacaAire = PlacaAire ?? c.PlacaAire;
        c.AmpacidadMinimaA = AmpacidadMinima is >= 0m ? AmpacidadMinima.Value : c.AmpacidadMinimaA;
        c.ProteccionMaximaA = ProteccionMaxima is >= 0m ? ProteccionMaxima.Value : c.ProteccionMaximaA;
        c.CorrienteEntradaVariadorA = CorrienteEntradaVariador is >= 0m ? CorrienteEntradaVariador.Value : c.CorrienteEntradaVariadorA;
        c.ProteccionMaximaVariadorA = ProteccionMaximaVariador is >= 0m ? ProteccionMaximaVariador.Value : c.ProteccionMaximaVariadorA;
        c.DescripcionDelEquipo = DescripcionDelEquipo ?? c.DescripcionDelEquipo;
        c.Servicio = Servicio;
        c.EspecificacionServicio = EspecificacionServicio ?? c.EspecificacionServicio;
        c.CorrientePlacaServicioA = CorrientePlacaServicio is >= 0m ? CorrientePlacaServicio.Value : c.CorrientePlacaServicioA;
        c.NoSimultaneoCon = NoSimultaneoCon is >= 1 ? NoSimultaneoCon : null;
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
        c.Cargas.Clear();
        foreach (var (a, n) in (Aparatos ?? []).Select((a, i) => (a, i + 1)))
            c.Cargas.Add(a.Crear($"El aparato {n} del circuito {Espacio}", avisos));
        // «Varios» se retiró (I-123): el grupo sale de lo que lleva el circuito. Sus motores y motocompresores
        // siguen haciéndolo grupo; la unidad regresa a la de un equipo.
        if (c.CapturaMotor == CapturaDeMotor.Grupo)
            c.CapturaMotor = CapturaDeMotor.Hp;
        if (c.PlacaAire == PlacaDeAireAcondicionado.Grupo)
            c.PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;

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

/// <summary>Un aparato del desglose — <see cref="CargaDelCircuito"/>.</summary>
public sealed class AparatoJson
{
    public string? Descripcion { get; set; }
    public int? Cantidad { get; set; }
    public UnidadConsumo? Unidad { get; set; }
    public decimal? CargaUnitaria { get; set; }
    public bool? Continua { get; set; }
    public decimal? FactorPotencia { get; set; }
    // Formato 3 — I-115: un motor o un motocompresor de un grupo. Solo se escriben si no son los de una carga.
    public ClaseDeAparato? Clase { get; set; }
    public CapturaDeMotor? CapturaMotor { get; set; }
    public decimal? Hp { get; set; }
    public decimal? CorrientePlaca { get; set; }
    public decimal? CorrienteSeleccion { get; set; }
    // Formato 5 — I-123: el subtipo de la carga, y con él su tipo. Sin él, la carga toma el del circuito.
    public SubtipoDeCarga? Subtipo { get; set; }
    // Formato 8: la no continua de un tablero alimentado; su continua es la carga (David, 2026-09-30).
    public decimal? NoContinua { get; set; }
    // Formato 9: un variador en el desplegable — su protección máxima (110-3(b)).
    public decimal? ProteccionMaxima { get; set; }

    public static AparatoJson De(CargaDelCircuito a) => new()
    {
        Descripcion = a.Descripcion,
        Cantidad = a.Cantidad,
        Unidad = a.Unidad,
        CargaUnitaria = a.CargaUnitaria,
        Continua = a.Continua,
        FactorPotencia = a.FactorPotencia,
        Clase = a.Clase == ClaseDeAparato.Carga ? null : a.Clase,
        CapturaMotor = a.CapturaMotor == CapturaDeMotor.Hp ? null : a.CapturaMotor,
        Hp = a.Hp,
        CorrientePlaca = a.CorrientePlacaA == 0m ? null : a.CorrientePlacaA,
        CorrienteSeleccion = a.CorrienteSeleccionA,
        Subtipo = a.Subtipo,
        NoContinua = a.EsTablero ? a.NoContinua : null,
        ProteccionMaxima = a.Clase == ClaseDeAparato.Variador && a.ProteccionMaximaA > 0m ? a.ProteccionMaximaA : null,
    };

    internal CargaDelCircuito Crear(string quien, List<string> avisos)
    {
        var a = new CargaDelCircuito();
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
        a.Clase = Clase ?? a.Clase;
        // Un aparato es HP o amperes; «Varios» es solo del circuito.
        a.CapturaMotor = CapturaMotor is CapturaDeMotor.Amperes ? CapturaDeMotor.Amperes : CapturaDeMotor.Hp;
        a.Hp = Hp is > 0m ? Hp : null;
        a.CorrientePlacaA = CircuitoJson.NoNegativo(CorrientePlaca, a.CorrientePlacaA, $"{quien}, en la corriente,", avisos);
        a.CorrienteSeleccionA = CorrienteSeleccion is > 0m ? CorrienteSeleccion : null;
        // El subtipo fija la clase; uno que solo va en el renglón (A/A con MCA) no se lee.
        if (Subtipo is { } s && Enum.IsDefined(s) && !s.SoloEnElRenglon())
            a.Subtipo = s;
        if (a.EsTablero)
            a.NoContinua = CircuitoJson.NoNegativo(NoContinua, a.NoContinua, $"{quien}, en la no continua,", avisos);
        if (a.Clase == ClaseDeAparato.Variador)
            a.ProteccionMaximaA = CircuitoJson.NoNegativo(ProteccionMaxima, a.ProteccionMaximaA, $"{quien}, en la protección máxima,", avisos);
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
