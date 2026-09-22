namespace PowerNode.DesignSuite.Calculo.Unidades;

/// <summary>
/// Un calibre estándar de conductor, tal como aparece en la Tabla 8 de la norma.
/// La designación (AWG o kcmil) es una etiqueta categórica, no una unidad: nunca se calcula por
/// fórmula, siempre se resuelve contra este catálogo de 40 tamaños estándar.
/// </summary>
public sealed record Calibre(
    string Designacion,
    decimal AreaMm2,
    decimal DiametroMm,
    decimal ResistenciaCuNoCubiertoOhmKm,
    decimal ResistenciaCuRecubiertoOhmKm,
    decimal? ResistenciaAlOhmKm)
{
    /// <summary>
    /// 310-10(h)(1): mínimo 1/0 AWG para poder correr conductores en paralelo. El texto de la norma
    /// redondea el área a 53.5 mm², pero el valor real de la Tabla 8 para 1/0 AWG es 53.49 mm² —
    /// se usa ese umbral para que 1/0 AWG mismo (el caso que cita el propio artículo) sí califique.
    /// </summary>
    public bool PermiteParalelo => AreaMm2 >= 53.49m;

    /// <summary>
    /// La designación <b>con su unidad</b>: «12 AWG», «250 kcmil». Es lo que se rotula en una tabla o
    /// en un plano — «12» a secas no dice si son 3.3 mm² o 127.
    /// </summary>
    public string DesignacionConUnidad => UnidadDe(Designacion);

    /// <summary>
    /// Le pone la unidad a una designación suelta, para cuando lo que se tiene es la cadena que
    /// guardó el cálculo y no el <see cref="Calibre"/> completo.
    ///
    /// <para>
    /// <b>El corte está en 250 y no es arbitrario:</b> en la Tabla 8 los AWG llegan hasta 4/0 y los
    /// kcmil empiezan en 250, así que <b>ningún número es las dos cosas</b> — no hay un «250 AWG» con
    /// el que confundirse. Los que traen diagonal (1/0 … 4/0) son AWG por construcción.
    /// </para>
    ///
    /// <para>
    /// Lo que no se reconoce se devuelve <b>tal cual</b>, sin unidad inventada: más vale un rótulo
    /// escueto que uno que afirme algo falso.
    /// </para>
    /// </summary>
    public static string UnidadDe(string? designacion)
    {
        var d = designacion?.Trim();
        if (string.IsNullOrEmpty(d)) return string.Empty;

        if (d.Contains('/')) return $"{d} AWG";

        return int.TryParse(d, System.Globalization.NumberStyles.Integer,
                System.Globalization.CultureInfo.InvariantCulture, out var n)
            ? n >= PrimerKcmil ? $"{d} kcmil" : $"{d} AWG"
            : d;
    }

    /// <summary>El calibre más chico que la Tabla 8 expresa en kcmil. Abajo de él, todo es AWG.</summary>
    private const int PrimerKcmil = 250;

    public override string ToString() => Designacion;
}
