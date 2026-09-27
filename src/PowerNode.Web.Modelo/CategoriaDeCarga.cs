using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>El tipo de carga del circuito, como lo agrupa el Art. 220 para el factor de demanda</b> — R-17
/// (David, 2026-09-24). Los factores van por tipo, no por continua / no continua: la tabla de
/// alumbrado (220-42) no es la de contactos (220-44) ni la de aparatos (220-53 a 220-56).
///
/// <para>
/// <b>Seis tipos desde el 2026-09-27</b> — I-74, <c>docs/decisiones/tipos-de-carga.md</c>. Motor y A/C
/// eran uno solo, y la unidad decidía el artículo. La regla: si cambia el factor de demanda del
/// Art. 220, es un tipo; si solo cambia el cálculo del circuito, es un selector dentro del tipo (como
/// el uso de los contactos). Los <b>nombres</b> de los valores van en el archivo: renombrar uno pide
/// leer el viejo (<see cref="DelArchivo"/>).
/// </para>
///
/// <para>
/// <b>Todos admiten factor de demanda, con justificación</b> — R-18. Motores (220-50, 430-26), A/C
/// (220-50, 440-6) y calefacción fija (220-51) se calculan al 100 % como regla general, pero 430-26 y
/// la Excepción de 220-51 permiten menos cuando no funcionan todos a la vez o trabajan por ciclos.
/// </para>
///
/// <para>
/// El circuito: Alumbrado y Contactos, Art. 210; Equipo y Calefacción, carga de placa
/// (<see cref="TipoCarga.Equipo"/>); <see cref="Motor"/>, Art. 430 (<see cref="MotoresEnHp"/>);
/// <see cref="AireAcondicionado"/>, Art. 440 (<see cref="AireAcondicionadoDePlaca"/>).
/// </para>
/// </summary>
public enum CategoriaDeCarga
{
    Alumbrado,
    Contactos,
    Equipo,
    Motor,
    AireAcondicionado,
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
        CategoriaDeCarga.Motor => "Motor",
        CategoriaDeCarga.AireAcondicionado => "A/C y refrig.",
        _ => "Calefacción",
    };

    /// <summary>Para el resumen, el documento y la memoria.</summary>
    public static string NombreCompleto(this CategoriaDeCarga c) => c switch
    {
        CategoriaDeCarga.Equipo => "Equipo (aparatos)",
        CategoriaDeCarga.Motor => "Motores",
        CategoriaDeCarga.AireAcondicionado => "Aire acondicionado y refrigeración",
        CategoriaDeCarga.CalefaccionFija => "Calefacción eléctrica fija",
        _ => c.Nombre(),
    };

    /// <summary>Qué va en cada tipo, con ejemplos. Es el tooltip del selector.</summary>
    public static string Descripcion(this CategoriaDeCarga c) => c switch
    {
        CategoriaDeCarga.Alumbrado => "Alumbrado: luminarias y alumbrado general — Tabla 220-42.",
        CategoriaDeCarga.Contactos => "Contactos: contactos de uso general. En vivienda, seleccionar el uso (cocina, lavadora, baño) — 210-11(c).",
        CategoriaDeCarga.Equipo => "Equipo: aparatos con su valor de placa, también los que traen motor: hornos, estufas, parrillas, secadoras, calentadores de agua, lavavajillas, equipo electrónico — Art. 422; 220-53 a 220-56.",
        CategoriaDeCarga.Motor => "Motor: bombas, ventiladores, extractores, compresores de aire, bandas — Art. 430. Se captura en HP, o en A si la placa no trae HP: la corriente sale de la tabla — 430-6(a)(1).",
        CategoriaDeCarga.AireAcondicionado => "A/C y refrigeración: equipos con motocompresor hermético: minisplit (también inverter frío/calor), bomba de calor, paquete, condensadora, cámara de refrigeración — Art. 440. Se captura la placa: ampacidad mínima y protección máxima (MCA, MOCP — 440-4(b)), o la corriente de carga nominal del compresor (440-6(a)).",
        _ => "Calefacción: calefacción por resistencia eléctrica: calefactores, cables calefactores, calderas eléctricas. Carga continua — 424-3(b); 220-51.",
    };

    /// <summary>
    /// Entra al alimentador como motor: 125 % del mayor y 100 % de los demás — 430-24, 440-33. Motor y
    /// A/C y refrigeración van en el mismo grupo (220-50, 440-33).
    /// </summary>
    public static bool EsDeMotor(this CategoriaDeCarga c) =>
        c is CategoriaDeCarga.Motor or CategoriaDeCarga.AireAcondicionado;

    /// <summary>
    /// El tipo con el que calcula el derivado no-motor. Motor y A/C no pasan por él: tienen su propio
    /// cálculo (430 y 440).
    /// </summary>
    public static TipoCarga TipoDelMotor(this CategoriaDeCarga c) => c switch
    {
        CategoriaDeCarga.Alumbrado => TipoCarga.Alumbrado,
        CategoriaDeCarga.Contactos => TipoCarga.Contactos,
        CategoriaDeCarga.Motor => TipoCarga.Fuerza,
        _ => TipoCarga.Equipo,
    };

    /// <summary>El nombre que va en el archivo.</summary>
    public static string AlArchivo(this CategoriaDeCarga c) => c.ToString();

    /// <summary>
    /// <b>El tipo de un archivo</b>, con los nombres viejos. «MotorOAireAcondicionado» (formato 1) ya no
    /// existe: regresa <c>null</c> con <paramref name="eraMotorOAire"/> para que quien lee decida — un
    /// renglón con HP es Motor; uno sin HP, A/C y refrigeración (I-74).
    /// </summary>
    public static CategoriaDeCarga? DelArchivo(string? nombre, out bool eraMotorOAire)
    {
        eraMotorOAire = nombre == "MotorOAireAcondicionado";
        return Enum.TryParse<CategoriaDeCarga>(nombre, out var c) && Enum.IsDefined(c) && !int.TryParse(nombre, out _) ? c : null;
    }

    /// <summary>
    /// Las justificaciones que le aplican a un tipo en un inmueble — R-19. Solo se ofrece lo que la
    /// norma permite ahí: la Tabla 220-42 si su renglón reduce (no en «todos los demás»); 220-44 y
    /// 220-56 fuera de vivienda; 220-53 a 220-55, 220-82 y 220-83 en vivienda; 220-84 en
    /// multifamiliar, 220-86 en escuelas, 220-88 en restaurantes. 220-60, 220-87 y «Otra», siempre.
    /// </summary>
    public static IReadOnlyList<JustificacionFactorDemanda> JustificacionesPosibles(this CategoriaDeCarga c, TipoDeInmueble inmueble)
    {
        // Motores, A/C y calefacción: solo con la condición de su propia sección, o si no coinciden.
        if (c.EsDeMotor())
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
