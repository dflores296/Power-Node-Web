namespace PowerNode.DesignSuite.Calculo.Unidades;

/// <summary>
/// Qué clase de carga cuelga de un circuito derivado — el eje de "tipo de carga", separado del nivel
/// (tablero/alimentador). Es lo único que decide qué calculadora corre: <see cref="Fuerza"/> va por el
/// Art. 430 y los otros tres por el Art. 210.
/// </summary>
public enum TipoCarga
{
    Alumbrado,
    Contactos,

    /// <summary>Motor de placa — Art. 430. Se dimensiona desde los Hp y las tablas de FLC, no desde VA capturados.</summary>
    Fuerza,

    /// <summary>
    /// Un aparato o equipo que se declara por su <b>consumo de placa</b>: aire acondicionado,
    /// refrigerador, cocina eléctrica, secadora, horno.
    ///
    /// <para>
    /// <b>Por qué no es <see cref="Fuerza"/>, que fue justo la queja del usuario el 2026-08-21:</b>
    /// casi todos estos aparatos traen un motor adentro, pero <b>no traen datos de placa de motor</b>.
    /// El formulario del Art. 430 pide Hp, tipo de motor y tipo de dispositivo de protección para
    /// entrar a las Tablas 430-247/248/249/250 — y la placa de un refrigerador no dice Hp: dice watts,
    /// o amperes, o VA. Un equipo así es un ensamble (motor + resistencias + control), no un motor.
    /// Meterlo en Fuerza obligaba a inventarle Hp, que es exactamente el número que después sale
    /// impreso en la memoria de cálculo.
    /// </para>
    ///
    /// <para>
    /// <b>Calcula por la vía del Art. 210</b>, igual que Alumbrado y Contactos: la carga sale de los
    /// renglones capturados. Lo que cambia es <b>qué se captura</b> — consumo declarado, sin tipo de
    /// contacto ni clase de luminaria, que en un aparato no significan nada.
    /// </para>
    ///
    /// <para>
    /// <b>Fuera a propósito:</b> el Art. 440 (aire acondicionado con MCA/MOCP marcados) y los factores
    /// de demanda del 220-55 (estufas eléctricas). Los dos son régimen propio, no un campo más; hoy el
    /// equipo se dimensiona por su consumo declarado, que es el caso general y el conservador.
    /// </para>
    /// </summary>
    Equipo
}

/// <summary>
/// La intención de diseño de un tablero: qué se espera colgarle. **No participa en ningún
/// cálculo** — el motor sigue mirando el <see cref="TipoCarga"/> de cada circuito, que es el
/// único que decide qué calculadora corre. Esto existe porque un tablero de fuerza y uno de
/// cargas generales no se capturan igual: cambia qué tipos de circuito ofrece la cuadrícula por
/// omisión y cómo se rotula el elemento en la topología. Un tablero declarado de un tipo NO impide
/// meterle un circuito de otro; solo deja de sugerirlo (usa <see cref="Mixto"/> si de verdad
/// lleva de todo).
/// </summary>
public enum TipoTablero
{
    /// <summary>Alumbrado y contactos — lo que el Excel original llamaba "cuadro de carga" normal.</summary>
    CargasGenerales,

    /// <summary>Motores, bombas, aire acondicionado — circuitos del Art. 430.</summary>
    Fuerza,

    /// <summary>Lleva de los dos; la cuadrícula ofrece todos los tipos de circuito sin sugerir ninguno.</summary>
    Mixto,

    /// <summary>
    /// Tablero de control: el que aloja el mando y la señalización de un proceso, no la carga.
    ///
    /// Se agregó el 2026-08-19, a pedido del usuario, para que la figura 4.2.93 de la NMX-J-136
    /// (TDC) tenga quién la pida — estaba dibujada y probada desde la revisión de simbología, pero
    /// ningún tipo de tablero podía elegirla. No cambia ningún cálculo: igual que los otros tres,
    /// <c>TipoTablero</c> clasifica lo que se le cuelga al tablero, y eso hoy sólo decide el símbolo
    /// y los valores por omisión de la captura.
    /// </summary>
    Control
}

