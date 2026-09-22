namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// <b>Un espacio del tablero con el dispositivo de protección montado en él.</b> Es lo físico y nada
/// más: dónde está, cuántos espacios ocupa, qué barras toca y qué número de parte se pidió. Lo que
/// cuelga aguas abajo vive en otra entidad.
///
/// <para>
/// <b>Por qué existe (2026-08-20).</b> Hasta esta fecha "el espacio con su interruptor" no era una
/// cosa en el modelo: estaba disuelto dentro de <see cref="CircuitoDerivado"/> como cuatro campos
/// suyos. La consecuencia era que <b>solo un circuito derivado podía ocupar un espacio</b> — y en la
/// obra no es así: cuando un tablero alimenta a otro, ese cable sale de un interruptor montado en un
/// espacio del padre, exactamente igual que el que alimenta unas luminarias. Como
/// <see cref="Alimentador"/> no tenía dónde declararlo, un tablero hijo <b>no ocupaba nada</b>: el
/// editor de gabinete pintaba ese espacio vacío y ofrecía montar ahí otro interruptor, la barra no lo
/// contaba para el desbalanceo, y el cuadro de carga reportaba espacios libres de más.
/// </para>
///
/// <para>
/// <b>La distinción que esto respeta es de la norma, no de diseño.</b> El Artículo 100 define
/// <i>alimentador</i> como los conductores que van de la fuente hasta el <b>dispositivo final</b> de
/// protección contra sobrecorriente, y <i>circuito derivado</i> como los que van <b>desde</b> ese
/// dispositivo final hasta las salidas. O sea: el interruptor de este espacio es la frontera, y de
/// qué lado cae lo que sigue <b>depende de lo que haya aguas abajo</b>, no del aparato:
/// </para>
///
/// <list type="bullet">
/// <item>Si lo que sigue son salidas o un equipo de utilización, este interruptor <b>es</b> el
/// dispositivo final → lo que sale es un <see cref="CircuitoDerivado"/>.</item>
/// <item>Si lo que sigue tiene sus propias protecciones (otro tablero, un CCM), este interruptor
/// <b>no</b> es el final → lo que sale es un <see cref="Alimentador"/>. El <b>408-36</b> lo respalda:
/// un tablero debe estar protegido por un dispositivo que puede estar <i>en cualquier punto del lado
/// de alimentación</i>, o sea el del padre.</item>
/// </list>
///
/// <para>
/// <b>Los dependientes apuntan aquí, y no al revés.</b> Sería tentador que el espacio tuviera dos
/// llaves anulables ("o un circuito derivado o un alimentador"), y sería repetir un error que este
/// repositorio ya pagó una vez: <see cref="Alimentador"/> tenía cuatro llaves foráneas anulables con
/// la invariante «exactamente una de cada lado» sostenida a mano en cada sitio, y eso fue justo lo
/// que vino a matar <see cref="ElementoTopologia"/>. Aquí el espacio es tonto: es una posición.
/// </para>
///
/// <para>
/// <b>Un espacio vacío no tiene renglón.</b> Cuántos espacios existe lo dice
/// <see cref="Tablero.NumeroEspacios"/>; aquí solo viven los ocupados. Materializar los vacíos sería
/// guardar cuarenta filas para decir que no hay nada.
/// </para>
/// </summary>
public class EspacioTablero
{
    public int Id { get; set; }

    public int TableroId { get; set; }
    public Tablero Tablero { get; set; } = null!;

    /// <summary>
    /// El <b>primer</b> espacio que ocupa, empezando en 1. Con <see cref="Polos"/> se sabe cuáles
    /// muerde: un 3 polos montado en el 7 ocupa 7, 9 y 11 — del mismo lado, nones a la izquierda y
    /// pares a la derecha. Eso lo resuelve <see cref="Calculo.Tableros.DistribucionBarras"/>, que
    /// sigue siendo el único lugar que sabe de geometría de barras.
    ///
    /// <para>
    /// <b>Es único por tablero, y ahora lo garantiza la base.</b> El índice único
    /// <c>(TableroId, Numero)</c> vivía en la tabla de circuitos, así que solo impedía que dos
    /// circuitos pelearan el mismo espacio; un alimentador se colaba porque ni siquiera declaraba
    /// espacio. Al mudarse aquí, el mismo índice cubre a los dos <b>sin escribir una sola
    /// validación</b>.
    /// </para>
    /// </summary>
    public int Numero { get; set; }

