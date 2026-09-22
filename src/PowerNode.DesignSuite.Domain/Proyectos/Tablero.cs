using PowerNode.DesignSuite.Calculo.Tableros;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Domain.Proyectos;

public class Tablero : ElementoTopologia
{
    /// <summary>
    /// Intención de diseño del tablero (cargas generales / fuerza / mixto). Dato de captura y
    /// presentación, no de cálculo: el motor sigue decidiendo por el <see cref="CircuitoDerivado.TipoCarga"/>
    /// de cada circuito. Ver <see cref="TipoTablero"/>.
    /// </summary>
    public TipoTablero TipoTablero { get; set; } = TipoTablero.CargasGenerales;

    public decimal TensionV { get; set; }
    public int Fases { get; set; }
    public int Hilos { get; set; }

    /// <summary>
    /// La identidad eléctrica del tablero — fases <b>y</b> hilos juntas.
    ///
    /// <para>
    /// <b>No es un atajo de comodidad: es lo que impide volver a cometer el bug del 2026-08-21.</b>
    /// Las barras energizadas NO salen de <see cref="Fases"/> por sí solo — un <c>1F-3H</c> tiene dos
    /// barras y un <c>1F-2H</c> tiene una, y las dos son «1 fase». Pasar solo las fases era lo que
    /// permitía preguntarlo mal; con esta propiedad la respuesta correcta es la que está a la mano.
    /// Ver <see cref="Calculo.Tableros.SistemaDelTablero"/>.
    /// </para>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public Calculo.Tableros.SistemaTablero Sistema => new(Fases, Hilos);

    public string? Gabinete { get; set; }

    /// <summary>
    /// Número de catálogo del modelo elegido, p.ej. "NQ424L225". Solo es memoria de qué se eligió:
    /// al seleccionarlo se copian sus características a los campos del tablero, que siguen siendo
    /// editables. Null = tablero capturado a mano, sin modelo de catálogo.
    /// </summary>
    public string? ModeloCatalogo { get; set; }

    /// <summary>
    /// Capacidad física del gabinete, en espacios de un polo (convención NEMA: numeración impar en
    /// la columna izquierda, par en la derecha; un circuito de 2 o 3 fases ocupa esa cantidad de
    /// espacios consecutivos). 0 = sin capturar todavía -- el editor de espacios (bloque 10) no
    /// dibuja filas hasta que se declare.
    ///
    /// <b>Cuando <see cref="SistemaBarras"/> es I-Line significa otra cosa:</b> cuántos derivados
    /// caben, no espacios de un polo.
    /// </summary>
    public int NumeroEspacios { get; set; }

    /// <summary>
    /// Cómo está construido el bus. Sale del catálogo al elegir un modelo, y decide si el editor de
    /// gabinete puede dibujar el interior: la cuadrícula de dos columnas con rotación por pares es
    /// de los tableros NQ y NF, y un I-Line no la tiene.
    ///
    /// El valor por omisión es el de siempre, así que los tableros que ya existían en la base
    /// quedan como estaban.
    /// </summary>
    public SistemaBarras SistemaBarras { get; set; } = SistemaBarras.ColumnasNema;

    /// <summary>
    /// Capacidad interruptiva del tablero, en kA. Sale del catálogo al elegir un modelo (un NQ son
    /// 10 kA; un NF, 18 kA con interruptores estándar y hasta 65 kA con EJB), pero se guarda en el
    /// tablero y no se lee del catálogo al vuelo: el usuario puede especificar un tablero que no
    /// esté en el catálogo, y el valor con el que se calculó tiene que quedar en el proyecto.
    ///
    /// <b>Null = no capturada</b>, y entonces la verificación contra la corriente de falla no se
    /// hace y lo dice. No se supone un valor: un falso "sí aguanta" aquí es un tablero que explota.
    /// </summary>
    public decimal? CapacidadInterruptivaKa { get; set; }

    /// <summary>
    /// <b>La capacidad nominal de la barra</b>, en amperes — lo que el tablero aguanta. Es contra
    /// este número que el <b>408-36</b> mide al dispositivo que lo protege: <i>«de valor nominal no
    /// mayor que la del panel»</i>. Ver <see cref="Calculo.Tableros.Verificacion408_36"/>.
    ///
    /// <para>
    /// <b><c>null</c> es un estado válido y no dispara ningún aviso.</b> Un tablero se puede calcular
    /// completo sin elegir modelo de catálogo, y declarar un incumplimiento por no tener el dato
    /// sería el error contrario al que la verificación evita — mismo criterio que
    /// <see cref="CapacidadInterruptivaKa"/>. Se llena solo al elegir un modelo del catálogo.
    /// </para>
    /// </summary>
    public decimal? CapacidadBarraA { get; set; }

    /// <summary>Zapatas principales o interruptor principal montado en el tablero. Ver <see cref="TipoAcometidaTablero"/>.</summary>
    public TipoAcometidaTablero TipoAcometida { get; set; } = TipoAcometidaTablero.ZapatasPrincipales;