/// <summary>
/// Tipo de lugar donde está instalado el transformador — dimensión de la Tabla 450-3(a)
/// (transformadores de más de 600 V). Cambia los porcentajes permitidos y, sobre todo, el
/// redondeo: los renglones de "cualquier lugar" traen la Nota 1 (se permite el estándar inmediato
/// SUPERIOR) y los de lugar supervisado no, así que ahí no se puede exceder el techo.
/// </summary>
public enum TipoLugarTransformador
{
    /// <summary>Cualquier lugar. Es el caso conservador y el que aplica salvo que se justifique el otro.</summary>
    CualquierLugar,

    /// <summary>
    /// Lugar supervisado. Nota 3 de la tabla, textual: "aquel en que las condiciones de
    /// mantenimiento y supervisión aseguren que solamente personal calificado supervisará y
    /// prestará servicio a la instalación de transformadores".
    /// </summary>
    Supervisado
}

/// <summary>
/// Con qué se protege el transformador. La Tabla 450-3(a) da porcentajes distintos para
/// interruptor automático y para fusible (p.ej. 600% vs 300% en el primario con impedancia ≤6%).
/// La Nota 4 manda tratar los fusibles electrónicos ajustables como interruptores automáticos.
/// </summary>
public enum DispositivoProteccionTransformador
{
    InterruptorAutomatico,
    Fusible
}

/// <summary>
/// Cómo se enfría el transformador. Decide cuánta capacidad pierde por altitud: el aire menos
/// denso enfría peor, y no todos los sistemas de enfriamiento sufren igual. Ver
/// <see cref="Casos.CalculadoraAltitudTransformador"/>.
/// </summary>
/// <summary>
/// Clasificación del transformador por su instalación, de la NMX-J-116. No es una preferencia de
/// montaje: la norma solo fabrica el tipo poste dentro de ciertas capacidades (monofásico de 5 a
/// 167 kVA, trifásico de 15 a 150 kVA), y arriba de ahí es tipo subestación.
/// </summary>
public enum TipoInstalacionTransformador
{
    /// <summary>Tipo subestación. El caso general, y el único posible arriba de 167/150 kVA.</summary>
    Subestacion,

    /// <summary>Tipo poste. Solo existe dentro del rango de capacidad que fija la norma.</summary>
    Poste
}

public enum TipoEnfriamientoTransformador
{
    /// <summary>Seco, enfriamiento natural por aire (AN / clase AA).</summary>
    SecoAireNatural,

    /// <summary>Seco con ventilación forzada (AF / clase AA-FA).</summary>
    SecoAireForzado,

    /// <summary>Sumergido en aceite, enfriamiento natural (ONAN / clase OA).</summary>
    AceiteAireNatural,

    /// <summary>Sumergido en aceite con radiadores y ventiladores (ONAF / clase OA-FA).</summary>
    AceiteAireForzado
}

/// <summary>
/// Cómo llega la alimentación al tablero, en el sentido físico del catálogo (Schneider NQ/NF lo
/// llama así en sus tablas de selección y cambia el número de catálogo: sufijo "L" vs "AB").
///
/// Importa para el editor de espacios, no para el cálculo: con interruptor principal, algunos
/// tableros lo alojan en un espacio propio y otros **se comen espacios de derivados** para
/// montarlo — el compendiado Schneider lo dice explícito para los NQ de 100 A: dos derivados en
/// monofásico, tres en trifásico.
/// </summary>
public enum TipoAcometidaTablero
{
    /// <summary>Zapatas principales: la acometida llega a zapatas, sin interruptor en el tablero.</summary>
    ZapatasPrincipales,

    /// <summary>Interruptor principal montado en el tablero mismo.</summary>
    InterruptorPrincipal
}

