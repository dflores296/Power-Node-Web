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

    /// <summary>
    /// El nombre del equipo capturado en el renglón —un motor, un variador, un A/A—, el de su línea del
    /// desplegable (David, 2026-09-30). Aparte de <see cref="Descripcion"/>, que es la etiqueta del espacio:
    /// igual que cada línea del desplegable lleva el suyo. Pasa con él a la línea y regresa.
    /// </summary>
    public string DescripcionDelEquipo { get; set; } = string.Empty;
    /// <summary>El tipo de carga de los seis del selector — R-17, I-74. Decide el factor de demanda y el cálculo.</summary>
    public CategoriaDeCarga Categoria
    {
        get => categoria;
        set { categoria = value; TipoElegido = true; }
    }

    private CategoriaDeCarga categoria = CategoriaDeCarga.Alumbrado;

    /// <summary>
    /// Ya se eligió el tipo: asignar <see cref="Categoria"/> lo marca. Un espacio nuevo no lo tiene, y en
    /// la captura (<see cref="CuadroDeCarga.ExigirTipo"/>) su carga no calcula hasta que se elige: con
    /// Alumbrado por omisión, un circuito que no se cambiaba quedaba calculado como alumbrado sin que
    /// nadie lo decidiera (David, 2026-09-29 — I-111).
    /// </summary>
    public bool TipoElegido { get; private set; }

    /// <summary>Carga capturada sin tipo elegido: no calcula ni entra al tablero. La pone <see cref="CuadroDeCarga"/>.</summary>
    public bool SinTipo { get; internal set; }

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

    /// <summary>
    /// Motor solo (HP o A): el proyectista declara que no arranca con la protección de la Tabla 430-52;
    /// el interruptor puede subir hasta 400 % de la FLC (300 % arriba de 100 A) — 430-52(c)(1)
    /// Excepción 2(3). Nunca se aplica sola (auditoría del 2026-09-29, P1-1).
    /// </summary>
    public bool NoArrancaConLaTabla { get; set; }

    /// <summary>A/C: qué trae la placa. Ver <see cref="PlacaDeAireAcondicionado"/>.</summary>
    public PlacaDeAireAcondicionado PlacaAire { get; set; } = PlacaDeAireAcondicionado.AmpacidadYProteccion;

    /// <summary>A/C: la ampacidad mínima de los conductores de la placa (MCA) — 440-4(b).</summary>
    public decimal AmpacidadMinimaA { get; set; }

    /// <summary>A/C: la protección máxima de la placa (MOCP) — 440-4(b).</summary>
    public decimal ProteccionMaximaA { get; set; }

    /// <summary>Motor con variador: la corriente nominal de entrada del variador — 430-122(a), I-119.</summary>
    public decimal CorrienteEntradaVariadorA { get; set; }

    /// <summary>Motor con variador: la protección máxima que marca su fabricante — 110-3(b), I-119.</summary>
    public decimal ProteccionMaximaVariadorA { get; set; }

    /// <summary>Un motor con variador de velocidad — 430 Parte J, I-119.</summary>
    public bool EsVariador => EsMotor && CapturaMotor == CapturaDeMotor.Variador;

    /// <summary>
    /// El servicio de un motor solo, en HP o en amperes: <c>null</c> = continuo, el de casi todos (nota de la
    /// Tabla 430-22(e)); si no, corta duración, intermitente, periódico o variable — 430-22(e), I-120.
    /// </summary>
    public PowerNode.DesignSuite.Calculo.TablasNom.ServicioDeMotor? Servicio { get; set; }

    /// <summary>Para cuánto tiempo está especificado el motor: la columna de la Tabla 430-22(e).</summary>
    public PowerNode.DesignSuite.Calculo.TablasNom.EspecificacionDeTiempo EspecificacionServicio { get; set; } =
        PowerNode.DesignSuite.Calculo.TablasNom.EspecificacionDeTiempo.Continuo;

    /// <summary>
    /// La corriente de placa de un motor en HP de servicio no continuo: 430-22(e) va sobre la placa, no
    /// sobre la tabla. En amperes es <see cref="CorrientePlacaA"/>.
    /// </summary>
    public decimal CorrientePlacaServicioA { get; set; }

    /// <summary>El servicio aplica: un motor solo, en HP o en amperes, con servicio no continuo.</summary>
    public bool TieneServicioNoContinuo =>
        EsMotor && Servicio is not null && CapturaMotor is CapturaDeMotor.Hp or CapturaDeMotor.Amperes;

    /// <summary>La corriente de placa con la que se aplica 430-22(e).</summary>
    public decimal CorrienteDePlacaDelServicioA => CapturaMotor == CapturaDeMotor.Amperes ? CorrientePlacaA : CorrientePlacaServicioA;

    /// <summary>
    /// Lo que un motor de servicio no continuo aporta al alimentador: el % de la Tabla 430-22(e) por su
    /// placa — 430-24 Excepción 1. 0 si no aplica. La pone <see cref="CuadroDeCarga"/>.
    /// </summary>
    public decimal CorrienteDeServicioA { get; internal set; }

    /// <summary>
    /// El espacio de un circuito que no funciona a la vez que este: del par, al alimentador va el mayor —
    /// 220-60, 430-24 Excepción 3, 440-33 Excepción 1 (I-121). <c>null</c> = sin par.
    /// </summary>
    public int? NoSimultaneoCon { get; set; }

    /// <summary>No entra al alimentador: es el menor de un par no simultáneo. Lo pone <see cref="CuadroDeCarga"/>.</summary>
    public bool OmitidoPorNoSimultaneo { get; internal set; }

    /// <summary>
    /// Lo capturado del motor o del equipo de A/C, en la forma que corresponde: HP, amperes, corriente
    /// nominal o ampacidad mínima. Con eso el renglón calcula, o dice por qué no (<see cref="Error"/>).
    /// </summary>
    public bool TieneCapturaDeMotor =>
        EsGrupo ? Cargas.Any(a => a.EsMaquina && a.TieneCapturaDeMaquina)
        : EsMotor ? CapturaMotor switch
        {
            CapturaDeMotor.Hp => Hp > 0m,
            CapturaDeMotor.Amperes => CorrientePlacaA > 0m,
            CapturaDeMotor.Variador => CorrienteEntradaVariadorA > 0m,
            _ => Cargas.Any(a => a.EsMaquina && a.TieneCapturaDeMaquina),
        }
        : EsAireAcondicionado && PlacaAire switch
        {
            PlacaDeAireAcondicionado.AmpacidadYProteccion => AmpacidadMinimaA > 0m,
            PlacaDeAireAcondicionado.Grupo => Cargas.Any(a => a.EsMaquina && a.TieneCapturaDeMaquina),
            _ => CorrientePlacaA > 0m,
        };

    /// <summary>
    /// <b>Varios motores, o motores y otras cargas</b> — 430-53, I-115: un Motor capturado como «Varios»;
    /// o varios motocompresores, o motocompresor y ventiladores u otras cargas — 440-22(b), I-116: un A/C
    /// capturado como «Varios». Cada máquina y cada otra carga es un aparato del desglose.
    /// </summary>
    public bool EsGrupo =>
        Categoria != CategoriaDeCarga.Tablero && Cargas.Count > 0 &&
        ((EsMotor && CapturaMotor == CapturaDeMotor.Grupo)
         || (EsAireAcondicionado && PlacaAire == PlacaDeAireAcondicionado.Grupo)
         // POR LO QUE LLEVA, NO POR UNA UNIDAD — I-123: un motor o un motocompresor entre sus cargas lo hace
         // grupo (430-53, 440-22(b)); también varios aparatos con motor sin otras cargas, que 220-18(a) manda
         // al Art. 430.
         || Cargas.Any(a => a.EsMaquina && TipoDe(a).EsDeMotor())
         || (Cargas.Any(a => a.EsMaquina) && Cargas.All(a => a.EsMaquina))
         // Un Motor o un A/C con cargas capturadas en el desplegable (con subtipo): sin máquinas, el grupo lo
         // dice («no tiene motores») en vez de ignorarlas. Las de otro tipo que traía de antes no cuentan.
         || (EsDeMotor && Cargas.Any(a => a.Subtipo is not null)));

    /// <summary>
    /// <b>Un motor solo, sin variador</b>: el del renglón (HP o A), o un grupo con un solo motor de
    /// cantidad 1 y nada más, que se calcula como motor. A él le aplica la Excepción 2 de 430-52(c)(1).
    /// </summary>
    public bool EsMotorSolo =>
        EsMotor && !EsVariador && (!EsGrupo
            ? CapturaMotor is CapturaDeMotor.Hp or CapturaDeMotor.Amperes
            : Cargas.Where(a => a.EsMaquina).ToList() is [{ Clase: ClaseDeAparato.Motor, Cantidad: 1 }]
              && !Cargas.Any(a => !a.EsMaquina && a.TotalVA > 0m));

    /// <summary>El tipo de una de sus cargas: el de su subtipo, o el del circuito — I-123.</summary>
    public CategoriaDeCarga TipoDe(CargaDelCircuito carga) => carga.TipoEn(this);

    /// <summary>
    /// <b>Los tipos de sus cargas</b> — I-123. Uno solo, el del renglón, si no se desglosa. Más de uno: el
    /// circuito lleva <b>cargas combinadas</b> (220-18(a), 440-34, 430-110(c)).
    /// </summary>
    public IReadOnlyList<CategoriaDeCarga> TiposDeSusCargas =>
        TieneDesglose ? [.. Cargas.Select(TipoDe).Distinct()] : [Categoria];

    /// <summary>Lleva cargas de más de un tipo: el cuadro dice «Combinadas».</summary>
    public bool TieneCargasCombinadas => TiposDeSusCargas.Count > 1;

    /// <summary>
    /// <b>La carga del circuito partida por tipo</b> — I-123: cada parte entra al alimentador con el factor
    /// de demanda de su tipo (220 Parte C). La pone <see cref="CuadroDeCarga"/>.
    /// </summary>
    public IReadOnlyList<PorcionDeCarga> Porciones { get; internal set; } = [];

    /// <summary>
    /// <b>La clase del circuito</b>, que sale de sus cargas — Art. 100, I-123. <c>null</c> sin carga.
    /// <list type="bullet">
    /// <item>Otro tablero: <see cref="ClaseDeCircuito.Alimentador"/>.</item>
    /// <item>Un solo equipo —el renglón de Aparatos, Motores o A/A, o una sola carga de cantidad 1 que no
    /// es alumbrado ni contactos—, o el contacto del refrigerador: <see cref="ClaseDeCircuito.Individual"/>.</item>
    /// <item>Con alumbrado o contactos de uso general: <see cref="ClaseDeCircuito.UsoGeneral"/>.</item>
    /// <item>Solo aparatos, o los contactos de vivienda para aparatos pequeños y lavadora:
    /// <see cref="ClaseDeCircuito.ParaAparatos"/>.</item>
    /// </list>
    /// </summary>
    public ClaseDeCircuito? ClaseDelCircuito
    {
        get
        {
            if (!TieneCarga)
                return null;
            if (Categoria == CategoriaDeCarga.Tablero)
                return ClaseDeCircuito.Alimentador;
            if (EsGrupoDeMotores && Cargas.Sum(a => Math.Max(1, a.Cantidad)) > 1)
                return ClaseDeCircuito.GrupoDeMotores;
            if (UsoEfectivo == UsoDeContactos.Refrigerador || Cargas.Any(a => a.Subtipo == SubtipoDeCarga.ContactoRefrigerador))
                return ClaseDeCircuito.Individual;
            if (!TieneDesglose)
                return Categoria switch
                {
                    CategoriaDeCarga.Alumbrado => ClaseDeCircuito.UsoGeneral,
                    CategoriaDeCarga.Contactos => UsoParaAparatos ? ClaseDeCircuito.ParaAparatos : ClaseDeCircuito.UsoGeneral,
                    CategoriaDeCarga.CalefaccionFija => ClaseDeCircuito.ParaAparatos,
                    _ => ClaseDeCircuito.Individual,
                };
            var tipos = TiposDeSusCargas;
            if (Cargas is [{ Cantidad: <= 1 } sola] && TipoDe(sola) is not (CategoriaDeCarga.Alumbrado or CategoriaDeCarga.Contactos))
                return ClaseDeCircuito.Individual;
            if (tipos.Contains(CategoriaDeCarga.Alumbrado) || (tipos.Contains(CategoriaDeCarga.Contactos) && !UsoParaAparatos))
                return ClaseDeCircuito.UsoGeneral;
            return ClaseDeCircuito.ParaAparatos;
        }
    }

    /// <summary>
    /// Un grupo de solo motores o motocompresores (430-53, 440-22(b)): el Art. 100 no le da nombre de clase;
    /// la pantalla y la memoria dicen «grupo» — I-123.
    /// </summary>
    public bool EsGrupoDeMotores => EsGrupo && Cargas.All(a => a.EsMaquina || TipoDe(a).EsDeMotor());

    /// <summary>Contactos de vivienda para aparatos pequeños o lavadora — 210-11(c)(1), (2).</summary>
    private bool UsoParaAparatos => UsoEfectivo is UsoDeContactos.AparatosPequenos or UsoDeContactos.Lavadora;

    /// <summary>Las salidas y cargas del desglose, por su cantidad. <c>null</c> sin desglose: es carga total.</summary>
    public int? Salidas => TieneDesglose ? Cargas.Sum(a => Math.Max(1, a.Cantidad)) : null;

    /// <summary>
    /// Capturado como <b>carga total</b>: el renglón de Alumbrado, Contactos o Calefacción, que es un grupo
    /// de salidas sin desglosar — I-123.
    /// </summary>
    public bool EsCargaTotal => !TieneDesglose &&
        Categoria is CategoriaDeCarga.Alumbrado or CategoriaDeCarga.Contactos or CategoriaDeCarga.CalefaccionFija;

    /// <summary>
    /// <b>La corriente del motor o del equipo de A/C</b>: la FLC de tabla de un motor (430-6(a)), la de
    /// 440-6(a) o la ampacidad mínima de un equipo de A/C (440-4(b)); en un grupo, la suma de sus
    /// máquinas. Es la que entra al alimentador (430-24, 440-33) y a la caída. 0 si no calcula (el
    /// renglón lleva <see cref="Error"/>). La pone <see cref="CuadroDeCarga"/>.
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
    public List<CargaDelCircuito> Cargas { get; } = [];

    /// <summary>
    /// Se desglosa. Un motor o un equipo de A/C es un solo equipo: no se desglosa, y los aparatos que
    /// trajera de otro tipo se conservan sin contar, como la continua y la no continua. Un grupo
    /// (<see cref="EsGrupo"/>) sí: sus aparatos son los motores y las otras cargas.
    /// </summary>
    public bool TieneDesglose => Cargas.Count > 0 && Categoria != CategoriaDeCarga.Tablero && (!EsDeMotor || EsGrupo);

    /// <summary>
    /// Abre el desglose. Si el circuito ya traía carga, se convierte en los primeros aparatos —
    /// continua y no continua por separado— para no perder lo capturado. En un motor la continua y la
    /// no continua son de otro tipo y no cuentan: no se convierten.
    /// </summary>
    public CargaDelCircuito AgregarCarga()
    {
        RenglonALineas();
        var nuevo = new CargaDelCircuito();
        Cargas.Add(nuevo);
        return nuevo;
    }

    /// <summary>
    /// <b>Lo capturado en el renglón pasa a sus líneas</b> — I-35, I-123, decisión
    /// <c>captura-en-el-desplegable.md</c>. Un motor, un motocompresor o un acondicionador de habitación, a
    /// una línea (con otra carga el circuito es un grupo: 430-53, 440-22(b)); una carga de placa, a una
    /// línea continua y otra no continua; otro tablero, a su línea con sus dos cantidades; un variador, a
    /// su línea. Un A/A con ampacidad de placa va solo en su circuito: se queda en el renglón. Con <paramref name="conSubtipo"/>, cada línea lleva
    /// ya su subtipo (el uso de vivienda, el de contactos); sin él, toma el tipo del circuito, como hasta
    /// el formato 4. <c>false</c> si no había nada que pasar.
    /// </summary>
    public bool RenglonALineas(bool conSubtipo = false)
    {
        if (TieneDesglose || (Categoria == CategoriaDeCarga.Tablero && Cargas.Count > 0))
            return false;
        // El nombre del equipo; en un archivo anterior, que no lo traía, el del espacio.
        var nombre = !string.IsNullOrWhiteSpace(DescripcionDelEquipo) ? DescripcionDelEquipo.Trim()
            : string.IsNullOrWhiteSpace(Descripcion) ? null : Descripcion.Trim();
        DescripcionDelEquipo = string.Empty;
        if (Categoria == CategoriaDeCarga.Tablero)
        {
            // Otro tablero capturado en el renglón (formato 7 o anterior): su línea, con sus dos cantidades.
            if (Continua <= 0m && NoContinua <= 0m)
                return false;
            Cargas.Add(new CargaDelCircuito
            {
                Descripcion = nombre ?? "Tablero",
                Subtipo = SubtipoDeCarga.TableroAlimentado,
                Unidad = Unidad,
                CargaUnitaria = Continua,
                NoContinua = NoContinua,
                FactorPotencia = FactorPotencia,
            });
            Continua = 0m;
            NoContinua = 0m;
            return true;
        }
        if (EsMotor && CapturaMotor is CapturaDeMotor.Hp or CapturaDeMotor.Amperes && TieneCapturaDeMotor)
            Cargas.Insert(0, new CargaDelCircuito
            {
                Descripcion = nombre ?? "Motor",
                Subtipo = SubtipoDeCarga.MotorUsoGeneral,
                CapturaMotor = CapturaMotor,
                Hp = Hp,
                CorrientePlacaA = CorrientePlacaA,
                FactorPotencia = FactorPotencia,
            });
        else if (EsVariador && CorrienteEntradaVariadorA > 0m)
            Cargas.Insert(0, new CargaDelCircuito
            {
                Descripcion = nombre ?? "Variador",
                Subtipo = SubtipoDeCarga.MotorVelocidadAjustable,
                CorrientePlacaA = CorrienteEntradaVariadorA,
                ProteccionMaximaA = ProteccionMaximaVariadorA,
                FactorPotencia = FactorPotencia,
            });
        else if (EsAireAcondicionado && PlacaAire == PlacaDeAireAcondicionado.CorrienteNominal && CorrientePlacaA > 0m)
            Cargas.Insert(0, new CargaDelCircuito
            {
                Descripcion = nombre ?? "Motocompresor",
                Subtipo = SubtipoDeCarga.Motocompresor,
                CorrientePlacaA = CorrientePlacaA,
                CorrienteSeleccionA = CorrienteSeleccionA,
                FactorPotencia = FactorPotencia,
            });
        else if (EsAireAcondicionado && PlacaAire == PlacaDeAireAcondicionado.Habitacion && CorrientePlacaA > 0m)
            Cargas.Insert(0, new CargaDelCircuito
            {
                Descripcion = nombre ?? "Aire de habitación",
                Subtipo = SubtipoDeCarga.AireDeHabitacion,
                CorrientePlacaA = CorrientePlacaA,
                FactorPotencia = FactorPotencia,
            });
        else if (!EsDeMotor && Categoria != CategoriaDeCarga.Tablero && (Continua > 0m || NoContinua > 0m))
        {
            SubtipoDeCarga? subtipo = !conSubtipo ? null
                : Categoria == CategoriaDeCarga.Contactos ? SubtiposDeCarga.DeUso(Uso)
                : SubtiposDeCarga.PorOmision(Categoria);
            var linea = nombre ?? "Carga capturada";
            if (Continua > 0m)
                Cargas.Add(new CargaDelCircuito { Descripcion = linea, Subtipo = subtipo, Unidad = Unidad, CargaUnitaria = Continua, Continua = true, FactorPotencia = FactorPotencia });
            if (NoContinua > 0m)
                Cargas.Add(new CargaDelCircuito { Descripcion = linea, Subtipo = subtipo, Unidad = Unidad, CargaUnitaria = NoContinua, FactorPotencia = FactorPotencia });
            Continua = 0m;
            NoContinua = 0m;
        }
        else
            return false;
        return true;
    }

    /// <summary>
    /// <b>Una sola línea de un solo equipo vuelve al renglón</b> — decisión <c>captura-en-el-desplegable.md</c>.
    /// Un motor, un motocompresor o un acondicionador de habitación, solos y de cantidad 1, se calculan
    /// con su propio artículo (430-22 y 430-52, con su servicio; 440-22(a); 440 Parte G), no como grupo.
    /// <c>false</c> si no aplica.
    /// </summary>
    public bool LineaARenglon()
    {
        if (Cargas is not [{ Cantidad: <= 1, Subtipo: { } subtipo } linea])
            return false;
        switch (subtipo)
        {
            case SubtipoDeCarga.MotorUsoGeneral:
                Categoria = CategoriaDeCarga.Motor;
                CapturaMotor = linea.CapturaMotor;
                Hp = linea.Hp;
                CorrientePlacaA = linea.CorrientePlacaA;
                break;
            case SubtipoDeCarga.Motocompresor:
                Categoria = CategoriaDeCarga.AireAcondicionado;
                PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
                CorrientePlacaA = linea.CorrientePlacaA;
                CorrienteSeleccionA = linea.CorrienteSeleccionA;
                break;
            case SubtipoDeCarga.AireDeHabitacion:
                Categoria = CategoriaDeCarga.AireAcondicionado;
                PlacaAire = PlacaDeAireAcondicionado.Habitacion;
                CorrientePlacaA = linea.CorrientePlacaA;
                break;
            case SubtipoDeCarga.MotorVelocidadAjustable:
                Categoria = CategoriaDeCarga.Motor;
                CapturaMotor = CapturaDeMotor.Variador;
                CorrienteEntradaVariadorA = linea.CorrientePlacaA;
                ProteccionMaximaVariadorA = linea.ProteccionMaximaA;
                break;
            default:
                return false;
        }
        FactorPotencia = linea.FactorPotencia;
        DescripcionDelEquipo = linea.Descripcion;
        Cargas.Clear();
        return true;
    }

    /// <summary>
    /// Pasa el circuito a un equipo que va solo — un A/A con ampacidad de placa —
    /// con lo capturado en su única línea. <c>false</c> si hay otras líneas: van solos (David, 2026-09-30).
    /// </summary>
    public bool PasarARenglon(SubtipoDeCarga subtipo)
    {
        if (Cargas.Count > 1)
            return false;
        var linea = Cargas.SingleOrDefault();
        switch (subtipo)
        {
            case SubtipoDeCarga.CargaCombinada:
                Categoria = CategoriaDeCarga.AireAcondicionado;
                PlacaAire = PlacaDeAireAcondicionado.AmpacidadYProteccion;
                break;
            default:
                return false;
        }
        if (linea is not null)
        {
            FactorPotencia = linea.FactorPotencia;
            DescripcionDelEquipo = linea.Descripcion;
        }
        Cargas.Clear();
        return true;
    }

    /// <summary>
    /// El subtipo de lo que está capturado en el renglón, para mostrarlo como su línea: un motor, un
    /// variador, un motocompresor, un A/A con ampacidad de placa o uno de habitación.
    /// <c>null</c> si el renglón no es un equipo.
    /// </summary>
    public SubtipoDeCarga? SubtipoDelRenglon =>
        TieneDesglose || Categoria == CategoriaDeCarga.Tablero ? null
        : EsMotor ? CapturaMotor == CapturaDeMotor.Variador ? SubtipoDeCarga.MotorVelocidadAjustable : SubtipoDeCarga.MotorUsoGeneral
        : EsAireAcondicionado ? PlacaAire switch
        {
            PlacaDeAireAcondicionado.AmpacidadYProteccion => SubtipoDeCarga.CargaCombinada,
            PlacaDeAireAcondicionado.Habitacion => SubtipoDeCarga.AireDeHabitacion,
            _ => SubtipoDeCarga.Motocompresor,
        }
        : null;

    /// <summary>
    /// Lo que la exclusividad de un subtipo no permite, ya redactado — decisión <c>captura-en-el-desplegable.md</c>:
    /// el contacto del refrigerador va solo y es uno; los de aparatos pequeños, lavadora y baño, solo con
    /// líneas de su subtipo. <c>null</c> si cumple.
    /// </summary>
    public string? ErrorDeExclusividad()
    {
        // Un alimentador puede llevar varios tableros (215-2(a)(1), 408-36), pero no otras cargas: esas van
        // en sus circuitos derivados (David, 2026-09-30).
        if (Cargas.Any(a => a.EsTablero) && Cargas.Any(a => !a.EsTablero))
            return "Un alimentador a tableros solo lleva tableros — Art. 100, 215. Pasa las demás cargas a otro circuito.";
        if (!TieneDesglose)
            return null;
        if (Cargas.FirstOrDefault(a => a.Subtipo?.VaSolo() == true) is { } sola && (Cargas.Count > 1 || sola.Cantidad > 1))
            return $"{sola.Subtipo!.Value.Nombre()}: va solo en su circuito y es uno — 210-52(b)(1) Excepción 2. Quita las demás líneas o pásalo a otro circuito.";
        if (Cargas.FirstOrDefault(a => a.Subtipo?.SoloConSuSubtipo() == true) is { } exclusiva
            && Cargas.Any(a => a.Subtipo != exclusiva.Subtipo))
            return $"Contactos · {exclusiva.Subtipo!.Value.Nombre()}: el circuito solo alimenta esas salidas — " +
                   (exclusiva.Subtipo == SubtipoDeCarga.ContactoAparatosPequenos ? "210-52(b)(2)" : exclusiva.Subtipo == SubtipoDeCarga.ContactoLavadora ? "210-11(c)(2)" : "210-11(c)(3)") +
                   ". Pasa las demás a otro circuito.";
        return null;
    }

    /// <summary>
    /// Con líneas de subtipo elegido, el tipo del circuito sigue a sus cargas: si ninguna es del tipo que
    /// tenía, toma el de la primera (un Motor que quedó solo con luminarias deja de ser Motor) — I-123.
    /// </summary>
    internal void SincronizarTipo()
    {
        var tipos = Cargas.Where(a => a.Subtipo is not null).Select(a => a.Subtipo!.Value.Tipo()).Distinct().ToList();
        if (tipos.Count > 0 && !tipos.Contains(Categoria))
            Categoria = tipos[0];
    }

    /// <summary>
    /// El aparato con motor que entra al 125 % — 220-18(a), I-118: el mayor de más de ⅛ hp, cuando va con
    /// otras cargas. <c>null</c> si no hay. Lo pone <see cref="CuadroDeCarga"/>.
    /// </summary>
    public CargaDelCircuito? MotorAl125 { get; internal set; }

    /// <summary>Agrega un motor al grupo, en HP — I-115; o, con <paramref name="clase"/>, un motocompresor — I-116.</summary>
    public CargaDelCircuito AgregarMotor(ClaseDeAparato clase = ClaseDeAparato.Motor)
    {
        var nuevo = new CargaDelCircuito { Clase = clase, FactorPotencia = FactorPotencia };
        Cargas.Add(nuevo);
        return nuevo;
    }

    /// <summary>
    /// El 440-4(b) de un equipo con MCA ya suma sus máquinas; en un grupo se capturan una por una —
    /// I-116. Pasa un A/C a «Varios»: el motocompresor capturado por corriente nominal se vuelve el
    /// primero del grupo.
    /// </summary>
    public void PasarAGrupoDeAire()
    {
        if (!EsAireAcondicionado || EsGrupo)
            return;
        if (!Cargas.Any(a => a.EsMaquina) && PlacaAire == PlacaDeAireAcondicionado.CorrienteNominal && CorrientePlacaA > 0m)
            Cargas.Insert(0, new CargaDelCircuito
            {
                Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? "Motocompresor" : Descripcion.Trim(),
                Clase = ClaseDeAparato.Motocompresor,
                CorrientePlacaA = CorrientePlacaA,
                CorrienteSeleccionA = CorrienteSeleccionA,
                FactorPotencia = FactorPotencia,
            });
        // Sin la unidad «Varios» (I-123): el grupo sale de sus motocompresores; la placa regresa a la nominal.
        if (PlacaAire == PlacaDeAireAcondicionado.Grupo)
            PlacaAire = PlacaDeAireAcondicionado.CorrienteNominal;
    }

    /// <summary>
    /// Pasa un Motor a «Varios» — 430-53, I-115. El motor que ya estaba capturado (HP o amperes) se
    /// vuelve el primer motor del grupo, para no perderlo; si el desglose ya trae motores, se quedan.
    /// </summary>
    public void PasarAGrupo()
    {
        if (!EsMotor || EsGrupo)
            return;
        if (!Cargas.Any(a => a.EsMaquina) && TieneCapturaDeMotor)
            Cargas.Insert(0, new CargaDelCircuito
            {
                Descripcion = string.IsNullOrWhiteSpace(Descripcion) ? "Motor" : Descripcion.Trim(),
                Clase = ClaseDeAparato.Motor,
                CapturaMotor = CapturaMotor,
                Hp = Hp,
                CorrientePlacaA = CorrientePlacaA,
                FactorPotencia = FactorPotencia,
            });
        // Sin la unidad «Varios» (I-123): el grupo sale de sus motores.
        if (CapturaMotor is CapturaDeMotor.Grupo or CapturaDeMotor.Variador)
            CapturaMotor = CapturaDeMotor.Hp;
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

    /// <summary>
    /// El F.P. más bajo que se admite — I-80. Arriba de 1, el ángulo de la corriente no existe y el
    /// alimentador tronaba; con 0 o negativo, la caída salía 0 % o negativa.
    /// </summary>
    public const decimal FactorPotenciaMinimo = 0.1m;

    /// <summary>Entre <see cref="FactorPotenciaMinimo"/> y 1, de un circuito o de un aparato.</summary>
    public static bool FactorPotenciaValido(decimal fp) => fp is >= FactorPotenciaMinimo and <= 1m;

    /// <summary>Polos del interruptor. Se cambia por <see cref="CuadroDeCarga.CambiarPolos"/>, que verifica que quepa.</summary>
    public int Polos { get; internal set; } = 1;

    /// <summary>
    /// Los polos que eligió el ingeniero. Al bajar las fases o los espacios, <see cref="Polos"/> se
    /// recorta; al regresar, vuelve a estos si hay lugar — I-83: un motor trifásico pasado a 1 fase
    /// se quedaba en 1 polo.
    /// </summary>
    public int PolosElegidos { get; internal set; } = 1;

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
    public bool TieneCarga => !EsContinuacion && !EsDelPrincipal && !SinTipo && (CargaInstaladaVA > 0m || TieneCapturaDeMotor);

    /// <summary>
    /// Algo capturado: descripción, carga, placa, aparatos o más de un polo. Un renglón así no se lo
    /// come el interruptor principal: se avisa y el principal espera a que uno de los dos se mueva.
    /// </summary>
    public bool TieneCaptura =>
        !string.IsNullOrWhiteSpace(Descripcion) || !string.IsNullOrWhiteSpace(DescripcionDelEquipo) || Continua > 0m || NoContinua > 0m || Hp > 0m || CorrientePlacaA > 0m
        || AmpacidadMinimaA > 0m || CorrienteEntradaVariadorA > 0m || Cargas.Count > 0 || Polos > 1
        || Servicio is not null || NoSimultaneoCon is not null;

    /// <summary>
    /// Lo que este circuito le carga a cada una de sus barras, en VA — la banda «BALANCEO DE FASES»
    /// del Excel (columnas BB, BC, BD): la carga entre el número de fases que toca.
    /// </summary>
    public decimal CargaPorFaseVA => Fases.Length == 0 ? 0m : CargaInstaladaVA / Fases.Length;

    /// <summary>
    /// <b>Quita el equipo del renglón</b> — el bote de su línea (David, 2026-09-30): borra lo capturado de
    /// motor, variador, A/A o carga, y el tipo, como un espacio nuevo. La descripción, los polos, la
    /// longitud y la canalización se quedan: son del espacio, no del equipo.
    /// </summary>
    public void QuitarEquipo()
    {
        Hp = null;
        CorrientePlacaA = 0m;
        CorrienteSeleccionA = null;
        AmpacidadMinimaA = 0m;
        ProteccionMaximaA = 0m;
        CorrienteEntradaVariadorA = 0m;
        ProteccionMaximaVariadorA = 0m;
        Continua = 0m;
        NoContinua = 0m;
        Servicio = null;
        NoArrancaConLaTabla = false;
        CapturaMotor = CapturaDeMotor.Hp;
        PlacaAire = PlacaDeAireAcondicionado.AmpacidadYProteccion;
        Cargas.Clear();
        DescripcionDelEquipo = string.Empty;
        categoria = CategoriaDeCarga.Alumbrado;
        TipoElegido = false;
    }

    internal void Limpiar()
    {
        Resultado = null;
        Error = null;
        CaidaCombinadaPct = null;
        CaidaAlimentadorPct = null;
        AvisoCaidaCombinada = null;
        AvisoAireDeHabitacion = null;
    }

    /// <summary>
    /// Un acondicionador de habitación en el desglose que pasa del 80 % del circuito (solo) o del 50 %
    /// (con otras cargas) — 440-62(b), (c), I-117. Ya redactado; <c>null</c> si cumple o no hay.
    /// </summary>
    public string? AvisoAireDeHabitacion { get; internal set; }

    /// <summary>
    /// Las reglas de su clase — 210-21(b), 210-23, 422-11(e) (I-124): notas y, si no se cumplen, avisos.
    /// Las pone <see cref="CuadroDeCarga"/>.
    /// </summary>
    public IReadOnlyList<ReglaDeClase> ReglasDeClase { get; internal set; } = [];
}

/// <summary>Una regla de la clase del circuito, ya redactada: su referencia, el texto y si es aviso — I-124.</summary>
public sealed record ReglaDeClase(string Referencia, string Texto, bool Aviso);
