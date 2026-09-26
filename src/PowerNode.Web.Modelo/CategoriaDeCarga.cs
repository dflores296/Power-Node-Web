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
/// son <see cref="TipoCarga.Equipo"/>: carga de placa. Un motor capturado en HP es
/// <see cref="TipoCarga.Fuerza"/> y se calcula por el Art. 430 — I-15, <see cref="MotoresEnHp"/>.
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
        CategoriaDeCarga.MotorOAireAcondicionado => "Motor / A/C: todo lo que funciona con motor o compresor: aire acondicionado, refrigeración, bombas, ventiladores, bombas de calor e inverter frío/calor — 220-50. En HP, circuito de motor por el Art. 430; en VA, W o A, carga de placa.",
        _ => "Calefacción: calefacción por resistencia eléctrica: calefactores, cables calefactores, calderas eléctricas. Carga continua — 424-3(b); 220-51.",
    };

    /// <summary>El tipo con el que calcula el motor: los tres de equipo son carga de placa.</summary>
    public static TipoCarga TipoDelMotor(this CategoriaDeCarga c) => c switch
    {
        CategoriaDeCarga.Alumbrado => TipoCarga.Alumbrado,
        CategoriaDeCarga.Contactos => TipoCarga.Contactos,
        _ => TipoCarga.Equipo,
    };

    /// <summary>
    /// Las justificaciones que le aplican a un tipo en un inmueble — R-19. Solo se ofrece lo que la
    /// norma permite ahí: la Tabla 220-42 si su renglón reduce (no en «todos los demás»); 220-44 y
    /// 220-56 fuera de vivienda; 220-53 a 220-55, 220-82 y 220-83 en vivienda; 220-84 en
    /// multifamiliar, 220-86 en escuelas, 220-88 en restaurantes. 220-60, 220-87 y «Otra», siempre.
    /// </summary>
    public static IReadOnlyList<JustificacionFactorDemanda> JustificacionesPosibles(this CategoriaDeCarga c, TipoDeInmueble inmueble)
    {
        // Motores y calefacción: solo con la condición de su propia sección, o si no coinciden.
        if (c == CategoriaDeCarga.MotorOAireAcondicionado)
            return [JustificacionFactorDemanda.MotoresNoSimultaneos, JustificacionFactorDemanda.CargasNoCoincidentes, JustificacionFactorDemanda.Otra];
        if (c == CategoriaDeCarga.CalefaccionFija)
            return [JustificacionFactorDemanda.CalefaccionPorCiclos, JustificacionFactorDemanda.CargasNoCoincidentes, JustificacionFactorDemanda.Otra];

        var vivienda = inmueble.EsVivienda();
        var tabla220_42 = inmueble.FilaTabla220_42() is not null;
        var lista = new List<JustificacionFactorDemanda>();

        switch (c)
        {
            case CategoriaDeCarga.Alumbrado:
                if (tabla220_42) lista.Add(JustificacionFactorDemanda.AlumbradoGeneral);
                break;
            case CategoriaDeCarga.Contactos:
                // Vivienda: 220-52 deja sumar los contactos al alumbrado general con la Tabla 220-42.
                // Fuera de vivienda: 220-44, «sujetos a la Tabla 220-42 o la Tabla 220-44».
                if (tabla220_42) lista.Add(JustificacionFactorDemanda.AlumbradoGeneral);
                if (!vivienda) lista.Add(JustificacionFactorDemanda.ContactosNoVivienda);
                break;
            default: // Equipo
                if (vivienda)
                    lista.AddRange([JustificacionFactorDemanda.AparatosFijosVivienda, JustificacionFactorDemanda.SecadorasVivienda, JustificacionFactorDemanda.EstufasVivienda]);
                else
                    lista.Add(JustificacionFactorDemanda.CocinaComercial);
                lista.Add(JustificacionFactorDemanda.CargasNoCoincidentes);
                break;
        }

        // Parte D: métodos opcionales, cada uno para su inmueble.
        if (inmueble is TipoDeInmueble.ViviendaUnifamiliar or TipoDeInmueble.ViviendaPopular)
            lista.AddRange([JustificacionFactorDemanda.ViviendaMetodoOpcional, JustificacionFactorDemanda.ViviendaExistente]);
        if (inmueble == TipoDeInmueble.ViviendaMultifamiliar)
            lista.Add(JustificacionFactorDemanda.ViviendaMultifamiliar);
        if (inmueble == TipoDeInmueble.Escuela)
            lista.Add(JustificacionFactorDemanda.Escuelas);
        if (inmueble == TipoDeInmueble.Restaurante)
            lista.Add(JustificacionFactorDemanda.RestauranteNuevo);
        lista.AddRange([JustificacionFactorDemanda.DemandaMaximaMedida, JustificacionFactorDemanda.Otra]);
        return lista;
    }
}