    /// <summary>Polos del interruptor montado: 1, 2 o 3.</summary>
    public int Polos { get; set; } = 1;

    /// <summary>
    /// Barras que toca, p.ej. "A", "AC", "ACB". <b>Es derivado, no una decisión</b>: sale de
    /// <see cref="Numero"/> + <see cref="Polos"/> + las fases del tablero — dónde atornillas el
    /// interruptor determina a qué barra queda conectado. Lo mantiene al día
    /// <c>Data/Calculo/AsignacionBarrasService</c>, que es el único que lo escribe.
    /// </summary>
    public string? Fase { get; set; }

    /// <summary>
    /// Número de catálogo del interruptor montado, p.ej. "QO120". <b>Null es un estado válido</b>, no
    /// un pendiente: la norma pide un dispositivo de protección adecuado, no un modelo en particular,
    /// y el proyecto se calcula y se entrega completo sin elegir uno. No participa en ningún cálculo
    /// de dimensionamiento; sirve para saber qué se compra y para verificar terminales y
    /// coordinación contra datos reales del catálogo.
    /// </summary>
    public string? ModeloCatalogoProteccion { get; set; }

    /// <summary>
    /// El interruptor lleva <b>neutro conmutado</b> (SWN): corta también el neutro, en disparo común
    /// con las fases.
    ///
    /// <para>
    /// <b>NO ES UN POLO MÁS Y NO OCUPA UN ESPACIO MÁS.</b> Corrección del usuario (2026-08-20): las
    /// barras son las tres fases y ya; el neutro pasa directo a su propia barra, que no se numera.
    /// El catálogo lo confirma — un <c>QO215SWN</c> está sembrado como 2 polos, no como 3. Es una
    /// <b>variante de construcción</b> del aparato: cambia qué número de parte se pide, no cuánto
    /// ocupa ni qué barras toca.
    /// </para>
    /// </summary>
    public bool NeutroConmutado { get; set; }

    /// <summary>
    /// Pulgadas de bus que se come el interruptor. <b>Solo aplica en I-Line</b>, donde lo que limita
    /// al derivado no es un espacio numerado sino la altura del bus; en un tablero NEMA todos los
    /// espacios miden igual y lo que se cuenta son espacios.
    ///
    /// <para>
    /// <b>Manda el catálogo.</b> Si el modelo elegido publica su medida, ésa se usa: es un hecho
    /// físico del aparato. Ésta solo llena el hueco cuando no hay modelo capturado.
    /// </para>
    ///
    /// <para>
    /// Este campo estaba <b>duplicado</b> en <c>CircuitoDerivado</c> y en <c>Alimentador</c> desde el
    /// 2026-08-16 — el parche con el que se resolvió el mismo problema en I-Line antes de que
    /// existiera esta entidad. Al mudarse aquí quedó en un solo lugar, que es la señal de que ésta
    /// era la pieza que faltaba y no otro parche.
    /// </para>
    /// </summary>
    public decimal? EspacioOcupadoPlg { get; set; }

