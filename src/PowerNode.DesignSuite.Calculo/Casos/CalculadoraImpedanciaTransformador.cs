namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>Lo que la impedancia del transformador determina: cuánta corriente de falla entrega y cuánto cae la tensión con carga.</summary>
public sealed record ResultadoImpedancia(
    decimal ImpedanciaPct,
    decimal ResistenciaPct,
    decimal ReactanciaPct,
    decimal CorrienteCortocircuitoSecundarioA,
    decimal RegulacionPct,
    string Explicacion);

/// <summary>
/// El modelo de impedancia (%Z) del transformador — lo que quedaba pendiente desde el bloque 9.7.
///
/// Sirve para dos cosas distintas, y las dos importan para un proyecto real:
///
/// **1. Corriente de cortocircuito en el secundario.** Icc = Is / (%Z/100). Es el número que decide
/// la capacidad interruptiva (kA) que debe tener el tablero que cuelga del transformador — un NQ
/// aguanta 10 kA y un NF con interruptores EJB llega a 65 kA. Poner un tablero con menos kA que la
/// falla disponible es un error de diseño peligroso, no un detalle.
///
/// Se usa la hipótesis de **barra infinita**: se desprecia la impedancia de la red aguas arriba, así
/// que el resultado es el peor caso. Es lo conservador y lo que se hace cuando no se tiene el dato
/// de la compañía suministradora.
///
/// **2. Regulación de tensión.** Cuánto cae la tensión del secundario al cargarlo, que es distinto
/// de la caída de tensión en el conductor (esa ya la calcula el motor con la Tabla 9).
///
/// Nada de esto sale de la NOM-001: son las relaciones eléctricas del transformador. El %Z y la
/// relación X/R vienen de la placa o de la ficha del fabricante, por eso se capturan.
/// </summary>
public static class CalculadoraImpedanciaTransformador
{
    public static ResultadoImpedancia Calcular(
        decimal corrienteSecundariaA,
        decimal impedanciaPct,
        decimal relacionXR,
        decimal factorPotenciaCarga,
        decimal factorCarga = 1m)
    {
        if (corrienteSecundariaA <= 0)
            throw new ArgumentOutOfRangeException(nameof(corrienteSecundariaA), "La corriente secundaria debe ser mayor a cero.");
        if (impedanciaPct <= 0)
            throw new ArgumentOutOfRangeException(nameof(impedanciaPct), "El %Z debe ser mayor a cero; viene de la placa del transformador.");
        if (relacionXR < 0)
            throw new ArgumentOutOfRangeException(nameof(relacionXR), "La relación X/R no puede ser negativa.");
        if (factorPotenciaCarga is <= 0 or > 1)
            throw new ArgumentOutOfRangeException(nameof(factorPotenciaCarga), "El factor de potencia va entre 0 y 1.");

        // Barra infinita: toda la impedancia del circuito de falla es la del transformador.
        var icc = Math.Round(corrienteSecundariaA / (impedanciaPct / 100m), 1);

        // %Z es la hipotenusa; X/R da el reparto entre la parte resistiva y la reactiva.
        var xr = (double)relacionXR;
        var resistenciaPct = (decimal)((double)impedanciaPct / Math.Sqrt(1 + xr * xr));
        var reactanciaPct = resistenciaPct * relacionXR;

        var cos = (double)factorPotenciaCarga;
        var sen = Math.Sqrt(Math.Max(0, 1 - cos * cos));

        // Aproximación estándar de regulación, con el término de segundo orden — el que se suele
        // omitir y que sí pesa cuando la reactancia domina.
        var primerOrden = (double)factorCarga * ((double)resistenciaPct * cos + (double)reactanciaPct * sen);
        var termino = (double)factorCarga * ((double)reactanciaPct * cos - (double)resistenciaPct * sen);
        var regulacion = primerOrden + termino * termino / 200.0;

        return new ResultadoImpedancia(
            impedanciaPct,
            Math.Round(resistenciaPct, 3),
            Math.Round(reactanciaPct, 3),
            icc,
            Math.Round((decimal)regulacion, 2),
            $"Con {impedanciaPct:N2} % de impedancia y X/R = {relacionXR:N1}, la corriente de " +
            $"cortocircuito disponible en el secundario es {icc:N0} A ({icc / 1000m:N1} kA) suponiendo " +
            $"barra infinita. El tablero que se conecte aquí debe tener capacidad interruptiva mayor. " +
            $"A plena carga con factor de potencia {factorPotenciaCarga:N2}, la regulación es " +
            $"{regulacion:N2} %.");
    }

    // La comparación de "¿aguanta la falla?" VIVÍA AQUÍ y se quitó el 2026-08-19: era un duplicado
    // más pobre de AnalisisCortocircuito.Verificar, que hace la misma cuenta (ka x 1000 >= falla) y
    // además distingue el caso "no evaluable" —sin falla o sin kA capturados— y redacta el aviso.
    //
    // Nadie llamaba a la de aquí; la que corre en la cascada siempre fue la otra. Dos criterios para
    // la misma pregunta es como nacen las divergencias: un día uno de los dos aprende a distinguir
    // "no se sabe" de "no aguanta" y el otro no, y el programa contesta distinto según por dónde se
    // entre. Se conserva UNA.
}
