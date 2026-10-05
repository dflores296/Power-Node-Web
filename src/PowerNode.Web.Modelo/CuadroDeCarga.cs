using System.Globalization;
using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Magnitudes;
using PowerNode.DesignSuite.Calculo.Tableros;
using PowerNode.DesignSuite.Calculo.Unidades;
using PowerNode.DesignSuite.Calculo.Validaciones;
using PowerNode.Web.Modelo.Archivo;

namespace PowerNode.Web.Modelo;

/// <summary>Un renglón del resumen: la carga de un tipo, su factor de demanda y lo que queda — R-17.</summary>
public sealed record CargaPorCategoria(CategoriaDeCarga Categoria, decimal InstaladaVA, decimal FactorDemanda, decimal DemandadaVA);

/// <summary>
/// El resumen de carga: por tipo de carga, cada uno con su factor de demanda (R-17), y la misma carga
/// partida en continua y no continua, que es la que decide el 125 % del alimentador (215-3).
/// </summary>
/// <param name="ContinuaDemandadaVA">La continua de todos los circuitos, cada uno con el F.D. de su tipo.</param>
/// <param name="Minimo220_52VA">
/// Lo que se agrega para que cada circuito de aparatos pequeños o de lavadora cuente 1500 VA —
/// 220-52. Renglón propio del resumen: no está en <see cref="NoContinuaVA"/>, que es la suma de los
/// renglones. Lleva el F.D. de contactos.
/// </param>
/// <param name="MotoresVA">
/// Los motores en HP, que no son continua ni no continua: van al alimentador por 430-24 — I-15.
/// </param>
public sealed record ResumenDeCarga(
    decimal ContinuaVA,
    decimal ContinuaDemandadaVA,
    decimal NoContinuaVA,
    decimal NoContinuaDemandadaVA,
    IReadOnlyDictionary<char, decimal> CargaPorFaseVA,
    decimal DesbalanceoPct,
    decimal InstaladaW = 0m,
    decimal DemandadaW = 0m,
    decimal FactorPotencia = 1m,
    decimal Minimo220_52VA = 0m,
    decimal Minimo220_52DemandadoVA = 0m,
    IReadOnlyList<CargaPorCategoria>? PorCategoria = null,
    decimal MotoresVA = 0m,
    decimal MotoresDemandadaVA = 0m,
    MinimoPorSuperficie? Superficie = null)
{
    public decimal InstaladaVA => ContinuaVA + NoContinuaVA + MotoresVA;

    /// <summary>La carga que va al alimentador: la instalada más los mínimos de 220-52 y 220-12 / 220-14(k).</summary>
    public decimal CalculadaVA => InstaladaVA + Minimo220_52VA + (Superficie?.AjusteTotalVA ?? 0m);
    public decimal DemandadaVA => ContinuaDemandadaVA + NoContinuaDemandadaVA + Minimo220_52DemandadoVA + MotoresDemandadaVA
                                  + (Superficie?.ContinuaDemandadaVA ?? 0m) + (Superficie?.NoContinuaDemandadaVA ?? 0m);
}

/// <summary>
/// <b>El mínimo de alumbrado general por superficie</b> — M-14. El alimentador no baja de la carga de la
/// Tabla 220-12 (220-12, 210-11(b)); lo que falta para llegar se agrega como alumbrado. En vivienda, los
/// contactos de uso general ya van dentro (220-14(j)); en bancos y oficinas, los contactos no bajan de
/// 11 VA/m² (220-14(k)).
/// </summary>
/// <param name="Renglon">El renglón de la Tabla 220-12: «Edificios de oficinas».</param>
/// <param name="AlumbradoCapturadoVA">El alumbrado capturado; en vivienda, con los contactos de uso general.</param>
/// <param name="AjusteAlumbradoVA">Lo que se agrega para llegar al mínimo — 220-12.</param>
/// <param name="AlumbradoContinuo">Fuera de vivienda, el alumbrado general opera 3 h o más: continuo.</param>
/// <param name="MinimoContactosVA">220-14(k): 11 VA/m²; <c>null</c> fuera de bancos y oficinas.</param>
/// <param name="AlumbradoNoGeneralVA">
/// El alumbrado capturado que no es general y no cuenta para el mínimo — I-157: anuncios (220-14(f)),
/// aparadores (220-14(g)) y portalámparas de trabajo pesado (220-14(e)).
/// </param>
public sealed record MinimoPorSuperficie(
    string Renglon,
    decimal VaPorM2,
    decimal AreaM2,
    decimal AlumbradoCapturadoVA,
    bool IncluyeContactos,
    decimal AjusteAlumbradoVA,
    bool AlumbradoContinuo,
    decimal? MinimoContactosVA,
    decimal ContactosCapturadosVA,
    decimal AjusteContactosVA,
    decimal ContinuaDemandadaVA,
    decimal NoContinuaDemandadaVA,
    decimal AlumbradoNoGeneralVA = 0m)
{
    public decimal MinimoAlumbradoVA => VaPorM2 * AreaM2;
    public decimal AjusteTotalVA => AjusteAlumbradoVA + AjusteContactosVA;
    public decimal AjusteContinuaVA => AlumbradoContinuo ? AjusteAlumbradoVA : 0m;
    public decimal AjusteNoContinuaVA => (AlumbradoContinuo ? 0m : AjusteAlumbradoVA) + AjusteContactosVA;
}

/// <summary>
/// <b>Cómo se combinan los factores de potencia de varias cargas.</b> Los W se suman directo; los
/// VAR también; los VA <b>no</b>: la aparente del conjunto es √(P² + Q²). El F.P. que resulta es
/// P / √(P² + Q²) — ni el promedio de los F.P. ni los W entre la suma aritmética de los VA.
/// </summary>
public static class FactorPotenciaCombinado
{
    /// <param name="cargas">Cada carga con sus VA y su F.P.</param>
    /// <returns>1 si no hay carga: sin corriente no hay ángulo que reportar.</returns>
    public static decimal De(IEnumerable<(decimal VA, decimal FactorPotencia)> cargas)
    {
        double p = 0, q = 0;
        foreach (var (va, fp) in cargas)
        {
            p += (double)(va * fp);
            q += (double)(va * TrianguloPotencias.SenoDelAngulo(fp));
        }

        var s = Math.Sqrt(p * p + q * q);
        return s <= 0 ? 1m : Math.Round((decimal)(p / s), 4);
    }
}

/// <summary>
/// Lo que lleva una barra del alimentador, en amperes: la parte continua y la no continua ya con su
/// factor de demanda, y la capacidad que piden juntas (215-2(a)(1): 125 % de la continua + 100 % de
/// la no continua).
/// </summary>
/// <param name="FactorContinua">1.25, o 1.00 con el ensamble aprobado al 100 % — 215-3.</param>
/// <param name="Motores">Los motores en HP que toca la barra, ya con su factor de demanda: su capacidad
/// es la de 430-24 — 125 % del mayor + la suma de los demás (I-15).</param>
public sealed record CorrienteDeFase(char Fase, decimal ContinuaA, decimal NoContinuaA, decimal FactorContinua, AgregadoMotores Motores = default)
{
    public decimal TotalA => ContinuaA + NoContinuaA + Motores.CorrienteRealA;
    public decimal CapacidadA => FactorContinua * ContinuaA + NoContinuaA + Motores.CapacidadMinimaA;
    public bool TieneMotores => Motores.MayorFlcA is not null;
}

/// <summary>
/// El renglón del alimentador: la fila 79 del Excel, «ALIMENTADOR Y PROTECCIÓN PRINCIPAL».
/// <see cref="Resultado"/> trae el interruptor principal en <c>ProteccionA</c>.
/// </summary>
/// <param name="Fases">La corriente de cada barra, en el orden de las barras.</param>
/// <param name="Gobierna">La fase más cargada, que es con la que se dimensiona. <c>null</c> sin carga.</param>
/// <param name="FactorPotencia">
/// El F.P. <b>de las cargas de la fase que gobierna</b>, combinado con su factor de demanda y el
/// mínimo de 220-52: el de la corriente con la que se calcula la caída de tensión del alimentador.
/// </param>
/// <summary>
/// El neutro del alimentador reducido por 220-61 — I-161: la fase de mayor carga al neutro, esa carga, la
/// corriente con la que se eligió (con el 70 % arriba de 200 A si aplicó) y el calibre.
/// </summary>
/// <param name="Impedancia">R y X del neutro reducido, de la Tabla 9, con las que entra a la caída — R3-2. Null si quedó igual que la fase.</param>
/// <param name="CalibreDeLaImpedancia">El calibre del que salieron esas R y X: el del neutro o, si la Tabla 9 no lo trae, el menor más cercano con datos.</param>
public sealed record NeutroDe220_61(char Fase, decimal DesbalanceA, decimal CorrienteA, Calibre Calibre, bool Con70Pct,
    PowerNode.DesignSuite.Calculo.TablasNom.ImpedanciaConductor? Impedancia = null, Calibre? CalibreDeLaImpedancia = null);

public sealed record RenglonDelAlimentador(
    ResultadoAlimentador? Resultado,
    string? Error,
    IReadOnlyList<string> Avisos,
    int Polos,
    IReadOnlyList<CorrienteDeFase>? Fases = null,
    CorrienteDeFase? Gobierna = null,
    decimal FactorPotencia = 1m);

/// <summary>
/// <b>El cuadro de carga completo de un tablero</b>: su cabecera, sus espacios y lo que sale de
/// calcularlos — el reparto por fases, el resumen de carga, el alimentador y el interruptor
/// principal.
///
/// <para>
/// <b>Reproduce la hoja «Cuadro de Carga» del Excel</b>, no una versión propia: los renglones de
/// espacios nones a la izquierda y pares a la derecha, el balanceo por fase en VA, el desbalanceo
/// en el pie, y el alimentador calculado con las mismas fórmulas que un renglón cualquiera. Lo que
/// cambia es de dónde salen los números: ahí eran <c>VLOOKUP</c> contra un libro externo que se
/// perdió, y aquí es el motor copiado de la versión de escritorio con las tablas de la NOM.
/// </para>
///
/// <para>
/// <b>Ninguna regla de la norma se escribe en esta clase.</b> Reparte, suma y arma las entradas;
/// quien decide protección, calibre, caída y tierra es <c>Calculo</c>.
/// </para>
/// </summary>
public sealed class CuadroDeCarga
{
    private readonly List<CircuitoDelCuadro> _circuitos = [];
    private readonly MotorNom _motor;

    /// <summary>Nace ya cuadriculado: los espacios existen desde el primer momento, vacíos.</summary>
    public CuadroDeCarga(MotorNom motor)
    {
        _motor = motor;
        Recalcular();
    }

    public DatosDelTablero Datos { get; } = new();

    /// <summary>
    /// En la captura, un circuito con carga no calcula hasta que se elige su tipo (I-111). Fuera de ella
    /// —pruebas, lo que no sea la pantalla— el tipo por omisión sigue siendo Alumbrado.
    /// </summary>
    public bool ExigirTipo { get; set; }

    public const string MensajeSinTipo = "Elegir el tipo de carga: de él salen el factor de demanda y el cálculo — Art. 220.";

    /// <summary>P2-1: en 3F-3H no hay neutro; la carga va entre fases.</summary>
    public const string MensajeUnPoloSinNeutro =
        "3F-3H (delta) no tiene neutro: un circuito de 1 polo no tiene conductor de regreso. La carga va entre fases: " +
        "cambia el circuito a 2 o 3 polos.";

    /// <summary>Un renglón por espacio del tablero, del 1 al <see cref="DatosDelTablero.NumeroEspacios"/>.</summary>
    public IReadOnlyList<CircuitoDelCuadro> Circuitos => _circuitos;

    public IEnumerable<CircuitoDelCuadro> Nones => _circuitos.Where(c => c.Espacio % 2 == 1);
    public IEnumerable<CircuitoDelCuadro> Pares => _circuitos.Where(c => c.Espacio % 2 == 0);

    /// <summary>Una sola barra: 1F-2H.</summary>
    public bool UnaSolaBarra => Datos.Barras.Count == 1;

    /// <summary>
    /// Cómo se listan los renglones del cuadro: nones arriba y pares abajo, como el Excel, que es lo
    /// que dice qué columna del gabinete es cada uno. Con una sola barra esa división no dice nada:
    /// van en orden, 1, 2, 3… (David, 2026-09-25 — I-54).
    /// </summary>
    public IReadOnlyList<IReadOnlyList<CircuitoDelCuadro>> Lados =>
        UnaSolaBarra ? [[.. _circuitos]] : [[.. Nones], [.. Pares]];

    public ResumenDeCarga Resumen { get; private set; } = Vacio();

    /// <summary>Los renglones de la Tabla 220-12, para elegir el uso del local — M-14.</summary>
    public IReadOnlyList<(string Inmueble, decimal VaPorM2)> FilasTabla220_12 => _motor.CargaUnitaria.Filas;

    public RenglonDelAlimentador Alimentador { get; private set; } = new(null, null, [], 3);

    /// <summary>
    /// Con el tablero como equipo de acometida: el conductor del electrodo de puesta a tierra (Tabla 250-66)
    /// y el puente de unión principal (250-28(d)(1)), por los conductores del alimentador, que son los de
    /// entrada a la acometida. <c>null</c> si no es equipo de acometida o el alimentador no calculó
    /// (auditoría del 2026-09-29, P3-3).
    /// </summary>
    public ResultadoTierraDeAcometida? TierraDeAcometida =>
        Alimentador.Resultado is { } a ? TierraDeAcometidaDe(a) : null;

    private ResultadoTierraDeAcometida? TierraDeAcometidaDe(ResultadoAlimentador a) =>
        Datos.EsEquipoDeAcometida
            ? PuestaTierraDeAcometida.Calcular(_motor.ElectrodoTierra, _motor.Calibres, a.CalibreFase, a.NumeroConductoresParalelo, Datos.MaterialConductor)
            : null;

    /// <summary>
    /// El sistema trae neutro: todos menos 3F-3H (delta). Sin él, ni el alimentador ni un derivado
    /// llevan neutro, y los documentos ponen «—» en su lugar — I-73.
    /// </summary>
    public bool SistemaConNeutro => Configuracion != ConfiguracionTablero.TresFasesTresHilos;

    /// <summary>
    /// El interior del tablero dibujado: un bloque por interruptor, con su renglón, su columna y
    /// cuántos espacios abarca. Nones a la izquierda y pares a la derecha — <b>la geometría no se
    /// decide aquí</b>, sale de <see cref="DistribucionBarras"/>.
    /// </summary>
    public IReadOnlyList<BloqueDelGabinete> Gabinete { get; private set; } = [];

    /// <summary>Espacios con un interruptor con carga capturada. Un multipolar cuenta por sus polos.</summary>
    public int EspaciosOcupados => Gabinete.Where(b => b.Ocupado).Sum(b => b.Espacios);

    public int EspaciosLibres => Datos.NumeroEspacios - EspaciosOcupados;

    /// <summary>
    /// Los espacios que se come el interruptor principal montado en espacios, en orden. Vacío con
    /// zócalo, con zapatas o si no se pudo montar (<see cref="AvisoDelPrincipal"/>).
    /// </summary>
    public IReadOnlyList<int> EspaciosDelPrincipal { get; private set; } = [];

    /// <summary>Por qué el principal no se montó en sus espacios, ya redactado. <c>null</c> = montado o no aplica.</summary>
    public string? AvisoDelPrincipal { get; private set; }

    /// <summary>
    /// El primer espacio del principal en espacios: el capturado si cabe; si no, el de omisión. Con
    /// dos o tres barras, la regla del escritorio (<see cref="AcomodoEnGabinete.UltimoHuecoDeLaColumnaPar"/>):
    /// los últimos pares, y si ahí hay un circuito, sube por la misma columna. Con una barra, el primer
    /// espacio libre desde el 1 (David, 2026-09-25).
    /// </summary>
    public int EspacioInicialDelPrincipal { get; private set; }

    /// <summary>El interruptor principal del tablero, en amperes. 0 mientras no haya carga capturada.</summary>
    public decimal InterruptorPrincipalA => Alimentador.Resultado?.ProteccionA ?? 0m;

    /// <summary>
    /// Ajusta la lista de espacios a <see cref="DatosDelTablero.NumeroEspacios"/> y recalcula. Los
    /// renglones que ya existían se conservan tal cual: cambiar el gabinete no debe borrar lo
    /// capturado.
    /// </summary>
    public void Recalcular()
    {
        AjustarEspacios();
        foreach (var c in _circuitos)
            c.SinTipo = false; // lo de antes no cuenta: se vuelve a marcar después de convertir las cargas
        ResolverOcupacion();
        ConvertirCargas();
        MarcarSinTipo();
        ResolverFases();
        PrepararCanalizaciones();
        CalcularCircuitos();
        ResolverNoSimultaneos();
        DibujarGabinete();
        CalcularResumen();
        CalcularAlimentador();
        DimensionarCanalizaciones();
        EvaluarCaidaCombinada();
        AbreviarErrorDeAislamiento();
    }

    /// <summary>
    /// El aislamiento es de la tabla pero no vale en el lugar capturado (THHN en lugar mojado): lo
    /// dice una vez, junto a Aislamiento y Lugar — I-95. Antes cada renglón decía que «no se
    /// reconoce» y en la misma frase lo listaba entre los reconocidos.
    /// </summary>
    public string? AvisoAislamientoDelLugar
    {
        get
        {
            var tabla = _motor.Aislamiento;
            var tipo = Datos.TipoAislamiento;
            var lugares = Enum.GetValues<LugarDeInstalacion>();
            if (tabla.TemperaturaMaxima(tipo, Datos.Lugar) is not null || lugares.All(l => tabla.TemperaturaMaxima(tipo, l) is null))
                return null;

            var donde = lugares.Where(l => tabla.TemperaturaMaxima(tipo, l) is not null).Select(l => l.Nombre()).ToList();
            var validos = tabla.DesignacionesReconocidas.Where(x => tabla.TemperaturaMaxima(x, Datos.Lugar) is not null).ToList();
            return $"{tipo} no se permite en lugar {Datos.Lugar.Nombre()}, solo en {Enumerar(donde)} — " +
                   $"Tabla 310-104(a), {(Datos.Lugar == LugarDeInstalacion.Mojado ? "310-10(c)" : "310-10(b)")}: ningún circuito calcula. " +
                   $"Cambiar el lugar o el aislamiento; en lugar {Datos.Lugar.Nombre()}: {string.Join(", ", validos)}.";
        }
    }

    private void AbreviarErrorDeAislamiento()
    {
        if (AvisoAislamientoDelLugar is null)
            return;

        var largo = $"'{Datos.TipoAislamiento}' no se reconoce";
        var corto = $"{Datos.TipoAislamiento} no vale en lugar {Datos.Lugar.Nombre()}: ver el aviso de Condiciones de cálculo.";
        foreach (var c in Circuitos.Where(c => c.Error?.StartsWith(largo, StringComparison.Ordinal) == true))
            c.Error = corto;
        if (Alimentador.Error?.StartsWith(largo, StringComparison.Ordinal) == true)
            Alimentador = Alimentador with { Error = corto };
    }

    /// <summary>
    /// Las canalizaciones con sus resultados: las de los derivados (T1, T2…) y al final la del
    /// alimentador.
    /// </summary>
    public IEnumerable<CanalizacionDelTablero> TodasLasCanalizaciones =>
        Datos.Canalizaciones.Append(Datos.CanalizacionAlimentador);

    // ---- Diámetro del fabricante (Capítulo 10, Nota 5) ----------------------------------------------
    // El aislamiento es uno para todo el tablero, así que el dato que falta va con él, en
    // «Condiciones de cálculo», y no escondido al pie de la tarjeta de canalizaciones (David,
    // 2026-09-28).

    /// <summary>
    /// El aislamiento del tablero no está en la Tabla 5 con ningún calibre: THHW-LS, THW-LS, USE,
    /// USE-2. THHW sí está, del 14 AWG al 2000 kcmil.
    /// </summary>
    public bool AislamientoFueraDeTabla5 =>
        _motor.Calibres.Listar().All(c => _motor.Dimensiones.Aislado(c.Designacion, Datos.TipoAislamiento) is null);

    /// <summary>
    /// Los calibres aislados que van en alguna canalización y piden el diámetro exterior del
    /// fabricante: los que la Tabla 5 no trae con el aislamiento del tablero (THHN en 1250 kcmil o
    /// más; todos en un LS) y los que ya traen uno capturado, que manda sobre la Tabla 5 y así se ve.
    /// De menor a mayor.
    /// </summary>
    public IReadOnlyList<string> CalibresConDiametroDelFabricante =>
        [.. TodasLasCanalizaciones
            .SelectMany(t => t.Ocupacion?.Renglones ?? [])
            .Where(r => r.Conductor.TipoAislamiento is not null)
            .Select(r => r.Conductor.Designacion)
            .Distinct()
            .Where(d => _motor.Dimensiones.Aislado(d, Datos.TipoAislamiento) is null
                        || Datos.DiametrosFabricante.ContainsKey(DatosDelTablero.ClaveDiametro(Datos.TipoAislamiento, d)))
            .OrderBy(d => _motor.Calibres.BuscarPorDesignacion(d)?.AreaMm2 ?? 0m)];

    /// <summary>
    /// El aviso de «Condiciones de cálculo» cuando el llenado de las canalizaciones pide el diámetro
    /// del fabricante — Capítulo 10, Nota 5. <c>null</c> si todo sale de la Tabla 5.
    /// </summary>
    public string? AvisoDiametroDelFabricante
    {
        get
        {
            var tipo = Datos.TipoAislamiento;
            var calibres = CalibresConDiametroDelFabricante;
            if (AislamientoFueraDeTabla5)
                return $"{tipo} no está en la Tabla 5 del Capítulo 10: las canalizaciones se calculan con el diámetro exterior del conductor que da su fabricante — Nota 5. "
                       + (calibres.Count == 0 ? "Los calibres aparecen aquí al capturar la carga de algún circuito." : "Capturar el de cada calibre.");

            var sinTabla = calibres.Where(d => _motor.Dimensiones.Aislado(d, tipo) is null).ToList();
            if (sinTabla.Count > 0)
            {
                var nombres = sinTabla.Select(Calibre.UnidadDe).ToList();
                var lista = nombres.Count == 1 ? nombres[0] : $"{string.Join(", ", nombres[..^1])} y {nombres[^1]}";
                return $"La Tabla 5 del Capítulo 10 no trae {tipo} en {lista}: {(sinTabla.Count == 1 ? "ese calibre se calcula" : "esos calibres se calculan")} con el diámetro exterior del conductor que da su fabricante — Nota 5.";
            }

            return calibres.Count > 0
                ? $"Hay diámetros del fabricante capturados para {tipo}: mandan sobre la Tabla 5 del Capítulo 10 — Nota 5. Borrado, el calibre regresa a la tabla."
                : null;
        }
    }

    /// <summary>Los avisos de todas las canalizaciones, con la canalización al frente.</summary>
    public IEnumerable<string> AvisosDeCanalizaciones =>
        TodasLasCanalizaciones.SelectMany(t => t.Avisos.Select(a => $"{NombreDe(t)}: {a}"));

    /// <summary>
    /// «Canalización T1», «Canalización del alimentador», con el nombre que le haya puesto el
    /// ingeniero: «Canalización Tubo pasillo».
    /// </summary>
    public static string NombreDe(CanalizacionDelTablero t) =>
        !t.EsAlimentador ? $"Canalización {t.Nombre}"
        : t.TieneNombre ? $"Canalización {t.Nombre} (alimentador)"
        : "Canalización del alimentador";

    /// <summary>Quita una canalización; cada uno de sus circuitos recibe un tubo nuevo.</summary>
    public void QuitarCanalizacion(CanalizacionDelTablero canalizacion)
    {
        Datos.Canalizaciones.Remove(canalizacion);
        foreach (var c in _circuitos.Where(c => c.Canalizacion == canalizacion.Id))
            c.Canalizacion = null;
        Recalcular();
    }

    /// <summary>
    /// Por qué salieron la protección y el calibre de este circuito, paso por paso. <c>null</c> si el
    /// renglón no calculó.
    /// </summary>
    /// <summary>
    /// El desglose de cada renglón se pide en cada dibujo de la pantalla; se arma una vez por cálculo
    /// (I-97). Se vacía al calcular los circuitos.
    /// </summary>
    private readonly Dictionary<CircuitoDelCuadro, DesgloseDeSeleccion?> _desgloses = [];

    public DesgloseDeSeleccion? Desglose(CircuitoDelCuadro c)
    {
        if (!_desgloses.TryGetValue(c, out var desglose))
            _desgloses[c] = desglose = ArmarDesglose(c);
        return desglose;
    }

    private DesgloseDeSeleccion? ArmarDesglose(CircuitoDelCuadro c)
    {
        if (c.Resultado is not { Detalle: { } detalle } r)
            return null;

        if (r.Grupo is { } grupo)
        {
            var divisorGrupo = TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
            var tension = TensionDelMotor(c);
            var capacidad = r.Citas.First(x => x.Referencia is "430-24" or "440-32" or "440-33" or "440-34");
            return DesgloseDeSeleccion.DeGrupo(
                _motor.Ampacidad, Datos, grupo,
                maquinas: [.. c.Cargas.Where(a => a.EsMaquina && a.CorrienteUnitariaA > 0m).Select(a =>
                    $"{NombreDeMaquina(c, a)}: {(a.Cantidad > 1 ? $"{a.Cantidad} × " : "")}{a.CorrienteUnitariaA:N2} A — {OrigenDeLaCorriente(a, c.Polos, tension)}")],
                otrasContinuaA: c.ContinuaVA / divisorGrupo,
                otrasNoContinuaA: c.NoContinuaVA / divisorGrupo,
                capacidad: capacidad.Descripcion[(capacidad.Descripcion.IndexOf(':') + 2)..],
                articuloCapacidad: capacidad.Referencia,
                proteccionA: r.ProteccionA,
                calibre: r.CalibreFase,
                conductoresPorFase: r.NumeroConductoresParalelo,
                d: detalle,
                citas: r.Citas,
                sobrecarga: c.Sobrecarga);
        }

        if (c.EsVariador)
            return DesgloseDeSeleccion.DeVariador(
                _motor.Ampacidad, Datos, c.CorrienteEntradaVariadorA, c.ProteccionMaximaVariadorA,
                r.ProteccionA, r.CalibreFase, r.NumeroConductoresParalelo, detalle, r.Citas,
                rango: r.Rango, criterio: CriterioDeLaProteccion(c), sobrecarga: c.Sobrecarga,
                bypass: (c.EntradaDeLaProteccion as DatosEntradaCircuitoDerivadoVariador)?.Bypass);

        if (c.EsMotor)
            return DesgloseDeSeleccion.DeMotor(
                _motor.Ampacidad, Datos,
                origenFlc: OrigenDeLaFlc(c),
                flcA: c.FlcA,
                porcentaje: PorcentajeProteccionMotor(c),
                seleccion: r.Rango?.Tabla430_52 ?? ProteccionDelMotor(c),
                calibre: r.CalibreFase,
                conductoresPorFase: r.NumeroConductoresParalelo,
                d: detalle,
                citas: r.Citas,
                servicio: r.Citas.FirstOrDefault(x => x.Referencia == "430-22(e)")?.Descripcion,
                rango: r.Rango,
                criterio: CriterioDeLaProteccion(c),
                sobrecarga: c.Sobrecarga);

        if (c.EsAireAcondicionado)
            return DesgloseDeSeleccion.DeAireAcondicionado(
                _motor.Ampacidad, Datos, c,
                proteccionA: r.ProteccionA,
                calibre: r.CalibreFase,
                conductoresPorFase: r.NumeroConductoresParalelo,
                d: detalle,
                citas: r.Citas,
                rango: r.Rango,
                criterio: CriterioDeLaProteccion(c),
                sobrecarga: c.Sobrecarga);

        var divisor = TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
        // Otro tablero se cita con el 215 — I-125.
        var tablero = c.Categoria == CategoriaDeCarga.Tablero;
        var articuloProteccion = tablero ? "215-3" : "210-20(a)";
        var articuloConductor = tablero ? "215-2(a)(1)" : "210-19(a)(1)";
        var factor = CargaContinua100Pct.Para(Datos.ConjuntoAprobado100Pct, null, articuloProteccion, articuloConductor).Factor;

        var desglose = DesgloseDeSeleccion.De(
            _motor.Ampacidad, Datos,
            iContinuaA: c.ContinuaVA / divisor,
            iNoContinuaA: c.NoContinuaVA / divisor,
            factorContinua: factor,
            articuloProteccion: articuloProteccion,
            articuloConductor: articuloConductor,
            proteccionA: r.ProteccionA,
            proteccionSinMinimo: TamanoEstandar(detalle.CapacidadMinimaA),
            calibre: r.CalibreFase,
            conductoresPorFase: r.NumeroConductoresParalelo,
            d: detalle,
            citas: r.Citas,
            referenciaMinimo: ProteccionMinima(c)?.Referencia);

        if (NotaDelMotorMayor(c) is { } motor)
            desglose = desglose with { Proteccion = [desglose.Proteccion[0], motor, .. desglose.Proteccion.Skip(1)] };
        if (c.AvisoAireDeHabitacion is { } habitacion)
            desglose = desglose with { Proteccion = [.. desglose.Proteccion, $"⚠ {habitacion[(habitacion.IndexOf(':') + 2)..]}"] };
        // La clase del circuito y sus reglas — I-124.
        if (c.ReglasDeClase.Count > 0)
            desglose = desglose with { Proteccion = [.. desglose.Proteccion, .. c.ReglasDeClase.Select(x => $"{(x.Aviso ? "⚠ " : "")}{x.Texto} — {x.Referencia}")] };
        // I-76: el circuito individual del refrigerador no tiene mínimo que citar, pero sí su excepción.
        return c.UsoEfectivo.Nota() is { } nota ? desglose with { Proteccion = [.. desglose.Proteccion, nota] } : desglose;
    }