    /// <summary>
    /// Este espacio lo ocupa el <b>interruptor principal del tablero</b>, no un derivado ni un
    /// alimentador saliente.
    ///
    /// <para>
    /// <b>Por qué el principal es un espacio como cualquier otro (2026-08-20).</b> Antes no lo era:
    /// el editor de gabinete lo dibujaba con un <c>for n = 1..EspaciosInterruptorPrincipal</c> que
    /// reservaba los espacios <b>1, 2 y 3</b> — mezclando las dos columnas, que es una geometría que
    /// no existe. Un interruptor de 3 polos ocupa N, N+2, N+4 <i>del mismo lado</i>, y eso ya lo
    /// sabía <see cref="Calculo.Tableros.DistribucionBarras"/>; el principal simplemente no pasaba
    /// por ahí, porque estaba resuelto a mano fuera del modelo.
    /// </para>
    ///
    /// <para>
    /// <b>Y se mueve.</b> Corrección del usuario: en obra el principal montado entre los derivados
    /// <i>casi siempre lo ponen hasta abajo</i>, no arriba. No hay nada que lo fije al espacio 1.
    /// </para>
    ///
    /// <para>
    /// <b>Es una bandera y no una llave foránea</b>, al revés que el circuito derivado y el
    /// alimentador, que sí apuntan aquí. La diferencia es real: aquellos son cosas que existen por su
    /// cuenta y se montan en un espacio; el principal <b>no tiene aguas abajo</b> — es el tablero
    /// mismo protegiéndose. Una llave de <c>Tableros</c> hacia acá además abriría un segundo camino
    /// de borrado hacia esta tabla, que es exactamente el problema que ya cuesta caro en
    /// <c>Alimentador</c> (ver <c>BorradoDeProyecto</c>).
    /// </para>
    ///
    /// <para>
    /// <b>Que exista este renglón NO es lo mismo que "el tablero tiene principal".</b> Un principal
    /// con alojamiento propio —el caso de los NQ de 150 A para arriba, los NF y los I-Line— sí
    /// existe y no ocupa espacio numerado: ahí <see cref="Tablero.EspaciosInterruptorPrincipal"/>
    /// vale 0 y este renglón no se crea. Ver <see cref="Tablero.PrincipalEnAlojamientoPropio"/>.
    /// </para>
    /// </summary>
    public bool EsInterruptorPrincipal { get; set; }

    /// <summary>
    /// <b>El renglón del directorio del tablero</b> — lo que exige 408-4(a): <i>«Cada circuito y
    /// modificación del circuito se debe identificar de forma legible con su propósito o uso
    /// específico, evidente y claro… La identificación se debe incluir en un directorio del circuito
    /// que se localiza en la parte frontal o interior de la puerta del tablero»</i>.
    ///
    /// <para>
    /// <b>Vive aquí, en el espacio, y no en cada ocupante</b>, porque la norma pide un renglón por
    /// <b>posición</b> y las posiciones son de cuatro clases: el interruptor principal, un circuito
    /// derivado, el alimentador que sale hacia otro tablero, y la <b>posición de reserva</b> — que
    /// 408-4(a) obliga a describir también: <i>«Las posiciones de reserva que contienen dispositivos
    /// de protección contra sobrecorriente o interruptores sin utilizar se deben describir según
    /// corresponda»</i>. Un campo en <c>Alimentador</c> habría dejado a la reserva sin dónde
    /// escribirse.
    /// </para>
    ///
    /// <para>
    /// <b>Lo escribe el usuario, no el programa.</b> Fue la instrucción explícita del usuario el
    /// 2026-08-25: <i>«ese nombre de circuito se lo tiene que poner el Usuario — "Alimentador tablero
    /// alumbrado", "Tablero contactos"… depende de la instalación»</i>. Y la norma le da la razón dos
    /// veces: pide propósito o uso <b>específico</b>, con detalle suficiente para diferenciar un
    /// circuito de otro, y prohíbe describirlo <i>«en una manera que dependa de condiciones
    /// provisionales de ocupación»</i>. Un texto derivado del nombre del destino no cumple ninguna de
    /// las dos: «Alimenta Tablero 3» no declara ningún propósito.
    /// </para>
    ///
    /// <para>
    /// <c>null</c> significa que no se ha escrito. Cuando el ocupante es un circuito derivado, su
    /// propia descripción sirve de omisión al imprimir — pero eso lo resuelve quien imprime, y no se
    /// copia aquí: dos copias del mismo texto se separan a la primera edición.
    /// </para>
    /// </summary>
    public string? Descripcion { get; set; }
}
