using PowerNode.DesignSuite.Calculo.Magnitudes;

namespace PowerNode.DesignSuite.Calculo.Tableros;

/// <summary>
/// La identidad eléctrica de un tablero: <b>cuántas fases lo alimentan y cuántos hilos trae</b>. Van
/// juntas porque <b>ninguna de las dos por separado dice cuántas barras energizadas hay</b>, y ése
/// fue exactamente el bug del 2026-08-21.
/// </summary>
public readonly record struct SistemaTablero(int Fases, int Hilos);

/// <summary>
/// En qué configuración está armado el tablero. <b>La cuenta de hilos NUNCA incluye la tierra</b>:
/// son conductores de circuito (fases + neutro). Si la incluyera, un 3F-4H tendría que llamarse 5H.
/// </summary>
public enum ConfiguracionTablero
{
    /// <summary>1F-2H — una fase y neutro, 127 V. <b>Una sola barra energizada.</b></summary>
    UnaFaseDosHilos,

    /// <summary>
    /// 1F-3H — devanado con derivación central: <b>dos barras energizadas</b> en oposición, más
    /// neutro. 240/120 V. La relación F-F : F-N es <b>2:1</b>, no √3.
    /// </summary>
    UnaFaseTresHilos,

    /// <summary>
    /// Dos fases tomadas de una estrella 3F-4H: <b>dos barras</b>, 220Y/127. La relación es <b>√3</b>.
    /// <b>No es una configuración de catálogo</b> —ningún tablero se vende rotulado «2F»— sino de
    /// instalación: se captura a mano.
    /// </summary>
    DosFasesDeEstrella,

    /// <summary>3F-3H — delta: <b>tres barras y sin neutro</b>. No hay tensión fase-neutro.</summary>
    TresFasesTresHilos,

    /// <summary>3F-4H — estrella: <b>tres barras</b> más neutro. 220Y/127. Relación √3.</summary>
    TresFasesCuatroHilos,
}

/// <summary>
/// Cuántas barras energizadas tiene un tablero y a qué tensión trabaja.
///
/// <para>
/// <b>Nace el 2026-08-21 de un defecto real.</b> Las barras salían solo de <c>Fases</c>, así que
/// <c>Fases = 1</c> daba <b>una</b> barra — y en el catálogo hay <b>36 modelos rotulados «1F-3H»</b>
/// (los NQ 20" monofásicos y los centros de carga QO) que tienen <b>dos</b>. La consecuencia era que
/// en el tablero residencial más común <b>no se podía crear un circuito de 2 polos</b>: no había
/// dónde meter la estufa ni el minisplit. Además su interruptor principal salía de 1 polo cuando el
/// catálogo dice que se come 2 espacios, y el desbalanceo daba siempre 0 porque todo caía en la
/// barra A.
/// </para>
///
/// <para>
/// <b>Confirmado con el compendiado Schneider y con un electricista de más experiencia</b>, no
/// deducido:
/// </para>
///
/// <list type="bullet">
/// <item>La tabla QOD publica las tres configuraciones lado a lado con su tensión:
/// <c>1F-2H → 127</c>, <c>2F-3H → 240/120</c>, <c>3F-4H → 220Y/127</c>. Con 127 V solo puede haber
/// una barra; para tener 240 <b>y</b> 120 a la vez hacen falta dos vivos en oposición más neutro.</item>
/// <item>La sección EZM lo dice en dos renglones seguidos: <i>«1F-2H a la salida (127 Vc.a.)»</i> y
/// <i>«1F-3H a la salida (240/120 Vc.a.)»</i>.</item>
/// <item>La nota al pie de las tablas de selección NQ: <i>«en los tableros de 100 A monofásicos se
/// utilizan <b>dos</b> de los circuitos derivados para el montaje del principal y en los de 100 A
/// trifásicos se utilizan <b>tres</b>»</i>. Un principal corta TODAS las barras: dos espacios ⇒ dos
/// polos ⇒ <b>dos barras</b>.</item>
/// <item>Textual del electricista: <i>«es 1 fase 3 hilos, siempre trae 2 barras de energía y una de
/// neutro»</i>; <i>«3 fases 3 hilos, 3 barras de energía sin barra de neutros»</i>; <i>«3 fases 4
/// hilos, 3 barras de energía y una barra de neutros»</i>.</item>
/// </list>
///
/// <para>
/// <b>Y una que resolvió el mismo dictamen:</b> <i>«nunca vas a ver un tablero que diga 2 fases 3
/// hilos»</i>. Los renglones que el compendiado rotula <c>2F-3H</c> son notación floja del mismo
/// 1F-3H — por eso los 14 modelos que estaban sembrados con <c>Fases = 2</c> se resembraron como
/// <c>1F-3H</c>. Después de eso <b>ningún modelo del catálogo produce <c>Fases = 2</c></b>: ese valor
/// queda solo para captura a mano, que es el caso legítimo de un tablero colgado de dos fases de una
/// estrella.
/// </para>
/// </summary>
public static class SistemaDelTablero
{
    /// <summary>
    /// La configuración a la que corresponde un par (fases, hilos).
    ///
    /// <para>
    /// <b>Los hilos se comparan con «al menos»</b> y no con igualdad: un tablero capturado a mano
    /// puede traer un número raro, y lo que decide es si alcanza para llevar neutro además de las
    /// fases. Así un 1F con 4 hilos capturados se sigue leyendo como 1F-3H y no cae a un caso que no
    /// existe.
    /// </para>
    /// </summary>
    public static ConfiguracionTablero De(SistemaTablero sistema) => sistema.Fases switch
    {
        <= 1 => sistema.Hilos >= 3 ? ConfiguracionTablero.UnaFaseTresHilos : ConfiguracionTablero.UnaFaseDosHilos,
        2 => ConfiguracionTablero.DosFasesDeEstrella,
        _ => sistema.Hilos >= 4 ? ConfiguracionTablero.TresFasesCuatroHilos : ConfiguracionTablero.TresFasesTresHilos,
    };

