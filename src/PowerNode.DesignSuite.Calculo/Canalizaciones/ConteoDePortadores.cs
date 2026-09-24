using PowerNode.DesignSuite.Calculo.Casos;
using PowerNode.DesignSuite.Calculo.Tableros;

namespace PowerNode.DesignSuite.Calculo.Canalizaciones;

/// <summary>Un circuito dentro de una canalización, con lo que hace falta para contar sus portadores.</summary>
/// <param name="Nombre">«Circuito 3», «Alimentador» — para el desglose y las citas.</param>
/// <param name="Polos">Conductores de fase del circuito (sus polos).</param>
/// <param name="LlevaNeutro">1 polo: siempre. 2 y 3 polos: solo con carga F-N.</param>
/// <param name="Barras">Las barras que ocupa, para validar el neutro compartido.</param>
/// <param name="JuegosEnEsteTubo">Conductores en paralelo que van en ESTA canalización (1 si cada
/// juego va en su propio tubo, que es lo normal — 310-10(h)(3)).</param>
public sealed record CircuitoEnCanalizacion(
    string Nombre,
    int Polos,
    bool LlevaNeutro,
    IReadOnlyList<char> Barras,
    int JuegosEnEsteTubo = 1);

/// <summary>Lo que aporta cada circuito al conteo, para la memoria.</summary>
public sealed record PortadoresDelCircuito(string Circuito, int Fases, int Neutros, string Motivo);

public sealed record ConteoDePortadores(
    int Portadores,
    IReadOnlyList<PortadoresDelCircuito> Desglose,
    bool NeutroCompartidoAplicado,
    IReadOnlyList<string> Avisos,
    IReadOnlyList<Cita> Citas);

