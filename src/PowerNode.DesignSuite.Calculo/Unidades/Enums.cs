namespace PowerNode.DesignSuite.Calculo.Unidades;

public enum MaterialConductor
{
    Cobre,
    Aluminio
}

/// <summary>Columna de temperatura de la Tabla 310-15(b)(16); la fija 110-14(c) según el amperaje de las terminales.</summary>
public enum TemperaturaAislamiento
{
    T60 = 60,
    T75 = 75,
    T90 = 90
}

/// <summary>Material de la canalización, relevante para la reactancia/resistencia CA de la Tabla 9.</summary>
public enum MaterialCanalizacion
{
    Pvc,
    Aluminio,
    Acero
}

/// <summary>
/// Cómo está instalado el conductor -- decide qué tabla de ampacidad (base 30°C) aplica: 310-15(b)(16)
/// para conductores en canalización, cable o directamente enterrados (hasta 3 portadores de
/// corriente); 310-15(b)(17) para conductores individuales al aire libre. Ambas comparten la misma
/// tabla de corrección por temperatura ambiente, 310-15(b)(2)(a) (ambas basadas en 30°C, verificado
/// contra el pie de página de cada una). Las tablas de 40°C (310-15(b)(18)/(19)/(20)/(21) --
/// aislamiento especial 150-250°C, conductores sostenidos por mensajero, o desnudos al aire libre)
/// quedan fuera de v1: son casos raros en un tablero/circuito de edificio normal, y requerirían
/// temperaturas de aislamiento que <see cref="TemperaturaAislamiento"/> no representa (esa solo
/// cubre 60/75/90°C) -- ver PLAN-V1.md.
/// </summary>
public enum MetodoInstalacion
{
    CanalizacionOCable,
    AlAireLibre
}

/// <summary>
/// Esquema de protección contra sobrecorriente de un transformador -- Tabla 450-3(b) (≤600V). Es
/// una decisión de diseño explícita, no algo que el motor pueda inferir: cambia el % de la corriente
/// nominal que se usa para elegir la protección, y si el secundario lleva protección propia o no.
/// </summary>
public enum EsquemaProteccionTransformador
{
    /// <summary>Solo el primario lleva protección -- el secundario no la requiere (450-3(b)).</summary>
    SoloPrimario,

    /// <summary>Primario y secundario llevan protección propia (450-3(b)).</summary>
    PrimarioYSecundario
}

/// <summary>
/// Simbología eléctrica con que la UI dibuja tableros, interruptores, transformadores, motores,
/// tierra, luminarias y contactos.
///
/// <b>Tiene un solo valor desde el 2026-08-19, y es una decisión, no un pendiente.</b> Hasta ese
/// día hubo dos —<c>Ansi</c> e <c>Iec60617</c>— y el usuario mandó quedarse sólo con la NMX:
///
/// <list type="bullet">
///   <item><description><b>La IEC 60617 es norma de paga y no la tenemos.</b> La librería IEC se
///   construyó con referencias de internet, que no alcanzan: sostener un juego de símbolos que no
///   se puede contrastar contra su fuente es afirmar algo que no podemos respaldar, y un plano
///   sellado no se firma con eso.</description></item>
///   <item><description><b>La NMX-J-136-ANCE sí la tenemos, y está basada en ANSI</b>, que era
///   justo la librería que ya servía de base. O sea que quedarse con una no fue perder la mitad
///   del trabajo: fue quedarse con la mitad verificable.</description></item>
/// </list>
///
/// <b>Por qué sobrevive el enum teniendo un solo miembro.</b> Porque la capa de dibujo nunca
/// referencia el estándar —sólo claves lógicas (<c>Simbolo.Interruptor</c>…)— y este tipo es lo
/// que mantiene abierta esa costura. Si algún día se compra la IEC, entra como miembro nuevo sin
/// tocar nada más. Borrarlo ahorraría un archivo y costaría esa puerta.
/// </summary>
public enum EstandarSimbologia
{
    /// <summary>
    /// NMX-J-136-ANCE, simbología **nacional** mexicana. Es la única, y la que aplica donde se
    /// entregan estos planos. Ojo: la norma da título, abreviatura y dibujo — **no dice dónde va
    /// cada símbolo**. La convención de uso es nuestra y vive en <c>Calculo/Simbologia/</c>.
    /// </summary>
    NmxJ136
}

