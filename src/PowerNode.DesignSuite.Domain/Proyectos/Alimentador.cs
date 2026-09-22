using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// La conexión entre dos elementos de la topología del proyecto (<see cref="ElementoTopologia"/>) — el
/// cable y su protección, no un atributo pegado al elemento que alimenta. ElementoOrigen es quién lo
/// alimenta a él (null = es la acometida del proyecto); ElementoDestino es a quién alimenta.
/// </summary>
public class Alimentador
{
    public int Id { get; set; }

    public int? ElementoOrigenId { get; set; }
    public ElementoTopologia? ElementoOrigen { get; set; }

    public int? ElementoDestinoId { get; set; }
    public ElementoTopologia? ElementoDestino { get; set; }

    public decimal LongitudM { get; set; }

    /// <summary>Número de conductores en paralelo por fase, elegido por el usuario (mínimo 1). Solo válido a partir de 1/0 AWG (310-10(h)(1)).</summary>
    public int NumeroConductoresParalelo { get; set; } = 1;

    public MaterialConductor MaterialConductor { get; set; } = MaterialConductor.Cobre;
    public MaterialCanalizacion MaterialCanalizacion { get; set; } = MaterialCanalizacion.Pvc;

    /// <summary>Designación del aislamiento del conductor (p.ej. "THHN", "THW", "TW") — Tabla 310-104(a). Ver el mismo campo en <see cref="CircuitoDerivado"/>.</summary>
    public string TipoAislamiento { get; set; } = "THHN";

    /// <summary>Lugar de instalación para la Tabla 310-104(a) -- true = seco, false = húmedo/mojado.</summary>
    public bool LugarInstalacionSeco { get; set; } = true;

    /// <summary>Canalización/cable (310-15(b)(16)) o al aire libre (310-15(b)(17)) -- ambas base 30°C.</summary>
    public MetodoInstalacion MetodoInstalacion { get; set; } = MetodoInstalacion.CanalizacionOCable;

    /// <summary>Temperatura ambiente de la corrida — Tabla 310-15(b)(2)(a). Default 30°C (factor 1, sin corrección).</summary>
    public decimal TemperaturaAmbienteC { get; set; } = 30m;

    /// <summary>Conductores portadores de corriente que comparten canalización con este alimentador — Tabla 310-15(b)(3)(a). Default 3 (sin ajuste).</summary>
    public int ConductoresAgrupados { get; set; } = 3;

    /// <summary>
    /// Factor de demanda de la carga <b>continua</b> que este alimentador transporta. <b>1.0 = sin
    /// reducción</b>, que es lo conservador y el valor por omisión.
    ///
    /// <para>
    /// <b>Vive aquí y no en el circuito derivado, y eso es norma, no preferencia.</b> El
    /// <b>220-42</b> lo dice al revés de como se implementó primero: <i>«esos factores no se deben
    /// aplicar para calcular el número de circuitos derivados para iluminación general»</i>. Y el
    /// <b>220-40</b> lo pone donde va: la carga calculada de un alimentador o una acometida es la
    /// suma de los derivados <i>«después de aplicar cualquier factor de demanda aplicable»</i>.
    /// </para>
    ///
    /// <para>
    /// <b>Hasta el 2026-08-20 estuvo en <c>CircuitoDerivado</c> y multiplicaba su carga</b>, o sea que
    /// encogía el conductor y el interruptor del propio derivado — la dirección peligrosa. Y la suma
    /// hacia arriba usaba los VA crudos, así que además <i>no le quitaba un solo ampere al
    /// alimentador</i>: hacía justo lo contrario de lo que promete.
    /// </para>
    ///
    /// <para>
    /// <b>El valor lo captura el proyectista</b>, no lo deduce el programa: automatizar el Art. 220
    /// exige tipo de inmueble y las excepciones de juicio de cada tabla, y queda fuera de v1 —
    /// decisión cerrada. Dentro del programa hay un panel con las diez tablas para consultarlas.
    /// </para>
    /// </summary>
    public decimal FactorDemandaContinua { get; set; } = 1m;

    /// <summary>Factor de demanda de la carga no continua. Ver <see cref="FactorDemandaContinua"/>.</summary>
    public decimal FactorDemandaNoContinua { get; set; } = 1m;

    /// <summary>
    /// Con qué tabla del Art. 220 —o con qué criterio— se justifica el factor aplicado. <b>Va a la
    /// memoria de cálculo:</b> un factor de demanda sin sustento es un número que reduce cobre y que
    /// nadie puede defender ante quien revisa.
    /// </summary>
    public string? JustificacionFactorDemanda { get; set; }

    /// <summary>Factor de potencia (cos θ) de la carga agregada, para la caída de tensión.</summary>
    public decimal FactorPotencia { get; set; } = 0.9m;

