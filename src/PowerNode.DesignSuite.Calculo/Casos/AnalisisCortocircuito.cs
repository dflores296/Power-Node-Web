namespace PowerNode.DesignSuite.Calculo.Casos;

/// <summary>Veredicto de comparar la falla disponible en un punto contra la capacidad interruptiva del aparato que está ahí.</summary>
public enum VeredictoCapacidadInterruptiva
{
    /// <summary>Falta un dato — la falla, la capacidad, o ambas. No se puede opinar, y no se opina.</summary>
    NoEvaluable,

    /// <summary>El aparato aguanta la falla disponible.</summary>
    Suficiente,

    /// <summary>La falla disponible supera lo que el aparato puede interrumpir. Error de diseño peligroso.</summary>
    Insuficiente
}

public sealed record ResultadoCapacidadInterruptiva(
    VeredictoCapacidadInterruptiva Veredicto,
    decimal? CorrienteFallaA,
    decimal? CapacidadInterruptivaKa,
    string? Advertencia);

/// <summary>
/// Propagación de la corriente de cortocircuito por el árbol y comparación contra la capacidad
/// interruptiva de cada aparato. Es lo que vuelve útiles al %Z del transformador y a los kA del
/// catálogo: por separado son datos sueltos; juntos dicen si el tablero que se está especificando
/// aguanta la falla que le va a llegar.
///
/// <b>Cómo se propaga.</b> Dos reglas nada más:
///
/// <list type="number">
/// <item>Un <b>transformador</b> es una frontera: recalcula la falla desde cero con su propio %Z
/// (Icc = Is / (%Z/100)), y lo que había aguas arriba deja de importar.</item>
/// <item>Todo lo demás (tableros, protecciones, cargas) la <b>deja pasar sin cambio</b>.</item>
/// </list>
///
/// <b>Ignorar la atenuación del cable es a propósito, y es lo conservador.</b> La falla real
/// disminuye con la longitud y la impedancia del conductor, así que propagarla sin atenuar
/// sobrestima — nunca subestima. Calcularla bien exige el método punto a punto (impedancia del
/// tramo por Tabla 9 más la del transformador, en cada elemento); los datos ya están en el modelo, así
/// que es una mejora posible, pero mientras tanto el resultado se equivoca del lado seguro.
///
/// <b>Por qué el transformador no suma la impedancia de la red de arriba.</b> Barra infinita:
/// se desprecia la impedancia aguas arriba, así que da el peor caso. Sumar la de la acometida daría
/// un número menor y más exacto; se deja para después por la misma razón que la atenuación del
/// cable, y con el mismo criterio de equivocarse del lado seguro.
/// </summary>
public static class AnalisisCortocircuito
{
    /// <summary>
    /// Corriente de falla en el secundario de un transformador, con barra infinita. Null si falta
    /// el %Z de placa — sin él no hay nada que calcular, y suponer una impedancia típica sería
    /// inventar el dato que decide qué tablero se compra.
    /// </summary>
    public static decimal? FallaEnSecundario(decimal corrienteSecundariaA, decimal impedanciaPct) =>
        corrienteSecundariaA > 0 && impedanciaPct > 0
            ? Math.Round(corrienteSecundariaA / (impedanciaPct / 100m), 1)
            : null;

    /// <summary>
    /// ¿El aparato aguanta? Devuelve <see cref="VeredictoCapacidadInterruptiva.NoEvaluable"/>
    /// cuando falta un dato, en vez de suponer que sí — un falso "suficiente" aquí es un tablero
    /// que explota.
    /// </summary>
    public static ResultadoCapacidadInterruptiva Verificar(
        string nombreDelElemento,
        decimal? corrienteFallaA,
        decimal? capacidadInterruptivaKa)
    {
        if (corrienteFallaA is not { } falla || falla <= 0)
            return new ResultadoCapacidadInterruptiva(
                VeredictoCapacidadInterruptiva.NoEvaluable, corrienteFallaA, capacidadInterruptivaKa, null);

        if (capacidadInterruptivaKa is not { } ka || ka <= 0)
            return new ResultadoCapacidadInterruptiva(
                VeredictoCapacidadInterruptiva.NoEvaluable, falla, capacidadInterruptivaKa,
                $"En '{nombreDelElemento}' hay {falla / 1000m:N1} kA de falla disponible, pero no se capturó su " +
                "capacidad interruptiva, así que no se pudo verificar si aguanta.");

        if (ka * 1000m >= falla)
            return new ResultadoCapacidadInterruptiva(
                VeredictoCapacidadInterruptiva.Suficiente, falla, ka, null);

        return new ResultadoCapacidadInterruptiva(
            VeredictoCapacidadInterruptiva.Insuficiente, falla, ka,
            $"'{nombreDelElemento}' tiene capacidad interruptiva de {ka:N0} kA y le llegan {falla / 1000m:N1} kA " +
            $"de falla disponible ({falla:N0} A). Hace falta un aparato de mayor capacidad, o reducir la falla. " +
            "(La falla se propaga sin atenuar por el cable, así que es el peor caso.)");
    }
}