/// <summary>
/// Variante de un contacto de uso general. GFCI (210-8) y Tierra Aislada (250-146(d)) no son solo
/// un símbolo distinto — traen requisitos de instalación reales (protección por falla a tierra;
/// conductor de tierra dedicado que no se bondea en tableros intermedios, respectivamente).
/// </summary>
/// <summary>
/// Dónde va montado el receptáculo. <b>Es un eje aparte del tipo y de la configuración</b>: un
/// contacto de piso puede ser normal o GFCI, y sigue siendo de piso.
///
/// <para>
/// Existe porque la NMX-J-136-ANCE publica <b>figuras propias</b> para dos montajes —4.2.32 (RECP,
/// receptáculo de piso) y 4.2.37 (RECPI, receptáculo para intemperie)— y sin este campo esos dos
/// dibujos vivían en la librería del programa sin que nada pudiera producirlos. Ver el barrido de
/// UX del 2026-08-19, punto 7.
/// </para>
///
/// <para>
/// <b>Ojo con la intemperie y el 210-8:</b> la figura de intemperie de esta librería lleva la
/// leyenda <c>GFCI</c> incorporada, por decisión del usuario, y eso concuerda con lo que la norma
/// pide para exteriores. Un contacto marcado de intemperie que NO se capturó como GFCI se dibuja
/// igual con esa leyenda, y el programa lo <b>advierte</b> en vez de callarlo.
/// </para>
/// </summary>
public enum MontajeContacto
{
    /// <summary>En muro, que es el caso normal y lo único que el programa representaba antes.</summary>
    Pared,

    /// <summary>En piso — NMX 4.2.32 (RECP).</summary>
    Piso,

    /// <summary>A la intemperie — NMX 4.2.37 (RECPI).</summary>
    Intemperie,
}

/// <summary>
/// Qué clase de luminaria es el renglón de alumbrado. Igual que <see cref="MontajeContacto"/>,
/// existe porque la NMX publica figuras propias —4.2.48 (LPI) y 4.2.49 (LPE), los dos arbotantes— y
/// sin este campo no había forma de pedirlas.
///
/// <b>No entra en ningún cálculo</b>: los VA salen de lo capturado en el renglón, no del tipo de
/// luminaria. Es dato de plano.
/// </summary>
public enum TipoLuminaria
{
    /// <summary>Salida de alumbrado genérica: techo o plafón. Es el caso normal.</summary>
    General,

    /// <summary>Arbotante interior, luminario en pared — NMX 4.2.48 (LPI).</summary>
    ArbotanteInterior,

    /// <summary>Arbotante exterior, luminario en pared — NMX 4.2.49 (LPE).</summary>
    ArbotanteExterior,
}

public enum TipoContacto
{
    Normal,
    Gfci,
    TierraAislada,
    Especial
}

/// <summary>
/// Cuántas salidas tiene el dispositivo. Es un eje INDEPENDIENTE de <see cref="TipoContacto"/>: un
/// contacto es «GFCI» o «tierra aislada» por su marca y sus requisitos de instalación, y aparte es
/// sencillo, dúplex o trifásico por su configuración física. Los dos ejes se combinan.
///
/// Existe desde el 2026-08-19, cuando el usuario definió la familia completa de símbolos de
/// contacto: hasta entonces el programa solo sabía dibujar el dúplex, y la configuración real del
/// dispositivo no se podía capturar en ninguna parte.
///
/// **No participa en ningún cálculo.** El VA sale de <c>VaUnitario × Cantidad</c>, que ya es por
/// dispositivo; cuántas salidas tenga ese dispositivo no cambia la carga que declaraste.
/// </summary>
public enum ConfiguracionContacto
{
    Sencillo,
    Duplex,
    Trifasico
}

/// <summary>Determina qué tabla de FLC aplica: 430-247 (CD), 430-248 (1φ), 430-249 (2φ), 430-250 (3φ).</summary>
public enum TipoAlimentacionMotor
{
    CorrienteContinua,
    Monofasico,
    DosFases,
    Trifasico
}

/// <summary>Filas de la Tabla 430-52 (tipo de motor).</summary>
public enum TipoMotor
{
    Monofasico,
    PolifasicoJaulaArdilla,
    DisenoBAltaEficiencia,
    Sincrono,
    RotorDevanado,
    CorrienteContinua
}

/// <summary>Columnas de la Tabla 430-52 (dispositivo de protección).</summary>
public enum TipoDispositivoProteccionMotor
{
    FusibleSinRetardoDeTiempo,
    FusibleDeDosElementosConRetardo,
    InterruptorDisparoInstantaneo,
    InterruptorTiempoInverso
}