    /// <summary>
    /// De dónde sale la FLC de un motor, ya redactado: «FLC = 15.20 A, 5 HP — Tabla 430-250, columna de
    /// 230 V (…), 430-6(a)», o en amperes, «FLC = 12.00 A: motor marcado en amperes, 3.93 HP — Tabla
    /// 430-250: 3 HP, 9.60 A; 5 HP, 15.20 A, 430-6(a)(1)».
    /// </summary>
    public string OrigenDeLaFlc(CircuitoDelCuadro c) =>
        c.EsGrupo
            // Un grupo de un solo motor se calcula como motor: la FLC es la de ese motor (I-115).
            ? c.Cargas.FirstOrDefault(a => a.EsMaquina && a.CorrienteUnitariaA > 0m) is { } a
                ? $"FLC = {a.CorrienteUnitariaA:N2} A, {NombreDeMaquina(c, a)}: {OrigenDeLaCorriente(a, c.Polos, TensionDelMotor(c))}" +
                  (a.MotorEnAmperes is null ? ", 430-6(a)" : "")
                : ""
        : c.MotorEnAmperes is { } m
            ? $"FLC = {c.FlcA:N2} A: motor marcado en amperes, {m.Hp:0.##} HP — {MotoresEnHp.Interpolacion(m, c.Polos)}, 430-6(a)(1)"
            : $"FLC = {c.FlcA:N2} A, {MotoresEnHp.Texto(c.Hp ?? 0m)} HP — {FuenteDeFlc(c)}, 430-6(a)";

    /// <summary>Los HP que la tabla trae para los polos y la tensión de este circuito — I-15. Los ofrece el selector.</summary>
    public IReadOnlyList<decimal> HpDisponibles(CircuitoDelCuadro c) => HpDisponibles(c.Polos);

    /// <summary>Los HP que la tabla trae con estos polos, a la tensión del tablero: el selector ofrece todos, por alimentación — I-114.</summary>
    public IReadOnlyList<decimal> HpDisponibles(int polos) =>
        MotoresEnHp.Disponibles(_motor.FlcMotor, polos, MotoresEnHp.Tension(polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV));

    /// <summary>
    /// «Monofásico 127 V · Tabla 430-248»: la alimentación de un motor con estos polos y la tabla de su FLC — I-114.
    /// Si la tabla no tiene la columna de esa tensión, la que se lee: «Trifásico 600 V · Tabla 430-250, columna de
    /// 575 V» (I-196).
    /// </summary>
    public string AlimentacionDelMotor(int polos)
    {
        var tension = TensionDelMotorV(polos);
        var columna = PowerNode.DesignSuite.Normativa.TablaFlcMotorJson.TensionDeColumna(MotoresEnHp.Alimentacion(polos), tension);
        return $"{MotoresEnHp.AlimentacionTexto(polos, tension)} · Tabla {MotoresEnHp.Tabla(polos)}" +
               (columna is { } col && col != (int)tension ? $", columna de {col} V" : "");
    }

    /// <summary>La tensión de un motor con estos polos, en volts enteros: F-N en 1 polo, F-F en 2 y 3 — I-190.</summary>
    public decimal TensionDelMotorV(int polos) => MotoresEnHp.Tension(polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);

    /// <summary>La FLC de tabla de un motor de <paramref name="hp"/> en este circuito; 0 si la tabla no lo trae. La enseña el selector.</summary>
    public decimal FlcDe(CircuitoDelCuadro c, decimal hp) => FlcDe(hp, c.Polos);

    public decimal FlcDe(decimal hp, int polos) =>
        MotoresEnHp.Flc(_motor.FlcMotor, hp, polos, MotoresEnHp.Tension(polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV)) ?? 0m;

    /// <summary>«Tabla 430-250, columna de 230 V (220 V: intervalo de 220 a 240 V)»: de dónde sale la FLC de este circuito.</summary>
    public string FuenteDeFlc(CircuitoDelCuadro c) => MotoresEnHp.Fuente(c.Polos, TensionDelMotor(c));

    /// <summary>
    /// El porcentaje de la FLC que puede tener la protección del derivado de un motor — Tabla 430-52,
    /// interruptor automático de tiempo inverso: 250 % en monofásicos y jaula de ardilla.
    /// </summary>
    public decimal PorcentajeProteccionMotor(CircuitoDelCuadro c) =>
        _motor.ProteccionMotor.PorcentajeMaximo(MotoresEnHp.TipoDeMotor(c.Polos), TipoDispositivoProteccionMotor.InterruptorTiempoInverso);

    /// <summary>
    /// <b>La protección que daría otro criterio</b> — M-20: la calculada (para decir cuál es cuando hay una
    /// fijada) y, al abrir un archivo de formato 12, la de «cond.» o «máx.» (I-182). <c>null</c> si el circuito
    /// no tiene rango calculado, o con <see cref="CriterioDeProteccion.Manual"/>.
    /// </summary>
    public decimal? ProteccionConCriterio(CircuitoDelCuadro c, CriterioDeProteccion criterio)
    {
        if (criterio == CriterioDeProteccion.Manual)
            return null;
        try
        {
            return c.EntradaDeLaProteccion switch
            {
                DatosEntradaCircuitoDerivadoMotor m => Recordado(m with
                {
                    CriterioProteccion = criterio.ParaElCalculo(m.Hp, c.NoArrancaConLaTabla),
                    ProteccionElegidaA = null,
                }).ProteccionA,
                DatosEntradaCircuitoDerivado440 a => Recordado(a with { CriterioProteccion = criterio.ParaElCalculo(), ProteccionElegidaA = null }).ProteccionA,
                DatosEntradaCircuitoDerivadoVariador v => Recordado(v with { CriterioProteccion = criterio.ParaElCalculo(), ProteccionElegidaA = null }).ProteccionA,
                _ => null,
            };
        }
        catch (Exception e) when (e is InvalidOperationException or ArgumentException)
        {
            return null;
        }
    }

    /// <summary>La protección calculada del circuito, aunque tenga una fijada — I-182.</summary>
    public decimal? ProteccionCalculada(CircuitoDelCuadro c) =>
        c.CriterioProteccion == CriterioDeProteccion.Automatico && c.Resultado?.Rango is not null
            ? c.Resultado.ProteccionA
            : ProteccionConCriterio(c, CriterioDeProteccion.Automatico);

    /// <summary>La protección quedó fijada por el proyectista, dentro de su rango — I-182.</summary>
    public static bool ProteccionFijada(CircuitoDelCuadro c) =>
        c.CriterioProteccion == CriterioDeProteccion.Manual && c.Resultado?.Rango is not null;

    /// <summary>
    /// <b>Escoger la protección en la celda</b> — I-182: un valor del rango la fija y guarda la huella del
    /// rango; el valor que da el cálculo, o <c>null</c>, la regresa al cálculo. Un valor fuera del rango no
    /// se acepta (la lista solo trae los del rango). Recalcula.
    /// </summary>
    public void FijarProteccion(CircuitoDelCuadro c, decimal? amperes)
    {
        if (c.Resultado?.Rango is not { } rango)
            return;
        if (amperes is { } a && rango.Valores.Contains(a) && a != ProteccionCalculada(c))
        {
            c.CriterioProteccion = CriterioDeProteccion.Manual;
            c.ProteccionElegidaA = a;
            c.HuellaDelFijado = Huella(rango);
        }
        else
        {
            c.CriterioProteccion = CriterioDeProteccion.Automatico;
            c.ProteccionElegidaA = null;
            c.HuellaDelFijado = null;
        }
        Recalcular();
    }

    /// <summary>
    /// La huella de un rango: su regla y sus valores — I-182. Cambia con el equipo (la FLC, la placa), la
    /// Excepción 2 o la serie; no con el conductor.
    /// </summary>
    internal static string Huella(RangoDeProteccion r) =>
        $"{r.Regla}:{string.Join("|", r.Valores.Select(v => v.ToString(CultureInfo.InvariantCulture)))}";

    private readonly List<ProteccionQueRegreso> _regresaron = [];

    /// <summary>
    /// <b>Las protecciones fijadas que regresaron al cálculo</b> porque cambió su rango — I-182. La pantalla las
    /// toma para avisar una vez; tomarlas las borra.
    /// </summary>
    public IReadOnlyList<ProteccionQueRegreso> TomarProteccionesQueRegresaron()
    {
        var todas = _regresaron.ToList();
        _regresaron.Clear();
        return todas;
    }

    /// <summary>
    /// Después de calcular un circuito con su protección fijada — I-182: si viene de un archivo, toma la huella
    /// del rango que salió; si el rango ya no es aquel con que se fijó, la regresa al cálculo y lo anota.
    /// <c>true</c> si hay que volver a calcular ese circuito; ya en calculada no puede volver a pasar.
    /// </summary>
    private bool RegresarSiCambioElRango(CircuitoDelCuadro c)
    {
        if (c.CriterioProteccion != CriterioDeProteccion.Manual || c.Resultado?.Rango is not { } rango)
            return false;
        var huella = Huella(rango);
        if (c.HuellaDelFijado is null)
        {
            c.HuellaDelFijado = huella;
            return false;
        }
        if (c.HuellaDelFijado == huella)
            return false;
        _regresaron.Add(new ProteccionQueRegreso(c.Espacio, c.ProteccionElegidaA ?? rango.ProteccionA, rango.MinimoA, rango.MaximoA));
        c.CriterioProteccion = CriterioDeProteccion.Automatico;
        c.ProteccionElegidaA = null;
        c.HuellaDelFijado = null;
        return true;
    }

    /// <summary>
    /// <b>Cómo quedó la protección dentro de su rango, y por qué</b> — M-20, I-182, para el desglose y la
    /// memoria: «Calculada: 15 A, el mayor que protege a 14 AWG (15 A) — 240-4; criterio para motores de 1 HP
    /// o menos», o «Fijada por el proyectista: 20 A, dentro del rango». <c>null</c> si el circuito no tiene rango.
    /// </summary>
    public string? CriterioDeLaProteccion(CircuitoDelCuadro c) =>
        PorQueLaProteccion(c) is { } porque
            ? $"{(ProteccionFijada(c) ? "Fijada por el proyectista" : "Calculada")}: {c.Resultado!.ProteccionA:N0} A, {porque}"
            : null;

    /// <summary>
    /// El porqué de <see cref="CriterioDeLaProteccion"/>, sin el valor: «el mayor que protege a 14 AWG (15 A) —
    /// 240-4; criterio…» o, fijada, «dentro del rango». <c>null</c> sin rango.
    /// </summary>
    public string? PorQueLaProteccion(CircuitoDelCuadro c)
    {
        if (c.Resultado is not { Rango: { } r } resultado)
            return null;
        if (ProteccionFijada(c))
            return "dentro del rango";
        var calibre = r.CalibreProtegido is { } protegido
            ? $"{protegido.DesignacionConUnidad} ({r.AmpacidadProtegidaA:0.##} A)" +
              (protegido.Designacion != resultado.CalibreFase.Designacion ? $", el calibre por ampacidad; {resultado.CalibreFase.DesignacionConUnidad} por caída de tensión sigue protegido" : "")
            : $"{resultado.CalibreFase.DesignacionConUnidad} ({resultado.Detalle?.AmpacidadConductorA ?? 0m:0.##} A)";
        return CriteriosDeProteccion.PorQueLaCalculada(r, calibre, c.EntradaDeLaProteccion is DatosEntradaCircuitoDerivadoMotor, c.NoArrancaConLaTabla);
    }

    /// <summary>
    /// <b>La ayuda de la celda «Protec. (A)»</b> — I-182: el rango y por qué se puede escoger, la calculada y
    /// su porqué, o la fijada y cuál es la calculada; qué verificar abajo del máximo. En palabras llanas, sin
    /// «auto», «cond.» ni «máx.». <c>null</c> sin rango.
    /// </summary>
    public string? AyudaDeLaProteccion(CircuitoDelCuadro c)
    {
        if (c.Resultado is not { Rango: { } r } resultado)
            return null;
        var lineas = new List<string>();
        var calculada = ProteccionCalculada(c);
        if (ProteccionFijada(c))
            lineas.Add($"Fijada: {resultado.ProteccionA:N0} A." + (calculada is { } calc
                ? $" El cálculo da {calc:N0} A; escogerlo en la lista lo regresa al cálculo."
                : ""));
        lineas.Add(r.Valores.Count == 1
            ? $"Solo se permite {r.MaximoA:N0} A — {r.Regla}."
            : $"Se permite de {r.MinimoA:N0} a {r.MaximoA:N0} A: {r.Regla} solo pone el techo.");
        if (!ProteccionFijada(c) && PorQueLaProteccion(c) is { } porque)
            lineas.Add($"Calculada: {resultado.ProteccionA:N0} A, {porque}.");
        if (r.Valores.Count > 1)
            lineas.Add($"{r.MaximoA:N0} A es el máximo {CriteriosDeProteccion.DeDondeElMaximo(r)}.");
        if (r.Arranque is { } arranque)
            lineas.Add($"{arranque.Descripcion.TrimEnd('.')} — {arranque.Referencia}.");
        if (!ProteccionFijada(c) && r.Valores.Count > 1)
            lineas.Add("Escoger otro valor para fijarlo.");
        return string.Join("\n", lineas);
    }

    /// <summary>
    /// La protección del derivado de un motor: techo de la Tabla 430-52, máximo permitido por
    /// 430-52(c)(1) y su Excepción 1 contra la lista de 240-6(a), y el tamaño de la serie — P1-1.
    /// </summary>
    public ProteccionDeMotor ProteccionDelMotor(CircuitoDelCuadro c) =>
        CalculadoraCircuitoDerivadoMotor.ProteccionDeLaTabla430_52(
            new ProteccionEstandarDeLaSerie(_motor.ProteccionEstandar, Datos.SerieInterruptores),
            c.FlcA * PorcentajeProteccionMotor(c) / 100m,
            c.NoArrancaConLaTabla ? c.FlcA : null);

    /// <summary>Hay equipos de A/C en el tablero: el grupo de motores del alimentador cita también 440-33.</summary>
    public bool TieneAireAcondicionado => _circuitos.Any(c => c.TieneCarga && c.EsAireAcondicionado);

    /// <summary>«Motores», o «Motores y A/C» con equipos de A/C: el grupo de 430-24 / 440-33.</summary>
    public string EtiquetaDeMotores => TieneAireAcondicionado ? "Motores y A/C" : "Motores";

    /// <summary>«430-24», o «430-24, 440-33» con equipos de A/C.</summary>
    public string ReferenciaDeMotores => TieneAireAcondicionado ? "430-24, 440-33" : "430-24";

    /// <summary>El desglose del alimentador, con la corriente de la fase que gobierna. <c>null</c> sin cálculo.</summary>
    public DesgloseDeSeleccion? DesgloseDelAlimentador()
    {
        if (Alimentador is not { Resultado: { Detalle: { } detalle } r, Gobierna: { } g })
            return null;

        return DesgloseDeSeleccion.De(
            _motor.Ampacidad, Datos,
            iContinuaA: g.ContinuaA,
            iNoContinuaA: g.NoContinuaA,
            factorContinua: g.FactorContinua,
            articuloProteccion: "215-3",
            articuloConductor: "215-2(a)(1)",
            proteccionA: r.ProteccionA,
            proteccionSinMinimo: TamanoEstandar(detalle.CapacidadMinimaA),
            calibre: r.CalibreFase,
            conductoresPorFase: r.NumeroConductoresParalelo,
            d: detalle,
            citas: r.Citas,
            referenciaMinimo: Datos.Minimo230_79?.Referencia,
            motores: g.Motores);
    }

    private decimal TamanoEstandar(decimal amperes) =>
        new ProteccionEstandarDeLaSerie(_motor.ProteccionEstandar, Datos.SerieInterruptores).SiguienteEstandar(amperes);

    /// <summary>
    /// Cambia los polos de un interruptor. Devuelve <b>el motivo por el que no se pudo</b>, ya
    /// redactado, o <c>null</c> si se aplicó — mismo criterio que el editor de gabinete de
    /// escritorio: soltar sin explicación se lee como que el programa se trabó.
    /// </summary>
    public string? CambiarPolos(CircuitoDelCuadro circuito, int polos)
    {
        if (!DistribucionBarras.PolosValidos(polos, Datos.Sistema))
            return $"Este tablero tiene {Datos.Barras.Count} barra(s), así que un interruptor de {polos} polos repetiría fase.";

        // Los mismos que al mover (I-69): todo lo capturado, también de 1 polo. Antes solo los
        // multipolares: el 3 pasaba a 3 polos y se comía el 7 con su carga — I-79.
        var ocupados = MontajesCapturados(excepto: circuito);
        if (EspaciosDelPrincipal.Count > 0)
            ocupados.Add(new MontajeEnGabinete(EspaciosDelPrincipal[0], EspaciosDelPrincipal.Count, "el interruptor principal"));

        var motivo = AcomodoEnGabinete.MotivoNoCabe(circuito.Espacio, polos, Datos.NumeroEspacios, ocupados);
        if (motivo is not null)
            return motivo;

        circuito.Polos = circuito.PolosElegidos = polos;
        Recalcular();
        return null;
    }

    /// <summary>
    /// Los circuitos con captura que no caben en un gabinete de <paramref name="espacios"/>: al
    /// reducirlo se borran — I-78. Un multipolar que empieza adentro no está aquí: se recorta.
    /// </summary>
    public IReadOnlyList<CircuitoDelCuadro> QuedanFuera(int espacios) =>
        [.. _circuitos.Where(c => c.Espacio > espacios && !c.EsContinuacion && !c.EsDelPrincipal && c.TieneCaptura)];

    /// <summary>
    /// Los multipolares que se quedan en el gabinete pero con menos polos, con <paramref name="espacios"/>
    /// y hasta <paramref name="maximoPolos"/> — I-83: un motor trifásico pasaba a 1 polo sin aviso.
    /// </summary>
    public IReadOnlyList<(CircuitoDelCuadro Circuito, int Quedan)> SeRecortan(int espacios, int maximoPolos) =>
        [.. _circuitos
            .Where(c => c.Espacio <= espacios && !c.EsContinuacion && !c.EsDelPrincipal && c.Polos > 1)
            .Select(c =>
            {
                var quedan = Math.Min(c.Polos, maximoPolos);
                while (quedan > 1 && !DistribucionBarras.CabeEnElTablero(c.Espacio, quedan, espacios))
                    quedan--;
                return (Circuito: c, Quedan: quedan);
            })
            .Where(x => x.Quedan < x.Circuito.Polos)];

    /// <summary>
    /// Lo que pasa al gabinete con <paramref name="espacios"/> y hasta <paramref name="maximoPolos"/>,
    /// o <c>null</c> si nada: los circuitos que se borran (I-78) y los que pierden polos (I-83). La
    /// página lo pregunta antes. Con <paramref name="etiqueta"/>, el cambio es de fases o hilos: «1F-2H
    /// llega hasta 8 espacios: no caben los circuitos 9 y 13; se borran.»
    /// </summary>
    public string? AvisoAlCambiar(int espacios, int maximoPolos, string? etiqueta = null)
    {
        var partes = new List<string>();

        var fuera = QuedanFuera(espacios);
        if (fuera.Count > 0)
        {
            var lista = Lista(fuera.Select(Nombre).ToList());
            partes.Add((etiqueta, fuera.Count == 1) switch
            {
                (null, true) => $"Con {espacios} espacios no cabe el circuito {lista}: se borra.",
                (null, false) => $"Con {espacios} espacios no caben los circuitos {lista}: se borran.",
                (_, true) => $"{etiqueta} llega hasta {espacios} espacios: no cabe el circuito {lista}; se borra.",
                (_, false) => $"{etiqueta} llega hasta {espacios} espacios: no caben los circuitos {lista}; se borran.",
            });
        }

        var recortados = SeRecortan(espacios, maximoPolos);
        if (recortados.Count == 1)
        {
            var (c, quedan) = recortados[0];
            partes.Add($"El circuito {Nombre(c)} pasa de {c.Polos} a {Polos(quedan)}; al regresar recupera sus polos si hay lugar.");
        }
        else if (recortados.Count > 1)
            partes.Add($"Pierden polos: {string.Join("; ", recortados.Select(x => $"{Nombre(x.Circuito)}, de {x.Circuito.Polos} a {x.Quedan}"))}. Al regresar recuperan sus polos si hay lugar.");

        return partes.Count == 0 ? null : string.Join(" ", partes);

        static string Nombre(CircuitoDelCuadro c) =>
            string.IsNullOrWhiteSpace(c.Descripcion) ? $"{c.Espacio}" : $"{c.Espacio} ({c.Descripcion.Trim()})";

        static string Polos(int n) => n == 1 ? "1 polo" : $"{n} polos";

        static string Lista(List<string> nombres)
        {
            const int maximo = 8;
            if (nombres.Count > maximo)
                nombres = [.. nombres.Take(maximo), $"{nombres.Count - maximo} más"];
            return nombres.Count == 1 ? nombres[0] : $"{string.Join(", ", nombres[..^1])} y {nombres[^1]}";
        }
    }

    // ---- Adentro -------------------------------------------------------------------------------

    private void AjustarEspacios()
    {
        while (_circuitos.Count < Datos.NumeroEspacios)
            _circuitos.Add(new CircuitoDelCuadro(_circuitos.Count + 1));

        if (_circuitos.Count > Datos.NumeroEspacios)
            _circuitos.RemoveRange(Datos.NumeroEspacios, _circuitos.Count - Datos.NumeroEspacios);
    }

    /// <summary>
    /// Quién se come qué renglón. Se recorre de arriba abajo, así que <b>gana el interruptor que
    /// empieza antes</b>: es determinista y coincide con cómo se lee el tablero.
    /// </summary>
    private void ResolverOcupacion()
    {
        foreach (var c in _circuitos)
            c.ContinuacionDe = null;

        // I-83: solo al crecer el tablero —más fases o más espacios— se regresan los polos recortados.
        // En cualquier otro recálculo no: borrar un circuito no debe hacer crecer al de arriba.
        var crecio = Datos.MaximoPolos > _maximoPolosVisto || Datos.NumeroEspacios > _espaciosVistos;
        _maximoPolosVisto = Datos.MaximoPolos;
        _espaciosVistos = Datos.NumeroEspacios;

        foreach (var c in _circuitos)
        {
            if (c.EsContinuacion)
                continue;

            // Un tablero al que le bajaron las barras o los espacios deja interruptores que ya no
            // caben. Se recortan aquí, que es lo mismo que hace el editor de gabinete al reabrir un
            // tablero editado: mejor un interruptor de menos polos que uno colgado de una barra que
            // no existe.
            if (c.Polos > Datos.MaximoPolos)
                c.Polos = Datos.MaximoPolos;
            while (c.Polos > 1 && !DistribucionBarras.CabeEnElTablero(c.Espacio, c.Polos, Datos.NumeroEspacios))
                c.Polos--;

            // Y al crecer, regresan a los que eligió el ingeniero si el lugar está libre. Si no lo
            // está, se quedan así y se olvida: el ingeniero decide si los amplía.
            if (crecio && c.PolosElegidos > c.Polos)
            {
                var objetivo = Math.Min(c.PolosElegidos, Datos.MaximoPolos);
                while (c.Polos < objetivo && DistribucionBarras.CabeEnElTablero(c.Espacio, c.Polos + 1, Datos.NumeroEspacios)
                       && Libre(DistribucionBarras.EspaciosQueOcupa(c.Espacio, c.Polos + 1)[^1]))
                    c.Polos++;
                if (c.Polos < objetivo)
                    c.PolosElegidos = c.Polos;
            }

            foreach (var ocupado in DistribucionBarras.EspaciosQueOcupa(c.Espacio, c.Polos).Skip(1))
                _circuitos[ocupado - 1].ContinuacionDe = c.Espacio;
        }

        MontarPrincipal();

        bool Libre(int espacio) =>
            _circuitos[espacio - 1] is { EsContinuacion: false, TieneCaptura: false };
    }

    private int _maximoPolosVisto;
    private int _espaciosVistos;

    /// <summary>
    /// El interruptor principal en espacios se come los suyos — decisión
    /// <c>montaje-del-interruptor-principal.md</c>. <b>Nunca borra lo capturado</b>: si un circuito
    /// ya está ahí, el principal no se monta y se avisa, hasta que se mueva uno de los dos.
    /// </summary>
    private void MontarPrincipal()
    {
        foreach (var c in _circuitos)
            c.EsDelPrincipal = false;
        EspaciosDelPrincipal = [];
        AvisoDelPrincipal = null;
        if (!Datos.PrincipalEnEspacios)
            return;

        var polos = Datos.PolosDelPrincipal;
        var inicio = EspacioInicialDelPrincipal = ResolverEspacioDelPrincipal(polos);
        if (!DistribucionBarras.CabeEnElTablero(inicio, polos, Datos.NumeroEspacios))
        {
            AvisoDelPrincipal = $"El interruptor principal de {polos} polos no cabe en un tablero de {Datos.NumeroEspacios} espacios.";
            return;
        }

        var espacios = DistribucionBarras.EspaciosQueOcupa(inicio, polos);
        var choque = espacios
            .Select(e => _circuitos[e - 1])
            .Select(r => r.ContinuacionDe is { } dueno ? _circuitos[dueno - 1] : r)
            .Where(c => c.TieneCaptura)
            .Select(c => c.Espacio)
            .Distinct()
            .Order()
            .ToList();
        if (choque.Count > 0)
        {
            var quien = choque.Count == 1
                ? $"el circuito {choque[0]} ya está"
                : $"los circuitos {string.Join(", ", choque[..^1])} y {choque[^1]} ya están";
            AvisoDelPrincipal = $"El interruptor principal no se montó en los espacios {string.Join("-", espacios)}: {quien} ahí. Mover el principal o el circuito.";
            return;
        }

        foreach (var e in espacios)
        {
            var r = _circuitos[e - 1];
            r.EsDelPrincipal = true;
            r.ContinuacionDe = e == inicio ? null : inicio;
        }
        EspaciosDelPrincipal = espacios;
    }

