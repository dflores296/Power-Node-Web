using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Canalizaciones;

// NACIDO EN LA WEB (Power-Node-Web, 2026-09-24) — el escritorio no calcula canalizaciones. Llevarlo
// allá está registrado en docs/conocimiento/motor-copiado.md del repo web. Decisión:
// docs/decisiones/canalizaciones-y-agrupamiento.md.

/// <summary>
/// Por dónde van los conductores. Cada tipo trae su propia regla de llenado y de ajuste por
/// agrupamiento — por eso no basta con «tubo»:
/// <list type="bullet">
/// <item>Tubo conduit (342 a 362): Tabla 1 del Capítulo 10 y ajuste con más de 3 portadores.</item>
/// <item>Niple de 60 cm o menos: 60 % y sin ajuste (Nota 4 del Cap. 10; 310-15(b)(3)(a)(2)).</item>
/// <item>Ductos y canales auxiliares metálicos: 20 % y ajuste solo con más de 30 (376-22, 366-23(a)).</item>
/// <item>Ductos y canales no metálicos: 20 % y ajuste como en tubo (378-22, 366-23(b)).</item>
/// <item>Superficiales: el número del fabricante (386-22, 388-22).</item>
/// </list>
/// Charola (392) queda para la entrega 2.
/// </summary>
public enum TipoCanalizacion
{
    TuboConduit,
    Niple,
    DuctoMetalico,
    DuctoNoMetalico,
    CanalAuxiliarMetalico,
    CanalAuxiliarNoMetalico,
    SuperficialMetalica,
    SuperficialNoMetalica,
}

/// <summary>
/// Los bloques de la Tabla 4 del Capítulo 10 que se ofrecen. Son 11: el DOF publica 12, pero el
/// segundo «Cédula 80» no puede ser Cédula 80 (ver <c>TablaTuboConduitJson</c> y la decisión).
/// </summary>
public enum TipoTuboConduit
{
    Emt,
    Ent,
    Fmc,
    Imc,
    LfncB,
    LfncA,
    Lfmc,
    Rmc,
    PvcCedula80,
    PvcCedula40,
    PvcTipoA,
}

public static class TiposDeCanalizacion
{
    public static string Nombre(this TipoCanalizacion t) => t switch
    {
        TipoCanalizacion.TuboConduit => "Tubo conduit",
        TipoCanalizacion.Niple => "Niple (60 cm o menos)",
        TipoCanalizacion.DuctoMetalico => "Ducto metálico",
        TipoCanalizacion.DuctoNoMetalico => "Ducto no metálico",
        TipoCanalizacion.CanalAuxiliarMetalico => "Canal auxiliar metálico",
        TipoCanalizacion.CanalAuxiliarNoMetalico => "Canal auxiliar no metálico",
        TipoCanalizacion.SuperficialMetalica => "Canalización superficial metálica",
        _ => "Canalización superficial no metálica",
    };

    /// <summary>El artículo que la regula, para las citas.</summary>
    public static string Articulo(this TipoCanalizacion t) => t switch
    {
        TipoCanalizacion.DuctoMetalico => "376",
        TipoCanalizacion.DuctoNoMetalico => "378",
        TipoCanalizacion.CanalAuxiliarMetalico or TipoCanalizacion.CanalAuxiliarNoMetalico => "366",
        TipoCanalizacion.SuperficialMetalica => "386",
        TipoCanalizacion.SuperficialNoMetalica => "388",
        _ => "Capítulo 10",
    };

    /// <summary>Tubo o niple: se dimensiona con las Tablas 1 y 4.</summary>
    public static bool EsTubo(this TipoCanalizacion t) => t is TipoCanalizacion.TuboConduit or TipoCanalizacion.Niple;

    /// <summary>Ducto o canal auxiliar: 20 % del área interior capturada.</summary>
    public static bool EsDuctoOCanal(this TipoCanalizacion t) =>
        t is TipoCanalizacion.DuctoMetalico or TipoCanalizacion.DuctoNoMetalico
            or TipoCanalizacion.CanalAuxiliarMetalico or TipoCanalizacion.CanalAuxiliarNoMetalico;

