using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Validaciones;

/// <summary>
/// Los límites del <b>CCM Modelo 6</b> del compendiado Schneider (capítulo 02, página 2/15), y las
/// opciones cuantificadas de su Formulario CCM (2/16).
///
/// <b>Nada de esto sale de la NOM-001-SEDE-2012</b> — es la ficha de un producto. Vive aparte de
/// <see cref="ValidacionesCcm"/> (que es Parte H del Art. 430, y aplica a cualquier CCM) por la misma
/// razón que <see cref="ValidacionesTransformador"/> vive aparte de las calculadoras del Art. 450:
/// mezclar norma con catálogo hace que la memoria de cálculo cite mal. Por eso corre solo cuando el
/// CCM declara <see cref="LineaCcm.Modelo6"/>.
///
/// <b>Por qué no hay catálogo de modelos.</b> Un CCM no es producto de línea: se manda hacer. El
/// compendiado no publica una tabla de números de catálogo como la de los tableros NQ/NF o la de los
/// transformadores secos — publica un formulario para pedirlo. Así que lo aprovechable de esas dos
/// páginas son rangos y opciones, que es justo lo que hay aquí.
/// </summary>
public static class EspecificacionCcmModelo6
{
    /// <summary>"Tensión nominal de operación máxima: 600 Vc.a." (2/15).</summary>
    public const decimal TensionMaximaV = 600m;

    /// <summary>"Para corrientes de aplicación de hasta 2500 amperes" (2/15).</summary>
    public const decimal CorrienteMaximaBarrasA = 2500m;

    /// <summary>"Interruptores derivados 15 hasta 1200 Amp." (2/15).</summary>
    public const decimal InterruptorDerivadoMinA = 15m;
    public const decimal InterruptorDerivadoMaxA = 1200m;

    /// <summary>
    /// Capacidades interruptivas que ofrece el Formulario CCM en 220 V (2/16). En 480 V el formulario
    /// solo dice "mayor a 25 kA", sin enumerar, así que <b>no se inventa una lista para esa tensión</b>.
    /// Sirve para poblar el selector de la UI y para avisar cuando se captura un valor fuera de línea.
    /// </summary>
    public static readonly decimal[] CapacidadesInterruptivasKa220 = [22m, 25m, 35m, 42m];

    /// <summary>
    /// Rango de potencia por tipo de arrancador, de la lista de 2/15. <c>null</c> = el compendiado no
    /// lo cubre, y entonces no se opina.
    ///
    /// <b>Ojo, la página se contradice sola.</b> Esta lista dice "tensión plena desde 0.5 HP hasta
    /// 400 HP", "estado sólido desde 1 HP hasta 600 HP" y "velocidad variable desde 1 HP hasta
    /// 500 HP"; pero unos renglones abajo, bajo "Características", dice "Potencias desde 1.5 HP a
    /// 400 HP en 220 Vc.a. y desde 1 HP a 500 HP en 460 Vc.a.", que no cuadra ni con el 0.5 HP ni con
    /// los 600 HP. Se implementaron los rangos <b>por tipo de arrancador</b> porque son los
    /// específicos, y la discrepancia quedó anotada en docs/cerrado/07-bitacora.md §3.6 en vez de
    /// resolverse a ciegas — es el mismo criterio que se usó con los porcentajes de altitud.
    /// </summary>
    public static (decimal Min, decimal Max)? RangoHp(TipoArranqueMotor arranque) => arranque switch
    {
        TipoArranqueMotor.TensionPlena => (0.5m, 400m),
        TipoArranqueMotor.Suave => (1m, 600m),                 // "arrancadores de estado sólido"
        TipoArranqueMotor.VariadorDeFrecuencia => (1m, 500m),

        // El compendiado no menciona estrella-delta en el Modelo 6. No opinar es lo correcto:
        // inventarle un rango sería exactamente el error de los porcentajes de altitud.
        _ => null,
    };

    // ---------------------------------------------------------------- revisiones

    /// <summary>Tensión de las barras contra la nominal máxima de operación del producto.</summary>
    public static AdvertenciaCcm? RevisarTension(string nombre, decimal tensionV)
    {
        if (tensionV <= TensionMaximaV)
            return null;

        return new AdvertenciaCcm("Compendiado Schneider 2/15",
            $"'{nombre}' está declarado a {tensionV:N0} V y el CCM Modelo 6 tiene tensión nominal de operación " +
            $"máxima de {TensionMaximaV:N0} V c.a. Ese producto no aplica a esta tensión.");
    }

