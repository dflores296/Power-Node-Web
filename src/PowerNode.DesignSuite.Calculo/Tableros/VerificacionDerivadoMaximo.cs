namespace PowerNode.DesignSuite.Calculo.Tableros;

/// <summary>
/// <b>Que el interruptor derivado no pase de lo que su tablero admite como derivado.</b>
///
/// <para>
/// <b>No es el 408-36, y conviene no confundirlos.</b> El 408-36 mide el dispositivo que
/// <i>alimenta</i> al tablero contra la corriente de su barra —es la protección del tablero
/// completo—. Esto de aquí mide cada <i>derivado</i> contra el aparato más grande que la familia de
/// derivados del tablero ofrece: un NQ admite QO y QOB, y esa familia se acaba en 125 A, aunque su
/// barra sea de 400.
/// </para>
///
/// <para>
/// <b>Por qué vive aparte y no dentro de <see cref="VerificacionesILine"/>, que es donde nació:</b>
/// no tiene nada de I-Line. Del 2026-08-15 al 2026-08-27 el dato
/// (<c>ModeloTablero.CapacidadMaximaDerivadaA</c>) solo estaba sembrado en los I-Line, y la
/// comparación vivía dentro de la verificación de ese tablero — así que <b>en un NQ, en un NF o en un
/// centro de carga no se revisaba nada</b>: la cascada devuelve lista vacía de avisos en cuanto el
/// tablero tiene espacios numerados. El techo estaba escrito en el texto de
/// <c>InterruptoresDerivados</c>, que es prosa para leer, no dato para comparar.
/// </para>
///
/// <para>
/// <b>Es aviso, no bloqueo, y calla cuando falta el dato.</b> Sin tope sembrado no hay contra qué
/// comparar, y sin protección calculada todavía no hay qué comparar: en los dos casos no se dice
/// nada, que no es lo mismo que decir que está bien.
/// </para>
/// </summary>
public static class VerificacionDerivadoMaximo
{
    /// <param name="derivados">Número de circuito y su protección calculada, si ya la tiene.</param>
    /// <param name="topeA">
    /// El derivado más grande que admite el modelo del tablero. <c>null</c> = el catálogo no lo
    /// declara, o el tablero se capturó a mano.
    /// </param>
    public static IReadOnlyList<string> Avisos(
        IEnumerable<(int Numero, decimal? ProteccionA)> derivados,
        decimal? topeA)
    {
        if (topeA is not { } tope)
            return [];

        return
        [
            .. derivados
                .Where(d => d.ProteccionA is { } proteccion && proteccion > tope)
                .Select(d =>
                    $"El derivado {d.Numero} es de {d.ProteccionA!.Value:N0} A y este tablero admite "
                    + $"derivados de hasta {tope:N0} A.")
        ];
    }
}