    public static bool EsSuperficial(this TipoCanalizacion t) =>
        t is TipoCanalizacion.SuperficialMetalica or TipoCanalizacion.SuperficialNoMetalica;

    public static string Nombre(this TipoTuboConduit t) => t switch
    {
        TipoTuboConduit.Emt => "EMT (metálico ligero)",
        TipoTuboConduit.Ent => "ENT (no metálico)",
        TipoTuboConduit.Fmc => "FMC (metálico flexible)",
        TipoTuboConduit.Imc => "IMC (metálico semipesado)",
        TipoTuboConduit.LfncB => "LFNC-B (no metálico flexible hermético)",
        TipoTuboConduit.LfncA => "LFNC-A (no metálico flexible hermético)",
        TipoTuboConduit.Lfmc => "LFMC (metálico flexible hermético)",
        TipoTuboConduit.Rmc => "RMC (metálico pesado)",
        TipoTuboConduit.PvcCedula80 => "PVC cédula 80",
        TipoTuboConduit.PvcCedula40 => "PVC cédula 40 / HDPE",
        _ => "PVC tipo A",
    };

    /// <summary>Rótulo corto para la tabla del cuadro: «PVC 40», «EMT».</summary>
    public static string Corto(this TipoTuboConduit t) => t switch
    {
        TipoTuboConduit.Emt => "EMT",
        TipoTuboConduit.Ent => "ENT",
        TipoTuboConduit.Fmc => "FMC",
        TipoTuboConduit.Imc => "IMC",
        TipoTuboConduit.LfncB => "LFNC-B",
        TipoTuboConduit.LfncA => "LFNC-A",
        TipoTuboConduit.Lfmc => "LFMC",
        TipoTuboConduit.Rmc => "RMC",
        TipoTuboConduit.PvcCedula80 => "PVC 80",
        TipoTuboConduit.PvcCedula40 => "PVC 40",
        _ => "PVC A",
    };

    public static string Articulo(this TipoTuboConduit t) => t switch
    {
        TipoTuboConduit.Emt => "358",
        TipoTuboConduit.Ent => "362",
        TipoTuboConduit.Fmc => "348",
        TipoTuboConduit.Imc => "342",
        TipoTuboConduit.LfncB or TipoTuboConduit.LfncA => "356",
        TipoTuboConduit.Lfmc => "350",
        TipoTuboConduit.Rmc => "344",
        _ => "352",
    };

    /// <summary>Tubo metálico: EMT, IMC, RMC, FMC y LFMC.</summary>
    public static bool EsMetalico(this TipoTuboConduit t) =>
        t is TipoTuboConduit.Emt or TipoTuboConduit.Imc or TipoTuboConduit.Rmc or TipoTuboConduit.Fmc or TipoTuboConduit.Lfmc;

    /// <summary>
    /// La columna de la Tabla 9 con la que se lee la reactancia de los conductores en esta
    /// canalización. Tubo metálico: acero, o aluminio si así se declara. No metálico: PVC. Ductos y
    /// canales: metálico → acero, no metálico → PVC — un supuesto, porque la Tabla 9 es de tubo.
    /// </summary>
    public static MaterialCanalizacion MaterialParaTabla9(TipoCanalizacion tipo, TipoTuboConduit tubo, bool aluminio)
    {
        var metalica = tipo switch
        {
            TipoCanalizacion.TuboConduit or TipoCanalizacion.Niple => tubo.EsMetalico(),
            TipoCanalizacion.DuctoMetalico or TipoCanalizacion.CanalAuxiliarMetalico or TipoCanalizacion.SuperficialMetalica => true,
            _ => false,
        };
        if (!metalica) return MaterialCanalizacion.Pvc;
        return aluminio ? MaterialCanalizacion.Aluminio : MaterialCanalizacion.Acero;
    }
}