    /// <summary>Valor nominal de la barra común contra la corriente de aplicación máxima del producto.</summary>
    public static AdvertenciaCcm? RevisarBarras(string nombre, decimal corrienteBarrasA)
    {
        if (corrienteBarrasA <= 0 || corrienteBarrasA <= CorrienteMaximaBarrasA)
            return null;

        return new AdvertenciaCcm("Compendiado Schneider 2/15",
            $"La barra común de '{nombre}' se declaró en {corrienteBarrasA:N0} A y el CCM Modelo 6 llega hasta " +
            $"{CorrienteMaximaBarrasA:N0} A de corriente de aplicación.");
    }

    /// <summary>
    /// La protección de una unidad de control contra el rango de interruptores derivados que ofrece
    /// el producto. Se le pasa la protección ya calculada por <c>CalculoCarga</c>, que es la que de
    /// verdad se va a instalar en la unidad.
    /// </summary>
    public static AdvertenciaCcm? RevisarInterruptorDerivado(string nombreCcm, string nombreUnidad, decimal proteccionA)
    {
        if (proteccionA <= 0 || (proteccionA >= InterruptorDerivadoMinA && proteccionA <= InterruptorDerivadoMaxA))
            return null;

        var lado = proteccionA < InterruptorDerivadoMinA ? "abajo" : "arriba";
        return new AdvertenciaCcm("Compendiado Schneider 2/15",
            $"La unidad '{nombreUnidad}' de '{nombreCcm}' pide una protección de {proteccionA:N0} A, que queda " +
            $"{lado} del rango de interruptores derivados del CCM Modelo 6 ({InterruptorDerivadoMinA:N0} a " +
            $"{InterruptorDerivadoMaxA:N0} A).");
    }

    /// <summary>Potencia de la unidad contra el rango de su tipo de arrancador.</summary>
    public static AdvertenciaCcm? RevisarPotenciaDeArrancador(
        string nombreCcm, string nombreUnidad, decimal hp, TipoArranqueMotor arranque)
    {
        if (hp <= 0 || RangoHp(arranque) is not { } rango)
            return null;

        if (hp >= rango.Min && hp <= rango.Max)
            return null;

        return new AdvertenciaCcm("Compendiado Schneider 2/15",
            $"La unidad '{nombreUnidad}' de '{nombreCcm}' es de {hp:N1} HP con arrancador {arranque}, y el CCM " +
            $"Modelo 6 lo ofrece de {rango.Min:N1} a {rango.Max:N0} HP.");
    }

    /// <summary>
    /// Capacidad interruptiva contra las que enumera el Formulario CCM. <b>Solo se revisa en 220 V</b>,
    /// porque en 480 V el formulario no enumera (dice "mayor a 25 kA") y no hay lista contra la cual
    /// comparar. Es un aviso de "no es de línea", no de "está mal": un valor distinto se puede
    /// especificar, solo deja de ser estándar.
    /// </summary>
    public static AdvertenciaCcm? RevisarCapacidadInterruptiva(string nombre, decimal tensionV, decimal? capacidadKa)
    {
        if (capacidadKa is not { } ka || ka <= 0 || tensionV != 220m)
            return null;

        if (CapacidadesInterruptivasKa220.Contains(ka))
            return null;

        return new AdvertenciaCcm("Compendiado Schneider 2/16",
            $"'{nombre}' declara {ka:N0} kA y el Formulario CCM ofrece en 220 V " +
            $"{string.Join(", ", CapacidadesInterruptivasKa220.Select(k => $"{k:N0}"))} kA. " +
            "Se puede especificar otro valor, pero deja de ser de línea.");
    }

    /// <summary>
    /// Todo lo del producto de una vez. Las unidades llegan ya con su protección calculada y sus
    /// datos de placa, porque son las dos cosas que hay que contrastar contra la ficha.
    /// </summary>
    public static IReadOnlyList<AdvertenciaCcm> Revisar(
        string nombre,
        decimal tensionV,
        decimal corrienteBarrasA,
        decimal? capacidadInterruptivaKa,
        IEnumerable<UnidadParaRevision> unidades)
    {
        var avisos = new List<AdvertenciaCcm>();

        if (RevisarTension(nombre, tensionV) is { } a) avisos.Add(a);
        if (RevisarBarras(nombre, corrienteBarrasA) is { } b) avisos.Add(b);
        if (RevisarCapacidadInterruptiva(nombre, tensionV, capacidadInterruptivaKa) is { } c) avisos.Add(c);

        foreach (var u in unidades)
        {
            if (RevisarInterruptorDerivado(nombre, u.Nombre, u.ProteccionA) is { } d) avisos.Add(d);
            if (RevisarPotenciaDeArrancador(nombre, u.Nombre, u.Hp, u.Arranque) is { } e) avisos.Add(e);
        }

        return avisos;
    }

    /// <summary>Lo que hace falta saber de una unidad de control para contrastarla contra la ficha del producto.</summary>
    public readonly record struct UnidadParaRevision(string Nombre, decimal ProteccionA, decimal Hp, TipoArranqueMotor Arranque);
}
