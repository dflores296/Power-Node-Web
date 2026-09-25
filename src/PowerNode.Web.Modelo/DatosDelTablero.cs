using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Tableros;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// La cabecera del cuadro de carga: <b>quién es el tablero, de qué sistema cuelga y con qué
/// condiciones se calcula</b>.
///
/// <para>
/// <b>Los campos salen del Excel</b> (<c>CC_NOM_2012_nuevo_nube.xlsx</c>, hoja «Cuadro de Carga»,
/// bloque de identificación de las filas 19 a 26), no de una idea de qué debería llevar un cuadro
/// de carga: TABLERO, CLAVE, UBICACIÓN, PROYECTO, CLIENTE, quién diseñó / revisó / aprobó, la
/// fecha y la revisión a la izquierda y a la derecha; MONTAJE, MATERIAL DE BARRAS y GABINETE NEMA
/// en medio; y TENSIÓN F-F, TENSIÓN F-N, FRECUENCIA, No. DE FASES y No. DE HILOS como el sistema.
/// </para>
///
/// <para>
/// <b>Lo que el Excel sacaba del catálogo no está</b> —CATÁLOGO, MARCA, INTERIOR, CAJA, FRENTE,
/// DIMENSIONES, CAPACIDAD—: ese bloque son ocho <c>INDEX/MATCH</c> contra la base de tableros
/// Square D, y este repo es público. Ver <c>docs/decisiones/sin-catalogo-square-d.md</c>. Lo único
/// que sobrevive de ahí es <see cref="CapacidadBarraA"/>, y se captura a mano porque sin ella el
/// 408-36 no se puede verificar.
/// </para>
/// </summary>
public sealed class DatosDelTablero
{
    // ---- Identificación (Excel B19:T26 y BJ19:BM26) --------------------------------------------