    private int ResolverEspacioDelPrincipal(int polos)
    {
        if (Datos.EspacioDelPrincipal is { } elegido && Datos.EspaciosValidosDelPrincipal.Contains(elegido))
            return elegido;

        var capturados = MontajesCapturados(excepto: null);
        if (Datos.Barras.Count == 1)
            return Enumerable.Range(1, Datos.NumeroEspacios)
                .FirstOrDefault(e => AcomodoEnGabinete.MotivoNoCabe(e, polos, Datos.NumeroEspacios, capturados) is null, 1);

        var ultimoArranque = Datos.NumeroEspacios - 2 * (polos - 1);
        if (ultimoArranque % 2 == 1)
            ultimoArranque--;
        return AcomodoEnGabinete.UltimoHuecoDeLaColumnaPar(polos, Datos.NumeroEspacios, capturados)
            ?? Math.Max(2, ultimoArranque);
    }

    /// <summary>Los interruptores con algo capturado, como montajes: lo que un movimiento no puede pisar.</summary>
    private List<MontajeEnGabinete> MontajesCapturados(CircuitoDelCuadro? excepto) =>
        [.. _circuitos
            .Where(c => c != excepto && !c.EsContinuacion && !c.EsDelPrincipal && c.TieneCaptura)
            .Select(c => new MontajeEnGabinete(c.Espacio, c.Polos, $"el circuito {c.Espacio}"))];

    // ---- Mover circuitos (I-69) ------------------------------------------------------------------

    /// <summary>El «espacio» del zócalo del principal al arrastrar: no es un espacio numerado (I-70).</summary>
    public const int Zocalo = 0;

    /// <summary>Lo que había antes del último movimiento u optimización: todo el tablero, para deshacer.</summary>
    private (List<CircuitoJson> Circuitos, int? EspacioDelPrincipal, MontajeDelPrincipal Montaje)? _antes;

    /// <summary>Cómo quedó el tablero justo después del último movimiento: si ya cambió, no se deshace.</summary>
    private string? _firmaDespues;

    /// <summary>
    /// ¿Hay un movimiento u optimización que deshacer? <b>Solo mientras nada más haya cambiado</b>:
    /// deshacer después de capturar otra cosa se llevaría esa captura.
    /// </summary>
    public bool PuedeDeshacer => _antes is not null && Firma() == _firmaDespues;

    private string Firma() =>
        string.Join("|", _circuitos.Select(c => System.Text.Json.JsonSerializer.Serialize(CircuitoJson.De(c), ContextoDelArchivo.Legible.CircuitoJson)))
        + "|" + Datos.EspacioDelPrincipal + "|" + Datos.MontajePrincipal;

    /// <summary>
    /// Lleva el interruptor del espacio <paramref name="origen"/> —con todo lo capturado: carga,
    /// aparatos, polos, canalización— a <paramref name="destino"/>, y el origen queda libre. Si el
    /// origen es del principal en espacios, mueve el principal. <b>Si no cabe, no cambia nada</b> y
    /// dice por qué: la regla del arrastre del escritorio (<see cref="AcomodoEnGabinete.MotivoNoCabe"/>).
    /// </summary>
    public ResultadoDelMovimiento MoverCircuito(int origen, int destino)
    {
        if (origen < Zocalo || origen > _circuitos.Count || destino < Zocalo || destino > _circuitos.Count)
            return ResultadoDelMovimiento.NoValida("Ese espacio no existe en el tablero.");

        // El zócalo del principal (I-70): solo el principal entra o sale de ahí.
        if (origen == Zocalo)
            return SacarDelZocalo(destino);
        var renglon = _circuitos[origen - 1];
        if (destino == Zocalo)
            return renglon.EsDelPrincipal
                ? MeterAlZocalo()
                : ResultadoDelMovimiento.NoValida("El zócalo es solo del interruptor principal.");
        if (renglon.EsDelPrincipal)
            return MoverPrincipal(destino);

        var c = renglon.ContinuacionDe is { } dueno ? _circuitos[dueno - 1] : renglon;
        if (!c.TieneCaptura)
            return ResultadoDelMovimiento.NoValida($"En el espacio {origen} no hay circuito que mover.");
        if (destino == c.Espacio)
            return ResultadoDelMovimiento.SinCambio;

        var ocupados = MontajesCapturados(excepto: c);
        if (EspaciosDelPrincipal.Count > 0)
            ocupados.Add(new MontajeEnGabinete(EspaciosDelPrincipal[0], EspaciosDelPrincipal.Count, "el interruptor principal"));
        if (AcomodoEnGabinete.MotivoNoCabe(destino, c.Polos, Datos.NumeroEspacios, ocupados) is { } motivo)
            // Si cabe en el tablero y aun así no se puede, es que alguien está ahí.
            return DistribucionBarras.CabeEnElTablero(destino, c.Polos, Datos.NumeroEspacios)
                ? ResultadoDelMovimiento.Ocupada(motivo)
                : ResultadoDelMovimiento.NoValida(motivo);

        Recordar();
        Reubicar([(c.Espacio, destino)]);
        Recalcular();
        _firmaDespues = Firma();
        return ResultadoDelMovimiento.Hecho($"El circuito {c.Espacio} pasó al espacio {DistribucionBarras.EspaciosQueOcupa(destino, c.Polos)[0]}{(c.Polos > 1 ? $" ({string.Join("-", DistribucionBarras.EspaciosQueOcupa(destino, c.Polos))})" : "")}.");
    }

    /// <summary>Del zócalo a los espacios: «En espacios del gabinete» desde <paramref name="destino"/>.</summary>
    private ResultadoDelMovimiento SacarDelZocalo(int destino)
    {
        if (!Datos.UsaInterruptorPrincipal || Datos.Barras.Count < 2 || Datos.MontajePrincipal != MontajeDelPrincipal.Zocalo)
            return ResultadoDelMovimiento.NoValida("No hay interruptor principal en el zócalo.");
        if (destino == Zocalo)
            return ResultadoDelMovimiento.SinCambio;

        var polos = Datos.PolosDelPrincipal;
        if (AcomodoEnGabinete.MotivoNoCabe(destino, polos, Datos.NumeroEspacios, MontajesCapturados(excepto: null)) is { } motivo)
            return DistribucionBarras.CabeEnElTablero(destino, polos, Datos.NumeroEspacios)
                ? ResultadoDelMovimiento.Ocupada(motivo)
                : ResultadoDelMovimiento.NoValida(motivo);

        Recordar();
        Datos.MontajePrincipal = MontajeDelPrincipal.EnEspacios;
        Datos.EspacioDelPrincipal = destino;
        Recalcular();
        _firmaDespues = Firma();
        return ResultadoDelMovimiento.Hecho($"El interruptor principal pasó del zócalo a los espacios {string.Join("-", EspaciosDelPrincipal)}.");
    }

    /// <summary>De los espacios al zócalo: «Zócalo propio», y sus espacios quedan libres.</summary>
    private ResultadoDelMovimiento MeterAlZocalo()
    {
        if (Datos.Barras.Count < 2)
            return ResultadoDelMovimiento.NoValida("Con una sola barra no hay zócalo para el principal.");

        var libres = string.Join("-", EspaciosDelPrincipal);
        Recordar();
        Datos.MontajePrincipal = MontajeDelPrincipal.Zocalo;
        Recalcular();
        _firmaDespues = Firma();
        return ResultadoDelMovimiento.Hecho($"El interruptor principal pasó al zócalo propio; los espacios {libres} quedan libres.");
    }

    private ResultadoDelMovimiento MoverPrincipal(int destino)
    {
        var polos = EspaciosDelPrincipal.Count;
        if (destino == EspaciosDelPrincipal[0])
            return ResultadoDelMovimiento.SinCambio;
        var ocupados = MontajesCapturados(excepto: null);
        if (AcomodoEnGabinete.MotivoNoCabe(destino, polos, Datos.NumeroEspacios, ocupados) is { } motivo)
            return DistribucionBarras.CabeEnElTablero(destino, polos, Datos.NumeroEspacios)
                ? ResultadoDelMovimiento.Ocupada(motivo)
                : ResultadoDelMovimiento.NoValida(motivo);

        Recordar();
        Datos.EspacioDelPrincipal = destino;
        Recalcular();
        _firmaDespues = Firma();
        return ResultadoDelMovimiento.Hecho($"El interruptor principal pasó a los espacios {string.Join("-", EspaciosDelPrincipal)}.");
    }

    /// <summary>
    /// Reacomoda los circuitos para el menor desbalanceo que se alcance moviendo lo menos posible —
    /// <see cref="BalanceoDeFases.Proponer"/>, el del escritorio: intercambios entre circuitos de los
    /// mismos polos y mudanzas a espacios libres, con la misma fórmula que el porcentaje de la tarjeta.
    /// El principal y los renglones capturados sin carga no se mueven.
    /// </summary>
    public PropuestaDeBalanceo OptimizarBalanceo()
    {
        var conCarga = _circuitos.Where(c => c.TieneCarga && c.Resultado is not null).ToList();
        var fijos = MontajesCapturados(excepto: null)
            .Where(m => !conCarga.Any(c => c.Espacio == m.EspacioInicial))
            .ToList();
        if (EspaciosDelPrincipal.Count > 0)
            fijos.Add(new MontajeEnGabinete(EspaciosDelPrincipal[0], EspaciosDelPrincipal.Count, "el interruptor principal"));

        var propuesta = BalanceoDeFases.Proponer(
            [.. conCarga.Select(c => new CircuitoBalanceable(c.Espacio.ToString(CultureInfo.InvariantCulture), c.Espacio, c.Polos, c.Resultado!.CorrienteDisenoA))],
            fijos, Datos.NumeroEspacios, Datos.Sistema);

        if (propuesta.Mejora)
        {
            Recordar();
            Reubicar([.. propuesta.Movimientos.Select(m => (m.De, m.A))]);
            Recalcular();
            _firmaDespues = Firma();
        }
        return propuesta;
    }

    /// <summary>Regresa el tablero a como estaba antes del último movimiento u optimización.</summary>
    public void Deshacer()
    {
        if (!PuedeDeshacer || _antes is not { } antes)
            return;
        for (var i = 0; i < _circuitos.Count; i++)
        {
            var nuevo = new CircuitoDelCuadro(i + 1);
            if (i < antes.Circuitos.Count)
                antes.Circuitos[i].Aplicar(nuevo, Datos, []);
            _circuitos[i] = nuevo;
        }
        Datos.EspacioDelPrincipal = antes.EspacioDelPrincipal;
        Datos.MontajePrincipal = antes.Montaje;
        _antes = null;
        _firmaDespues = null;
        Recalcular();
    }

    private void Recordar() =>
        _antes = ([.. _circuitos.Select(CircuitoJson.De)], Datos.EspacioDelPrincipal, Datos.MontajePrincipal);

    /// <summary>
    /// Mueve varios circuitos a la vez —un intercambio incluido—: primero se toman todos, luego se
    /// vacían sus orígenes y al final se ponen en sus destinos.
    /// </summary>
    private void Reubicar(IReadOnlyList<(int De, int A)> movimientos)
    {
        var tomados = movimientos.Select(m => (m.A, Datos: CircuitoJson.De(_circuitos[m.De - 1]))).ToList();
        foreach (var (de, _) in movimientos)
            _circuitos[de - 1] = new CircuitoDelCuadro(de);
        foreach (var (a, datos) in tomados)
        {
            var nuevo = new CircuitoDelCuadro(a);
            datos.Aplicar(nuevo, Datos, []);
            _circuitos[a - 1] = nuevo;
        }
        // El par no simultáneo sigue al circuito que se movió — I-121.
        var destinos = movimientos.ToDictionary(m => m.De, m => m.A);
        foreach (var c in _circuitos)
            if (c.NoSimultaneoCon is { } n && destinos.TryGetValue(n, out var ahora))
                c.NoSimultaneoCon = ahora;
    }

    /// <summary>
    /// De lo que dice la placa (VA, W o A) a volt-amperes — I-25. <b>La regla no se escribe aquí</b>:
    /// es <see cref="ConsumoDePlaca.AVoltAmperes"/>, la misma que usa el escritorio, con la tensión y
    /// los polos del circuito para que unos amperes capturados regresen como los mismos amperes
    /// calculados.
    /// </summary>
    private void MarcarSinTipo()
    {
        foreach (var c in _circuitos)
        {
            c.SinTipo = false;
            c.SinTipo = ExigirTipo && !c.TipoElegido && c.TieneCarga;
        }
    }

    private void ConvertirCargas()
    {
        foreach (var c in _circuitos)
        {
            c.SincronizarTipo();
            // I-46: el uso de los contactos solo cuenta en vivienda (210-11(c), 220-52). Con líneas de subtipo
            // elegido, el uso sale de ellas: «Contactos · Ap. pequeños» (captura-en-el-desplegable.md).
            var uso = c.TieneDesglose && c.Cargas.Any(a => a.Subtipo is not null)
                ? c.Cargas.Select(a => a.Subtipo?.Uso()).FirstOrDefault(u => u is not null) ?? UsoDeContactos.General
                : c.Categoria == CategoriaDeCarga.Contactos ? c.Uso : UsoDeContactos.General;
            c.UsoEfectivo = Datos.Inmueble.AplicaUsoDeContactos() ? uso : UsoDeContactos.General;
            c.CorrienteDeMotorA = 0m;
            c.MotorEnAmperes = null;
            c.MotorVA = 0m;
            c.MotorAl125 = null;
            c.CorrienteDeServicioA = 0m;

            // UN GRUPO, DE CUALQUIER TIPO — I-115, I-123: motores o motocompresores entre sus cargas.
            if (c.EsGrupo)
            {
                c.Ajuste220_52VA = 0m;
                SumarGrupo(c);
                continue;
            }

            // I-15, I-74: un motor o un equipo de A/C no tiene carga continua ni no continua. Su
            // corriente es la FLC de tabla (430-6(a)) o la de la placa (440-6(a), 440-4(b)), y sus
            // VA, esa corriente por la tensión del circuito.
            if (c.EsDeMotor)
            {
                c.ContinuaVA = 0m;
                c.NoContinuaVA = 0m;
                c.Ajuste220_52VA = 0m;
                if (CorrienteDeMotor(c) is { } corriente)
                {
                    c.CorrienteDeMotorA = corriente;
                    c.MotorVA = corriente * TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
                }
                c.Porciones = [new PorcionDeCarga(c.Categoria, 0m, 0m, c.MotorVA)];
                continue;
            }

            // OTROS TABLEROS (David, 2026-09-30): uno o varios, cada uno en su línea con su continua y su no
            // continua; el alimentador lleva la suma — 215-2(a)(1), 220-40.
            if (c.Categoria == CategoriaDeCarga.Tablero)
            {
                c.RenglonALineas();
                if (c.Cargas.Count > 0)
                    SumarTableros(c);
            }
            else if (c.TieneDesglose)
                SumarDesglose(c);
            // 424-3(b): la calefacción fija de ambiente es carga continua. Lo capturado como no continua
            // pasa a continua — R-18. En el desglose, cada carga de calefacción ya es continua.
            else if (c.Categoria == CategoriaDeCarga.CalefaccionFija && c.NoContinua > 0m)
            {
                c.Continua += c.NoContinua;
                c.NoContinua = 0m;
            }

            c.ContinuaVA = AVoltAmperes(c, c.Continua);
            c.NoContinuaVA = AVoltAmperes(c, c.NoContinua);
            if (!c.TieneDesglose && c.Categoria != CategoriaDeCarga.Tablero)
                c.Porciones = [new PorcionDeCarga(c.Categoria, c.ContinuaVA, c.NoContinuaVA, 0m)];
            else if (c.Categoria == CategoriaDeCarga.Tablero)
                c.Porciones = [new PorcionDeCarga(CategoriaDeCarga.Tablero, c.ContinuaVA, c.NoContinuaVA, 0m)];
            c.Ajuste220_52VA = c.TieneCarga && c.UsoEfectivo.ReferenciaCargaMinima() is not null
                ? Math.Max(0m, UsosDeContactos.CargaMinimaAlimentadorVA - c.CargaInstaladaVA)
                : 0m;
        }
    }

    /// <summary>
    /// <b>La carga del circuito sale de sus cargas</b> — I-35, I-123. Cada una se convierte a VA con su F.P.
    /// y la tensión y los polos del circuito, con el mínimo de su subtipo (220-14); el circuito queda en
    /// VA, con la suma de las continuas, la de las no continuas, el F.P. combinado (P / √(P² + Q²)) y la
    /// carga partida por tipo (<see cref="CircuitoDelCuadro.Porciones"/>).
    /// </summary>
    private void SumarDesglose(CircuitoDelCuadro c)
    {
        var divisor = TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
        var tension = TensionDelMotor(c);
        foreach (var a in c.Cargas)
        {
            PrepararCarga(a, c);
            // Un aparato con motor (I-118): su corriente, de la tabla o de su placa, por la tensión del circuito.
            CorrienteDeMaquina(a, c.Polos, tension);
            a.TotalVA = a.EsMaquina ? a.Cantidad * a.CorrienteUnitariaA * divisor : VADeLaCarga(a, c);
        }
        AplicarMinimoPorCircuito(c);

        // 220-18(a) — I-118: un aparato con motor de más de ⅛ hp, junto con otras cargas: el motor mayor
        // al 125 % y lo demás al 100 %. El 125 % se lo da el cálculo del derivado a la continua: el motor
        // mayor —una unidad— va ahí; los demás motores, a la no continua. En calefacción todo es continuo.
        var motores = c.Cargas.Where(a => a.EsMaquina && a.CorrienteUnitariaA > 0m).ToList();
        c.MotorAl125 = c.Cargas.Any(a => !a.EsMaquina && a.TotalVA > 0m)
            ? motores.Where(MasDeUnOctavoDeHp).OrderByDescending(a => a.CorrienteUnitariaA).FirstOrDefault()
            : null;

        var porciones = new Dictionary<CategoriaDeCarga, (decimal Continua, decimal NoContinua)>();
        void Sumar(CategoriaDeCarga tipo, decimal continua, decimal noContinua)
        {
            var (x, y) = porciones.GetValueOrDefault(tipo);
            porciones[tipo] = (x + continua, y + noContinua);
        }
        foreach (var a in c.Cargas)
        {
            var tipo = c.TipoDe(a);
            if (!a.EsMaquina)
                Sumar(tipo, EsContinua(a) ? a.TotalVA : 0m, EsContinua(a) ? 0m : a.TotalVA);
            else if (tipo == CategoriaDeCarga.CalefaccionFija)
                Sumar(tipo, a.TotalVA, 0m); // 424-3(b): el motor de la calefacción también es continuo
            else
            {
                var al125 = ReferenceEquals(a, c.MotorAl125) ? a.CorrienteUnitariaA * divisor : 0m;
                Sumar(tipo, al125, a.TotalVA - al125);
            }
        }

        c.Porciones = [.. porciones.Select(p => new PorcionDeCarga(p.Key, p.Value.Continua, p.Value.NoContinua, 0m))];
        c.Unidad = UnidadConsumo.VoltAmperes;
        c.Continua = c.Porciones.Sum(p => p.ContinuaVA);
        c.NoContinua = c.Porciones.Sum(p => p.NoContinuaVA);
        c.FactorPotencia = c.Continua + c.NoContinua > 0m
            ? FactorPotenciaCombinado.De(c.Cargas.Select(a => (a.TotalVA, a.FactorPotencia)))
            : CircuitoDelCuadro.FactorPotenciaSupuesto;
    }

    /// <summary>
    /// <b>Uno o varios tableros alimentados</b> — 215-2(a)(1), 220-40. Cada línea trae la continua y la no
    /// continua ya calculadas de su tablero; se convierten a VA con su F.P. y se suman, sin otro factor de
    /// demanda. El circuito queda en VA, como un desglose.
    /// </summary>
    private void SumarTableros(CircuitoDelCuadro c)
    {
        decimal EnVA(CargaDelCircuito a, decimal valor) => ConsumoDePlaca.AVoltAmperes(
            valor, a.Unidad, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV, c.Polos, a.FactorPotencia);
        decimal continua = 0m, noContinua = 0m;
        foreach (var a in c.Cargas)
        {
            a.Cantidad = 1;
            var (x, y) = a.EsTablero ? (EnVA(a, a.CargaUnitaria), EnVA(a, a.NoContinua)) : (0m, 0m);
            a.TotalVA = x + y;
            continua += x;
            noContinua += y;
        }
        c.Porciones = [new PorcionDeCarga(CategoriaDeCarga.Tablero, continua, noContinua, 0m)];
        c.Unidad = UnidadConsumo.VoltAmperes;
        c.Continua = continua;
        c.NoContinua = noContinua;
        c.FactorPotencia = continua + noContinua > 0m
            ? FactorPotenciaCombinado.De(c.Cargas.Select(a => (a.TotalVA, a.FactorPotencia)))
            : CircuitoDelCuadro.FactorPotenciaSupuesto;
    }

    /// <summary>
    /// <b>Un grupo de motores</b> — I-115, 430-53. Cada máquina con su corriente por unidad (la FLC de
    /// tabla, o la de un motor en amperes, 430-6(a)(1)); las otras cargas, a VA como en cualquier
    /// desglose. Las máquinas suman <see cref="CircuitoDelCuadro.MotorVA"/> y
    /// <see cref="CircuitoDelCuadro.CorrienteDeMotorA"/>; las otras, la continua y la no continua. El
    /// F.P. del circuito, el combinado de todos. Cada parte, con su tipo (I-123).
    /// </summary>
    private void SumarGrupo(CircuitoDelCuadro c)
    {
        var divisor = TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
        var tension = TensionDelMotor(c);
        foreach (var a in c.Cargas)
        {
            PrepararCarga(a, c);
            CorrienteDeMaquina(a, c.Polos, tension);
            a.TotalVA = a.EsMaquina ? a.Cantidad * a.CorrienteUnitariaA * divisor : VADeLaCarga(a, c);
        }
        AplicarMinimoPorCircuito(c);

        c.ContinuaVA = c.Cargas.Where(EsContinua).Sum(a => a.TotalVA);
        c.NoContinuaVA = c.Cargas.Where(a => !a.EsMaquina && !EsContinua(a)).Sum(a => a.TotalVA);
        c.CorrienteDeMotorA = c.Cargas.Where(a => a.EsMaquina).Sum(a => a.Cantidad * a.CorrienteUnitariaA);
        c.MotorVA = c.Cargas.Where(a => a.EsMaquina).Sum(a => a.TotalVA);
        c.Porciones =
        [
            .. c.Cargas.GroupBy(c.TipoDe).Select(g => new PorcionDeCarga(
                g.Key,
                g.Where(EsContinua).Sum(a => a.TotalVA),
                g.Where(a => !a.EsMaquina && !EsContinua(a)).Sum(a => a.TotalVA),
                g.Where(a => a.EsMaquina).Sum(a => a.TotalVA))),
        ];
        if (c.CargaInstaladaVA > 0m)
            c.FactorPotencia = FactorPotenciaCombinado.De(c.Cargas.Select(a => (a.TotalVA, a.FactorPotencia)));
    }

    /// <summary>
    /// Antes de sumar una carga: la cantidad, al menos 1; un «Contacto» sin subtipo ni carga toma 180 VA
    /// (220-14(i), como hasta el formato 4); y la que su tipo o subtipo hace continua lo es: calefacción
    /// (424-3(b)), calentador de agua (422-13) y anuncios (600-5(b)).
    /// </summary>
    private static void PrepararCarga(CargaDelCircuito a, CircuitoDelCuadro c)
    {
        a.Cantidad = Math.Max(1, a.Cantidad);
        a.ReferenciaMinimo = null;
        a.ReferenciaContinua = null;
        if (a.Subtipo is null && a.EsContactoSinCarga)
        {
            a.Unidad = UnidadConsumo.VoltAmperes;
            a.CargaUnitaria = CargaDelCircuito.VAPorContacto;
        }
        var siempre = a.Subtipo?.SiempreContinua() ?? (c.TipoDe(a) == CategoriaDeCarga.CalefaccionFija ? "424-3(b)" : null);
        if (siempre is not null && a.Clase == ClaseDeAparato.Carga)
        {
            a.Continua = true;
            a.ReferenciaContinua = siempre;
        }
    }

    /// <summary>Los VA de una carga, con el mínimo de su subtipo por unidad — 220-14 (I-123).</summary>
    private decimal VADeLaCarga(CargaDelCircuito a, CircuitoDelCuadro c)
    {
        var va = VADeCarga(a, c.Polos);
        if (a.Subtipo?.MinimoUnitarioVA(Datos.Inmueble.EsVivienda()) is { } minimo && va < a.Cantidad * minimo.VA)
        {
            a.ReferenciaMinimo = minimo.Referencia;
            return a.Cantidad * minimo.VA;
        }
        return va;
    }

    /// <summary>Anuncios y contorno: 1200 VA por circuito — 220-14(f). Lo que falta, a la primera de ellas.</summary>
    private static void AplicarMinimoPorCircuito(CircuitoDelCuadro c)
    {
        var anuncios = c.Cargas.Where(a => a.Subtipo?.MinimoPorCircuitoVA() is not null).ToList();
        if (anuncios.Count == 0)
            return;
        var (minimo, referencia) = anuncios[0].Subtipo!.Value.MinimoPorCircuitoVA()!.Value;
        var falta = minimo - anuncios.Sum(a => a.TotalVA);
        if (falta <= 0m)
            return;
        anuncios[0].TotalVA += falta;
        anuncios[0].ReferenciaMinimo = referencia;
    }

    /// <summary>
    /// Los VA de una carga del desglose: de su placa, en VA, W o A; un acondicionador de habitación, con su
    /// corriente total por la tensión del circuito (I-117).
    /// </summary>
    private decimal VADeCarga(CargaDelCircuito a, int polos) =>
        a.Clase == ClaseDeAparato.AireDeHabitacion
            ? a.Cantidad * a.CorrientePlacaA * TensionDeCalculo.Divisor(polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV)
            : a.Cantidad * ConsumoDePlaca.AVoltAmperes(
                a.CargaUnitaria, a.Unidad, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV, polos, a.FactorPotencia);

    /// <summary>Una carga marcada continua. Un acondicionador de habitación, no: su regla es la de 440-62.</summary>
    private static bool EsContinua(CargaDelCircuito a) => a.Clase == ClaseDeAparato.Carga && a.Continua;

    /// <summary>
    /// 440-62(b), (c) — I-117: los acondicionadores de habitación del desglose no pasan del 80 % del
    /// circuito si van solos, ni del 50 % con otras cargas. Solo avisa: el circuito ya calculó.
    /// </summary>
    private static string? AvisoDeHabitacion(CircuitoDelCuadro c)
    {
        if (!c.TieneDesglose || c.EsDeMotor || c.Resultado is not { } r)
            return null;
        var unidades = c.Cargas.Where(a => a.Clase == ClaseDeAparato.AireDeHabitacion && a.CorrientePlacaA > 0m).ToList();
        if (unidades.Count == 0)
            return null;
        if (c.Polos == 3)
            return $"Circuito {c.Espacio}: un acondicionador de habitación es monofásico — 440-60. En un circuito trifásico, captúralo con su placa (A/C, corriente nominal o MCA).";
        if (unidades.FirstOrDefault(a => a.CorrientePlacaA > 40m) is { } grande)
            return $"Circuito {c.Espacio}: {NombreDeMaquina(c, grande)} ({grande.CorrientePlacaA:0.##} A) pasa de los 40 A de un acondicionador de habitación — 440-62(a)(2).";

        var total = unidades.Sum(a => a.Cantidad * a.CorrientePlacaA);
        var conOtras = c.Cargas.Any(a => a.Clase != ClaseDeAparato.AireDeHabitacion && a.TotalVA > 0m);
        var fraccion = conOtras ? 0.5m : 0.8m;
        if (total <= fraccion * r.ProteccionA)
            return null;
        return $"Circuito {c.Espacio}: {(unidades.Sum(a => a.Cantidad) == 1 ? "el acondicionador de habitación" : "los acondicionadores de habitación")} " +
               $"({total:0.##} A) pasa{(unidades.Sum(a => a.Cantidad) == 1 ? "" : "n")} del {fraccion * 100m:0} % del circuito de {r.ProteccionA:0} A " +
               $"({fraccion * r.ProteccionA:0.##} A) — 440-62({(conOtras ? "c" : "b")}). " +
               (conOtras ? "Llévalo a un circuito propio: A/A y refrig., unidad «De hab.»." : "Captúralo como A/A y refrig., unidad «De hab.»: el circuito sale del tamaño que lo deja en 80 %.");
    }

