using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// Un renglón del cuadro de carga: <b>el espacio de la barra y lo que se le colgó</b>.
///
/// <para>
/// Hay uno por espacio del tablero, ocupado o no — igual que el Excel, que trae los 42 renglones
/// dibujados y en blanco los que no se usan. Un interruptor de 2 o 3 polos <b>se queda en el
/// renglón donde empieza</b> y se come los siguientes de su lado (N, N+2, N+4): esos renglones
/// quedan marcados con <see cref="ContinuacionDe"/> y no capturan nada, porque repetir la carga
/// haría que sumar la columna la contara dos o tres veces.
/// </para>
/// </summary>
public sealed class CircuitoDelCuadro
{
    public CircuitoDelCuadro(int espacio) => Espacio = espacio;

    /// <summary>El número de circuito, que es el número de espacio en la barra. Nones a la izquierda, pares a la derecha.</summary>
    public int Espacio { get; }

    public string Descripcion { get; set; } = string.Empty;
    /// <summary>El tipo de carga de los seis del selector — R-17, I-74. Decide el factor de demanda y el cálculo.</summary>
    public CategoriaDeCarga Categoria { get; set; } = CategoriaDeCarga.Alumbrado;

    /// <summary>
    /// El tipo con el que calcula el motor, que sale de <see cref="Categoria"/>: equipo y calefacción
    /// son <see cref="TipoCarga.Equipo"/> —carga de placa—; Motor es <see cref="TipoCarga.Fuerza"/>.
    /// Asignarlo fija la categoría (Fuerza → Motor).
    /// </summary>
    public TipoCarga Tipo
    {
        get => Categoria.TipoDelMotor();
        set => Categoria = value switch
        {
            TipoCarga.Alumbrado => CategoriaDeCarga.Alumbrado,
            TipoCarga.Contactos => CategoriaDeCarga.Contactos,
            TipoCarga.Fuerza => CategoriaDeCarga.Motor,
            _ => CategoriaDeCarga.Equipo,
        };
    }

    /// <summary>
    /// Para qué es el circuito, si es de contactos: aparatos pequeños, lavadora o baño de vivienda
    /// piden 20 A — 210-11(c). Se conserva al cambiar de tipo, pero solo cuenta en Contactos
    /// (<see cref="UsoEfectivo"/>).
    /// </summary>
    public UsoDeContactos Uso { get; set; } = UsoDeContactos.General;

    /// <summary>
    /// El uso que cuenta: el capturado, en Contactos y en una vivienda de más de 60 m²; General en
    /// todo lo demás — 210-11(c) y 220-52 son de vivienda (I-46). Lo pone <see cref="CuadroDeCarga"/>.
    /// </summary>
    public UsoDeContactos UsoEfectivo { get; internal set; } = UsoDeContactos.General;

    /// <summary>
    /// En qué unidad viene lo que se capturó: VA, W o A, como lo diga la placa. <b>VA por omisión</b>,
    /// que es lo único que la pantalla aceptaba antes — lo ya capturado no cambia.
    /// </summary>
    public UnidadConsumo Unidad { get; set; } = UnidadConsumo.VoltAmperes;

    /// <summary>
    /// <b>Tipo Motor: Art. 430</b> — I-15, I-74. La carga continua y la no continua no cuentan; se
    /// conservan por si se regresa a otro tipo.
    /// </summary>
    public bool EsMotor => Categoria == CategoriaDeCarga.Motor;

    /// <summary><b>Tipo A/C y refrigeración: Art. 440</b> — I-74. Como <see cref="EsMotor"/>, sin continua ni no continua.</summary>
    public bool EsAireAcondicionado => Categoria == CategoriaDeCarga.AireAcondicionado;

    /// <summary>Motor o A/C: un equipo de un circuito, sin desglose, que entra al alimentador por 430-24 / 440-33.</summary>
    public bool EsDeMotor => Categoria.EsDeMotor();

    /// <summary>Un motor se captura en HP o, si la placa no los trae, en amperes — 430-6(a)(1).</summary>
    public CapturaDeMotor CapturaMotor { get; set; } = CapturaDeMotor.Hp;

    /// <summary>
    /// <b>Los caballos de placa de un motor</b> — I-15. Solo cuentan en Motor capturado en HP; al
    /// cambiar de tipo se conservan, como <see cref="Uso"/>. <c>null</c> o 0 = motor todavía no elegido.
    /// </summary>
    public decimal? Hp { get; set; }