    public string Tablero { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string Ubicacion { get; set; } = string.Empty;
    public string Proyecto { get; set; } = string.Empty;
    public string Cliente { get; set; } = string.Empty;
    public string Diseno { get; set; } = string.Empty;
    public string Reviso { get; set; } = string.Empty;
    public string Aprobo { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Revision { get; set; } = string.Empty;

    // ---- Gabinete ------------------------------------------------------------------------------

    /// <summary>SOBREPONER o EMPOTRAR, como el Excel (J23). Texto libre: no hay catálogo que validar.</summary>
    public string Montaje { get; set; } = "Sobreponer";

    /// <summary>Material de las barras (J24). Es dato del tablero, no del conductor de los circuitos.</summary>
    public MaterialConductor MaterialBarras { get; set; } = MaterialConductor.Cobre;

    /// <summary>Tipo de envolvente NEMA (J25): 1, 3R, 12… Texto libre por la misma razón que <see cref="Montaje"/>.</summary>
    public string GabineteNema { get; set; } = "1";

    /// <summary>
    /// Espacios de un polo del gabinete. <b>42 es el tope del formato</b>: el Excel trae 21 renglones
    /// de nones y 21 de pares, y para un tablero más chico se ocultaban renglones a mano. Aquí no
    /// hace falta ocultar nada — el cuadro se dibuja con los espacios que se declaren.
    /// </summary>
    public int NumeroEspacios { get; set; } = 24;

    /// <summary>
    /// Los tamaños de gabinete que se ofrecen. 1F-2H tiene una sola barra: centros de carga de 1, 2,
    /// 4, 6 y 8 espacios, los que se venden; de 12 en adelante no existen para 1F-2H (David,
    /// 2026-09-25 — I-54). Los demás, de 6 a 42 como el Excel.
    /// </summary>
    public IReadOnlyList<int> EspaciosValidos =>
        SistemaDelTablero.De(Sistema) == ConfiguracionTablero.UnaFaseDosHilos
            ? [1, 2, 4, 6, 8]
            : [6, 12, 18, 24, 30, 36, 42];

    /// <summary>
    /// Al cambiar de configuración, un gabinete que no existe para la nueva pasa al más cercano: de
    /// 24 espacios a 1F-2H, 8; de 4 en 1F-2H a 3F, 6.
    /// </summary>
    private void AjustarEspacios()
    {
        var validos = EspaciosValidos;
        if (!validos.Contains(NumeroEspacios))
            NumeroEspacios = validos.FirstOrDefault(e => e >= NumeroEspacios, validos[^1]);
    }

    /// <summary>Zapatas principales o interruptor principal propio. Decide qué dice el 408-36.</summary>
    public TipoAcometidaTablero TipoAcometida { get; set; } = TipoAcometidaTablero.InterruptorPrincipal;

    /// <summary>
    /// Capacidad nominal de la barra, en amperes. <b><c>null</c> es válido y no dispara nada</b>:
    /// declarar un incumplimiento por un dato que no se capturó es el error contrario al que la
    /// verificación evita. Ver <see cref="Verificacion408_36"/>.
    /// </summary>
    public decimal? CapacidadBarraA { get; set; }

    /// <summary>
    /// El tablero es el medio de desconexión de la acometida: el primero después del medidor. Solo
    /// entonces aplica el mínimo de 230-79, que depende de <see cref="Inmueble"/> — R-11. Falso por
    /// omisión. Sustituye al «Mínimo del principal (A)», que solo avisaba (David, 2026-09-24).
    /// </summary>
    public bool EsEquipoDeAcometida { get; set; }

    /// <summary>
    /// El inmueble del tablero — R-19. <b>Uno solo para dos criterios</b>: el mínimo del principal de
    /// 230-79 (si es <see cref="EsEquipoDeAcometida"/>) y las justificaciones del factor de demanda
    /// (Tabla 220-42, vivienda o no). Ver <see cref="TipoDeInmueble"/>.
    /// </summary>
    public TipoDeInmueble Inmueble { get; set; } = TipoDeInmueble.Otro;

    /// <summary>
    /// El mínimo del principal por 230-79, con su referencia. <c>null</c> si el tablero no es equipo de
    /// acometida o si es vivienda unifamiliar, donde la norma dice «según la carga conectada»: sin
    /// número, manda el cálculo.
    /// </summary>
    public (decimal Amperes, string Referencia)? Minimo230_79 => EsEquipoDeAcometida ? Inmueble.Minimo230_79() : null;

    /// <summary>
    /// La familia de interruptores que se instala, que decide de qué tamaños de la 240-6(a) se
    /// escoge. <b>Centro de carga (NEMA) por omisión</b> — decisión de David del 2026-09-23.
    /// </summary>
    public SerieDeInterruptores SerieInterruptores { get; set; } = SerieDeInterruptores.CentroDeCargaNema;

    // ---- Sistema (Excel T22:T26) ---------------------------------------------------------------

    /// <summary>
    /// La tensión del sistema: entre fases, salvo en 1F-2H, que no tiene dos fases y es la de fase a
    /// neutro (127 V). Por eso la pantalla la rotula «Tensión F-N» en 1F-2H — I-53.
    /// </summary>
    public decimal TensionFaseFaseV { get; set; } = 220m;

    /// <summary>
    /// Al cambiar las fases, los hilos pasan a los del sistema más común de esas fases: «1 fase,
    /// 4 hilos» no es nada — el motor lo leía como 1F-3H sin avisar. Siempre al más común, y no solo
    /// cuando quedan inválidos: de 3F-4H a 2F y de regreso a 3F, conservar los 3 hilos dejaba un
    /// 3F-3H (delta) que nadie pidió.
    /// </summary>
    public int Fases
    {
        get => _fases;
        set
        {
            if (value == _fases)
                return;
            _fases = value;
            _hilos = HilosPorOmision(value);
            AjustarTension();
            AjustarEspacios();
        }
    }
    private int _fases = 3;

    /// <summary>Fases más neutro. La tierra no se cuenta: 1F-2H es fase, neutro y tierra.</summary>
    public int Hilos
    {
        get => _hilos;
        set
        {
            if (value == _hilos)
                return;
            _hilos = value;
            AjustarTension();
            AjustarEspacios();
        }
    }
    private int _hilos = 4;

    /// <summary>
    /// Las tensiones nominales de la NOM para la configuración — 110-4 y 220-5(a). En 1F-2H, de fase
    /// a neutro; en las demás, entre fases. 1F-3H es 120/240: con 220 V calculaba 110 V fase-neutro,
    /// que no existe (I-53). La primera es la que se propone al cambiar de configuración.
    /// </summary>
    public IReadOnlyList<decimal> TensionesNominales => SistemaDelTablero.De(Sistema) switch
    {
        ConfiguracionTablero.UnaFaseDosHilos => [127m, 120m],
        ConfiguracionTablero.UnaFaseTresHilos => [240m],
        ConfiguracionTablero.DosFasesDeEstrella => [220m, 208m],
        ConfiguracionTablero.TresFasesTresHilos => [220m, 208m, 240m, 440m, 460m, 480m, 600m],
        _ => [220m, 208m, 480m, 600m],
    };

    /// <summary>
    /// Al cambiar de configuración, una tensión que no es nominal para la nueva pasa a la que sí lo
    /// es: 127 V de un 1F-2H no son 127 V entre fases de un 2F-3H. Una que ya es nominal se queda:
    /// de 3F-4H a 480 V a 3F-3H, sigue en 480.
    /// </summary>
    private void AjustarTension()
    {
        if (!TensionesNominales.Contains(TensionFaseFaseV))
            TensionFaseFaseV = TensionesNominales[0];
    }

    /// <summary>La tensión capturada no es nominal de la NOM para la configuración — 110-4.</summary>
    public string? AvisoTension
    {
        get
        {
            if (TensionesNominales.Contains(TensionFaseFaseV))
                return null;
            var configuracion = SistemaDelTablero.De(Sistema);
            if (configuracion == ConfiguracionTablero.UnaFaseDosHilos)
                return $"{TensionFaseFaseV:0.##} V no es una tensión nominal de la NOM para 1F-2H (fase y neutro): 127 o 120 V — 110-4.";
            var nominales = configuracion switch
            {
                ConfiguracionTablero.UnaFaseTresHilos => "120/240 V",
                ConfiguracionTablero.DosFasesDeEstrella => "220/127 o 208/120 V",
                ConfiguracionTablero.TresFasesTresHilos => "220, 208, 240, 440, 460, 480 o 600 V entre fases",
                _ => "220Y/127, 208Y/120, 480Y/277 o 600Y/347 V",
            };
            var fn = configuracion == ConfiguracionTablero.TresFasesTresHilos
                ? ""
                : $" Con {TensionFaseFaseV:0.##} V entre fases, la de fase a neutro queda en {TensionFaseNeutroV:0.##} V.";
            return $"{TensionFaseFaseV:0.##} V no es una tensión nominal de la NOM para {EtiquetaSistema}: {nominales} — 110-4.{fn}";
        }
    }

    /// <summary>
    /// Los hilos que admite cada número de fases: 1F-2H o 1F-3H; 2F de estrella solo con neutro,
    /// 3 hilos; 3F-3H (delta) o 3F-4H (estrella).
    /// </summary>
    public IReadOnlyList<int> HilosValidos => HilosDe(Fases);

    private static int[] HilosDe(int fases) => fases switch
    {
        <= 1 => [2, 3],
        2 => [3],
        _ => [3, 4],
    };

    /// <summary>El más común de cada caso: 1F-2H, 2F-3H, 3F-4H.</summary>
    private static int HilosPorOmision(int fases) => fases switch { <= 1 => 2, 2 => 3, _ => 4 };
    public int FrecuenciaHz { get; set; } = 60;

    /// <summary>
    /// Fases <b>y</b> hilos juntos, que es lo único que dice cuántas barras energizadas hay. Un
    /// 1F-3H tiene dos y un 1F-2H tiene una, y las dos cosas son «1 fase».
    /// </summary>
    public SistemaTablero Sistema => new(Fases, Hilos);

    /// <summary>
    /// La tensión fase-neutro que le corresponde al sistema.
    ///
    /// <para>
    /// <b>No es siempre ÷√3</b>, que es lo que hace el Excel (<c>=ROUND(T22/SQRT(3),1)</c>) y lo que
    /// hacía esta pantalla antes: en un 1F-3H la derivación central es <b>÷2</b> —240 da 120, no
    /// 138.6— y en un 1F-2H la tensión capturada ya <i>es</i> la fase-neutro. Lo resuelve
    /// <see cref="SistemaDelTablero"/>, que es el único lugar del motor donde vive esa regla.
    /// </para>
    /// </summary>
    public decimal TensionFaseNeutroV => SistemaDelTablero.TensionFaseNeutro(TensionFaseFaseV, Sistema);

    /// <summary>Las barras del tablero: 'A', 'B', 'C' según el sistema.</summary>
    public IReadOnlyList<char> Barras => SistemaDelTablero.Barras(Sistema);

    /// <summary>Máximo de polos de un interruptor de este tablero: tantos como barras.</summary>
    public int MaximoPolos => SistemaDelTablero.MaximoPolos(Sistema);

    /// <summary>
    /// Cómo se rotula el sistema en el encabezado del cuadro: «3F-4H (estrella)». Sale de la
    /// configuración que reconoce el motor, no de concatenar fases e hilos, para que un 1F-3H diga
    /// que es derivación central y no se lea como un monofásico cualquiera.
    /// </summary>
    public string EtiquetaSistema => SistemaDelTablero.De(Sistema) switch
    {
        ConfiguracionTablero.UnaFaseDosHilos => "1F-2H",
        ConfiguracionTablero.UnaFaseTresHilos => "1F-3H (derivación central)",
        ConfiguracionTablero.DosFasesDeEstrella => "2F-3H (dos fases de estrella)",
        ConfiguracionTablero.TresFasesTresHilos => "3F-3H (delta, sin neutro)",
        _ => "3F-4H (estrella)",
    };

    public bool UsaInterruptorPrincipal => TipoAcometida == TipoAcometidaTablero.InterruptorPrincipal;

    // ---- Factores de demanda, por tipo de carga (antes: continua y no continua, Excel AX79 y AZ79) --

    /// <summary>
    /// <b>Factor de demanda por tipo de carga</b> — R-17. 220-40: la carga del alimentador es la suma
    /// de los derivados «después de aplicar cualquier factor de demanda aplicable», y el Art. 220 los
    /// da por tipo. Los captura el proyectista; 1.0 = sin reducción. Motores y A/C (430-26) y
    /// calefacción (220-51, Excepción) también, con su condición — R-18. Sustituye a los dos factores
    /// del Excel (continua y no continua): el 125 % de 215-3 se sigue aplicando a la parte continua.
    /// </summary>
    public decimal FactorDemandaAlumbrado { get; set; } = 1m;
    public decimal FactorDemandaContactos { get; set; } = 1m;
    public decimal FactorDemandaEquipo { get; set; } = 1m;
    public decimal FactorDemandaMotores { get; set; } = 1m;
    public decimal FactorDemandaCalefaccion { get; set; } = 1m;

    /// <summary>El factor de un tipo.</summary>
    public decimal FactorDeDemanda(CategoriaDeCarga categoria) => categoria switch
    {
        CategoriaDeCarga.Alumbrado => FactorDemandaAlumbrado,
        CategoriaDeCarga.Contactos => FactorDemandaContactos,
        CategoriaDeCarga.Equipo => FactorDemandaEquipo,
        CategoriaDeCarga.MotorOAireAcondicionado => FactorDemandaMotores,
        _ => FactorDemandaCalefaccion,
    };

    /// <summary>Cambia el factor de un tipo.</summary>
    public void CambiarFactorDeDemanda(CategoriaDeCarga categoria, decimal factor)
    {
        switch (categoria)
        {
            case CategoriaDeCarga.Alumbrado: FactorDemandaAlumbrado = factor; break;
            case CategoriaDeCarga.Contactos: FactorDemandaContactos = factor; break;
            case CategoriaDeCarga.Equipo: FactorDemandaEquipo = factor; break;
            case CategoriaDeCarga.MotorOAireAcondicionado: FactorDemandaMotores = factor; break;
            default: FactorDemandaCalefaccion = factor; break;
        }
    }

    /// <summary>Algún tipo reduce su carga: la memoria pide justificación — R-12.</summary>
    public bool ReduceCargaPorDemanda => CategoriasDeCarga.Todas.Any(c => FactorDeDemanda(c) < 1m);

    /// <summary>Con qué se justifica el factor de cada tipo. Varias por tipo — R-12, R-17.</summary>
    public Dictionary<CategoriaDeCarga, HashSet<JustificacionFactorDemanda>> Justificaciones { get; } =
        CategoriasDeCarga.Todas.ToDictionary(c => c, _ => new HashSet<JustificacionFactorDemanda>());

    /// <summary>El texto de «Otra — criterio del proyectista», por tipo.</summary>
    public Dictionary<CategoriaDeCarga, string> JustificacionOtra { get; } =
        CategoriasDeCarga.Todas.ToDictionary(c => c, _ => string.Empty);

    /// <summary>
    /// La justificación de un tipo como la imprime la memoria, en el orden de la lista. <c>null</c> si
    /// falta: ninguna escogida, o solo «Otra» sin texto.
    /// </summary>
    public string? JustificacionDe(CategoriaDeCarga categoria)
    {
        if (!Justificaciones.TryGetValue(categoria, out var escogidas))
            return null;

        var partes = categoria.JustificacionesPosibles(Inmueble)
            .Where(escogidas.Contains)
            .Select(j => j == JustificacionFactorDemanda.Otra
                ? (string.IsNullOrWhiteSpace(JustificacionOtra[categoria]) ? null : $"Criterio del proyectista: {JustificacionOtra[categoria].Trim()}")
                : j == JustificacionFactorDemanda.AlumbradoGeneral && Inmueble.FilaTabla220_42() is { } fila
                    ? $"{j.Nombre()} ({fila})"
                    : j.Nombre())
            .OfType<string>()
            .ToList();
        return partes.Count == 0 ? null : string.Join("; ", partes);
    }

    /// <summary>Los tipos con factor menor que 1 y sin justificación.</summary>
    public IEnumerable<CategoriaDeCarga> SinJustificacion =>
        CategoriasDeCarga.Todas.Where(c => FactorDeDemanda(c) < 1m && JustificacionDe(c) is null);

    // ---- Condiciones de cálculo (Excel columnas DA a DR, iguales en todos los renglones) --------

    public MaterialConductor MaterialConductor { get; set; } = MaterialConductor.Cobre;

    /// <summary>
    /// El aislamiento del conductor, con su designación de la Tabla 310-104(a): decide la columna
    /// de temperatura de la Tabla 310-15(b)(16) en la que se aplican los factores de corrección
    /// (110-14(c)). <b>THHN por omisión</b>, que es lo que el programa suponía sin preguntar. Fijo
    /// en THHN, un circuito instalado con THW-LS podía salir con un calibre de menos: con 9
    /// conductores agrupados, 20 A continuos piden 10 AWG en THHN y 8 AWG en THW-LS.
    /// </summary>
    public string TipoAislamiento { get; set; } = "THHN";

    /// <summary>
    /// Lugar seco, o húmedo/mojado. Algunos aislamientos cambian de temperatura según el lugar
    /// (THHW-LS: 90 °C seco, 75 °C mojado) y otros no se permiten fuera de lugar seco (THHN) —
    /// Tabla 310-104(a).
    /// </summary>
    public bool LugarSeco { get; set; } = true;

    /// <summary>
    /// Las terminales del circuito —interruptor y equipo— están aprobadas e identificadas para 75 °C:
    /// 110-14(c)(1)a.(3). Muchos interruptores de centro de carga vienen marcados 60/75 °C. <b>Falso por
    /// omisión</b>: sin declaración, 60 °C hasta 100 A, que es la regla general y lo que se calculaba.
    /// </summary>
    public bool TerminalesMarcadas75C { get; set; }

    public decimal TemperaturaAmbienteC { get; set; } = 30m;

    // ---- Canalizaciones (decisión canalizaciones-y-agrupamiento, 2026-09-24) -------------------
    // ANTES: «Agrupados» (un número para todo el tablero) y «Canalización» (PVC, aluminio o acero,
    // solo para la Tabla 9). Un solo número se aplicaba igual a cada derivado y al alimentador — I-39.
    // Ahora se captura qué circuitos van juntos y el agrupamiento y el material salen de ahí.

    /// <summary>Las canalizaciones de los circuitos derivados, en orden de número: T1, T2…</summary>
    public List<CanalizacionDelTablero> Canalizaciones { get; } = [];

    /// <summary>La del alimentador, que va solo.</summary>
    public CanalizacionDelTablero CanalizacionAlimentador { get; } = new("Alimentador");

    /// <summary>
    /// La mayor parte de la carga es no lineal (electrónica, iluminación LED, equipo de cómputo): en
    /// 3F-4H el neutro lleva armónicas y cuenta como portador — 310-15(b)(5)(3).
    /// </summary>
    public bool CargaNoLineal { get; set; }

    /// <summary>
    /// Diámetro exterior del fabricante, en mm, por aislamiento y calibre («THW-LS|12»). Solo para
    /// aislamientos que no están en la Tabla 5 del Capítulo 10 — Nota 5.
    /// </summary>
    public Dictionary<string, decimal> DiametrosFabricante { get; } = [];

    public static string ClaveDiametro(string aislamiento, string designacion) => $"{aislamiento}|{designacion}";

    /// <summary>
    /// El tubo con el que nace cada canalización: EMT (David, 2026-09-24). No se captura en pantalla;
    /// se decide en cada canalización. Las pruebas de los casos de referencia, calculados con la
    /// Tabla 9 en PVC, lo fijan en PVC.
    /// </summary>
    public TipoTuboConduit TuboAlNacer { get; set; } = TipoTuboConduit.Emt;

    /// <summary>
    /// Agrega una canalización con el número libre más chico, en su lugar de la lista. Automática: la
    /// que nace al capturar la carga de un circuito, y se quita sola al quedarse sin circuitos.
    /// </summary>
    public CanalizacionDelTablero NuevaCanalizacion(bool automatica = false)
    {
        var n = 1;
        while (Canalizaciones.Any(c => c.Id == $"T{n}")) n++;
        var nueva = new CanalizacionDelTablero($"T{n}", automatica) { Tubo = TuboAlNacer };
        Canalizaciones.Insert(n - 1, nueva); // T1…T(n-1) existen y van antes
        return nueva;
    }

    public CanalizacionDelTablero? Canalizacion(string? id) =>
        id is null ? null : Canalizaciones.FirstOrDefault(c => c.Id == id);
    // SIN FACTOR DE POTENCIA DEL TABLERO. Un tablero no tiene F.P.: lo tienen sus cargas. Cada
    // circuito lleva el suyo (CircuitoDelCuadro.FactorPotencia) y el del alimentador resulta de
    // combinarlos. Decidido por David el 2026-09-23 — docs/decisiones/factor-de-potencia-por-circuito.md.
    public decimal CaidaMaxDerivadoPct { get; set; } = 3m;

    /// <summary>
    /// Límite de caída del alimentador, por tramo: <b>2 % por omisión</b>, para que con el 3 % del
    /// derivado la suma quede en el 5 % combinado — 215-2(a)(4) NOTA 2, 210-19(a)(1) NOTA 4 (R-15).
    /// Estuvo fijo en 5 % (R-01) y luego en 3 %, que con el derivado sumaba 6 %.
    /// </summary>
    public decimal CaidaMaxAlimentadorPct { get; set; } = 2m;

    /// <summary>
    /// Caída combinada alimentador + derivado hasta la salida más lejana: 5 % — 215-2(a)(4) NOTA 2 y
    /// 210-19(a)(1) NOTA 4. Fijo: lo da la norma, no el proyecto. Solo avisa.
    /// </summary>
    public const decimal CaidaMaxCombinadaPct = 5m;

    /// <summary>
    /// Aviso cuando los dos límites por tramo suman más que el combinado: con ellos, un circuito puede
    /// quedar arriba del 5 % aunque cada tramo cumpla el suyo. <c>null</c> si suman 5 % o menos.
    /// </summary>
    public string? AvisoLimitesDeCaida =>
        CaidaMaxAlimentadorPct + CaidaMaxDerivadoPct > CaidaMaxCombinadaPct
            ? $"Los límites suman {CaidaMaxAlimentadorPct + CaidaMaxDerivadoPct:0.##} %: un circuito puede quedar arriba del " +
              $"{CaidaMaxCombinadaPct:0} % combinado — 210-19(a)(1) NOTA 4, 215-2(a)(4) NOTA 2."
            : null;

    /// <summary>Longitud del alimentador que llega al tablero, en metros — la «L» de la fila 79 del Excel.</summary>
    public decimal LongitudAlimentadorM { get; set; } = 20m;

    /// <summary>
    /// El ensamble (gabinete + interruptor principal) está aprobado para operar al 100 % de su
    /// valor nominal — 215-3. <b>Falso por omisión</b>, que es la regla general: es dato de placa y
    /// de marcado, no una opción de diseño.
    /// </summary>
    public bool ConjuntoAprobado100Pct { get; set; }
}
