using PowerNode.DesignSuite.Calculo.Canalizaciones;
using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Magnitudes;
using PowerNode.DesignSuite.Calculo.Tableros;
using PowerNode.DesignSuite.Calculo.Validaciones;

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
    IReadOnlyList<CargaPorCategoria>? PorCategoria = null)
{
    public decimal InstaladaVA => ContinuaVA + NoContinuaVA;

    /// <summary>La carga que va al alimentador: la instalada más el mínimo de 220-52.</summary>
    public decimal CalculadaVA => InstaladaVA + Minimo220_52VA;
    public decimal DemandadaVA => ContinuaDemandadaVA + NoContinuaDemandadaVA + Minimo220_52DemandadoVA;
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
/// <param name="FactorPotencia">
/// El F.P. <b>de las cargas de la fase que gobierna</b>, combinado. Es el que corresponde a la
/// corriente con la que se calcula la caída de tensión del alimentador.
/// </param>
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
        ResolverFases();
        PrepararCanalizaciones();
        CalcularCircuitos();
        DibujarGabinete();
        CalcularResumen();
        CalcularAlimentador();
        DimensionarCanalizaciones();
        EvaluarCaidaCombinada();
    }

    /// <summary>
    /// Las canalizaciones con sus resultados: las de los derivados (T1, T2…) y al final la del
    /// alimentador.
    /// </summary>
    public IEnumerable<CanalizacionDelTablero> TodasLasCanalizaciones =>
        Datos.Canalizaciones.Append(Datos.CanalizacionAlimentador);

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
    public DesgloseDeSeleccion? Desglose(CircuitoDelCuadro c)
    {
        if (c.Resultado is not { Detalle: { } detalle } r)
            return null;

        var divisor = TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
        var factor = CargaContinua100Pct.Para(Datos.ConjuntoAprobado100Pct, null, "210-20(a)", "210-19(a)(1)").Factor;

        return DesgloseDeSeleccion.De(
            _motor.Ampacidad, Datos,
            iContinuaA: c.ContinuaVA / divisor,
            iNoContinuaA: c.NoContinuaVA / divisor,
            factorContinua: factor,
            articuloProteccion: "210-20(a)",
            articuloConductor: "210-19(a)(1)",
            proteccionA: r.ProteccionA,
            proteccionSinMinimo: TamanoEstandar(detalle.CapacidadMinimaA),
            calibre: r.CalibreFase,
            conductoresPorFase: r.NumeroConductoresParalelo,
            d: detalle,
            citas: r.Citas,
            referenciaMinimo: c.UsoEfectivo.ReferenciaProteccionMinima());
    }

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
            referenciaMinimo: Datos.Minimo230_79?.Referencia);
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
            if (c.TieneDesglose)
                SumarDesglose(c);

            // 424-3(b): la calefacción fija de ambiente es carga continua. Lo capturado como no continua
            // pasa a continua — R-18.
            if (c.Categoria == CategoriaDeCarga.CalefaccionFija && c.NoContinua > 0m)
            {
                c.Continua += c.NoContinua;
                c.NoContinua = 0m;
            }

            c.ContinuaVA = AVoltAmperes(c, c.Continua);
            c.NoContinuaVA = AVoltAmperes(c, c.NoContinua);
            c.Ajuste220_52VA = c.TieneCarga && c.UsoEfectivo.ReferenciaCargaMinima() is not null
                ? Math.Max(0m, UsosDeContactos.CargaMinimaAlimentadorVA - c.CargaInstaladaVA)
                : 0m;
        }
    }

    /// <summary>
    /// <b>La carga del circuito sale de sus aparatos</b> — I-35. Cada uno se convierte a VA con su F.P.
    /// y la tensión y los polos del circuito; el circuito queda en VA, con la suma de los continuos, la
    /// de los no continuos y el F.P. combinado (P / √(P² + Q²)). Un «Contacto» sin carga toma 180 VA
    /// (220-14(i)); en calefacción todos son continuos (424-3(b)).
    /// </summary>
    private void SumarDesglose(CircuitoDelCuadro c)
    {
        foreach (var a in c.Aparatos)
        {
            if (a.EsContactoSinCarga)
            {
                a.Unidad = UnidadConsumo.VoltAmperes;
                a.CargaUnitaria = AparatoDelCircuito.VAPorContacto;
            }
            if (c.Categoria == CategoriaDeCarga.CalefaccionFija)
                a.Continua = true;
            a.Cantidad = Math.Max(1, a.Cantidad);
            a.TotalVA = a.Cantidad * ConsumoDePlaca.AVoltAmperes(
                a.CargaUnitaria, a.Unidad, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV, c.Polos, a.FactorPotencia);
        }

        c.Unidad = UnidadConsumo.VoltAmperes;
        c.Continua = c.Aparatos.Where(a => a.Continua).Sum(a => a.TotalVA);
        c.NoContinua = c.Aparatos.Where(a => !a.Continua).Sum(a => a.TotalVA);
        c.FactorPotencia = c.Continua + c.NoContinua > 0m
            ? FactorPotenciaCombinado.De(c.Aparatos.Select(a => (a.TotalVA, a.FactorPotencia)))
            : CircuitoDelCuadro.FactorPotenciaSupuesto;
    }

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
        var sistemaConNeutro = Configuracion != ConfiguracionTablero.TresFasesTresHilos;
        foreach (var c in _circuitos)
        {
            // I-41: 1 polo siempre con neutro; 2 y 3 polos solo con «+N».
            c.LlevaNeutro = sistemaConNeutro && (c.Polos == 1 || c.ConNeutro);
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
            if (!canal.TierraComun)
                conductores.Add(Conductor(x.Nombre, PapelConductor.Tierra, x.Tierra, juegosPorCanalizacion, canal.TierraDesnuda));
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

    private void CalcularCircuitos()
    {
        foreach (var c in _circuitos)
        {
            c.Limpiar();

            if (!c.TieneCarga || c.CanalizacionEfectiva is not { } canal)
                continue;

            try
            {
                c.Resultado = _motor.NoMotor(Datos.SerieInterruptores).Calcular(new DatosEntradaCircuitoDerivadoNoMotor(
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
                    LugarInstalacionSeco: Datos.LugarSeco,
                    TerminalesMarcadas75C: Datos.TerminalesMarcadas75C,
                    // SIN MÍNIMO POR TIPO DE CARGA: solo el que exige 210-11(c) según el uso.
                    ProteccionMinimaA: c.UsoEfectivo.ReferenciaProteccionMinima() is null ? null : UsosDeContactos.ProteccionMinimaViviendaA,
                    ReferenciaProteccionMinima: c.UsoEfectivo.ReferenciaProteccionMinima()));
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

        // Los kW de verdad: la potencia activa de cada circuito, no los VA totales por un F.P. que el
        // tablero no tiene.
        var conCarga = _circuitos.Where(c => c.TieneCarga).ToList();
        // Con los 1500 VA de 220-52 a su F.P., igual que el total en kVA: si no, la demandada salía
        // mayor que la instalada.
        var instaladaW = conCarga.Sum(c => c.PotenciaActivaW + c.Ajuste220_52VA * c.FactorPotencia);
        // CADA CIRCUITO CON EL FACTOR DE SU TIPO — R-17. La parte continua y la no continua llevan el
        // mismo factor; lo que las distingue es el 125 % del alimentador, no la demanda.
        var demandadaW = conCarga.Sum(c => c.CargaCalculadaVA * FactorDeDemanda(c) * c.FactorPotencia);
        var minimo220_52 = conCarga.Sum(c => c.Ajuste220_52VA);

        Resumen = new ResumenDeCarga(
            ContinuaVA: continua,
            ContinuaDemandadaVA: conCarga.Sum(c => c.ContinuaVA * FactorDeDemanda(c)),
            NoContinuaVA: noContinua,
            NoContinuaDemandadaVA: conCarga.Sum(c => c.NoContinuaVA * FactorDeDemanda(c)),
            CargaPorFaseVA: porFase,
            DesbalanceoPct: desbalanceo,
            InstaladaW: instaladaW,
            DemandadaW: demandadaW,
            FactorPotencia: FactorPotenciaCombinado.De(conCarga.Select(c => (c.CargaInstaladaVA, c.FactorPotencia))),
            Minimo220_52VA: minimo220_52,
            Minimo220_52DemandadoVA: conCarga.Sum(c => c.Ajuste220_52VA * FactorDeDemanda(c)),
            PorCategoria:
            [
                .. Enum.GetValues<CategoriaDeCarga>().Select(categoria =>
                {
                    var instalada = conCarga.Where(c => c.Categoria == categoria).Sum(c => c.CargaInstaladaVA);
                    var factor = Datos.FactorDeDemanda(categoria);
                    return new CargaPorCategoria(categoria, instalada, factor, instalada * factor);
                }),
            ]);
    }

    /// <summary>El factor de demanda del tipo del circuito — R-17.</summary>
    private decimal FactorDeDemanda(CircuitoDelCuadro c) => Datos.FactorDeDemanda(c.Categoria);

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

        // Cada circuito con el factor de demanda de su tipo — R-17.
        IReadOnlyDictionary<char, decimal> Sumar(Func<CircuitoDelCuadro, decimal> va) =>
            CalculadoraDesbalanceo.CorrientePorFase(
                [.. conCarga.Select(c => new CorrientePorCircuito(
                    c.Fases,
                    FactorDeDemanda(c) * va(c) / TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV)))],
                Datos.Barras);

        var continua = Sumar(c => c.ContinuaVA);
        // Los 1500 VA de 220-52 son carga de alimentador: entran aquí, no en el derivado.
        var noContinua = Sumar(c => c.NoContinuaVA + c.Ajuste220_52VA);

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

        foreach (var c in _circuitos.Where(c => c.TieneCarga))
        {
            var divisor = TensionDeCalculo.Divisor(c.Polos, Datos.TensionFaseNeutroV, Datos.TensionFaseFaseV);
            var iContinua = FactorDeDemanda(c) * c.ContinuaVA / divisor;
            var iNoContinua = FactorDeDemanda(c) * (c.NoContinuaVA + c.Ajuste220_52VA) / divisor;
            var theta = (decimal)(Math.Acos((double)c.FactorPotencia) * 180.0 / Math.PI);
            var fases = c.Fases.Where(angulo.ContainsKey).ToList();

            // Ángulo y sentido de la corriente en cada fase que toca.
            IEnumerable<(char Fase, decimal Angulo)> Direcciones()
            {
                if (fases.Count == 2)
                {
                    var vff = new Fasor(1m, angulo[fases[0]]) - new Fasor(1m, angulo[fases[1]]);
                    yield return (fases[0], vff.AnguloGrados - theta);
                    yield return (fases[1], vff.AnguloGrados - theta + 180m);
                }
                else
                    foreach (var f in fases)
                        yield return (f, angulo[f] - theta);
            }

            foreach (var (f, anguloCorriente) in Direcciones())
            {
                continuaA[f] += iContinua;
                noContinuaA[f] += iNoContinua;
                fasorContinua[f] += new Fasor(iContinua, anguloCorriente);
                fasorNoContinua[f] += new Fasor(iNoContinua, anguloCorriente);
            }
        }

        return [.. barras.Select(b => new CorrienteDeFaseAlimentador(
            b, continuaA[b], noContinuaA[b], angulo[b], fasorContinua[b], fasorNoContinua[b]))];
    }

    /// <summary>Ángulo de V fase-neutro: A 0°, B −120°, C 120°; en 1F-3H las dos barras están en oposición.</summary>
    private decimal AnguloDeTension(char barra) =>
        SistemaDelTablero.De(Datos.Sistema) == ConfiguracionTablero.UnaFaseTresHilos
            ? (barra == 'A' ? 0m : 180m)
            : barra switch { 'A' => 0m, 'B' => -120m, _ => 120m };

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

        // EL F.P. DEL ALIMENTADOR NO SE CAPTURA: resulta de las cargas que lleva. Se toman las de la
        // fase que gobierna, porque es la corriente de esa fase la que entra a la caída de tensión.
        // Cada circuito aporta lo que le cuelga a esa barra (sus VA entre sus polos).
        var fpAlimentador = FactorPotenciaCombinado.De(
            _circuitos
                .Where(c => c.TieneCarga && c.Fases.Contains(gobierna.Fase))
                .Select(c => (c.CargaPorFaseVA, c.FactorPotencia)));

        var canal = Datos.CanalizacionAlimentador;
        canal.Limpiar();
        var conNeutro = Configuracion != ConfiguracionTablero.TresFasesTresHilos;

        // EL MOTOR RECIBE LA CARGA TOTAL Y LAS CORRIENTES DE CADA FASE — R-04. Con ellas elige la
        // fase que gobierna, aplica el factor de demanda (220-40, con la carga real en su cita) y
        // calcula la caída fase por fase con el neutro (R-02). Antes la web le entregaba 3 × los VA
        // de la fase más cargada y reescribía la cita 220-40: ya no.
        ResultadoAlimentador Calcular() => _motor.Alimentador(Datos.SerieInterruptores).Calcular(new DatosEntradaAlimentador(
                // Ya con el factor de demanda de cada tipo (R-17): por eso el motor va con F.D. 1 abajo.
                CargaContinuaVA: Resumen.ContinuaDemandadaVA,
                CargaNoContinuaVA: Resumen.NoContinuaDemandadaVA + Resumen.Minimo220_52DemandadoVA,
                NumeroFases: polos,
                TensionFaseNeutroV: Datos.TensionFaseNeutroV,
                TensionFaseFaseV: Datos.TensionFaseFaseV,
                LongitudM: Datos.LongitudAlimentadorM,
                NumeroConductoresParalelo: 1,
                // De SU canalización — I-39: el alimentador ya no hereda el agrupamiento de los derivados.
                NumeroConductoresAgrupados: canal.Ajuste!.ConductoresParaElMotor,
                TemperaturaAmbienteC: Datos.TemperaturaAmbienteC + canal.SumadorAzoteaC,
                MaterialConductor: Datos.MaterialConductor,
                MaterialCanalizacion: canal.MaterialParaTabla9,
                // Solo para la caída balanceada de un alimentador sin neutro (3F-3H).
                FactorPotencia: fpAlimentador,
                CaidaTensionMaxPct: Datos.CaidaMaxAlimentadorPct,
                PisoPracticoCalibreMm2: null,
                FactorDemandaContinua: 1m,
                FactorDemandaNoContinua: 1m,
                TipoAislamiento: Datos.TipoAislamiento,
                LugarInstalacionSeco: Datos.LugarSeco,
                ConjuntoAprobado100Pct: Datos.ConjuntoAprobado100Pct,
                TerminalesMarcadas75C: Datos.TerminalesMarcadas75C,
                CorrientesPorFase: CorrientesParaElMotor(),
                ConNeutro: SistemaDelTablero.De(Datos.Sistema) != ConfiguracionTablero.TresFasesTresHilos,
                // 230-79: el principal SUBE al mínimo si el tablero es el de la acometida — R-11.
                ProteccionMinimaA: Datos.Minimo230_79?.Amperes,
                ReferenciaProteccionMinima: Datos.Minimo230_79?.Referencia));

        try
        {
            // CONDUCTORES EN PARALELO. Un juego por canalización, todas iguales (310-10(h)(3)): el
            // conteo no cambia. Si se declaran todos en una, cada conductor cuenta
            // (310-15(b)(3)(a)): más portadores pueden pedir más cobre, y más cobre más juegos. Se
            // repite hasta que el número de juegos deje de cambiar.
            var juegos = 1;
            ResultadoAlimentador resultado;
            for (var vuelta = 0; ; vuelta++)
            {
                Contar(canal, [new CircuitoEnCanalizacion("alimentador", polos, conNeutro, [.. Datos.Barras], juegos)]);
                resultado = Calcular();
                var enUno = canal.JuegosEnUnTubo ? resultado.NumeroConductoresParalelo : 1;
                if (enUno == juegos) break;
                if (vuelta == 3)
                    throw new InvalidOperationException(
                        "Con todos los conductores en paralelo en una sola canalización, el número de juegos no se estabiliza: "
                        + "declara un juego por canalización.");
                juegos = enUno;
            }

            var n = resultado.NumeroConductoresParalelo;
            canal.CanalizacionesIguales = canal.JuegosEnUnTubo ? 1 : n;
            Dimensionar(canal,
                [new ConductoresDelCircuito("alimentador", polos, conNeutro, resultado.CalibreFase, resultado.CalibreNeutro, resultado.CalibreTierra)],
                canal.JuegosEnUnTubo ? n : 1);

            // La fase que gobierna la decide el motor; la de aquí es la misma regla, para mostrarla.
            if (resultado.FaseQueGobierna is { } fase)
                gobierna = fases.Single(f => f.Fase == fase);
            Alimentador = new RenglonDelAlimentador(resultado, null, Avisos(resultado), polos, fases, gobierna, fpAlimentador);
        }
        catch (Exception ex)
        {
            Alimentador = new RenglonDelAlimentador(null, ex.Message, [], polos, fases, gobierna, fpAlimentador);
        }
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
        if (mayor is not null && resultado.ProteccionA < mayorDerivado)
            avisos.Add(
                $"El interruptor principal ({resultado.ProteccionA:N0} A) es menor que el derivado más grande " +
                $"({mayorDerivado:N0} A, circuito {mayor.Espacio}). El principal se dispararía con una carga que ese " +
                "derivado sí admite: revisa la carga capturada o sube el principal.");

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
        }

        // R-12: el factor de demanda lo decide el proyectista, pero no sin sustento. Uno por tipo (R-17).
        foreach (var categoria in Datos.SinJustificacion)
            avisos.Add(
                $"El factor de demanda de {categoria.NombreCompleto().ToLowerInvariant()} es menor que 1 y no tiene justificación. " +
                "Escoge en «Resumen de carga» la tabla o sección del Art. 220 que lo sustenta — 220-40.");

        // 210-11(c)(1): los circuitos de aparatos pequeños son DOS O MÁS. Con uno solo capturado, se
        // dice; con ninguno, el tablero puede no ser de vivienda y no hay nada que decir.
        var aparatos = _circuitos.Where(c => c.TieneCarga && c.UsoEfectivo == UsoDeContactos.AparatosPequenos).ToList();
        if (aparatos.Count == 1)
            avisos.Add(
                $"Solo el circuito {aparatos[0].Espacio} es de aparatos pequeños. En vivienda se exigen dos o más circuitos " +
                "de 20 A para los contactos de cocina, despensa y comedor — 210-11(c)(1). No aplica en vivienda popular de " +
                "hasta 60 m².");

        return avisos;
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
