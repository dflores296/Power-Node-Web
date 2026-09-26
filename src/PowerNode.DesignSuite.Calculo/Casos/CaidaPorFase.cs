using PowerNode.DesignSuite.Calculo.Magnitudes;

namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>
/// Lo que lleva una fase de un alimentador, <b>antes</b> del factor de demanda (lo aplica
/// <see cref="CalculadoraAlimentador"/>, 220-40).
///
/// <para>
/// Dos lecturas de la misma corriente, cada una para lo suyo:
/// </para>
/// <list type="bullet">
/// <item><see cref="ContinuaA"/> y <see cref="NoContinuaA"/>: <b>suma aritmética</b> de las
/// corrientes de los circuitos que tocan la fase. Dimensiona protección y conductor
/// (215-2(a)(1), 215-3). Del lado seguro: nunca es menor que la suma fasorial.</item>
/// <item><see cref="FasorContinua"/> y <see cref="FasorNoContinua"/>: <b>suma fasorial</b>, con
/// la corriente saliendo por la fase y el ángulo respecto a V<sub>AN</sub> = 0°. Da la caída de
/// tensión con el neutro — R-02.</item>
/// </list>
/// </summary>
/// <param name="AnguloTensionGrados">Ángulo de V fase-neutro: A 0°, B −120°, C 120° en estrella;
/// A 0°, B 180° en 1F-3H.</param>
/// <param name="Motores">Los motores que toca la fase, con su FLC de tabla y la protección de su
/// derivado — I-15. <b>No</b> van en <paramref name="ContinuaA"/> ni en
/// <paramref name="NoContinuaA"/>: su capacidad es la de 430-24 (125 % del mayor + la suma de los
/// demás), no la de 215-3. Vacío = la fase no lleva motores.</param>
/// <param name="FasorMotores">La suma fasorial de las FLC de esos motores, al 100 %: la corriente
/// que de verdad circula en operación normal, para la caída de tensión.</param>
public sealed record CorrienteDeFaseAlimentador(
    char Fase,
    decimal ContinuaA,
    decimal NoContinuaA,
    decimal AnguloTensionGrados,
    Fasor FasorContinua,
    Fasor FasorNoContinua,
    AgregadoMotores Motores = default,
    Fasor FasorMotores = default);

/// <summary>La caída de una fase del alimentador, con su corriente ya con demanda.</summary>
public sealed record CaidaDeFase(char Fase, Fasor Corriente, decimal CaidaV, decimal CaidaPct);

/// <summary>
/// <b>La caída de tensión de un alimentador con neutro, fase por fase</b> — R-02.
///
/// <para>
/// La tensión que le llega a la carga de la fase f es V<sub>f</sub> − Z·I<sub>f</sub> −
/// Z·I<sub>N</sub>: la fase cae por su corriente y el neutro, que regresa la suma fasorial de todas,
/// cae por la suya. La caída es la componente de Z·(I<sub>f</sub> + I<sub>N</sub>) en la dirección
/// de V<sub>f</sub>:
/// </para>
/// <code>e_f = Re[ Z · (I_f + I_N) · conj(û_f) ],   I_N = Σ I_f,   Z = (R + jX) · L / N</code>
/// <para>
/// Balanceado en 3F-4H, I<sub>N</sub> = 0 y sale lo mismo que √3·L·I·(R cosθ + X senθ) / V<sub>FF</sub>.
/// En 1F-2H, I<sub>N</sub> = I<sub>A</sub> y sale 2·L·I·(R cosθ + X senθ) / V<sub>FN</sub>. El
/// equivalente balanceado ignoraba el neutro: con el caso base a 80 m, fase C 4.62 % contra 6.77 %.
/// </para>
/// </summary>
public static class CaidaPorFase
{
    /// <summary>El neutro regresa la suma fasorial de las corrientes de fase.</summary>
    public static Fasor CorrienteDeNeutro(IReadOnlyList<Fasor> corrientesDeFase) =>
        corrientesDeFase.Aggregate(new Fasor(0m, 0m), (suma, i) => suma + i);

    /// <param name="corrientes">Corriente de cada fase con demanda, y el ángulo de su tensión F-N.</param>
    /// <param name="rOhmKm">Resistencia del conductor — Tabla 9. El neutro es del mismo calibre.</param>
    /// <param name="nParalelo">Conductores por fase (y por neutro).</param>
    public static IReadOnlyList<CaidaDeFase> Calcular(
        IReadOnlyList<(char Fase, Fasor Corriente, decimal AnguloTensionGrados)> corrientes,
        decimal rOhmKm, decimal xOhmKm, decimal longitudM, int nParalelo, decimal tensionFaseNeutroV)
    {
        var neutro = CorrienteDeNeutro([.. corrientes.Select(c => c.Corriente)]);
        var km = longitudM / 1000m / nParalelo;
        var r = (double)(rOhmKm * km);
        var x = (double)(xOhmKm * km);

        return
        [
            .. corrientes.Select(c =>
            {
                var suma = c.Corriente + neutro;
                // Girar al eje de la tensión de la fase: la parte real es la caída en su dirección.
                var giro = new Fasor(suma.Magnitud, suma.AnguloGrados - c.AnguloTensionGrados);
                var real = (double)giro.Real;
                var imaginario = (double)giro.Imaginario;
                var caidaV = (decimal)(r * real - x * imaginario);
                return new CaidaDeFase(c.Fase, c.Corriente, caidaV, caidaV * 100m / tensionFaseNeutroV);
            }),
        ];
    }
}
