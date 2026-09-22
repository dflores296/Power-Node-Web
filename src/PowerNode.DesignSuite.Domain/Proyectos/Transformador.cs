using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Un transformador, como elemento de la topología — al mismo nivel que Tablero, no un atributo de un
/// Alimentador. El primario y el secundario son corridas de conductor distintas (cambia tensión,
/// corriente, y a veces material), así que cada lado necesita su propio Alimentador con su propio
/// cálculo. Bloque 10: ya tiene protección propia (Art. 450-3(b), ver <see cref="Calculo"/>) --
/// sigue sin modelo de impedancia/regulación (%Z), eso queda para un plan aparte.
/// </summary>
public class Transformador : ElementoTopologia
{
    /// <summary>
    /// Número de catálogo del modelo elegido, p.ej. "112T132H". Solo es memoria de qué se eligió: al
    /// seleccionarlo se copian sus características al transformador, que siguen siendo editables.
    /// Null = capturado a mano, sin modelo de catálogo — y es un caso normal, no un pendiente: los
    /// transformadores de media a baja no están en el compendiado y simplemente no tienen modelo.
    ///
    /// <b>El %Z no se llena desde aquí</b>: el catálogo no lo publica y sigue saliendo de la placa.
    /// </summary>
    public string? ModeloCatalogo { get; set; }

    public decimal CapacidadKva { get; set; }
    public decimal TensionPrimariaV { get; set; }
    public decimal TensionSecundariaV { get; set; }

    /// <summary>1 (monofásico) o 3 (trifásico) — decide si la corriente nominal usa la raíz de 3 o no.</summary>
    public int NumeroFases { get; set; } = 3;

    /// <summary>Esquema de protección (450-3(b)) -- decisión de diseño del usuario, no algo que el motor infiera.</summary>
    public EsquemaProteccionTransformador EsquemaProteccion { get; set; } = EsquemaProteccionTransformador.SoloPrimario;

    /// <summary>
    /// Impedancia porcentual de placa (%Z). Determina la corriente de cortocircuito disponible en
    /// el secundario y la regulación de tensión. Viene de la placa o de la ficha del fabricante,
    /// no se calcula. 0 = no capturada, y entonces no se calcula ni falla ni regulación.
    /// </summary>
    public decimal ImpedanciaPct { get; set; }

    /// <summary>
    /// Relación X/R de placa. Reparte el %Z entre su parte resistiva y su parte reactiva, que es lo
    /// que decide la regulación. Valor típico de un transformador de distribución: 2 a 8.
    /// </summary>
    public decimal RelacionXR { get; set; } = 3m;

    /// <summary>
    /// Factor de potencia (cos θ) de la carga que alimenta el secundario. <b>Solo se usa para la
    /// regulación de tensión</b> del %Z — no interviene en Ip/Is, ni en la protección, ni en la
    /// corriente de falla (ésa es a factor de potencia de cortocircuito, no de carga).
    ///
    /// <para>
    /// <b>Era una constante enterrada en la cascada</b> (<c>factorPotenciaCarga: 0.9m</c>, auditoría
    /// 2026-08-19 §4.1 grupo C), y ahí estaba la incoherencia: el <i>mismo concepto</i> es campo
    /// capturable en <see cref="CircuitoDerivado.FactorPotencia"/>, <see cref="Alimentador.FactorPotencia"/>
    /// y <see cref="Carga.FactorPotencia"/>, y solo el transformador lo tenía escondido. Ahora es
    /// campo, con el mismo 0.9 por omisión, así que ninguna regulación ya calculada cambia.
    /// </para>
    /// </summary>
    public decimal FactorPotenciaCarga { get; set; } = 0.9m;