    /// <summary>
    /// <b>Declaración del proyectista:</b> el ensamble —envolvente y dispositivos de sobrecorriente
    /// juntos— está aprobado para operar al <b>100 %</b> de su valor nominal, y por lo tanto no se le
    /// aplica el 125 % de la carga continua. <b>Falso por omisión</b>, que es la regla general.
    ///
    /// <para>
    /// <b>Es dato de placa y de marcado, no una opción de diseño</b>, y por eso se declara en vez de
    /// deducirse: la excepción de 215-2(a)(1) y 215-3 habla del <b>ensamble</b>, no del interruptor, y hay
    /// gabinetes marcados <i>"Not rated for 100% rated circuit breakers"</i>. El programa lo
    /// contradice cuando el modelo elegido del catálogo es de 80 % —ahí sí hay evidencia—, pero no lo
    /// concede solo. Ver <c>CargaContinua100Pct</c>.
    /// </para>
    /// </summary>
    public bool ConjuntoAprobado100Pct { get; set; }

    /// <summary>
    /// <b>El espacio del tablero de origen donde va montado el interruptor que lo alimenta</b>, o
    /// <c>null</c> cuando su origen no es un tablero de espacios.
    ///
    /// <para>
    /// <b>Que sea opcional es la pieza que hace cerrar todo esto</b>, y la razón por la que no se
    /// eligió el camino de «que todo alimentador salga de un circuito derivado»: un alimentador puede
    /// salir de una <see cref="Acometida"/>, de un <see cref="Transformador"/> o de un
    /// <see cref="CentroControlMotores"/>, y ahí <b>no hay espacios que ocupar</b>. Obligar a que
    /// siempre hubiera uno habría exigido inventar espacios en equipos que no los tienen.
    /// </para>
    ///
    /// <para>
    /// <b>Cuando sí lo hay, ese interruptor NO es el dispositivo final</b> — el destino trae los
    /// suyos—, y por eso lo que sale de él es un alimentador y no un circuito derivado. Es la
    /// definición del Artículo 100 aplicada al pie de la letra, y el <b>408-36</b> la respalda: la
    /// protección obligatoria de un tablero puede estar <i>en cualquier punto del lado de
    /// alimentación</i>, o sea en el tablero padre.
    /// </para>
    /// </summary>
    public int? EspacioTableroId { get; set; }

    /// <inheritdoc cref="EspacioTableroId"/>
    public EspacioTablero? Espacio { get; set; }

    /// <summary>
    /// Pulgadas de bus que ocupa en un tablero I-Line. <b>Atajo al espacio, no un campo</b>: el dato
    /// vive en <see cref="EspacioTablero.EspacioOcupadoPlg"/> desde el 2026-08-20, donde dejó de
    /// estar duplicado entre esta clase y <see cref="CircuitoDerivado"/>.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public decimal? EspacioOcupadoPlg
    {
        get => Espacio?.EspacioOcupadoPlg;
        set
        {
            if (Espacio is null && value is null) return;

            // Declarar pulgadas de bus ES declarar que va montado en un tablero, así que el espacio
            // se crea aquí en vez de exigir que quien captura lo arme aparte. De qué tablero es lo
            // resuelve el contexto al guardar, a partir del origen -- ver DesignSuiteDbContext.
            Espacio ??= new EspacioTablero();
            Espacio.EspacioOcupadoPlg = value;
        }
    }

    // =================================================================================
    // DERIVACIÓN DE UN ALIMENTADOR — 240-21(b). Agregado el 2026-08-25.
    // =================================================================================

    /// <summary>
    /// <b>Este alimentador no sale de un interruptor propio: se deriva de otro alimentador</b>, en un
    /// punto intermedio de su recorrido, y va <b>sin protección contra sobrecorriente en la
    /// derivación</b> — que es exactamente lo que permite 240-21(b) bajo cinco juegos de condiciones.
    /// <c>null</c> es el caso normal: un alimentador que arranca en su propio interruptor.
    ///
    /// <para>
    /// <b>Por qué la derivación es un alimentador y no una entidad aparte (2026-08-25).</b> Lo pidió
    /// el usuario al notar el hueco: <i>«ese alimentador puede alimentar varias cargas, no solo un
    /// tablero hijo»</i>. La tentación era una entidad nueva colgada entre el alimentador y sus
    /// destinos, y habría costado caro: <see cref="ElementoTopologia.AlimentadorEntrante"/> es uno a
    /// uno, así que cada destino habría necesitado <b>dos</b> llaves foráneas anulables con la
    /// invariante «exactamente una» sostenida a mano en cada sitio — el mismo error que ya mató a una
    /// versión anterior de <see cref="ElementoTopologia"/> y que <see cref="EspacioTablero"/> vino a
    /// desarmar.
    /// </para>
    ///
    /// <para>
    /// Siendo un alimentador más, la derivación <b>reusa todo lo que ya funciona</b>: la cascada la
    /// recorre, la caída se acumula, el cortocircuito se propaga, el cuadro la lista y el destino
    /// sigue teniendo un solo alimentador entrante. Lo único propio suyo son las condiciones de
    /// 240-21(b) y una regla de dimensionamiento distinta.
    /// </para>
    ///
    /// <para>
    /// <b>Una derivación no ocupa espacio en el tablero</b> (<see cref="EspacioTableroId"/> queda en
    /// <c>null</c>): no hay interruptor que atornillar, ése es el punto. El espacio lo ocupa el
    /// alimentador padre, una sola vez, y por eso el conteo de espacios libres sigue cuadrando sin
    /// tocar nada.
    /// </para>
    /// </summary>
    public int? DerivadoDeAlimentadorId { get; set; }

