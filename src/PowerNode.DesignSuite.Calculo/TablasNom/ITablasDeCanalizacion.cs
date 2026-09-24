using PowerNode.DesignSuite.Calculo.Canalizaciones;

namespace PowerNode.DesignSuite.Calculo.TablasNom;

// NACIDO EN LA WEB (2026-09-24): las tablas del Capítulo 10 para dimensionar canalizaciones.

/// <summary>Un tamaño comercial de la Tabla 4 del Capítulo 10, con sus áreas disponibles.</summary>
/// <param name="DesignacionMetrica">16, 21, 27… — la designación métrica.</param>
/// <param name="TamanoComercial">«½», «¾», «1 ¼»… tal como la publica la tabla.</param>
public sealed record TamanoDeTubo(
    int DesignacionMetrica,
    string TamanoComercial,
    decimal DiametroInteriorMm,
    decimal Area100Mm2,
    decimal Area60Mm2,
    decimal Area53Mm2,
    decimal Area31Mm2,
    decimal Area40Mm2)
{
    /// <summary>
    /// El área disponible con ese porcentaje de ocupación. Se usa la <b>columna publicada</b> cuando
    /// existe (53, 31, 40 y 60 %) y no el producto, que redondea distinto.
    /// </summary>
    public decimal AreaDisponible(decimal porcentaje) => porcentaje switch
    {
        53m => Area53Mm2,
        31m => Area31Mm2,
        40m => Area40Mm2,
        60m => Area60Mm2,
        _ => Area100Mm2 * porcentaje / 100m,
    };

    /// <summary>«27 (1)»: designación métrica y, entre paréntesis, el tamaño comercial.</summary>
    public string Rotulo => $"{DesignacionMetrica} ({TamanoComercial})";
}

/// <summary>Tabla 1 del Capítulo 10 — porcentaje de la sección del tubo que pueden ocupar los conductores.</summary>
public interface ITablaOcupacion
{
    /// <summary>53 con un conductor, 31 con dos y 40 con más de dos.</summary>
    decimal PorcentajeMaximo(int numeroConductores);
}

/// <summary>Tabla 4 del Capítulo 10 — dimensiones del tubo conduit por tipo.</summary>
public interface ITablaTuboConduit
{
    /// <summary>Los tamaños que existen de ese tipo, de menor a mayor. Los «––» no se incluyen.</summary>
    IReadOnlyList<TamanoDeTubo> Tamanos(TipoTuboConduit tipo);
}

/// <summary>Tablas 5 y 8 del Capítulo 10 — dimensiones de los conductores.</summary>
public interface ITablaDimensionesConductor
{
    /// <summary>
    /// Diámetro y área aproximados del conductor aislado (Tabla 5). Null si ese tipo o ese calibre no
    /// está en la tabla — THW-LS y THHW-LS, por ejemplo: la Nota 5 pide entonces las dimensiones
    /// reales.
    /// </summary>
    (decimal DiametroMm, decimal AreaMm2)? Aislado(string designacion, string tipoAislamiento);

    /// <summary>Diámetro y área total del conductor desnudo, trenzado (Tabla 8; Nota 8 del Cap. 10).</summary>
    (decimal DiametroMm, decimal AreaMm2)? Desnudo(string designacion);
}

/// <summary>Tabla 310-15(b)(3)(c) — lo que se suma a la temperatura ambiente en azoteas al sol.</summary>
public interface ITablaTemperaturaAzotea
{
    /// <summary>°C que se suman según la distancia del techo a la base del tubo, en mm.</summary>
    decimal Sumador(decimal alturaSobreTechoMm);
}
