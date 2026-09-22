namespace PowerNode.DesignSuite.Calculo.Tableros;

/// <summary>Qué protege a un tablero, para poder decirlo en el aviso.</summary>
public enum OrigenProteccionTablero
{
    /// <summary>Su propio interruptor principal, esté entre los derivados o en alojamiento propio.</summary>
    InterruptorPropio,

    /// <summary>El dispositivo del tablero que lo alimenta — el caso de las zapatas principales.</summary>
    LadoDeAlimentacion,

    /// <summary>Nada: es de zapatas y no cuelga de nadie.</summary>
    Ninguna,
}

/// <summary>
/// <b>408-36: todo tablero tiene que estar protegido.</b>
///
/// <para>
/// Texto de la norma, verificado contra el corpus antes de escribir esto:
/// <i>«un panel de alumbrado y control debe estar protegido por un dispositivo de protección contra
/// sobrecorriente que tenga un valor nominal no mayor que la del panel de alumbrado y control. Este
/// dispositivo se debe ubicar dentro o en cualquier punto en el lado de alimentación del panel.»</i>
/// </para>
///
/// <para>
/// <b>Lo que se dictamina es el número</b>: «<i>no mayor que la del panel</i>», o sea el dispositivo
/// que lo protege contra la capacidad de su barra. La otra mitad —«<i>dentro o en el lado de
/// alimentación</i>»— <b>no se puede dictaminar y por eso no se intenta</b>: un tablero raíz suele
/// ser equipo de acometida, cuyo dispositivo vive fuera de lo que este programa modela. Ver el
/// resguardo de <see cref="OrigenProteccionTablero.Ninguna"/> dentro de <see cref="Verificar"/>.
/// </para>
///
/// <para>
/// <b>Zapatas no significa «sin protección».</b> Es la confusión que este archivo existe para
/// evitar: un tablero de main lugs sí está protegido — por el interruptor del tablero que lo
/// alimenta. Lo que no tiene es un dispositivo <i>propio</i>.
/// </para>
///
/// <para>
/// <b>Criterios de campo del usuario (2026-08-20), que NO se programan y por eso quedan escritos
/// aquí.</b> Cuándo se elige uno u otro es decisión del proyectista, no del programa:
/// </para>
///
/// <list type="bullet">
/// <item><b>Zapatas</b> cuando el padre está <i>cerca</i> y su protección le queda adecuada al hijo.</item>
/// <item><b>Interruptor principal</b> cuando el tablero está <i>retirado</i> del padre.</item>
/// <item><b>Interruptor principal</b> también cuando un alimentador grande —digamos 400 A— llega a
/// una caja y de ahí se reparte a tableros más chicos: <b>ese interruptor de 400 A no los
/// protege</b>, así que cada uno necesita el suyo. Es el caso que hace que la regla NO se pueda
/// reducir a «la protección del padre siempre alcanza».</item>
/// </list>
///
/// <para>
/// <b>Ese último caso todavía no se puede modelar</b> — no existe una caja de derivación como
/// elemento de topología, así que hoy cada tablero cuelga directo de su padre y el dispositivo que
/// lo protege es uno solo. Cuando exista, esta verificación es el lugar donde entra.
/// </para>
/// </summary>
public static class Verificacion408_36
{
    /// <summary>El resultado: <c>null</c> en <see cref="Aviso"/> cuando no hay nada que reportar.</summary>
    /// <param name="Origen">Qué lo protege — para el texto del aviso y para las pruebas.</param>
    /// <param name="Aviso">El texto redactado, o <c>null</c> si cumple o si falta el dato.</param>
    public readonly record struct Resultado(OrigenProteccionTablero Origen, string? Aviso);

    /// <summary>
    /// Verifica un tablero.
    /// </summary>
    /// <param name="capacidadBarraA">
    /// La capacidad nominal del tablero. <b><c>null</c> no dispara ningún aviso</b>: es un dato que
    /// puede no estar capturado, y declarar un incumplimiento por no tener el dato es el error
    /// contrario al que esta clase evita. Mismo criterio que el resto del motor.
    /// </param>
    /// <param name="proteccionA">
    /// El dispositivo que lo protege, en amperes — el mismo número esté montado en este tablero o en
    /// el padre, porque en los dos casos es <b>uno</b> el que protege al alimentador y al tablero.
    /// </param>
    /// <param name="tieneInterruptorPrincipal">Si el tablero trae dispositivo propio.</param>
    /// <param name="tieneAlimentadorEntrante">Si cuelga de algo — un tablero, un transformador, una acometida.</param>
    public static Resultado Verificar(
        decimal? capacidadBarraA,
        decimal proteccionA,
        bool tieneInterruptorPrincipal,
        bool tieneAlimentadorEntrante)
    {
        var origen = tieneInterruptorPrincipal
            ? OrigenProteccionTablero.InterruptorPropio
            : tieneAlimentadorEntrante
                ? OrigenProteccionTablero.LadoDeAlimentacion
                : OrigenProteccionTablero.Ninguna;

        // UN TABLERO DE ZAPATAS SIN PADRE NO SE DICTAMINA, y esto merece explicación porque la
        // primera versión sí lo reportaba -- y lo cachó su propia prueba de punta a punta, no la
        // pantalla.
        //
        // La tentación es leer "no cuelga de nadie" como "nadie lo protege". No lo es: un tablero
        // raíz suele ser equipo de acometida, y ahí el dispositivo que lo cubre está del lado de la
        // compañía suministradora o en un medio de desconexión que este programa NO modela. Zapatas
        // principales en equipo de acometida es una instalación legítima y común.
        //
        // Así que reportarlo sería declarar un incumplimiento por un dato que no existe en el
        // modelo, que es exactamente el error que el resguardo de la capacidad de barra evita tres
        // renglones más abajo. Un elemento sin alimentador ya se ve como tal en la topología; no
        // hace falta que la norma opine de eso.
        if (origen == OrigenProteccionTablero.Ninguna)
            return new Resultado(origen, null);

        if (capacidadBarraA is not { } capacidad || capacidad <= 0m || proteccionA <= 0m)
            return new Resultado(origen, null);

        if (proteccionA <= capacidad)
            return new Resultado(origen, null);

        // La otra mitad: "de valor nominal no mayor que la del panel".
        return origen == OrigenProteccionTablero.InterruptorPropio
            ? new Resultado(origen,
                $"Su interruptor principal es de {Amperes(proteccionA)} y la barra del tablero es de " +
                $"{Amperes(capacidad)}. El 408-36 no admite protegerlo con un dispositivo mayor que su " +
                "propia capacidad: usa un tablero de más capacidad o reparte la carga.")
            : new Resultado(origen,
                $"Es de zapatas principales, así que lo protege el dispositivo de su lado de alimentación, " +
                $"y ése es de {Amperes(proteccionA)} contra una barra de {Amperes(capacidad)}. Ese " +
                "interruptor no lo protege: ponle interruptor principal a este tablero, o usa uno de más " +
                "capacidad.");
    }

    private static string Amperes(decimal valor) =>
        $"{valor.ToString(valor == decimal.Truncate(valor) ? "0" : "0.##")} A";
}
