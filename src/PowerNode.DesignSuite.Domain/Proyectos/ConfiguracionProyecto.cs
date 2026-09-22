using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Límites y criterios configurables por proyecto. La norma deja algunos de estos valores como
/// nota informativa (caída de tensión) o sin definir un umbral (desbalanceo); aquí se fijan los
/// defaults de práctica y se permite ajustarlos.
/// </summary>
public class ConfiguracionProyecto
{
    public int Id { get; set; }
    public int ProyectoId { get; set; }
    public Proyecto Proyecto { get; set; } = null!;

    /// <summary>Límite de caída de tensión en un circuito derivado. Error duro. Nota informativa 210-19(a)(1) NOTA 4: 3%.</summary>
    public decimal CaidaTensionMaxDerivadoPct { get; set; } = 3m;

    /// <summary>
    /// Límite de caída de tensión de <b>un</b> alimentador, por sí solo. <b>Error duro</b>: el motor
    /// sube el calibre hasta cumplirlo.
    ///
    /// <b>Campo nuevo (A16, 2026-08-17), y el motivo importa:</b> hasta esa fecha este límite salía de
    /// <see cref="CaidaTensionMaxCombinadoPct"/> — o sea, el valor llamado "combinado" se aplicaba a
    /// cada tramo por separado, que es justo lo que la norma <i>no</i> dice. El nombre prometía algo
    /// que el código no hacía. Ahora son dos cosas distintas: ésta es criterio de diseño por tramo, y
    /// la otra es la verificación acumulada de la rama.
    ///
    /// <b>Arranca en 2 %</b>, que con el 3 % del derivado da los <b>5 % combinados</b> de la NOTA 4 —
    /// la práctica común de diseño, y la única repartición en la que el acumulado <i>cumple solo</i>.
    ///
    /// <para>
    /// <b>Estuvo en 5 % del 2026-08-17 al 2026-08-20</b>, y no por criterio: era el valor que este
    /// límite tenía cuando se llamaba "combinado" y se aplicaba por tramo, y se conservó para que
    /// ningún proyecto ya capturado cambiara de calibres al partir el campo en dos. Ese cuidado ya lo
    /// hizo la migración —escribió 5 en los renglones que existían— así que este número solo decide
    /// con qué nace un proyecto NUEVO, y 5 % + 3 % = 8 % hacía que el aviso de acumulado saltara
    /// prácticamente siempre. Un aviso que salta siempre deja de leerse.
    /// </para>
    ///
    /// <para>
    /// <b>Los proyectos ya guardados no se tocan:</b> conservan lo que tengan en su renglón.
    /// </para>
    /// </summary>
    public decimal CaidaTensionMaxAlimentadorPct { get; set; } = 2m;

    /// <summary>
    /// Límite de caída de tensión <b>combinada</b> de la rama completa (alimentador(es) + derivado),
    /// acumulada desde la acometida y reiniciada en cada transformador.
    ///
    /// <b>Aviso, no error duro</b> — la NOTA 4 de 210-19(a)(1) es informativa, no requisito. Lo
    /// consume <see cref="Calculo.Casos.CaidaTensionAcumulada"/>. Nota informativa: 5%.
    /// </summary>
    public decimal CaidaTensionMaxCombinadoPct { get; set; } = 5m;

    /// <summary>Umbral de aviso de desbalanceo entre fases de un tablero. Diagnóstico, no bloquea.</summary>
    public decimal DesbalanceoMaxPct { get; set; } = 10m;

    /// <summary>
    /// Piso de calibre más estricto que el normativo (2.08 mm² / 14 AWG, Art. 210-19(a)(4)) para
    /// circuitos de Alumbrado. Null = usa el piso normativo.
    ///
    /// <para>
    /// <b>Nace en 3.31 mm² (12 AWG) desde el 2026-08-21, a pedido del usuario:</b> el criterio
    /// conservador viene <b>activado</b> en un proyecto nuevo en vez de tener que acordarse de
    /// prenderlo. La norma permite 14 AWG en alumbrado; la práctica de este despacho no baja de 12.
    /// Se puede vaciar en Configuración y entonces vuelve a mandar el piso normativo.
    /// </para>
    ///
    /// <para><b>Los proyectos ya guardados no se tocan:</b> conservan lo que tengan en su renglón,
    /// incluido el <c>null</c> con el que nacieron.</para>
    /// </summary>
    public decimal? PisoPracticoAlumbradoMm2 { get; set; } = CalibresDePractica.DoceAwgMm2;

    /// <summary>
    /// Igual que <see cref="PisoPracticoAlumbradoMm2"/> pero para Contactos, y <b>un escalón más
    /// arriba: 5.26 mm² (10 AWG)</b>, dictado por el usuario el 2026-08-21.
    /// </summary>
    public decimal? PisoPracticoContactosMm2 { get; set; } = CalibresDePractica.DiezAwgMm2;

