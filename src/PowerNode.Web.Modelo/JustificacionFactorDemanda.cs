namespace PowerNode.Web.Modelo;

/// <summary>
/// Con qué se justifica un factor de demanda menor que 1 en el alimentador — 220-40 lo permite
/// «después de aplicar cualquier factor de demanda aplicable», y el Art. 220 dice cuáles. El factor lo
/// captura el proyectista; esto es solo el sustento que imprime la memoria (R-12, David 2026-09-24).
/// Se escogen varias: un tablero puede reducir alumbrado por una tabla y contactos por otra.
/// </summary>
public enum JustificacionFactorDemanda
{
    AlumbradoGeneral,
    ContactosNoVivienda,
    AparatosFijosVivienda,
    SecadorasVivienda,
    EstufasVivienda,
    CocinaComercial,
    CargasNoCoincidentes,
    ViviendaMetodoOpcional,
    ViviendaExistente,
    ViviendaMultifamiliar,
    Escuelas,
    DemandaMaximaMedida,
    RestauranteNuevo,

    /// <summary>Criterio del proyectista, con su texto en <see cref="DatosDelTablero.JustificacionOtra"/>.</summary>
    Otra,
}

public static class JustificacionesFactorDemanda
{
    /// <summary>El texto que imprime la memoria: referencia y qué es.</summary>
    public static string Nombre(this JustificacionFactorDemanda j) => j switch
    {
        JustificacionFactorDemanda.AlumbradoGeneral => "Tabla 220-42 — alumbrado general",
        JustificacionFactorDemanda.ContactosNoVivienda => "220-44 — contactos en inmuebles que no son vivienda",
        JustificacionFactorDemanda.AparatosFijosVivienda => "220-53 — cuatro o más aparatos fijos en vivienda, 75 %",
        JustificacionFactorDemanda.SecadorasVivienda => "Tabla 220-54 — secadoras de ropa en vivienda",
        JustificacionFactorDemanda.EstufasVivienda => "Tabla 220-55 — estufas y aparatos de cocción en vivienda",
        JustificacionFactorDemanda.CocinaComercial => "Tabla 220-56 — equipo de cocina comercial",
        JustificacionFactorDemanda.CargasNoCoincidentes => "220-60 — cargas no coincidentes",
        JustificacionFactorDemanda.ViviendaMetodoOpcional => "220-82 — vivienda, método opcional",
        JustificacionFactorDemanda.ViviendaExistente => "220-83 — vivienda existente",
        JustificacionFactorDemanda.ViviendaMultifamiliar => "220-84 — vivienda multifamiliar",
        JustificacionFactorDemanda.Escuelas => "Tabla 220-86 — escuelas",
        JustificacionFactorDemanda.DemandaMaximaMedida => "220-87 — instalación existente con demanda máxima medida",
        JustificacionFactorDemanda.RestauranteNuevo => "Tabla 220-88 — restaurante nuevo",
        _ => "Otra — criterio del proyectista",
    };
}
