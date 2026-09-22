using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

public sealed record ResultadoProteccionTransformador(
    decimal ProteccionPrimarioA,
    decimal? ProteccionSecundarioA,
    IReadOnlyList<Cita> Citas);

/// <summary>
/// Art. 450-3(b) — protección contra sobrecorriente de un transformador de 600 V o menos, como %
/// de su corriente nominal de placa (<see cref="CalculadoraTransformador"/> da Ip/Is; esta clase
/// decide qué breaker/fusible le corresponde a cada uno). Dos esquemas, decisión de diseño del
/// usuario (<see cref="EsquemaProteccionTransformador"/>) -- la Tabla 450-3(b) no dice cuál usar,
/// solo qué % aplica en cada caso:
///
/// <list type="bullet">
/// <item><b>Solo primario</b>: 125% (Ip ≥ 9A), 167% (Ip &lt; 9A) o 300% (Ip &lt; 2A). Sin protección
/// en el secundario.</item>
/// <item><b>Primario y secundario</b>: primario siempre 250% (cualquier corriente); secundario 125%
/// (Is ≥ 9A) o 167% (Is &lt; 9A).</item>
/// </list>
///
/// <b>Redondeo -- verificado contra las notas reales de la tabla, no de memoria:</b> el 125% es el
/// ÚNICO porcentaje que trae permiso explícito de redondear hacia el estándar inmediato SUPERIOR
/// (nota 1: "cuando el 125% ... no corresponde a un valor estándar ... se permitirá elegir el valor
/// nominal estándar inmediatamente superior"). 167%, 300% y 250% no traen esa nota -- el valor
/// elegido no debe exceder el techo calculado, así que ahí se redondea hacia el inmediato INFERIOR.
///
/// <b>Nota 3 -- protección térmica coordinada de fábrica.</b> Es un <i>permiso que sube el techo</i>
/// del primario, no una regla de redondeo, y solo aplica al esquema <b>primario y secundario</b>:
/// en la tabla real el marcador "véase nota 3" está en las tres celdas de 250 % de ese renglón y en
/// <b>ninguna</b> del renglón de "primario solamente". Textual: un transformador equipado por el
/// fabricante con protección térmica coordinada contra sobrecarga, dispuesta para interrumpir la
/// corriente del primario, admite protección primaria de hasta <b>6 veces</b> la corriente nominal
/// si su impedancia no pasa del 6 %, y de <b>4 veces</b> si pasa del 6 % pero no del 10 %.
/// <list type="bullet">
/// <item>%Z &gt; 10 %: la nota no concede nada -- se queda en 250 %.</item>
/// <item>%Z sin capturar (0): tampoco se aplica. No se puede saber si toca 6x o 4x, y suponer el
/// más flojo sería inventar. Se queda en 250 % y la cita dice por qué.</item>
/// <item>Redondeo: la nota dice "que no sea más de", o sea techo -- inmediato INFERIOR, igual que
/// el 250 % que sustituye. No hereda el permiso de la nota 1.</item>
/// </list>
/// El permiso es un <b>máximo, no una obligación</b>, y se aplica solo cuando el usuario declara la
/// protección térmica (<c>Transformador.ProteccionTermicaCoordinadaFabrica</c>, false por omisión),
/// así que ningún cálculo existente cambia de resultado al aparecer esto.
///
/// <b>Fuera de v1:</b> 450-3(a) (transformadores &gt;600V, Tabla 450-3(a)) y 450-3(c)
/// (transformadores de potencial) -- fuera del alcance real de este proyecto (distribución de baja
/// tensión). También la nota 2 (el secundario repartido en hasta seis interruptores agrupados), que
/// es decisión de diseño y no un número que el motor pueda elegir solo.
/// </summary>
public static class CalculadoraProteccionTransformador
{
    public static ResultadoProteccionTransformador Calcular(
        ITablaProteccionEstandar proteccionEstandar,
        decimal corrientePrimariaA,
        decimal corrienteSecundariaA,
        EsquemaProteccionTransformador esquema,
        bool proteccionTermicaCoordinadaFabrica = false,
        decimal impedanciaPct = 0m)
    {
        var citas = new List<Cita>();

        if (esquema == EsquemaProteccionTransformador.SoloPrimario)
        {
            var pct = corrientePrimariaA < 2m ? 300m : corrientePrimariaA < 9m ? 167m : 125m;
            var techo = corrientePrimariaA * pct / 100m;
            var proteccionPrimario = pct == 125m
                ? proteccionEstandar.SiguienteEstandar(techo)
                : proteccionEstandar.AnteriorEstandar(techo)
                    ?? throw new InvalidOperationException(
                        $"Ni el valor estándar más chico de 240-6(a) cabe dentro del techo de {techo:0.##} A ({pct}% x {corrientePrimariaA:0.##} A) -- transformador demasiado pequeño para este esquema.");

            citas.Add(new Cita("450-3(b)",
                $"Protección del primario solamente: {pct}% x {corrientePrimariaA:0.##} A = {techo:0.##} A -> {proteccionPrimario} A" +
                (pct == 125m ? " (nota 1: redondeo al inmediato superior)" : " (sin nota de redondeo: no debe exceder el techo, inmediato inferior)")));
            citas.Add(new Cita("450-3(b)", "Con este esquema, el secundario no requiere protección propia."));

            return new ResultadoProteccionTransformador(proteccionPrimario, null, citas);
        }
        else
        {
            // 250% en toda la banda de corriente. Las tres celdas de este renglón traen "véase nota
            // 3", que NO es una nota de redondeo: es el permiso de subir el techo cuando el
            // transformador trae protección térmica coordinada de fábrica.
            var (multiplo, razonNota3) = MultiploNota3(proteccionTermicaCoordinadaFabrica, impedanciaPct);
            var techoPrimario = corrientePrimariaA * multiplo;
            var proteccionPrimario = proteccionEstandar.AnteriorEstandar(techoPrimario)
                ?? throw new InvalidOperationException(
                    $"Ni el valor estándar más chico de 240-6(a) cabe dentro del techo de {techoPrimario:0.##} A ({multiplo * 100m:0.##}% x {corrientePrimariaA:0.##} A) -- transformador demasiado pequeño para este esquema.");
            citas.Add(new Cita("450-3(b)",
                $"Protección del primario (con secundario protegido aparte): {multiplo * 100m:0.##}% x {corrientePrimariaA:0.##} A = {techoPrimario:0.##} A -> {proteccionPrimario} A " +
                "(techo máximo: se elige el estándar inmediato inferior)"));
            citas.Add(new Cita("450-3(b) nota 3", razonNota3));

            var pctSecundario = corrienteSecundariaA < 9m ? 167m : 125m;
            var techoSecundario = corrienteSecundariaA * pctSecundario / 100m;
            var proteccionSecundario = pctSecundario == 125m
                ? proteccionEstandar.SiguienteEstandar(techoSecundario)
                : proteccionEstandar.AnteriorEstandar(techoSecundario)
                    ?? throw new InvalidOperationException(
                        $"Ni el valor estándar más chico de 240-6(a) cabe dentro del techo de {techoSecundario:0.##} A ({pctSecundario}% x {corrienteSecundariaA:0.##} A) del secundario.");

            citas.Add(new Cita("450-3(b)",
                $"Protección del secundario: {pctSecundario}% x {corrienteSecundariaA:0.##} A = {techoSecundario:0.##} A -> {proteccionSecundario} A" +
                (pctSecundario == 125m ? " (nota 1: redondeo al inmediato superior)" : " (sin nota de redondeo: no debe exceder el techo, inmediato inferior)")));

            return new ResultadoProteccionTransformador(proteccionPrimario, proteccionSecundario, citas);
        }
    }

