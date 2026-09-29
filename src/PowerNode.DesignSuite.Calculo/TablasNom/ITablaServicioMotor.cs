namespace PowerNode.DesignSuite.Calculo.TablasNom;

/// <summary>La clase de servicio de un motor que no trabaja continuo — Tabla 430-22(e). Nace en Power Node Web (I-120).</summary>
public enum ServicioDeMotor
{
    /// <summary>Accionamiento de válvulas, elevación o descenso de rodillos…</summary>
    CortaDuracion,

    /// <summary>Elevadores y montacargas, máquinas herramienta, bombas, puentes levadizos…</summary>
    Intermitente,

    /// <summary>Rodillos, máquinas de manipulación de minerales y carbón…</summary>
    Periodico,

    /// <summary>Servicio variable.</summary>
    Variable,
}

/// <summary>Para cuánto tiempo está especificado el motor — las cuatro columnas de la Tabla 430-22(e).</summary>
public enum EspecificacionDeTiempo
{
    Minutos5,
    Minutos15,
    Minutos30y60,
    Continuo,
}

/// <summary>
/// <b>Tabla 430-22(e)</b>: el porcentaje de la corriente de placa del motor que debe tener el conductor de
/// un motor de servicio no continuo. <c>null</c> donde la tabla dice «-» (corta duración, especificado
/// para servicio continuo).
/// </summary>
public interface ITablaServicioMotor
{
    decimal? Porcentaje(ServicioDeMotor servicio, EspecificacionDeTiempo especificacion);
}
