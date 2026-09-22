using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Validaciones;

/// <summary>Un aviso de la Parte H sobre un centro de control de motores: qué está fuera y por qué.</summary>
public sealed record AdvertenciaCcm(string Referencia, string Mensaje);

/// <summary>Veredicto de comparar la protección del CCM contra el valor nominal de su barra común (430-94).</summary>
public enum VeredictoBarrasCcm
{
    /// <summary>No se capturó el valor nominal de la barra. No se puede opinar, y no se opina.</summary>
    NoEvaluable,

    /// <summary>La protección cabe: no excede el valor nominal de la barra común.</summary>
    Cabe,

    /// <summary>La protección excede el valor nominal de la barra. Es lo que 430-94 prohíbe.</summary>
    Excede
}

/// <summary>
/// Lo que el Artículo 430, <b>Parte H</b> ("Centros de control de motores") agrega de verificable
/// sobre lo que ya calcula la cascada. Son dos reglas, y las dos salen del texto literal de la NOM
/// (verificado contra el corpus, no de memoria):
///
/// <list type="bullet">
/// <item><b>430-94</b> — <i>"El valor nominal en amperes o el ajuste del dispositivo de protección
/// contra sobrecorriente no debe exceder el valor nominal de la barra conductora común de
/// potencia."</i></item>
/// <item><b>430-95</b> — <i>"Si se utiliza como equipo de acometida, cada centro de control de
/// motores debe estar equipado de un solo medio principal de desconexión que desconecte todos los
/// conductores de fase de acometida."</i></item>
/// </list>
///
/// Todo lo demás de la Parte H es constructivo y no calculable desde este modelo: 430-96 (unión de
/// secciones con conductor de puesta a tierra por Tabla 250-122), 430-97 (soporte de barras,
/// disposición A‑B‑C, separaciones de la Tabla 430-97, barreras) y 430-98 (marcado).
/// </summary>
public static class ValidacionesCcm
{
    /// <summary>
    /// 430-94: la protección no puede exceder el valor nominal de la barra conductora común. Nota
    /// que la regla es sobre <b>el dispositivo</b> (su valor nominal o su ajuste), no sobre la
    /// corriente de diseño — por eso se compara la protección ya redondeada al estándar de 240-6(a),
    /// que es justo donde el redondeo hacia arriba puede sacarla del rango de la barra.
    ///
    /// <paramref name="corrienteBarrasA"/> en 0 significa "no capturado" y devuelve
    /// <see cref="VeredictoBarrasCcm.NoEvaluable"/>: no se supone un valor de barra, igual que no se
    /// suponen los kA de un tablero.
    /// </summary>
    public static VeredictoBarrasCcm VerificarBarras(decimal proteccionA, decimal corrienteBarrasA) =>
        corrienteBarrasA <= 0 ? VeredictoBarrasCcm.NoEvaluable
        : proteccionA <= corrienteBarrasA ? VeredictoBarrasCcm.Cabe
        : VeredictoBarrasCcm.Excede;

    /// <summary>430-94, con el aviso ya redactado. Null cuando cabe o cuando no hay con qué evaluar.</summary>
    public static AdvertenciaCcm? RevisarBarras(string nombre, decimal proteccionA, decimal corrienteBarrasA)
    {
        switch (VerificarBarras(proteccionA, corrienteBarrasA))
        {
            case VeredictoBarrasCcm.Excede:
                return new AdvertenciaCcm("430-94",
                    $"La protección de '{nombre}' resultó de {proteccionA:N0} A y la barra conductora común de " +
                    $"potencia es de {corrienteBarrasA:N0} A. El 430-94 no permite que el valor nominal o el " +
                    "ajuste del dispositivo exceda el de la barra: hay que subir la capacidad de barras del " +
                    "centro de control de motores, o repartir la carga en otro.");

            case VeredictoBarrasCcm.NoEvaluable:
                return new AdvertenciaCcm("430-98(a)",
                    $"No se capturó el valor nominal de la barra conductora común de '{nombre}', así que no se " +
                    "pudo verificar el 430-94. El 430-98(a) obliga a que ese valor venga marcado en el equipo.");

            default:
                return null;
        }
    }

    /// <summary>
    /// 430-95: un CCM usado como equipo de acometida necesita <b>su propio</b> medio principal de
    /// desconexión. Si la protección quedó aguas arriba (430-94(1)), el CCM no lo tiene.
    /// </summary>
    public static AdvertenciaCcm? RevisarEquipoAcometida(
        string nombre, bool esEquipoAcometida, UbicacionProteccionCcm ubicacion)
    {
        if (!esEquipoAcometida || ubicacion == UbicacionProteccionCcm.PrincipalInterno)
            return null;

        return new AdvertenciaCcm("430-95",
            $"'{nombre}' está declarado como equipo de acometida, pero su protección se ubicó antes del centro " +
            "de control de motores (430-94(1)). El 430-95 le exige un solo medio principal de desconexión que " +
            "desconecte todos los conductores de fase de acometida, es decir un principal dentro del propio CCM.");
    }

    /// <summary>Todas las revisiones de una vez, para que la cascada las junte en un solo lugar.</summary>
    public static IReadOnlyList<AdvertenciaCcm> Revisar(
        string nombre,
        decimal proteccionA,
        decimal corrienteBarrasA,
        bool esEquipoAcometida,
        UbicacionProteccionCcm ubicacion)
    {
        var avisos = new List<AdvertenciaCcm>();

        if (RevisarBarras(nombre, proteccionA, corrienteBarrasA) is { } a) avisos.Add(a);
        if (RevisarEquipoAcometida(nombre, esEquipoAcometida, ubicacion) is { } b) avisos.Add(b);

        return avisos;
    }
}
