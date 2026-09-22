using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Simbologia;

/// <summary>
/// Qué símbolo le toca a un tablero según su <see cref="TipoTablero"/>.
///
/// Los dibujos son los de la **NMX-J-136-ANCE**, figuras 4.2.90 a 4.2.93, que es la norma mexicana
/// de simbología — la NOM-001-SEDE no define símbolos. Ver
/// <c>Simbologia/Nmx/SimbologiaNmx.xaml</c> para la tabla completa.
///
/// <b>La NMX es de referencia, no obligatoria, y no dice dónde va cada símbolo</b> — sólo da
/// título, abreviatura y dibujo. Así que la correspondencia de abajo es **convención nuestra**,
/// decidida por el usuario el 2026-08-19, y este archivo es el único lugar donde vive.
///
/// Igual que <see cref="ClaveSimboloContacto"/>, esto vive fuera de la interfaz para que se pueda
/// probar: la decisión anterior estaba escrita dentro de un ViewModel y de un estilo XAML, y ahí
/// nadie la podía verificar.
/// </summary>
public static class ClaveSimboloTablero
{
    /// <summary>4.2.91 TDG. El tablero de distribución genérico, y el respaldo de todo lo demás.</summary>
    public const string DistribucionGeneral = "TableroDistribucionGeneral";

    /// <summary>4.2.92 TDA, tablero de distribución de alumbrado.</summary>
    public const string Alumbrado = "TableroAlumbrado";

    /// <summary>
    /// 4.2.90 TEG, tablero eléctrico general — el rectángulo solo.
    ///
    /// <b>Descartado como símbolo de uso</b> el 2026-08-19: un rectángulo vacío no aporta al plano
    /// nada que el genérico no diga mejor. El dibujo se conserva porque la figura existe en la norma
    /// y hay que poder leerla en planos ajenos, pero <see cref="Para"/> nunca lo devuelve.
    /// </summary>
    public const string General = "TableroGeneral";

    /// <summary>4.2.93 TDC, tablero de distribución de control.</summary>
    public const string Control = "TableroControl";

    /// <summary>
    /// Tablero de distribución de fuerza: el genérico 4.2.91 con el recuadro de leyenda
    /// <c>FZA</c> encima.
    ///
    /// <b>No es una figura de la NMX</b> — su tabla llega hasta 4.2.93 y no trae tablero de fuerza.
    /// Es una derivación nuestra, y no es un dibujo inventado: la norma misma especializa el
    /// genérico de dos maneras, rellenando media caja (así nace el 4.2.92 TDA) o colgándole un
    /// recuadro con leyenda (así nace el 4.2.93 TDC). Ésta usa la segunda, con la misma caja y la
    /// misma altura de letra que el TDC.
    ///
    /// Se eligió sobre las otras dos candidatas por dos razones, decididas por el usuario el
    /// 2026-08-19: es la única que **crece** —el siguiente tipo de tablero es una abreviatura nueva
    /// en la misma caja, no un dibujo nuevo que inventar—, y es la única que **no se puede leer
    /// mal**: la alternativa de rellenar el triángulo contrario obliga a distinguir de qué lado
    /// quedó el negro, que es justo lo que se pierde en una fotocopia o de lejos.
    /// </summary>
    public const string Fuerza = "TableroFuerza";

    /// <summary>
    /// La clave del símbolo para un tablero.
    ///
    /// **La correspondencia es convención nuestra, no un dato de la norma.** La NMX clasifica los
    /// tableros por lo que distribuyen; <see cref="TipoTablero"/> los clasifica por lo que se les
    /// cuelga. Son taxonomías parecidas, no iguales, y la norma no dice dónde usar cada figura, así
    /// que la decisión es de diseño. Quedó cerrada por el usuario el 2026-08-19:
    ///
    /// <list type="bullet">
    ///   <item><description><see cref="TipoTablero.CargasGenerales"/> (alumbrado y contactos) → 4.2.92 TDA.
    ///   <b>Revisado y conservado a propósito el 2026-08-19</b>, cuando el usuario señaló la tensión:
    ///   la figura se llama «de alumbrado» y este tablero lleva alumbrado <i>y</i> contactos, así que
    ///   el nombre de la figura dice menos de lo que el tablero realmente distribuye. Se dejó porque
    ///   <b>en la práctica mexicana «tablero de alumbrado» ya significa el de uso general</b> —nadie
    ///   espera que sólo lleve luminarias—, y porque las alternativas costaban más de lo que
    ///   arreglaban: pasarlo al genérico TDG lo dejaría compartiendo dibujo con Mixto (dos tipos, un
    ///   símbolo) y dejaría al TDA sin uso. <b>No lo reabras sin razón nueva:</b> ya se comparó contra
    ///   la norma y contra las dos alternativas.</description></item>
    ///   <item><description><see cref="TipoTablero.Fuerza"/> → el genérico con leyenda <c>FZA</c>. Ver <see cref="Fuerza"/>.</description></item>
    ///   <item><description><see cref="TipoTablero.Control"/> → 4.2.93 TDC.</description></item>
    ///   <item><description><see cref="TipoTablero.Mixto"/> → 4.2.91 TDG, que es el genérico.</description></item>
    /// </list>
    ///
    /// El 4.2.90 TEG (rectángulo solo) quedó descartado y no lo devuelve ningún tipo; ver
    /// <see cref="General"/>. Con eso se disolvió también la duda de TEG contra TDG por posición en
    /// la topología, que había quedado abierta: ya no hay dos figuras peleándose por el mismo
    /// tablero. La posición sigue siendo dato útil para etiquetar y numerar, no para elegir dibujo.
    /// </summary>
    public static string Para(TipoTablero tipo) => tipo switch
    {
        TipoTablero.CargasGenerales => Alumbrado,
        TipoTablero.Fuerza => Fuerza,
        TipoTablero.Control => Control,
        TipoTablero.Mixto => DistribucionGeneral,
        _ => DistribucionGeneral,
    };

    /// <summary>
    /// Si el tipo tiene un símbolo que de verdad le corresponde, o se está usando el genérico a
    /// falta de uno. La interfaz lo usa para avisarlo en vez de sustituir en silencio.
    ///
    /// **Hoy los cuatro tipos tienen el suyo**, así que siempre devuelve true. Antes del 2026-08-19
    /// <see cref="TipoTablero.Fuerza"/> devolvía false porque caía al genérico; ya no, desde que
    /// tiene el símbolo con leyenda <c>FZA</c>. Se conserva el método —en vez de borrarlo— porque el
    /// aviso lo sigue necesitando el siguiente tipo de tablero que aparezca sin figura propia, y
    /// porque quitarlo obligaría a reconstruir el mismo camino en la interfaz.
    /// </summary>
    public static bool TieneSimboloPropio(TipoTablero tipo) => tipo switch
    {
        TipoTablero.CargasGenerales => true,
        TipoTablero.Fuerza => true,
        TipoTablero.Control => true,
        TipoTablero.Mixto => true,
        _ => false,
    };
}
