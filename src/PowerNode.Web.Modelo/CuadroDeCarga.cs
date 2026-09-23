using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Tableros;
using PowerNode.DesignSuite.Calculo.Validaciones;

namespace PowerNode.Web.Modelo;

/// <summary>El resumen de carga del encabezado del Excel (bloque «RESUMEN DE CARGA», filas 21 a 26).</summary>
public sealed record ResumenDeCarga(
    decimal ContinuaVA,
    decimal FactorDemandaContinua,
    decimal ContinuaDemandadaVA,
    decimal NoContinuaVA,
    decimal FactorDemandaNoContinua,
    decimal NoContinuaDemandadaVA,
    IReadOnlyDictionary<char, decimal> CargaPorFaseVA,
    decimal DesbalanceoPct)
{
    public decimal InstaladaVA => ContinuaVA + NoContinuaVA;
    public decimal DemandadaVA => ContinuaDemandadaVA + NoContinuaDemandadaVA;
}

/// <summary>
/// Lo que lleva una barra del alimentador, en amperes: la parte continua y la no continua ya con su
/// factor de demanda, y la capacidad que piden juntas (215-2(a)(1): 125 % de la continua + 100 % de
/// la no continua).
/// </summary>
/// <param name="FactorContinua">1.25, o 1.00 con el ensamble aprobado al 100 % — 215-3.</param>
public sealed record CorrienteDeFase(char Fase, decimal ContinuaA, decimal NoContinuaA, decimal FactorContinua)
{
    public decimal TotalA => ContinuaA + NoContinuaA;
    public decimal CapacidadA => FactorContinua * ContinuaA + NoContinuaA;
}

