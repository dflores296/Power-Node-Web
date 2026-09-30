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
    // Los usos de vivienda — 210-11(c), 210-52(b)(1) Exc. 2 (David, 2026-09-30: antes, un selector del renglón).
    ContactoAparatosPequenos,
    ContactoLavadora,
    ContactoBano,
    ContactoRefrigerador,

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
    /// El tipo al que pertenece.
    /// </summary>
    public static CategoriaDeCarga Tipo(this SubtipoDeCarga s) => s switch
    {
        SubtipoDeCarga.Luminarias or SubtipoDeCarga.PortalamparasPesado or SubtipoDeCarga.Anuncios or SubtipoDeCarga.Aparador
            => CategoriaDeCarga.Alumbrado,
        SubtipoDeCarga.ContactoUsoGeneral or SubtipoDeCarga.ContactoMultiple or SubtipoDeCarga.EnsambleDeSalidas
            or SubtipoDeCarga.ContactoAparatosPequenos or SubtipoDeCarga.ContactoLavadora or SubtipoDeCarga.ContactoBano
            or SubtipoDeCarga.ContactoRefrigerador
            => CategoriaDeCarga.Contactos,
        SubtipoDeCarga.AparatoFijo or SubtipoDeCarga.Secadora or SubtipoDeCarga.Coccion or SubtipoDeCarga.CocinaComercial
            or SubtipoDeCarga.CalentadorDeAgua or SubtipoDeCarga.AparatoConMotor or SubtipoDeCarga.OtraCargaEspecifica
            => CategoriaDeCarga.Equipo,
        SubtipoDeCarga.MotorUsoGeneral or SubtipoDeCarga.MotorVelocidadAjustable => CategoriaDeCarga.Motor,
        SubtipoDeCarga.Motocompresor or SubtipoDeCarga.CargaCombinada or SubtipoDeCarga.AireDeHabitacion
            => CategoriaDeCarga.AireAcondicionado,
        SubtipoDeCarga.CalefaccionResistencia or SubtipoDeCarga.CalefaccionConMotor => CategoriaDeCarga.CalefaccionFija,
        _ => CategoriaDeCarga.Tablero,
    };

    /// <summary>
    /// Cómo se captura: una carga de placa, un motor (HP o A), un motocompresor (corriente nominal) o un
    /// acondicionador de habitación (corriente total). Lo fija el subtipo.
    /// </summary>
    public static ClaseDeAparato Clase(this SubtipoDeCarga s) => s switch
    {
        SubtipoDeCarga.MotorUsoGeneral or SubtipoDeCarga.AparatoConMotor or SubtipoDeCarga.CalefaccionConMotor => ClaseDeAparato.Motor,
        SubtipoDeCarga.Motocompresor => ClaseDeAparato.Motocompresor,
        SubtipoDeCarga.AireDeHabitacion => ClaseDeAparato.AireDeHabitacion,
        SubtipoDeCarga.MotorVelocidadAjustable => ClaseDeAparato.Variador,
        _ => ClaseDeAparato.Carga,
    };

    /// <summary>
    /// Solo en el renglón: un equipo de A/C con MCA y MOCP es un solo equipo que no se mezcla con otras
    /// cargas (440-4(b)). Otro tablero y el motor con variador ya no: un alimentador puede llevar varios
    /// tableros, y un circuito, varios variadores como grupo — 430-53 (David, 2026-09-30).
    /// </summary>
    public static bool SoloEnElRenglon(this SubtipoDeCarga s) => s is SubtipoDeCarga.CargaCombinada;

    /// <summary>El uso de vivienda de un subtipo de contactos — 210-11(c); <c>null</c> si no es uno.</summary>
    public static UsoDeContactos? Uso(this SubtipoDeCarga s) => s switch
    {
        SubtipoDeCarga.ContactoAparatosPequenos => UsoDeContactos.AparatosPequenos,
        SubtipoDeCarga.ContactoLavadora => UsoDeContactos.Lavadora,
        SubtipoDeCarga.ContactoBano => UsoDeContactos.Bano,
        SubtipoDeCarga.ContactoRefrigerador => UsoDeContactos.Refrigerador,
        _ => null,
    };

    /// <summary>El subtipo de un uso de vivienda (el de un renglón de contactos de un archivo anterior).</summary>
    public static SubtipoDeCarga DeUso(UsoDeContactos uso) => uso switch
    {
        UsoDeContactos.AparatosPequenos => SubtipoDeCarga.ContactoAparatosPequenos,
        UsoDeContactos.Lavadora => SubtipoDeCarga.ContactoLavadora,
        UsoDeContactos.Bano => SubtipoDeCarga.ContactoBano,
        UsoDeContactos.Refrigerador => SubtipoDeCarga.ContactoRefrigerador,
        _ => SubtipoDeCarga.ContactoUsoGeneral,
    };

    /// <summary>
    /// <b>Va solo en su circuito, cantidad 1</b> — decisión <c>captura-en-el-desplegable.md</c>:
    /// el A/A con ampacidad de placa (440-4(b)) y el contacto del refrigerador (210-52(b)(1) Exc. 2). Un
    /// tablero alimentado va con otros tableros; un variador, con otros motores como grupo (430-53).
    /// </summary>
    public static bool VaSolo(this SubtipoDeCarga s) =>
        s is SubtipoDeCarga.CargaCombinada or SubtipoDeCarga.ContactoRefrigerador;

    /// <summary>
    /// Solo con líneas de su mismo subtipo: los contactos de aparatos pequeños, de lavadora y de baño
    /// alimentan solo esas salidas — 210-52(b)(2), 210-11(c)(2), (3).
    /// </summary>
    public static bool SoloConSuSubtipo(this SubtipoDeCarga s) =>
        s is SubtipoDeCarga.ContactoAparatosPequenos or SubtipoDeCarga.ContactoLavadora or SubtipoDeCarga.ContactoBano;

    /// <summary>El subtipo de una línea nueva del desplegable, según el tipo.</summary>
    public static SubtipoDeCarga PorOmision(CategoriaDeCarga tipo) => tipo switch
    {
        CategoriaDeCarga.Alumbrado => SubtipoDeCarga.Luminarias,
        CategoriaDeCarga.Contactos => SubtipoDeCarga.ContactoUsoGeneral,
        CategoriaDeCarga.Motor => SubtipoDeCarga.MotorUsoGeneral,
        CategoriaDeCarga.AireAcondicionado => SubtipoDeCarga.Motocompresor,
        CategoriaDeCarga.CalefaccionFija => SubtipoDeCarga.CalefaccionResistencia,
        CategoriaDeCarga.Tablero => SubtipoDeCarga.TableroAlimentado,
        _ => SubtipoDeCarga.OtraCargaEspecifica,
    };

    /// <summary>Los subtipos de un tipo, en el orden de la guía.</summary>
    public static IReadOnlyList<SubtipoDeCarga> De(CategoriaDeCarga tipo) =>
        [.. Enum.GetValues<SubtipoDeCarga>().Where(s => s.Tipo() == tipo)];

    /// <summary>
    /// <b>La carga mínima de cada unidad</b> — 220-14: 180 VA por contacto (i), 90 VA por contacto de
    /// uno múltiple de cuatro o más (i), 600 VA por portalámparas de servicio pesado (e), 180 VA por
    /// tramo de ensamble de salidas (h); en vivienda, 5000 VA por secadora (220-54). <c>null</c> sin
    /// mínimo. Los anuncios llevan el suyo por circuito (<see cref="MinimoPorCircuitoVA"/>).
    /// </summary>
    public static (decimal VA, string Referencia)? MinimoUnitarioVA(this SubtipoDeCarga s, bool vivienda) => s switch
    {
        SubtipoDeCarga.ContactoUsoGeneral => (180m, "220-14(i)"),
        SubtipoDeCarga.ContactoMultiple => (90m, "220-14(i)"),
        SubtipoDeCarga.PortalamparasPesado => (600m, "220-14(e)"),
        SubtipoDeCarga.EnsambleDeSalidas => (180m, "220-14(h)"),
        // 220-14(i) no se aplica a los de aparatos pequeños ni de lavadora: llevan los 1500 VA de 220-52.
        SubtipoDeCarga.ContactoBano or SubtipoDeCarga.ContactoRefrigerador => (180m, "220-14(i)"),
        SubtipoDeCarga.Secadora when vivienda => (5000m, "220-54"),
        _ => null,
    };

    /// <summary>Anuncios y contorno: 1200 VA por cada circuito exigido — 220-14(f), 600-5(a).</summary>
    public static (decimal VA, string Referencia)? MinimoPorCircuitoVA(this SubtipoDeCarga s) =>
        s == SubtipoDeCarga.Anuncios ? (1200m, "220-14(f)") : null;

    /// <summary>
    /// Siempre continua: el calentador de agua con almacenamiento (422-13) y la calefacción fija de
    /// ambiente (424-3(b)). <c>null</c> si se decide por la placa.
    /// </summary>
    public static string? SiempreContinua(this SubtipoDeCarga s) => s switch
    {
        SubtipoDeCarga.CalentadorDeAgua => "422-13",
        SubtipoDeCarga.CalefaccionResistencia or SubtipoDeCarga.CalefaccionConMotor => "424-3(b)",
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
        SubtipoDeCarga.ContactoAparatosPequenos => "Ap. pequeños",
        SubtipoDeCarga.ContactoLavadora => "Lavadora",
        SubtipoDeCarga.ContactoBano => "Baño",
        SubtipoDeCarga.ContactoRefrigerador => "Refrigerador",
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