    /// <summary>
    /// El múltiplo de la corriente nominal que puede alcanzar la protección del primario, y la
    /// explicación de por qué. Sin nota 3 es 2.5 (el 250 % de la tabla); con ella, 6 o 4 según el %Z.
    /// <para>
    /// Los límites son los de la nota textual: "no tienen una impedancia de más del 6 por ciento"
    /// (o sea %Z ≤ 6 → 6x) y "más del 6 por ciento pero no más del 10 por ciento" (6 &lt; %Z ≤ 10 →
    /// 4x). Arriba del 10 % la nota simplemente no dice nada, así que no concede nada.
    /// </para>
    /// </summary>
    private static (decimal Multiplo, string Razon) MultiploNota3(bool proteccionTermica, decimal impedanciaPct)
    {
        if (!proteccionTermica)
            return (2.5m, "No aplica: el transformador no está declarado con protección térmica coordinada de fábrica, " +
                          "así que rige el 250 % de la tabla.");

        // El %Z se captura de la placa y 0 significa "no capturada" (misma convención que
        // CalculadoraImpedanciaTransformador). Sin él no se puede saber si toca 6x o 4x, y suponer
        // el más flojo aflojaría la protección sin sustento.
        if (impedanciaPct <= 0m)
            return (2.5m, "No se pudo aplicar: el transformador declara protección térmica coordinada de fábrica, " +
                          "pero la nota reparte el permiso según la impedancia (%Z) y no se capturó. Se mantiene el " +
                          "250 % de la tabla; capturar el %Z de placa puede permitir hasta 600 %.");

        if (impedanciaPct <= 6m)
            return (6m, $"Aplicada: protección térmica coordinada de fábrica dispuesta para interrumpir la corriente " +
                        $"del primario, con impedancia de {impedanciaPct:0.##} % (no mayor al 6 %), así que la nota " +
                        $"permite hasta 6 veces la corriente nominal.");

        if (impedanciaPct <= 10m)
            return (4m, $"Aplicada: protección térmica coordinada de fábrica dispuesta para interrumpir la corriente " +
                        $"del primario, con impedancia de {impedanciaPct:0.##} % (mayor al 6 % pero no mayor al 10 %), " +
                        $"así que la nota permite hasta 4 veces la corriente nominal.");

        return (2.5m, $"No aplica: la impedancia de {impedanciaPct:0.##} % pasa del 10 %, y la nota solo concede el " +
                      $"permiso hasta ese valor. Rige el 250 % de la tabla.");
    }
}