/// <summary>
/// Cómo está puesto a tierra el neutro del sistema en la acometida. Los tres valores salen de la
/// terminología literal del Art. 250 de la NOM-001-SEDE-2012 (verificada contra el corpus, no
/// traída de otra norma): 250-184 "Sistemas con neutro sólidamente puesto a tierra", 250-186
/// "Sistemas con neutro puesto a tierra a través de una impedancia" (y 250-36 para el caso de alta
/// impedancia), y 250-21, que reconoce sistemas de 50 a menos de 1000 V que NO requieren ser
/// puestos a tierra.
///
/// Deliberadamente NO se usa la clasificación TN-S / TN-C-S / TT / IT: es de la IEC 60364, no de
/// la NOM, y este programa cita la NOM.
///
/// Hoy es un dato de captura y de memoria de cálculo -- ningún motor lo consume todavía.
/// </summary>
public enum PuestaTierraSistema
{
    /// <summary>Neutro conectado directamente al electrodo, sin impedancia intercalada (250-184). Lo normal en baja tensión.</summary>
    SolidamentePuestoATierra,

    /// <summary>Neutro puesto a tierra a través de una impedancia que limita la corriente de falla (250-186, 250-36).</summary>
    PuestoATierraPorImpedancia,

    /// <summary>Sistema sin puesta a tierra del neutro -- solo donde la norma lo permite (250-21).</summary>
    NoPuestoATierra
}

/// <summary>
/// Con qué hipótesis se evalúa la corriente de cortocircuito disponible en la instalación.
///
/// **Las dos son válidas y el usuario elige — no es que una sea "la buena" y la otra un
/// remedio.** El deber ser en un proyecto formal es pedirle a la suministradora la corriente de
/// falla en el punto de entrega; pero exigir ese dato para poder calcular bloquearía el trabajo
/// (anteproyectos, cotizaciones, revisiones rápidas, o simplemente que CFE todavía no conteste),
/// así que <see cref="BarraInfinita"/> es el valor por omisión y nunca deja de estar disponible.
/// </summary>
public enum MetodoCortocircuito
{
    /// <summary>
    /// Se desprecia la impedancia de la red aguas arriba: toda la impedancia del circuito de falla
    /// es la del transformador. Da el **peor caso** (la falla más alta posible), así que dimensionar
    /// con esto queda del lado seguro. Es lo que se hace cuando no se tiene el dato de la
    /// suministradora, y también una decisión de diseño perfectamente defendible por sí sola.
    /// </summary>
    BarraInfinita,

    /// <summary>
    /// Se usa la corriente de falla que reporta la suministradora en el punto de entrega
    /// (<see cref="Domain.Proyectos.Acometida.CorrienteFallaDisponibleA"/>). Más apegado a la
    /// realidad de la instalación, y lo que corresponde en un proyecto formal.
    /// </summary>
    CorrienteDeLaSuministradora
}

/// <summary>
/// Dónde vive el dispositivo que protege un centro de control de motores. <b>430-94 admite las dos
/// opciones y las enumera</b> ("Esta protección debe ser proporcionada por: (1) Un dispositivo [...]
/// ubicado antes del centro de control de motores o (2) un dispositivo principal [...] ubicado
/// dentro del centro de control de motores"), así que es una decisión de diseño del usuario, no algo
/// que el programa pueda deducir de la topología.
///
/// No cambia el valor calculado de la protección — cambia dónde se instala, y con ello si el CCM
/// tiene medio de desconexión propio, que es lo que 430-95 exige cuando se usa como equipo de
/// acometida.
/// </summary>
public enum UbicacionProteccionCcm
{
    /// <summary>430-94(1): el dispositivo está antes del CCM (típicamente en el tablero que lo alimenta).</summary>
    AguasArriba,