    /// <inheritdoc cref="DerivadoDeAlimentadorId"/>
    public Alimentador? DerivadoDe { get; set; }

    /// <summary>Las derivaciones que salen de este alimentador. Vacía en el caso normal.</summary>
    public ICollection<Alimentador> Derivaciones { get; set; } = [];

    /// <summary>Este tramo es una derivación de otro alimentador, y se rige por 240-21(b).</summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool EsDerivacion => DerivadoDeAlimentadorId is not null || DerivadoDe is not null;

    // ---------------------------------------------------------------------------------
    // Las condiciones declarativas de 240-21(b). TODAS son anulables a propósito: no son
    // números que el motor pueda deducir, son hechos de la instalación que solo sabe quien
    // la diseña. `null` = no se capturó, y una condición no capturada NO se declara
    // incumplida -- misma regla que en cortocircuito y en el catálogo de I-Line.
    //
    // Solo tienen sentido cuando EsDerivacion; en un alimentador normal se quedan nulas.
    // ---------------------------------------------------------------------------------

    /// <summary>240-21(b)(2)(2), (b)(4)(4), (b)(5)(2): termina en un solo interruptor automático o un
    /// solo conjunto de fusibles que limita la carga a la ampacidad del conductor.</summary>
    public bool? TerminaEnUnSoloDispositivo { get; set; }

    /// <summary>240-21(b)(2)(3), (b)(3)(4), (b)(4)(5): protegida contra daño físico en canalización
    /// aprobada u otros medios aprobados.</summary>
    public bool? ProtegidaContraDanoFisico { get; set; }

    /// <summary>240-21(b)(1)(2): los conductores no se extienden más allá del tablero o desconectador
    /// que alimentan.</summary>
    public bool? NoSeExtiendeMasAllaDelTablero { get; set; }

    /// <summary>240-21(b)(1)(3): salvo en el punto de conexión, van en canalización desde la
    /// derivación hasta la envolvente que alimentan. Es <b>más estricto</b> que
    /// <see cref="ProtegidaContraDanoFisico"/>, y por eso es un dato aparte.</summary>
    public bool? EnCanalizacionDesdeLaDerivacion { get; set; }

    /// <summary>240-21(b)(1)(4): instalación en campo en la que los conductores salen de la envolvente
    /// donde se hizo la derivación. Enciende el piso de 1/10 de la protección del alimentador.</summary>
    public bool SaleDeLaEnvolvente { get; set; }

    /// <summary>240-21(b)(5): los conductores están en el exterior del edificio o estructura, salvo en
    /// el punto de terminación de la carga.</summary>
    public bool EnExterior { get; set; }

    /// <summary>240-21(b)(4)(1): mantenimiento y supervisión aseguran que solo lo atiende personal
    /// calificado.</summary>
    public bool? PersonalCalificado { get; set; }

    /// <summary>240-21(b)(4): altura de las paredes de la nave industrial, en metros. El caso pide más
    /// de 11.00 m.</summary>
    public decimal? AlturaParedNaveM { get; set; }

    /// <summary>240-21(b)(4)(2): longitud horizontal de la derivación, en metros (máximo 8.00 m,
    /// contra los 30.00 m de longitud total).</summary>
    public decimal? LongitudHorizontalM { get; set; }

    /// <summary>240-21(b)(4)(6): los conductores son continuos de extremo a extremo, sin empalmes.</summary>
    public bool? SinEmpalmes { get; set; }

    /// <summary>240-21(b)(4)(8): los conductores no atraviesan paredes, pisos ni techos.</summary>
    public bool? NoAtraviesaMurosNiPisosNiTechos { get; set; }

    /// <summary>240-21(b)(4)(9): a qué altura del piso se hizo la derivación, en metros. El caso pide
    /// 9.00 m o más.</summary>
    public decimal? AlturaDeLaDerivacionM { get; set; }

    /// <summary>240-21(b)(5)(3): la protección es parte integral del medio de desconexión o está
    /// inmediatamente adyacente a él.</summary>
    public bool? DesconectadorIntegradoOAdyacente { get; set; }

    /// <summary>240-21(b)(5)(4): el medio de desconexión está en un lugar fácilmente accesible.</summary>
    public bool? DesconectadorFacilmenteAccesible { get; set; }

    /// <summary>
    /// 240-21(b)(3)(3): suma de las longitudes de primario y secundario, en metros, <b>excluyendo la
    /// parte del primario que ya esté protegida a su ampacidad</b>. Solo aplica cuando la derivación
    /// alimenta un transformador, y por eso no se deduce de <see cref="LongitudM"/>: la porción
    /// excluida es un juicio del diseñador, no una resta.
    /// </summary>
    public decimal? LongitudPrimarioMasSecundarioM { get; set; }

    public CalculoAlimentador? Calculo { get; set; }
}