    /// <summary>
    /// El fabricante lo equipó con protección térmica coordinada contra sobrecarga, dispuesta para
    /// interrumpir la corriente del primario. Es la condición que abre la <b>nota 3 de la Tabla
    /// 450-3(b)</b>, y con ella la protección del primario puede llegar a 6 veces (o 4, según el %Z)
    /// la corriente nominal, en vez del 250 % de la tabla.
    /// <para>
    /// <b>Es declaración del usuario, no algo deducible</b> — como <c>EsMedioDesconexion</c>: sale de
    /// la placa o de la ficha del fabricante, y el programa no puede inferirla de la topología. Por
    /// omisión <c>false</c>, porque suponerla sin verla aflojaría la protección sin sustento.
    /// </para>
    /// <para>
    /// Solo influye en el esquema <b>primario y secundario</b>: en la tabla real el marcador
    /// "véase nota 3" está únicamente en ese renglón. Ver
    /// <see cref="Calculo.Casos.CalculadoraProteccionTransformador"/>.
    /// </para>
    /// </summary>
    public bool ProteccionTermicaCoordinadaFabrica { get; set; }

    /// <summary>
    /// Tipo de lugar. Solo lo usa la Tabla 450-3(a) (más de 600 V): cambia los porcentajes y el
    /// redondeo. "Cualquier lugar" es lo conservador y el valor por omisión.
    /// </summary>
    public TipoLugarTransformador TipoLugar { get; set; } = TipoLugarTransformador.CualquierLugar;

    /// <summary>Con qué se protege. La Tabla 450-3(a) da porcentajes distintos para interruptor y fusible.</summary>
    public DispositivoProteccionTransformador DispositivoProteccion { get; set; }
        = DispositivoProteccionTransformador.InterruptorAutomatico;

    /// <summary>Elevador: sube la tensión (p.ej. a 480 V para bajar el calibre de un tramo largo).</summary>
    public bool EsElevador => TensionSecundariaV > TensionPrimariaV;

    /// <summary>Si el primario pasa de 600 V aplica la Tabla 450-3(a); si no, la 450-3(b).</summary>
    public bool EsMediaTension => TensionPrimariaV > 600m;

    /// <summary>
    /// Cómo está instalado, según la clasificación de la NMX-J-116. Descriptivo, pero la norma le
    /// pone un límite real: el tipo poste solo se fabrica hasta 167 kVA en monofásico y 150 kVA en
    /// trifásico. Ver <c>ValidacionesTransformador.RevisarInstalacion</c>.
    /// </summary>
    public TipoInstalacionTransformador TipoInstalacion { get; set; } = TipoInstalacionTransformador.Subestacion;

    /// <summary>
    /// Transformador **tipo costa**: el 5.1.2 de la NMX-J-116 lo exceptúa de los límites normales
    /// de ambiente y lo normaliza para 50 °C de máxima y 40 °C de promedio en 24 h, contra los
    /// 40 y 30 °C del resto. Es el dato que decide si un sitio caluroso está o no dentro de las
    /// condiciones de servicio del transformador.
    ///
    /// Ojo: **no cambia el derrateo por altitud.** La Tabla 1 no distingue tipo costa, y aquí no se
    /// le inventa un corrimiento que la norma no da.
    /// </summary>
    public bool EsTipoCosta { get; set; }

    /// <summary>
    /// Para qué altitud se diseñó el transformador, en metros sobre el nivel del mar. La NMX-J-116
    /// 5.1.3 normaliza dos: **1 000 m** (para sitios de hasta 1 000) y **2 300 m** (para sitios de
    /// 1 000 a 2 300); arriba de eso se especifica la altitud real.
    ///
    /// Null = usar la que el 5.1.3 exige para la altitud del proyecto, que es lo que debería
    /// haberse pedido al comprarlo. Se captura aparte porque el caso que sí derratea es
    /// justamente el transformador que NO se especificó bien: uno diseñado para 1 000 m instalado
    /// a 2 200.
    /// </summary>
    public decimal? AltitudDisenoMsnm { get; set; }

    /// <summary>
    /// Sistema de enfriamiento. Es dato de placa y se usa para describirlo; **ya no interviene en
    /// el derrateo por altitud** -- la NMX-J-116 da un solo porcentaje (0,4 % por cada 100 m) sin
    /// distinguir enfriamiento. La versión anterior variaba el porcentaje según este campo, con
    /// valores tomados de otra norma.
    /// </summary>
    public TipoEnfriamientoTransformador TipoEnfriamiento { get; set; } = TipoEnfriamientoTransformador.AceiteAireNatural;

    public CalculoTransformador? Calculo { get; set; }
}