    /// <summary>430-94(2): un dispositivo principal dentro del propio CCM.</summary>
    PrincipalInterno
}

/// <summary>
/// Línea de producto del centro de control de motores. <b>Es lo que decide si corren las
/// validaciones de rango del fabricante:</b> los límites del compendiado Schneider (600 V, 2500 A,
/// derivados de 15 a 1200 A, HP por tipo de arrancador) son de <i>ese</i> producto, y aplicárselos a
/// un CCM de otra marca sería citar una ficha que no le corresponde.
///
/// Por eso <see cref="NoEspecificada"/> es el valor por omisión: sin línea declarada solo corren las
/// verificaciones de la NOM (430-94 y 430-95), que sí aplican a cualquier CCM.
/// </summary>
public enum LineaCcm
{
    /// <summary>Capturado a mano, sin línea de catálogo. Solo se le verifica la NOM.</summary>
    NoEspecificada,

    /// <summary>CCM Modelo 6 estándar e inteligente (compendiado Schneider 2/15). Ver <see cref="Validaciones.EspecificacionCcmModelo6"/>.</summary>
    Modelo6
}

/// <summary>
/// Tipo de gabinete de un centro de control de motores, de las opciones del Formulario CCM del
/// compendiado (2/16). Dato de captura y de entregable; no entra en ningún cálculo eléctrico.
/// </summary>
public enum TipoGabineteCcm
{
    /// <summary>NEMA 1 (con empaques). Interior.</summary>
    Nema1,

    /// <summary>NEMA 3R. A la intemperie.</summary>
    Nema3R
}

/// <summary>
/// Cómo arranca el motor de una unidad de control (430-98(b)). <b>Es dato descriptivo, no de
/// cálculo:</b> ningún motor de esta suite lo consume — sale en la memoria y en el entregable, y
/// nada más.
///
/// La razón es que del lado de la <i>fuerza</i>, que es lo que este programa dimensiona, los cuatro
/// se calculan igual: conductor al 125 % de la FLC (430-22), protección contra cortocircuito por
/// 430-52 y sobrecarga por 430-32. Lo que los distingue vive en el <b>control</b> (contactores,
/// electrónica, rampas), que está fuera del alcance de v1 a propósito.
/// </summary>
public enum TipoArranqueMotor
{
    /// <summary>A tensión plena, directo a la línea. El caso normal.</summary>
    TensionPlena,

    EstrellaDelta,

    /// <summary>Arrancador suave (estado sólido).</summary>
    Suave,

    /// <summary>Variador de frecuencia. Art. 430 Parte J, "Sistemas de accionamiento de velocidad ajustable".</summary>
    VariadorDeFrecuencia
}

/// <summary>
/// Qué aparato es un elemento de <see cref="Domain.Proyectos.Proteccion"/>. Los tipos de medio de
/// desconexión que la norma admite están en 430-109; aquí se modelan los que se colocan como
/// aparato propio en el unifilar.
/// </summary>
public enum TipoProteccion
{
    /// <summary>Interruptor automático en caja moldeada, en su propio gabinete.</summary>
    InterruptorCajaMoldeada,

    /// <summary>Interruptor de seguridad usado como desconectador (con o sin fusibles).</summary>
    InterruptorDeSeguridad,

    /// <summary>Seccionador con fusibles.</summary>
    SeccionadorConFusibles
}

/// <summary>
/// Si un modelo de catálogo sigue vigente o es de una línea anterior. Existe porque el catálogo se
/// actualiza <b>sumando</b>, nunca borrando: un proyecto ya capturado puede referenciar un modelo
/// descontinuado, y quitárselo dejaría su memoria de cálculo sin de dónde salió el equipo.
/// </summary>
public enum VigenciaCatalogo
{
    Vigente,
    LineaAnterior
}