    /// <summary>
    /// <b>La corriente de placa</b>, en A: la de un motor marcado en amperes (430-6(a)(1)) o la de carga
    /// nominal de un equipo de A/C (440-6(a)). Se comparte: es el mismo dato de placa en los dos tipos.
    /// </summary>
    public decimal CorrientePlacaA { get; set; }

    /// <summary>A/C: la corriente de selección del circuito derivado, si la placa la trae — 440-6(a) Excepción 1.</summary>
    public decimal? CorrienteSeleccionA { get; set; }

    /// <summary>A/C por corriente nominal: la protección al 175 % no aguanta el arranque; sube a 225 % — 440-22(a).</summary>
    public bool ArranqueAl225 { get; set; }

    /// <summary>A/C: qué trae la placa. Ver <see cref="PlacaDeAireAcondicionado"/>.</summary>
    public PlacaDeAireAcondicionado PlacaAire { get; set; } = PlacaDeAireAcondicionado.AmpacidadYProteccion;

    /// <summary>A/C: la ampacidad mínima de los conductores de la placa (MCA) — 440-4(b).</summary>
    public decimal AmpacidadMinimaA { get; set; }

    /// <summary>A/C: la protección máxima de la placa (MOCP) — 440-4(b).</summary>
    public decimal ProteccionMaximaA { get; set; }

    /// <summary>
    /// Lo capturado del motor o del equipo de A/C, en la forma que corresponde: HP, amperes, corriente
    /// nominal o ampacidad mínima. Con eso el renglón calcula, o dice por qué no (<see cref="Error"/>).
    /// </summary>
    public bool TieneCapturaDeMotor =>
        EsMotor ? (CapturaMotor == CapturaDeMotor.Hp ? Hp > 0m : CorrientePlacaA > 0m)
        : EsAireAcondicionado && (PlacaAire == PlacaDeAireAcondicionado.AmpacidadYProteccion ? AmpacidadMinimaA > 0m : CorrientePlacaA > 0m);

    /// <summary>
    /// <b>La corriente del motor o del equipo de A/C</b>: la FLC de tabla de un motor (430-6(a)), la de
    /// 440-6(a) o la ampacidad mínima de un equipo de A/C (440-4(b)). Es la que entra al alimentador
    /// (430-24, 440-33) y a la caída. 0 si no calcula (el renglón lleva <see cref="Error"/>). La pone
    /// <see cref="CuadroDeCarga"/>.
    /// </summary>
    public decimal CorrienteDeMotorA { get; internal set; }

    /// <summary>La corriente a plena carga de un motor — 430-6(a). 0 si no es Motor o no calcula.</summary>
    public decimal FlcA => EsMotor ? CorrienteDeMotorA : 0m;

    /// <summary>
    /// Un motor capturado en amperes, llevado a la tabla: sus caballos interpolados — 430-6(a)(1).
    /// <c>null</c> en HP o fuera de la tabla. La pone <see cref="CuadroDeCarga"/>.
    /// </summary>
    public MotorEnAmperes? MotorEnAmperes { get; internal set; }

    /// <summary>
    /// La carga del motor o del equipo de A/C en VA: su corriente por la tensión del circuito (× √3 en
    /// 3 polos). Es la que va al balanceo, al resumen y a los kW; el derivado y el alimentador se
    /// calculan con la corriente.
    /// </summary>
    public decimal MotorVA { get; internal set; }

    /// <summary>
    /// Los aparatos que alimenta, si se desglosa — I-35. Con al menos uno, la carga, la unidad y el
    /// F.P. del circuito salen de ellos (la suma, y el F.P. combinado) y no se capturan.
    /// </summary>
    public List<AparatoDelCircuito> Aparatos { get; } = [];

    /// <summary>
    /// Se desglosa. Un motor o un equipo de A/C es un solo equipo: no se desglosa, y los aparatos que
    /// trajera de otro tipo se conservan sin contar, como la continua y la no continua.
    /// </summary>
    public bool TieneDesglose => Aparatos.Count > 0 && !EsDeMotor;