    /// <summary>
    /// Lo que hay que revisar de un circuito, para el indicador de su renglón: las reglas de su clase que no
    /// se cumplen (210-23, Tabla 210-21(b)(3), 422-11(e)), la caída combinada de más de 5 % y 440-62. Antes
    /// solo estaban en la tarjeta «Avisos» al pie y en la memoria (auditoría del 2026-09-29, P3, riesgo 1).
    /// </summary>
    public IReadOnlyList<string> AvisosDe(CircuitoDelCuadro c) =>
    [
        .. c.ReglasDeClase.Where(x => x.Aviso).Select(x => $"{x.Texto.TrimEnd('.')} — {x.Referencia}."),
        .. c.AvisoCaidaCombinada is { } caida ? [caida] : Array.Empty<string>(),
        .. c.AvisoAireDeHabitacion is { } habitacion ? [habitacion] : Array.Empty<string>(),
        .. AvisosDelBypass(c),
        // R4-1: su canalización no cabe en ningún tubo de la Tabla 4.
        .. c.CanalizacionEfectiva is { NingunTamanoAlcanza: true } k
            ? [$"Ningún tubo de la Tabla 4 admite los conductores de su canalización ({NombreDe(k)}) — Capítulo 10, Tabla 1. Reparte los circuitos en más canalizaciones o cambia a ducto o charola."]
            : Array.Empty<string>(),
    ];

    /// <summary>
    /// <b>Lo que hay que revisar del bypass de un variador</b> — M-23: que ningún tamaño quede entre el 125 % de la
    /// entrada y el máximo (la menor de la del fabricante y la de la Tabla 430-52 del motor); o un variador con
    /// bypass en un grupo, donde 430-53 lo cuenta con su corriente de entrada.
    /// </summary>
    private static IEnumerable<string> AvisosDelBypass(CircuitoDelCuadro c)
    {
        if (c.EntradaDeLaProteccion is DatosEntradaCircuitoDerivadoVariador { Bypass: { } b } && c.Resultado?.Rango is { } r
            && r.MaximoA < r.CapacidadMinimaA)
            yield return $"Con el bypass, ningún tamaño queda entre el 125 % de la entrada del variador ({r.CapacidadMinimaA:N2} A) y el máximo " +
                         $"({r.MaximoA:N0} A, el menor de la máxima del fabricante y la de la Tabla 430-52 para el motor de {MotoresEnHp.Texto(b.Hp)} HP): " +
                         "se usa el máximo. Revisar con el fabricante del conjunto variador y bypass — 430-122(b), 110-3(b).";
        if (c.EsGrupo)
            foreach (var a in c.Cargas.Where(a => a.Clase == ClaseDeAparato.Variador && a.ConBypass))
                yield return $"{NombreDeMaquina(c, a)}: el bypass no entra al cálculo del grupo, que lo cuenta con su corriente de entrada — 430-53. " +
                             "430-122(b) se aplica con el variador solo en su circuito.";
    }

    /// <summary>Los avisos de los circuitos que no son de caída: por ahora, 440-62 (I-117).</summary>
    public IEnumerable<string> AvisosDeCircuitos =>
        _circuitos.Where(c => c.AvisoAireDeHabitacion is not null).Select(c => c.AvisoAireDeHabitacion!)
            .Concat(_circuitos.SelectMany(c => c.ReglasDeClase.Where(x => x.Aviso).Select(x => $"Circuito {c.Espacio}: {x.Texto.TrimEnd('.')} — {x.Referencia}.")))
            .Concat(_avisosNoSimultaneos);

    /// <summary>
    /// Más de 93.25 W (⅛ hp) — 220-18(a). En HP, los de la tabla empiezan en ⅙; en amperes, sus HP
    /// interpolados (430-6(a)(1)).
    /// </summary>
    private static bool MasDeUnOctavoDeHp(CargaDelCircuito a) =>
        (a.CapturaMotor == CapturaDeMotor.Hp ? a.Hp : a.MotorEnAmperes?.Hp) > 0.125m;

    /// <summary>
    /// Lo que un desglose de aparatos con motor no puede calcular — I-118: un motor que la tabla no
    /// trae, o un circuito que solo alimenta motores, que es del Art. 430 — 220-18(a).
    /// </summary>
    private static string? ErrorDeMotoresEnDesglose(CircuitoDelCuadro c)
    {
        if (c.Cargas.FirstOrDefault(a => a.EsMaquina && a.Error is not null) is { } malo)
            return $"{NombreDeMaquina(c, malo)}: {malo.Error}";
        if (c.Cargas.Any(a => a.EsMaquina && a.CorrienteUnitariaA > 0m) && !c.Cargas.Any(a => !a.EsMaquina && a.TotalVA > 0m))
            return "220-18(a): el circuito solo alimenta motores, y eso se calcula con el Art. 430 (430-53). Quita las líneas " +
                   "sin carga del desplegable, o captura las otras cargas del circuito.";
        return null;
    }

    /// <summary>
    /// «Lavadora: 5.00 A × 127.02 V = 635 VA, el motor mayor, entra al 125 %…» — 220-18(a), I-118. Null
    /// si el desglose no lleva un motor que lo pida.
    /// </summary>
    public string? NotaDelMotorMayor(CircuitoDelCuadro c)
    {
        if (!c.TieneDesglose || c.EsDeMotor || c.MotorAl125 is not { } m)
            return null;
        var divisor = TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
        return $"{NombreDeMaquina(c, m)}, el motor mayor: {m.CorrienteUnitariaA:N2} A = {m.CorrienteUnitariaA * divisor:#,0} VA entra como " +
               "continua (al 125 %); los demás motores, al 100 % — 220-18(a)";
    }

    /// <summary>
    /// La corriente de una máquina del desglose, por unidad: la FLC de tabla por sus HP, la de un motor
    /// marcado en amperes (sus HP, interpolados — 430-6(a)(1)) o la de 440-6(a) de un motocompresor. En
    /// cero, con <see cref="CargaDelCircuito.Error"/> si la tabla no la trae. Una carga queda en cero.
    /// </summary>
    private void CorrienteDeMaquina(CargaDelCircuito a, int polos, decimal tension)
    {
        a.CorrienteUnitariaA = 0m;
        a.MotorEnAmperes = null;
        a.Error = null;
        if (!a.EsMaquina || !a.TieneCapturaDeMaquina)
            return;

        if (a.Clase == ClaseDeAparato.Motocompresor)
            a.CorrienteUnitariaA = CalculadoraCarga440.CorrienteBase(a.CorrientePlacaA, a.CorrienteSeleccionA);
        else if (a.Clase == ClaseDeAparato.Variador)
            a.CorrienteUnitariaA = a.CorrientePlacaA; // la de entrada del variador — 430-122(a)
        else if (a.CapturaMotor == CapturaDeMotor.Amperes)
        {
            a.MotorEnAmperes = MotoresEnHp.DeAmperes(_motor.FlcMotor, a.CorrientePlacaA, polos, tension);
            a.CorrienteUnitariaA = a.MotorEnAmperes?.Amperes ?? 0m;
            if (a.MotorEnAmperes is null)
                a.Error = MotoresEnHp.SinFilaEnAmperes(_motor.FlcMotor, a.CorrientePlacaA, polos, tension);
        }
        else
        {
            a.CorrienteUnitariaA = MotoresEnHp.Flc(_motor.FlcMotor, a.Hp!.Value, polos, tension) ?? 0m;
            if (a.CorrienteUnitariaA == 0m)
                a.Error = MotoresEnHp.SinFila(_motor.FlcMotor, a.Hp.Value, polos, tension);
        }
    }

    /// <summary>
    /// La corriente de un motor o un equipo de A/C, de lo capturado: la FLC de la tabla por sus HP, la
    /// de un motor marcado en amperes (430-6(a)(1), que deja sus HP interpolados en
    /// <see cref="CircuitoDelCuadro.MotorEnAmperes"/>), o la de la placa del A/C. <c>null</c> si no
    /// calcula: sin captura, o fuera de la tabla.
    /// </summary>
    private decimal? CorrienteDeMotor(CircuitoDelCuadro c)
    {
        if (c.EsAireAcondicionado)
            return AireAcondicionadoDePlaca.Corriente(c);

        if (c.CapturaMotor == CapturaDeMotor.Hp)
            return c.Hp > 0m ? MotoresEnHp.Flc(_motor.FlcMotor, c.Hp.Value, c.Polos, TensionDelMotor(c)) : null;
        // Con variador, la corriente del circuito es la de entrada del variador — 430-122(a), I-119.
        if (c.CapturaMotor == CapturaDeMotor.Variador)
            return c.CorrienteEntradaVariadorA > 0m ? c.CorrienteEntradaVariadorA : null;

        c.MotorEnAmperes = MotoresEnHp.DeAmperes(_motor.FlcMotor, c.CorrientePlacaA, c.Polos, TensionDelMotor(c));
        return c.MotorEnAmperes?.Amperes;
    }

    /// <summary>La tensión con la que se entra a la tabla de FLC: la del circuito — <see cref="MotoresEnHp.Tension"/>.</summary>
    private decimal TensionDelMotor(CircuitoDelCuadro c) =>
        MotoresEnHp.Tension(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);

    private decimal AVoltAmperes(CircuitoDelCuadro c, decimal valor) =>
        ConsumoDePlaca.AVoltAmperes(
            valor, c.Unidad, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV, c.Polos, c.FactorPotencia);

    /// <summary>Las barras que toca cada renglón. La resuelve la geometría del tablero.</summary>
    private void ResolverFases()
    {
        foreach (var c in _circuitos)
            c.Fases = c.EsContinuacion
                ? string.Empty
                : DistribucionBarras.FasesQueOcupa(c.Espacio, c.Polos, Datos.Sistema);
    }

    private ConfiguracionTablero Configuracion => SistemaDelTablero.De(Datos.Sistema);

    /// <summary>
    /// ANTES DE CALCULAR: a qué canalización va cada circuito, cuántos portadores lleva cada una y si
    /// el ajuste por agrupamiento aplica según su tipo — I-39. No depende del calibre, así que no hay
    /// ciclo: el conteo sale de polos y neutros, y el calibre sale después con ese factor.
    /// </summary>
    private void PrepararCanalizaciones()
    {
        foreach (var c in _circuitos)
        {
            // I-41: 1 polo siempre con neutro; 2 y 3 polos solo con «+N».
            // Un motor trifásico no lleva neutro, aunque la casilla se haya marcado antes de pasar a Motor — I-114.
            c.LlevaNeutro = SistemaConNeutro && (c.Polos == 1 || (c.ConNeutro && !(c.EsMotor && c.Polos == 3)));
            c.CanalizacionEfectiva = null;
        }

        var derivados = _circuitos.Where(c => !c.EsContinuacion).ToList();
        foreach (var c in derivados.Where(c => c.Canalizacion is not null && Datos.Canalizacion(c.Canalizacion) is null))
            c.Canalizacion = null; // la quitaron

        // Una canalización automática que se quedó sin circuitos con carga se va, y deja libre su número.
        var ocupadas = derivados.Where(c => c.TieneCarga && c.Canalizacion is not null).Select(c => c.Canalizacion!).ToHashSet();
        foreach (var t in Datos.Canalizaciones.Where(t => t.Automatica && !ocupadas.Contains(t.Id)).ToList())
        {
            Datos.Canalizaciones.Remove(t);
            foreach (var c in derivados.Where(c => c.Canalizacion == t.Id))
                c.Canalizacion = null;
        }

        // 1 circuito, 1 tubo (David, 2026-09-24): el que tiene carga y no va en ninguna recibe el suyo.
        foreach (var c in derivados.Where(c => c.TieneCarga && c.Canalizacion is null))
            c.Canalizacion = Datos.NuevaCanalizacion(automatica: true).Id;

        foreach (var canal in Datos.Canalizaciones)
        {
            canal.Limpiar();
            foreach (var c in derivados.Where(c => c.Canalizacion == canal.Id))
                c.CanalizacionEfectiva = canal;
            canal.Circuitos = [.. derivados.Where(c => c.TieneCarga && c.Canalizacion == canal.Id)];
            if (canal.Circuitos.Count > 0)
                Contar(canal, [.. canal.Circuitos.Select(c => new CircuitoEnCanalizacion(
                    $"circuito {c.Espacio}", c.Polos, c.LlevaNeutro, c.Fases.ToCharArray()))]);
        }
    }

    /// <summary>Conteo, ajuste y azotea de una canalización.</summary>
    private void Contar(CanalizacionDelTablero canal, IReadOnlyList<CircuitoEnCanalizacion> circuitos, decimal? ocupacionPct = null)
    {
        var conteo = ContadorDePortadores.Contar(circuitos, Configuracion, Datos.CargaNoLineal, canal.NeutroCompartido);
        canal.Conteo = conteo;
        canal.Ajuste = AjusteDeAgrupamientoPorCanalizacion.Evaluar(canal.Tipo, conteo.Portadores, canal.AreaInteriorMm2, ocupacionPct);
        canal.FactorAgrupamiento = _motor.Agrupamiento.Factor(canal.Ajuste.ConductoresParaElMotor);
        canal.SumadorAzoteaC = 0m;
        if (canal.AlturaSobreTechoMm is { } altura && canal.Tipo.EsTubo())
        {
            try { canal.SumadorAzoteaC = _motor.Azotea.Sumador(altura); }
            catch (InvalidOperationException ex) { canal.Avisos.Add(ex.Message); }
        }
        foreach (var aviso in conteo.Avisos.Where(a => !canal.Avisos.Contains(a)))
            canal.Avisos.Add(aviso);
    }

    /// <summary>
    /// DESPUÉS DE CALCULAR: el tamaño de cada canalización con los calibres que resultaron —
    /// Capítulo 10. Y lo que solo se sabe con el resultado: un circuito que salió en paralelo dentro
    /// de un tubo compartido, o una superficial metálica que pasó del 20 % y pierde la exención de
    /// 386-22 (entonces se recalcula con el ajuste).
    /// </summary>
    private void DimensionarCanalizaciones()
    {
        var recalcular = false;
        foreach (var canal in TodasLasCanalizaciones.Where(t => !t.EsAlimentador).ToList())
        {
            Dimensionar(canal, [.. canal.Circuitos.Where(c => c.Resultado is not null)
                .Select(ConductoresDe)]);

            var enParalelo = canal.Circuitos.Where(c => c.Resultado?.NumeroConductoresParalelo > 1).ToList();
            if (canal.Circuitos.Count == 1 && enParalelo.Count == 1)
                canal.CanalizacionesIguales = enParalelo[0].Resultado!.NumeroConductoresParalelo;
            else
                foreach (var c in enParalelo)
                    canal.Avisos.Add($"El circuito {c.Espacio} salió con {c.Resultado!.NumeroConductoresParalelo} conductores por fase: "
                        + "cada juego va en su propia canalización y todas iguales — 310-10(h)(3). Pásalo a una canalización solo para él.");

            if (canal.Tipo == TipoCanalizacion.SuperficialMetalica && canal.Ajuste is { Aplica: false } && canal.Ocupacion?.OcupacionPct > 20m)
                recalcular = true;
        }

        if (!recalcular)
            return;

        // 386-22: la exención pedía no pasar del 20 %. Se vuelve a contar con la ocupación real.
        foreach (var canal in Datos.Canalizaciones.Where(t => t.Tipo == TipoCanalizacion.SuperficialMetalica && t.Ocupacion?.OcupacionPct > 20m))
            Contar(canal, [.. canal.Circuitos.Select(c => new CircuitoEnCanalizacion(
                $"circuito {c.Espacio}", c.Polos, c.LlevaNeutro, c.Fases.ToCharArray()))], canal.Ocupacion!.OcupacionPct);
        CalcularCircuitos();
        foreach (var canal in TodasLasCanalizaciones.Where(t => !t.EsAlimentador).ToList())
            Dimensionar(canal, [.. canal.Circuitos.Where(c => c.Resultado is not null)
                .Select(ConductoresDe)]);
    }

    private sealed record ConductoresDelCircuito(
        string Nombre, int Polos, bool LlevaNeutro,
        DesignSuite.Calculo.Unidades.Calibre Fase, DesignSuite.Calculo.Unidades.Calibre Neutro, DesignSuite.Calculo.Unidades.Calibre Tierra);

    private static ConductoresDelCircuito ConductoresDe(CircuitoDelCuadro c) =>
        new($"circuito {c.Espacio}", c.Polos, c.LlevaNeutro, c.Resultado!.CalibreFase, c.Resultado.CalibreNeutro, c.Resultado.CalibreTierra);

    /// <summary>
    /// Arma la lista de conductores de una canalización y le pide el tamaño al motor. Con neutro
    /// compartido va un solo neutro, el mayor; con tierra común una sola tierra, la mayor — 250-122(c).
    /// </summary>
    private void Dimensionar(
        CanalizacionDelTablero canal,
        IReadOnlyList<ConductoresDelCircuito> circuitos,
        int juegosPorCanalizacion = 1)
    {
        if (circuitos.Count == 0)
        {
            canal.Ocupacion = null;
            return;
        }

        var aislamiento = Datos.TipoAislamiento;
        decimal? Diametro(string designacion) =>
            Datos.DiametrosFabricante.TryGetValue(DatosDelTablero.ClaveDiametro(aislamiento, designacion), out var d) ? d : null;
        ConductorEnCanalizacion Conductor(string circuito, PapelConductor papel, DesignSuite.Calculo.Unidades.Calibre calibre, int cantidad, bool desnudo = false) =>
            new(circuito, papel, calibre.Designacion, desnudo ? null : aislamiento, desnudo ? null : Diametro(calibre.Designacion), cantidad);

        var neutroCompartido = canal.Conteo?.NeutroCompartidoAplicado == true;
        var conductores = new List<ConductorEnCanalizacion>();
        foreach (var x in circuitos)
        {
            conductores.Add(Conductor(x.Nombre, PapelConductor.Fase, x.Fase, x.Polos * juegosPorCanalizacion));
            if (x.LlevaNeutro && !(neutroCompartido && x.Polos == 1))
                conductores.Add(Conductor(x.Nombre, PapelConductor.Neutro, x.Neutro, juegosPorCanalizacion));
            // Los juegos en paralelo en la misma canalización llevan una sola tierra — 250-122(f), R4-2.
            if (!canal.TierraComun)
                conductores.Add(Conductor(x.Nombre, PapelConductor.Tierra, x.Tierra, 1, canal.TierraDesnuda));
        }
        if (neutroCompartido)
        {
            var mayor = circuitos.Where(x => x.Polos == 1).Select(x => x.Neutro).MaxBy(k => k.AreaMm2)!;
            conductores.Add(Conductor("neutro compartido", PapelConductor.Neutro, mayor, 1));
        }
        if (canal.TierraComun)
        {
            var mayor = circuitos.Select(x => x.Tierra).MaxBy(k => k.AreaMm2)!;
            conductores.Add(Conductor("tierra común", PapelConductor.Tierra, mayor, 1, canal.TierraDesnuda));
        }

        try
        {
            canal.Ocupacion = _motor.Ocupacion.Calcular(
                canal.Tipo, canal.Tubo, conductores, canal.AnchoMm, canal.AltoMm,
                canal.AreaInteriorMm2, canal.MaxConductoresFabricante, canal.TamanoFijado);
            foreach (var aviso in canal.Ocupacion.Avisos.Where(a => !canal.Avisos.Contains(a)))
                canal.Avisos.Add(aviso);
        }
        catch (InvalidOperationException ex)
        {
            canal.Ocupacion = null;
            canal.Avisos.Add(ex.Message);
        }
    }

    // ---- LO QUE YA SE CALCULÓ (I-97) --------------------------------------------------------------
    // Cambiar la carga de un circuito recalcula el tablero entero, y los otros 41 derivados llegaban al
    // motor con la misma entrada que la vez anterior. La entrada es un record de puros valores y el
    // resultado no se modifica después: con la misma entrada y la misma serie, el mismo resultado —
    // o el mismo error. En el navegador es la mitad del tiempo de Recalcular.

    private readonly Dictionary<(SerieDeInterruptores, object), (ResultadoCircuitoDerivado? Resultado, Exception? Error)> _recordados = [];
    private const int MaximoRecordados = 1000;

    private ResultadoCircuitoDerivado Recordado<T>(T entrada) where T : notnull
    {
        var llave = (Datos.SerieInterruptores, (object)entrada);
        if (!_recordados.TryGetValue(llave, out var r))
        {
            if (_recordados.Count >= MaximoRecordados)
                _recordados.Clear();
            try
            {
                r = (entrada switch
                {
                    DatosEntradaCircuitoDerivadoNoMotor d => _motor.NoMotor(Datos.SerieInterruptores).Calcular(d),
                    DatosEntradaCircuitoDerivadoMotor d => _motor.Motor(Datos.SerieInterruptores).Calcular(d),
                    DatosEntradaCircuitoDerivado440 d => _motor.AireAcondicionado(Datos.SerieInterruptores).Calcular(d),
                    DatosEntradaCircuitoDerivadoGrupo d => _motor.Grupo(Datos.SerieInterruptores).Calcular(d),
                    DatosEntradaCircuitoDerivadoVariador d => _motor.Variador(Datos.SerieInterruptores).Calcular(d),
                    _ => throw new ArgumentException($"Entrada sin calculadora: {typeof(T).Name}"),
                }, null);
            }
            catch (Exception ex)
            {
                r = (null, ex);
            }
            _recordados[llave] = r;
        }
        if (r.Error is { } error)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw(error);
        return r.Resultado!;
    }

    // ---- EL ALIMENTADOR, TAMBIÉN (I-188) ------------------------------------------------------------
    // Se calcula siete veces por recálculo: el resultado y la comparación de 1 a 6 conductores por fase
    // (I-162). Cambiar la longitud de un derivado o abrir un desplegable no le cambia la entrada. Su entrada
    // es de puros valores salvo la lista de corrientes por fase, que se compara elemento por elemento.

    private readonly Dictionary<LlaveDelAlimentador, (ResultadoAlimentador? Resultado, Exception? Error)> _alimentadoresRecordados = [];
    private const int MaximoAlimentadoresRecordados = 200;

    private readonly record struct LlaveDelAlimentador(SerieDeInterruptores Serie, DatosEntradaAlimentador Entrada, CorrientesComparables Corrientes);

    /// <summary>Las corrientes por fase, comparadas por su contenido: la lista se compararía por referencia.</summary>
    private readonly struct CorrientesComparables(IReadOnlyList<CorrienteDeFaseAlimentador>? lista) : IEquatable<CorrientesComparables>
    {
        private readonly IReadOnlyList<CorrienteDeFaseAlimentador>? _lista = lista;

        public bool Equals(CorrientesComparables otra) =>
            _lista is null ? otra._lista is null : otra._lista is not null && _lista.SequenceEqual(otra._lista);

        public override bool Equals(object? obj) => obj is CorrientesComparables otra && Equals(otra);

        public override int GetHashCode()
        {
            var h = new HashCode();
            foreach (var x in _lista ?? [])
                h.Add(x);
            return h.ToHashCode();
        }
    }

    private ResultadoAlimentador AlimentadorRecordado(DatosEntradaAlimentador entrada)
    {
        var llave = new LlaveDelAlimentador(Datos.SerieInterruptores, entrada with { CorrientesPorFase = null }, new CorrientesComparables(entrada.CorrientesPorFase));
        if (!_alimentadoresRecordados.TryGetValue(llave, out var r))
        {
            if (_alimentadoresRecordados.Count >= MaximoAlimentadoresRecordados)
                _alimentadoresRecordados.Clear();
            try
            {
                r = (_motor.Alimentador(Datos.SerieInterruptores).Calcular(entrada), null);
            }
            catch (Exception ex)
            {
                r = (null, ex);
            }
            _alimentadoresRecordados[llave] = r;
        }
        if (r.Error is { } error)
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw(error);
        return r.Resultado!;
    }

    private void CalcularCircuitos()
    {
        _desgloses.Clear();
        foreach (var c in _circuitos)
        {
            c.Limpiar();

            if (c.SinTipo)
            {
                c.Error = MensajeSinTipo;
                continue;
            }
            if (!c.TieneCarga || c.CanalizacionEfectiva is not { } canal)
                continue;

            // 3F-3H (delta) no tiene neutro: un circuito de 1 polo no tiene por dónde regresar. Antes se
            // calculaba a la tensión entre fases con fase y tierra (auditoría del 2026-09-29, P2-1).
            if (!SistemaConNeutro && c.Polos == 1)
            {
                c.Error = MensajeUnPoloSinNeutro;
                continue;
            }

            try
            {
                if (CalcularEquipo(c, canal))
                {
                    // I-182: una protección fijada para otro rango regresa al cálculo, y solo este circuito se
                    // vuelve a calcular. Ya en calculada no puede volver a pasar: una vez como máximo.
                    if (RegresarSiCambioElRango(c))
                    {
                        c.Limpiar();
                        CalcularEquipo(c, canal);
                        _regresaron[^1] = _regresaron[^1] with { CalculadaA = c.Resultado?.ProteccionA };
                    }
                    continue;
                }
                if (c.ErrorDeExclusividad() is { } exclusividad)
                {
                    c.Error = exclusividad;
                    continue;
                }
                if (c.TieneDesglose && ErrorDeMotoresEnDesglose(c) is { } errorDeMotores)
                {
                    c.Error = errorDeMotores;
                    continue;
                }

                c.Resultado = Recordado(new DatosEntradaCircuitoDerivadoNoMotor(
                    TipoCarga: TipoCargaDelCalculo(c),
                    CargaContinuaVA: c.ContinuaVA,
                    CargaNoContinuaVA: c.NoContinuaVA,
                    // NumeroFases es el del CIRCUITO —cuántas barras toca, o sea sus polos—, NO el
                    // del tablero. Pasar el del tablero fue un bug real de la primera versión de
                    // esta pantalla: un circuito de 1 polo con 720 VA daba 1.89 A en vez de 5.67 A,
                    // porque el motor repartía la carga entre tres fases.
                    NumeroFases: c.Polos,
                    TensionFaseNeutroV: Datos.TensionFaseNeutroV,
                    TensionFaseFaseV: Datos.TensionFaseFaseV,
                    LongitudM: c.LongitudM,
                    NumeroConductoresParalelo: 1,
                    // DE SU CANALIZACIÓN, no del tablero — I-39: los portadores según el tipo de
                    // canalización, la temperatura con la azotea al sol y la columna de la Tabla 9.
                    NumeroConductoresAgrupados: canal.Ajuste!.ConductoresParaElMotor,
                    TemperaturaAmbienteC: Datos.TemperaturaAmbienteC + canal.SumadorAzoteaC,
                    MaterialConductor: Datos.MaterialConductor,
                    MaterialCanalizacion: canal.MaterialParaTabla9,
                    // El del circuito: la nota 2 de la Tabla 9 usa «el ángulo del factor de potencia
                    // del circuito».
                    FactorPotencia: c.FactorPotencia,
                    CaidaTensionMaxPct: Datos.CaidaMaxDerivadoPct,
                    // SIN PISO PRÁCTICO DE CALIBRE -- va null a propósito, y es una diferencia
                    // deliberada con la versión de escritorio, que lo trae encendido por omisión
                    // (12 AWG en alumbrado, 10 en contactos). Lo quitó David el 2026-09-22 con un
                    // caso concreto: contactos salía en 10 AWG y un equipo con la misma carga por
                    // fase en 12, y esa diferencia no la produce ningún artículo de la norma, la
                    // producía el piso. Ver docs/decisiones/sin-piso-practico-de-calibre.md.
                    PisoPracticoCalibreMm2: null,
                    TipoAislamiento: Datos.TipoAislamiento,
                    Lugar: Datos.Lugar,
                    TerminalesMarcadas75C: Datos.TerminalesMarcadas75C,
                    // SIN MÍNIMO POR TIPO DE CARGA: solo el que exige la norma para el circuito — 210-11(c) según
                    // el uso; 210-19(a)(3) para una estufa doméstica de 8.75 kW o más (I-124).
                    ProteccionMinimaA: ProteccionMinima(c)?.Amperes,
                    ReferenciaProteccionMinima: ProteccionMinima(c)?.Referencia));
                c.AvisoAireDeHabitacion = AvisoDeHabitacion(c);
            }
            catch (Exception ex)
            {
                c.Error = ex.Message;
            }
        }
        foreach (var c in _circuitos)
        {
            c.Sobrecarga = SobrecargaRequerida.De(c);
            c.ReglasDeClase = ReglasDeClase(c);
        }
    }

