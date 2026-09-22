using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Validaciones;

/// <summary>Un aviso de la norma sobre el transformador: qué está fuera de lo permitido y por qué.</summary>
public sealed record AdvertenciaTransformador(string Referencia, string Mensaje);

/// <summary>
/// Las condiciones generales de servicio de la <b>NMX-J-116</b> que el programa puede verificar
/// contra los datos que ya captura. Son avisos, no bloqueos: describen cuándo el transformador
/// queda fuera de sus condiciones normalizadas de operación, que es una decisión del ingeniero.
///
/// Nada de esto sale de la NOM-001-SEDE-2012 — es la norma del transformador.
/// </summary>
public static class ValidacionesTransformador
{
    // ---------------------------------------------------------------- clasificación por instalación

    /// <summary>Capacidades en que se fabrica cada tipo, del punto "En función de su instalación".</summary>
    public static (decimal Min, decimal Max)? RangoDePoste(int numeroFases) => numeroFases switch
    {
        1 => (5m, 167m),      // tipo poste monofásico de 5 kVA a 167 kVA
        3 => (15m, 150m),     // tipo poste trifásico de 15 kVA a 150 kVA
        _ => null,
    };

    /// <summary>
    /// Un transformador tipo poste solo existe dentro de su rango de capacidad; arriba de ahí la
    /// norma lo clasifica como tipo subestación. Al revés no hay problema: nada impide meter un
    /// transformador chico en una subestación.
    /// </summary>
    public static AdvertenciaTransformador? RevisarInstalacion(
        decimal capacidadKva, int numeroFases, TipoInstalacionTransformador instalacion)
    {
        if (instalacion != TipoInstalacionTransformador.Poste)
            return null;

        if (RangoDePoste(numeroFases) is not { } rango)
            return null;

        if (capacidadKva >= rango.Min && capacidadKva <= rango.Max)
            return null;

        var sistema = numeroFases == 1 ? "monofásico" : "trifásico";
        return new AdvertenciaTransformador("NMX-J-116, clasificación por instalación",
            $"Un transformador tipo poste {sistema} se fabrica de {rango.Min:N0} a {rango.Max:N0} kVA, y este " +
            $"es de {capacidadKva:N1} kVA. Arriba de ese rango la norma lo clasifica como tipo subestación.");
    }

    // ---------------------------------------------------------------- ambiente (5.1.2)

    /// <summary>
    /// Límites de temperatura ambiente del 5.1.2: máxima instantánea y promedio en 24 h. El
    /// transformador tipo costa tolera más — 50 °C de máxima y 40 °C de promedio, contra 40 y 30.
    /// </summary>
    public static (decimal Maxima, decimal Promedio24h) LimitesDeAmbiente(bool esTipoCosta) =>
        esTipoCosta ? (50m, 40m) : (40m, 30m);

    /// <summary>
    /// ¿El sitio está dentro de las condiciones de servicio del transformador? Compara la
    /// temperatura promedio de 24 h capturada del proyecto contra el límite del 5.1.2.
    ///
    /// Ojo: esto es distinto del derrateo por altitud (Tabla 1). Aquí se pregunta si el
    /// transformador es apto para el clima del sitio; allá, cuánta capacidad entrega a esa altura.
    /// </summary>
    public static AdvertenciaTransformador? RevisarAmbiente(decimal temperaturaPromedio24hC, bool esTipoCosta)
    {
        var (maxima, promedio) = LimitesDeAmbiente(esTipoCosta);

        if (temperaturaPromedio24hC <= promedio)
            return null;

        var sugerencia = esTipoCosta
            ? "Ya es tipo costa, así que hay que revisar la selección con el fabricante."
            : "Un transformador tipo costa tolera 40 °C de promedio y 50 °C de máxima.";

        return new AdvertenciaTransformador("NMX-J-116, 5.1.2",
            $"El sitio promedia {temperaturaPromedio24hC:N1} °C en 24 h y este transformador está normalizado " +
            $"para {promedio:N0} °C de promedio ({maxima:N0} °C de máxima). {sugerencia}");
    }

    // ---------------------------------------------------------------- tensión de operación (5.1.5)

    /// <summary>Margen sobre la tensión nominal del secundario: 5 % con carga, 10 % en vacío.</summary>
    public const decimal MargenConCargaPct = 5m;
    public const decimal MargenEnVacioPct = 10m;

    /// <summary>
    /// El 5.1.5 dice que el transformador debe operar correctamente con 5 % arriba de la tensión
    /// nominal del lado de baja a capacidad nominal (cuando el factor de potencia es 80 % o mayor),
    /// y con 10 % arriba en vacío. Si el tablero que cuelga del secundario declara una tensión más
    /// alta que eso, la combinación está fuera de lo normalizado.
    /// </summary>
    public static AdvertenciaTransformador? RevisarTensionDeOperacion(
        decimal tensionNominalSecundariaV, decimal tensionOperacionV, bool enVacio = false)
    {
        if (tensionNominalSecundariaV <= 0)
            return null;

        var margen = enVacio ? MargenEnVacioPct : MargenConCargaPct;
        var techo = tensionNominalSecundariaV * (1m + margen / 100m);

        if (tensionOperacionV <= techo)
            return null;

        var exceso = (tensionOperacionV / tensionNominalSecundariaV - 1m) * 100m;
        return new AdvertenciaTransformador("NMX-J-116, 5.1.5",
            $"La tensión de operación ({tensionOperacionV:N0} V) queda {exceso:N1} % arriba de la nominal del " +
            $"secundario ({tensionNominalSecundariaV:N0} V). La norma admite hasta {margen:N0} % " +
            $"{(enVacio ? "en vacío" : "a capacidad nominal con factor de potencia de 80 % o mayor")}, " +
            $"es decir {techo:N0} V.");
    }

    /// <summary>Todas las revisiones de una vez, para que la cascada las junte en un solo lugar.</summary>
    public static IReadOnlyList<AdvertenciaTransformador> Revisar(
        decimal capacidadKva,
        int numeroFases,
        TipoInstalacionTransformador instalacion,
        bool esTipoCosta,
        decimal temperaturaPromedio24hC,
        decimal tensionNominalSecundariaV,
        decimal? tensionOperacionV)
    {
        var avisos = new List<AdvertenciaTransformador>();

        if (RevisarInstalacion(capacidadKva, numeroFases, instalacion) is { } a) avisos.Add(a);
        if (RevisarAmbiente(temperaturaPromedio24hC, esTipoCosta) is { } b) avisos.Add(b);
        if (tensionOperacionV is { } v && RevisarTensionDeOperacion(tensionNominalSecundariaV, v) is { } c)
            avisos.Add(c);

        return avisos;
    }
}
