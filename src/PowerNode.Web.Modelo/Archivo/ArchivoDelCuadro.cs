using System.Text.Json;
using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo.Archivo;

/// <summary>
/// <b>El tablero en un archivo</b> — I-05. La aplicación no tiene servidor ni base de datos
/// (decisión <c>blazor-webassembly-sin-backend</c>): lo capturado se guarda en un archivo del equipo
/// y se abre de vuelta. <b>1 archivo = 1 cuadro de carga</b>, como el Excel
/// (<c>alcance-v1-un-tablero</c>); para varios tableros a la vez, una pestaña del navegador por
/// archivo. Ver <c>docs/decisiones/archivo-del-tablero.md</c>.
///
/// <para>
/// <b>Solo lo capturado, nunca los resultados.</b> Al abrir, el cuadro se recalcula con el motor de
/// la versión que lo abre: si una regla se corrige, el archivo viejo sale con el cálculo corregido,
/// no con los números que tenía al guardarse.
/// </para>
///
/// <para>
/// <b>Lo que falta en el archivo se queda como en un tablero nuevo.</b> Por eso cada campo es
/// opcional al leer: un archivo de una versión anterior, sin los campos que se agreguen después,
/// abre con sus valores por omisión.
/// </para>
/// </summary>
public static class ArchivoDelCuadro
{
    /// <summary>Lo que dice que el archivo es de Power Node.</summary>
    public const string Formato = "power-node/cuadro-de-carga";

    /// <summary>
    /// Sube cuando un archivo nuevo ya no se puede leer igual que uno anterior. 2: seis tipos de carga,
    /// «Motor / A/C» partido en Motor y A/C y refrigeración (I-74). 3: varios motores en un circuito
    /// — el Motor «Varios» y la clase de cada aparato (I-115). 4: el A/C «Varios» y «Hab.», el
    /// motocompresor y el acondicionador de habitación en el desglose (I-116, I-117); la versión 3,
    /// que ya se publicó, no los conoce. 5: el tipo es de cada carga —su subtipo— y el tipo Tablero
    /// (I-123, I-125); la 4 los leería mal. Los anteriores se siguen leyendo: sus cargas sin subtipo toman
    /// el tipo de su circuito, y se calculan igual que antes. 6: el área servida y el renglón de la Tabla
    /// 220-12 (M-14); la 5 los ignoraría y el alimentador saldría sin el mínimo. 7: los subtipos de uso de
    /// vivienda de los contactos (captura-en-el-desplegable.md); la 6 no los conoce. 8: varios tableros en un
    /// alimentador, cada uno con su no continua en su línea; la 7 los leería sin ella. 9: el variador en el
    /// desplegable, con su protección máxima; la 8 lo perdería. 10: el nombre del equipo del renglón,
    /// aparte del del espacio; la 9 lo perdería. 11: el lugar seco, húmedo o mojado (auditoría del
    /// 2026-09-29, P1-2), y el motor que no arranca con la Tabla 430-52 (P1-1); la 10 leería todo como
    /// seco y perdería la Excepción 2.
    /// </summary>
    public const int Version = 11;

    /// <summary>El archivo, listo para escribirse.</summary>
    public static string Guardar(CuadroDeCarga cuadro, DateTimeOffset cuando) =>
        JsonSerializer.Serialize(Armar(cuadro, cuando.ToString("yyyy-MM-ddTHH:mm:sszzz")), ContextoDelArchivo.Legible.ArchivoJson);

    /// <summary>
    /// Lo capturado, sin la fecha de guardado: dos cuadros con la misma huella tienen lo mismo. Sirve
    /// para saber si hay cambios sin guardar.
    /// </summary>
    public static string Huella(CuadroDeCarga cuadro) =>
        JsonSerializer.Serialize(Armar(cuadro, guardado: null), ContextoDelArchivo.Legible.ArchivoJson);