    /// <summary>
    /// Un grupo, otro tablero, un variador, un motor o un equipo de A/C: los que tienen su propio cálculo.
    /// <c>false</c> si el circuito es de cargas de placa.
    /// </summary>
    private bool CalcularEquipo(CircuitoDelCuadro c, CanalizacionDelTablero canal)
    {
        if (c.EsGrupo)
            CalcularGrupo(c, canal);
        else if (c.Categoria == CategoriaDeCarga.Tablero)
            CalcularAlimentadorATablero(c, canal);
        else if (c.EsVariador)
            CalcularVariador(c, canal);
        else if (c.EsMotor)
            CalcularMotor(c, canal);
        else if (c.EsAireAcondicionado)
            CalcularAireAcondicionado(c, canal);
        else
            return false;
        return true;
    }

    /// <summary>
    /// <b>Las reglas de la clase del circuito</b> — I-124. Lo que la norma pide según alimente un solo
    /// equipo o dos o más salidas, ya redactado; las que no se cumplen van como aviso. No cambian el
    /// cálculo: el proyectista decide.
    /// <list type="bullet">
    /// <item>210-21(b)(1): el contacto de un circuito individual, de valor no menor que el circuito.</item>
    /// <item>Tabla 210-21(b)(3): el valor de los contactos de un circuito de dos o más.</item>
    /// <item>210-23: las cargas que admite cada tamaño de circuito de dos o más salidas.</item>
    /// <item>422-11(e): la protección de un solo aparato no operado por motor, sin valor marcado.</item>
    /// </list>
    /// </summary>
    private IReadOnlyList<ReglaDeClase> ReglasDeClase(CircuitoDelCuadro c)
    {
        if (c.Resultado is not { } r || c.ClaseDelCircuito is not { } clase)
            return [];
        // I-160: el tope de 1 polo de la familia vale para cualquier clase, también para un subtablero.
        var unPolo = ReglaDeUnPolo(c, r.ProteccionA);
        if (clase == ClaseDeCircuito.Alimentador || c.EsGrupoDeMotores)
            return unPolo is null ? [] : [unPolo];
        var reglas = new List<ReglaDeClase>(ReglasDe210_8(c, r.ProteccionA));
        if (unPolo is not null)
            reglas.Add(unPolo);
        if (ReglaDe210_6(c) is { } tension)
            reglas.Add(tension);
        var tipos = c.TiposDeSusCargas;
        var proteccion = r.ProteccionA;
        var conContactos = tipos.Contains(CategoriaDeCarga.Contactos);
        var divisor = TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
        reglas.AddRange(ReglasDe600_5(c, proteccion));

        if (clase == ClaseDeCircuito.Individual)
        {
            if (conContactos)
                reglas.Add(new("210-21(b)(1)", $"Un contacto sencillo en su circuito individual es de valor no menor que el circuito: {proteccion:N0} A.", false));
            // 422-11(e): un solo aparato no operado por motor, sin valor de protección marcado.
            var aparato = c.TieneDesglose ? c.Cargas.SingleOrDefault() : null;
            var noMotor = c.Categoria == CategoriaDeCarga.Equipo && !c.EsDeMotor && (aparato is null ? !c.TieneDesglose : !aparato.EsMaquina && c.TipoDe(aparato) == CategoriaDeCarga.Equipo);
            if (noMotor)
            {
                var corriente = c.CargaInstaladaVA / divisor;
                var maximo = corriente <= 13.3m ? 20m : TamanoEstandar(1.5m * corriente);
                if (proteccion > maximo)
                    reglas.Add(new("422-11(e)", $"Un solo aparato no operado por motor, de {corriente:N2} A: sin valor marcado, su protección no pasa de {maximo:N0} A " +
                        $"({(corriente <= 13.3m ? "20 A hasta 13.30 A" : "150 % de su corriente, o el tamaño siguiente")}); la del circuito es de {proteccion:N0} A. Revisar la placa.", true));
            }
            return reglas;
        }

        // Dos o más salidas: uso general o para aparatos.
        // 210-3: los que no son individuales van de 15 a 50 A. La excepción —más de 50 A para cargas que
        // no son de alumbrado— es solo de instalaciones industriales con mantenimiento y supervisión
        // calificados, y la captura no lo sabe: se avisa siempre y se nombra — auditoría del 2026-10-02, N-2.
        if (proteccion > 50m)
            reglas.Add(new("210-3", $"Circuito de {SalidasDe(c)} de {proteccion:N0} A: 210-3 limita a 50 A los circuitos derivados que no son individuales. " +
                "Separar en circuitos individuales. Solo en una instalación industrial con mantenimiento y supervisión calificados se permite más de 50 A " +
                "para cargas que no son de alumbrado (210-3, Excepción).", true));
        if (conContactos)
        {
            var fuera = proteccion > 20m && c.UsoEfectivo == UsoDeContactos.General && ContactosDeUsoGeneral(c);
            reglas.Add(fuera
                ? new("Tabla 210-21(b)(3)", $"Los contactos de uso general (15 o 20 A) no van en un circuito de {proteccion:N0} A: ahí son {ValorDeContactos(proteccion)}. Partir los contactos en circuitos de 15 o 20 A.", true)
                : new("Tabla 210-21(b)(3)", $"Los contactos de un circuito de {proteccion:N0} A: {ValorDeContactos(proteccion)}.", false));
        }
        var alumbradoComun = tipos.Contains(CategoriaDeCarga.Alumbrado) && (!c.TieneDesglose
            || c.Cargas.Any(a => c.TipoDe(a) == CategoriaDeCarga.Alumbrado && a.Subtipo != SubtipoDeCarga.PortalamparasPesado));
        if (proteccion <= 20m)
        {
            // 210-23(a)(2): el equipo fijo que no es luminaria, con alumbrado o equipo con clavija, no pasa del 50 %.
            var equipoFijoA = c.Porciones.Where(p => p.Tipo is CategoriaDeCarga.Equipo or CategoriaDeCarga.CalefaccionFija).Sum(p => p.TotalVA) / divisor;
            if (equipoFijoA > 0m && (tipos.Contains(CategoriaDeCarga.Alumbrado) || conContactos) && equipoFijoA > 0.5m * proteccion)
                reglas.Add(new("210-23(a)(2)", $"El equipo fijo ({equipoFijoA:N2} A), junto con alumbrado o contactos, no pasa del 50 % del circuito de {proteccion:N0} A ({0.5m * proteccion:N2} A). Llevarlo a un circuito propio.", true));
        }
        else if (proteccion <= 30m)
        {
            if (alumbradoComun || (tipos.Contains(CategoriaDeCarga.Alumbrado) && Datos.Inmueble.EsVivienda()))
                reglas.Add(new("210-23(b)", $"Un circuito de {proteccion:N0} A con alumbrado solo alimenta portalámparas de servicio pesado, y fuera de vivienda. Partir el alumbrado en circuitos de 15 o 20 A.", true));
        }
        else if (proteccion <= 50m)
        {
            if (tipos.Contains(CategoriaDeCarga.Alumbrado) && (alumbradoComun || Datos.Inmueble.EsVivienda()))
                reglas.Add(new("210-23(c)", $"Un circuito de {proteccion:N0} A alimenta equipo de cocción fijo, portalámparas de servicio pesado fuera de vivienda o calefacción por infrarrojo; no alumbrado común.", true));
        }
        else if (tipos.Contains(CategoriaDeCarga.Alumbrado))
            reglas.Add(new("210-23(d)", $"Un circuito de más de 50 A ({proteccion:N0} A) solo alimenta salidas que no son de alumbrado.", true));
        return reglas;
    }

    /// <summary>
    /// <b>Protección de las personas con ICFT</b> — 210-8 (auditoría del 2026-09-29, P3-4): contactos
    /// monofásicos de 15 y 20 A en los lugares que la norma nombra. De los que la captura distingue: el
    /// baño, en vivienda (a)(1) y fuera de ella (b)(1); los de la cubierta del mueble de cocina, en vivienda
    /// (a)(6). Es un requisito, no un incumplimiento: nota, sin aviso. En vivienda popular de hasta 60 m²
    /// se permiten contactos normales con el ICFT al principio del circuito. (210-12, el ICFA, en la NOM es
    /// «se podrán proteger»: no se exige.)
    /// </summary>
    private IEnumerable<ReglaDeClase> ReglasDe210_8(CircuitoDelCuadro c, decimal proteccionA)
    {
        if (c.Polos != 1 || proteccionA > 20m)
            yield break;
        HashSet<SubtipoDeCarga?> subtipos = c.TieneDesglose ? [.. c.Cargas.Select(a => a.Subtipo)] : [SubtiposDeCarga.DeUso(c.Uso)];
        var vivienda = Datos.Inmueble.EsVivienda();
        var popular = Datos.Inmueble == TipoDeInmueble.ViviendaPopular
            ? " En vivienda popular de hasta 60 m² se permiten contactos normales con el ICFT al principio del circuito."
            : "";
        if (subtipos.Contains(SubtipoDeCarga.ContactoBano))
            yield return new(vivienda ? "210-8(a)(1)" : "210-8(b)(1)",
                "Los contactos del baño requieren protección de las personas con interruptor de circuito por falla a tierra (ICFT)." + popular, false);
        if (vivienda && subtipos.Contains(SubtipoDeCarga.ContactoAparatosPequenos))
            yield return new("210-8(a)(6)",
                "Los contactos en la cubierta del mueble de cocina requieren protección de las personas con interruptor de circuito por falla a tierra (ICFT)." + popular, false);
    }

    /// <summary>
    /// <b>El circuito de anuncios</b> — 600-5 (I-156): la salida exigida no comparte circuito con otras
    /// cargas (a); y el circuito no pasa de 20 A, o de 30 A si es de tubos de neón (b). El mínimo de 20 A y
    /// la carga continua ya los aplica el cálculo; esto solo avisa lo que el proyectista tiene que resolver.
    /// </summary>
    private static IEnumerable<ReglaDeClase> ReglasDe600_5(CircuitoDelCuadro c, decimal proteccionA)
    {
        if (!c.TieneDesglose || !c.Cargas.Any(a => a.Subtipo == SubtipoDeCarga.Anuncios))
            yield break;
        if (c.Cargas.Any(a => a.Subtipo != SubtipoDeCarga.Anuncios))
            yield return new("600-5(a)",
                "La salida para anuncios va en un circuito de 20 A como mínimo que no alimenta otras cargas. Llevar las demás a otro circuito.", true);
        if (proteccionA > 20m)
            yield return new("600-5(b)(2)",
                $"Un circuito de anuncios no pasa de 20 A; este es de {proteccionA:N0} A. Solo los de tubos de neón llegan a 30 A — 600-5(b)(1). Partir los anuncios en circuitos de 20 A.", true);
    }

    /// <summary>
    /// <b>Contactos a 277 V</b> — 210-6 (auditoría del 2026-09-29, P3, riesgo 6). En 480Y/277 V un circuito de
    /// contactos de 1 polo queda a 277 V a tierra: 210-6(c)(6) lo permite para equipo de utilización con
    /// cordón y clavija, pero es poco común; se anota. En vivienda, 210-6(a)(2) limita a 120 V entre
    /// conductores las cargas con cordón y clavija de 1440 VA o menos: aviso.
    /// </summary>
    private ReglaDeClase? ReglaDe210_6(CircuitoDelCuadro c)
    {
        if (c.Polos != 1 || !c.TiposDeSusCargas.Contains(CategoriaDeCarga.Contactos)
            // 277 V nominal: 480 / √3 = 277.13 V.
            || Datos.TensionFaseNeutroV is var tension && (tension <= 150m || Math.Round(tension) > 277m))
            return null;
        return Datos.Inmueble.EsVivienda()
            ? new("210-6(a)(2)", $"En vivienda, los contactos para cargas con cordón y clavija de 1440 VA o menos no pasan de 120 V entre conductores; este circuito queda a {tension:N0} V.", true)
            : new("210-6(c)(6)", $"Contactos a {tension:N0} V a tierra: se permiten para equipo de utilización con cordón y clavija de esa tensión; no son contactos de uso general de 127 V.", false);
    }

    /// <summary>
    /// <b>Un interruptor de 1 polo más grande que los de su familia</b> — I-160 (auditoría del 2026-10-02):
    /// un subtablero de 225 A en 1 polo calcula bien, pero ese interruptor no existe. En centro de carga,
    /// arriba de 70 A; con la lista completa, arriba de 125 A. En riel DIN ya lo dice el aviso del tablero
    /// (más de 125 A). Criterio del proyectista, no de la NOM: aviso.
    /// </summary>
    private ReglaDeClase? ReglaDeUnPolo(CircuitoDelCuadro c, decimal proteccionA)
    {
        var serie = Datos.SerieInterruptores;
        if (c.Polos != 1 || serie == SerieDeInterruptores.RielDinIec || proteccionA <= serie.MaximoUnPolo())
            return null;
        var familia = serie == SerieDeInterruptores.CentroDeCargaNema ? "en centro de carga" : "en centro de carga y en riel DIN";
        return new("serie de interruptores",
            $"Interruptor de 1 polo de {proteccionA:N0} A: {familia} los de 1 polo llegan, por lo común, a {serie.MaximoUnPolo():N0} A. " +
            "Pasar el circuito a 2 o 3 polos, o confirmar que la familia que se instala tiene ese tamaño. Criterio del proyectista, no de la NOM.", true);
    }

    /// <summary>«3 salidas», o «varias salidas» si es carga total sin desglose.</summary>
    private static string SalidasDe(CircuitoDelCuadro c) => c.Salidas is int n and > 1 ? $"{n} salidas" : "varias salidas";

    /// <summary>Tabla 210-21(b)(3): el valor de los contactos según el del circuito.</summary>
    private static string ValorDeContactos(decimal circuitoA) => circuitoA switch
    {
        <= 15m => "de 15 A como máximo",
        <= 20m => "de 15 o 20 A",
        <= 30m => "de 30 A",
        <= 40m => "de 40 o 50 A",
        <= 50m => "de 50 A",
        _ => "la tabla llega a 50 A: un circuito mayor alimenta un contacto individual",
    };

    /// <summary>Contactos de uso general (15 o 20 A): el renglón de contactos, o salidas de uso general en el desplegable.</summary>
    private static bool ContactosDeUsoGeneral(CircuitoDelCuadro c) =>
        !c.TieneDesglose || c.Cargas.Any(a => c.TipoDe(a) == CategoriaDeCarga.Contactos && a.Subtipo is null or SubtipoDeCarga.ContactoUsoGeneral or SubtipoDeCarga.ContactoMultiple);

    /// <summary>
    /// La protección mínima que la norma pide para el circuito: 20 A en los de vivienda de 210-11(c); 40 A
    /// si alimenta estufas domésticas de 8.75 kW o más — 210-19(a)(3) (I-124); 20 A en el de anuncios —
    /// 600-5(a) (I-156). <c>null</c> sin mínimo.
    /// </summary>
    private (decimal Amperes, string Referencia)? ProteccionMinima(CircuitoDelCuadro c)
    {
        var coccion = Datos.Inmueble.EsVivienda()
            ? c.Cargas.Where(a => c.TieneDesglose && a.Subtipo == SubtipoDeCarga.Coccion).Sum(a => a.TotalVA)
            : 0m;
        if (coccion >= 8750m)
            return (40m, "210-19(a)(3)");
        // 600-5(a): la salida para anuncios, en un circuito «de cuando menos 20 amperes». Como el mínimo de
        // 1200 VA de 220-14(f), cada circuito de anuncios se toma como el exigido.
        if (c.TieneDesglose && c.Cargas.Any(a => a.Subtipo == SubtipoDeCarga.Anuncios))
            return (20m, "600-5(a)");
        return c.UsoEfectivo.ReferenciaProteccionMinima() is { } referencia
            ? (UsosDeContactos.ProteccionMinimaViviendaA, referencia)
            : null;
    }

    /// <summary>
    /// El tipo con el que calcula el derivado no-motor — I-123: con algún contacto, Contactos (sin 240-4(b));
    /// solo alumbrado, Alumbrado; si no, Equipo. Sin desglose, el del renglón, como siempre.
    ///
    /// <para>
    /// <b>240-4(b)(1) por lo que alimenta</b> — I-124: el tamaño siguiente sobre la ampacidad se niega al
    /// circuito «de salidas múltiples que alimenta a contactos para cargas portátiles». Un circuito
    /// individual de contactos —el del refrigerador— no lo es: calcula como Equipo.
    /// </para>
    /// </summary>
    private static TipoCarga TipoCargaDelCalculo(CircuitoDelCuadro c)
    {
        if (c.ClaseDelCircuito == ClaseDeCircuito.Individual && c.TiposDeSusCargas.Contains(CategoriaDeCarga.Contactos))
            return TipoCarga.Equipo;
        if (!c.TieneDesglose)
            return c.Tipo;
        var tipos = c.TiposDeSusCargas;
        return tipos.Contains(CategoriaDeCarga.Contactos) ? TipoCarga.Contactos
            : tipos.All(t => t == CategoriaDeCarga.Alumbrado) ? TipoCarga.Alumbrado
            : TipoCarga.Equipo;
    }

    /// <summary>
    /// <b>El alimentador a otro tablero</b> — I-125: la carga calculada del otro tablero, la no continua
    /// más el 125 % de la continua — 215-2(a)(1), 215-3. Con el cálculo del derivado citado con el 215
    /// (<see cref="ClaseDeTramo.Alimentador"/>), la caída máxima de un alimentador y sin mínimo por uso.
    /// </summary>
    private void CalcularAlimentadorATablero(CircuitoDelCuadro c, CanalizacionDelTablero canal) =>
        c.Resultado = Recordado(new DatosEntradaCircuitoDerivadoNoMotor(
            TipoCarga: TipoCarga.Equipo,
            CargaContinuaVA: c.ContinuaVA,
            CargaNoContinuaVA: c.NoContinuaVA,
            NumeroFases: c.Polos,
            TensionFaseNeutroV: Datos.TensionFaseNeutroV,
            TensionFaseFaseV: Datos.TensionFaseFaseV,
            LongitudM: c.LongitudM,
            NumeroConductoresParalelo: 1,
            NumeroConductoresAgrupados: canal.Ajuste!.ConductoresParaElMotor,
            TemperaturaAmbienteC: Datos.TemperaturaAmbienteC + canal.SumadorAzoteaC,
            MaterialConductor: Datos.MaterialConductor,
            MaterialCanalizacion: canal.MaterialParaTabla9,
            FactorPotencia: c.FactorPotencia,
            CaidaTensionMaxPct: Datos.CaidaMaxAlimentadorPct,
            PisoPracticoCalibreMm2: null,
            TipoAislamiento: Datos.TipoAislamiento,
            Lugar: Datos.Lugar,
            TerminalesMarcadas75C: Datos.TerminalesMarcadas75C,
            Tramo: ClaseDeTramo.Alimentador));

    /// <summary>
    /// <b>El derivado de un motor</b> — I-15, Art. 430: FLC de tabla, conductor al 125 % (430-22),
    /// protección de la Tabla 430-52 con interruptor de tiempo inverso. Las condiciones (canalización,
    /// temperatura, aislamiento, terminales) son las mismas que las de cualquier renglón.
    /// </summary>
    private void CalcularMotor(CircuitoDelCuadro c, CanalizacionDelTablero canal)
    {
        var tension = TensionDelMotor(c);
        var enAmperes = c.CapturaMotor == CapturaDeMotor.Amperes;
        if (c.FlcA <= 0m)
        {
            c.Error = enAmperes
                ? MotoresEnHp.SinFilaEnAmperes(_motor.FlcMotor, c.CorrientePlacaA, c.Polos, tension)
                : MotoresEnHp.SinFila(_motor.FlcMotor, c.Hp!.Value, c.Polos, tension);
            return;
        }

        // Servicio no continuo — 430-22(e), I-120: el % de la tabla sobre la corriente de placa.
        ServicioNoContinuo? servicio = null;
        if (c.TieneServicioNoContinuo)
        {
            var placa = c.CorrienteDePlacaDelServicioA;
            if (placa <= 0m)
            {
                c.Error = "430-22(e): el conductor de un motor de servicio no continuo va sobre la corriente de placa del motor. " +
                          "Captúrala en el detalle del circuito (la flecha junto a la descripción).";
                return;
            }
            if (_motor.ServicioMotor.Porcentaje(c.Servicio!.Value, c.EspecificacionServicio) is not { } porcentaje)
            {
                c.Error = "Tabla 430-22(e): un motor de servicio de corta duración no se especifica para funcionamiento continuo. " +
                          "Elige para cuántos minutos está especificado.";
                return;
            }
            servicio = new ServicioNoContinuo(c.Servicio.Value, c.EspecificacionServicio, porcentaje, placa);
            c.CorrienteDeServicioA = porcentaje * placa / 100m;
        }

        // En amperes, los caballos interpolados (430-6(a)(1)) van solo a la cita; la FLC es la corriente.
        var entrada = DatosDeUnMotor(c, canal, enAmperes ? c.MotorEnAmperes!.Hp : c.Hp!.Value, enAmperes ? c.FlcA : null) with { Servicio = servicio };
        c.EntradaDeLaProteccion = entrada;
        c.Resultado = Recordado(entrada);
    }

    private DatosEntradaCircuitoDerivadoMotor DatosDeUnMotor(CircuitoDelCuadro c, CanalizacionDelTablero canal, decimal hp, decimal? flcMarcadaEnAmperesA)
    {
        var tension = TensionDelMotor(c);
        return new DatosEntradaCircuitoDerivadoMotor(
            Hp: hp,
            FlcMarcadaEnAmperesA: flcMarcadaEnAmperesA,
            TipoAlimentacion: MotoresEnHp.Alimentacion(c.Polos),
            TipoMotor: MotoresEnHp.TipoDeMotor(c.Polos),
            // Lo que se monta en un tablero de derivados: interruptor automático de tiempo inverso.
            TipoDispositivoProteccion: TipoDispositivoProteccionMotor.InterruptorTiempoInverso,
            TensionNominalMotorV: tension,
            // PARA LA CAÍDA, LA TENSIÓN A LA QUE ESTÁ CONECTADO. El motor calcula la de un monofásico
            // con «la fase-neutro», y un monofásico de 2 polos está entre fases: 220 V, no 127.
            TensionFaseNeutroV: c.Polos == 1 ? Datos.TensionFaseNeutroV : Datos.TensionFaseFaseV,
            TensionFaseFaseV: Datos.TensionFaseFaseV,
            LongitudM: c.LongitudM,
            NumeroConductoresParalelo: 1,
            NumeroConductoresAgrupados: canal.Ajuste!.ConductoresParaElMotor,
            TemperaturaAmbienteC: Datos.TemperaturaAmbienteC + canal.SumadorAzoteaC,
            MaterialConductor: Datos.MaterialConductor,
            MaterialCanalizacion: canal.MaterialParaTabla9,
            FactorPotencia: c.FactorPotencia,
            CaidaTensionMaxPct: Datos.CaidaMaxDerivadoPct,
            PisoPracticoCalibreMm2: null,
            TipoAislamiento: Datos.TipoAislamiento,
            Lugar: Datos.Lugar,
            TerminalesMarcadas75C: Datos.TerminalesMarcadas75C,
            NoArrancaConLaTabla: c.NoArrancaConLaTabla,
            MotorYArrancadorMarcados75C: c.MotorYArrancadorMarcados75C,
            // M-20: la calculada, por los HP — la mayor que protege al conductor hasta 1 HP (criterio, I-183).
            CriterioProteccion: c.CriterioProteccion.ParaElCalculo(hp, c.NoArrancaConLaTabla),
            ProteccionElegidaA: c.CriterioProteccion == CriterioDeProteccion.Manual ? c.ProteccionElegidaA : null);
    }

    /// <summary>
    /// <b>Varios motores, o motores y otras cargas, en un circuito</b> — I-115: conductor por 430-24 y
    /// protección por 430-53(c)(4) (<see cref="CalculadoraCircuitoDerivadoGrupo"/>). Todas las máquinas
    /// van a la tensión y los polos del circuito.
    ///
    /// <para>
    /// Un grupo de un solo motor y nada más es un motor: se calcula como tal, con el redondeo hacia
    /// arriba de 430-52(c)(1) Excepción 1, que 430-53(c)(4) no trae.
    /// </para>
    /// </summary>
    private void CalcularGrupo(CircuitoDelCuadro c, CanalizacionDelTablero canal)
    {
        if (c.Cargas.FirstOrDefault(a => a.EsMaquina && a.Error is not null) is { } malo)
        {
            c.Error = $"{NombreDeMaquina(c, malo)}: {malo.Error}";
            return;
        }

        var maquinas = c.Cargas.Where(a => a.EsMaquina && a.CorrienteUnitariaA > 0m).ToList();
        var otras = c.Cargas.Where(a => !a.EsMaquina && a.TotalVA > 0m).ToList();
        if (maquinas.Count == 0)
        {
            c.Error = "430-53: el grupo no tiene motores. Agrégalos en el desplegable (la flecha junto a la descripción), o cambia el tipo del circuito.";
            return;
        }

        if (maquinas is [{ Clase: ClaseDeAparato.Motor, Cantidad: 1 } solo] && otras.Count == 0)
        {
            var entrada = DatosDeUnMotor(c, canal, solo.MotorEnAmperes?.Hp ?? solo.Hp!.Value,
                solo.CapturaMotor == CapturaDeMotor.Amperes ? solo.CorrienteUnitariaA : null);
            c.EntradaDeLaProteccion = entrada;
            c.Resultado = Recordado(entrada);
            return;
        }

        var divisor = TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
        var tension = TensionDelMotor(c);
        c.Resultado = Recordado(new DatosEntradaCircuitoDerivadoGrupo(
            Miembros: new MiembrosDelGrupo(maquinas.Select(a => new MiembroDelGrupo(
                NombreDeMaquina(c, a),
                a.Clase switch
                {
                    ClaseDeAparato.Motocompresor => ClaseDeMiembro.Motocompresor,
                    ClaseDeAparato.Variador => ClaseDeMiembro.Variador,
                    _ => ClaseDeMiembro.Motor,
                },
                a.Cantidad,
                a.CorrienteUnitariaA,
                OrigenDeLaCorriente(a, c.Polos, tension),
                a.Clase == ClaseDeAparato.Motor ? a.MotorEnAmperes?.Hp ?? a.Hp : null,
                a.Clase == ClaseDeAparato.Variador && a.ProteccionMaximaA > 0m ? a.ProteccionMaximaA : null))),
            OtrasContinuaA: c.ContinuaVA / divisor,
            OtrasNoContinuaA: c.NoContinuaVA / divisor,
            MayorOtraCargaA: otras.Count == 0 ? 0m : otras.Max(a => a.TotalVA / a.Cantidad) / divisor,
            TipoMotor: MotoresEnHp.TipoDeMotor(c.Polos),
            TipoDispositivoProteccion: TipoDispositivoProteccionMotor.InterruptorTiempoInverso,
            // El motocompresor mayor no arranca al 175 %: 225 % — 440-22(a), 440-22(b)(1) (I-116).
            RequiereArranque: c.EsAireAcondicionado && c.ArranqueAl225,
            // Como el derivado de un motor y el de A/C: 2 polos es monofásico entre fases.
            NumeroFases: c.Polos == 3 ? 3 : 1,
            TensionFaseNeutroV: c.Polos == 1 ? Datos.TensionFaseNeutroV : Datos.TensionFaseFaseV,
            TensionFaseFaseV: Datos.TensionFaseFaseV,
            LongitudM: c.LongitudM,
            NumeroConductoresParalelo: 1,
            NumeroConductoresAgrupados: canal.Ajuste!.ConductoresParaElMotor,
            TemperaturaAmbienteC: Datos.TemperaturaAmbienteC + canal.SumadorAzoteaC,
            MaterialConductor: Datos.MaterialConductor,
            MaterialCanalizacion: canal.MaterialParaTabla9,
            FactorPotencia: c.FactorPotencia,
            CaidaTensionMaxPct: Datos.CaidaMaxDerivadoPct,
            PisoPracticoCalibreMm2: null,
            TipoAislamiento: Datos.TipoAislamiento,
            Lugar: Datos.Lugar,
            TerminalesMarcadas75C: Datos.TerminalesMarcadas75C));
    }

