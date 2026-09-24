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
    public CircuitoDelCuadro(int espacio)
    {
        Espacio = espacio;
        CanalizacionPropia = new($"Circuito {espacio}", esPropia: true);
    }

    /// <summary>El número de circuito, que es el número de espacio en la barra. Nones a la izquierda, pares a la derecha.</summary>
    public int Espacio { get; }

    public string Descripcion { get; set; } = string.Empty;
    /// <summary>El tipo de carga de los cinco del selector — R-17. Decide el factor de demanda.</summary>
    public CategoriaDeCarga Categoria { get; set; } = CategoriaDeCarga.Alumbrado;

    /// <summary>
    /// El tipo con el que calcula el motor, que sale de <see cref="Categoria"/>: motor, A/C y
    /// calefacción son <see cref="TipoCarga.Equipo"/>. Asignarlo fija la categoría (Fuerza → motor).
    /// </summary>
    public TipoCarga Tipo
    {
        get => Categoria.TipoDelMotor();
        set => Categoria = value switch
        {
            TipoCarga.Alumbrado => CategoriaDeCarga.Alumbrado,
            TipoCarga.Contactos => CategoriaDeCarga.Contactos,
            TipoCarga.Fuerza => CategoriaDeCarga.MotorOAireAcondicionado,
            _ => CategoriaDeCarga.Equipo,
        };
    }

    /// <summary>
    /// Para qué es el circuito, si es de contactos: aparatos pequeños, lavadora o baño de vivienda
    /// piden 20 A — 210-11(c). Se conserva al cambiar de tipo, pero solo cuenta en Contactos
    /// (<see cref="UsoEfectivo"/>).
    /// </summary>
    public UsoDeContactos Uso { get; set; } = UsoDeContactos.General;

    /// <summary>El uso que cuenta: el capturado en Contactos, General en cualquier otro tipo.</summary>
    public UsoDeContactos UsoEfectivo => Tipo == TipoCarga.Contactos ? Uso : UsoDeContactos.General;

    /// <summary>
    /// En qué unidad viene lo que se capturó: VA, W o A, como lo diga la placa. <b>VA por omisión</b>,
    /// que es lo único que la pantalla aceptaba antes — lo ya capturado no cambia.
    /// </summary>
    public UnidadConsumo Unidad { get; set; } = UnidadConsumo.VoltAmperes;

    /// <summary>
    /// Los aparatos que alimenta, si se desglosa — I-35. Con al menos uno, la carga, la unidad y el
    /// F.P. del circuito salen de ellos (la suma, y el F.P. combinado) y no se capturan.
    /// </summary>
    public List<AparatoDelCircuito> Aparatos { get; } = [];

    public bool TieneDesglose => Aparatos.Count > 0;

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

    /// <summary>Las barras que toca, en el orden en que las toca: «A», «AB», «ABC». La resuelve la geometría del tablero, no se captura.</summary>
    public string Fases { get; internal set; } = "A";

    /// <summary>
    /// La canalización compartida por la que va («T1»), o <c>null</c>: va en la suya propia, con la
    /// configuración por omisión del tablero. Si el circuito pasa por varias, la del tramo más
    /// desfavorable — 310-15(a)(2).
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

    /// <summary>
    /// La canalización propia del circuito, guardada con él: su configuración (tipo, tubo, tierra
    /// desnuda, azotea, tamaño y nombre) sobrevive a cada recálculo — David, 2026-09-24.
    /// </summary>
    public CanalizacionDelTablero CanalizacionPropia { get; }

    /// <summary>La canalización con la que se calculó: la compartida o la propia. La mantiene <see cref="CuadroDeCarga"/>.</summary>
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

    public decimal CargaInstaladaVA => ContinuaVA + NoContinuaVA;

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

    public bool TieneCarga => !EsContinuacion && CargaInstaladaVA > 0m;

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
