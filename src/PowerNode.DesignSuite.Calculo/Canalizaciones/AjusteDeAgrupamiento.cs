using PowerNode.DesignSuite.Calculo.Casos;

namespace PowerNode.DesignSuite.Calculo.Canalizaciones;

/// <param name="Aplica">Si la Tabla 310-15(b)(3)(a) se aplica en esta canalización.</param>
/// <param name="ConductoresParaElMotor">Lo que se le pasa a los calculadores como
/// <c>NumeroConductoresAgrupados</c>: los portadores si aplica, 1 si no (la tabla da 1.00 hasta 3).</param>
public sealed record AjusteDeAgrupamiento(bool Aplica, int ConductoresParaElMotor, string Motivo, Cita Cita);

/// <summary>
/// <b>Si la Tabla 310-15(b)(3)(a) se aplica, según el tipo de canalización.</b> NACIDO EN LA WEB
/// (2026-09-24). Cada artículo trae su propia regla:
/// <list type="bullet">
/// <item>Tubo conduit: con más de 3 portadores — 310-15(b)(3)(a).</item>
/// <item>Niple de 60 cm o menos: no — 310-15(b)(3)(a)(2) y Nota 4 del Capítulo 10.</item>
/// <item>Ducto metálico: solo con más de 30 — 376-22(b). Canal auxiliar metálico: igual — 366-23(a).</item>
/// <item>Ducto no metálico — 378-22 — y canal auxiliar no metálico — 366-23(b): como en tubo.</item>
/// <item>Superficial metálica: no, si pasa de 2500 mm², lleva 30 portadores o menos y no pasa del 20 % —
/// 386-22. Superficial no metálica: como en tubo — 388-22 no la exime.</item>
/// </list>
/// Los calculadores de derivado y alimentador no cambian: reciben el número que devuelve esto.
/// </summary>
public static class AjusteDeAgrupamientoPorCanalizacion
{
    /// <param name="areaInteriorMm2">Solo superficial metálica: el área interior de la canalización.</param>
    /// <param name="ocupacionPct">Solo superficial metálica: la ocupación ya calculada, si se conoce.
    /// Sin ella se supone que no pasa del 20 % y el llamador la confirma después.</param>
    public static AjusteDeAgrupamiento Evaluar(
        TipoCanalizacion tipo,
        int portadores,
        decimal? areaInteriorMm2 = null,
        decimal? ocupacionPct = null)
    {
        const string tabla = "Tabla 310-15(b)(3)(a)";
        AjusteDeAgrupamiento Si(string referencia, string motivo) =>
            new(portadores > 3, portadores, motivo, new Cita(referencia, motivo));
        AjusteDeAgrupamiento No(string referencia, string motivo) =>
            new(false, 1, motivo, new Cita(referencia, motivo));

        switch (tipo)
        {
            case TipoCanalizacion.Niple:
                return No("310-15(b)(3)(a)(2)", $"Niple de 60 cm o menos: sin ajuste por agrupamiento ({portadores} portadores) — también Nota 4 del Capítulo 10.");

            case TipoCanalizacion.DuctoMetalico:
                return portadores > 30
                    ? Si("376-22(b)", $"Ducto metálico con {portadores} portadores, más de 30: se aplica la {tabla}.")
                    : No("376-22(b)", $"Ducto metálico con {portadores} portadores: el ajuste solo aplica con más de 30.");

            case TipoCanalizacion.CanalAuxiliarMetalico:
                return portadores > 30
                    ? Si("366-23(a)", $"Canal auxiliar metálico con {portadores} portadores, más de 30: se aplica la {tabla}.")
                    : No("366-23(a)", $"Canal auxiliar metálico con {portadores} portadores: el ajuste solo aplica con más de 30.");

            case TipoCanalizacion.SuperficialMetalica:
                var exenta = areaInteriorMm2 is > 2500m && portadores <= 30 && (ocupacionPct ?? 0m) <= 20m;
                return exenta
                    ? No("386-22", $"Superficial metálica de {areaInteriorMm2:N0} mm², {portadores} portadores y ocupación de 20 % o menos: sin ajuste.")
                    : Si("386-22", $"Superficial metálica que no cumple las tres condiciones de 386-22: se aplica la {tabla} ({portadores} portadores).");

            case TipoCanalizacion.DuctoNoMetalico:
                return Si("378-22", $"Ducto no metálico con {portadores} portadores: {tabla}.");

            case TipoCanalizacion.CanalAuxiliarNoMetalico:
                return Si("366-23(b)", $"Canal auxiliar no metálico con {portadores} portadores: {tabla}.");

            case TipoCanalizacion.SuperficialNoMetalica:
                return Si("388-22", $"Superficial no metálica con {portadores} portadores: {tabla}.");

            default:
                return Si("310-15(b)(3)(a)", portadores > 3
                    ? $"Tubo conduit con {portadores} portadores: {tabla}."
                    : $"Tubo conduit con {portadores} portadores: 3 o menos, sin ajuste.");
        }
    }
}