    /// <summary>
    /// Abre el desglose. Si el circuito ya traía carga, se convierte en los primeros aparatos —
    /// continua y no continua por separado— para no perder lo capturado.
    /// </summary>
    public AparatoDelCircuito AgregarAparato()
    {
        if (!TieneDesglose)
        {
            var nombre = string.IsNullOrWhiteSpace(Descripcion) ? "Carga capturada" : Descripcion.Trim();
            if (Continua > 0m)
                Aparatos.Add(new AparatoDelCircuito { Descripcion = nombre, Unidad = Unidad, CargaUnitaria = Continua, Continua = true, FactorPotencia = FactorPotencia });
            if (NoContinua > 0m)
                Aparatos.Add(new AparatoDelCircuito { Descripcion = nombre, Unidad = Unidad, CargaUnitaria = NoContinua, FactorPotencia = FactorPotencia });
        }

        var nuevo = new AparatoDelCircuito();
        Aparatos.Add(nuevo);
        return nuevo;
    }

    /// <summary>La carga continua <b>tal como viene en la placa</b>, en <see cref="Unidad"/>.</summary>
    public decimal Continua { get; set; }

    /// <summary>La carga no continua tal como viene en la placa, en <see cref="Unidad"/>.</summary>
    public decimal NoContinua { get; set; }

    /// <summary>
    /// La carga continua ya en volt-amperes, que es con lo que calcula el motor. La convierte
    /// <see cref="CuadroDeCarga"/> con <c>ConsumoDePlaca.AVoltAmperes</c> —la misma regla del
    /// escritorio (<c>CircuitoDerivado.VaUnitarioDe</c>)—, porque la conversión necesita la tensión
    /// y el factor de potencia, que son del tablero.
    /// </summary>
    public decimal ContinuaVA { get; internal set; }

    /// <summary>La carga no continua en volt-amperes. Ver <see cref="ContinuaVA"/>.</summary>
    public decimal NoContinuaVA { get; internal set; }
    public decimal LongitudM { get; set; } = 20m;

    /// <summary>
    /// El factor de potencia <b>de esta carga</b>. La NOM lo pide por circuito: la nota 2 de la
    /// Tabla 9 define la impedancia eficaz con «el ángulo del factor de potencia <b>del circuito</b>».
    ///
    /// <para>
    /// <b>0.9 es un valor supuesto, no de la norma</b> —la NOM no fija ninguno—; se cambia con el dato
    /// de placa: 1.0 en una resistencia, ~0.8 en un compresor. Entra en dos lugares: la conversión de
    /// W a VA y la caída de tensión. <b>No mueve la protección ni el calibre</b> de una carga
    /// capturada en VA o en A: la NOM dimensiona con la corriente (220-14(a), 220-18(b)).
    /// </para>
    /// </summary>
    public decimal FactorPotencia { get; set; } = FactorPotenciaSupuesto;

    /// <summary>El F.P. con el que nace cada renglón. Decisión de David del 2026-09-23.</summary>
    public const decimal FactorPotenciaSupuesto = 0.9m;

    /// <summary>Polos del interruptor. Se cambia por <see cref="CuadroDeCarga.CambiarPolos"/>, que verifica que quepa.</summary>
    public int Polos { get; internal set; } = 1;

    /// <summary>
    /// El espacio del interruptor multipolar que se comió este renglón, o <c>null</c> si el renglón
    /// es suyo. Lo mantiene <see cref="CuadroDeCarga"/>.
    /// </summary>
    public int? ContinuacionDe { get; internal set; }

    /// <summary>
    /// El renglón es del interruptor principal montado en espacios, no de un circuito: no captura
    /// nada. El primero de sus espacios lo lleva sin <see cref="ContinuacionDe"/>; los demás, con él.
    /// Lo mantiene <see cref="CuadroDeCarga"/> — decisión <c>montaje-del-interruptor-principal.md</c>.
    /// </summary>
    public bool EsDelPrincipal { get; internal set; }

    /// <summary>Las barras que toca, en el orden en que las toca: «A», «AB», «ABC». La resuelve la geometría del tablero, no se captura.</summary>
    public string Fases { get; internal set; } = "A";

    /// <summary>
    /// La canalización por la que va («T1»). <c>null</c> solo sin carga: al capturarla, el recálculo
    /// le da un tubo nuevo — 1 circuito, 1 tubo (David, 2026-09-24). Si el circuito pasa por varias,
    /// la del tramo más desfavorable — 310-15(a)(2).
    /// </summary>
    public string? Canalizacion { get; set; }

