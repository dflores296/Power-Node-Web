using PowerNode.DesignSuite.Calculo.TablasNom;
using PowerNode.DesignSuite.Calculo.Unidades;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Protección contra sobrecorriente de un transformador de **más de 600 V** — Tabla 450-3(a).
///
/// Existe porque faltaba: <see cref="CalculadoraProteccionTransformador"/> implementa solo la
/// 450-3(b) (≤600 V) y **nunca recibía la tensión**, así que aplicaba esa tabla a cualquier
/// transformador. Un transformador de acometida de 13 200 V se estaba protegiendo con la tabla
/// equivocada, en silencio — y ese es el caso más común de un proyecto real.
///
/// La tabla tiene cuatro dimensiones y todas cambian el resultado:
///
/// | | ≤6% de impedancia | &gt;6% y ≤10% |
/// |---|---|---|
/// | **Cualquier lugar** — primario | interruptor 600%, fusible 300% | interruptor 400%, fusible 300% |
/// | **Cualquier lugar** — secundario &gt;600V | interruptor 300%, fusible 250% | interruptor 250%, fusible 225% |
/// | **Cualquier lugar** — secundario ≤600V | 125% | 125% |
/// | **Supervisado** — primario | interruptor 600%, fusible 300% | interruptor 400%, fusible 300% |
/// | **Supervisado** — secundario &gt;600V | interruptor 300%, fusible 250% (Nota 5) | 250% (Nota 5) |
/// | **Supervisado** — secundario ≤600V | 250% (Nota 5) | 250% (Nota 5) |
///
/// Además, en lugar supervisado la tabla permite un renglón **sin protección de secundario**:
/// primario con interruptor al 300% o fusible al 250%, para cualquier impedancia.
///
/// **Redondeo, verificado nota por nota (no de memoria):** los renglones de "cualquier lugar"
/// traen la Nota 1 — *"se permitirá tomar el valor nominal o ajuste estándar inmediatamente
/// superior"* — así que ahí se redondea hacia arriba. Los de lugar supervisado **no la traen**, de
/// modo que el valor no debe exceder el techo y se toma el estándar inmediato inferior. Es la
/// misma distinción que ya se había verificado para la 450-3(b).
///
/// **La impedancia es obligatoria aquí**, a diferencia de la 450-3(b): la tabla se indexa por ella.
/// Sin %Z de placa no hay renglón que elegir, y se prefiere tronar a inventar uno.
///
/// **Fuera de alcance, igual que en 450-3(b):** la Nota 5 (el transformador con protección térmica
/// coordinada de fábrica puede no llevar protección de secundario) y la Nota 2 (el secundario se
/// puede componer de hasta seis interruptores agrupados). Las dos son decisiones de diseño que
/// este motor no modela.
/// </summary>
public static class CalculadoraProteccion450_3a
{
    /// <summary>Arriba de este valor de impedancia la Tabla 450-3(a) ya no da renglón.</summary>
    public const decimal ImpedanciaMaximaPct = 10m;