/// <summary>
/// <b>Cuántos conductores portadores de corriente lleva una canalización</b> — el número con el que
/// se entra a la Tabla 310-15(b)(3)(a). NACIDO EN LA WEB (2026-09-24).
///
/// <para>Las reglas, todas de 310-15(b):</para>
/// <list type="bullet">
/// <item>Cada conductor de fase cuenta, y en paralelo cada uno de ellos — (b)(3)(a).</item>
/// <item>El neutro de un circuito de 1 polo <b>cuenta</b>: regresa toda la corriente del circuito.
/// (b)(5)(1) solo perdona al neutro que lleva el desbalance <i>de otros conductores del mismo
/// circuito</i>, y en un 1F-2H no hay otros.</item>
/// <item>2 fases + neutro de un sistema 3F-4H en estrella: el neutro lleva ≈ la corriente de fase y
/// <b>cuenta</b> — (b)(5)(2). En 1F-3H solo lleva el desbalance: no cuenta — (b)(5)(1).</item>
/// <item>3 fases + neutro en 3F-4H: no cuenta — (b)(5)(1) —, salvo carga mayormente no lineal —
/// (b)(5)(3).</item>
/// <item>La puesta a tierra no cuenta nunca — (b)(6).</item>
/// <item>Neutro compartido (circuito multiconductor, 210-4): varios circuitos de 1 polo en barras
/// distintas con un solo neutro, que se cuenta como el del multipolar equivalente.</item>
/// </list>
/// </summary>
public static class ContadorDePortadores
{
    public static ConteoDePortadores Contar(
        IReadOnlyList<CircuitoEnCanalizacion> circuitos,
        ConfiguracionTablero sistema,
        bool cargaNoLineal,
        bool neutroCompartido)
    {
        var desglose = new List<PortadoresDelCircuito>();
        var avisos = new List<string>();
        var citas = new List<Cita>();

        // ¿Se puede compartir el neutro? Solo entre circuitos de 1 polo, en barras distintas.
        var monopolares = circuitos.Where(c => c.Polos == 1 && c.LlevaNeutro).ToList();
        var compartido = false;
        if (neutroCompartido)
        {
            var barras = monopolares.Select(c => c.Barras.FirstOrDefault()).ToList();
            var barrasDelSistema = SistemaDelTablero.MaximoPolos(new SistemaTablero(FasesDe(sistema), HilosDe(sistema)));
            string? motivo =
                sistema is ConfiguracionTablero.UnaFaseDosHilos or ConfiguracionTablero.TresFasesTresHilos
                    ? "el sistema no tiene más de una fase con neutro"
                : monopolares.Count < 2 ? "hacen falta al menos dos circuitos de 1 polo en la canalización"
                : barras.Distinct().Count() != barras.Count ? $"dos circuitos de 1 polo están en la misma barra ({string.Join(", ", barras)}): el neutro llevaría la suma de sus corrientes"
                : monopolares.Count > barrasDelSistema ? $"son {monopolares.Count} circuitos de 1 polo y el tablero tiene {barrasDelSistema} barras"
                : null;

            if (motivo is null)
            {
                compartido = true;
                avisos.Add($"Neutro compartido ({string.Join(", ", monopolares.Select(c => c.Nombre))}): los interruptores deben "
                    + "desconectar simultáneamente todas las fases —multipolar o con las palancas unidas— — 210-4(b); y los conductores "
                    + "de fase y el neutro se agrupan con amarres en el tablero — 210-4(d).");
            }
            else
            {
                avisos.Add($"No se aplica el neutro compartido: {motivo}. Cada circuito se cuenta con su propio neutro.");
            }
        }

        var total = 0;
        foreach (var c in circuitos)
        {
            var juegos = Math.Max(1, c.JuegosEnEsteTubo);
            var fases = c.Polos * juegos;

            if (compartido && c.Polos == 1 && c.LlevaNeutro)
            {
                desglose.Add(new PortadoresDelCircuito(c.Nombre, fases, 0, "neutro compartido: se cuenta una vez, abajo"));
                total += fases;
                continue;
            }

            var (cuenta, motivo) = NeutroCuenta(c.Polos, c.LlevaNeutro, sistema, cargaNoLineal);
            var neutros = cuenta ? juegos : 0;
            desglose.Add(new PortadoresDelCircuito(c.Nombre, fases, neutros, motivo));
            total += fases + neutros;
        }

        if (compartido)
        {
            var k = monopolares.Count;
            var (cuenta, motivo) = NeutroCuenta(k, true, sistema, cargaNoLineal);
            desglose.Add(new PortadoresDelCircuito("Neutro compartido", 0, cuenta ? 1 : 0,
                $"como el de un circuito de {k} fases: {motivo}"));
            total += cuenta ? 1 : 0;
            citas.Add(new Cita("210-4(a)", $"Circuito multiconductor: {string.Join(", ", monopolares.Select(c => c.Nombre))} con un solo neutro."));
        }

        citas.Add(new Cita("310-15(b)(3)(a)", $"Conductores portadores de corriente en la canalización: {total}."));
        citas.Add(new Cita("310-15(b)(6)", "El conductor de puesta a tierra no se cuenta."));

        return new ConteoDePortadores(total, desglose, compartido, avisos, citas);
    }

    /// <summary>Si el neutro de un circuito de esos polos cuenta como portador, y por qué.</summary>
    public static (bool Cuenta, string Motivo) NeutroCuenta(int polos, bool llevaNeutro, ConfiguracionTablero sistema, bool cargaNoLineal)
    {
        if (!llevaNeutro)
            return (false, "sin neutro");
        if (polos == 1)
            return (true, "1 fase + neutro: el neutro regresa toda la corriente — 310-15(b)(5)(1) no lo exime");
        if (polos == 2)
            return sistema is ConfiguracionTablero.TresFasesCuatroHilos or ConfiguracionTablero.DosFasesDeEstrella
                ? (true, "2 fases + neutro de estrella: lleva ≈ la corriente de fase — 310-15(b)(5)(2)")
                : (false, "2 fases + neutro con derivación central: solo el desbalance — 310-15(b)(5)(1)");
        return cargaNoLineal
            ? (true, "3 fases + neutro con carga mayormente no lineal: armónicas — 310-15(b)(5)(3)")
            : (false, "3 fases + neutro: solo el desbalance — 310-15(b)(5)(1)");
    }

    private static int FasesDe(ConfiguracionTablero s) => s switch
    {
        ConfiguracionTablero.UnaFaseDosHilos or ConfiguracionTablero.UnaFaseTresHilos => 1,
        ConfiguracionTablero.DosFasesDeEstrella => 2,
        _ => 3,
    };

    private static int HilosDe(ConfiguracionTablero s) => s switch
    {
        ConfiguracionTablero.UnaFaseDosHilos => 2,
        ConfiguracionTablero.TresFasesCuatroHilos => 4,
        _ => 3,
    };
}