    /// <summary>
    /// Lee un archivo y arma un cuadro nuevo, ya recalculado. <see cref="Apertura.Error"/> si no se
    /// puede leer; <see cref="Apertura.Avisos"/> con lo que se leyó distinto de como venía.
    /// </summary>
    public static Apertura Abrir(string texto, MotorNom motor)
    {
        // LA VERSIÓN, ANTES QUE TODO LO DEMÁS (I-115): un formato más nuevo puede traer valores que
        // esta versión no conoce —un «Grupo» en la captura del motor— y la lectura completa fallaría
        // con «no es de Power Node» en vez de pedir que se recargue la página.
        try
        {
            using var documento = JsonDocument.Parse(texto);
            if (documento.RootElement.ValueKind == JsonValueKind.Object
                && documento.RootElement.TryGetProperty("formato", out var formato) && formato.ValueKind == JsonValueKind.String
                && formato.GetString() == Formato
                && documento.RootElement.TryGetProperty("version", out var v) && v.TryGetInt32(out var leida) && leida > Version)
                return MasNueva(leida);
        }
        catch (JsonException)
        {
            return Apertura.Fallo("El archivo no es de Power Node: no se pudo leer.");
        }

        ArchivoJson? archivo;
        try
        {
            archivo = JsonSerializer.Deserialize(texto, ContextoDelArchivo.Legible.ArchivoJson);
        }
        catch (JsonException)
        {
            return Apertura.Fallo("El archivo no es de Power Node: no se pudo leer.");
        }

        if (archivo?.Formato != Formato)
            return Apertura.Fallo("El archivo no es de Power Node: no es un cuadro de carga guardado desde aquí.");
        if (archivo.Version is not { } version || version < 1)
            return Apertura.Fallo("El archivo no dice de qué versión es.");
        if (version > Version)
            return MasNueva(version);

        var cuadro = new CuadroDeCarga(motor);
        var avisos = new List<string>();
        try
        {
            Aplicar(archivo, cuadro, avisos);
        }
        catch (Exception e) when (e is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            return Apertura.Fallo($"El archivo está dañado: {e.Message}");
        }
        cuadro.Recalcular();
        return new Apertura(cuadro, null, avisos);
    }

    private static Apertura MasNueva(int version) =>
        Apertura.Fallo($"El archivo es de una versión más nueva de Power Node (formato {version}; esta lee hasta el {Version}). Recarga la página con Ctrl+F5 y vuelve a abrirlo.");

    /// <summary>El nombre sugerido del archivo: el del tablero, o su clave, sin caracteres que el sistema no acepte.</summary>
    public static string NombreSugerido(DatosDelTablero datos)
    {
        var nombre = !string.IsNullOrWhiteSpace(datos.Tablero) ? datos.Tablero
            : !string.IsNullOrWhiteSpace(datos.Clave) ? datos.Clave
            : "Tablero";
        var invalidos = new HashSet<char>(Path.GetInvalidFileNameChars()) { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };
        var limpio = new string(nombre.Trim().Select(c => invalidos.Contains(c) || char.IsControl(c) ? '-' : c).ToArray());
        return $"{limpio}.powernode.json";
    }

    // ---- Del cuadro al archivo -------------------------------------------------------------------