    public static ResultadoProteccionTransformador Calcular(
        ITablaProteccionEstandar proteccionEstandar,
        decimal corrientePrimariaA,
        decimal corrienteSecundariaA,
        decimal tensionSecundariaV,
        decimal impedanciaPct,
        TipoLugarTransformador lugar,
        DispositivoProteccionTransformador dispositivo,
        bool protegerSecundario = true)
    {
        if (impedanciaPct <= 0)
            throw new ArgumentOutOfRangeException(nameof(impedanciaPct),
                "La Tabla 450-3(a) se indexa por la impedancia del transformador: hay que capturar el %Z de placa.");
        if (impedanciaPct > ImpedanciaMaximaPct)
            throw new ArgumentOutOfRangeException(nameof(impedanciaPct), impedanciaPct,
                $"La Tabla 450-3(a) solo llega hasta {ImpedanciaMaximaPct}% de impedancia.");

        var citas = new List<Cita>();
        var baja = impedanciaPct <= 6m;
        var rango = baja ? "impedancia no mayor al 6%" : "impedancia mayor al 6% pero máximo el 10%";

        // En "cualquier lugar" aplica la Nota 1 y se puede subir al estándar inmediato superior.
        // En lugar supervisado no hay tal nota: el valor no debe exceder el techo.
        var redondeaArriba = lugar == TipoLugarTransformador.CualquierLugar;

        // ---------------------------------------------------------------- primario
        // El renglón de lugar supervisado SIN protección de secundario usa porcentajes propios
        // (interruptor 300%, fusible 250%) y vale para cualquier impedancia.
        var soloPrimarioSupervisado = lugar == TipoLugarTransformador.Supervisado && !protegerSecundario;

        var pctPrimario = soloPrimarioSupervisado
            ? (dispositivo == DispositivoProteccionTransformador.InterruptorAutomatico ? 300m : 250m)
            : dispositivo == DispositivoProteccionTransformador.InterruptorAutomatico
                ? (baja ? 600m : 400m)
                : 300m;

        var proteccionPrimario = Elegir(proteccionEstandar, corrientePrimariaA, pctPrimario, redondeaArriba, "primario");

        citas.Add(new Cita("450-3(a)",
            $"Transformador de más de 600 V, {EtiquetaLugar(lugar)}, {rango}. " +
            $"Protección del primario con {EtiquetaDispositivo(dispositivo)}: {pctPrimario}% × " +
            $"{corrientePrimariaA:0.##} A = {corrientePrimariaA * pctPrimario / 100m:0.##} A → {proteccionPrimario} A" +
            (redondeaArriba
                ? " (Nota 1: se permite el estándar inmediato superior)."
                : " (sin nota de redondeo: no debe exceder el techo, estándar inmediato inferior).")));

        if (soloPrimarioSupervisado)
        {
            citas.Add(new Cita("450-3(a)",
                "En lugar supervisado la tabla no exige protección del secundario con este esquema."));
            return new ResultadoProteccionTransformador(proteccionPrimario, null, citas);
        }

        // ---------------------------------------------------------------- secundario
        var secundarioEsAltaTension = tensionSecundariaV > 600m;

        var pctSecundario = lugar == TipoLugarTransformador.CualquierLugar
            ? secundarioEsAltaTension
                ? dispositivo == DispositivoProteccionTransformador.InterruptorAutomatico
                    ? (baja ? 300m : 250m)
                    : (baja ? 250m : 225m)
                : 125m
            // Lugar supervisado: 300%/250% con impedancia baja e interruptor, 250% en los demás.
            : secundarioEsAltaTension && baja && dispositivo == DispositivoProteccionTransformador.InterruptorAutomatico
                ? 300m
                : 250m;

        var proteccionSecundario = Elegir(proteccionEstandar, corrienteSecundariaA, pctSecundario, redondeaArriba, "secundario");

        citas.Add(new Cita("450-3(a)",
            $"Protección del secundario ({(secundarioEsAltaTension ? "más de 600 V" : "600 V o menos")}): " +
            $"{pctSecundario}% × {corrienteSecundariaA:0.##} A = {corrienteSecundariaA * pctSecundario / 100m:0.##} A → " +
            $"{proteccionSecundario} A."));

        return new ResultadoProteccionTransformador(proteccionPrimario, proteccionSecundario, citas);
    }

    private static decimal Elegir(
        ITablaProteccionEstandar tabla, decimal corriente, decimal pct, bool haciaArriba, string lado)
    {
        var techo = corriente * pct / 100m;
        return haciaArriba
            ? tabla.SiguienteEstandar(techo)
            : tabla.AnteriorEstandar(techo)
                ?? throw new InvalidOperationException(
                    $"Ni el valor estándar más chico de 240-6(a) cabe dentro del techo de {techo:0.##} A " +
                    $"({pct}% × {corriente:0.##} A) para el {lado}.");
    }

    private static string EtiquetaLugar(TipoLugarTransformador l) =>
        l == TipoLugarTransformador.Supervisado ? "lugar supervisado (Nota 3)" : "cualquier lugar";

    private static string EtiquetaDispositivo(DispositivoProteccionTransformador d) =>
        d == DispositivoProteccionTransformador.InterruptorAutomatico ? "interruptor automático" : "fusible";
}