    /// <summary>
    /// Igual que los dos anteriores pero para circuitos de <see cref="Calculo.Unidades.TipoCarga.Equipo"/>.
    ///
    /// <para>
    /// <b>Nace en null a propósito</b> (piso normativo). Un aparato se dimensiona por su consumo de
    /// placa y no hay una costumbre de despacho que imponerle: una campana de 90 W en 12 AWG sería
    /// cable de sobra. Queda capturable para quien sí quiera fijarle uno.
    /// </para>
    /// </summary>
    public decimal? PisoPracticoEquipoMm2 { get; set; }

    /// <summary>
    /// Estándar de simbología que la UI (bloque 10) usa para dibujar tableros, interruptores,
    /// transformadores, motores, tierra, etc. Ver <see cref="EstandarSimbologia"/>.
    ///
    /// <b>Hoy tiene un solo valor posible</b> (NMX-J-136) desde que el usuario mandó borrar la
    /// librería IEC el 2026-08-19. Se conserva la propiedad —en vez de quitarla— porque es lo que
    /// deja la puerta abierta a otra simbología sin migrar de nuevo, y porque cada proyecto ya
    /// guardado tiene su valor escrito.
    /// </summary>
    public EstandarSimbologia EstandarSimbologia { get; set; } = EstandarSimbologia.NmxJ136;

    /// <summary>
    /// Altitud del sitio sobre el nivel del mar, en metros. Derratea la capacidad de los
    /// transformadores del proyecto (NMX-J-116 / IEC 60076-2). Es de proyecto y no de cada
    /// transformador porque la obra está donde está. 0 = nivel del mar, sin derrateo.
    ///
    /// **No sale de la NOM-001**: se verificó que la norma solo menciona altitud en el Art. 922
    /// (líneas aéreas &gt;50 kV) y para separaciones, no para capacidad.
    /// </summary>
    public decimal AltitudMsnm { get; set; }

    /// <summary>
    /// Temperatura ambiente promedio máxima del sitio en un período de 24 h, en °C. Es la que la
    /// Tabla 1 de la NMX-J-116 compara para decidir si un transformador puede operar a capacidad
    /// nominal a la altitud del proyecto o si hay que derratearlo.
    ///
    /// Default 30 °C: es el valor que la propia norma da para 1 000 msnm y el ambiente de
    /// referencia del 5.1.2 ("el promedio del ambiente durante cualquier período de 24 h no exceda
    /// de 30 °C").
    /// </summary>
    public decimal TemperaturaAmbiente24hC { get; set; } = 30m;

    /// <summary>
    /// Hasta cuántos conductores en paralelo por fase puede alcanzar el motor <b>por su cuenta</b>
    /// cuando ni el calibre más grande del catálogo baja la caída de tensión al límite. Al llegar
    /// aquí se rinde con un error que dice que hay que acortar la corrida o relajar el límite.
    ///
    /// <para>
    /// <b>Es un tope práctico, no normativo</b> — 310-10(h) exige que los paralelos sean del mismo
    /// calibre, material, longitud y terminación, y no fija un máximo. Estaba enterrado como constante
    /// privada de <c>SeleccionConductor</c> hasta la auditoría del 2026-08-19 (§4.1 grupo C); el
    /// valor por omisión es el mismo 6 que tenía, así que ningún proyecto cambia de calibres.
    /// </para>
    ///
    /// <para>
    /// <b>Va aquí y no en los defaults de captura</b>: es un tope contra el que el motor se detiene,
    /// no un valor inicial de un campo. Un N capturado mayor que este tope gana — el motor nunca
    /// baja de lo que el proyectista pidió.
    /// </para>
    /// </summary>
    public int MaxConductoresParaleloAutomatico { get; set; } = 6;

    /// <summary>
    /// <b>Con qué valores nace un elemento nuevo de este proyecto</b> — el nivel intermedio entre los
    /// límites de arriba y el campo del elemento. Ver <see cref="Proyectos.DefaultsDeCaptura"/> para
    /// por qué son tres niveles y por qué ninguno se mueve de lugar.
    ///
    /// <para>
    /// <b>Ojo con la diferencia:</b> todo lo de arriba en esta clase son <b>topes contra los que el
    /// motor verifica</b>; esto son <b>valores iniciales de captura</b> y el motor no los lee nunca.
    /// Mezclarlos sería confundir "el proyecto no admite más de 3 % de caída" con "los circuitos de
    /// este proyecto nacen en aluminio".
    /// </para>
    /// </summary>
    public DefaultsDeCaptura Defaults { get; set; } = new();
}