    /// <summary>
    /// Casilla «+N» de un circuito de 2 o 3 polos: la carga es F-N (220/127 V) y lleva neutro. Un
    /// circuito de 1 polo siempre lo lleva; ver <see cref="LlevaNeutro"/>.
    /// </summary>
    public bool ConNeutro { get; set; }

    /// <summary>
    /// Si el circuito lleva neutro: 1 polo sí; 2 y 3 polos solo con <see cref="ConNeutro"/>; nunca en
    /// un tablero sin neutro (3F-3H). Lo resuelve <see cref="CuadroDeCarga"/> — I-41: antes todos
    /// los circuitos salían con neutro, aunque la carga fuera F-F.
    /// </summary>
    public bool LlevaNeutro { get; internal set; } = true;

    /// <summary>La canalización con la que se calculó. La mantiene <see cref="CuadroDeCarga"/>.</summary>
    public CanalizacionDelTablero? CanalizacionEfectiva { get; internal set; }

    public ResultadoCircuitoDerivado? Resultado { get; internal set; }

    /// <summary>Lo que el motor rechazó, ya redactado. <c>null</c> = el renglón calculó.</summary>
    public string? Error { get; internal set; }

    /// <summary>
    /// Caída del alimentador + la de este circuito, en %. <c>null</c> si falta alguno de los dos
    /// cálculos — R-01.
    /// </summary>
    public decimal? CaidaCombinadaPct { get; internal set; }

    /// <summary>La caída del alimentador en la fase de este circuito, la que entra a la combinada. <c>null</c> sin alimentador.</summary>
    public decimal? CaidaAlimentadorPct { get; internal set; }

    /// <summary>El aviso de caída combinada mayor que 5 %, ya redactado. <c>null</c> = cumple o no aplica.</summary>
    public string? AvisoCaidaCombinada { get; internal set; }

    public bool EsContinuacion => ContinuacionDe is not null;

    public decimal CargaInstaladaVA => ContinuaVA + NoContinuaVA + MotorVA;

    /// <summary>
    /// Lo que le falta a la carga capturada para llegar a los 1500 VA con los que entra al
    /// alimentador un circuito de aparatos pequeños o de lavadora — 220-52(a) y (b). Entra como
    /// carga no continua. Cero en cualquier otro uso o si ya pasa de 1500 VA. No cambia el cálculo del
    /// propio derivado: 220-52 es carga de alimentador.
    /// </summary>
    public decimal Ajuste220_52VA { get; internal set; }

    /// <summary>La carga con la que entra al alimentador: la capturada más el mínimo de 220-52.</summary>
    public decimal CargaCalculadaVA => CargaInstaladaVA + Ajuste220_52VA;

    /// <summary>
    /// La potencia activa, en W: los VA por el F.P. del circuito. Para un renglón capturado en W
    /// regresa exactamente los W de la placa.
    /// </summary>
    public decimal PotenciaActivaW => CargaInstaladaVA * FactorPotencia;

    /// <summary>
    /// Con algo que calcular. Un motor o un equipo de A/C con su placa cuenta aunque no calcule (un
    /// motor que la tabla no trae): así el renglón dice por qué no (<see cref="Error"/>) en vez de
    /// quedarse en blanco.
    /// </summary>
    public bool TieneCarga => !EsContinuacion && !EsDelPrincipal && (CargaInstaladaVA > 0m || TieneCapturaDeMotor);

    /// <summary>
    /// Algo capturado: descripción, carga, placa, aparatos o más de un polo. Un renglón así no se lo
    /// come el interruptor principal: se avisa y el principal espera a que uno de los dos se mueva.
    /// </summary>
    public bool TieneCaptura =>
        !string.IsNullOrWhiteSpace(Descripcion) || Continua > 0m || NoContinua > 0m || Hp > 0m || CorrientePlacaA > 0m
        || AmpacidadMinimaA > 0m || Aparatos.Count > 0 || Polos > 1;

    /// <summary>
    /// Lo que este circuito le carga a cada una de sus barras, en VA — la banda «BALANCEO DE FASES»
    /// del Excel (columnas BB, BC, BD): la carga entre el número de fases que toca.
    /// </summary>
    public decimal CargaPorFaseVA => Fases.Length == 0 ? 0m : CargaInstaladaVA / Fases.Length;

    internal void Limpiar()
    {
        Resultado = null;
        Error = null;
        CaidaCombinadaPct = null;
        CaidaAlimentadorPct = null;
        AvisoCaidaCombinada = null;
    }
}