    /// <summary>
    /// <b>Un motor con variador</b> — I-119, 430 Parte J: la corriente de entrada del variador al 125 %
    /// (430-122(a)) y la protección máxima de su fabricante (110-3(b)).
    /// </summary>
    private void CalcularVariador(CircuitoDelCuadro c, CanalizacionDelTablero canal)
    {
        // M-23 (AM-7): con bypass, el motor con su FLC de tabla a la tensión del circuito y su Tabla 430-52.
        DesviacionDelVariador? bypass = null;
        if (c.VariadorConBypass)
        {
            var hp = c.HpMotorDelVariador ?? 0m;
            var flc = hp > 0m ? FlcDe(hp, c.Polos) : 0m;
            if (flc <= 0m)
            {
                c.Error = hp > 0m
                    ? $"430-122(b): la tabla no trae un motor de {MotoresEnHp.Texto(hp)} HP {AlimentacionDelMotor(c.Polos)}. Escoge los HP del motor que mueve el variador."
                    : "430-122(b): con bypass el conductor lleva también el 125 % de la corriente del motor. Escoge los HP del motor que mueve " +
                      "el variador, en el desplegable (la flecha junto a la descripción).";
                return;
            }
            var porcentaje = PorcentajeProteccionMotor(c);
            bypass = new DesviacionDelVariador(hp, flc, porcentaje, CalculadoraCircuitoDerivadoMotor.ProteccionDeLaTabla430_52(
                new ProteccionEstandarDeLaSerie(_motor.ProteccionEstandar, Datos.SerieInterruptores), flc * porcentaje / 100m));
        }
        var entrada = new DatosEntradaCircuitoDerivadoVariador(
            CorrienteEntradaA: c.CorrienteEntradaVariadorA,
            ProteccionMaximaA: c.ProteccionMaximaVariadorA,
            NumeroFases: c.Polos == 3 ? 3 : 1,
            TensionFaseNeutroV: c.Polos == 1 ? Datos.TensionFaseNeutroV : Datos.TensionFaseFaseV,
            TensionFaseFaseV: Datos.TensionFaseFaseV,
            LongitudM: c.LongitudM,
            NumeroConductoresParalelo: 1,
            NumeroConductoresAgrupados: canal.Ajuste!.ConductoresParaElMotor,
            TemperaturaAmbienteC: Datos.TemperaturaAmbienteC + canal.SumadorAzoteaC,
            MaterialConductor: Datos.MaterialConductor,
            MaterialCanalizacion: canal.MaterialParaTabla9,
            FactorPotencia: c.FactorPotencia,
            CaidaTensionMaxPct: Datos.CaidaMaxDerivadoPct,
            TipoAislamiento: Datos.TipoAislamiento,
            Lugar: Datos.Lugar,
            TerminalesMarcadas75C: Datos.TerminalesMarcadas75C,
            // M-20, fase 2: el automático de un variador es la máxima del fabricante.
            CriterioProteccion: c.CriterioProteccion.ParaElCalculo(),
            ProteccionElegidaA: c.CriterioProteccion == CriterioDeProteccion.Manual ? c.ProteccionElegidaA : null,
            Bypass: bypass);
        c.EntradaDeLaProteccion = entrada;
        c.Resultado = Recordado(entrada);
    }

    /// <summary>«Extractor», o «Motor 2» si no se describió: el número de su renglón en el desglose.</summary>
    public static string NombreDeMaquina(CircuitoDelCuadro c, CargaDelCircuito a) =>
        !string.IsNullOrWhiteSpace(a.Descripcion) ? a.Descripcion.Trim()
        : $"{a.Clase switch { ClaseDeAparato.Motocompresor => "Motocompresor", ClaseDeAparato.Variador => "Variador", _ => "Motor" }} {c.Cargas.IndexOf(a) + 1}";

    /// <summary>
    /// De dónde sale la corriente de una máquina del grupo, para la cita: «½ HP, Tabla 430-248, columna
    /// de 127 V», «marcado en amperes: …, 430-6(a)(1)» o la placa de un motocompresor.
    /// </summary>
    public string OrigenDeLaCorriente(CargaDelCircuito a, int polos) =>
        OrigenDeLaCorriente(a, polos, MotoresEnHp.Tension(polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV));

    private string OrigenDeLaCorriente(CargaDelCircuito a, int polos, decimal tension) =>
        a.Clase == ClaseDeAparato.Variador
            ? $"corriente nominal de entrada del variador — 430-122(a)" +
              (a.ProteccionMaximaA > 0m ? $"; protección máxima del fabricante {a.ProteccionMaximaA:0.##} A — 110-3(b)" : "")
        : a.Clase == ClaseDeAparato.Motocompresor
            ? a.CorrienteSeleccionA > a.CorrientePlacaA
                ? $"corriente de selección del circuito derivado de la placa (mayor que la de carga nominal, {a.CorrientePlacaA:0.##} A) — 440-6(a) Exc. 1"
                : "corriente de carga nominal de la placa — 440-6(a)"
            : a.MotorEnAmperes is { } m
                ? $"marcado en amperes, {m.Hp:0.##} HP por interpolación ({MotoresEnHp.Interpolacion(m, polos)}) — 430-6(a)(1)"
                : $"{MotoresEnHp.Texto(a.Hp ?? 0m)} HP, {MotoresEnHp.Fuente(polos, tension)}";

    /// <summary>
    /// <b>El derivado de un equipo de A/C o refrigeración</b> — I-74, Art. 440: con la corriente de la
    /// placa (440-6(a), 440-22(a), 440-32) o con su ampacidad mínima y su protección máxima (440-4(b)).
    /// Las condiciones del tramo, como en cualquier renglón.
    /// </summary>
    private void CalcularAireAcondicionado(CircuitoDelCuadro c, CanalizacionDelTablero canal)
    {
        var porPlaca = c.PlacaAire == PlacaDeAireAcondicionado.AmpacidadYProteccion;
        var deHabitacion = c.PlacaAire == PlacaDeAireAcondicionado.Habitacion;
        var entrada = new DatosEntradaCircuitoDerivado440(
            // Un equipo de 2 polos es monofásico entre fases; el de 3, trifásico.
            NumeroFases: c.Polos == 3 ? 3 : 1,
            TensionFaseNeutroV: c.Polos == 1 ? Datos.TensionFaseNeutroV : Datos.TensionFaseFaseV,
            TensionFaseFaseV: Datos.TensionFaseFaseV,
            LongitudM: c.LongitudM,
            NumeroConductoresParalelo: 1,
            NumeroConductoresAgrupados: canal.Ajuste!.ConductoresParaElMotor,
            TemperaturaAmbienteC: Datos.TemperaturaAmbienteC + canal.SumadorAzoteaC,
            MaterialConductor: Datos.MaterialConductor,
            MaterialCanalizacion: canal.MaterialParaTabla9,
            FactorPotencia: c.FactorPotencia,
            CaidaTensionMaxPct: Datos.CaidaMaxDerivadoPct,
            TipoAislamiento: Datos.TipoAislamiento,
            Lugar: Datos.Lugar,
            TerminalesMarcadas75C: Datos.TerminalesMarcadas75C,
            CorrienteNominalPlacaA: porPlaca || deHabitacion ? null : c.CorrientePlacaA,
            CorrienteSeleccionCircuitoA: porPlaca || deHabitacion ? null : c.CorrienteSeleccionA,
            RequiereArranque: !porPlaca && !deHabitacion && c.ArranqueAl225,
            AmpacidadMinimaPlacaA: porPlaca ? c.AmpacidadMinimaA : null,
            ProteccionMaximaPlacaA: porPlaca ? c.ProteccionMaximaA : null,
            // 440 Parte G — I-117: una sola unidad de motor, en su circuito.
            CorrienteTotalHabitacionA: deHabitacion ? c.CorrientePlacaA : null,
            // M-20, fase 2: el automático de un equipo de A/C es el máximo (440-22(a) o la placa).
            CriterioProteccion: c.CriterioProteccion.ParaElCalculo(),
            ProteccionElegidaA: c.CriterioProteccion == CriterioDeProteccion.Manual ? c.ProteccionElegidaA : null);
        c.EntradaDeLaProteccion = deHabitacion ? null : entrada;
        c.Resultado = Recordado(entrada);
    }

    /// <summary>
    /// Arma los bloques del interior. Cada interruptor empieza en su espacio y abarca sus polos
    /// hacia abajo por la MISMA columna, que es como se apilan los polos físicamente.
    /// </summary>
    private void DibujarGabinete()
    {
        Gabinete =
        [
            .. _circuitos
                .Where(c => !c.EsContinuacion)
                .Select(c =>
                {
                    if (c.EsDelPrincipal)
                        return new BloqueDelGabinete(
                            Circuito: c,
                            Fila: UnaSolaBarra ? c.Espacio : (c.Espacio + 1) / 2,
                            Columna: UnaSolaBarra || c.Espacio % 2 == 1 ? 1 : 2,
                            Espacios: EspaciosDelPrincipal.Count,
                            Numeros: string.Join("-", EspaciosDelPrincipal),
                            Barras: DistribucionBarras.FasesQueOcupa(c.Espacio, EspaciosDelPrincipal.Count, Datos.Sistema),
                            EsPrincipal: true);

                    var espacios = DistribucionBarras.EspaciosQueOcupa(c.Espacio, c.Polos);

                    return new BloqueDelGabinete(
                        Circuito: c,
                        // El espacio 1 y el 2 están en el primer renglón; el 3 y el 4, en el segundo.
                        // Con una sola barra, una sola columna en orden (I-54).
                        Fila: UnaSolaBarra ? c.Espacio : (c.Espacio + 1) / 2,
                        Columna: UnaSolaBarra || c.Espacio % 2 == 1 ? 1 : 2,
                        Espacios: c.Polos,
                        Numeros: string.Join("-", espacios),
                        Barras: c.Fases);
                })
        ];
    }

    private void CalcularResumen()
    {
        var continua = _circuitos.Where(c => c.TieneCarga).Sum(c => c.ContinuaVA);
        var noContinua = _circuitos.Where(c => c.TieneCarga).Sum(c => c.NoContinuaVA);

        // El reparto por fase en VA, como las columnas BB/BC/BD del Excel: la carga del circuito
        // entre las barras que toca. Es el balanceo que se imprime.
        var porFase = Datos.Barras.ToDictionary(b => b, _ => 0m);
        foreach (var c in _circuitos.Where(c => c.TieneCarga))
            foreach (var fase in c.Fases)
                if (porFase.ContainsKey(fase))
                    porFase[fase] += c.CargaPorFaseVA;

        // El porcentaje, en cambio, sale del motor y se mide en CORRIENTE, no en VA: en un
        // interruptor de 3 polos de 20 A circulan 20 A por cada línea, no un tercio por cada una.
        // Es la misma fórmula (max-min)/max del Excel con el insumo correcto.
        var desbalanceo = CalculadoraDesbalanceo.Porcentaje(
            [.. _circuitos
                .Where(c => c.Resultado is not null)
                .Select(c => new CorrientePorCircuito(c.Fases, c.Resultado!.CorrienteDisenoA))],
            Datos.Barras);

        // Los kW de verdad: la potencia activa de cada circuito, no los VA totales por un F.P. que el
        // tablero no tiene.
        var conCarga = _circuitos.Where(c => c.TieneCarga).ToList();
        // Con los 1500 VA de 220-52 a su F.P., igual que el total en kVA: si no, la demandada salía
        // mayor que la instalada.
        var instaladaW = conCarga.Sum(c => c.PotenciaActivaW + c.Ajuste220_52VA * c.FactorPotencia);
        // CADA CIRCUITO CON EL FACTOR DE SU TIPO — R-17. La parte continua y la no continua llevan el
        // mismo factor; lo que las distingue es el 125 % del alimentador, no la demanda.
        var superficie = MinimoDeSuperficie(conCarga);
        var demandadaW = conCarga.Sum(c => Demandada(c) * c.FactorPotencia)
                         + (superficie is { } sp ? sp.ContinuaDemandadaVA + sp.NoContinuaDemandadaVA : 0m);
        instaladaW += superficie?.AjusteTotalVA ?? 0m;
        var minimo220_52 = conCarga.Sum(c => c.Ajuste220_52VA);

        Resumen = new ResumenDeCarga(
            MotoresVA: conCarga.Sum(c => c.MotorVA),
            MotoresDemandadaVA: conCarga.Sum(MotorDemandada),
            ContinuaVA: continua,
            ContinuaDemandadaVA: conCarga.Sum(ContinuaDemandada),
            NoContinuaVA: noContinua,
            NoContinuaDemandadaVA: conCarga.Sum(NoContinuaDemandada),
            CargaPorFaseVA: porFase,
            DesbalanceoPct: desbalanceo,
            InstaladaW: instaladaW,
            DemandadaW: demandadaW,
            FactorPotencia: FactorPotenciaCombinado.De(conCarga.Select(c => (c.CargaInstaladaVA, c.FactorPotencia))),
            Minimo220_52VA: minimo220_52,
            Minimo220_52DemandadoVA: conCarga.Sum(AjusteDemandado),
            Superficie: superficie,
            PorCategoria:
            [
                .. Enum.GetValues<CategoriaDeCarga>().Select(categoria =>
                {
                    // POR TIPO DE CARGA, no de circuito — I-123: un circuito combinado aporta a varios renglones.
                    var instalada = conCarga.SelectMany(c => c.Porciones).Where(p => p.Tipo == categoria).Sum(p => p.TotalVA);
                    var factor = Datos.FactorDeDemanda(categoria);
                    return new CargaPorCategoria(categoria, instalada, factor, instalada * factor);
                }),
            ]);
    }

    /// <summary>
    /// El factor de demanda del tipo del circuito — R-17. Cero en el menor de un par no simultáneo: no
    /// entra al alimentador — 220-60 (I-121).
    /// </summary>
    private decimal Fd(CircuitoDelCuadro c, CategoriaDeCarga tipo) => c.OmitidoPorNoSimultaneo ? 0m : Datos.FactorDeDemanda(tipo);

    /// <summary>
    /// <b>La demanda de un circuito, carga por carga</b> — I-123: cada parte con el factor de su tipo
    /// (220 Parte C); el mínimo de 220-52, con el de contactos. Cero si es el menor de un par no simultáneo.
    /// </summary>
    private decimal ContinuaDemandada(CircuitoDelCuadro c) => c.Porciones.Sum(p => p.ContinuaVA * Fd(c, p.Tipo));
    private decimal NoContinuaDemandada(CircuitoDelCuadro c) => c.Porciones.Sum(p => p.NoContinuaVA * Fd(c, p.Tipo));
    private decimal MotorDemandada(CircuitoDelCuadro c) => c.Porciones.Sum(p => p.MotorVA * Fd(c, p.Tipo));
    private decimal AjusteDemandado(CircuitoDelCuadro c) => c.Ajuste220_52VA * Fd(c, c.Categoria);
    private decimal Demandada(CircuitoDelCuadro c) => ContinuaDemandada(c) + NoContinuaDemandada(c) + MotorDemandada(c) + AjusteDemandado(c);

    /// <summary>
    /// <b>El mínimo por superficie</b> — M-14, 220-12 y 220-14(j)(k). <c>null</c> sin área servida o si el
    /// renglón de la Tabla 220-12 no se encuentra. El ajuste se aplica con el F.D. de su tipo.
    /// </summary>
    private MinimoPorSuperficie? MinimoDeSuperficie(IReadOnlyList<CircuitoDelCuadro> conCarga)
    {
        if (Datos.AreaServidaM2 <= 0m)
            return null;
        var uso = Datos.UsoTabla220_12Efectivo;
        var fila = _motor.CargaUnitaria.Filas.FirstOrDefault(f => f.Inmueble.StartsWith(uso, StringComparison.OrdinalIgnoreCase));
        if (fila.Inmueble is null)
            return null;

        decimal DelTipo(CircuitoDelCuadro c, CategoriaDeCarga tipo) => c.Porciones.Where(p => p.Tipo == tipo).Sum(p => p.TotalVA);
        // I-157: anuncios, aparadores y portalámparas de trabajo pesado son alumbrado, pero no general —
        // 220-14(e), (f), (g): no llenan el mínimo.
        decimal NoGeneral(CircuitoDelCuadro c) => c.TieneDesglose
            ? c.Cargas.Where(a => c.TipoDe(a) == CategoriaDeCarga.Alumbrado && a.Subtipo?.NoEsAlumbradoGeneral() == true).Sum(a => a.TotalVA)
            : 0m;
        var vivienda = Datos.Inmueble.EsVivienda();
        var area = Datos.AreaServidaM2;
        var noGeneral = conCarga.Sum(NoGeneral);
        // 220-14(j): en vivienda, los contactos de uso general de 20 A o menos —el baño incluido— van dentro
        // de los 33 VA/m²; los de aparatos pequeños y lavadora, no (220-52).
        var alumbrado = conCarga.Sum(c => DelTipo(c, CategoriaDeCarga.Alumbrado)) - noGeneral
            + (vivienda ? conCarga.Where(c => c.UsoEfectivo is UsoDeContactos.General or UsoDeContactos.Bano).Sum(c => DelTipo(c, CategoriaDeCarga.Contactos)) : 0m);
        var ajusteAlumbrado = Math.Max(0m, fila.VaPorM2 * area - alumbrado);

        // 220-14(k): bancos y oficinas, contactos = el mayor entre 180 VA por contacto y 11 VA/m².
        var oficinas = fila.Inmueble.StartsWith("Bancos", StringComparison.OrdinalIgnoreCase)
                       || fila.Inmueble.StartsWith("Edificios de oficinas", StringComparison.OrdinalIgnoreCase);
        var contactos = conCarga.Sum(c => DelTipo(c, CategoriaDeCarga.Contactos));
        decimal? minimoContactos = oficinas ? 11m * area : null;
        var ajusteContactos = minimoContactos is { } mc ? Math.Max(0m, mc - contactos) : 0m;

        var alumbradoDemandado = ajusteAlumbrado * Datos.FactorDeDemanda(CategoriaDeCarga.Alumbrado);
        var contactosDemandado = ajusteContactos * Datos.FactorDeDemanda(CategoriaDeCarga.Contactos);
        return new MinimoPorSuperficie(
            fila.Inmueble, fila.VaPorM2, area, alumbrado, vivienda, ajusteAlumbrado, !vivienda,
            minimoContactos, contactos, ajusteContactos,
            ContinuaDemandadaVA: vivienda ? 0m : alumbradoDemandado,
            NoContinuaDemandadaVA: (vivienda ? alumbradoDemandado : 0m) + contactosDemandado,
            AlumbradoNoGeneralVA: noGeneral);
    }

    /// <summary>
    /// El mínimo por superficie, repartido parejo entre las barras — M-14: no es de un circuito; se toma
    /// balanceado, como el alumbrado general que representa. En A por barra.
    /// </summary>
    private (decimal ContinuaA, decimal NoContinuaA) SuperficiePorBarra() =>
        Resumen.Superficie is { } sp
            ? (sp.ContinuaDemandadaVA / (Datos.Barras.Count * Datos.TensionFaseNeutroV), sp.NoContinuaDemandadaVA / (Datos.Barras.Count * Datos.TensionFaseNeutroV))
            : (0m, 0m);

    /// <summary>
    /// <b>Cargas no simultáneas</b> — I-121: de cada par, al alimentador va la mayor; la menor se omite —
    /// 220-60, 430-24 Excepción 3, 440-33 Excepción 1. La carga que se compara es la que entra al
    /// alimentador, con su factor de demanda. Un par con un circuito sin carga, o que ya no existe, avisa.
    /// </summary>
    private void ResolverNoSimultaneos()
    {
        _avisosNoSimultaneos.Clear();
        foreach (var c in _circuitos)
            c.OmitidoPorNoSimultaneo = false;

        decimal Carga(CircuitoDelCuadro c) =>
            c.Porciones.Sum(p => p.TotalVA * Datos.FactorDeDemanda(p.Tipo)) + c.Ajuste220_52VA * Datos.FactorDeDemanda(c.Categoria);
        var mayorDe = new Dictionary<CircuitoDelCuadro, CircuitoDelCuadro>();
        foreach (var c in _circuitos.Where(c => c.NoSimultaneoCon is not null && !c.EsContinuacion && !c.EsDelPrincipal))
        {
            var otro = _circuitos.FirstOrDefault(x => x.Espacio == c.NoSimultaneoCon);
            if (otro is null || otro == c || otro.EsContinuacion || otro.EsDelPrincipal || !otro.TieneCarga || !c.TieneCarga)
            {
                if (c.TieneCarga)
                    _avisosNoSimultaneos.Add(
                        $"Circuito {c.Espacio}: no simultáneo con el {c.NoSimultaneoCon}, que no tiene carga o ya no es un circuito. Revisa el par (220-60).");
                continue;
            }
            // El menor; en un empate, el de número mayor. Un par capturado de los dos lados da lo mismo.
            var (menor, mayor) = Carga(c) < Carga(otro) || (Carga(c) == Carga(otro) && c.Espacio > otro.Espacio) ? (c, otro) : (otro, c);
            menor.OmitidoPorNoSimultaneo = true;
            mayorDe[menor] = mayor;
        }
        foreach (var (menor, mayor) in mayorDe)
            _avisosNoSimultaneos.Add(
                $"Circuito {menor.Espacio}: no entra al alimentador — no funciona a la vez que el circuito {mayor.Espacio}, que es mayor " +
                $"(220-60{(menor.EsMotor ? ", 430-24 Excepción 3" : menor.EsAireAcondicionado ? ", 440-33 Excepción 1" : "")}).");
    }

    private readonly List<string> _avisosNoSimultaneos = [];

    /// <summary>
    /// <b>La corriente de cada barra del alimentador</b> — M-02. Cada fase del alimentador lleva su
    /// propia corriente, y el conductor y el principal se dimensionan con <b>la más cargada</b>, no
    /// con la carga total repartida como si el tablero estuviera balanceado. Es lo que hacía el
    /// Excel (<c>CU79</c>/<c>CV79</c>: <c>MAX(1.25·CO78+CR78, …)</c> entre la tensión F-N).
    ///
    /// <para>
    /// <b>Se suma en corriente, no en VA</b>, con la misma regla del desbalanceo
    /// (<see cref="CalculadoraDesbalanceo.CorrientePorFase"/>): un interruptor de 2 polos a 220 V
    /// lleva su corriente completa por cada una de sus dos líneas, no la mitad de sus VA entre 127.
    /// La corriente de cada circuito sale de su carga, no de su resultado, para que un renglón que el
    /// motor rechazó —por caída de tensión, por ejemplo— siga pesando en el alimentador.
    /// </para>
    /// </summary>
    private IReadOnlyList<CorrienteDeFase> CorrientesPorFase()
    {
        var conCarga = _circuitos.Where(c => c.TieneCarga).ToList();

        // Cada carga con el factor de demanda de su tipo — R-17, I-123.
        IReadOnlyDictionary<char, decimal> Sumar(Func<CircuitoDelCuadro, decimal> demandada) =>
            CalculadoraDesbalanceo.CorrientePorFase(
                [.. conCarga.Select(c => new CorrientePorCircuito(
                    c.Fases,
                    demandada(c) / TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV)))],
                Datos.Barras);

        var continua = Sumar(ContinuaDemandada);
        // Los 1500 VA de 220-52 son carga de alimentador: entran aquí, no en el derivado.
        var noContinua = Sumar(c => NoContinuaDemandada(c) + AjusteDemandado(c));

        // El mismo 125 % (o 100 %, con el ensamble aprobado) que aplica la calculadora del
        // alimentador: la fase que gobierna es la que pide más capacidad, no la de más corriente.
        var factor = CargaContinua100Pct.Para(
            Datos.ConjuntoAprobado100Pct, null,
            ClaseDeTramo.Alimentador.CapacidadMinima(), ClaseDeTramo.Alimentador.Excepcion100Pct()).Factor;