    private static ArchivoJson Armar(CuadroDeCarga cuadro, string? guardado)
    {
        var d = cuadro.Datos;
        return new ArchivoJson
        {
            Formato = Formato,
            Version = Version,
            Guardado = guardado,
            Datos = new DatosJson
            {
                Tablero = d.Tablero, Clave = d.Clave, Ubicacion = d.Ubicacion, Proyecto = d.Proyecto, Cliente = d.Cliente,
                Diseno = d.Diseno, Reviso = d.Reviso, Aprobo = d.Aprobo, Fecha = d.Fecha, Revision = d.Revision,
                Montaje = d.Montaje, MaterialBarras = d.MaterialBarras, GabineteNema = d.GabineteNema,
                NumeroEspacios = d.NumeroEspacios, TipoAcometida = d.TipoAcometida,
                MontajePrincipal = d.MontajePrincipal, EspacioDelPrincipal = d.EspacioDelPrincipal, CapacidadBarraA = d.CapacidadBarraA,
                EsEquipoDeAcometida = d.EsEquipoDeAcometida, Inmueble = d.Inmueble, SerieInterruptores = d.SerieInterruptores,
                AreaServidaM2 = d.AreaServidaM2 > 0m ? d.AreaServidaM2 : null, UsoTabla220_12 = d.UsoTabla220_12,
                TensionFaseFaseV = d.TensionFaseFaseV, Fases = d.Fases, Hilos = d.Hilos, FrecuenciaHz = d.FrecuenciaHz,
                FactoresDeDemanda = CategoriasDeCarga.Todas.ToDictionary(c => c.AlArchivo(), d.FactorDeDemanda),
                Justificaciones = d.Justificaciones
                    .Where(j => j.Value.Count > 0)
                    .ToDictionary(j => j.Key.AlArchivo(), j => j.Value.OrderBy(x => x).ToList()),
                JustificacionOtra = d.JustificacionOtra
                    .Where(j => !string.IsNullOrWhiteSpace(j.Value))
                    .ToDictionary(j => j.Key.AlArchivo(), j => j.Value),
                MaterialConductor = d.MaterialConductor, TipoAislamiento = d.TipoAislamiento, Lugar = d.Lugar,
                TerminalesMarcadas75C = d.TerminalesMarcadas75C, TemperaturaAmbienteC = d.TemperaturaAmbienteC,
                CargaNoLineal = d.CargaNoLineal, NeutroReducido220_61 = d.NeutroReducido220_61,
                ConductoresPorFaseAlimentador = d.ConductoresPorFaseAlimentador,
                DiametrosFabricante = d.DiametrosFabricante.Count == 0 ? null : new Dictionary<string, decimal>(d.DiametrosFabricante),
                TuboAlNacer = d.TuboAlNacer,
                CaidaMaxDerivadoPct = d.CaidaMaxDerivadoPct, CaidaMaxAlimentadorPct = d.CaidaMaxAlimentadorPct,
                LongitudAlimentadorM = d.LongitudAlimentadorM, ConjuntoAprobado100Pct = d.ConjuntoAprobado100Pct,
            },
            // Solo los renglones con algo capturado: un espacio vacío abre vacío de todas formas.
            Circuitos = cuadro.Circuitos
                .Where(c => !c.EsContinuacion)
                .Select(CircuitoJson.De)
                .Where(c => !c.EsVacio)
                .ToList(),
            Canalizaciones = d.Canalizaciones.Select(CanalizacionJson.De).ToList(),
            CanalizacionAlimentador = CanalizacionJson.De(d.CanalizacionAlimentador),
        };
    }

    // ---- Del archivo al cuadro -------------------------------------------------------------------

