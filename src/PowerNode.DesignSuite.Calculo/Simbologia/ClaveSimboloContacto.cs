using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Simbologia;

/// <summary>
/// Qué símbolo le toca a un contacto. Cruza sus dos ejes —<see cref="TipoContacto"/>, que es la
/// marca y sus requisitos de instalación, y <see cref="ConfiguracionContacto"/>, que es cuántas
/// salidas tiene— y devuelve la clave lógica del diccionario de simbología.
///
/// **Vive aquí y no en la interfaz a propósito.** Antes esta decisión estaba escrita como
/// DataTriggers sueltos dentro de dos XAML distintos, así que no se podía probar y se podía
/// desincronizar sola. Aquí es una función pura, cubierta por pruebas, y la interfaz solo consume el
/// resultado.
///
/// Devuelve la clave, nunca un dibujo: qué se traza para cada clave lo decide el estándar de
/// simbología que el proyecto tenga configurado (ANSI o IEC), y eso se resuelve en la capa de UI.
/// </summary>
public static class ClaveSimboloContacto
{
    /// <summary>Lo que se dibuja cuando el circuito es de Contactos pero no se capturó ningún detalle.</summary>
    public const string Predeterminada = "Contacto";

    /// <summary>
    /// La clave del símbolo para un contacto.
    ///
    /// Null en cualquiera de los dos ejes se lee como el valor por omisión —<see
    /// cref="TipoContacto.Normal"/> y <see cref="ConfiguracionContacto.Duplex"/>—, que es lo que el
    /// programa dibujaba antes de que estos ejes existieran. Así ningún proyecto ya capturado cambia
    /// de dibujo por haber agregado el campo.
    /// </summary>
    /// <summary>El montaje de piso — NMX 4.2.32 (RECP).</summary>
    public const string Piso = "ContactoPiso";

    /// <summary>El montaje a la intemperie — NMX 4.2.37 (RECPI). Su dibujo lleva la leyenda GFCI.</summary>
    public const string Intemperie = "ContactoIntemperie";

    /// <summary>
    /// La clave del símbolo cruzando los <b>tres</b> ejes: tipo, configuración y montaje.
    ///
    /// <para>
    /// <b>El MONTAJE manda sobre los otros dos</b>, y no es una preferencia: la NMX publica <b>una
    /// sola figura</b> para piso y una para intemperie. No existe dibujado un «GFCI de piso dúplex»,
    /// así que en cuanto el montaje deja de ser pared, el dibujo es el del montaje. Cuando eso hace
    /// perder información capturada —un GFCI de piso, que se dibujaría sin su marca— lo dice
    /// <see cref="TieneSimboloPropio"/>, para que la interfaz avise en vez de callarlo.
    /// </para>
    /// </summary>
    public static string Para(TipoContacto? tipo, ConfiguracionContacto? configuracion, MontajeContacto? montaje)
        => (montaje ?? MontajeContacto.Pared) switch
        {
            MontajeContacto.Piso => Piso,
            MontajeContacto.Intemperie => Intemperie,
            _ => Para(tipo, configuracion),
        };

    public static string Para(TipoContacto? tipo, ConfiguracionContacto? configuracion)
    {
        var t = tipo ?? TipoContacto.Normal;
        var c = configuracion ?? ConfiguracionContacto.Duplex;

        // Especial no tiene símbolo propio: es la salida de escape para lo que no encaja en las
        // otras tres, y se dibuja como un contacto normal de su misma configuración.
        if (t == TipoContacto.Especial)
            t = TipoContacto.Normal;

        return (t, c) switch
        {
            (TipoContacto.Normal, ConfiguracionContacto.Sencillo) => "ContactoSencillo",
            (TipoContacto.Normal, ConfiguracionContacto.Duplex) => "Contacto",
            (TipoContacto.Normal, ConfiguracionContacto.Trifasico) => "ContactoTrifasico",

            (TipoContacto.Gfci, ConfiguracionContacto.Sencillo) => "ContactoGfciSencillo",
            (TipoContacto.Gfci, ConfiguracionContacto.Duplex) => "ContactoGfci",

            (TipoContacto.TierraAislada, ConfiguracionContacto.Sencillo) => "ContactoTierraAisladaSencillo",
            (TipoContacto.TierraAislada, ConfiguracionContacto.Duplex) => "ContactoTierraAislada",

            // GFCI trifásico y tierra aislada trifásico NO tienen símbolo dibujado: la lámina de
            // referencia del usuario no los trae y no se inventaron. Se cae a la variante dúplex de
            // la MARCA, que es el dato que no se puede perder —un GFCI mal rotulado es un problema
            // de seguridad—, en vez de a la variante trifásica sin marca.
            (TipoContacto.Gfci, _) => "ContactoGfci",
            (TipoContacto.TierraAislada, _) => "ContactoTierraAislada",

            _ => Predeterminada,
        };
    }

    /// <summary>
    /// Si la combinación tiene símbolo propio o se está cayendo a otro. La interfaz lo usa para
    /// avisar en vez de dibujar algo distinto en silencio, que es el error que este trabajo
    /// persigue: un símbolo sustituido sin aviso se lee como un hecho.
    /// </summary>
    /// <summary>
    /// Si el dibujo representa <b>todo</b> lo capturado, contando el montaje.
    ///
    /// <para>
    /// Las figuras de piso e intemperie <b>no distinguen</b> sencillo de dúplex ni marcan tierra
    /// aislada, así que capturar esos datos en un contacto de piso significa que el plano no los va
    /// a decir. La interfaz lo advierte: un símbolo que se queda corto sin avisar se lee como un
    /// hecho.
    /// </para>
    ///
    /// <para>
    /// <b>La intemperie tiene su propio matiz.</b> Su figura ya lleva la leyenda <c>GFCI</c>, así que
    /// un contacto de intemperie capturado como GFCI está perfectamente representado — y uno que NO
    /// se capturó como GFCI se dibujaría con una marca que su captura no respalda. Eso también se
    /// advierte, y de paso empuja hacia el 210-8, que es lo que la instalación va a pedir.
    /// </para>
    /// </summary>
    public static bool TieneSimboloPropio(
        TipoContacto? tipo, ConfiguracionContacto? configuracion, MontajeContacto? montaje)
    {
        var m = montaje ?? MontajeContacto.Pared;
        if (m == MontajeContacto.Pared)
            return TieneSimboloPropio(tipo, configuracion);

        var t = tipo ?? TipoContacto.Normal;
        var c = configuracion ?? ConfiguracionContacto.Duplex;

        // La figura del montaje no dice cuántas salidas tiene.
        if (c != ConfiguracionContacto.Duplex)
            return false;

        return m == MontajeContacto.Intemperie
            ? t == TipoContacto.Gfci                              // su figura YA marca GFCI
            : t is TipoContacto.Normal or TipoContacto.Especial;  // la de piso no marca nada
    }

    public static bool TieneSimboloPropio(TipoContacto? tipo, ConfiguracionContacto? configuracion)
    {
        var t = tipo ?? TipoContacto.Normal;
        var c = configuracion ?? ConfiguracionContacto.Duplex;
        return c != ConfiguracionContacto.Trifasico
               || t is TipoContacto.Normal or TipoContacto.Especial;
    }
}