        var motores = MotoresPorFase();
        var (supC, supN) = SuperficiePorBarra();
        return
        [
            .. Datos.Barras.Select(f => new CorrienteDeFase(f, continua[f] + supC, noContinua[f] + supN, factor, motores[f].Agregado))
        ];
    }

    /// <summary>
    /// <b>Los motores de cada barra</b> — I-15: cada motor en HP con su FLC (y el F.D. de su tipo) en
    /// las barras que toca, agrupados para 430-24 (el mayor y el resto) y 430-62(a) (la mayor
    /// protección de derivado), y la suma fasorial de sus FLC para la caída. La corriente y su
    /// sentido en cada barra, como los demás circuitos (<see cref="Direcciones"/>).
    /// </summary>
    private Dictionary<char, (AgregadoMotores Agregado, Fasor Fasor)> MotoresPorFase()
    {
        var miembros = Datos.Barras.ToDictionary(b => b, _ => new List<(MotorDelAlimentador Miembro, decimal Angulo)>());
        foreach (var m in MotoresDelAlimentador())
            foreach (var (fase, angulo) in Direcciones(m.Circuito))
                miembros[fase].Add((m, angulo));

        return Datos.Barras.ToDictionary(b => b, b => AgregarPorFase(miembros[b]));
    }

    /// <summary>
    /// Un motor o equipo de A/C del grupo de 430-24 / 440-33: su corriente completa, el F.D. de su tipo,
    /// la protección de su derivado (para 430-62(a)) y si su corriente ya trae el 25 % de su motor
    /// mayor (la MCA de placa, 440-4(b)).
    /// </summary>
    private sealed record MotorDelAlimentador(CircuitoDelCuadro Circuito, decimal CorrienteA, decimal FactorDemanda, decimal? ProteccionA, bool YaMayorada);

    /// <summary>
    /// <b>La protección del derivado que entra a 430-62(a) y 430-63</b>: «con base en el valor máximo
    /// permitido para el tipo específico de uno de los dispositivos protectores de acuerdo con 430-52»
    /// (M-20, pregunta 6: la lectura literal, David, 2026-10-03). En un motor solo, el máximo de 430-52
    /// —el de la lista de 240-6(a)—, aunque se instale menos; en lo demás, la protección del circuito.
    /// </summary>
    private static decimal? ProteccionPara430_62(CircuitoDelCuadro c) =>
        c.Resultado?.Rango?.MaximoPermitidoA ?? c.Resultado?.ProteccionA;

    private IEnumerable<MotorDelAlimentador> MotoresDelAlimentador() =>
        _circuitos
            .Where(c => c.TieneCarga && (c.EsDeMotor || c.EsGrupo) && c.CorrienteDeMotorA > 0m && !c.OmitidoPorNoSimultaneo)
            .SelectMany(c => c.EsGrupo
                // UN GRUPO ENTRA MOTOR POR MOTOR — I-115: el 125 % de 430-24 es del motor mayor del
                // alimentador, no del circuito que lo lleva. Todos con la protección de su circuito.
                ? c.Cargas
                    .Where(a => a.EsMaquina && a.CorrienteUnitariaA > 0m)
                    // Cada una con el F.D. de su tipo — I-123.
                    .SelectMany(a => Enumerable.Repeat((a.CorrienteUnitariaA, Tipo: c.TipoDe(a)), a.Cantidad))
                    .Select(m => new MotorDelAlimentador(c, m.CorrienteUnitariaA, Fd(c, m.Tipo), ProteccionPara430_62(c), false))
                : c.CorrienteDeServicioA > 0m
                    // SERVICIO NO CONTINUO — 430-24 Excepción 1 (I-120): con el valor de 430-22(e), que ya trae
                    // su porcentaje; no compite por el 125 % del mayor.
                    ? [new MotorDelAlimentador(c, c.CorrienteDeServicioA, Fd(c, c.Categoria), ProteccionPara430_62(c), true)]
                    : [new MotorDelAlimentador(
                        c, c.CorrienteDeMotorA, Fd(c, c.Categoria),
                        // Sin derivado calculado no hay protección que aportar al techo de 430-62(a).
                        ProteccionPara430_62(c),
                        c.EsAireAcondicionado && c.PlacaAire == PlacaDeAireAcondicionado.AmpacidadYProteccion)]);

    /// <summary>
    /// <b>El grupo de una barra</b> para 430-24 y 430-62(a):
    /// <list type="bullet">
    /// <item>El mayor —el de mayor corriente, 440-7— entra <b>completo</b> y con su 125 %: el factor de
    /// demanda de 430-26 no lo reduce, porque el alimentador tiene que alcanzar «para la carga máxima
    /// determinada de acuerdo con el tamaño y número de los motores», y un motor solo puede trabajar a
    /// plena carga (M-12). Antes el F.D. lo reducía también: con 0.5 y un motor de 5 hp, 9.5 A para
    /// un motor de 15.2 A.</item>
    /// <item>Los demás, con el F.D. de su tipo.</item>
    /// <item>Una unidad con MCA no compite por el mayor y entra al 100 %: su MCA ya trae el 25 % de su
    /// motor mayor, 440-4(b); antes recibía otro 25 % (M-13, decisión de David, 2026-09-29).</item>
    /// </list>
    /// La suma fasorial, para la caída, con las mismas corrientes.
    /// </summary>
    private static (AgregadoMotores Agregado, Fasor Fasor) AgregarPorFase(IReadOnlyList<(MotorDelAlimentador Miembro, decimal Angulo)> miembros)
    {
        if (miembros.Count == 0)
            return (AgregadoMotores.Vacio, new Fasor(0m, 0m));

        var mayor = miembros.Where(x => !x.Miembro.YaMayorada).Select(x => x.Miembro)
            .Aggregate((MotorDelAlimentador?)null, (a, m) => a is null || m.CorrienteA > a.CorrienteA ? m : a);
        decimal Cuenta(MotorDelAlimentador m) => ReferenceEquals(m, mayor) ? m.CorrienteA : m.FactorDemanda * m.CorrienteA;

        // 430-62(a): la mayor protección de derivado; en un empate cuenta el primero. Es del circuito:
        // en un grupo protege a todos sus motores, y todos se descuentan de «los demás» (I-115).
        var conProteccion = miembros.Select(x => x.Miembro).Where(m => m.ProteccionA is not null)
            .GroupBy(m => m.Circuito)
            .Select(g => (Proteccion: g.First().ProteccionA!.Value, Corriente: g.Sum(Cuenta)))
            .Aggregate(((decimal Proteccion, decimal Corriente)?)null, (a, p) => a is null || p.Proteccion > a.Value.Proteccion ? p : a);

        var agregado = new AgregadoMotores(
            MayorFlcA: mayor?.CorrienteA ?? 0m,
            SumaRestoFlcA: miembros.Where(x => !ReferenceEquals(x.Miembro, mayor)).Sum(x => Cuenta(x.Miembro)),
            MayorProteccionDerivadoA: conProteccion?.Proteccion,
            FlcDelMayorProteccionA: conProteccion?.Corriente ?? 0m);
        var fasor = miembros.Aggregate(new Fasor(0m, 0m), (f, x) => f + new Fasor(Cuenta(x.Miembro), x.Angulo));
        return (agregado, fasor);
    }

    /// <summary>
    /// Por qué barras sale la corriente de un circuito y con qué ángulo: 1 polo, el de V<sub>FN</sub> − θ;
    /// 2 polos, sale por una con el de V<sub>FF</sub> − θ y regresa por la otra; 3 polos, cada una con el
    /// de su V<sub>FN</sub> − θ.
    /// </summary>
    private IEnumerable<(char Fase, decimal Angulo)> Direcciones(CircuitoDelCuadro c)
    {
        var theta = (decimal)(Math.Acos((double)c.FactorPotencia) * 180.0 / Math.PI);
        var fases = c.Fases.Where(Datos.Barras.Contains).ToList();
        if (fases.Count == 2)
        {
            var vff = new Fasor(1m, AnguloDeTension(fases[0])) - new Fasor(1m, AnguloDeTension(fases[1]));
            yield return (fases[0], vff.AnguloGrados - theta);
            yield return (fases[1], vff.AnguloGrados - theta + 180m);
        }
        else
            foreach (var f in fases)
                yield return (f, AnguloDeTension(f) - theta);
    }

    /// <summary>
    /// <b>Las corrientes de cada fase para el motor</b>, ya con el factor de demanda del tipo de cada
    /// circuito (R-17): el motor las recibe con F.D. 1 — R-04 y R-02. La suma
    /// aritmética dimensiona (la misma de <see cref="CorrientesPorFase"/>); la fasorial da la caída con
    /// el neutro. Cada circuito aporta según cómo circula su corriente:
    /// <list type="bullet">
    /// <item><b>1 polo</b>: sale por su fase y regresa por el neutro, con el ángulo de V<sub>FN</sub> − θ.</item>
    /// <item><b>2 polos</b>: sale por una fase y regresa por la otra, con el ángulo de V<sub>FF</sub> − θ.
    /// No toca el neutro.</item>
    /// <item><b>3 polos</b>: carga balanceada, cada fase con su V<sub>FN</sub> − θ. Suma cero en el neutro.</item>
    /// </list>
    /// Los 1500 VA de 220-52 entran como no continua, con el ángulo del circuito.
    /// </summary>
    private IReadOnlyList<CorrienteDeFaseAlimentador> CorrientesParaElMotor()
    {
        var barras = Datos.Barras;
        var angulo = barras.ToDictionary(b => b, b => AnguloDeTension(b));
        var continuaA = barras.ToDictionary(b => b, _ => 0m);
        var noContinuaA = barras.ToDictionary(b => b, _ => 0m);
        var fasorContinua = barras.ToDictionary(b => b, _ => new Fasor(0m, 0m));
        var fasorNoContinua = barras.ToDictionary(b => b, _ => new Fasor(0m, 0m));

        // Un motor en HP no tiene continua ni no continua: aporta cero aquí y entra con sus motores. Las
        // otras cargas de un grupo, sí (I-115).
        foreach (var c in _circuitos.Where(c => c.TieneCarga && (!c.EsMotor || c.EsGrupo)))
        {
            var divisor = TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
            var iContinua = ContinuaDemandada(c) / divisor;
            var iNoContinua = (NoContinuaDemandada(c) + AjusteDemandado(c)) / divisor;

            foreach (var (f, anguloCorriente) in Direcciones(c))
            {
                continuaA[f] += iContinua;
                noContinuaA[f] += iNoContinua;
                fasorContinua[f] += new Fasor(iContinua, anguloCorriente);
                fasorNoContinua[f] += new Fasor(iNoContinua, anguloCorriente);
            }
        }

        // El mínimo por superficie (M-14), parejo y en fase con su tensión.
        var (supC, supN) = SuperficiePorBarra();
        foreach (var b in barras)
        {
            continuaA[b] += supC;
            noContinuaA[b] += supN;
            fasorContinua[b] += new Fasor(supC, angulo[b]);
            fasorNoContinua[b] += new Fasor(supN, angulo[b]);
        }

        var motores = MotoresPorFase();
        return [.. barras.Select(b => new CorrienteDeFaseAlimentador(
            b, continuaA[b], noContinuaA[b], angulo[b], fasorContinua[b], fasorNoContinua[b], motores[b].Agregado, motores[b].Fasor))];
    }

    /// <summary>Ángulo de V fase-neutro: A 0°, B −120°, C 120°; en 1F-3H las dos barras están en oposición.</summary>
    private decimal AnguloDeTension(char barra) =>
        SistemaDelTablero.De(Datos.Sistema) == ConfiguracionTablero.UnaFaseTresHilos
            ? (barra == 'A' ? 0m : 180m)
            : barra switch { 'A' => 0m, 'B' => -120m, _ => 120m };

    private void CalcularAlimentador()
    {
        var polos = Datos.Barras.Count;
        NeutroReducido = null;
        OpcionesDeParalelo = [];
        ConductoresPorFaseAutomatico = null;
        FijadoConPisoDeParalelo = false;

        if (Resumen.InstaladaVA <= 0m)
        {
            Alimentador = new RenglonDelAlimentador(null, null, [], polos);
            return;
        }

        var fases = CorrientesPorFase();
        // Empate: gana la primera barra, que es determinista y es como se lee el tablero.
        var gobierna = fases.Aggregate((max, f) => f.CapacidadA > max.CapacidadA ? f : max);

        // EL F.P. DEL ALIMENTADOR NO SE CAPTURA: resulta de las cargas que lleva. Se toman las de la
        // fase que gobierna, porque es la corriente de esa fase la que entra a la caída de tensión.
        // Cada circuito aporta lo que le cuelga a esa barra (sus VA entre sus polos) CON SU FACTOR DE
        // DEMANDA Y EL MÍNIMO DE 220-52, igual que la corriente de la caída (CorrientesParaElMotor).
        // Con la carga instalada salía otro (0.98 contra 0.97 en la cocina de 1F-2H) — I-47.
        var fpAlimentador = FactorPotenciaCombinado.De(
            _circuitos
                .Where(c => c.TieneCarga && c.Fases.Contains(gobierna.Fase))
                .Select(c => (Demandada(c) / c.Fases.Length, c.FactorPotencia)));

        var canal = Datos.CanalizacionAlimentador;
        canal.Limpiar();
        var conNeutro = SistemaConNeutro;

        // EL MOTOR RECIBE LA CARGA TOTAL Y LAS CORRIENTES DE CADA FASE — R-04. Con ellas elige la
        // fase que gobierna, aplica el factor de demanda (220-40, con la carga real en su cita) y
        // calcula la caída fase por fase con el neutro (R-02). Antes la web le entregaba 3 × los VA
        // de la fase más cargada y reescribía la cita 220-40: ya no.
        //
        // N: el automático empieza en 1 y sube hasta el tope; fijado, va de N a N, exacto — I-162.
        // R4-3: con N fijado de 2 o más, 1/0 AWG de piso (310-10(h)(1)) en vez de rechazar el N.
        ResultadoAlimentador Calcular(CanalizacionDelTablero canal, int n, int tope, bool pisoDeParalelo = false) => AlimentadorRecordado(new DatosEntradaAlimentador(
                // Ya con el factor de demanda de cada tipo (R-17): por eso el motor va con F.D. 1 abajo.
                CargaContinuaVA: Resumen.ContinuaDemandadaVA + (Resumen.Superficie?.ContinuaDemandadaVA ?? 0m),
                CargaNoContinuaVA: Resumen.NoContinuaDemandadaVA + Resumen.Minimo220_52DemandadoVA + (Resumen.Superficie?.NoContinuaDemandadaVA ?? 0m),
                NumeroFases: polos,
                TensionFaseNeutroV: Datos.TensionFaseNeutroV,
                TensionFaseFaseV: Datos.TensionFaseFaseV,
                LongitudM: Datos.LongitudAlimentadorM,
                NumeroConductoresParalelo: n,
                // De SU canalización — I-39: el alimentador ya no hereda el agrupamiento de los derivados.
                NumeroConductoresAgrupados: canal.Ajuste!.ConductoresParaElMotor,
                TemperaturaAmbienteC: Datos.TemperaturaAmbienteC + canal.SumadorAzoteaC,
                MaterialConductor: Datos.MaterialConductor,
                MaterialCanalizacion: canal.MaterialParaTabla9,
                // Solo para la caída balanceada de un alimentador sin neutro (3F-3H).
                FactorPotencia: fpAlimentador,
                CaidaTensionMaxPct: Datos.CaidaMaxAlimentadorPct,
                PisoPracticoCalibreMm2: pisoDeParalelo && n > 1 ? PisoDeParaleloMm2 : null,
                FactorDemandaContinua: 1m,
                FactorDemandaNoContinua: 1m,
                TipoAislamiento: Datos.TipoAislamiento,
                Lugar: Datos.Lugar,
                ConjuntoAprobado100Pct: Datos.ConjuntoAprobado100Pct,
                TerminalesMarcadas75C: Datos.TerminalesMarcadas75C,
                CorrientesPorFase: CorrientesParaElMotor(),
                ConNeutro: SistemaConNeutro,
                // 230-79: el principal SUBE al mínimo si el tablero es el de la acometida — R-11.
                ProteccionMinimaA: Datos.Minimo230_79?.Amperes,
                ReferenciaProteccionMinima: Datos.Minimo230_79?.Referencia,
                MaxConductoresParaleloAutomatico: tope));

        // El alimentador completo con N automático (null) o fijado, sobre una canalización: conteo,
        // motor, neutro de 220-61 y tamaño. Deja la canalización con lo que resultó.
        (ResultadoAlimentador, NeutroDe220_61?) Resolver(CanalizacionDelTablero canal, int? fijo, bool pisoDeParalelo = false)
        {
            ResultadoAlimentador resultado;
            if (fijo is int k)
            {
                // Con N fijado el número de juegos ya se sabe: no hay vueltas.
                Contar(canal, [new CircuitoEnCanalizacion("alimentador", polos, conNeutro, [.. Datos.Barras], canal.JuegosEnUnTubo ? k : 1)]);
                resultado = Calcular(canal, k, k, pisoDeParalelo);
                if (pisoDeParalelo)
                    resultado = ConPisoDeParaleloCitado(resultado, k);
            }
            else
            {
                // CONDUCTORES EN PARALELO. Un juego por canalización, todas iguales (310-10(h)(3)): el
                // conteo no cambia. Si se declaran todos en una, cada conductor cuenta
                // (310-15(b)(3)(a)): más portadores pueden pedir más cobre, y más cobre más juegos. Se
                // repite hasta que el número de juegos deje de cambiar.
                var juegos = 1;
                for (var vuelta = 0; ; vuelta++)
                {
                    Contar(canal, [new CircuitoEnCanalizacion("alimentador", polos, conNeutro, [.. Datos.Barras], juegos)]);
                    resultado = Calcular(canal, 1, DatosDelTablero.MaximoConductoresPorFase);
                    var enUno = canal.JuegosEnUnTubo ? resultado.NumeroConductoresParalelo : 1;
                    if (enUno == juegos) break;
                    if (vuelta == 3)
                        throw new InvalidOperationException(
                            "Con todos los conductores en paralelo en una sola canalización, el número de juegos no se estabiliza: "
                            + "declara un juego por canalización.");
                    juegos = enUno;
                }
            }

            // 220-61: el neutro a su carga de desbalance, si se pidió y se permite — I-161. Antes de
            // dimensionar la canalización, que lleva el neutro que se instala.
            NeutroDe220_61? neutro = null;
            if (Datos.NeutroReducido220_61 && PorQueNoSeReduceElNeutro is null)
                (resultado, neutro) = ReducirNeutro(resultado, canal);

            var n = resultado.NumeroConductoresParalelo;
            canal.CanalizacionesIguales = canal.JuegosEnUnTubo ? 1 : n;
            Dimensionar(canal,
                [new ConductoresDelCircuito("alimentador", polos, conNeutro, resultado.CalibreFase, resultado.CalibreNeutro, resultado.CalibreTierra)],
                canal.JuegosEnUnTubo ? n : 1);

            // R4-1: si ningún tubo de la Tabla 4 admite los conductores, el alimentador no está resuelto.
            if (canal.NingunTamanoAlcanza)
                throw new InvalidOperationException(NingunTuboAlcanza(canal, n));

            // R4-2: los N juegos en una sola canalización llevan una sola tierra — 250-122(f).
            if (n > 1 && canal.JuegosEnUnTubo)
                resultado = resultado with
                {
                    Citas = [.. resultado.Citas.Select(c => c.Referencia == "250-122(f)"
                        ? new Cita("250-122(f)", $"Fases en paralelo, todas en la misma canalización: basta un conductor de tierra ({resultado.CalibreTierra}).")
                        : c)],
                };
            return (resultado, neutro);
        }

        // LAS OPCIONES: cada N de 1 al tope, fijado, en una copia de la canalización para no tocar la
        // del tablero — I-162. Las que no cumplen dicen por qué, para el error de un N fijado.
        var opciones = new List<OpcionDeParalelo>();
        var porQueNo = new Dictionary<int, Exception>();
        for (var k = 1; k <= DatosDelTablero.MaximoConductoresPorFase; k++)
        {
            var copia = canal.Copia();
            try
            {
                var (r, _) = Resolver(copia, k);
                opciones.Add(new OpcionDeParalelo(k, r.CalibreFase, r.CalibreNeutro, r.CalibreTierra,
                    r.Detalle?.AmpacidadConductorA ?? 0m, r.CaidaTensionPct, $"{copia.Rotulo} · {copia.TamanoRotulo}",
                    k * (polos * r.CalibreFase.AreaMm2 + (conNeutro ? r.CalibreNeutro.AreaMm2 : 0m))
                        + (copia.JuegosEnUnTubo ? 1 : k) * r.CalibreTierra.AreaMm2,
                    LlenadoSinVerificar: copia.LlenadoSinVerificar));
            }
            catch (Exception ex)
            {
                porQueNo[k] = ex;
            }
        }

        var fijado = Datos.ConductoresPorFaseAlimentador;
        try
        {
            ResultadoAlimentador resultado;
            if (fijado is int k)
            {
                try { ConductoresPorFaseAutomatico = Resolver(canal.Copia(), null).Item1.NumeroConductoresParalelo; }
                catch (Exception) { /* el automático tampoco cumple: se dice en la cita y en el selector */ }
                var conPiso = porQueNo.TryGetValue(k, out var ex) && ex is ConductoresParaleloNoPermitidoException;
                if (ex is not null && !conPiso)
                    throw new InvalidOperationException(PorQueNoCumple(k, ex, opciones));
                (resultado, NeutroReducido) = Resolver(canal, k, conPiso);
                FijadoConPisoDeParalelo = conPiso;
                if (conPiso)
                    opciones.Add(new OpcionDeParalelo(k, resultado.CalibreFase, resultado.CalibreNeutro, resultado.CalibreTierra,
                        resultado.Detalle?.AmpacidadConductorA ?? 0m, resultado.CaidaTensionPct, $"{canal.Rotulo} · {canal.TamanoRotulo}",
                        k * (polos * resultado.CalibreFase.AreaMm2 + (conNeutro ? resultado.CalibreNeutro.AreaMm2 : 0m))
                            + (canal.JuegosEnUnTubo ? 1 : k) * resultado.CalibreTierra.AreaMm2, PorPisoDeParalelo: true,
                        LlenadoSinVerificar: canal.LlenadoSinVerificar));
                var auto = ConductoresPorFaseAutomatico is int a
                    ? (a == k ? "; el automático da lo mismo." : $"; el automático daba {a}.")
                    : "; el automático no encuentra cómo.";
                resultado = resultado with
                {
                    Citas = [.. resultado.Citas, new Cita("310-10(h)(1)",
                        $"{k} {(k == 1 ? "conductor" : "conductores")} por fase, fijado por el proyectista" + auto
                        + (k > 1 ? " En paralelo, de 1/0 AWG o mayor — 310-10(h)(1), y todos iguales — 310-10(h)(2)"
                            + (canal.JuegosEnUnTubo ? "." : "; cada canalización con su juego completo — 300-3(b)(1), y su tierra — 250-122(f).") : ""))],
                };
            }
            else
            {
                (resultado, NeutroReducido) = Resolver(canal, null);
                ConductoresPorFaseAutomatico = resultado.NumeroConductoresParalelo;
            }

            OpcionesDeParalelo = [.. opciones.OrderBy(o => o.PorFase).Select(o => o with { EsAutomatico = o.PorFase == ConductoresPorFaseAutomatico })];

            // La fase que gobierna la decide el motor; la de aquí es la misma regla, para mostrarla.
            if (resultado.FaseQueGobierna is { } fase)
                gobierna = fases.Single(f => f.Fase == fase);
            Alimentador = new RenglonDelAlimentador(resultado, null, Avisos(resultado), polos, fases, gobierna, fpAlimentador);
        }
        catch (Exception ex)
        {
            NeutroReducido = null;
            OpcionesDeParalelo = [.. opciones.Select(o => o with { EsAutomatico = o.PorFase == ConductoresPorFaseAutomatico })];
            Alimentador = new RenglonDelAlimentador(null, ex.Message, [], polos, fases, gobierna, fpAlimentador);
        }
    }

    /// <summary>
    /// Las formas de correr el alimentador de 1 al tope de conductores por fase, las que cumplen — I-162.
    /// Vacía sin carga. Una sola con 1 por fase cuando no hay más que comparar.
    /// </summary>
    public IReadOnlyList<OpcionDeParalelo> OpcionesDeParalelo { get; private set; } = [];

    /// <summary>
    /// Cuántos conductores de tierra lleva el alimentador: uno por canalización en paralelo, o uno solo si
    /// los juegos van todos en la misma — 250-122(f), R4-2.
    /// </summary>
    public int TierrasDelAlimentador => Alimentador.Resultado is { NumeroConductoresParalelo: var n }
        ? (n > 1 && Datos.CanalizacionAlimentador.JuegosEnUnTubo ? 1 : n)
        : 1;

    /// <summary>
    /// 250-24(c)(2), en su letra: cada neutro de acometida en paralelo con el área total de las fases en
    /// paralelo «en las canalizaciones», no solo las de su tubo (lectura del NEC) — R4-4, decisión
    /// <c>neutro-del-alimentador-por-220-61.md</c>.
    /// </summary>
    private const string CadaUnoEnParalelo =
        ", cada uno en paralelo, con el área total de las fases en paralelo, como lo dice la NOM — 250-24(c)(2)";

    /// <summary>1/0 AWG: el menor conductor que se permite en paralelo — 310-10(h)(1), Tabla 8.</summary>
    private const decimal PisoDeParaleloMm2 = 53.49m;

    /// <summary>
    /// El N fijado salía con conductores menores de 1/0 AWG y se calculó con 1/0 de piso — R4-3. Cumple
    /// 310-10(h)(1), sobredimensionado por ese mínimo.
    /// </summary>
    public bool FijadoConPisoDeParalelo { get; private set; }

    /// <summary>La cita «Piso práctico» del motor, dicha como lo que es: el mínimo de 310-10(h)(1) — R4-3.</summary>
    private static ResultadoAlimentador ConPisoDeParaleloCitado(ResultadoAlimentador r, int k) => r with
    {
        Citas = [.. r.Citas.Select(c => c.Referencia == "Piso práctico"
            ? new Cita("310-10(h)(1)", $"Con {k} conductores por fase, por ampacidad, 240-4 y caída bastaba un calibre menor que 1/0 AWG; " +
                $"en paralelo el mínimo es 1/0 AWG, así que cada uno es de {r.CalibreFase} (sobredimensionado por ese mínimo).")
            : c)],
    };

    /// <summary>R4-1: el error cuando ningún tubo admite los conductores del alimentador, con qué hacer.</summary>
    private static string NingunTuboAlcanza(CanalizacionDelTablero canal, int n)
    {
        var motivo = canal.Ocupacion!.Avisos.FirstOrDefault(a => a.StartsWith("Ningún"))?.Replace(" Reparte los conductores en más de una canalización.", "")
            ?? $"Ningún {canal.Rotulo} de la Tabla 4 admite estos conductores.";
        var que = canal.JuegosEnUnTubo && n > 1
            ? "Quita «Paralelos en un tubo» (un juego por canalización), cambia a ducto o charola, o baja los conductores por fase."
            : "Cambia a ducto o charola, o sube los conductores por fase para repartirlos en más canalizaciones.";
        return $"{motivo.TrimEnd('.')} — Capítulo 10, Tabla 1. {que}";
    }

    /// <summary>El N que da el automático, aunque el proyectista haya fijado otro. Null si no encuentra ninguno.</summary>
    public int? ConductoresPorFaseAutomatico { get; private set; }

    /// <summary>
    /// Se ofrece comparar: hay más de una opción, o una sola de 250 kcmil o más, que en la práctica
    /// conviene partir, o el N fijado no cumple.
    /// </summary>
    public bool ConvieneCompararParalelos =>
        OpcionesDeParalelo.Count > 1
        || OpcionesDeParalelo.Any(o => o.PorFase == 1 && o.Fase.AreaMm2 >= 127m) // 250 kcmil, Tabla 8
        || Datos.ConductoresPorFaseAlimentador is not null;

    /// <summary>Por qué no cumple un N fijado, en palabras del proyectista, y cuáles sí.</summary>
    private string PorQueNoCumple(int k, Exception ex, IReadOnlyList<OpcionDeParalelo> cumplen)
    {
        var con = $"Con {k} {(k == 1 ? "conductor" : "conductores")} por fase";
        var porque = ex switch
        {
            ConductoresParaleloNoPermitidoException e when System.Text.RegularExpressions.Regex.Match(e.Message, @"resultante \((.+?)\) es menor") is { Success: true } m =>
                $"{con}, cada uno sale de {_motor.Calibres.Listar().FirstOrDefault(c => c.Designacion == m.Groups[1].Value)?.DesignacionConUnidad ?? m.Groups[1].Value}, y 310-10(h)(1) pide 1/0 AWG o mayor en paralelo.",
            ConductoresParaleloNoPermitidoException =>
                $"{con}, cada uno sale menor que 1/0 AWG, y 310-10(h)(1) pide 1/0 AWG o mayor en paralelo.",
            CaidaTensionExcedidaException =>
                $"{con}, ni el calibre más grande del catálogo baja la caída de tensión a {Datos.CaidaMaxAlimentadorPct:0.##} %.",
            InvalidOperationException e when e.Message.Contains("ampacidad") =>
                $"{con}, ni el calibre más grande del catálogo alcanza la ampacidad o queda protegido — 240-4.",
            _ => $"{con}: {ex.Message}",
        };
        // Las que cumplen con el llenado verificado, y aparte las que cumplen por ampacidad pero sin poder
        // verificar el llenado (falta un diámetro del fabricante): no se recomiendan como seguras.
        var lista = cumplen.Where(o => o.LlenadoSinVerificar is null).Select(o => o.PorFase.ToString()).ToList();
        var sinVerificar = cumplen.Where(o => o.LlenadoSinVerificar is not null).ToList();
        var cuales = lista.Count switch
        {
            0 when sinVerificar.Count > 0 => "",
            0 => $" Ningún número de 1 a {DatosDelTablero.MaximoConductoresPorFase} cumple.",
            1 => $" Cumple {lista[0]} por fase.",
            _ => $" Cumplen {string.Join(", ", lista[..^1])} o {lista[^1]} por fase.",
        };
        if (sinVerificar.Count > 0)
        {
            var ns = sinVerificar.Select(o => o.PorFase.ToString()).ToList();
            var faltan = string.Join(", ", sinVerificar.Select(o => o.LlenadoSinVerificar).Distinct());
            cuales += ns.Count == 1
                ? $" {ns[0]} por fase cumple por ampacidad; el llenado de su canalización no se pudo verificar"
                : $" {string.Join(", ", ns[..^1])} o {ns[^1]} por fase cumplen por ampacidad; el llenado de su canalización no se pudo verificar";
            cuales += $" (falta el diámetro del fabricante de {faltan} — Capítulo 10, Nota 5).";
        }
        return porque + cuales;
    }

    /// <summary>La reducción del neutro del alimentador por 220-61, si se aplicó. Null sin ella — I-161.</summary>
    public NeutroDe220_61? NeutroReducido { get; private set; }

    /// <summary>
    /// Por qué no se ofrece reducir el neutro del alimentador, o null si se puede — 220-61(c), I-161. Solo
    /// en 3F-4H y 1F-3H: en 1F-2H el neutro lleva la corriente de la fase; en 2F-3H de estrella lo prohíbe
    /// (c)(1); con carga no lineal, (c)(2).
    /// </summary>
    public string? PorQueNoSeReduceElNeutro => Configuracion switch
    {
        ConfiguracionTablero.TresFasesTresHilos => "3F-3H: el alimentador no lleva neutro.",
        ConfiguracionTablero.UnaFaseDosHilos => "1F-2H: el neutro lleva la misma corriente que la fase.",
        ConfiguracionTablero.DosFasesDeEstrella => "2 fases + neutro de una estrella: el neutro lleva ≈ la corriente de fase y no se reduce — 220-61(c)(1).",
        _ when Datos.CargaNoLineal => "Con carga no lineal, esa parte del neutro no se reduce — 220-61(c)(2).",
        _ => null,
    };

    /// <summary>
    /// <b>El neutro a su carga de desbalance</b> — 220-61(a): la mayor carga neta entre el neutro y cualquier
    /// fase. Cada circuito que lleva neutro cuenta su corriente demandada en cada fase que toca (un motor,
    /// su FLC); el mínimo de 220-12, parejo. Lo que pasa de 200 A, al 70 % — 220-61(b)(2), salvo lo de los
    /// circuitos de 2 polos con neutro en 3F-4H, que no se reduce — (c)(1). El calibre: la menor ampacidad
    /// utilizable que lleva esa corriente por conductor (mismos factores y columna que la fase); no menor
    /// que la tierra de equipos (215-2(a)(2); en paralelo, con el área del juego), ni, en un equipo de acometida,
    /// que la Tabla 250-66 y el 12.5 % del área de las fases arriba de 1100 kcmil (250-24(c)(1); en paralelo,
    /// cada uno — (c)(2)), ni de 1/0 AWG en paralelo (310-10(h)(1)); nunca mayor que la fase.
    /// </summary>
    private (ResultadoAlimentador, NeutroDe220_61?) ReducirNeutro(ResultadoAlimentador r, CanalizacionDelTablero canal)
    {
        if (r.Detalle is not { } d)
            return (r, null);
        var vfn = Datos.TensionFaseNeutroV;
        var reducible = Datos.Barras.ToDictionary(b => b, _ => 0m);
        var fija = Datos.Barras.ToDictionary(b => b, _ => 0m);
        foreach (var c in _circuitos.Where(c => c.TieneCarga && c.LlevaNeutro))
        {
            var divisor = TensionDeCalculo.Divisor(c.Polos, vfn, Datos.TensionFaseFaseV);
            var i = c.EsMotor && !c.EsGrupo ? c.CorrienteDeMotorA : Demandada(c) / divisor;
            var destino = c.Polos == 2 && Configuracion == ConfiguracionTablero.TresFasesCuatroHilos ? fija : reducible;
            foreach (var f in c.Fases)
                destino[f] += i;
        }
        var (supC, supN) = SuperficiePorBarra();
        foreach (var b in Datos.Barras)
            reducible[b] += supC + supN;

        decimal Efectiva(char b) => fija[b] + (reducible[b] > 200m ? 200m + 0.7m * (reducible[b] - 200m) : reducible[b]);
        var fase = Datos.Barras.Aggregate((m, b) => Efectiva(b) > Efectiva(m) ? b : m);
        var desbalance = reducible[fase] + fija[fase];
        var corriente = Efectiva(fase);
        var con70 = reducible[fase] > 200m;

        var n = r.NumeroConductoresParalelo;
        var tAisl = (TemperaturaAislamiento)d.TemperaturaAislamientoC;
        var tTerm = (TemperaturaAislamiento)d.TemperaturaTerminalesC;
        var metodo = MetodoInstalacion.CanalizacionOCable;
        decimal? Util(Calibre cal)
        {
            if (_motor.Ampacidad.Ampacidad(cal, Datos.MaterialConductor, tAisl, metodo) is not decimal propia)
                return null;
            var corregida = propia * d.FactorTemperatura * d.FactorAgrupamiento;
            return tAisl == tTerm ? corregida
                : _motor.Ampacidad.Ampacidad(cal, Datos.MaterialConductor, tTerm, metodo) is decimal tope ? Math.Min(corregida, tope) : null;
        }
        // 215-2(a)(2): no menor que la tierra de 250-122, «excepto que no se debe aplicar 250-122(f) cuando
        // los conductores puestos a tierra estén instalados en paralelo»: en paralelo cumple el juego, con
        // el área de los N (David, 2026-10-03, I-165).
        // 250-24(c)(1) en un equipo de acometida: no menor que la Tabla 250-66 y, con más de 1100 kcmil de
        // cobre (1750 de aluminio) en las fases, que el 12.5 % de su área; en paralelo, cada uno — (c)(2).
        // Es la misma regla que la del puente de unión principal (250-28(d)(1)): se toma de ahí. Antes solo
        // se pedía la tabla — revisión de cabos sueltos del 2026-10-03.
        var pisos = new List<Calibre>();
        var acometida = TierraDeAcometidaDe(r);
        if (acometida is not null)
            pisos.Add(acometida.PuenteDeUnion);
        var calibre = _motor.Calibres.Listar().OrderBy(c => c.AreaMm2).FirstOrDefault(c =>
            Util(c) is decimal u && u * n >= corriente
            && c.AreaMm2 * n >= r.CalibreTierra.AreaMm2
            && pisos.All(p => c.AreaMm2 >= p.AreaMm2)
            && (n == 1 || c.PermiteParalelo)) ?? r.CalibreFase;
        if (calibre.AreaMm2 >= r.CalibreFase.AreaMm2)
            calibre = r.CalibreFase;

        // R3-2 (ronda 3): EL NEUTRO REDUCIDO CAE MÁS. La caída de cada fase lleva la del neutro, que
        // regresa la suma fasorial: con su calibre, no con el de la fase. Si así pasa del límite, el
        // neutro sube hasta que no pase (o hasta la fase, que ya cumplía).
        var porCaida = false;
        var caida = CaidaConNeutro(r, calibre, canal);
        while (caida is { } c0 && c0.Max(x => x.CaidaPct) > Datos.CaidaMaxAlimentadorPct && calibre.AreaMm2 < r.CalibreFase.AreaMm2)
        {
            calibre = _motor.Calibres.Listar().OrderBy(c => c.AreaMm2).First(c => c.AreaMm2 > calibre.AreaMm2);
            porCaida = true;
            caida = CaidaConNeutro(r, calibre, canal);
        }

        var porConductor = n > 1 ? $" ({corriente / n:N2} A por conductor, {n} en paralelo)" : "";
        var igual = calibre.Designacion == r.CalibreFase.Designacion;
        var cita = new Cita("220-61",
            $"Neutro a su carga de desbalance: la mayor entre el neutro y una fase es la de la fase {fase}, {desbalance:N2} A" +
            (con70 ? $"; lo que pasa de 200 A, al 70 % — 220-61(b)(2): {corriente:N2} A" : "") +
            (fija[fase] > 0m ? $" (los circuitos de 2 polos con neutro, {fija[fase]:N2} A, sin reducir — 220-61(c)(1))" : "") +
            $" -> neutro {calibre}{porConductor}, no menor que el conductor de puesta a tierra de equipos de 250-122 ({r.CalibreTierra})" +
            (n > 1 ? $", con el área de los {n} en paralelo ({n} × {calibre.AreaMm2:0.##} = {n * calibre.AreaMm2:0.##} mm² contra {r.CalibreTierra.AreaMm2:0.##} mm²; 250-122(f) no se aplica) — 215-2(a)(2); cada uno de 1/0 AWG o mayor — 310-10(h)(1)" : " — 215-2(a)(2)") +
            (acometida is null ? ""
                : acometida.PuentePorPorcentaje
                    ? $" ni que el 12.5 % del área de las fases de la acometida ({acometida.AreaAcometidaMm2:0.##} mm² → {acometida.PuenteDeUnion}) — 250-24(c)(1)" + (n > 1 ? CadaUnoEnParalelo : "")
                    : $" ni que la Tabla 250-66 ({acometida.ConductorElectrodo}) — 250-24(c)(1)" + (n > 1 ? CadaUnoEnParalelo : "")) +
            (porCaida ? $"; sube a {calibre} para que la caída de tensión, que lleva la del neutro, no pase de {Datos.CaidaMaxAlimentadorPct:0.##} %" : "") +
            (igual ? "; queda igual que la fase." : "."));
        var citas = new List<Cita>(r.Citas) { cita };
        if (!igual && caida is { } porFase && r.CaidaPorFase is { Count: > 0 } antes)
        {
            var peor = porFase.Aggregate((m, f) => f.CaidaPct > m.CaidaPct ? f : m);
            citas.Add(new Cita("Tabla 9",
                $"Caída por fase con el neutro de {calibre}: " + string.Join("; ", porFase.Select(f => $"{f.Fase} {f.CaidaPct:0.00}%")) +
                $" -- manda la fase {peor.Fase} (con el neutro igual a la fase, {antes.Max(f => f.CaidaPct):0.00}%)."));
            r = r with
            {
                CaidaPorFase = porFase,
                CaidaTensionPct = peor.CaidaPct,
                Detalle = d with { CaidaTensionV = peor.CaidaV },
            };
        }
        return (r with { CalibreNeutro = calibre, Citas = citas },
            new NeutroDe220_61(fase, desbalance, corriente, calibre, con70, igual ? null : ImpedanciaDe(calibre, canal),
                igual ? null : CalibreConImpedancia(calibre, canal)));
    }

    /// <summary>
    /// La caída de cada fase con un neutro de otro calibre — R3-2. La misma de <see cref="CaidaPorFase"/>, con
    /// la Z del neutro aparte: e_f = Re[ (Z_f · I_f + Z_N · I_N) · conj(û_f) ]. Las corrientes, con demanda,
    /// son las que ya usó el motor. Null si el resultado no trae caída por fase. Un calibre sin R ni X en la
    /// Tabla 9 toma las del menor más cercano con datos (más resistencia: del lado seguro).
    /// </summary>
    private PowerNode.DesignSuite.Calculo.TablasNom.ImpedanciaConductor? ImpedanciaDe(Calibre calibre, CanalizacionDelTablero canal) =>
        CalibreConImpedancia(calibre, canal) is { } conDatos
            ? _motor.Impedancia.Impedancia(conDatos, Datos.MaterialConductor, canal.MaterialParaTabla9)
            : null;

    /// <summary>El calibre del que se leen R y X: el mismo, o el menor más cercano que sí está en la Tabla 9.</summary>
    private Calibre? CalibreConImpedancia(Calibre calibre, CanalizacionDelTablero canal) =>
        _motor.Calibres.Listar().Where(c => c.AreaMm2 <= calibre.AreaMm2).OrderByDescending(c => c.AreaMm2)
            .FirstOrDefault(c => _motor.Impedancia.Impedancia(c, Datos.MaterialConductor, canal.MaterialParaTabla9) is not null);

    private IReadOnlyList<CaidaDeFase>? CaidaConNeutro(ResultadoAlimentador r, Calibre neutro, CanalizacionDelTablero canal)
    {
        if (r.CaidaPorFase is not { Count: > 0 } fases || r.CorrienteNeutro is not { } iN || r.Detalle is not { } d)
            return null;
        if (ImpedanciaDe(neutro, canal) is not { } z)
            return null;
        var km = Datos.LongitudAlimentadorM / 1000m / r.NumeroConductoresParalelo;
        double rf = (double)(d.ResistenciaOhmKm * km), xf = (double)(d.ReactanciaOhmKm * km);
        double rn = (double)(z.ROhmKm * km), xn = (double)(z.XOhmKm * km);
        return
        [
            .. fases.Select(f =>
            {
                var u = AnguloDeTension(f.Fase);
                var i = new Fasor(f.Corriente.Magnitud, f.Corriente.AnguloGrados - u);
                var n = new Fasor(iN.Magnitud, iN.AnguloGrados - u);
                var caidaV = (decimal)(rf * (double)i.Real - xf * (double)i.Imaginario + rn * (double)n.Real - xn * (double)n.Imaginario);
                return new CaidaDeFase(f.Fase, f.Corriente, caidaV, caidaV * 100m / Datos.TensionFaseNeutroV);
            }),
        ];
    }

    /// <summary>
    /// <b>La caída combinada alimentador + derivado, por circuito</b> — R-01. La suma y la comparación
    /// son de <see cref="CaidaTensionAcumulada"/>, del motor; aquí solo se arma la ruta de dos tramos.
    ///
    /// <para>
    /// La caída del alimentador es la de <b>la fase del circuito</b> (R-02): la mayor de las que toca
    /// si es multipolar. Sin caída por fase (alimentador sin neutro), la del alimentador.
    /// </para>
    /// </summary>
    private void EvaluarCaidaCombinada()
    {
        if (Alimentador.Resultado is not { } alimentador)
            return;

        foreach (var c in _circuitos.Where(c => c.Resultado is not null))
        {
            var caidaAlimentador = alimentador.CaidaPorFase?
                .Where(f => c.Fases.Contains(f.Fase))
                .Select(f => f.CaidaPct)
                .DefaultIfEmpty(alimentador.CaidaTensionPct)
                .Max() ?? alimentador.CaidaTensionPct;
            var r = CaidaTensionAcumulada.Evaluar(
                [new TramoCaida("Alimentador", caidaAlimentador), new TramoCaida($"Circuito {c.Espacio}", c.Resultado!.CaidaTensionPct)],
                DatosDelTablero.CaidaMaxCombinadaPct);

            c.CaidaCombinadaPct = r.AcumuladaPct;
            c.CaidaAlimentadorPct = caidaAlimentador;
            if (r.ExcedeLimite)
                c.AvisoCaidaCombinada =
                    $"Caída combinada del circuito {c.Espacio}: alimentador {caidaAlimentador:N2} % + circuito " +
                    $"{c.Resultado.CaidaTensionPct:N2} % = {r.AcumuladaPct:N2} %, mayor que el " +
                    $"{DatosDelTablero.CaidaMaxCombinadaPct:N0} % recomendado — 215-2(a)(4) NOTA 2, 210-19(a)(1) NOTA 4.";
        }
    }

    /// <summary>Los circuitos con caída combinada mayor que 5 %, en orden de espacio.</summary>
    public IEnumerable<CircuitoDelCuadro> ConCaidaCombinadaExcedida =>
        _circuitos.Where(c => c.AvisoCaidaCombinada is not null);

    private IReadOnlyList<string> Avisos(ResultadoAlimentador resultado)
    {
        var avisos = new List<string>();

        // 408-36: el dispositivo que protege al tablero contra la capacidad de su barra. Calla
        // cuando falta el dato, que es lo correcto: no se declara un incumplimiento por un campo
        // vacío.
        if (Verificacion408_36.Verificar(
                Datos.CapacidadBarraA, resultado.ProteccionA,
                Datos.UsaInterruptorPrincipal, tieneAlimentadorEntrante: true).Aviso is { } aviso408)
            avisos.Add(aviso408);

        var mayor = _circuitos
            .Where(c => c.Resultado is not null)
            .OrderByDescending(c => c.Resultado!.ProteccionA)
            .ThenBy(c => c.Espacio)
            .FirstOrDefault();
        var mayorDerivado = mayor?.Resultado!.ProteccionA ?? 0m;

        // M-03: un principal más chico que un derivado. No es criterio de diseño, es un error de
        // coordinación básico: el principal se dispara con una carga que el derivado sí admite.
        // Aviso, no bloqueo, mientras David no decida otra cosa.
        //
        // CON UN MOTOR, EL DERIVADO ES GRANDE A PROPÓSITO (I-15): 430-52 lo dimensiona para el
        // arranque, no para la carga; en un equipo de A/C, 440-22(a) o su placa (I-74). Lo que hay que
        // decir es hasta dónde deja subir el principal 430-62(a), no que la carga capturada esté mal.
        if (mayor is { EsDeMotor: true } && resultado.ProteccionA < mayorDerivado)
            avisos.Add(
                $"El interruptor principal ({resultado.ProteccionA:N0} A) es menor que la protección del " +
                $"{(mayor.EsMotor ? "motor" : "equipo de A/C")} del circuito {mayor.Espacio} ({mayorDerivado:N0} A), " +
                // La regla con que se calculó, no si el circuito se captura como grupo: un grupo de un solo
                // motor —como se captura en el desplegable— va por 430-52, no por 430-53(c)(4).
                (mayor.Resultado!.Grupo is { } grupoMayor ? $"que {grupoMayor.Regla} dimensiona para el arranque"
                    : mayor.EsVariador ? "la que marca el fabricante del variador — 110-3(b)"
                    : mayor.EsMotor ? (mayor.Resultado!.Rango is { EsElMaximo: false }
                        ? "escogida dentro del rango de 430-52(c)(1)"
                        : "que 430-52 dimensiona para el arranque")
                    : mayor.PlacaAire == PlacaDeAireAcondicionado.AmpacidadYProteccion ? "la que permite su placa — 440-4(b)"
                    : mayor.PlacaAire == PlacaDeAireAcondicionado.Habitacion ? "la de su circuito — 440-62"
                    : "que 440-22(a) dimensiona para el arranque") +
                $". El principal podría dispararse al arrancar el {(mayor.EsMotor ? "motor" : "equipo")}" +
                (resultado.TechoProteccion430_62A is { } techo
                    ? $": 430-62(a) y 430-63 permiten subirlo hasta {techo:N2} A. Criterio del proyectista."
                    : ". Criterio del proyectista."));
        else if (mayor is not null && resultado.ProteccionA < mayorDerivado)
            avisos.Add(
                $"El interruptor principal ({resultado.ProteccionA:N0} A) es menor que el derivado más grande " +
                $"({mayorDerivado:N0} A, circuito {mayor.Espacio}). El principal se dispararía con una carga que ese " +
                "derivado sí admite: revisa la carga capturada o sube el principal.");

        // 430-62(a): con motores la protección del alimentador tiene TECHO, y el redondeo al tamaño
        // estándar lo puede rebasar. El motor ya retiró el aviso si el conductor lo permite (430-62(b)).
        if (resultado.ProteccionExcedeTecho430_62 && resultado.TechoProteccion430_62A is { } maximo)
            avisos.Add(
                $"El interruptor principal ({resultado.ProteccionA:N0} A) excede el máximo de 430-62(a) y 430-63 ({maximo:N2} A): " +
                "la máxima que permite 430-52 al mayor derivado de motor (no la instalada) más las demás cargas. Revisa los motores o elige un tamaño que no pase del máximo.");

        // Los dos criterios de diseño que NO son de la norma (vienen del Excel). Se REPORTAN, no se
        // aplican: el número que se imprime sale del motor, y el criterio lo decide quien firma. Los
        // textos no mencionan el Excel —quien usa la página no sabe cuál es—; el origen vive en
        // docs/decisiones/interruptor-principal-criterios-del-excel.md (CONFIRMADA · David · 2026-09-23).
        if (resultado.ProteccionA > 0m && resultado.ProteccionA == mayorDerivado)
            avisos.Add(
                $"El interruptor principal quedó igual que el derivado más grande ({resultado.ProteccionA:N0} A). " +
                "La NOM lo permite; subirlo un tamaño ayuda a que, ante una falla en ese circuito, se dispare el " +
                "derivado y no el principal. Criterio del proyectista.");

        // Riel DIN se acaba en 125 A. Arriba de eso la serie no tiene tamaño y se tomó el de la NOM:
        // se dice, porque ese interruptor ya no es de riel DIN.
        if (Datos.SerieInterruptores == SerieDeInterruptores.RielDinIec)
        {
            var circuitos = _circuitos
                .Where(c => c.Resultado is { ProteccionA: > SeriesDeInterruptores.MaximoRielDinA })
                .Select(c => (c.Espacio, c.Resultado!.ProteccionA))
                .ToList();
            (string, decimal)? principal = resultado.ProteccionA > SeriesDeInterruptores.MaximoRielDinA
                ? (Datos.UsaInterruptorPrincipal ? "el principal" : "la protección del alimentador", resultado.ProteccionA)
                : null;
            if (AvisoRielDin(circuitos, principal) is { } avisoDin)
                avisos.Add(avisoDin);

            // Por abajo también se acaba (16 A): un motor cuyo máximo por la Tabla 430-52 es 15 A, o un
            // A/C en el mínimo de 440-22(a), toman 15 A de la lista de 240-6(a) — P1-1.
            var chicos = _circuitos
                .Where(c => c.Resultado is { } r && r.ProteccionA <= SeriesDeInterruptores.MaximoRielDinA
                            && !SerieDeInterruptores.RielDinIec.Admite(r.ProteccionA))
                .Select(c => $"{c.Espacio} ({c.Resultado!.ProteccionA:N0} A)")
                .ToList();
            if (chicos.Count > 0)
                avisos.Add(
                    $"En riel DIN el interruptor más chico es de {SeriesDeInterruptores.RielDin[0]:N0} A. " +
                    (chicos.Count == 1 ? $"El circuito {chicos[0]} no puede" : $"Los circuitos {Enumerar(chicos)} no pueden") +
                    " pasar de su máximo, así que se tomó de la lista completa de 240-6(a); ese tamaño no es de riel DIN.");
        }

        // R-12: el factor de demanda lo decide el proyectista, pero no sin sustento. Uno por tipo (R-17).
        foreach (var categoria in Datos.SinJustificacion)
            avisos.Add(
                $"El factor de demanda de {categoria.NombreCompleto().ToLowerInvariant()} es menor que 1 y no tiene justificación. " +
                "Escoge en «Resumen de carga» la tabla o sección del Art. 220 que lo sustenta — 220-40.");

        if (AvisoDe220_56() is { } cocina)
            avisos.Add(cocina);

        // 210-11(c)(1): los circuitos de aparatos pequeños son DOS O MÁS. Con uno solo capturado, se
        // dice; con ninguno, no hay nada que decir. Solo en vivienda: fuera de ella el uso no cuenta (I-46).
        var aparatos = _circuitos.Where(c => c.TieneCarga && c.UsoEfectivo == UsoDeContactos.AparatosPequenos).ToList();
        if (aparatos.Count == 1)
            avisos.Add(
                $"Solo el circuito {aparatos[0].Espacio} es de aparatos pequeños. La vivienda exige dos o más circuitos " +
                "de 20 A para los contactos de cocina, despensa y comedor — 210-11(c)(1).");

        return avisos;
    }

    /// <summary>
    /// <b>El piso de 220-56</b> — I-155: fuera de vivienda, el factor de demanda del equipo de cocina «en
    /// ningún caso» deja la carga del alimentador abajo de la suma de los dos equipos más grandes. Solo avisa:
    /// el factor es del proyectista (R-12). No aplica con la Tabla 220-88, que sustituye a la Parte C.
    /// </summary>
    private string? AvisoDe220_56()
    {
        var factor = Datos.FactorDeDemanda(CategoriaDeCarga.Equipo);
        if (factor >= 1m || Datos.Inmueble.EsVivienda()
            || Datos.Justificaciones[CategoriaDeCarga.Equipo].Contains(JustificacionFactorDemanda.RestauranteNuevo))
            return null;
        // Cada unidad por separado: una línea de 2 freidoras son dos equipos.
        var equipos = _circuitos
            .Where(c => c.TieneCarga && c.TieneDesglose && !c.OmitidoPorNoSimultaneo)
            .SelectMany(c => c.Cargas.Where(a => a.Subtipo == SubtipoDeCarga.CocinaComercial && c.TipoDe(a) == CategoriaDeCarga.Equipo))
            .SelectMany(a => Enumerable.Repeat(a.TotalVA / a.Cantidad, a.Cantidad))
            .OrderByDescending(va => va)
            .ToList();
        if (equipos.Count == 0)
            return null;
        var total = equipos.Sum();
        var demandada = total * factor;
        var piso = equipos.Take(2).Sum();
        if (demandada >= piso)
            return null;
        return $"El equipo de cocina queda en {demandada:N0} VA con el factor de demanda de {factor:0.00}; 220-56 no deja que la carga " +
               $"del alimentador baje de la suma de los dos equipos más grandes, {piso:N0} VA. Sube el factor de aparatos a " +
               $"{Math.Ceiling(piso / total * 100m) / 100m:0.00} o más.";
    }

    /// <summary>
    /// «En riel DIN no hay interruptores de más de 125 A. El circuito 2 (150 A) y el principal (175 A)
    /// se calcularon con la lista completa de 240-6(a); esos tamaños ya no son de riel DIN.» Singular
    /// o plural según cuántos sean — R-06. <c>null</c> si no hay ninguno.
    /// </summary>
    private static string? AvisoRielDin(IReadOnlyList<(int Espacio, decimal ProteccionA)> circuitos, (string Nombre, decimal ProteccionA)? principal)
    {
        var partes = new List<string>();
        if (circuitos.Count == 1)
            partes.Add($"el circuito {circuitos[0].Espacio} ({circuitos[0].ProteccionA:N0} A)");
        else if (circuitos.Count > 1)
            partes.Add($"los circuitos {Enumerar(circuitos.Select(c => $"{c.Espacio}"))} " +
                       $"({Enumerar(circuitos.Select(c => $"{c.ProteccionA:N0} A"))})");
        if (principal is { } p)
            partes.Add($"{p.Nombre} ({p.ProteccionA:N0} A)");

        var total = circuitos.Count + (principal is null ? 0 : 1);
        if (total == 0)
            return null;

        var sujeto = Enumerar(partes);
        return $"En riel DIN no hay interruptores de más de {SeriesDeInterruptores.MaximoRielDinA:N0} A. " +
               char.ToUpperInvariant(sujeto[0]) + sujeto[1..] +
               (total == 1
                   ? " se calculó con la lista completa de 240-6(a); ese tamaño ya no es de riel DIN."
                   : " se calcularon con la lista completa de 240-6(a); esos tamaños ya no son de riel DIN.");
    }

    /// <summary>«a», «a y b», «a, b y c».</summary>
    private static string Enumerar(IEnumerable<string> elementos)
    {
        var lista = elementos.ToList();
        return lista.Count <= 1 ? string.Concat(lista) : string.Join(", ", lista[..^1]) + " y " + lista[^1];
    }

    private static ResumenDeCarga Vacio() =>
        new(0m, 0m, 0m, 0m, new Dictionary<char, decimal>(), 0m);
}
