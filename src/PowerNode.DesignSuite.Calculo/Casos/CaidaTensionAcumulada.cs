namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>Un tramo de la ruta: un alimentador o el circuito derivado del final.</summary>
/// <param name="Descripcion">Cómo se nombra en el aviso — "Alimentador a TD-2", "Circuito 14".</param>
public sealed record TramoCaida(string Descripcion, decimal CaidaPct);

/// <summary>
/// La caída de tensión <b>acumulada</b> por la rama — hallazgo A16 de la auditoría.
///
/// <b>Lo que dice la norma</b> (210-19(a)(1) NOTA 4, y su gemela 215-2(a)(4) NOTA 2): los conductores
/// se dimensionan para no exceder <b>3 %</b> en la salida más lejana <i>"y en los que la caída máxima
/// de tensión combinada de los circuitos alimentadores y de los circuitos derivados hasta el contacto
/// más lejano no supere <b>5 por ciento</b>"</i>.
///
/// <b>El hueco que cierra:</b> antes cada tramo se verificaba por su cuenta, así que un derivado al
/// 3 % colgado de un alimentador al 5 % daba <b>8 % acumulado</b> y el programa lo daba por bueno.
/// Nadie sumaba la rama porque nadie la recorría.
///
/// <b>Es AVISO, no bloqueo, y eso no es pereza:</b> el 3/5 % es una <b>nota informativa</b> de la
/// norma, no un requisito. El límite por tramo sí es error duro (es criterio de diseño del proyecto,
/// y el usuario lo captura); el acumulado se reporta. Por eso esta clase no sube calibres: los tramos
/// ya vienen calculados y ella solo suma y compara.
///
/// <b>Las cuatro decisiones de diseño, confirmadas con el usuario el 2026-08-17:</b>
/// <list type="number">
/// <item><b>El transformador corta la acumulación.</b> Del otro lado hay otro sistema de tensión, y
/// la caída se mide contra la nominal de <i>ese</i> sistema. Quien recorre el árbol arranca una ruta
/// nueva en cada transformador; esta clase solo ve rutas ya cortadas.</item>
/// <item><b>Se suman porcentajes</b>, no volts. Dentro de un mismo sistema la base es la misma, así
/// que la suma es legítima — y lo es <i>porque</i> el transformador ya cortó.</item>
/// <item><b>El límite por alimentador dejó de ser el 5 %</b> y pasó a ser valor propio de diseño
/// (<c>CaidaTensionMaxAlimentadorPct</c>). El 5 % es ahora esta verificación, que es lo que la norma
/// de verdad dice. Antes el campo se llamaba "combinado" y se aplicaba por tramo: el nombre prometía
/// algo que el código no hacía.</item>
/// <item><b>La ruta llega hasta cada circuito hoja</b>, que es "la salida más lejana" en este modelo.
/// </item>
/// </list>
/// </summary>
public static class CaidaTensionAcumulada
{
    /// <param name="ExcedeLimite">Solo el aviso. Nada del resultado del cálculo cambia por esto.</param>
    /// <param name="Aviso">Texto listo para mostrar, o <c>null</c> si la ruta cumple.</param>
    public sealed record Resultado(decimal AcumuladaPct, bool ExcedeLimite, string? Aviso, IReadOnlyList<Cita> Citas);

    /// <summary>
    /// Suma una ruta ya cortada por transformadores y la compara contra el límite combinado.
    ///
    /// Una ruta <b>vacía</b> devuelve 0 y sin aviso: es el caso de un tablero colgado directo de la
    /// acometida sin circuitos calculados todavía, y no hay nada que reportar. Igual que en el resto
    /// de esta casa, <b>callar cuando falta el dato</b> es la conducta correcta — inventar un
    /// acumulado sería peor que no tenerlo.
    /// </summary>
    public static Resultado Evaluar(IReadOnlyList<TramoCaida> ruta, decimal limiteCombinadoPct)
    {
        var acumulada = ruta.Sum(t => t.CaidaPct);
        var citas = new List<Cita>();

        if (ruta.Count == 0)
            return new Resultado(0m, false, null, citas);

        var desglose = string.Join(" + ", ruta.Select(t => $"{t.Descripcion} {t.CaidaPct:0.##}%"));

        // La cita es de la NOTA, y se nombra como nota -- no como exigencia. El motor nunca dice
        // "210-19 exige" para la caída de tensión, y esto no es la excepción.
        citas.Add(new Cita("210-19(a)(1) NOTA 4",
            $"Caída acumulada por la rama: {desglose} = {acumulada:0.##}% (referencia informativa: {limiteCombinadoPct:0.##}%)"));

        if (acumulada <= limiteCombinadoPct)
            return new Resultado(acumulada, false, null, citas);

        var aviso =
            $"Caída de tensión acumulada {acumulada:0.##}%, por encima del {limiteCombinadoPct:0.##}% combinado que recomienda la " +
            $"NOTA 4 de 210-19(a)(1). Ruta: {desglose}. Es una nota informativa de la norma, no un requisito: cada tramo por " +
            $"separado sí cumple su límite.";

        citas.Add(new Cita("210-19(a)(1) NOTA 4", aviso));
        return new Resultado(acumulada, true, aviso, citas);
    }
}
