namespace PowerNode.Web.Modelo;

/// <summary>
/// <b>El subtipo de una carga</b> — I-123, decisión <c>cargas-y-clases-de-circuito.md</c>. Dice qué
/// sección de la norma la calcula: 180 VA por contacto (220-14(i)), 600 VA por portalámparas de servicio
/// pesado (220-14(e)), la secadora en vivienda (220-54)… Cada subtipo es de un tipo
/// (<see cref="SubtiposDeCarga.Tipo"/>); lo que dice cada uno y sus artículos están en
/// <see cref="GuiaDeCargas"/>, la misma fuente que enseña la guía.
/// </summary>
public enum SubtipoDeCarga
{
    // Alumbrado
    Luminarias,
    PortalamparasPesado,
    Anuncios,
    Aparador,

    // Contactos
    ContactoUsoGeneral,
    ContactoMultiple,
    EnsambleDeSalidas,

    // Aparatos y cargas específicas
    AparatoFijo,
    Secadora,
    Coccion,
    CocinaComercial,
    CalentadorDeAgua,
    AparatoConMotor,
    OtraCargaEspecifica,

    // Motores
    MotorUsoGeneral,
    MotorVelocidadAjustable,

    // Aire acondicionado y refrigeración
    Motocompresor,
    CargaCombinada,
    AireDeHabitacion,

    // Calefacción eléctrica fija de ambiente
    CalefaccionResistencia,
    CalefaccionConMotor,

    // Alimentador a tablero
    TableroAlimentado,
}

public static class SubtiposDeCarga
{
    /// <summary>
    /// El tipo al que pertenece. El alimentador a tablero es de <see cref="CategoriaDeCarga"/> hasta que
    /// ese tipo exista en el modelo (fase B); mientras, <c>null</c>.
    /// </summary>
    public static CategoriaDeCarga? Tipo(this SubtipoDeCarga s) => s switch
    {
        SubtipoDeCarga.Luminarias or SubtipoDeCarga.PortalamparasPesado or SubtipoDeCarga.Anuncios or SubtipoDeCarga.Aparador
            => CategoriaDeCarga.Alumbrado,
        SubtipoDeCarga.ContactoUsoGeneral or SubtipoDeCarga.ContactoMultiple or SubtipoDeCarga.EnsambleDeSalidas
            => CategoriaDeCarga.Contactos,
        SubtipoDeCarga.AparatoFijo or SubtipoDeCarga.Secadora or SubtipoDeCarga.Coccion or SubtipoDeCarga.CocinaComercial
            or SubtipoDeCarga.CalentadorDeAgua or SubtipoDeCarga.AparatoConMotor or SubtipoDeCarga.OtraCargaEspecifica
            => CategoriaDeCarga.Equipo,
        SubtipoDeCarga.MotorUsoGeneral or SubtipoDeCarga.MotorVelocidadAjustable => CategoriaDeCarga.Motor,
        SubtipoDeCarga.Motocompresor or SubtipoDeCarga.CargaCombinada or SubtipoDeCarga.AireDeHabitacion
            => CategoriaDeCarga.AireAcondicionado,
        SubtipoDeCarga.CalefaccionResistencia or SubtipoDeCarga.CalefaccionConMotor => CategoriaDeCarga.CalefaccionFija,
        _ => null,
    };

    /// <summary>El nombre corto, basado en la NOM (David, 2026-09-29): el del selector.</summary>
    public static string Nombre(this SubtipoDeCarga s) => s switch
    {
        SubtipoDeCarga.Luminarias => "Luminarias",
        SubtipoDeCarga.PortalamparasPesado => "Portalámparas pesado",
        SubtipoDeCarga.Anuncios => "Anuncios y contorno",
        SubtipoDeCarga.Aparador => "Aparador",
        SubtipoDeCarga.ContactoUsoGeneral => "Uso general",
        SubtipoDeCarga.ContactoMultiple => "Múltiple (4 o más)",
        SubtipoDeCarga.EnsambleDeSalidas => "Ensamble de salidas",
        SubtipoDeCarga.AparatoFijo => "Fijo",
        SubtipoDeCarga.Secadora => "Secadora",
        SubtipoDeCarga.Coccion => "Cocción",
        SubtipoDeCarga.CocinaComercial => "Cocina comercial",
        SubtipoDeCarga.CalentadorDeAgua => "Calentador de agua",
        SubtipoDeCarga.AparatoConMotor => "Con motor",
        SubtipoDeCarga.OtraCargaEspecifica => "Otra carga",
        SubtipoDeCarga.MotorUsoGeneral => "Uso general",
        SubtipoDeCarga.MotorVelocidadAjustable => "Velocidad ajustable",
        SubtipoDeCarga.Motocompresor => "Motocompresor",
        SubtipoDeCarga.CargaCombinada => "Carga combinada",
        SubtipoDeCarga.AireDeHabitacion => "De habitación",
        SubtipoDeCarga.CalefaccionResistencia => "Resistencia",
        SubtipoDeCarga.CalefaccionConMotor => "Con motor",
        _ => "Tablero",
    };
}
