using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// El conductor de puesta a tierra de equipos, con las tres reglas de 250-122 que aplican aquí —
/// no solo la tabla.
///
/// <b>Existía solo el renglón de la tabla</b> (250-122 por el amperaje de la protección) en las tres
/// calculadoras, con la misma línea copiada. Desde el 2026-08-17 las tres pasan por aquí. Faltaban:
///
/// <list type="number">
/// <item><b>250-122(b)</b>: <i>"Cuando se incrementa el tamaño de los conductores de fase, se debe
/// incrementar el tamaño de los conductores de puesta a tierra de equipos […] proporcionalmente al
/// área en mm² o kcmil de los conductores de fase."</i> En este programa la fase se incrementa
/// <b>seguido</b> —el paso de caída de tensión sube el calibre, y en alimentadores largos sube dos y
/// tres tamaños—, así que la tierra se quedaba corta justo en los casos que más importan.</item>
/// <item><b>250-122(a)</b>, última frase: <i>"en ningún caso se exigirá que sean mayores que los
/// conductores de los circuitos que alimentan el equipo"</i>. Es un tope, y aparece de verdad en
/// motores, donde 430-52 permite una protección muy por arriba de la ampacidad del conductor.</item>
/// <item><b>250-122(f)</b> es la sección que manda un conductor de tierra por canalización cuando las
/// fases van en paralelo. La regla ya se aplicaba; la cita decía <c>310-10(h)(5)</c>, que es otra
/// cosa.</item>
/// </list>
///
/// <b>250-122(d)(2)</b> (motores con disparo instantáneo) no se resuelve aquí: depende del tipo de
/// dispositivo del circuito de motor, así que lo decide <see cref="CalculadoraCircuitoDerivadoMotor"/> y
/// entra por <paramref name="proteccionParaTablaA"/>.
/// </summary>
public static class PuestaTierraEquipos
{
    /// <param name="proteccionParaTablaA">Con qué amperaje se entra a la Tabla 250-122. Normalmente es
    /// la protección del circuito; en un motor con disparo instantáneo es otra cosa — 250-122(d)(2).</param>
    /// <param name="calibreFaseBase">El calibre que exigía la ampacidad, antes de crecer.</param>
    /// <param name="calibreFaseFinal">El que se va a instalar.</param>
    public static (Calibre Calibre, List<Cita> Citas) Seleccionar(
        ITablaPuestaTierra tabla,
        ICatalogoCalibres catalogo,
        decimal proteccionParaTablaA,
        MaterialConductor material,
        Calibre calibreFaseBase,
        Calibre calibreFaseFinal,
        int nParalelo)
    {
        var citas = new List<Cita>();
        var deTabla = tabla.CalibreMinimo(proteccionParaTablaA, material);
        var elegido = deTabla;

        citas.Add(new Cita("250-122", $"Protección {proteccionParaTablaA:0.##} A -> tierra {deTabla} (Tabla 250-122)"));

        // 250-122(b): la fase creció -> la tierra crece en la misma proporción de área.
        if (calibreFaseFinal.AreaMm2 > calibreFaseBase.AreaMm2 && calibreFaseBase.AreaMm2 > 0m)
        {
            var proporcion = calibreFaseFinal.AreaMm2 / calibreFaseBase.AreaMm2;
            var areaMinima = deTabla.AreaMm2 * proporcion;
            var proporcional = catalogo.BuscarPorAreaMinima(areaMinima);

            if (proporcional.AreaMm2 > elegido.AreaMm2)
            {
                elegido = proporcional;
                citas.Add(new Cita("250-122(b)",
                    $"La fase subió de {calibreFaseBase} a {calibreFaseFinal} (x{proporcion:0.##} en área), así que la tierra sube " +
                    $"proporcionalmente: {deTabla.AreaMm2:0.##} mm² x {proporcion:0.##} = {areaMinima:0.##} mm² -> {elegido}"));
            }
        }

        // 250-122(a): la tierra nunca se exige mayor que el conductor de fase del circuito.
        if (elegido.AreaMm2 > calibreFaseFinal.AreaMm2)
        {
            citas.Add(new Cita("250-122(a)",
                $"La tabla pedía {elegido}, mayor que el conductor de fase ({calibreFaseFinal}); la norma no exige que la tierra " +
                $"sea mayor que los conductores del circuito, así que se topa en {calibreFaseFinal}"));
            elegido = calibreFaseFinal;
        }

        if (nParalelo > 1)
            citas.Add(new Cita("250-122(f)",
                $"Fases en paralelo: un conductor de tierra completo ({elegido}) por cada canalización, {nParalelo} en total"));

        return (elegido, citas);
    }
}
