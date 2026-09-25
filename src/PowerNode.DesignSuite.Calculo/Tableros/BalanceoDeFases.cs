using PowerNode.DesignSuite.Calculo.Validaciones;

namespace PowerNode.DesignSuite.Calculo.Tableros;

/// <summary>Un circuito que se puede reacomodar: dónde está, cuánto ocupa y cuánta corriente lleva.</summary>
/// <param name="Etiqueta">Cómo nombrarlo al reportar el movimiento («Circuito 12 «Alumbrado»»).</param>
public readonly record struct CircuitoBalanceable(string Etiqueta, int EspacioInicial, int Polos, decimal CorrienteA);

/// <summary>Un circuito que cambia de espacio.</summary>
public readonly record struct Reacomodo(string Etiqueta, int De, int A);

/// <summary>Lo que se propone hacer, y cuánto mejora. Vacío de movimientos = ya está lo mejor que se puede.</summary>
public sealed record PropuestaDeBalanceo(
    decimal DesbalanceoAntesPct,
    decimal DesbalanceoDespuesPct,
    IReadOnlyList<Reacomodo> Movimientos)
{
    public bool Mejora => Movimientos.Count > 0;
}

/// <summary>
/// Reacomoda los circuitos de un tablero para <b>minimizar el desbalanceo entre barras</b>.
///
/// <para>
/// <b>Por qué es un comando aparte y no pasa al dar de alta un circuito.</b> El usuario preguntó si
/// el orden del acomodo afecta el balanceo, y sí: el espacio determina la barra, así que dónde queda
/// montado un circuito decide qué fase carga. Pero <b>en el momento de crearlo un circuito tiene 0
/// VA</b> —sus renglones se capturan después—, así que «colócalo donde el balanceo sea menor» empata
/// en todos los huecos y no optimiza nada. El balanceo solo se puede resolver <b>con las cargas ya
/// capturadas</b>, y por eso se pide cuando uno quiere, no al vuelo.
/// </para>
///
/// <para>
/// <b>Qué mueve y qué no.</b> Solo circuitos derivados. El interruptor principal y los que alimentan
/// a otros tableros <b>se quedan donde están</b>: los primeros porque su lugar lo fija el catálogo, y
/// los segundos porque mover una salida es tocar la topología, no acomodar carga.
/// </para>
///
/// <para>
/// <b>Un multipolar que toma TODAS las barras no desbalancea, y por eso no se mueve.</b> El criterio
/// de <see cref="CalculadoraDesbalanceo"/> —el mismo que ya usa el diagnóstico y que venía del Excel
/// original— suma la corriente completa del circuito a <b>cada</b> barra que toca, que es lo correcto
/// para una carga equilibrada: en un 3 polos de 20 A circulan 20 A por las tres líneas. Consecuencia:
/// en un tablero trifásico un 3 polos aporta lo mismo a A, B y C se ponga donde se ponga. Lo único
/// que de verdad reparte carga son los circuitos de <b>menos polos que barras</b>.
/// </para>
///
/// <para>
/// <b>La búsqueda es local y determinista</b>, no un óptimo garantizado: se prueban los intercambios
/// entre circuitos <b>del mismo número de polos</b> y las mudanzas a un espacio libre, y se aplica el
/// mejor mientras siga mejorando. Se prefiere así a un reacomodo global —que daría algo más parejo—
/// porque un reacomodo global <b>renumera el tablero entero</b>, y el número de circuito es lo que
/// está rotulado en la tapa y escrito en el plano. Mover tres circuitos y ganar ocho puntos vale más
/// que mover cuarenta y ganar nueve.
/// </para>
///
/// <para>
/// <b>El empate se rompe por dispersión</b> (Power Node Web, 2026-09-25, I-69). Con toda la carga en
/// una barra, ningún movimiento solo baja el (máx − mín) / máx: la otra barra sigue en cero y el
/// porcentaje en 100 %. La búsqueda se detenía ahí, aunque dos movimientos seguidos llegaban a 0 %.
/// Ahora, a igual porcentaje, gana el acomodo con las corrientes más parejas (la suma de los
/// cuadrados de su diferencia con el promedio), y la búsqueda sigue.
/// </para>
/// </summary>
public static class BalanceoDeFases
{
    /// <summary>Tope de vueltas de la búsqueda. Es un seguro contra un ciclo, no un límite que se alcance.</summary>
    private const int MaximoIteraciones = 100;

    /// <summary>Mejora mínima para que valga la pena mover algo, en puntos de desbalanceo.</summary>
    private const decimal MejoraMinimaPct = 0.01m;

    /// <summary>
    /// Qué conviene mover. <b>No modifica nada</b>: devuelve la propuesta para que quien llama la
    /// enseñe y decida.
    /// </summary>
    /// <param name="circuitos">Los circuitos derivados con espacio asignado y corriente ya calculada.</param>
    /// <param name="fijos">Lo que no se mueve: el principal y los alimentadores salientes.</param>
    public static PropuestaDeBalanceo Proponer(
        IReadOnlyList<CircuitoBalanceable> circuitos,
        IReadOnlyList<MontajeEnGabinete> fijos,
        int numeroEspacios,
        SistemaTablero sistema)
    {
        var barras = DistribucionBarras.BarrasDe(sistema);

        // Con una sola barra no hay desbalanceo que repartir: todo cae en la misma.
        if (barras.Count < 2 || circuitos.Count == 0)
            return new PropuestaDeBalanceo(0m, 0m, []);

        var origen = circuitos.Select(c => c.EspacioInicial).ToArray();
        var actual = (int[])origen.Clone();

        var antes = Evaluar(circuitos, actual, sistema, barras);
        var mejor = antes;

        for (var vuelta = 0; vuelta < MaximoIteraciones; vuelta++)
        {
            var (candidato, nota) = MejorMovimiento(circuitos, actual, fijos, numeroEspacios, sistema, barras, mejor);

            if (candidato is null)
                break;

            actual = candidato;
            mejor = nota;
        }

        var movimientos = circuitos
            .Select((c, i) => (c.Etiqueta, De: origen[i], A: actual[i]))
            .Where(m => m.De != m.A)
            .Select(m => new Reacomodo(m.Etiqueta, m.De, m.A))
            .ToList();

        return new PropuestaDeBalanceo(antes.Pct, mejor.Pct, movimientos);
    }