    /// <summary>
    /// <b>Declaración del proyectista:</b> el ensamble —envolvente y dispositivos de sobrecorriente
    /// juntos— de ESTE tablero —el gabinete con su interruptor principal— está aprobado para operar al <b>100 %</b> de su valor nominal, y por lo tanto no se le
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
    /// Cuántos espacios de derivados se come el interruptor principal, cuando va montado entre las
    /// secciones en vez de en un espacio propio. 0 = no ocupa ninguno (zapatas principales, o
    /// principal con su propio alojamiento abajo).
    ///
    /// No es un detalle cosmético: esos espacios quedan inutilizables para circuitos derivados y
    /// el editor de gabinete tiene que reservarlos. El compendiado Schneider lo documenta para los
    /// NQ de 100 A con interruptor principal — 2 espacios en 1 fase 3 hilos, 3 en 3 fases 4 hilos.
    /// </summary>
    public int EspaciosInterruptorPrincipal { get; set; }

    /// <summary>¿Este tablero trae interruptor principal? Lo dice su acometida.</summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool UsaInterruptorPrincipal => TipoAcometida == TipoAcometidaTablero.InterruptorPrincipal;

    /// <summary>
    /// El principal va montado <b>entre los derivados</b>, quitándoles espacios numerados.
    ///
    /// <para>
    /// <b>Lo decide el marco del interruptor, no la acometida</b>, y por eso se lee de
    /// <see cref="EspaciosInterruptorPrincipal"/> y no de <see cref="TipoAcometida"/>. En un NQ de
    /// 100 A el principal es un QO del mismo marco que un derivado, así que se atornilla entre
    /// ellos — nota textual del catálogo: <i>"en los tableros de 100 A monofásicos se utilizan dos de
    /// los circuitos derivados para el montaje del principal y en los tableros de 100 A trifásicos se
    /// utilizan tres"</i>, y se comprueba sola en que el NQ 14" de 100 A con principal se vende en 15
    /// y 27 polos en vez de 18 y 30. De 150 A para arriba el marco ya es otro y trae kit propio.
    /// </para>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool PrincipalEntreLosDerivados => UsaInterruptorPrincipal && EspaciosInterruptorPrincipal > 0;

    /// <summary>
    /// El principal tiene <b>alojamiento propio</b>: existe, protege al tablero y <b>no le quita ni
    /// un espacio</b> a los derivados. Va abajo, centrado, fuera de las dos columnas.
    ///
    /// <para>
    /// Es el caso de los NQ de 150 A para arriba, los NF y los I-Line — todos los que el catálogo
    /// siembra con <see cref="EspaciosInterruptorPrincipal"/> en 0 y acometida de interruptor
    /// principal. <b>El editor de gabinete lo dibujaba como si no existiera</b>, que es la otra mitad
    /// del hueco que destapó el usuario el 2026-08-20.
    /// </para>
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public bool PrincipalEnAlojamientoPropio => UsaInterruptorPrincipal && EspaciosInterruptorPrincipal <= 0;

    /// <summary>
    /// El espacio donde está montado el interruptor principal, o <c>null</c> si no ocupa ninguno.
    /// Lo crea y lo mantiene <c>EspaciosDeTableroService</c>.
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public EspacioTablero? EspacioDelPrincipal => Espacios.FirstOrDefault(e => e.EsInterruptorPrincipal);

    /// <summary>
    /// <b>Los espacios ocupados de este tablero</b> — la lista única contra la que se cuenta el
    /// interior: qué barra muerde cada interruptor, cuántos espacios quedan libres, si dos cosas
    /// pelean el mismo lugar.
    ///
    /// <para>
    /// <b>Es única a propósito.</b> Aquí caen tanto los espacios de los que sale un
    /// <see cref="CircuitoDerivado"/> como los de los que sale un <see cref="Alimentador"/> hacia
    /// otro elemento. Antes del 2026-08-20 solo existían los primeros, así que un tablero hijo no
    /// ocupaba nada: el gabinete ofrecía su espacio otra vez y la barra no lo contaba.
    /// </para>
    ///
    /// <para>Solo los ocupados. Cuántos hay en total lo dice <see cref="NumeroEspacios"/>.</para>
    /// </summary>
    public List<EspacioTablero> Espacios { get; set; } = [];

    public List<CircuitoDerivado> CircuitosDerivados { get; set; } = [];

    /// <summary>
    /// Resultado propio del tablero, sin importar qué lo alimenta: el interruptor principal (que
    /// protege el Alimentador entrante, o la acometida si es el tablero raíz) y el desbalanceo
    /// entre fases. El interruptor principal se calcula tratando la suma de sus circuitos como uno
    /// solo — así lo hace el Excel original, sin factores de demanda escalonados del Art. 220.
    /// </summary>
    public CalculoTablero? Calculo { get; set; }
}
