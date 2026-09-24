using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>El tipo de carga del circuito, como lo agrupa el Art. 220 para el factor de demanda</b> — R-17
/// (David, 2026-09-24). Los factores van por tipo, no por continua / no continua: la tabla de
/// alumbrado (220-42) no es la de contactos (220-44) ni la de aparatos (220-53 a 220-56), y dos tipos
/// no se reducen nunca: motores y aire acondicionado (220-50, van por 430-24 y 440) y calefacción
/// eléctrica fija (220-51, al 100 %).
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
    /// <summary>Los tipos cuyo factor de demanda captura el proyectista.</summary>
    public static readonly IReadOnlyList<CategoriaDeCarga> Reducibles =
        [CategoriaDeCarga.Alumbrado, CategoriaDeCarga.Contactos, CategoriaDeCarga.Equipo];

    public static bool AdmiteFactorDeDemanda(this CategoriaDeCarga c) => Reducibles.Contains(c);

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

    /// <summary>Por qué no se reduce. <c>null</c> en los tipos que sí admiten factor.</summary>
    public static string? SinReduccion(this CategoriaDeCarga c) => c switch
    {
        CategoriaDeCarga.MotorOAireAcondicionado => "220-50: se calcula con 430-24 y 440, sin factor de demanda",
        CategoriaDeCarga.CalefaccionFija => "220-51: al 100 % de la carga conectada",
        _ => null,
    };

    /// <summary>El tipo con el que calcula el motor: los tres de equipo son carga de placa.</summary>
    public static TipoCarga TipoDelMotor(this CategoriaDeCarga c) => c switch
    {
        CategoriaDeCarga.Alumbrado => TipoCarga.Alumbrado,
        CategoriaDeCarga.Contactos => TipoCarga.Contactos,
        _ => TipoCarga.Equipo,
    };

    /// <summary>Las justificaciones que le aplican a cada tipo. La Parte D (métodos opcionales) y «Otra», a todos.</summary>
    public static IReadOnlyList<JustificacionFactorDemanda> JustificacionesPosibles(this CategoriaDeCarga c)
    {
        IEnumerable<JustificacionFactorDemanda> propias = c switch
        {
            CategoriaDeCarga.Alumbrado => [JustificacionFactorDemanda.AlumbradoGeneral],
            // 220-44: los contactos de inmuebles que no son vivienda van «sujetos a los factores de
            // demanda de la Tabla 220-42 o la Tabla 220-44».
            CategoriaDeCarga.Contactos => [JustificacionFactorDemanda.AlumbradoGeneral, JustificacionFactorDemanda.ContactosNoVivienda],
            CategoriaDeCarga.Equipo =>
            [
                JustificacionFactorDemanda.AparatosFijosVivienda, JustificacionFactorDemanda.SecadorasVivienda,
                JustificacionFactorDemanda.EstufasVivienda, JustificacionFactorDemanda.CocinaComercial,
                JustificacionFactorDemanda.CargasNoCoincidentes,
            ],
            _ => [],
        };
        if (!c.AdmiteFactorDeDemanda())
            return [];

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
