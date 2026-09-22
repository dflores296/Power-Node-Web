namespace PowerNode.DesignSuite.Domain.Proyectos;

/// <summary>
/// Hace nacer un elemento nuevo con los <see cref="DefaultsDeCaptura"/> de su proyecto.
///
/// <para>
/// <b>Se llama AL CREAR, una sola vez.</b> No es un hook de guardado ni un interceptor de EF, y eso
/// es la decisión de diseño, no un detalle: si los defaults se aplicaran en cada <c>SaveChanges</c>,
/// editar un circuito a un criterio distinto del proyecto sería <b>imposible</b> — el proyecto se lo
/// comería en el siguiente guardado. El nivel del elemento manda; éste solo lo rellena la primera vez.
/// </para>
///
/// <para>
/// <b>Sin configuración capturada no hace nada</b>, y el elemento se queda con los valores por
/// omisión de su clase — que son los mismos números. Un proyecto sin configuración se comporta
/// exactamente como antes de que existiera este nivel.
/// </para>
///
/// <para>
/// <b>Qué NO hace: no toca un elemento existente.</b> Cambiar los defaults de un proyecto a medio
/// capturar afecta a los circuitos que se creen después, no a los que ya están — que es lo que se
/// espera de un valor por omisión y lo contrario de lo que se espera de un límite. Si algún día hace
/// falta "aplicar a todos", que sea una acción explícita del usuario con su confirmación, no un
/// efecto lateral de editar la configuración.
/// </para>
/// </summary>
public static class AplicarDefaults
{
    /// <summary>Rellena un circuito recién creado. Devuelve el mismo circuito, para poder encadenar.</summary>
    public static CircuitoDerivado ANuevoCircuito(CircuitoDerivado circuito, ConfiguracionProyecto? configuracion)
    {
        if (configuracion?.Defaults is not { } d) return circuito;

        circuito.FactorPotencia = d.FactorPotencia;
        circuito.TemperaturaAmbienteC = d.TemperaturaAmbienteC;
        circuito.ConductoresAgrupados = d.ConductoresAgrupados;
        circuito.MaterialConductor = d.MaterialConductor;
        circuito.MaterialCanalizacion = d.MaterialCanalizacion;
        circuito.TipoAislamiento = d.TipoAislamiento;
        circuito.MetodoInstalacion = d.MetodoInstalacion;
        circuito.LugarInstalacionSeco = d.LugarInstalacionSeco;
        circuito.NumeroConductoresParalelo = d.NumeroConductoresParalelo;
        circuito.ConjuntoAprobado100Pct = d.ConjuntoAprobado100Pct;

        return circuito;
    }

    /// <summary>
    /// Rellena un alimentador recién creado. <b>Los factores de demanda no van aquí</b>: no son campo
    /// del alimentador — su carga es el agregado de lo que cuelga aguas abajo, donde cada circuito ya
    /// aplicó el suyo.
    /// </summary>
    public static Alimentador ANuevoAlimentador(Alimentador alimentador, ConfiguracionProyecto? configuracion)
    {
        if (configuracion?.Defaults is not { } d) return alimentador;

        alimentador.FactorPotencia = d.FactorPotencia;
        alimentador.TemperaturaAmbienteC = d.TemperaturaAmbienteC;
        alimentador.ConductoresAgrupados = d.ConductoresAgrupados;
        alimentador.MaterialConductor = d.MaterialConductor;
        alimentador.MaterialCanalizacion = d.MaterialCanalizacion;
        alimentador.TipoAislamiento = d.TipoAislamiento;
        alimentador.MetodoInstalacion = d.MetodoInstalacion;
        alimentador.LugarInstalacionSeco = d.LugarInstalacionSeco;
        alimentador.NumeroConductoresParalelo = d.NumeroConductoresParalelo;
        alimentador.ConjuntoAprobado100Pct = d.ConjuntoAprobado100Pct;

        // El factor de demanda nace aquí desde el 2026-08-20: es del alimentador, no del derivado.
        alimentador.FactorDemandaContinua = d.FactorDemandaContinua;
        alimentador.FactorDemandaNoContinua = d.FactorDemandaNoContinua;

        return alimentador;
    }

    /// <summary>
    /// Rellena un transformador recién creado. <b>Solo la X/R</b>: la capacidad, las tensiones y el %Z
    /// son datos de placa que salen de la ficha del fabricante o del catálogo, no criterios de diseño
    /// que un proyecto pueda predecir.
    /// </summary>
    public static Transformador ANuevoTransformador(Transformador transformador, ConfiguracionProyecto? configuracion)
    {
        if (configuracion?.Defaults is not { } d) return transformador;

        transformador.RelacionXR = d.RelacionXRTransformador;

        // El mismo factor de potencia que heredan circuitos y alimentadores: es el de la carga del
        // proyecto, y no había razón para que el transformador fuera el único con el suyo aparte.
        transformador.FactorPotenciaCarga = d.FactorPotencia;

        return transformador;
    }
}