    /// <summary>
    /// Las barras energizadas del tablero. <b>El neutro no está aquí y nunca lo estará</b>: va a la
    /// barra de neutros, que es un accesorio de la masa y no se numera ni se reparte.
    /// </summary>
    public static IReadOnlyList<char> Barras(SistemaTablero sistema) => De(sistema) switch
    {
        ConfiguracionTablero.UnaFaseDosHilos => ['A'],
        ConfiguracionTablero.UnaFaseTresHilos or ConfiguracionTablero.DosFasesDeEstrella => ['A', 'B'],
        _ => ['A', 'B', 'C'],
    };

    /// <summary>
    /// La tensión fase-neutro que le corresponde a la fase-fase capturada.
    ///
    /// <para>
    /// <b>Son tres relaciones distintas y confundirlas cambia el calibre.</b> Estaba escrita como
    /// <c>Fases &gt;= 2 ? /√3 : tal cual</c> <b>en ocho lugares</b>, y con esa forma un centro de
    /// carga de 240 V daba <b>138.6 V</b> de fase-neutro en vez de 120.
    /// </para>
    ///
    /// <list type="bullet">
    /// <item><b>1F-2H:</b> la tensión capturada YA ES la fase-neutro. No hay fase-fase que dividir.</item>
    /// <item><b>1F-3H:</b> derivación central, <b>÷2</b> — 240 da 120.</item>
    /// <item><b>Estrella (2F o 3F-4H):</b> <b>÷√3</b> — 220 da 127.</item>
    /// <item><b>Delta (3F-3H):</b> <b>no hay neutro</b>, así que no hay fase-neutro. Se devuelve la
    /// fase-fase para no inventar una tensión que no existe ni repartir un cero por el sistema; un
    /// circuito de 1 polo a neutro sencillamente no es representable ahí. Hoy el catálogo <b>no
    /// tiene ningún modelo delta</b>, así que este caso solo se alcanza capturando a mano.</item>
    /// </list>
    /// </summary>
    public static decimal TensionFaseNeutro(decimal tensionFaseFaseV, SistemaTablero sistema) => De(sistema) switch
    {
        ConfiguracionTablero.UnaFaseDosHilos => tensionFaseFaseV,
        ConfiguracionTablero.UnaFaseTresHilos => tensionFaseFaseV / 2m,
        ConfiguracionTablero.DosFasesDeEstrella or ConfiguracionTablero.TresFasesCuatroHilos =>
            SistemaTrifasico.TensionFaseDesdeLinea(tensionFaseFaseV, ConexionSistema.Estrella),
        _ => tensionFaseFaseV,
    };

    /// <summary>
    /// Cuántos polos puede tener como máximo un interruptor de este tablero: tantos como barras.
    /// Es también <b>los polos del interruptor principal</b>, que corta todas.
    /// </summary>
    public static int MaximoPolos(SistemaTablero sistema) => Barras(sistema).Count;

    /// <summary>¿El sistema lleva neutro? Solo el delta no lo lleva.</summary>
    public static bool TieneNeutro(SistemaTablero sistema) =>
        De(sistema) != ConfiguracionTablero.TresFasesTresHilos;
}
