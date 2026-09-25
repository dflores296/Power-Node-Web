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

    /// <summary>Sube cuando un archivo nuevo ya no se puede leer igual que uno anterior.</summary>
    public const int Version = 1;

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
            return Apertura.Fallo($"El archivo es de una versión más nueva de Power Node (formato {version}; esta lee hasta el {Version}). Recarga la página con Ctrl+F5 y vuelve a abrirlo.");

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
                NumeroEspacios = d.NumeroEspacios, TipoAcometida = d.TipoAcometida, CapacidadBarraA = d.CapacidadBarraA,
                EsEquipoDeAcometida = d.EsEquipoDeAcometida, Inmueble = d.Inmueble, SerieInterruptores = d.SerieInterruptores,
                TensionFaseFaseV = d.TensionFaseFaseV, Fases = d.Fases, Hilos = d.Hilos, FrecuenciaHz = d.FrecuenciaHz,
                FactoresDeDemanda = CategoriasDeCarga.Todas.ToDictionary(c => c, d.FactorDeDemanda),
                Justificaciones = d.Justificaciones
                    .Where(j => j.Value.Count > 0)
                    .ToDictionary(j => j.Key, j => j.Value.OrderBy(x => x).ToList()),
                JustificacionOtra = d.JustificacionOtra
                    .Where(j => !string.IsNullOrWhiteSpace(j.Value))
                    .ToDictionary(j => j.Key, j => j.Value),
                MaterialConductor = d.MaterialConductor, TipoAislamiento = d.TipoAislamiento, LugarSeco = d.LugarSeco,
                TerminalesMarcadas75C = d.TerminalesMarcadas75C, TemperaturaAmbienteC = d.TemperaturaAmbienteC,
                CargaNoLineal = d.CargaNoLineal,
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
            d.TipoAcometida = a.TipoAcometida ?? d.TipoAcometida;
            d.CapacidadBarraA = a.CapacidadBarraA;
            d.EsEquipoDeAcometida = a.EsEquipoDeAcometida ?? d.EsEquipoDeAcometida;
            d.Inmueble = a.Inmueble ?? d.Inmueble;
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
            d.TensionFaseFaseV = a.TensionFaseFaseV ?? d.TensionFaseFaseV;
            d.FrecuenciaHz = a.FrecuenciaHz ?? d.FrecuenciaHz;
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

            foreach (var (categoria, factor) in a.FactoresDeDemanda ?? [])
                d.CambiarFactorDeDemanda(categoria, factor);
            foreach (var (categoria, lista) in a.Justificaciones ?? [])
                foreach (var j in lista)
                    d.Justificaciones[categoria].Add(j);
            foreach (var (categoria, texto) in a.JustificacionOtra ?? [])
                d.JustificacionOtra[categoria] = texto;

            d.MaterialConductor = a.MaterialConductor ?? d.MaterialConductor;
            d.TipoAislamiento = a.TipoAislamiento ?? d.TipoAislamiento;
            d.LugarSeco = a.LugarSeco ?? d.LugarSeco;
            d.TerminalesMarcadas75C = a.TerminalesMarcadas75C ?? d.TerminalesMarcadas75C;
            d.TemperaturaAmbienteC = a.TemperaturaAmbienteC ?? d.TemperaturaAmbienteC;
            d.CargaNoLineal = a.CargaNoLineal ?? d.CargaNoLineal;
            foreach (var (clave, mm) in a.DiametrosFabricante ?? [])
                d.DiametrosFabricante[clave] = mm;
            d.TuboAlNacer = a.TuboAlNacer ?? d.TuboAlNacer;
            d.CaidaMaxDerivadoPct = a.CaidaMaxDerivadoPct ?? d.CaidaMaxDerivadoPct;
            d.CaidaMaxAlimentadorPct = a.CaidaMaxAlimentadorPct ?? d.CaidaMaxAlimentadorPct;
            d.LongitudAlimentadorM = a.LongitudAlimentadorM ?? d.LongitudAlimentadorM;
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
            c.Aplicar(cuadro.Circuitos[espacio - 1], d);
        }
    }
}

/// <summary>Lo que resultó de abrir un archivo: el cuadro, o por qué no se pudo.</summary>
public sealed record Apertura(CuadroDeCarga? Cuadro, string? Error, IReadOnlyList<string> Avisos)
{
    public static Apertura Fallo(string error) => new(null, error, []);
}