/// <summary>
/// El renglón del alimentador: la fila 79 del Excel, «ALIMENTADOR Y PROTECCIÓN PRINCIPAL».
/// <see cref="Resultado"/> trae el interruptor principal en <c>ProteccionA</c>.
/// </summary>
/// <param name="Fases">La corriente de cada barra, en el orden de las barras.</param>
/// <param name="Gobierna">La fase más cargada, que es con la que se dimensiona. <c>null</c> sin carga.</param>
public sealed record RenglonDelAlimentador(
    ResultadoAlimentador? Resultado,
    string? Error,
    IReadOnlyList<string> Avisos,
    int Polos,
    IReadOnlyList<CorrienteDeFase>? Fases = null,
    CorrienteDeFase? Gobierna = null);

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

    /// <summary>Un renglón por espacio del tablero, del 1 al <see cref="DatosDelTablero.NumeroEspacios"/>.</summary>
    public IReadOnlyList<CircuitoDelCuadro> Circuitos => _circuitos;

    public IEnumerable<CircuitoDelCuadro> Nones => _circuitos.Where(c => c.Espacio % 2 == 1);
    public IEnumerable<CircuitoDelCuadro> Pares => _circuitos.Where(c => c.Espacio % 2 == 0);

    public ResumenDeCarga Resumen { get; private set; } = Vacio();

    public RenglonDelAlimentador Alimentador { get; private set; } = new(null, null, [], 3);

    /// <summary>
    /// El interior del tablero dibujado: un bloque por interruptor, con su renglón, su columna y
    /// cuántos espacios abarca. Nones a la izquierda y pares a la derecha — <b>la geometría no se
    /// decide aquí</b>, sale de <see cref="DistribucionBarras"/>.
    /// </summary>
    public IReadOnlyList<BloqueDelGabinete> Gabinete { get; private set; } = [];

    /// <summary>Espacios con un interruptor con carga capturada. Un multipolar cuenta por sus polos.</summary>
    public int EspaciosOcupados => Gabinete.Where(b => b.Ocupado).Sum(b => b.Espacios);

    public int EspaciosLibres => Datos.NumeroEspacios - EspaciosOcupados;

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
        ResolverOcupacion();
        ConvertirCargas();
        CalcularCircuitos();
        DibujarGabinete();
        CalcularResumen();
        CalcularAlimentador();
    }

    /// <summary>
    /// Cambia los polos de un interruptor. Devuelve <b>el motivo por el que no se pudo</b>, ya
    /// redactado, o <c>null</c> si se aplicó — mismo criterio que el editor de gabinete de
    /// escritorio: soltar sin explicación se lee como que el programa se trabó.
    /// </summary>
    public string? CambiarPolos(CircuitoDelCuadro circuito, int polos)
    {
        if (!DistribucionBarras.PolosValidos(polos, Datos.Sistema))
            return $"Este tablero tiene {Datos.Barras.Count} barra(s), así que un interruptor de {polos} polos repetiría fase.";

        var ocupados = _circuitos
            .Where(c => c != circuito && !c.EsContinuacion && c.Polos > 1)
            .Select(c => new MontajeEnGabinete(c.Espacio, c.Polos, $"el circuito {c.Espacio}"))
            .ToList();

        var motivo = AcomodoEnGabinete.MotivoNoCabe(circuito.Espacio, polos, Datos.NumeroEspacios, ocupados);
        if (motivo is not null)
            return motivo;

        circuito.Polos = polos;
        Recalcular();
        return null;
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

            foreach (var ocupado in DistribucionBarras.EspaciosQueOcupa(c.Espacio, c.Polos).Skip(1))
                _circuitos[ocupado - 1].ContinuacionDe = c.Espacio;
        }
    }

    /// <summary>
    /// De lo que dice la placa (VA, W o A) a volt-amperes — I-25. <b>La regla no se escribe aquí</b>:
    /// es <see cref="ConsumoDePlaca.AVoltAmperes"/>, la misma que usa el escritorio, con la tensión y
    /// los polos del circuito para que unos amperes capturados regresen como los mismos amperes
    /// calculados.
    /// </summary>
    private void ConvertirCargas()
    {
        foreach (var c in _circuitos)
        {
            c.ContinuaVA = AVoltAmperes(c, c.Continua);
            c.NoContinuaVA = AVoltAmperes(c, c.NoContinua);
        }
    }

    private decimal AVoltAmperes(CircuitoDelCuadro c, decimal valor) =>
        ConsumoDePlaca.AVoltAmperes(
            valor, c.Unidad, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV, c.Polos, Datos.FactorPotencia);

    private void CalcularCircuitos()
    {
        foreach (var c in _circuitos)
        {
            c.Limpiar();
            c.Fases = c.EsContinuacion
                ? string.Empty
                : DistribucionBarras.FasesQueOcupa(c.Espacio, c.Polos, Datos.Sistema);

            if (!c.TieneCarga)
                continue;

            try
            {
                c.Resultado = _motor.NoMotor.Calcular(new DatosEntradaCircuitoDerivadoNoMotor(
                    TipoCarga: c.Tipo,
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
                    NumeroConductoresAgrupados: Datos.ConductoresAgrupados,
                    TemperaturaAmbienteC: Datos.TemperaturaAmbienteC,
                    MaterialConductor: Datos.MaterialConductor,
                    MaterialCanalizacion: Datos.MaterialCanalizacion,
                    FactorPotencia: Datos.FactorPotencia,
                    CaidaTensionMaxPct: Datos.CaidaMaxDerivadoPct,
                    // SIN PISO PRÁCTICO DE CALIBRE -- va null a propósito, y es una diferencia
                    // deliberada con la versión de escritorio, que lo trae encendido por omisión
                    // (12 AWG en alumbrado, 10 en contactos). Lo quitó David el 2026-09-22 con un
                    // caso concreto: contactos salía en 10 AWG y un equipo con la misma carga por
                    // fase en 12, y esa diferencia no la produce ningún artículo de la norma, la
                    // producía el piso. Ver docs/decisiones/sin-piso-practico-de-calibre.md.
                    PisoPracticoCalibreMm2: null));
            }
            catch (Exception ex)
            {
                c.Error = ex.Message;
            }
        }
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
                    var espacios = DistribucionBarras.EspaciosQueOcupa(c.Espacio, c.Polos);

                    return new BloqueDelGabinete(
                        Circuito: c,
                        // El espacio 1 y el 2 están en el primer renglón; el 3 y el 4, en el segundo.
                        Fila: (c.Espacio + 1) / 2,
                        Columna: c.Espacio % 2 == 1 ? 1 : 2,
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

        Resumen = new ResumenDeCarga(
            ContinuaVA: continua,
            FactorDemandaContinua: Datos.FactorDemandaContinua,
            ContinuaDemandadaVA: continua * Datos.FactorDemandaContinua,
            NoContinuaVA: noContinua,
            FactorDemandaNoContinua: Datos.FactorDemandaNoContinua,
            NoContinuaDemandadaVA: noContinua * Datos.FactorDemandaNoContinua,
            CargaPorFaseVA: porFase,
            DesbalanceoPct: desbalanceo);
    }

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

        IReadOnlyDictionary<char, decimal> Sumar(Func<CircuitoDelCuadro, decimal> va, decimal factorDemanda) =>
            CalculadoraDesbalanceo.CorrientePorFase(
                [.. conCarga.Select(c => new CorrientePorCircuito(
                    c.Fases,
                    factorDemanda * va(c) / TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV)))],
                Datos.Barras);

        var continua = Sumar(c => c.ContinuaVA, Datos.FactorDemandaContinua);
        var noContinua = Sumar(c => c.NoContinuaVA, Datos.FactorDemandaNoContinua);

        // El mismo 125 % (o 100 %, con el ensamble aprobado) que aplica la calculadora del
        // alimentador: la fase que gobierna es la que pide más capacidad, no la de más corriente.
        var factor = CargaContinua100Pct.Para(
            Datos.ConjuntoAprobado100Pct, null,
            ClaseDeTramo.Alimentador.CapacidadMinima(), ClaseDeTramo.Alimentador.Excepcion100Pct()).Factor;

        return
        [
            .. Datos.Barras.Select(f => new CorrienteDeFase(f, continua[f], noContinua[f], factor))
        ];
    }

    private void CalcularAlimentador()
    {
        var polos = Datos.Barras.Count;

        if (Resumen.InstaladaVA <= 0m)
        {
            Alimentador = new RenglonDelAlimentador(null, null, [], polos);
            return;
        }

        var fases = CorrientesPorFase();
        // Empate: gana la primera barra, que es determinista y es como se lee el tablero.
        var gobierna = fases.Aggregate((max, f) => f.CapacidadA > max.CapacidadA ? f : max);

        try
        {
            // LA FASE MÁS CARGADA, COMO SI LAS DEMÁS LLEVARAN LO MISMO. La calculadora copiada del
            // escritorio divide la carga entre √3·V_FF (o la tensión que toque al sistema), así que se
            // le entrega la carga que da exactamente la corriente de esa fase: su corriente por el
            // mismo divisor. En un 3F-4H eso es 3 × los VA de la fase. Así la protección, el conductor
            // y la caída de tensión salen con la corriente real de la barra que más lleva, sin
            // reescribir el motor. Ver M-02 en docs/estado/HALLAZGOS.md.
            //
            // El factor de demanda NO se multiplica aquí: la calculadora lo aplica y lo deja escrito
            // en la memoria con su cita del 220-40, que es lo que tiene que ver quien revisa por qué
            // el alimentador lleva menos cobre. Por eso se deshace en la corriente de la fase antes
            // de convertirla — el motor lo vuelve a aplicar.
            var divisor = TensionDeCalculo.Divisor(polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
            var resultado = _motor.Alimentador.Calcular(new DatosEntradaAlimentador(
                CargaContinuaVA: SinDemanda(gobierna.ContinuaA, Datos.FactorDemandaContinua) * divisor,
                CargaNoContinuaVA: SinDemanda(gobierna.NoContinuaA, Datos.FactorDemandaNoContinua) * divisor,
                NumeroFases: polos,
                TensionFaseNeutroV: Datos.TensionFaseNeutroV,
                TensionFaseFaseV: Datos.TensionFaseFaseV,
                LongitudM: Datos.LongitudAlimentadorM,
                NumeroConductoresParalelo: 1,
                NumeroConductoresAgrupados: Datos.ConductoresAgrupados,
                TemperaturaAmbienteC: Datos.TemperaturaAmbienteC,
                MaterialConductor: Datos.MaterialConductor,
                MaterialCanalizacion: Datos.MaterialCanalizacion,
                FactorPotencia: Datos.FactorPotencia,
                CaidaTensionMaxPct: Datos.CaidaMaxAlimentadorPct,
                PisoPracticoCalibreMm2: null,
                FactorDemandaContinua: Datos.FactorDemandaContinua,
                FactorDemandaNoContinua: Datos.FactorDemandaNoContinua,
                ConjuntoAprobado100Pct: Datos.ConjuntoAprobado100Pct));

            resultado = resultado with { Citas = [.. resultado.Citas.Select(c => c.Referencia == "220-40" ? Cita220_40(gobierna) : c)] };
            Alimentador = new RenglonDelAlimentador(resultado, null, Avisos(resultado), polos, fases, gobierna);
        }
        catch (Exception ex)
        {
            Alimentador = new RenglonDelAlimentador(null, ex.Message, [], polos, fases, gobierna);
        }
    }

    /// <summary>
    /// La cita del 220-40 con la carga <b>del tablero</b>. La que escribe la calculadora habla de la
    /// carga que recibió, que aquí es la equivalente de la fase que gobierna (3 × sus VA en un
    /// 3F-4H): cierta, pero ilegible para quien revisa la memoria contra el resumen de carga.
    /// </summary>
    private Cita Cita220_40(CorrienteDeFase gobierna) => new("220-40",
        $"Factor de demanda sobre la carga acumulada: continua {Resumen.ContinuaVA:0.##} VA x {Datos.FactorDemandaContinua} = "
        + $"{Resumen.ContinuaDemandadaVA:0.##} VA; no continua {Resumen.NoContinuaVA:0.##} VA x {Datos.FactorDemandaNoContinua} = "
        + $"{Resumen.NoContinuaDemandadaVA:0.##} VA. Se aplica antes de elegir la fase más cargada (fase {gobierna.Fase}). "
        + "Es criterio de diseño del proyectista: el Art. 220 no se automatiza.");

    /// <summary>Deshace un factor de demanda. Un factor de cero deja la carga en cero, que es lo que el motor recibiría de todos modos.</summary>
    private static decimal SinDemanda(decimal valor, decimal factorDemanda) =>
        factorDemanda == 0m ? 0m : valor / factorDemanda;

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
        if (mayor is not null && resultado.ProteccionA < mayorDerivado)
            avisos.Add(
                $"El interruptor principal ({resultado.ProteccionA:N0} A) es menor que el derivado más grande " +
                $"({mayorDerivado:N0} A, circuito {mayor.Espacio}). El principal se dispararía con una carga que ese " +
                "derivado sí admite: revisa la carga capturada o sube el principal.");

        // Los dos criterios del Excel que NO son de la norma. Se REPORTAN, no se aplican: el
        // número que se imprime sale del motor, y el criterio de diseño lo decide quien firma.
        // Ver docs/decisiones/interruptor-principal-criterios-del-excel.md.
        if (resultado.ProteccionA > 0m && resultado.ProteccionA == mayorDerivado)
            avisos.Add(
                $"El interruptor principal quedó en {resultado.ProteccionA:N0} A, igual que el derivado más grande. " +
                "El Excel original subía el principal al siguiente tamaño estándar en este caso. Es criterio de " +
                "diseño, no de la NOM: la 240-6(a) no lo pide.");

        if (resultado.ProteccionA is > 0m and < 30m)
            avisos.Add(
                $"El interruptor principal calculado es de {resultado.ProteccionA:N0} A. El Excel original nunca " +
                "bajaba de 30 A. Es criterio de diseño, no de la NOM.");

        return avisos;
    }

    private static ResumenDeCarga Vacio() =>
        new(0m, 1m, 0m, 0m, 1m, 0m, new Dictionary<char, decimal>(), 0m);
}