    private static void Aplicar(ArchivoJson archivo, CuadroDeCarga cuadro, List<string> avisos)
    {
        var d = cuadro.Datos;
        if (archivo.Datos is { } a)
        {
            d.Tablero = a.Tablero ?? d.Tablero;
            d.Clave = a.Clave ?? d.Clave;
            d.Ubicacion = a.Ubicacion ?? d.Ubicacion;
            d.Proyecto = a.Proyecto ?? d.Proyecto;
            d.Cliente = a.Cliente ?? d.Cliente;
            d.Diseno = a.Diseno ?? d.Diseno;
            d.Reviso = a.Reviso ?? d.Reviso;
            d.Aprobo = a.Aprobo ?? d.Aprobo;
            d.Fecha = a.Fecha ?? d.Fecha;
            d.Revision = a.Revision ?? d.Revision;
            d.Montaje = a.Montaje ?? d.Montaje;
            d.MaterialBarras = a.MaterialBarras ?? d.MaterialBarras;
            d.GabineteNema = a.GabineteNema ?? d.GabineteNema;
            // I-84: sin dato es válido (null); negativa, no.
            if (a.CapacidadBarraA is < 0m and var barra)
                avisos.Add($"El archivo dice {barra:0.##} A de capacidad de barra, que no puede ser negativa; se abrió sin ese dato.");
            else
                d.CapacidadBarraA = a.CapacidadBarraA;
            d.EsEquipoDeAcometida = a.EsEquipoDeAcometida ?? d.EsEquipoDeAcometida;
            d.Inmueble = a.Inmueble ?? d.Inmueble;
            if (a.AreaServidaM2 is < 0m and var area)
                avisos.Add($"El archivo dice {area:0.##} m² de área servida, que no puede ser negativa; se abrió sin ese dato.");
            else
                d.AreaServidaM2 = a.AreaServidaM2 ?? 0m;
            d.UsoTabla220_12 = string.IsNullOrWhiteSpace(a.UsoTabla220_12) ? null : a.UsoTabla220_12;
            d.SerieInterruptores = a.SerieInterruptores ?? d.SerieInterruptores;

            // En este orden: cambiar las fases reajusta los hilos, la tensión y los espacios; lo del
            // archivo se pone después, encima.
            if (a.Fases is { } fases)
            {
                if (fases is >= 1 and <= 3)
                    d.Fases = fases;
                else
                    avisos.Add($"El archivo dice {fases} fases; se dejó en {d.Fases}.");
            }
            if (a.Hilos is { } hilos)
            {
                if (d.HilosValidos.Contains(hilos))
                    d.Hilos = hilos;
                else
                    avisos.Add($"{d.Fases} fase(s) con {hilos} hilos no es un sistema; se dejó en {d.Hilos} hilos.");
            }
            // I-77: con 0 V el cálculo divide entre cero, también al convertir un archivo de formato 1.
            if (a.TensionFaseFaseV is { } tension)
            {
                if (tension >= DatosDelTablero.TensionMinimaV)
                    d.TensionFaseFaseV = tension;
                else
                    avisos.Add($"El archivo dice {tension:0.##} V; la tensión va de {DatosDelTablero.TensionMinimaV:0} V en adelante. Se abrió con {d.TensionFaseFaseV:0.##} V.");
            }
            if (a.FrecuenciaHz is { } hz)
            {
                if (hz >= DatosDelTablero.FrecuenciaMinimaHz)
                    d.FrecuenciaHz = hz;
                else
                    avisos.Add($"El archivo dice {hz} Hz; la frecuencia va de {DatosDelTablero.FrecuenciaMinimaHz} Hz en adelante. Se abrió con {d.FrecuenciaHz} Hz.");
            }
            if (a.NumeroEspacios is { } espacios)
            {
                if (d.EspaciosValidos.Contains(espacios))
                    d.NumeroEspacios = espacios;
                else
                {
                    d.NumeroEspacios = d.EspaciosValidos.FirstOrDefault(e => e >= espacios, d.EspaciosValidos[^1]);
                    avisos.Add($"Un gabinete de {espacios} espacios no se ofrece para {d.EtiquetaSistema}; se abrió con {d.NumeroEspacios}.");
                }
            }

            // Después de las fases: entrar a 1F-2H pone zapatas, y lo del archivo va encima.
            d.TipoAcometida = a.TipoAcometida ?? d.TipoAcometida;
            d.MontajePrincipal = a.MontajePrincipal ?? d.MontajePrincipal;
            d.EspacioDelPrincipal = a.EspacioDelPrincipal;

            foreach (var (nombre, factor) in a.FactoresDeDemanda ?? [])
                foreach (var categoria in Categorias(nombre))
                    d.CambiarFactorDeDemanda(categoria, factor);
            foreach (var (nombre, lista) in a.Justificaciones ?? [])
                foreach (var categoria in Categorias(nombre))
                    foreach (var j in lista)
                        d.Justificaciones[categoria].Add(j);
            foreach (var (nombre, texto) in a.JustificacionOtra ?? [])
                foreach (var categoria in Categorias(nombre))
                    d.JustificacionOtra[categoria] = texto;

            d.MaterialConductor = a.MaterialConductor ?? d.MaterialConductor;
            d.TipoAislamiento = a.TipoAislamiento ?? d.TipoAislamiento;
            // Hasta el formato 10, «húmedo o mojado» era una opción: se abre como mojado, la más estricta, y se dice.
            if (a.Lugar is { } lugar)
                d.Lugar = lugar;
            else if (a.LugarSeco is { } seco)
            {
                d.Lugar = seco ? LugarDeInstalacion.Seco : LugarDeInstalacion.Mojado;
                if (!seco)
                    avisos.Add("El archivo decía lugar «húmedo o mojado», que ahora son dos opciones: se abrió como mojado, la más " +
                               "estricta — Tabla 310-104(a), 310-10(c). Si el lugar es húmedo, cámbialo en Condiciones de cálculo.");
            }
            d.TerminalesMarcadas75C = a.TerminalesMarcadas75C ?? d.TerminalesMarcadas75C;
            d.TemperaturaAmbienteC = a.TemperaturaAmbienteC ?? d.TemperaturaAmbienteC;
            d.CargaNoLineal = a.CargaNoLineal ?? d.CargaNoLineal;
            d.NeutroReducido220_61 = a.NeutroReducido220_61 ?? d.NeutroReducido220_61;
            d.ConductoresPorFaseAlimentador = a.ConductoresPorFaseAlimentador;
            foreach (var (clave, mm) in a.DiametrosFabricante ?? [])
                d.DiametrosFabricante[clave] = mm;
            d.TuboAlNacer = a.TuboAlNacer ?? d.TuboAlNacer;
            d.CaidaMaxDerivadoPct = a.CaidaMaxDerivadoPct ?? d.CaidaMaxDerivadoPct;
            d.CaidaMaxAlimentadorPct = a.CaidaMaxAlimentadorPct ?? d.CaidaMaxAlimentadorPct;
            d.LongitudAlimentadorM = CircuitoJson.NoNegativo(a.LongitudAlimentadorM, d.LongitudAlimentadorM, "El alimentador, en la longitud,", avisos);
            d.ConjuntoAprobado100Pct = a.ConjuntoAprobado100Pct ?? d.ConjuntoAprobado100Pct;
        }

        // Los renglones existen hasta que se recalcula con los espacios ya puestos.
        cuadro.Recalcular();

        foreach (var t in archivo.Canalizaciones ?? [])
        {
            if (string.IsNullOrWhiteSpace(t.Id) || t.Id == "Alimentador" || d.Canalizaciones.Any(x => x.Id == t.Id))
                continue;
            var tubo = new CanalizacionDelTablero(t.Id, t.Automatica ?? false);
            t.Aplicar(tubo);
            if (d.NombreOcupado(tubo.Nombre, null))
            {
                // Editado a mano: dos con el mismo nombre se verían iguales en «Canal.» — I-94.
                avisos.Add($"Dos canalizaciones se llaman «{tubo.Nombre}»; la {tubo.Id} se abrió con su número.");
                tubo.Nombre = tubo.Id;
            }
            d.Canalizaciones.Add(tubo);
        }
        archivo.CanalizacionAlimentador?.Aplicar(d.CanalizacionAlimentador);

        foreach (var c in archivo.Circuitos ?? [])
        {
            if (c.Espacio is not { } espacio || espacio < 1 || espacio > cuadro.Circuitos.Count)
            {
                avisos.Add($"El circuito {c.Espacio} no cabe en un tablero de {d.NumeroEspacios} espacios; se omitió.");
                continue;
            }
            c.Aplicar(cuadro.Circuitos[espacio - 1], d, avisos);
        }
    }

    /// <summary>
    /// Los tipos a los que va un factor o una justificación del archivo. El «MotorOAireAcondicionado»
    /// del formato 1 va a los dos que salieron de él: Motor y A/C y refrigeración (I-74).
    /// </summary>
    private static IEnumerable<CategoriaDeCarga> Categorias(string nombre) =>
        CategoriasDeCarga.DelArchivo(nombre, out var eraMotorOAire) is { } categoria ? [categoria]
        : eraMotorOAire ? [CategoriaDeCarga.Motor, CategoriaDeCarga.AireAcondicionado]
        : [];
}

/// <summary>Lo que resultó de abrir un archivo: el cuadro, o por qué no se pudo.</summary>
public sealed record Apertura(CuadroDeCarga? Cuadro, string? Error, IReadOnlyList<string> Avisos)
{
    public static Apertura Fallo(string error) => new(null, error, []);
}
