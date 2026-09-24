namespace PowerNode.Web.Modelo;

/// <summary>
/// Para qué es un circuito de contactos. Solo importa en vivienda: 210-11(c) exige circuitos de
/// <b>20 A</b> para aparatos pequeños, lavadora y baño, y 220-52 carga al alimentador <b>1500 VA</b>
/// por cada circuito de aparatos pequeños o de lavadora. <b>General por omisión</b>: sin mínimo, la
/// protección sale de la carga y 240-6(a).
///
/// <para>
/// Sustituye al mínimo de 20 A en todos los contactos, que era criterio del Excel y no de la NOM
/// (David, 2026-09-24 — <c>docs/decisiones/minimo-de-proteccion-por-uso.md</c>).
/// </para>
/// </summary>
public enum UsoDeContactos
{
    General,

    /// <summary>Cocina, despensa, comedor y desayunador — 210-11(c)(1), 210-52(b).</summary>
    AparatosPequenos,

    /// <summary>Contacto de la lavadora — 210-11(c)(2), 210-52(f).</summary>
    Lavadora,

    /// <summary>Contactos de cuarto de baño — 210-11(c)(3), 210-52(d).</summary>
    Bano,
}

public static class UsosDeContactos
{
    /// <summary>La protección mínima de un circuito de vivienda con este uso — 210-11(c).</summary>
    public const decimal ProteccionMinimaViviendaA = 20m;

    /// <summary>La carga mínima con la que entra al alimentador — 220-52(a) y (b).</summary>
    public const decimal CargaMinimaAlimentadorVA = 1500m;

    public static string Nombre(this UsoDeContactos uso) => uso switch
    {
        UsoDeContactos.AparatosPequenos => "Aparatos pequeños (cocina)",
        UsoDeContactos.Lavadora => "Lavadora",
        UsoDeContactos.Bano => "Baño",
        _ => "General",
    };

    /// <summary>Para el selector del renglón, que mide 108 px: «Cocina» en vez de «Aparatos pequeños (cocina)».</summary>
    public static string NombreCorto(this UsoDeContactos uso) => uso switch
    {
        UsoDeContactos.AparatosPequenos => "Cocina",
        _ => uso.Nombre(),
    };

    /// <summary>La referencia del 20 A. <c>null</c> en uso general: no hay mínimo.</summary>
    public static string? ReferenciaProteccionMinima(this UsoDeContactos uso) => uso switch
    {
        UsoDeContactos.AparatosPequenos => "210-11(c)(1)",
        UsoDeContactos.Lavadora => "210-11(c)(2)",
        UsoDeContactos.Bano => "210-11(c)(3)",
        _ => null,
    };

    /// <summary>La referencia de los 1500 VA. <c>null</c> si el uso no la tiene (general y baño).</summary>
    public static string? ReferenciaCargaMinima(this UsoDeContactos uso) => uso switch
    {
        UsoDeContactos.AparatosPequenos => "220-52(a)",
        UsoDeContactos.Lavadora => "220-52(b)",
        _ => null,
    };
}
