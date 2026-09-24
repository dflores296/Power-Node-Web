using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>El tipo de carga del circuito, como lo agrupa el Art. 220 para el factor de demanda</b> — R-17
/// (David, 2026-09-24). Los factores van por tipo, no por continua / no continua: la tabla de
/// alumbrado (220-42) no es la de contactos (220-44) ni la de aparatos (220-53 a 220-56).
///
/// <para>
/// <b>Los cinco admiten factor de demanda, con justificación</b> — R-18. Motores y A/C (220-50) y
/// calefacción fija (220-51) se calculan al 100 % como regla general, pero 430-26 y la Excepción de
/// 220-51 permiten menos cuando no funcionan todos a la vez o trabajan por ciclos. Hasta el
/// 2026-09-24 esos dos quedaban fijos en 1.00: era más estricto que la norma.
/// </para>
///
/// <para>
/// Para el cálculo del circuito, <see cref="MotorOAireAcondicionado"/> y <see cref="CalefaccionFija"/>
/// son <see cref="TipoCarga.Equipo"/>: carga de placa. El cálculo propio de motores (Art. 430) está
/// pendiente — I-15.
/// </para>
/// </summary>
public enum CategoriaDeCarga
{
    Alumbrado,
    Contactos,
    Equipo,
    MotorOAireAcondicionado,
    CalefaccionFija,
}

public static class CategoriasDeCarga
{
    /// <summary>Todos los tipos: cada uno lleva su factor de demanda, 1.0 por omisión.</summary>
    public static readonly IReadOnlyList<CategoriaDeCarga> Todas = Enum.GetValues<CategoriaDeCarga>();

    /// <summary>Para el selector del renglón, que mide 108 px.</summary>
    public static string Nombre(this CategoriaDeCarga c) => c switch
    {
        CategoriaDeCarga.Alumbrado => "Alumbrado",
        CategoriaDeCarga.Contactos => "Contactos",
        CategoriaDeCarga.Equipo => "Equipo",
        CategoriaDeCarga.MotorOAireAcondicionado => "Motor / A/C",
        _ => "Calefacción",
    };

    /// <summary>Para el resumen, el documento y la memoria.</summary>
    public static string NombreCompleto(this CategoriaDeCarga c) => c switch
    {
        CategoriaDeCarga.Equipo => "Equipo (aparatos)",
        CategoriaDeCarga.MotorOAireAcondicionado => "Motores y aire acondicionado",
        CategoriaDeCarga.CalefaccionFija => "Calefacción eléctrica fija",
        _ => c.Nombre(),
    };

    /// <summary>Qué va en cada tipo, con ejemplos. Es el tooltip del selector.</summary>
    public static string Descripcion(this CategoriaDeCarga c) => c switch
    {
        CategoriaDeCarga.Alumbrado => "Alumbrado: luminarias y alumbrado general — Tabla 220-42.",
        CategoriaDeCarga.Contactos => "Contactos: contactos de uso general. En vivienda, seleccionar el uso (cocina, lavadora, baño) — 210-11(c).",
        CategoriaDeCarga.Equipo => "Equipo: aparatos que no son motor ni calefacción de ambiente: hornos, estufas, parrillas, secadoras, calentadores de agua, equipo electrónico — 220-53 a 220-56.",
        CategoriaDeCarga.MotorOAireAcondicionado => "Motor / A/C: todo lo que funciona con motor o compresor: aire acondicionado, refrigeración, bombas, ventiladores, bombas de calor e inverter frío/calor — 220-50, Art. 430 y 440.",
        _ => "Calefacción: calefacción por resistencia eléctrica: calefactores, cables calefactores, calderas eléctricas. Carga continua — 424-3(b); 220-51.",
    };

    /// <summary>El tipo con el que calcula el motor: los tres de equipo son carga de placa.</summary>
    public static TipoCarga TipoDelMotor(this CategoriaDeCarga c) => c switch
    {
        CategoriaDeCarga.Alumbrado => TipoCarga.Alumbrado,
        CategoriaDeCarga.Contactos => TipoCarga.Contactos,
        _ => TipoCarga.Equipo,
    };

    /// <summary>Las justificaciones que le aplican a cada tipo.</summary>
    public static IReadOnlyList<JustificacionFactorDemanda> JustificacionesPosibles(this CategoriaDeCarga c)
    {
        // Motores y calefacción: solo con la condición de su propia sección, o si no coinciden.
        if (c == CategoriaDeCarga.MotorOAireAcondicionado)
            return [JustificacionFactorDemanda.MotoresNoSimultaneos, JustificacionFactorDemanda.CargasNoCoincidentes, JustificacionFactorDemanda.Otra];
        if (c == CategoriaDeCarga.CalefaccionFija)
            return [JustificacionFactorDemanda.CalefaccionPorCiclos, JustificacionFactorDemanda.CargasNoCoincidentes, JustificacionFactorDemanda.Otra];

        IEnumerable<JustificacionFactorDemanda> propias = c switch
        {
            CategoriaDeCarga.Alumbrado => [JustificacionFactorDemanda.AlumbradoGeneral],
            // 220-44: los contactos de inmuebles que no son vivienda van «sujetos a los factores de
            // demanda de la Tabla 220-42 o la Tabla 220-44».
            CategoriaDeCarga.Contactos => [JustificacionFactorDemanda.AlumbradoGeneral, JustificacionFactorDemanda.ContactosNoVivienda],
            _ =>
            [
                JustificacionFactorDemanda.AparatosFijosVivienda, JustificacionFactorDemanda.SecadorasVivienda,
                JustificacionFactorDemanda.EstufasVivienda, JustificacionFactorDemanda.CocinaComercial,
                JustificacionFactorDemanda.CargasNoCoincidentes,
            ],
        };

        return
        [
            .. propias,
            JustificacionFactorDemanda.ViviendaMetodoOpcional, JustificacionFactorDemanda.ViviendaExistente,
            JustificacionFactorDemanda.ViviendaMultifamiliar, JustificacionFactorDemanda.Escuelas,
            JustificacionFactorDemanda.DemandaMaximaMedida, JustificacionFactorDemanda.RestauranteNuevo,
            JustificacionFactorDemanda.Otra,
        ];
    }
}