    /// <summary>
    /// El mejor cambio de una vuelta: el intercambio o la mudanza que más baja el desbalanceo.
    /// Devuelve <c>null</c> si ninguno mejora.
    /// </summary>
    private static (int[]? Acomodo, Nota Nota) MejorMovimiento(
        IReadOnlyList<CircuitoBalanceable> circuitos,
        int[] actual,
        IReadOnlyList<MontajeEnGabinete> fijos,
        int numeroEspacios,
        SistemaTablero sistema,
        IReadOnlyList<char> barras,
        Nota mejorConocida)
    {
        int[]? mejorAcomodo = null;
        var mejorNota = mejorConocida;

        // 1. INTERCAMBIOS entre circuitos del mismo número de polos. Son válidos por construcción:
        //    ocupan exactamente la misma forma, así que no hace falta comprobar si caben.
        for (var i = 0; i < circuitos.Count; i++)
            for (var j = i + 1; j < circuitos.Count; j++)
            {
                if (circuitos[i].Polos != circuitos[j].Polos || actual[i] == actual[j])
                    continue;

                var prueba = (int[])actual.Clone();
                (prueba[i], prueba[j]) = (prueba[j], prueba[i]);

                var d = Evaluar(circuitos, prueba, sistema, barras);
                if (d.MejorQue(mejorNota))
                    (mejorAcomodo, mejorNota) = (prueba, d);
            }

        // 2. MUDANZAS a un espacio libre. Aquí sí hay que preguntar si cabe, porque el hueco puede
        //    no tener la forma que el interruptor necesita.
        for (var i = 0; i < circuitos.Count; i++)
        {
            var ocupadoPorLosDemas = OtrosMontajes(circuitos, actual, fijos, excepto: i);

            for (var destino = 1; destino <= numeroEspacios; destino++)
            {
                if (destino == actual[i])
                    continue;

                if (AcomodoEnGabinete.MotivoNoCabe(destino, circuitos[i].Polos, numeroEspacios, ocupadoPorLosDemas) is not null)
                    continue;

                var prueba = (int[])actual.Clone();
                prueba[i] = destino;

                var d = Evaluar(circuitos, prueba, sistema, barras);
                if (d.MejorQue(mejorNota))
                    (mejorAcomodo, mejorNota) = (prueba, d);
            }
        }

        return (mejorAcomodo, mejorNota);
    }

    /// <summary>
    /// El desbalanceo de un acomodo dado. <b>Se calcula con <see cref="CalculadoraDesbalanceo"/></b>,
    /// que es el mismo que produce el número que el usuario ve en la pantalla del tablero: proponer
    /// con una fórmula y diagnosticar con otra daría movimientos que no mejoran lo que se mide.
    /// </summary>
    private static Nota Evaluar(
        IReadOnlyList<CircuitoBalanceable> circuitos,
        int[] acomodo,
        SistemaTablero sistema,
        IReadOnlyList<char> barras)
    {
        var corrientes = new List<CorrientePorCircuito>(circuitos.Count);

        for (var i = 0; i < circuitos.Count; i++)
        {
            var polos = Math.Clamp(circuitos[i].Polos, 1, barras.Count);
            corrientes.Add(new CorrientePorCircuito(
                DistribucionBarras.FasesQueOcupa(acomodo[i], polos, sistema),
                circuitos[i].CorrienteA));
        }

        var porFase = CalculadoraDesbalanceo.CorrientePorFase(corrientes, barras).Values.ToList();
        var promedio = porFase.Average();
        return new Nota(
            CalculadoraDesbalanceo.Porcentaje(corrientes, barras),
            porFase.Sum(i => (i - promedio) * (i - promedio)));
    }

    /// <summary>
    /// Qué tan bueno es un acomodo: primero el desbalanceo que se ve en pantalla; a igual desbalanceo,
    /// la dispersión de las corrientes por barra.
    /// </summary>
    private readonly record struct Nota(decimal Pct, decimal Dispersion)
    {
        public bool MejorQue(Nota otra) =>
            Pct < otra.Pct - MejoraMinimaPct
            || (Math.Abs(Pct - otra.Pct) < MejoraMinimaPct && Dispersion < otra.Dispersion * 0.9999m);
    }

    /// <summary>Lo que ocupa espacio para el circuito <paramref name="excepto"/>: los fijos y sus compañeros.</summary>
    private static List<MontajeEnGabinete> OtrosMontajes(
        IReadOnlyList<CircuitoBalanceable> circuitos, int[] acomodo,
        IReadOnlyList<MontajeEnGabinete> fijos, int excepto)
    {
        var lista = fijos.ToList();

        for (var k = 0; k < circuitos.Count; k++)
            if (k != excepto && acomodo[k] >= 1)
                lista.Add(new MontajeEnGabinete(acomodo[k], circuitos[k].Polos, circuitos[k].Etiqueta));

        return lista;
    }
}
